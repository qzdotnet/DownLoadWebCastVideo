using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DownLoadWebCastVideo
{
    public partial class FrmMain : Form
    {
        public FrmMain()
        {
            InitializeComponent();
        }

        private List<DownLoadInfo> downLoadEntityList;
        private void FrmMain_Load(object sender, EventArgs e)
        {
            this.dataGridView1.DataSource = DownLoadHelp.GetCourseList();
        }

        private void btnDownLoad_Click(object sender, EventArgs e)
        {
            Course course = this.dataGridView1.SelectedRows[0].DataBoundItem as Course;
            downLoadEntityList = DownLoadHelp.GetDownLoadInfo(course);

            this.txtUrl.Text = "";
            lblToolip.Text = "共" + downLoadEntityList.Count + "课";
            backgroundWorker1.WorkerReportsProgress = true;
            this.progressBar1.Maximum = downLoadEntityList.Count-1;
            backgroundWorker1.RunWorkerAsync(downLoadEntityList);
            this.btnDownLoad.Enabled = false;
            

        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            List<DownLoadInfo> downloadList = e.Argument as List<DownLoadInfo>;

            int i = 0;
            foreach (var downloadInfo in downloadList)
            {
                DownLoadHelp.DownLoad(downloadInfo);
                backgroundWorker1.ReportProgress(i++);
            }

        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.btnDownLoad.Enabled = true;
            foreach (DownLoadInfo info in downLoadEntityList)
            {
                foreach (Link link in info.DownLoadItem)
                {
                    if (link.LinkUrl.EndsWith("mp3") || link.LinkUrl.EndsWith("mp4") || link.LinkUrl.EndsWith("wmv")) continue;
                    this.txtUrl.AppendText(link.LinkUrl + System.Environment.NewLine);
                }
            }
            Clipboard.SetText(this.txtUrl.Text);

        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar1.Value = e.ProgressPercentage;
            lblToolip.Text = "已下载" + e.ProgressPercentage;
        }
    }
}
