using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;

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
            string range = match.Groups[1].Value;
            from = ushort.Parse(range.Substring(1, range.IndexOf(",")-1));
            to = ushort.Parse(range.Substring(range.IndexOf(",")+1, range.Length-range.IndexOf(",")-2));
            
            // TODO 404 not found handling
            // Start downloading
            int numEpisodes = to - from + 1;
            double progress = 0.0;
            while (from <= to) {
                string episodeDir = string.Format(@"{0}\{1:000}", savePath, from);
                Directory.CreateDirectory(episodeDir);

                IList<string> imagePaths = new List<string>();
                IList<string> fileUrls = getFileUrls(ReadHtml(string.Format("{0}&articleNo={1}", site, from)));

                for (int i=1; i<=fileUrls.Count; i++) {
                    progress += 1.0 / fileUrls.Count / numEpisodes * 100.0;
                    worker.ReportProgress((int)progress, string.Format("下載第{0}話中... ({0}話/{1}話 : {2}頁/{3}頁)", from, to, i, fileUrls.Count));
                    
                    string imagePath = string.Format(@"{0}\{1:000}-{2:00}.jpg", episodeDir, from, i);
                    webClient.DownloadFile(fileUrls[i-1], imagePath);
                    imagePaths.Add(imagePath);
                }
                Combine(imagePaths, episodeDir+".jpg");
                worker.ReportProgress((int)progress, string.Format("已下載第{0}話至: {1}\n", from, episodeDir));
                from++;
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

        // Combine images
        private void Combine(IList<string> imagePaths, string savePath)
        {
            // Load images
            int width = int.MinValue;
            int height = 0;
            IList<Bitmap> bitmaps = new List<Bitmap>();
            foreach (string path in imagePaths) {
                Bitmap bitmap = new Bitmap(path);
                width = Math.Max(width, bitmap.Width);
                height += bitmap.Height;
                bitmaps.Add(bitmap);
            }

            // Combine images (Fixed x-coordinate)
            Bitmap output = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(output)) {
                int currentHeight = 0;
                foreach (Bitmap bitmap in bitmaps) {
                    g.DrawImage(bitmap, 0, currentHeight);
                    currentHeight += bitmap.Height;
                }
            }

            // Save image
            EncoderParameters parameters = new EncoderParameters();
            parameters.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 80L);
            output.Save(savePath, GetEncoder(ImageFormat.Jpeg), parameters);
        }

        private ImageCodecInfo GetEncoder(ImageFormat format)
        {
            ImageCodecInfo[] codecs = ImageCodecInfo.GetImageDecoders();
            foreach (ImageCodecInfo codec in codecs) {
                if (codec.FormatID == format.Guid) {
                    return codec;
                }
            }
            return null;
        }
    }
}
