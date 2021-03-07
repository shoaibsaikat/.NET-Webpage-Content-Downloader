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
        public Form1()
        {
            InitializeComponent();

            labelAuthor.MaximumSize = new Size(300, 300);
            labelAuthor.Text = "Developed by: Mina Shoaib Rahman\nMail: shoaibsaikat@gmail.com";
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
            // http://mis.molwa.gov.bd/freedom-fighter-list?division_id=3&district_id=21&thana_id=192&page=189
            var url = textBoxUrl.Text;
            var tag = textBoxTag.Text;

            var lastIndex = lastPageIndex(url);
            var baseUrl = url.Remove(lastIndex + 1);

            if (tag != null)
            {
                using (WebClient client = new WebClient()) // WebClient class inherits IDisposable
                {
                    var page = 1;
                    while (page <= numericUpDownCount.Value)
                    {
                        Stream data = client.OpenRead(baseUrl + page);
                        StreamReader reader = new StreamReader(data);
                        string html = reader.ReadToEnd();
                        var htmlDoc = new HtmlDocument();
                        htmlDoc.LoadHtml(html);

                        var htmlData = htmlDoc.DocumentNode.SelectNodes("//" + tag).ToList();
                        for (int i = 1; i < htmlData.Count; i++) // skip first row, as it's a header
                        {
                            richTextBoxOutput.Text += htmlData[i].OuterHtml;
                        }
                        page++;
                    }
                }
            }
        }
    }
}
