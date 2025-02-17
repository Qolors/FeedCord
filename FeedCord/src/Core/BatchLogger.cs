using System.Text;
using System.Threading.Tasks.Dataflow;
using FeedCord.Core.Interfaces;
using Microsoft.Extensions.Logging;

namespace FeedCord.Core;

public class BatchLogger : IBatchLogger
{
    private readonly ILogger<BatchLogger> _logger;
    private BufferBlock<LogAggregator> _bufferBlock;
    private ActionBlock<LogAggregator> _processingBlock;

    public BatchLogger(ILogger<BatchLogger> logger)
    {
        _logger = logger;
        
        _bufferBlock = new BufferBlock<LogAggregator>();
        
        _processingBlock = new ActionBlock<LogAggregator>(ProcessLogItem, new ExecutionDataflowBlockOptions 
        { 
            MaxDegreeOfParallelism = 1 // Ensures logs print sequentially
        });
        
        _bufferBlock.LinkTo(_processingBlock, new DataflowLinkOptions { PropagateCompletion = true });
    }

    public async Task ConsumeLogData(LogAggregator logItem)
    {
        await _bufferBlock.SendAsync(logItem);
    }

    private void ProcessLogItem(LogAggregator logItem)
    {
        var batchSummary = new StringBuilder();
        batchSummary.AppendLine($"> Batch Run for {logItem.InstanceId} finished:");
        batchSummary.AppendLine($"> Started At: {logItem.StartTime} | Finished At: {logItem.EndTime}");
    
        if (!logItem.UrlStatuses.IsEmpty)
        {
            int totalUrls = logItem.UrlStatuses.Count;
            int failedCount = logItem.UrlStatuses.Count(kvp => kvp.Value != 200);
            batchSummary.AppendLine($"> {totalUrls} URLs tested with {failedCount} failed responses.");
        
            if (failedCount > 0)
            {
                batchSummary.AppendLine("> The following URLs had bad responses:");
                foreach (var issue in logItem.UrlStatuses.Where(kvp => kvp.Value != 200))
                {
                    var statusText = issue.Value == -99 ? "Request Timed Out" : issue.Value.ToString();
                    batchSummary.AppendLine($"> Url: {issue.Key}, Response Status: {statusText}");
                }
            }
        }
    
        if (logItem.NewPostCount == 0)
        {
            batchSummary.AppendLine("> No new posts found. Posts extracted from feeds:");
            foreach (var (url, post) in logItem.LatestPosts)
            {
                batchSummary.AppendLine($"> Url: {url} | Title: {post?.Title} | Publish Date: {post?.PublishDate}");
            }
        }
        else
        {
            batchSummary.AppendLine($"> {logItem.NewPostCount} new posts found in the feed.");
        }
        
        _logger.LogInformation(batchSummary.ToString());
    
        logItem.Reset();
    }


}