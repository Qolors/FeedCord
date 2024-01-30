using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeedCord.src.Common.Interfaces
{
    public interface IDiscordPayloadService
    {
        StringContent BuildForumWithPost(Post post);
        StringContent BuildPayloadWithPost(Post post);
    }
}
