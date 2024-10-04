using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Infrastructure.BackgroundJobs;

namespace RecycleBinCleaner;

public class ContentRecycleBinCleaner(
    IContentService _contentService,
    ILogger<ContentRecycleBinCleaner> _logger)
    : IRecurringBackgroundJob
{
    public Task RunJobAsync()
    {
        try
        {
            var maxItemsToDelete = 25;
            var compare = DateTime.Now.AddMonths(-1).Ticks;
            var all = _contentService.GetPagedContentInRecycleBin(0, maxItemsToDelete, out var total)
                .Where(p => p.UpdateDate.Ticks < compare).ToList();
            if (all.Count == 0)
            {
                _logger.LogInformation("Nothing to delete in the content recycle bin");
            }
            else
            {
                all.ForEach(x =>
                {
                    _logger.LogDebug("Deleting document '{docName}' from the recycle bin.", x.Name);
                    _contentService.Delete(x);
                });

                _logger.LogInformation(
                    "Content bin {total} items deleted. If more than {maxItemsToDelete} old documents exists in recycle bin more items will be deleted next run",
                    all.Count,
                    maxItemsToDelete);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,"Error during deleting  of items in the Content Recycle Bin");
        }

        return Task.CompletedTask;
    }

    //Run this task every hour
    public TimeSpan Period => TimeSpan.FromMinutes(60);

    //Let's give the server some time to boot up
    public TimeSpan Delay => TimeSpan.FromMinutes(1);

    public event EventHandler? PeriodChanged;
}