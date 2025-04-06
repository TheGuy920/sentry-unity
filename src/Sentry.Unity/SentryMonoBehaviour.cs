using System;
using Sentry.Unity.Integrations;
using UnityEngine;

namespace Sentry.Unity;

/// <summary>
/// Singleton and DontDestroyOnLoad setup.
/// </summary>
[AddComponentMenu("")] // Hides it from being added as a component in the inspector
public partial class SentryMonoBehaviour : MonoBehaviour
{
    public static SentryMonoBehaviour CreateInstance()
    {
        // HideAndDontSave excludes the gameObject from the scene meaning it does not get destroyed on loading/unloading
        var sentryGameObject = new GameObject("SentryMonoBehaviour") { hideFlags = HideFlags.HideAndDontSave };
        return sentryGameObject.AddComponent<SentryMonoBehaviour>();
    }
}

/// <summary>
/// A MonoBehaviour used to provide access to helper methods used during Performance Auto Instrumentation
/// </summary>
public partial class SentryMonoBehaviour
{
    public void StartAwakeSpan(MonoBehaviour monoBehaviour) =>
        _sentrySdk?.GetSpan()?.StartChild("awake", $"{monoBehaviour.gameObject.name}.{monoBehaviour.GetType().Name}");

    public void FinishAwakeSpan() => _sentrySdk?.GetSpan()?.Finish(SpanStatus.Ok);
}

/// <summary>
///  A MonoBehavior used to forward application focus events to subscribers.
/// </summary>
public partial class SentryMonoBehaviour
{
    /// <summary>
    /// Hook to receive an event when the application gains focus.
    /// </summary>
    public event Action? ApplicationResuming;

    /// <summary>
    /// Hook to receive an event when the application loses focus.
    /// </summary>
    public event Action? ApplicationPausing;

    private SentrySdk? _sentrySdk;
    public void SetSentrySdk(SentrySdk sentrySdk) => _sentrySdk ??= sentrySdk;

    // Keeping internal track of running state because OnApplicationPause and OnApplicationFocus get called during startup and would fire false resume events
    internal bool _isRunning = true;

    private IApplication? _application;
    internal IApplication Application
    {
        get
        {
            _application ??= ApplicationAdapter.Instance;
            return _application;
        }
        set => _application = value;
    }

    /// <summary>
    /// Updates the SDK's internal pause status
    /// </summary>
    public void UpdatePauseStatus(bool paused)
    {
        if (paused && _isRunning)
        {
            _isRunning = false;
            ApplicationPausing?.Invoke();
        }
        else if (!paused && !_isRunning)
        {
            _isRunning = true;
            ApplicationResuming?.Invoke();
        }
    }

    /// <summary>
    /// To receive Pause events.
    /// </summary>
    internal void OnApplicationPause(bool pauseStatus) => UpdatePauseStatus(pauseStatus);

    /// <summary>
    /// To receive Focus events.
    /// </summary>
    /// <param name="hasFocus"></param>
    internal void OnApplicationFocus(bool hasFocus) => UpdatePauseStatus(!hasFocus);

    // The GameObject has to destroy itself since it was created with HideFlags.HideAndDontSave
    private void OnApplicationQuit() => Destroy(gameObject);

    private void Awake()
    {
        // This prevents object from being destroyed when unloading the scene since using HideFlags.HideAndDontSave
        // doesn't guarantee its persistence on all platforms i.e. WebGL
        // (see https://github.com/getsentry/sentry-unity/issues/1678 for more details)
        DontDestroyOnLoad(gameObject);
    }
}
