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
    public partial class HomeForm : Form
    {
        public HomeForm()
        {
            InitializeComponent();
        }

        /// <summary>
        /// 开始归类按钮
        /// </summary>
        private void sortButton_Click(object sender, EventArgs e)
        {
            sortBackgroundWorker.RunWorkerAsync(inTextBox.Text);
        }

        /// <summary>
        /// 导步归类开始
        /// </summary>
        private void sortBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            DirectoryInfo directorys = new DirectoryInfo(e.Argument as string);// 遍历文件夹
            FileInfo[] files = directorys.GetFiles("*.mp4", SearchOption.AllDirectories);
            for (int i = 0; i < files.Length; i++)
            {
                if (sortBackgroundWorker.CancellationPending)// 取消检测
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
                    string time = DateFunction.GetMediaData(files[i].FullName);// 返回空白，即此方法没有获取到有效信息

                    //gffdsfaffd   MP4获取失败，尝试把所有属性列出来看看

                    if (time == "") MessageBox.Show("获取失败");

                        if (time == "") time = string.Format("{0:yyyyMMdd}", files[i].LastWriteTime);// 获取文件最后写入时间
                    if (time == null)// 返回了null值，表示时间格式化时失败了
                    {
                        e.Cancel = true;
                        return;
                    }
                    DirectoryInfo dir = new DirectoryInfo(files[i].DirectoryName);
                    files[i].MoveTo(Path.Combine(files[i].DirectoryName, time + "_" + new DirectoryInfo(files[i].DirectoryName).Name + "_" + files[i].Name).Replace("mmexport", "").Replace("wx_camera_", ""));
                    ///
                    sortBackgroundWorker.ReportProgress(Percents.Get(i, files.Length), files[i].FullName);// 进度传出
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

        /// <summary>
        /// 导步归类进度
        /// </summary>
        private void sortBackgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar.Value = (e.ProgressPercentage < 101) ? e.ProgressPercentage : progressBar.Value;
            progressLabel.Text = progressBar.Value.ToString() + "% " + e.UserState as string;
        }

        /// <summary>
        /// 导步归类结束
        /// </summary>
        private void sortBackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                progressLabel.Text = "后台错误";
                progressBar.Value = 0;
                return;
            }

            if (e.Cancelled)
            {
                progressLabel.Text = "已取消";
                return;
            }

            progressBar.Value = 100;
            progressLabel.Text = "完成";
        }

        /// <summary>
        /// 归类文件夹浏览
        /// </summary>
        private void inButton_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK) inTextBox.Text = folderBrowserDialog1.SelectedPath;
        }

        /// <summary>
        /// 目标文件夹浏览
        /// </summary>
        private void outButton_Click(object sender, EventArgs e)
        {
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK) outTextBox.Text = folderBrowserDialog1.SelectedPath;
        }

        /// <summary>
        /// 取消归类按钮
        /// </summary>
        private void button1_Click(object sender, EventArgs e)
        {
            sortBackgroundWorker.CancelAsync();
        }
        

        ///

    }
}
