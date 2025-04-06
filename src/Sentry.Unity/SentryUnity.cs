using System;
using System.ComponentModel;
using System.Reflection;
using JetBrains.Annotations;
using Sentry.Extensibility;

namespace Sentry.Unity;

/// <summary>
/// Sentry Unity initialization class.
/// </summary>
public static class SentryUnity
{
    /// <summary>
    /// Initializes Sentry Unity SDK while configuring the options.
    /// </summary>
    /// <param name="sentryUnityOptionsConfigure">Callback to configure the options.</param>
    public static SentryUnitySdk Init(Action<SentryUnityOptions> sentryUnityOptionsConfigure,
        [CanBeNull] Assembly caller = null)
    {
        var options = new SentryUnityOptions();
        sentryUnityOptionsConfigure.Invoke(options);

        return Init(options, caller ?? Assembly.GetCallingAssembly());
    }

    /// <summary>
    /// Initializes Sentry Unity SDK while providing an options object.
    /// </summary>
    /// <param name="options">The options object.</param>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static SentryUnitySdk Init(SentryUnityOptions options, [CanBeNull] Assembly caller = null)
    {
        return SentryUnitySdk.Init(options, caller ?? Assembly.GetCallingAssembly());
    }

    /// <summary>
    /// Represents the crash state of the games's previous run.
    /// Used to determine if the last execution terminated normally or crashed.
    /// </summary>
    public enum CrashedLastRun
    {
        /// <summary>
        /// The LastRunState is unknown. This might be due to the SDK not being initialized, native crash support
        /// missing, or being disabled.
        /// </summary>
        Unknown,

        /// <summary>
        /// The application did not crash during the last run.
        /// </summary>
        DidNotCrash,

        /// <summary>
        /// The application crashed during the last run.
        /// </summary>
        Crashed
    }
}
