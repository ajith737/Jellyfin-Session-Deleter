# Jellyfin Session Deleter Plugin

A Jellyfin plugin that automatically deletes excess device sessions per user based on a configurable limit.

## About

When the scheduled task runs, it iterates all Jellyfin users and retrieves their registered devices via `IDeviceManager`. If a user has more devices than the configured maximum, the plugin deletes the least recently used devices (sorted by `DateLastActivity` ascending) until the count is at or below the limit.

## Configuration

- **Max Sessions Per User** (default: `5`): The maximum number of device sessions allowed per user. Any devices beyond this limit will be deleted, starting with the oldest (least recently active).

The configuration can be changed in the Jellyfin Dashboard under **Plugins > Session Deleter**.

## Scheduled Task

- **Name**: Session Deleter
- **Category**: Administration
- **Default Trigger**: Daily at 4:00 AM

The task can also be run manually from the Jellyfin Dashboard under **Scheduled Tasks**.

## Installation

Place the compiled `Jellyfin.Plugin.SessionDeleter.dll` into your Jellyfin plugins directory and restart the server.

## Building

Requires the .NET 10 SDK. Targets Jellyfin 12.0.

```bash
dotnet build Jellyfin.Plugin.SessionDeleter/Jellyfin.Plugin.SessionDeleter.csproj --configuration Release
```

## License

This plugin is licensed under the [GNU General Public License v3.0](https://www.gnu.org/licenses/gpl-3.0.html).
