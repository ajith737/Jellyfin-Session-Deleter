using MediaBrowser.Model.Plugins;

namespace Jellyfin.Plugin.SessionDeleter.Configuration;

/// <summary>
/// Plugin configuration.
/// </summary>
public class PluginConfiguration : BasePluginConfiguration
{
    /// <summary>
    /// Gets or sets the maximum number of sessions (devices) allowed per user.
    /// </summary>
    public int MaxSessionsPerUser { get; set; } = 5;

    /// <summary>
    /// Gets or sets a value indicating whether the task should run in dry-run mode.
    /// </summary>
    public bool DryRun { get; set; }
}
