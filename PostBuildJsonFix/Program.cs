using System.Text.Json.Nodes;
using Mono.Cecil;

namespace PostBuildJsonFix;

static class Program
{
    static void Main(string[] args)
    {
        // get all implicit and referenced assemblies to output dir
        // Load all libraries in ./obj/project.assets.json
        var projectAssetsPath = Path.Combine(Directory.GetCurrentDirectory(), "obj", "project.assets.json");
        var projectAssetsJson = JsonNode.Parse(File.ReadAllText(projectAssetsPath))!;
        var jsonLibraries = projectAssetsJson["targets"]!.AsObject().First().Value!;

        // Libraries base path is UserDir/.nuget/packages/
        var librariesBasePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".nuget",
            "packages");

        List<string> libraries = [];
        foreach (var (key, value) in jsonLibraries.AsObject())
        {
            if (value!["compile"] is not JsonObject jobj)
                continue;

            var libName = jobj.Select(kvp => kvp.Key).First();
            if (libName.EndsWith("_._") || libName.StartsWith("ref"))
            {
                var alternatives = projectAssetsJson["libraries"]![key]!["files"]!.AsArray()
                    .Select(i => i?.AsValue().GetValue<string>())
                    .Where(f => f is not null && (
                        f.StartsWith("lib/net4") ||
                        f.StartsWith("lib/netstandard")
                    ) && f.EndsWith(".dll") && !f.StartsWith("lib/net45")).ToArray();

                if (alternatives.Length > 0)
                {
                    libraries.AddRange(alternatives.Select(alt =>
                        Path.Combine(librariesBasePath, key.ToLower(), alt!)));
                    continue;
                }

                alternatives = projectAssetsJson["libraries"]![key]!["files"]!.AsArray()
                    .Select(i => i?.AsValue().GetValue<string>())
                    .Where(f => f is not null && (
                        f.StartsWith("ref/net4") ||
                        f.StartsWith("ref/netstandard")
                    ) && f.EndsWith(".dll") && !f.StartsWith("ref/net45")).ToArray();

                if (alternatives.Length > 0)
                {
                    libraries.AddRange(alternatives.Select(alt =>
                        Path.Combine(librariesBasePath, key.ToLower(), alt!)));
                }

                continue;
            }

            libraries.Add(Path.Combine(librariesBasePath, key.ToLower(), libName));
        }

        Directory.SetCurrentDirectory(args[0]);

        // copy all libraries to output dir and do not overwrite
        foreach (var lib in libraries)
        {
            if (!File.Exists(lib))
                continue;

            var destPath = Path.GetFileName(lib);

            if (File.Exists(destPath))
                continue;

            File.Copy(lib, destPath);
        }

