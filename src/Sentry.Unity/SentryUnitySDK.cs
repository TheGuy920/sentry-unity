using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using HarmonyLib;
using Sentry.Extensibility;
using Sentry.Unity.Integrations;
using UnityEngine;

namespace Sentry.Unity;

public class SentryUnitySdk
{
    private SentrySdk _sentrySdk = null!;
    private readonly SentryUnityOptions _options;
    private IDisposable _dotnetSdk = null!;
    private FileStream? _lockFile;

    /// <summary>
    /// The Sentry SDK instance.
    /// </summary>
    public SentrySdk SentrySdk => _sentrySdk;

    private SentryUnitySdk(SentryUnityOptions options)
    {
        _options = options;
    }

    internal static SentryUnitySdk? Init(SentrySdk sdk, SentryUnityOptions options, Assembly caller)
    {
        var unitySdk = new SentryUnitySdk(options);

        var namespaces = AccessTools.GetTypesFromAssembly(caller).Select(t => t.Namespace).Distinct();
        options.AddEventProcessor(new DefaultFilter(namespaces));
        options.SetupUnityLogging();
        MainThreadData.CollectData();

        // On Standalone, we disable cache dir in case multiple app instances run over the same path.
        // Note: we cannot use a named Mutex, because Unit doesn't support it. Instead, we create a file with `FileShare.None`.
        // https://forum.unity.com/threads/unsupported-internal-call-for-il2cpp-mutex-createmutex_internal-named-mutexes-are-not-supported.387334/
        if (ApplicationAdapter.Instance.Platform is RuntimePlatform.WindowsPlayer && options.CacheDirectoryPath is not null)
        {
            try
            {
                unitySdk._lockFile = new FileStream(Path.Combine(options.CacheDirectoryPath, "sentry-unity.lock"), FileMode.OpenOrCreate,
                    FileAccess.ReadWrite, FileShare.None);
            }
            catch (Exception ex)
            {
                options.DiagnosticLogger?.LogWarning("An exception was thrown while trying to " +
                                                     "acquire a lockfile on the config directory: .NET event cache will be disabled.", ex);
                options.CacheDirectoryPath = null;
                options.AutoSessionTracking = false;
            }
        }

        unitySdk._sentrySdk = sdk;
        unitySdk._dotnetSdk = unitySdk._sentrySdk.Init(options);

        if (options.NativeContextWriter is { } contextWriter)
        {
            unitySdk._sentrySdk.ConfigureScope((scope) =>
            {
                var task = Task.Run(() => contextWriter.Write(scope)).ContinueWith(t =>
                {
                    if (t.Exception is not null)
                    {
                        options.DiagnosticLogger?.LogWarning(
                            "Failed to synchronize scope to the native SDK: {0}", t.Exception);
                    }
                });
            });
        }

        ApplicationAdapter.Instance.Quitting += unitySdk.Close;

        return unitySdk;
    }

    public void Close()
    {
        _options.DiagnosticLogger?.LogDebug("Closing the sentry-dotnet SDK");
        try
        {
            ApplicationAdapter.Instance.Quitting -= Close;
            _options.NativeSupportCloseCallback?.Invoke();
            _options.NativeSupportCloseCallback = null;

            _dotnetSdk.Dispose();
        }
        catch (Exception ex)
        {
            _options.DiagnosticLogger?.Log(SentryLevel.Warning,
                "Exception while closing the .NET SDK.", ex);
        }

        try
        {
            // We don't really need to close, Windows would release the lock anyway, but let's be nice.
            _lockFile?.Close();
        }
        catch (Exception ex)
        {
            _options.DiagnosticLogger?.Log(SentryLevel.Warning,
                "Exception while releasing the lockfile on the config directory.", ex);
        }
    }

    public SentryUnity.CrashedLastRun CrashedLastRun()
    {
        if (_options.CrashedLastRun is null)
        {
            _options.DiagnosticLogger?.LogDebug("The SDK does not have a 'CrashedLastRun' set. " +
                                                "This might be due to a missing or disabled native integration.");
            return SentryUnity.CrashedLastRun.Unknown;
        }

        return _options.CrashedLastRun.Invoke()
            ? SentryUnity.CrashedLastRun.Crashed
            : SentryUnity.CrashedLastRun.DidNotCrash;
    }
}
