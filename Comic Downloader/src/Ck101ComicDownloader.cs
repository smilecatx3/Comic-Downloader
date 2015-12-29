using System;
using System.ComponentModel;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;

namespace Comic_Downloader
{
    class Ck101ComicDownloader
    {
        private BackgroundWorker worker;
        private WebClient webClient = new WebClient();

        public Ck101ComicDownloader(BackgroundWorker worker)
        {
            this.worker = worker;
        }

        public void download(string saveDirPath, string url)
        {
            if (!url.StartsWith("http://comic.ck101.com"))
                throw new Exception("目前只支援卡提諾漫畫下載");

            int pageNumbers = getPageNum(ReadHtml(url));
            string urlRoot = url.Substring(0, url.LastIndexOf("/"));
            string dirName = (saveDirPath.EndsWith(@"\") ? saveDirPath : saveDirPath+@"\") + urlRoot.Substring(urlRoot.LastIndexOf("/")+1);
            Directory.CreateDirectory(dirName);

            for (int i=1; i<=pageNumbers; i++) {
                worker.ReportProgress((int)((double)i/pageNumbers*100.0), string.Format("下載第{0}張圖片中... ({0}/{1})", i, pageNumbers));
                string picUrl = getPicUrl(ReadHtml(string.Format("{0}/{1}", urlRoot, i)));
                string savePath = string.Format(@"{0}\{1}.jpg", dirName, i);
                webClient.DownloadFile(picUrl, savePath);
                worker.ReportProgress(0, string.Format("已儲存圖片至: {0}\n", savePath));
            }
        }

        private string ReadHtml(string url)
        {
            StreamReader reader = new StreamReader(WebRequest.Create(url).GetResponse().GetResponseStream());
            string data = reader.ReadToEnd();
            reader.Close();
            return data;
        }

        private int getPageNum(string html)
        {
            // 擷取選擇頁碼區塊
            int begin = html.IndexOf("<!--btnWrap-->");
            int end = html.IndexOf("<!--btnWrapEnd-->");
            string part = html.Substring(begin, end-begin);
            // 擷取最後頁碼
            begin = part.LastIndexOf("<option");
            end = part.LastIndexOf("</option>");
            part = part.Substring(begin, end-begin);
            Match match = Regex.Match(part, "<option value=\"(\\d*).*");
            return int.Parse(match.Groups[1].Value);
        }

        private string getPicUrl(string html)
        {
            int begin = html.IndexOf("<img id = 'defualtPagePic'");
            int end = html.IndexOf("alt=\"Bad Image\"");
            string part = html.Substring(begin, end-begin);
            Match match = Regex.Match(part, "<img id = 'defualtPagePic' src=\"(.+)\" alt=.*");
            return match.Groups[1].Value;
        }
    }
}
