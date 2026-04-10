using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Jellyfin.Data.Queries;
using MediaBrowser.Controller.Devices;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Tasks;
using Microsoft.Extensions.Logging;

namespace Jellyfin.Plugin.SessionDeleter.ScheduledTasks;

/// <summary>
/// Scheduled task that deletes excess device sessions per user.
/// </summary>
public class SessionDeleterTask : IScheduledTask
{
    private readonly IDeviceManager _deviceManager;
    private readonly IUserManager _userManager;
    private readonly ILogger<SessionDeleterTask> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="SessionDeleterTask"/> class.
    /// </summary>
    /// <param name="deviceManager">Instance of the <see cref="IDeviceManager"/> interface.</param>
    /// <param name="userManager">Instance of the <see cref="IUserManager"/> interface.</param>
    /// <param name="logger">Instance of the <see cref="ILogger{SessionDeleterTask}"/> interface.</param>
    public SessionDeleterTask(
        IDeviceManager deviceManager,
        IUserManager userManager,
        ILogger<SessionDeleterTask> logger)
    {
        _deviceManager = deviceManager;
        _userManager = userManager;
        _logger = logger;
    }

    /// <inheritdoc />
    public string Name => "Session Deleter";

    /// <inheritdoc />
    public string Key => "SessionDeleter";

    /// <inheritdoc />
    public string Description => "Deletes the least recently used device sessions for users who exceed the max session limit.";

    /// <inheritdoc />
    public string Category => "Administration";

    /// <inheritdoc />
    public async Task ExecuteAsync(IProgress<double> progress, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(SessionDeleterPlugin.Instance?.Configuration);

        var maxSessions = SessionDeleterPlugin.Instance.Configuration.MaxSessionsPerUser;
        var dryRun = SessionDeleterPlugin.Instance.Configuration.DryRun;
        var users = _userManager.Users.ToList();
        var processedUsers = 0;

        foreach (var user in users)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var deviceResult = _deviceManager.GetDevices(new DeviceQuery { UserId = user.Id });
            var devices = deviceResult.Items;

            if (devices.Count <= maxSessions)
            {
                processedUsers++;
                progress.Report((double)processedUsers / users.Count * 100);
                continue;
            }

            var sortedDevices = devices
                .OrderBy(d => d.DateLastActivity)
                .ToList();

            var devicesToDelete = sortedDevices.Take(devices.Count - maxSessions);
            var idType = sortedDevices[0].Id.GetType();
            var deleteById = _deviceManager.GetType()
                .GetMethods()
                .FirstOrDefault(m => m.Name == "DeleteDevice"
                    && m.GetParameters().Length == 1
                    && m.GetParameters()[0].ParameterType == idType);

            foreach (var device in devicesToDelete)
            {
                if (dryRun)
                {
                    _logger.LogInformation(
                        "[Dry Run] Would delete device for user {Username}: {DeviceName} (Last active: {DateLastActivity})",
                        user.Username,
                        device.DeviceName,
                        device.DateLastActivity);
                    continue;
                }

                _logger.LogInformation(
                    "Deleting device for user {Username}: {DeviceName} (Last active: {DateLastActivity})",
                    user.Username,
                    device.DeviceName,
                    device.DateLastActivity);

                if (deleteById is not null)
                {
                    var deleteTask = (Task?)deleteById.Invoke(_deviceManager, new object[] { device.Id });
                    if (deleteTask is not null)
                    {
                        await deleteTask.ConfigureAwait(false);
                    }
                }
                else
                {
                    await _deviceManager.DeleteDevice(device).ConfigureAwait(false);
                }
            }

            processedUsers++;
            progress.Report((double)processedUsers / users.Count * 100);
        }

        progress.Report(100);
    }

    /// <inheritdoc />
    public IEnumerable<TaskTriggerInfo> GetDefaultTriggers()
    {
        yield return new TaskTriggerInfo
        {
            Type = TaskTriggerInfoType.DailyTrigger,
            TimeOfDayTicks = TimeSpan.FromHours(4).Ticks
        };
    }
}
