using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeedCord.src.Common.Interfaces
{
    internal interface IRssProcessorService
    {
        Task<Post> ParseRssFeedAsync(string xmlContent);
    }
}
