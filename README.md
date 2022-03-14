# Lidgren.Network [![Lidgren.Network/main](https://github.com/AscensionGameDev/Lidgren.Network/workflows/Lidgren.Network%2Fmain/badge.svg)](https://github.com/AscensionGameDev/Lidgren.Network/actions?query=workflow%3ALidgren.Network%2Fmain) [![Nuget](https://img.shields.io/nuget/v/AscensionGameDev.Lidgren.Network?color=%230072b0)](https://www.nuget.org/packages/AscensionGameDev.Lidgren.Network) [![Discord](https://img.shields.io/discord/363106200243535872?color=%237289DA&label=Discord)](https://discord.gg/CvvsVpPMuF)
Lidgren.Network is a networking library for .NET framework, which uses a single UDP socket to deliver a simple API for connecting a client to a server, reading and sending messages.

This has been updated for use with Unity3D, feel free to send PRs for other bugs fixes.
To use this in Unity3D just enable the experimental .NET framework.
you can do this in Edit -> Project Settings -> Player -> Other Settings -> Api Compatibility Level -> .NET 4.6

Platforms supported:
- Linux
- Mac
- OSX

Platforms/Toolchains which need testing:
- Android
- iPhone
- Xamarin

Future Roadmap:
- Update to latest .NET 4.6
- Investigate officially supporting .NET Core.
- Improve test suite so that tests are run on all platforms we support, for each release.
