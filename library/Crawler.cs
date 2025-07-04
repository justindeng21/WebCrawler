using System.Collections.Concurrent;
using Parser;



namespace WebCrawler{
    public static class HtmlFetcher
    {
        private static readonly HttpClient client = new HttpClient();
        public static async Task<HttpResponseMessage> GetHtml(string url)
        {
            return await client.GetAsync(url);
        }
    }

    public static class Crawler{

        private static int maxConcurrentThreads = 5;
        
        private static int crawledCount = 0;

        public static async Task CrawlAsync(string startingUrl, int limit)
        {
            var toCrawl = new ConcurrentQueue<string>();
            var crawledUrls = new ConcurrentDictionary<string, byte>();
            var semaphore = new SemaphoreSlim(maxConcurrentThreads);
            var tasks = new List<Task>();
            toCrawl.Enqueue(startingUrl);

            var startingUrlDomain = new Uri(startingUrl).Host;

            while (!toCrawl.IsEmpty || semaphore.CurrentCount < maxConcurrentThreads)
            {

                if (toCrawl.TryDequeue(out var currentUrl))
                {
                    if (!Uri.IsWellFormedUriString(currentUrl, UriKind.Absolute))
                    {
                        Console.WriteLine($"Err: Malformed URL = {currentUrl}");
                        continue;
                    }

                    await semaphore.WaitAsync();
                    var task = Task.Run(async () =>
                    {
                        try
                        {

                            int currentCount = Interlocked.Increment(ref crawledCount);
                            if (currentCount > limit)
                            {
                                semaphore.Release();
                                return;
                            }
                            var responseMessage = await HtmlFetcher.GetHtml(currentUrl);
                            if (responseMessage != null && responseMessage.IsSuccessStatusCode)
                            {
                                var content = await responseMessage.Content.ReadAsStringAsync();
                                var anchorTags = HtmlParser.ParseAnchorTags(content);
                                var uri = new Uri(currentUrl);
                                crawledUrls.TryAdd(currentUrl, 0);
                                if (anchorTags != null)
                                {
                                    foreach (var node in anchorTags)
                                    {
                                        if (node.Attributes["href"] == null) continue;
                                        var href = node.Attributes["href"].Value;
                                        try
                                        {
                                            var absoluteUrl = new Uri(uri, href).AbsoluteUri;
                                            var absoluteUri = new Uri(absoluteUrl);
                                            if (!crawledUrls.ContainsKey(absoluteUrl) && !toCrawl.Contains(absoluteUrl) && crawledCount < limit && startingUrlDomain == absoluteUri.Host) toCrawl.Enqueue(absoluteUrl);

                                        }
                                        catch (UriFormatException)
                                        {
                                            Console.WriteLine("Err: Uri Format exception");
                                        }
                                    }
                                }

                            }
                            else if (responseMessage != null)
                            {
                                Console.WriteLine($"Err: Status Code = {responseMessage.StatusCode}");
                            }
                            else
                            {
                                Console.WriteLine("Err: responseMessage is null");
                            }
                        }
                        catch (Exception err)
                        {
                            Console.WriteLine($"Err: Task failed - {err}");
                        }

                        finally
                        {
                            semaphore.Release();
                        }

                    });
                    tasks.Add(task);
                }
                else
                {
                    await Task.Delay(100);
                }

            }
            await Task.WhenAll(tasks);
            PrintCrawledLinks(crawledUrls.Keys);
        }

        public static void PrintCrawledLinks(IEnumerable<string> crawledUrls){
            int count = 1;
            foreach (string url in crawledUrls)
            {
                Console.WriteLine($"{count}:{url}");
                count += 1;
            }
        }
    }
}

