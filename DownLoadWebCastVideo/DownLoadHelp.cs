using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CsharpHttpHelper;
using System.Text.RegularExpressions;
using System.Net.Http;
using System.Net;
using System.IO;

namespace DownLoadWebCastVideo
{
    public static class DownLoadHelp
    {
        public static string MainUrl = "http://msdnwebcast.azurewebsites.net";
        public static List<Course> GetCourseList()
        {
            //CsharpHttpHelper.HttpHelper helper = new HttpHelper();
            //HttpItem item = new HttpItem();
            //item.URL = "http://msdnwebcast.azurewebsites.net/webcast/3/651/";
            //item.Timeout = 50000;
            //HttpResult request = helper.GetHtml(item);
            //string html = request.Html;
            System.Net.HttpWebRequest webrequest = (HttpWebRequest)HttpWebRequest.Create("http://msdnwebcast.azurewebsites.net/webcast/3/651/");
            HttpWebResponse response = (HttpWebResponse)webrequest.GetResponse();  //获取响应，即发送请求
            Stream responseStream = response.GetResponseStream();
            StreamReader streamReader = new StreamReader(responseStream, Encoding.UTF8);
            string html = streamReader.ReadToEnd();


            List<Course> result = new List<Course>();
            List<string> strlist = new List<string>();
            var txts = Regex.Matches(html, "(?is)<tr[^>]*?>(.+?)</tr>").OfType<Match>().Select(x => x.Groups[1].Value);
            foreach (var strtr in txts)
            {
                strlist.Add(strtr);
            }
            string prttern = "<a(\\s+(href=\"(?<url>([^\"])*)\"|'([^'])*'|\\w+=\"(([^\"])*)\"|'([^'])*'))+>(?<text>(.*?))</a>";

            foreach (var itemTr in strlist)
            {
                if (itemTr.IndexOf("<th>") != -1) continue;
                var tdlist = Regex.Matches(itemTr, "(?is)<td>(.+?)</td>").OfType<Match>().Select(x => x.Groups[1].Value).ToList<string>();
                Course entity = new Course();
                var maths = Regex.Matches(tdlist[0], prttern);
                entity.Name = maths[0].Groups["text"].Value;
                entity.Url = maths[0].Groups["url"].Value;
                entity.Type = tdlist[1];
                entity.Num = tdlist[2];
                entity.LastDate = tdlist[3];
                entity.LastNote = tdlist[4];
                result.Add(entity);

            }

            return result;
        }

        public static List<DownLoadInfo> GetDownLoadInfo(Course entity)
        {
            List<DownLoadInfo> result = new List<DownLoadInfo>();

            if (entity == null) return null;

            CsharpHttpHelper.HttpHelper helper = new HttpHelper();
            HttpItem item = new HttpItem();
            item.URL = MainUrl + entity.Url;
            HttpResult httpResult = helper.GetHtml(item);

            List<string> strList = new List<string>();

            Regex reg = new Regex(@"(?m)<div class=""left""[^>]*>(?<div>(?:\w|\W)*?)</div[^>]*>", RegexOptions.Multiline | RegexOptions.IgnoreCase);
            Match mc = reg.Match(httpResult.Html);
            var divLeft = mc.Groups["div"].Value.Trim();
            string[] sdownLoadInfo = divLeft.Split(new string[] { "</ol>" }, StringSplitOptions.RemoveEmptyEntries);
            var txts = Regex.Matches(divLeft, "(?is)<a(.+?)</ol>").OfType<Match>().Select(x => x.Groups[1].Value).ToList<string>();

            foreach (var strdownInfo in sdownLoadInfo)
            {
                string[] stemp = strdownInfo.Split(new string[] { "<ol>" }, StringSplitOptions.RemoveEmptyEntries);
                DownLoadInfo downLoadInfo = new DownLoadInfo();
                downLoadInfo.Name = GetTitleContent(stemp[0], "a");
                downLoadInfo.Note = GetTitleContent(stemp[0], "p");
                var maths = Regex.Matches(stemp[1], "(?is)<li>(.+?)</li>").OfType<Match>().Select(x => x.Groups[1].Value).ToList<string>();
                downLoadInfo.StartDate = maths[0];
                downLoadInfo.Level = maths[1];
                downLoadInfo.Product = maths[2];

                downLoadInfo.CourseName = maths[3];
                downLoadInfo.Tag = maths[4];
                downLoadInfo.Teach = maths[5];
                downLoadInfo.DownLoadItem = new List<Link>();

                string Tags = stemp[1].Split(new string[] { "<li class=\"downloads\">" },StringSplitOptions.RemoveEmptyEntries)[1];

                string[] links = Tags.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);

                foreach (var strlink in links)
                {
                    if (strlink.IndexOf("<a") == -1) continue;
                    Link link = new Link();
                    link.LinkName = GetTitleContent(strlink, "a");
                    link.LinkUrl = GetTitleContent(strlink, "a", "href");
                    downLoadInfo.DownLoadItem.Add(link);

                }
                result.Add(downLoadInfo);
            }

