using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace Comic_Downloader
{
    public partial class Form1 : Form
    {
        private Exception runException = null;

        public Form1()
        {
            InitializeComponent();
            FormBorderStyle = FormBorderStyle.FixedSingle;
            ActiveControl = label1; // 隱藏游標
        }

        private void textBox_saveDirPath_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
                textBox_saveDirPath.Text = folderBrowserDialog1.SelectedPath;
            ActiveControl = label1; // 隱藏游標
        }

        private void button_start_Click(object sender, EventArgs e)
        {
            if (textBox_saveDirPath.TextLength * textbox_url.TextLength == 0) {
                MessageBox.Show("請先選擇儲存路徑、以及指定下載網址", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            button_start.Enabled = false;
            statusLabel1.Text = "初始化中...";
            progressBar1.Value = 0;
            runException = null;
            backgroundWorker1.RunWorkerAsync();
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            try {
                string url = textbox_url.Text;
                string savePath = textBox_saveDirPath.Text;
                AbstractComicDownloader downloader;
                if (url.Contains("comic.ck101"))
                    downloader = new Ck101Downloader(savePath, sender as BackgroundWorker);
                else if (url.Contains("comico"))
                    downloader = new ComicoDownloader(savePath, sender as BackgroundWorker);
                else
                    throw new Exception("此下載器不支援此網站");
                downloader.Download(url);
            } catch (Exception ex) {
                runException = ex;
                Console.WriteLine(ex.StackTrace);
            }
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            string state = e.UserState.ToString();
            if (state.StartsWith("下")) {
                statusLabel1.Text = state;
                progressBar1.Value = e.ProgressPercentage;
            } else if (state.StartsWith("已")) {
                textBox_message.AppendText(state);
            }
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (runException == null) {
                statusLabel1.Text = "就緒";
                textBox_message.AppendText("已全數下載完成\n\n");
            } else {
                MessageBox.Show("下載時出現錯誤", "錯誤", MessageBoxButtons.OK, MessageBoxIcon.Error);
                textBox_message.AppendText("下載時出現錯誤!! 錯誤訊息: ");
                textBox_message.AppendText(runException.Message + "\n\n");
                progressBar1.Value = 0;
            }
            button_start.Enabled = true;
        }
    }
}
