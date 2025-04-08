using System;
using System.Collections.Generic;
using System.Linq;
using Sentry.Extensibility;

namespace Sentry.Unity;

/// <summary>
/// Default filter.
/// </summary>
public class DefaultFilter(IEnumerable<string> namespaces) : ISentryEventProcessor
{
    /// <summary>
    /// Process
    /// </summary>
    /// <param name="event"></param>
    /// <returns></returns>
    public SentryEvent? Process(SentryEvent @event)
    {
        var srcErr = @event.Exception?.TargetSite.Module.Assembly.GetName().Name ?? Guid.NewGuid().ToString();
        return namespaces.Any(ns => srcErr.Equals(ns, StringComparison.InvariantCultureIgnoreCase))
            ? @event
            : null;
    }
}
