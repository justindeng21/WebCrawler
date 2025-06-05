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
        public static async Task Crawl(string startingUrl, int limit)
        {
            var toCrawl = new Queue<string> {};
            var crawledUrls = new HashSet<string> {};
            toCrawl.Enqueue(startingUrl);
            
            while (toCrawl.Count > 0 && crawledUrls.Count < limit)
            {
                var currentUrl = toCrawl.Dequeue();
                if (!Uri.IsWellFormedUriString(currentUrl, UriKind.Absolute))
                {
                    Console.WriteLine($"Malformed URL:{currentUrl}");
                    continue;
                }

                try
                {
                    var responseMessage = await HtmlFetcher.GetHtml(currentUrl);
                    if (!responseMessage.IsSuccessStatusCode)
                    {
                        Console.WriteLine($"Failed to fetch {currentUrl}\nStatus Code: {responseMessage.StatusCode}");
                        continue;
                    }

                    else
                    {
                        var content = await responseMessage.Content.ReadAsStringAsync();
                        var anchorTags = HtmlParser.ParseAnchorTags(content);
                        crawledUrls.Add(currentUrl);
                        var uri = new Uri(currentUrl);
                        if (anchorTags == null) continue;
                        foreach (HtmlNode node in anchorTags)
                        {
                            var href = node.Attributes["href"].Value;
                            if (href == null) continue;
                            try
                            {
                                var absoluteUrl = new Uri(uri, href).AbsoluteUri;
                                if (!crawledUrls.Contains(absoluteUrl) && !toCrawl.Contains(absoluteUrl)) toCrawl.Enqueue(absoluteUrl);
                                
                            }
                            catch (UriFormatException err)
                            {
                                Console.WriteLine(err);
                            }
                        }
                        
                    }
                }

                catch (Exception e)
                {
                    Console.WriteLine("Failed to fetch file");
                    Console.WriteLine(e);
                }
            }
            PrintCrawledLinks(crawledUrls);
        }

        public static void PrintCrawledLinks(HashSet<string> crawledUrls){
            foreach (string url in crawledUrls)
            {
                Console.WriteLine(url);
            }
        }
    }
}

