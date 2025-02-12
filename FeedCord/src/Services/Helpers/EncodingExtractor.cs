using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;

namespace FeedCord.Services.Helpers;

public static partial class EncodingExtractor
{
    // Setting maximum bytes in case we are reading a large feed here
    private const int MAX_BYTES = 2048;
    public static string ConvertBytesByComparing(byte[] bytes, HttpContentHeaders headers)
    {
        var serverDeclaredEncoding = headers?.ContentType?.CharSet;

        if (string.IsNullOrEmpty(serverDeclaredEncoding))
        {
            serverDeclaredEncoding = "utf-8";
        }

        var xmlDeclaredEncoding = TryGuessXmlEncoding(bytes);
        
        // Currently always trusting RSS Prolog declaration
        var finalDeclaredEncoding = !string.IsNullOrEmpty(xmlDeclaredEncoding) ?
            xmlDeclaredEncoding : 
            serverDeclaredEncoding;
        
        try
        {
            var finalEncoding = Encoding.GetEncoding(finalDeclaredEncoding);
            return finalEncoding.GetString(bytes);
        }
        catch
        {
            return Encoding.UTF8.GetString(bytes);
        }
    }

    private static string? TryGuessXmlEncoding(byte[] bytes)
    {
        var length = Math.Min(bytes.Length, MAX_BYTES);

        var prologContent = Encoding.ASCII.GetString(bytes, 0, length);
        
        var match = EncodingRegex().Match(prologContent);
        
        return match is { Success: true, Groups.Count: > 1 } ? 
            match.Groups[1].Value.Trim() : 
            null;
    }

    [GeneratedRegex("encoding\\s*=\\s*[\"']([^\"']+)[\"']", RegexOptions.IgnoreCase, "en-US")]
    private static partial Regex EncodingRegex();
}