            return result;
        }

        /// <summary>  
        /// 获取字符中指定标签的值  
        /// </summary>  
        /// <param name="str">字符串</param>  
        /// <param name="title">标签</param>  
        /// <returns>值</returns>  
        public static string GetTitleContent(string str, string title)
        {
            string tmpStr = string.Format("<{0}[^>]*?>(?<Text>[^<]*)</{1}>", title, title); //获取<title>之间内容  

            Match TitleMatch = Regex.Match(str, tmpStr, RegexOptions.IgnoreCase);

            string result = TitleMatch.Groups["Text"].Value;
            return result;
        }
        /// <summary>  
        /// 获取字符中指定标签的值  
        /// </summary>  
        /// <param name="str">字符串</param>  
        /// <param name="title">标签</param>  
        /// <param name="attrib">属性名</param>  
        /// <returns>属性</returns>  
        public static string GetTitleContent(string str, string title, string attrib)
        {

            string tmpStr = string.Format("<{0}[^>]*?{1}=(['\"\"]?)(?<url>[^'\"\"\\s>]+)\\1[^>]*>", title, attrib); //获取<title>之间内容  

            Match TitleMatch = Regex.Match(str, tmpStr, RegexOptions.IgnoreCase);

            string result = TitleMatch.Groups["url"].Value;
            return result;
        }

        public static void DownLoad(DownLoadInfo info)
        {
            foreach (var link in info.DownLoadItem)
            {
                CsharpHttpHelper.HttpHelper helper = new HttpHelper();
                HttpItem item = new HttpItem();
                item.URL = link.LinkUrl.Replace("&amp;","&");
                HttpResult httpResult = helper.GetHtml(item);
                string strdownloadUrl = httpResult.RedirectUrl.Replace("www.microsoft.com", "download.microsoft.com");
                //DownLoadFile(strdownloadUrl, "D:\\");
                link.LinkUrl = strdownloadUrl;

            }

        }

        private static void DownLoadFile(string urladdress,string filePath)
        {
            WebClient client = new WebClient();
            client.DownloadFile(urladdress, filePath + System.IO.Path.GetFileName(urladdress));
        }

    }

    public class Course
    {
        public string Name { get; set; }

        public string Type { get; set; }

        public string Num { get; set; }

        public string LastDate { get; set; }

        public string LastNote { get; set; }

        public string Url { get; set; }
    }

    public class DownLoadInfo
    {
        public string Name { get; set; }
        public string Note { get; set; }
        public string StartDate { get; set; }

        public string Level { get; set; }

        public string Product { get; set; }

        public string CourseName { get; set; }

        public string Teach { get; set; }

        public string Tag { get; set; }

        public List<Link> DownLoadItem { get; set; }
    }

    public class Link
    {
        public string LinkName { get; set; }
        public string LinkUrl { get; set; }
    }

}
