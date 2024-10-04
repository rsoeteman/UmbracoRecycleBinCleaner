using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.BackgroundJobs;

namespace RecycleBinCleaner;

public class MediaRecycleBinCleaner(
    IMediaService _mediaService,
    ILogger<MediaRecycleBinCleaner> _logger)
    : IRecurringBackgroundJob
{
    public Task RunJobAsync()
    {
        try
        {
            var maxItemsToDelete = 25;
            var compare = DateTime.Now.AddMonths(-1).Ticks;
            var all = _mediaService.GetPagedMediaInRecycleBin(0, maxItemsToDelete, out var total)
                .Where(p => p.UpdateDate.Ticks < compare).ToList();
            if (all.Count == 0)
            {
                _logger.LogInformation("Nothing to delete in the media recycle bin");
            }
            else
            {
                all.ForEach(x =>
                {
                    _logger.LogDebug("Deleting media item '{docName}' from the recycle bin.", x.Name);
                    _mediaService.Delete(x);
                });

                _logger.LogInformation(
                    "Media bin {total} items deleted. If more than {maxItemsToDelete} old media items exists in recycle bin more items will be deleted next run",
                    all.Count,
                    maxItemsToDelete);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,"Error during deleting of items in the  Media Recycle Bin");
        }

        return Task.CompletedTask;
    }

    //Run this task every hour/
    public TimeSpan Period => TimeSpan.FromMinutes(60);

    //Let's give the server some time to boot up
    public TimeSpan Delay => TimeSpan.FromMinutes(2);

    public event EventHandler? PeriodChanged;
}