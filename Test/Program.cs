using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;

namespace Test
{
    class Program
    {
        static void Main(string[] args)
        {
            //为了保存需要下载的 url 同时防止重复下载，我们需要分别用两个集合来存放将要下载的 url 和 已经下载的 url 
            //因为在保存 url 的同时需要保存于 url 相关的一些其他信息， 如深度，所以这是使用 Dictionary 来存放这些 url
            // 具体类型为  Dictionary<string,int> 其中 string是url字符串， int是该url相对于基url的深度
            //每次开始时都会检查未下载的集合，如果已经为空，说明已经下载完毕，如果还有 url ，那么就去除第一个 url 加入到下载的集合中，并且下载这个 url 的资源
            // C# 已经封装好了 HttpWebRequest 和 HttpWebResponse 所以实现起来方便不少
            Dictionary<string, int> unload = new Dictionary<string, int>(); //未下载的
            Dictionary<string, int> loaded = new Dictionary<string, int>(); //已经下载的

            unload.Add("http://news.sina.com.cn/", 0);  //在未下载的集合中添加 url 并制定深度为 0
            string baseUrl = "news.sina.com.cn";        //声明  BaseUrl

            while (unload.Count > 0)                    //如果未下载集合的数量大于0
            {
                string url = unload.First().Key;        //获取位下载集合中的第一个的url
                int depth = unload.First().Value;       //获取对应url的深度
                loaded.Add(url, depth);                 //在已下载集合中加入获取的url
                unload.Remove(url);                     //从未下载集合中移除url

                Console.WriteLine("Now loading " + url);    //打印 现在开始加载 对应的 url 值

                HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
                req.Method = "GET";
                req.Accept = "text/html";
                req.UserAgent = "Mozilla/4.0 (compatible; MSIE 8.0; Windows NT 6.1; Trident/4.0)";

                try
                {
                    string html = null;
                    HttpWebResponse res = (HttpWebResponse)req.GetResponse();
                    using (StreamReader reader = new StreamReader(res.GetResponseStream()))
                    {
                        html = reader.ReadToEnd();
                        if (!string.IsNullOrEmpty(html))
                        {
                            Console.WriteLine("Download OK!\n");
                        }
                    }
                    string[] links = GetLinks(html);
                    AddUrls(links, depth + 1, baseUrl, unload, loaded);
                }
                catch (WebException we)
                {
                    Console.WriteLine(we.Message);
                }
            }
        }

        private static string[] GetLinks(string html)
        {
            const string pattern = @"http://([\w-]+\.)+[\w-]+(/[\w- ./?%&=]*)?";
            Regex r = new Regex(pattern, RegexOptions.IgnoreCase);
            MatchCollection m = r.Matches(html);
            string[] links = new string[m.Count];

            for (int i = 0; i < m.Count; i++)
            {
                links[i] = m[i].ToString();
            }
            return links;
        }

        private static bool UrlAvailable(string url, Dictionary<string, int> unload, Dictionary<string, int> loaded)
        {
            if (unload.ContainsKey(url) || loaded.ContainsKey(url))
            {
                return false;
            }
            if (url.Contains(".jpg") || url.Contains(".gif")
                || url.Contains(".png") || url.Contains(".css")
                || url.Contains(".js"))
            {
                return false;
            }
            return true;
        }

        private static void AddUrls(string[] urls, int depth, string baseUrl, Dictionary<string, int> unload, Dictionary<string, int> loaded)
        {
            if (depth >= 3)
            {
                return;
            }
            foreach (string url in urls)
            {
                string cleanUrl = url.Trim();
                int end = cleanUrl.IndexOf(' ');
                if (end > 0)
                {
                    cleanUrl = cleanUrl.Substring(0, end);
                }
                if (UrlAvailable(cleanUrl, unload, loaded))
                {
                    if (cleanUrl.Contains(baseUrl))
                    {
                        unload.Add(cleanUrl, depth);
                    }
                    else
                    {
                        // 外链
                    }
                }
            }
        }
    }
}
