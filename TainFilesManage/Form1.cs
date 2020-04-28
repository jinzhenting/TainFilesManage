using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace TainFilesManage
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void sortButton_Click(object sender, EventArgs e)
        {
            backgroundWorker1.RunWorkerAsync(inTextBox.Text);
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
                DirectoryInfo directorys = new DirectoryInfo(e.Argument as string);// 遍历文件夹
                FileInfo[] files = directorys.GetFiles("*.arw", SearchOption.AllDirectories);
                for (int i = 0; i < files.Length; i++)
                {
                if (backgroundWorker1.CancellationPending)// 取消检测
                {
                    e.Cancel = true;
                    return;
                }
                ///
                Regex regNum = new Regex(@"^([0-9]{8})[_].");
                if (regNum.IsMatch(files[i].Name)) continue;
                ///

                try// 访问权限捕捉
                {
                    string time = DateTimeFunction.GetTakeMediaDataTime(files[i].FullName);
                    if (time == "") time = string.Format("{0:yyyyMMdd}", files[i].LastWriteTime);
                    DirectoryInfo dir = new DirectoryInfo(files[i].DirectoryName);
                    files[i].MoveTo(Path.Combine(files[i].DirectoryName, time + "_" + new DirectoryInfo(files[i].DirectoryName).Name + "_" + files[i].Name).Replace("mmexport", "").Replace("wx_camera_", ""));
                    ///
                    backgroundWorker1.ReportProgress(Percents.Get(i, files.Length), files[i].FullName);// 进度传出
                }
                #region 异常
                catch (UnauthorizedAccessException ex)
                {
                    if (MessageBox.Show("无权限操作，请尝试使用管理员权限运行本程序\r\n\r\n当前文件：" + files[i].FullName + "\r\n\r\n错误描述如下\r\n\r\n" + ex + "\r\n\r\n是否继续？", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK) { continue; }
                    else
                    {
                        e.Cancel = true;
                        return;
                    }
                }
                catch (FileNotFoundException ex)
                {
                    if (MessageBox.Show("文件或文件夹不存在\r\n\r\n当前文件：" + files[i].FullName + "\r\n\r\n错误描述如下\r\n\r\n" + ex + "\r\n\r\n是否继续归类？", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK) { continue; }
                    else
                    {
                        e.Cancel = true;
                        return;
                    }
                }
                catch (Exception ex)
                {
                    if (MessageBox.Show("发生未知错误\r\n\r\n当前文件：" + files[i].FullName + "\r\n\r\n错误描述如下" + ex + "\r\n\r\n是否继续归类？", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK) { continue; }
                    else
                    {
                        e.Cancel = true;
                        return;
                    }
                }
                #endregion 异常 
            }
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            toolStripProgressBar1.Value = (e.ProgressPercentage < 101) ? e.ProgressPercentage : toolStripProgressBar1.Value;
            toolStripStatusLabel1.Text = toolStripProgressBar1.Value.ToString() + "% " + e.UserState as string;
        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                toolStripStatusLabel1.Text = "后台错误";
                toolStripProgressBar1.Value = 0;
                return;
            }

            if (e.Cancelled)
            {
                toolStripStatusLabel1.Text = "已取消";
                return;
            }

            toolStripProgressBar1.Value = 100;
            toolStripStatusLabel1.Text = "完成";
        }

        private void inButton_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK) inTextBox.Text = folderBrowserDialog1.SelectedPath;
        }

        private void outButton_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK) outTextBox.Text = folderBrowserDialog1.SelectedPath;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            backgroundWorker1.CancelAsync();
        }
    }
}
