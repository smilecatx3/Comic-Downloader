using System.IO;
using System.Net;
using System.ComponentModel;

namespace Comic_Downloader
{
    abstract class AbstractComicDownloader
    {
        protected string savePath;
        protected BackgroundWorker worker;
        protected WebClient webClient = new WebClient();

        protected AbstractComicDownloader(string savePath, BackgroundWorker worker)
        {
            this.savePath = savePath;
            this.worker = worker;
        }
        
        protected string ReadHtml(string url)
        {
            using (var reader = new StreamReader(WebRequest.Create(url).GetResponse().GetResponseStream())) {
                return reader.ReadToEnd();
            }
        }

        public abstract void Download(string url);
    }
}
