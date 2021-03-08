using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.IO;
using HtmlAgilityPack;
using HtmlDocument = HtmlAgilityPack.HtmlDocument;

namespace Web_Page_Content_Downloader
{
    public partial class Form1 : Form
    {
        private String _data;
        private String Data
        {
            get
            {
                return _data;
            }
            set
            {
                _data = value;
            }
        }
        public Form1()
        {
            InitializeComponent();

            labelAuthor.MaximumSize = new Size(300, 300);
            labelAuthor.Text = "Developed by: Mina Shoaib Rahman\nMail: shoaibsaikat@gmail.com";
            Data = string.Empty;

            InitializeBackgroundWorker();
        }

        // Set up the BackgroundWorker object by 
        // attaching event handlers. 
        private void InitializeBackgroundWorker()
        {
            backgroundWorkerProgress.DoWork += new DoWorkEventHandler(backgroundWorkerProgress_DoWork);
            backgroundWorkerProgress.RunWorkerCompleted += new RunWorkerCompletedEventHandler(backgroundWorkerProgress_RunWorkerCompleted);
            backgroundWorkerProgress.ProgressChanged += new ProgressChangedEventHandler(backgroundWorkerProgress_ProgressChanged);
        }

        private int lastPageIndex(String url)
        {
            int i = url.Length - 1;
            for (; i >= 0; i--)
            {
                if (url[i] < '0' || url[i] > '9')
                    break;
            }
            return i;
        }

        private void buttonDone_Click(object sender, EventArgs e)
        {
            if (!backgroundWorkerProgress.IsBusy)
            {
                buttonDone.Enabled = false;
                buttonCancel.Enabled = true;
                Data = string.Empty;
                backgroundWorkerProgress.RunWorkerAsync();
            }
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            if (!backgroundWorkerCancel.IsBusy)
            {
                backgroundWorkerCancel.RunWorkerAsync();
                buttonCancel.Enabled = false;
            }
        }

        private void backgroundWorkerProgress_DoWork(object sender, DoWorkEventArgs e)
        {
            // http://mis.molwa.gov.bd/freedom-fighter-list?division_id=3&district_id=21&thana_id=192&page=189
            BackgroundWorker worker = sender as BackgroundWorker;
            var url = textBoxUrl.Text;
            var tag = textBoxTag.Text;

            var lastIndex = lastPageIndex(url);
            var baseUrl = url.Remove(lastIndex + 1);

            if (tag != null)
            {
                using (WebClient client = new WebClient()) // WebClient class inherits IDisposable
                {
                    var page = 1;
                    var ratio = 100 / numericUpDownCount.Value;
                    while (page <= numericUpDownCount.Value)
                    {
                        if (worker.CancellationPending)
                        {
                            e.Cancel = true;
                            break;
                        }
                        Stream data = client.OpenRead(baseUrl + page);
                        StreamReader reader = new StreamReader(data);
                        string html = reader.ReadToEnd();
                        var htmlDoc = new HtmlDocument();
                        htmlDoc.LoadHtml(html);

                        var htmlData = htmlDoc.DocumentNode.SelectNodes("//" + tag).ToList();
                        for (int i = 1; i < htmlData.Count; i++) // skip first row, as it's a header
                        {
                            Data += htmlData[i].OuterHtml;
                        }
                        worker.ReportProgress((int)(page * ratio));
                        page++;
                    }
                }
            }
        }

        // This event handler updates the progress.
        private void backgroundWorkerProgress_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar1.Value = e.ProgressPercentage;
            richTextBoxOutput.Text = Data;
        }

        // This event handler deals with the results of the background operation.
        private void backgroundWorkerProgress_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                progressBar1.Value = 100;
            }
            else if (e.Error == null)
            {
                progressBar1.Value = 100;
            }
            buttonDone.Enabled = true;
            buttonCancel.Enabled = false;
        }

        private void backgroundWorkerCancel_DoWork(object sender, DoWorkEventArgs e)
        {
            if (backgroundWorkerProgress.WorkerSupportsCancellation)
            {
                backgroundWorkerProgress.CancelAsync();
            }
        }
    }
}
