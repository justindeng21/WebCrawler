class Program
{
    static async Task Main(string[] args)
    {
        string startUrl = "https://finance.yahoo.com/";
        int limit = 20;
        var watch = System.Diagnostics.Stopwatch.StartNew();
        await WebCrawler.Crawler.Crawl(startUrl, limit);
        watch.Stop();
        var elapsedMs = watch.ElapsedMilliseconds;
        Console.WriteLine($"Scan time: {elapsedMs}ms");
        
    }
}




