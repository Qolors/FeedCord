using FeedCord.src.Common;
using FeedCord.src.Common.Interfaces;
using FeedCord.src.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FeedCord.src.DiscordNotifier
{
    internal class Notifier : INotifier
    {
        private HttpClient httpClient;
        private string webhook;
        public Notifier(Config config, IHttpClientFactory httpClientFactory) 
        {
            httpClient = httpClientFactory.CreateClient();
            webhook = config.Webhook;
        }
        public async Task SendNotificationsAsync(List<Post> newPosts)
        {
            foreach (Post post in newPosts)
            {
                var content = PayloadService.BuildPayloadWithPost(post);

                if (content is null)
                    continue;

                var response = await httpClient.PostAsync(webhook, content);
                response.EnsureSuccessStatusCode();

            }
        }
    }
}
