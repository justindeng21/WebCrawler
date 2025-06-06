class Program
{
    static async Task Main(string[] args)
    {
        string startUrl = "https://finance.yahoo.com/";
        int limit = 20;

        var watch = System.Diagnostics.Stopwatch.StartNew();
        await WebCrawler.Crawler.CrawlAsync(startUrl, limit);
        watch.Stop();
        Console.WriteLine($"Scan time: {watch.ElapsedMilliseconds}ms");
        
    }
}




