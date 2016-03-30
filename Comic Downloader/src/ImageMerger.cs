using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace Comic_Downloader
{
    class ImageMerger
    {
        private FileInfo saveFile;
        private int count = 1;
        private EncoderParameters parameters;
        private ImageCodecInfo encoder;

        public ImageMerger(string saveFilePath)
        {
            saveFile = new FileInfo(saveFilePath);
            if (!saveFile.Directory.Exists)
                throw new Exception("The directory path does not exist: " + saveFile);

            parameters = new EncoderParameters();
            parameters.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, 80L);

            encoder = Array.Find<ImageCodecInfo>(
                    ImageCodecInfo.GetImageDecoders(), 
                    codec => (codec.FormatID == ImageFormat.Jpeg.Guid));
        }

        public void Merge(Queue<Image> queue)
        {
            // Set the width and height of the merged image
            Queue<Image> newQueue = new Queue<Image>();
            int width = 0;
            int height = 0;
            while (queue.Count > 0) {
                Image nextImage = queue.Peek();
                // GDI+ limitation (pixels <= 65535*65535)
                if (height+nextImage.Height > ushort.MaxValue)
                    break;
                width = Math.Max(width, nextImage.Width);
                height += nextImage.Height;
                newQueue.Enqueue(queue.Dequeue());
            }

            // Combine images (Fixed x-coordinate)
            Image output = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(output)) {
                int currentHeight = 0;
                foreach (Image image in newQueue) {
                    g.DrawImage(image, 0, currentHeight);
                    currentHeight += image.Height;
                    image.Dispose();
                }
            }

            // Save file
            string fileName = saveFile.FullName;
            if ((queue.Count > 0) || (count > 1))
                fileName = fileName.Replace(".jpg", string.Format("-{0}.jpg", count));
            output.Save(fileName, encoder, parameters);

            // Merge the rest images
            if (queue.Count > 0) { 
                count++;
                Merge(queue);
            }
        }

    }
}
