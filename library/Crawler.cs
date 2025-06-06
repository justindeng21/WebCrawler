using System.Collections.Concurrent;
using HtmlAgilityPack;
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
        public static async Task CrawlAsync(string startingUrl, int limit)
        {
            var toCrawl = new ConcurrentQueue<string>();
            var crawledUrls = new ConcurrentDictionary<string, byte>();
            var semaphore = new SemaphoreSlim(maxConcurrentThreads);
            var tasks = new List<Task>();
            toCrawl.Enqueue(startingUrl);

            while (crawledUrls.Count < limit)
            {
                if (toCrawl.TryDequeue(out var currentUrl))
                {
                    if (!Uri.IsWellFormedUriString(currentUrl, UriKind.Absolute))
                    {
                        Console.WriteLine($"Err: Malformed URL - {currentUrl}");
                        continue;
                    }

                    await semaphore.WaitAsync();
                    var task = Task.Run(async () =>
                    {
                        try
                        {
                            var responseMessage = await HtmlFetcher.GetHtml(currentUrl);
                            if (responseMessage != null && responseMessage.IsSuccessStatusCode)
                            {
                                var content = await responseMessage.Content.ReadAsStringAsync();
                                var anchorTags = HtmlParser.ParseAnchorTags(content);
                                var uri = new Uri(currentUrl);
                                crawledUrls.TryAdd(currentUrl, 0);
                                if (crawledUrls.Count >= limit) return;
                                if (anchorTags != null)
                                {
                                    foreach (HtmlNode node in anchorTags)
                                    {
                                        if (node.Attributes["href"] == null) continue;
                                        var href = node.Attributes["href"].Value;
                                        try
                                        {
                                            var absoluteUrl = new Uri(uri, href).AbsoluteUri;
                                            if (!crawledUrls.ContainsKey(absoluteUrl) && !toCrawl.Contains(absoluteUrl) && crawledUrls.Count < limit) toCrawl.Enqueue(absoluteUrl);

                                        }
                                        catch (UriFormatException err)
                                        {
                                            Console.WriteLine("Err: Uri Format exception");
                                        }
                                    }
                                }

                            }
                            else
                            {
                                Console.WriteLine($"Err: Failed to fetch");
                            }
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
                    await Task.Delay(50);
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

