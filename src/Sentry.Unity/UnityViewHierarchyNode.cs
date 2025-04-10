using System.Collections.Generic;
using Newtonsoft.Json;
using Sentry.Extensibility;
using Sentry.Internal.Extensions;

namespace Sentry.Unity;

internal class UnityViewHierarchyNode : ViewHierarchyNode
{
    public string? Tag { get; set; }
    public string? Position { get; set; }
    public string? Rotation { get; set; }
    public string? Scale { get; set; }
    public bool? Active { get; set; }

    public List<string>? Extras { get; set; }

    public UnityViewHierarchyNode(string name) : base(name) { }

    protected override void WriteAdditionalProperties(SentryJsonWriter writer, IDiagnosticLogger? logger)
    {
        if (!string.IsNullOrWhiteSpace(Tag))
        {
            writer.WriteString("tag", Tag);
        }

        if (!string.IsNullOrWhiteSpace(Position))
        {
            writer.WriteString("position", Position);
        }
        if (!string.IsNullOrWhiteSpace(Rotation))
        {
            writer.WriteString("rotation", Rotation);
        }
        if (!string.IsNullOrWhiteSpace(Scale))
        {
            writer.WriteString("scale", Scale);
        }

        if (Active is { } active)
        {
            writer.WriteString("active", active.ToString());
        }

        if (Extras is { } extras)
        {
            writer.WritePropertyName("extras");
            writer.WriteStartArray();
            foreach (var extra in extras)
            {
                writer.WriteStringValue(extra);
            }
            writer.WriteEndArray();
        }
    }
}
