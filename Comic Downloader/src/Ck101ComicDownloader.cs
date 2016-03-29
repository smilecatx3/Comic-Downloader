using System.ComponentModel;
using System.Text.RegularExpressions;
using System.IO;

namespace Comic_Downloader
{
    class Ck101Downloader : AbstractComicDownloader
    {
        public Ck101Downloader(string savePath, BackgroundWorker worker)
            : base(savePath, worker)
        {
        }

        override public void Download(string url)
        {
            uint numFiles = GetFileAmount(ReadHtml(url));
            string urlRoot = url.Substring(0, url.LastIndexOf("/"));
            string dirName = (savePath.EndsWith(@"\") ? savePath : savePath+@"\") + urlRoot.Substring(urlRoot.LastIndexOf("/")+1);
            Directory.CreateDirectory(dirName);

            for (int i=1; i<=numFiles; i++) {
                worker.ReportProgress((int)((double)i/numFiles*100.0), string.Format("下載第{0}張圖片中... ({0}/{1})", i, numFiles));
                string fileUrl = GetFileUrl(ReadHtml(string.Format("{0}/{1}", urlRoot, i)));
                string fileName = string.Format(@"{0}\{1}.jpg", dirName, i);
                webClient.DownloadFile(fileUrl, fileName);
                worker.ReportProgress(0, string.Format("已儲存圖片至: {0}\n", fileName));
            }
        }

        private string GetFileUrl(string html)
        {
            int begin = html.IndexOf("<img id = 'defualtPagePic'");
            int end = html.IndexOf("alt=\"Bad Image\"");
            string part = html.Substring(begin, end-begin);
            Match match = Regex.Match(part, "<img id = 'defualtPagePic' src=\"(.+)\" alt=.*");
            return match.Groups[1].Value;
        }

        private uint GetFileAmount(string html)
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
            return uint.Parse(match.Groups[1].Value);
        }

    }
}
