using WebCrawler;

var watch = System.Diagnostics.Stopwatch.StartNew();
await Crawler.Crawl("https://web-scraping.dev/", 50);
watch.Stop();
var elapsedMs = watch.ElapsedMilliseconds;
Console.WriteLine($"Scan time: {elapsedMs}ms");








