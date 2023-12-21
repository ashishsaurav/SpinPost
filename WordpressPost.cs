using System;
using System.Reflection.Metadata;
using System.Text.RegularExpressions;
using static System.Runtime.InteropServices.JavaScript.JSType;
using RestSharp;
using Newtonsoft.Json;

namespace SpinPost
{
    public class RSSFeed
    {
        public int PostId { get; set; }
        public string Title { get; set; }
        public string Links { get; set; }
        public string Content { get; set; }
    }
    #region jsonclass
    // Root myDeserializedClass = JsonConvert.DeserializeObject<List<Root>>(myJsonResponse);
    public class Content
    {
        public string rendered { get; set; }
        public bool @protected { get; set; }
    }

    public class Root
    {
        public int id { get; set; }
        public DateTime modified { get; set; }
        public Title title { get; set; }
        public Content content { get; set; }
    }

    public class Title
    {
        public string rendered { get; set; }
    }
    #endregion

    public class WordpressPost
	{
        public DateTime LastModifiedDate { get; set; }
        public string FeedURL { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string WebsiteUrl { get; set; }
        public string Authorization { get; set; }
        public WordpressPost(DynamicParam param)
        {
            LastModifiedDate = Convert.ToDateTime(param.LastModifiedDate);
            FeedURL = param.FeedURL;
            Username = param.Username;
            Password = param.Password;
            WebsiteUrl = param.WebsiteUrl;
            Authorization = param.Authorization;
        }
        public List<RSSFeed> GetAllPost()
        {
            List<RSSFeed> feed = new List<RSSFeed>();
            try
            {
                int i = 1;
                while (true)
                {
                    var client = new RestClient();
                    var request = new RestRequest(FeedURL + "?_fields=id,content,title,modified&per_page=100&page="+i, Method.Get);
                    request.AddHeader("Authorization", Authorization);
                    RestResponse response = client.ExecuteAsync(request).Result;
                    if (response.StatusCode.ToString() == "Unauthorized")
                        return feed;
                    var posts = JsonConvert.DeserializeObject<List<Root>>(response.Content);

                    var postToUpdate = posts.Where(x => x.modified < LastModifiedDate).ToList();
                    foreach (var post in postToUpdate)
                    {
                        feed.Add(new RSSFeed
                        {
                            PostId = post.id,
                            Title = post.title.rendered,
                            Content = ExtractTextFromHTML(post.content.rendered),
                            Links = "", //ParseHypelink(post.content.rendered)
                        });
                    }
                    i++;
                }
                return feed;
            }
            catch(Exception)
            {
                return feed;
            }
        }
        public string InsertPost(RSSFeed rssFeed)
        {
            try
            {
                var client = new RestClient();
                var request = new RestRequest(FeedURL + "/" + rssFeed.PostId, Method.Post);
                request.AddHeader("Authorization", Authorization);
                request.AddHeader("Content-Type", "application/x-www-form-urlencoded");
                request.AddParameter("title", rssFeed.Title);
                request.AddParameter("content", rssFeed.Content);
                RestResponse response = client.ExecuteAsync(request).Result;
                if (response.StatusCode.ToString().ToLower() != "ok")
                    return "Unauthorized";
                else
                    return "Ok";
            }
            catch(Exception)
            {
                throw;
            }
        }
        public string ExtractTextFromHTML(string html)
        {
            try
            {
                Regex rRemScript = new Regex(@"<script[^>]*>[\s\S]*?</script>");
                var text = rRemScript.Replace(html, "");
                Regex rUrlScript = new Regex(@"href=""http(s)?://(" + WebsiteUrl + @"(?:[\w-])+.)+[\w-]+([/$&+,:;=?@#|'.^*()%!-""]*)?");
                text = rUrlScript.Replace(text, "");
                /*const string tagWhiteSpace = @"(>|$)(\W|\n|\r)+<";
                const string stripFormatting = @"<[^>]*(>|$)";
                const string lineBreak = @"<(br|BR)\s{0,1}\/{0,1}>";
                var lineBreakRegex = new Regex(lineBreak, RegexOptions.Multiline);
                var stripFormattingRegex = new Regex(stripFormatting, RegexOptions.Multiline);
                var tagWhiteSpaceRegex = new Regex(tagWhiteSpace, RegexOptions.Multiline);

                text = System.Net.WebUtility.HtmlDecode(text);
                text = tagWhiteSpaceRegex.Replace(text, "><");
                text = lineBreakRegex.Replace(text, Environment.NewLine);
                text = stripFormattingRegex.Replace(text, string.Empty);*/

                return text;
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.Message);
                return html;
            }
        }
        public string ParseHypelink(string html)
        {
            var websiteurl = WebsiteUrl.Split(',').ToList();
            string readmore = "<br><h3> Read More From Official Website :- </h3> <br>";
            List<string> urlList = new List<string>();
            foreach(var web in websiteurl)
            {
                var url = Regex.Match(html, @"http(s)?://((?:"+web+@")+.)+[\w-]+(/[\w-.]*)?");
                if(!string.IsNullOrEmpty(url.ToString()) && !url.ToString().Contains("wp-"))
                    urlList.Add("<a href=\"" + url.ToString() + "\">" + url.ToString() + "</a>");
            }
            readmore += string.Join("<br>", urlList.Distinct());
            return readmore;
        }
    }
}

