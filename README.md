# SyncStream
SyncStream is a desktop application designed to stream audio simultaneously to multiple Bluetooth devices.
# SyncStream

SyncStream is a desktop application designed to stream audio simultaneously to multiple Bluetooth devices. It uses the Microsoft Bluetooth stack and supports fallback mechanisms (e.g., virtual audio devices) if direct Bluetooth streaming fails.

## Features
- Discover and connect to multiple Bluetooth devices.
- Stream audio directly to connected devices.
- Fallback mechanism using a virtual audio device if Bluetooth streaming fails.
- Modern and responsive UI with a dark theme.

## Prerequisites
Before running the application, ensure you have the following installed:
1. **.NET Framework 4.8 SDK**: Download it from [here](https://dotnet.microsoft.com/download/dotnet-framework/net48).
2. **NAudio Library**: For audio capture and playback.
3. **32feet.NET Library**: For Bluetooth communication.
4. **CSCore Library**: For advanced audio processing.

You can install the required NuGet packages by running:
```bash
dotnet add package NAudio
dotnet add package 32feet.NET
dotnet add package CSCore

dotnet restore
dotnet build( run application )
Note: This applications may not work well on few Pc's .
