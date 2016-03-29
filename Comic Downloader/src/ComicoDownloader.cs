using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.IO;

namespace Comic_Downloader
{
    class ComicoDownloader : AbstractComicDownloader
    {
        private string site;
        private ushort from, to;

        public ComicoDownloader(string savePath, BackgroundWorker worker)
            : base(savePath, worker)
        {
        }

        override public void Download(string url)
        {
            // Parse URL
            Match match = Regex.Match(url, "(<[0-9]+,[0-9]+>)(http.+)");
            if (!match.Success)
                throw new Exception("網址格式: <from回數,to回數>網址");
            site = match.Groups[2].Value;
            string condition = match.Groups[1].Value;
            from = ushort.Parse(condition.Substring(1, condition.IndexOf(",")-1));
            to = ushort.Parse(condition.Substring(condition.IndexOf(",")+1, condition.Length-condition.IndexOf(",")-2));
            
            // TODO 404 not found handling
            // Start downloading
            int numEpisodes = to - from + 1;
            for (int i=1; from<=to; i++, from++) {
                string saveDir = string.Format(@"{0}\{1}", savePath, from);
                Directory.CreateDirectory(saveDir);

                IList<string> fileUrls = getFileUrls(ReadHtml(string.Format("{0}&articleNo={1}", site, from)));
                for (int j=0; j<fileUrls.Count; j++) {
                    worker.ReportProgress(
                            (int)((double)((j+1)/fileUrls.Count)*(i/numEpisodes*100.0)), 
                            string.Format("下載第{0}話中... ({0}回/{1}回 : {2}頁/{3}頁)", i, numEpisodes, j+1, fileUrls.Count));
                    webClient.DownloadFile(fileUrls[j], string.Format(@"{0}\{1}-{2:00}.jpg", saveDir, i, j));
                }
                worker.ReportProgress(0, string.Format("已下載第{0}話至: {1}\n", i, saveDir));
            }
        }

        private IList<string> getFileUrls(string html)
        {
            IList<string> list = new List<string>();
            html = html.Substring(html.IndexOf("<section class=\"m-section-detail__body\" id=\"_comicTop\">"));
            html = html.Substring(0, html.IndexOf("</section>"));
            string[] lines = html.Replace("\r", "").Split('\n');
            foreach (string line in lines) {
                if (!line.Contains("img src"))
                    continue;
                string url = line.Substring(line.IndexOf("\"")+1);
                list.Add(url.Substring(0, url.IndexOf("\"")));
            }
            return list;
        }

    }
}
