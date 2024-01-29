using FeedCord.src.Common;
using FeedCord.src.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeedCord.src.Factories.Interfaces
{
    public interface INotifierFactory
    {
        INotifier Create(Config config);
    }
}
