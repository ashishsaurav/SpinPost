using System;
using System.Collections.Specialized;
using System.Net;
using System.Text;
using System.Text.Json.Nodes;

namespace SpinPost
{
	public class SpinWriter
	{
		public SpinWriter()
		{
		}
        public string SpinContent(string content)
        {
            try
            {
                string json = String.Empty;
                NameValueCollection data = new NameValueCollection();
                data["email_address"] = "";
                data["api_key"] = "";
                data["action"] = "unique_variation";
                data["add_html_markup"] = "false";
                data["use_html_linebreaks"] = "false";
                data["auto_protected_terms"] = "true";
                data["text"] = content;
                var client = new WebClient();
                var response = client.UploadValues("http://www.spinrewriter.com/action/api", "POST", data);
                json = Encoding.UTF8.GetString(response);
                var result = JsonValue.Parse(json);
                if (result["response"].ToString().Contains("API quota exceeded"))
                    return "No";
                if (result["response"].ToString().Contains("7 seconds"))
                    return "Wait";
                return result["response"].ToString();
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}

