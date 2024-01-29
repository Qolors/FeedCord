using FeedCord.src.Common;
using FeedCord.src.Common.Interfaces;
using FeedCord.src.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeedCord.src.Factories.Interfaces
{
    public interface IRssCheckerBackgroundServiceFactory
    {
        RssCheckerBackgroundService Create(Config config, IFeedProcessor feedProcessor, INotifier notifier);
    }
}
