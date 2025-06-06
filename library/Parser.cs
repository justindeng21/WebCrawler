using HtmlAgilityPack;


namespace Parser {

    public static class HtmlParser
    {
        public static HtmlNodeCollection? ParseAnchorTags(string htmlString)
        {
            var doc = new HtmlDocument();
            doc.LoadHtml(htmlString);
            return doc.DocumentNode.SelectNodes("//a");
        }


    }


}



