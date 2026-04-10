using System;
using System.Collections.Generic;
using Jellyfin.Plugin.SessionDeleter.Configuration;
using MediaBrowser.Common.Configuration;
using MediaBrowser.Common.Plugins;
using MediaBrowser.Model.Plugins;
using MediaBrowser.Model.Serialization;

namespace Jellyfin.Plugin.SessionDeleter;

/// <summary>
/// Plugin entrypoint.
/// </summary>
public class SessionDeleterPlugin : BasePlugin<PluginConfiguration>, IHasWebPages
{
    private readonly Guid _id = new("a4c1f9d2-7e3b-4f6a-9d8c-2b5e1f0a3c7d");

    /// <summary>
    /// Initializes a new instance of the <see cref="SessionDeleterPlugin"/> class.
    /// </summary>
    /// <param name="applicationPaths">Instance of the <see cref="IApplicationPaths"/> interface.</param>
    /// <param name="xmlSerializer">Instance of the <see cref="IXmlSerializer"/> interface.</param>
    public SessionDeleterPlugin(IApplicationPaths applicationPaths, IXmlSerializer xmlSerializer)
        : base(applicationPaths, xmlSerializer)
    {
        Instance = this;
    }

    /// <summary>
    /// Gets the current plugin instance.
    /// </summary>
    public static SessionDeleterPlugin? Instance { get; private set; }

    /// <inheritdoc />
    public override Guid Id => _id;

    /// <inheritdoc />
    public override string Name => "Session Deleter";

    /// <inheritdoc />
    public override string Description => "Deletes the least recently used device sessions for users who exceed the max session limit.";

    /// <inheritdoc />
    public IEnumerable<PluginPageInfo> GetPages()
    {
        yield return new PluginPageInfo
        {
            Name = "configPage",
            EmbeddedResourcePath = GetType().Namespace + ".Configuration.configPage.html"
        };
    }
}
