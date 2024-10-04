using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Extensions;

namespace RecycleBinCleaner.Composers;

public class AddBackGroundTasksComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        builder.Services.AddRecurringBackgroundJob<ContentRecycleBinCleaner>();
        builder.Services.AddRecurringBackgroundJob<MediaRecycleBinCleaner>();
    }
}