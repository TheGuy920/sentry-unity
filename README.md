<p align="center">
  <a href="https://sentry.io" target="_blank" align="left">
    <img src="https://raw.githubusercontent.com/getsentry/sentry-unity/main/.github/sentry-wordmark-dark-400x119.svg" width="280">
  </a>
  <a href="https://docs.sentry.io/platforms/unity/" target="_blank" align="right">
    <img src="https://raw.githubusercontent.com/getsentry/sentry-unity/main/.github/unity-verified-logo.svg" width="280">
  </a>
  <br />
</p>
<p align="center">

Sentry SDK for Unity (Newtonsoft.Json EDITION)
===========

> [!IMPORTANT]
> This is a port of [Sentry.Unity](https://github.com/getsentry/sentry-unity) that uses [Newtonsoft.Json](https://nuget.org/packages/newtonsoft.json) instead of [System.Text.Json](https://www.nuget.org/packages/System.Text.Json)
> It ***only*** targets [netstandard 2.1](https://learn.microsoft.com/en-us/dotnet/standard/net-standard?tabs=net-standard-2-1)
> It supports multiple instances running side-by-side
> It only tracks exceptions that include the initializing assembly
  ```cs
  MyAssembly.Main() { SentryUnity.Init }
  ...
  [Log] MyAssembly.MyClass.Function threw an exception: ...
  ```

> [!TIP]
> Do ***NOT*** call `Sentry.Init`
> Call `SentryUnity.Init` instead!
> You must wait until Unity has loaded, e.g. `Entry.Awake` before calling `Init`


## Unity Versions
It was tested and confirmed to work with the following unity versions
- [2022.3.1](https://unity.com/releases/editor/whats-new/2022.3.1)

## Reason
It resolves the `System.TypeLoadException` stemming from Mono implementation differences in [netstandard 2.1](https://learn.microsoft.com/en-us/dotnet/standard/net-standard?tabs=net-standard-2-1)
- [https://github.com/godotengine/godot/issues/42271#55](https://github.com/godotengine/godot/issues/42271#issuecomment-773703955)
- [https://github.com/godotengine/godot/issues/42271#95](https://github.com/godotengine/godot/issues/42271#issuecomment-774456295)
- [https://discussions.unity.com/t/unity-future-net-development-status/](https://discussions.unity.com/t/unity-future-net-development-status/836646/402?page=21)
- [https://github.com/getsentry/sentry-unity/issues/1777](https://github.com/getsentry/sentry-unity/issues/1777)

> [!CAUTION]
> I completely destroyed whatever workflow they had going on in an effort to get it compiling, ported, and done with

___

[![build](https://github.com/getsentry/sentry-unity/workflows/CI/badge.svg?branch=main)](https://github.com/getsentry/sentry-unity/actions?query=branch%3Amain)
[![Discord Chat](https://img.shields.io/discord/621778831602221064?logo=discord&logoColor=ffffff&color=7389D8)](https://discord.gg/PXa5Apfe7K)

## Documentation

Sentry has extensive documentation for this SDK.

Check the Sentry [Unity documentation for more details](https://docs.sentry.io/platforms/unity/).

Blog posts: 
* [How to get started with Sentry's Unity SDK](https://blog.sentry.io/how-to-get-started-with-sentrys-unity-sdk-part-1/)
* [Enabling Out-of-the-Box Performance Insights in Unity Games with the Sentry SDK](https://sentry.engineering/blog/enabling-out-of-the-box-performance-insights-in-the-unity-sdk)


## Setup Sentry SDK with Unity

[![YouTube Video of Setup Sentry SDK with Unity](https://img.youtube.com/vi/P0E9upOSznE/0.jpg)](https://www.youtube.com/watch?v=P0E9upOSznE)

## Resources

* [![Documentation](https://img.shields.io/badge/documentation-sentry.io-green.svg)](https://docs.sentry.io/platforms/unity/)
* [![Discussions](https://img.shields.io/github/discussions/getsentry/sentry-unity.svg)](https://github.com/getsentry/sentry-unity/discussions)
* [![Discord Chat](https://img.shields.io/discord/621778831602221064?logo=discord&logoColor=ffffff&color=7389D8)](https://discord.gg/PXa5Apfe7K)  
* [![Stack Overflow](https://img.shields.io/badge/stack%20overflow-sentry-green.svg)](http://stackoverflow.com/questions/tagged/sentry)
* [![Twitter Follow](https://img.shields.io/twitter/follow/getsentry?label=getsentry&style=social)](https://twitter.com/intent/follow?screen_name=getsentry)
