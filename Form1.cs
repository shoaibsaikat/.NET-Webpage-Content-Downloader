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
using HtmlDocument = HtmlAgilityPack.HtmlDocument;
using System.Resources;
using System.Reflection;

namespace Web_Page_Content_Downloader
{
    public partial class Form1 : Form
    {
        private BackgroundWorker backgroundWorkerProgress;
        private BackgroundWorker backgroundWorkerCancel;
        private ResourceManager rm;

        private string _data;
        private string Data
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

        private string _baseUrl;
        private string BaseUrl
        {
            get
            {
                return _baseUrl;
            }
            set
            {
                _baseUrl = value;
            }
        }
        private string _endUrl;
        private string EndUrl
        {
            get
            {
                return _endUrl;
            }
            set
            {
                _endUrl = value;
            }
        }
        public Form1()
        {
            InitializeComponent();

            Data = string.Empty;
            rm = new ResourceManager("Web_Page_Content_Downloader.Form1", Assembly.GetExecutingAssembly());

            InitializeBackgroundWorker();
        }

        // Set up the BackgroundWorker object by 
        // attaching event handlers. 
        private void InitializeBackgroundWorker()
        {
            backgroundWorkerProgress = new BackgroundWorker();
            backgroundWorkerCancel = new BackgroundWorker();

            backgroundWorkerProgress.WorkerSupportsCancellation = true;
            backgroundWorkerProgress.WorkerReportsProgress = true;

            backgroundWorkerProgress.DoWork += new DoWorkEventHandler(backgroundWorkerProgress_DoWork);
            backgroundWorkerProgress.RunWorkerCompleted += new RunWorkerCompletedEventHandler(backgroundWorkerProgress_RunWorkerCompleted);
            backgroundWorkerProgress.ProgressChanged += new ProgressChangedEventHandler(backgroundWorkerProgress_ProgressChanged);

            backgroundWorkerCancel.DoWork += new DoWorkEventHandler(backgroundWorkerCancel_DoWork);
        }

        private void buttonDone_Click(object sender, EventArgs e)
        {
            if (!backgroundWorkerProgress.IsBusy)
            {
                buttonDone.Enabled = false;
                buttonCancel.Enabled = true;
                Data = string.Empty;
                richTextBoxOutput.Text = Data;
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

            if (tag != null && url != null && numericUpDownPosition.Value != 0)
            {
                var lastIndex = url.Length - numericUpDownPosition.Value;
                int i = (int)lastIndex;

                while (i < url.Length && url[i] >= '0' && url[i] <= '9')
                    i++;

                EndUrl = url.Substring(i);
                BaseUrl = url.Remove((int)lastIndex);

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
                        Stream data = client.OpenRead(BaseUrl + page + EndUrl);
                        StreamReader reader = new StreamReader(data);
                        string html = reader.ReadToEnd();
                        var htmlDoc = new HtmlDocument();
                        htmlDoc.LoadHtml(html);

                        var htmlData = htmlDoc.DocumentNode.SelectNodes("//" + tag).ToList();
                        for (i = 1; i < htmlData.Count; i++) // skip first row, as it's a header
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
            richTextBoxOutput.Text = Data;
            //richTextBoxOutput.Text = BaseUrl + "\n" + EndUrl;
        }

        private void backgroundWorkerCancel_DoWork(object sender, DoWorkEventArgs e)
        {
            if (backgroundWorkerProgress.WorkerSupportsCancellation)
            {
                backgroundWorkerProgress.CancelAsync();
            }
        }

        private void buttonCopy_Click(object sender, EventArgs e)
        {
            if (richTextBoxOutput != null && !string.IsNullOrWhiteSpace(richTextBoxOutput.Text))
            {
                Clipboard.SetText(richTextBoxOutput.Text);
                MessageBox.Show(rm.GetString("labelCopied"));
            }
            else
            {
                MessageBox.Show(rm.GetString("labelNotCopied"));
            }
        }

        private void buttonAbout_Click(object sender, EventArgs e)
        {
            string msg = rm.GetString("labelDevelopedBy") + "\t" + rm.GetString("author") + "\n" +
                         rm.GetString("labelMail") + "\t\t" + rm.GetString("mail");
            MessageBox.Show(msg);
        }
    }
}
