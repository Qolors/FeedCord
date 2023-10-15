namespace FeedCord.src;

public class Program
{
    public static void Main(string[] args) =>
        new Startup().Initiliaze(args).GetAwaiter().GetResult();
}
