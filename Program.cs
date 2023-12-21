using System.Reflection;
using System.Security.Cryptography;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SpinPost;
public class Program
{
    public static void Main()
    {
        try
        {
            Console.WriteLine("Starting Update Wordpress Post");
            var startup = new Startup();
            WordpressPost wordpressPost = new WordpressPost(startup.DynamicParam);
            SpinWriter spinWriter = new SpinWriter();
            var rssFeeds = wordpressPost.GetAllPost();
            if (rssFeeds == null || rssFeeds.Count <= 0)
                Console.WriteLine("No Old Wordpress Post Present");
            else
                Console.WriteLine("Fetched " + rssFeeds.Count() + " Wordpress Post");
            int i = 1;
            foreach (var feed in rssFeeds)
            {
                try
                {
                    Console.WriteLine("Starting Content Spinning - " + feed.PostId);
                    var spinnedContent = spinWriter.SpinContent(feed.Content);
                    if (spinnedContent == "No")
                        break;
                    if (spinnedContent == "Wait")
                    {
                        System.Threading.Thread.Sleep(7000);
                        spinnedContent = spinWriter.SpinContent(feed.Content);
                    }
                    feed.Content = spinnedContent + feed.Links;
                    Console.WriteLine("Content Spinned Successfully");
                    var response = wordpressPost.InsertPost(feed);
                    if (response == "Unauthorized")
                        break;
                    Console.WriteLine("Updated Wordpress Post Succesfully - " + feed.PostId + " Post No - " + i);
                    System.Threading.Thread.Sleep(7000);
                    i++;
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
        }
        catch(Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
    static void BuildConfig(IConfigurationBuilder builder)
    {
        builder.SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", optional: true);
    }

}
public class Startup
{
    public Startup()
    {
        var path = Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
        var builder = new ConfigurationBuilder()
                  .SetBasePath(Path.GetDirectoryName(Assembly.GetEntryAssembly().Location))
                  .AddJsonFile("appsettings.json", optional: false);
        IConfiguration config = builder.Build();
        DynamicParam = config.GetSection("DynamicParam").Get<DynamicParam>();
    }

    public DynamicParam DynamicParam { get; private set; }
}
public class DynamicParam
{
    public string FeedURL { get; set; }
    public string Authorization { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public string LastModifiedDate { get; set; }
    public string WebsiteUrl { get; set; }
}