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
                    //声明 html 用来接收爬虫数据
                    string html = null;
                    //获取 Response 响应流
                    HttpWebResponse res = (HttpWebResponse)req.GetResponse();
                    using (StreamReader reader = new StreamReader(res.GetResponseStream()))
                    {
                        //为 html 赋值。
                        html = reader.ReadToEnd();
                        if (!string.IsNullOrEmpty(html))    //如果 html 不为空则打印响应的日志
                        {
                            Console.WriteLine("Download OK!\n");
                        }
                    }

                    //设置 url 数组 
                    string[] links = GetLinks(html);
                    //将 url 添加进去
                    AddUrls(links, depth + 1, baseUrl, unload, loaded);
                }
                catch (WebException we)
                {
                    //遇到异常则打印出来
                    Console.WriteLine(we.Message);
                }
            }
        }

        private static string[] GetLinks(string html)       //根据传入的 html 代码获取代码中的 url 链接
        {
            //设置一个正则表达式 过滤非 url
            const string pattern = @"http://([\w-]+\.)+[\w-]+(/[\w- ./?%&=]*)?";

            //Regex 声明 正则对象 pattern 表示匹配的正则表达式
            Regex r = new Regex(pattern, RegexOptions.IgnoreCase);
            //表示已迭代方式将正则表达式模式应用于输入输入字符串所找到的成功匹配的集合
            MatchCollection m = r.Matches(html);
            //创建一个数组数组长度为 迭代的集合长度
            string[] links = new string[m.Count];
            //循环迭代集合将集合中的之放到 数组中
            for (int i = 0; i < m.Count; i++)
            {
                links[i] = m[i].ToString();
            }
            //返回这个集合
            return links;
        }


        //获取可获得页面的url  传入 url  , 未下载的集合 和 已下载的集合
        private static bool UrlAvailable(string url, Dictionary<string, int> unload, Dictionary<string, int> loaded)
        {
            //如果 未下载的集合中包含这个 url 或 已下载的集合中包含这个url 
            if (unload.ContainsKey(url) || loaded.ContainsKey(url))
            {
                //直接返回false ,因为已经加入到 集合中说明 url 已经判断过了不会错
                return false;
            }
            //如果 url 中包含 .jpg  或 .gif 等指向的静态文件则返回false
            if (url.Contains(".jpg") || url.Contains(".gif")
                || url.Contains(".png") || url.Contains(".css")
                || url.Contains(".js"))
            {
                return false;
            }
            return true;    //否则返回 true ,表示这个 url 是可以获得页面的
        }

        //添加 url 方法
        private static void AddUrls(string[] urls, int depth, string baseUrl, Dictionary<string, int> unload, Dictionary<string, int> loaded)
        {
            //如果深度大于等于3则直接返回
            if (depth >= 3)
            {
                return;
            }
            //使用 foreach 循环 数组 urls
            foreach (string url in urls)
            {
                //去除 url 的空格
                string cleanUrl = url.Trim();
                //获取第一个在 url 中第一个空格索引的位置
                int end = cleanUrl.IndexOf(' ');
                //如果这个索引位置大于0 
                if (end > 0)
                {
                    //截取字符串 cleanUrl 从0开始到结束位置
                    cleanUrl = cleanUrl.Substring(0, end);
                }
                //进行判断 如果 属于可获得页面的 url
                if (UrlAvailable(cleanUrl, unload, loaded))
                {
                    //如果新的url 中包含 基础url 的字符串
                    if (cleanUrl.Contains(baseUrl))
                    {
                        //在未加载的集合中添加 键为新的url 值为url深度
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
