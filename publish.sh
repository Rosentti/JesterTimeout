#!/bin/sh
dotnet publish -c Release
zip JesterTimeout-1.0.1.zip bin/Release/netstandard2.1/publish/JesterTimeout.dll CHANGELOG.md README.md icon.png LICENSE manifest.json