        return;
        // Process the main Sentry.dll assembly
        // ProcessAssembly("Sentry.dll");
        //
        // // Process all other assemblies
        // foreach (var file in Directory.GetFiles(".", "*.dll"))
        // {
        //     if (Path.GetFileName(file).StartsWith("Sentry"))
        //         continue;
        //
        //     ProcessAssembly(file);
        // }
    }

    private static void ProcessAssembly(string filePath)
    {
        // Read the assembly into memory
        var assembly = AssemblyDefinition.ReadAssembly(filePath, new ReaderParameters { ReadWrite = true });

        // Update the assembly name to prepend "Sentry."
        var name = assembly.Name;
        if (!name.Name.StartsWith("Sentry."))
        {
            name.Name = "Sentry." + name.Name;
            assembly.Name = name;
        }

        // Update all references in this assembly to also have "Sentry."
        foreach (var reference in assembly.MainModule.AssemblyReferences)
        {
            if (!File.Exists("Sentry." + assembly.Name.Name + ".dll") && !File.Exists(assembly.Name.Name + ".dll"))
                continue;

            if (!reference.Name.StartsWith("Sentry"))
            {
                Console.WriteLine(reference.Name);
                reference.Name = "Sentry." + reference.Name;
            }
        }


        // Update all types, methods, and properties in the assembly to use the "Sentry." namespace
        foreach (var module in assembly.Modules)
        {
            foreach (var type in module.Types)
            {
                // Update namespace of the type itself
                if (type.Namespace != null && !type.Namespace.StartsWith("Sentry"))
                {
                    if (File.Exists("Sentry." + type.Namespace + ".dll") || File.Exists(type.Namespace + ".dll"))
                    {
                        Console.WriteLine(type.Namespace);
                        type.Namespace = "Sentry." + type.Namespace;
                    }
                }

                // Update methods and their parameters
                foreach (var method in type.Methods)
                {
                    // Update the return type
                    if (method.ReturnType.Namespace != null && !method.ReturnType.Namespace.StartsWith("Sentry"))
                    {
                        try
                        {
                            if (File.Exists("Sentry." + method.ReturnType.Namespace + ".dll") ||
                                File.Exists(method.ReturnType.Namespace + ".dll"))
                            {
                                Console.WriteLine(method.ReturnType.Namespace);
                                method.ReturnType.Namespace = "Sentry." + method.ReturnType.Namespace;
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(
                                $"Error processing return type [{method.ReturnType.Namespace}]: {method.Name} - {ex.Message}");
                        }
                    }

                    // Update parameter types
                    foreach (var param in method.Parameters)
                    {
                        if (param.ParameterType.Namespace != null &&
                            !param.ParameterType.Namespace.StartsWith("Sentry"))
                        {
                            try
                            {
                                if (File.Exists("Sentry." + param.ParameterType.Namespace + ".dll") ||
                                    File.Exists(param.ParameterType.Namespace + ".dll"))
                                {
                                    Console.WriteLine(param.ParameterType.Namespace);
                                    param.ParameterType.Namespace = "Sentry." + param.ParameterType.Namespace;
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(
                                    $"Error processing parameter type [{param.ParameterType.Namespace}]: {param.ParameterType.Name} - {ex.Message}");
                            }
                        }
                    }
                }

                // Update fields and properties
                foreach (var field in type.Fields)
                {
                    if (field.FieldType.Namespace != null && !field.FieldType.Namespace.StartsWith("Sentry"))
                    {
                        if (File.Exists("Sentry." + field.FieldType.Namespace + ".dll") ||
                            File.Exists(field.FieldType.Namespace + ".dll"))
                        {
                            Console.WriteLine(field.FieldType.Namespace);
                            field.FieldType.Namespace = "Sentry." + field.FieldType.Namespace;
                        }
                    }
                }

                foreach (var property in type.Properties)
                {
                    if (property.PropertyType.Namespace != null &&
                        !property.PropertyType.Namespace.StartsWith("Sentry"))
                    {
                        if (File.Exists("Sentry." + property.PropertyType.Namespace + ".dll") ||
                            File.Exists(property.PropertyType.Namespace + ".dll"))
                        {
                            Console.WriteLine(property.PropertyType.Namespace);
                            property.PropertyType.Namespace = "Sentry." + property.PropertyType.Namespace;
                        }
                    }
                }
            }
        }

        // Update all `using` statements in the IL
        foreach (var module in assembly.Modules)
        {
            foreach (var type in module.Types)
            {
                foreach (var method in type.Methods)
                {
                    // Update method's references (i.e., type names) to point to "Sentry."
                    foreach (var param in method.Parameters)
                    {
                        if (param.ParameterType.Namespace != null &&
                            !param.ParameterType.Namespace.StartsWith("Sentry"))
                        {
                            try
                            {
                                if (File.Exists("Sentry." + param.ParameterType.Namespace + ".dll") ||
                                    File.Exists(param.ParameterType.Namespace + ".dll"))
                                {
                                    Console.WriteLine(param.ParameterType.Namespace);
                                    param.ParameterType.Namespace = "Sentry." + param.ParameterType.Namespace;
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine(
                                    $"Error processing parameter type [{param.ParameterType.Namespace}]: {param.ParameterType.Name} - {ex.Message}");
                            }
                        }
                    }

                    if (method.ReturnType.Namespace != null && !method.ReturnType.Namespace.StartsWith("Sentry"))
                    {
                        if (File.Exists("Sentry." + method.ReturnType.Namespace + ".dll") ||
                            File.Exists(method.ReturnType.Namespace + ".dll"))
                        {
                            Console.WriteLine(method.ReturnType.Namespace);
                            method.ReturnType.Namespace = "Sentry." + method.ReturnType.Namespace;
                        }
                    }
                }
            }
        }

        // Write back the modified assembly
        var newFilePath = Path.Combine(Path.GetDirectoryName(filePath)!,
            Path.GetFileName(filePath).StartsWith("Sentry.") ? "Patched." + Path.GetFileName(filePath) : "Sentry." + Path.GetFileName(filePath));

        Console.WriteLine(newFilePath);
        assembly.Write(newFilePath);

        // Optionally delete the original
        File.Delete(filePath);
    }
}
