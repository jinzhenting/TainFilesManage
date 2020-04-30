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
            if (sortBackgroundWorker.IsBusy) return;
            sortBackgroundWorker.RunWorkerAsync(inTextBox.Text);
        }

        /// <summary>
        /// 导步归类开始
        /// </summary>
        private void sortBackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            sortBackgroundWorker.ReportProgress(1, "文件扫描中...");// 进度传出
            DirectoryInfo directorys = new DirectoryInfo(e.Argument as string);// 遍历文件夹
            FileInfo[] files = directorys.GetFiles("*.*", SearchOption.AllDirectories);
            for (int i = 0; i < files.Length; i++)
            {
                if (sortBackgroundWorker.CancellationPending)// 取消检测
                {
                    e.Cancel = true;
                    return;
                }

                ///

                sortBackgroundWorker.ReportProgress(Percents.Get(i, files.Length), files[i].FullName);// 进度传出

                ///

                DirectoryInfo dir = new DirectoryInfo(files[i].DirectoryName);// 父级文件夹名称
                string fileName = Path.GetFileNameWithoutExtension(files[i].Name).Replace("mmexport", "").Replace("wx_camera_", "");// 不包含扩展名的文件名
                string extension = Path.GetExtension(files[i].Name).ToLower();// 扩展名
                string folddrName = dir.Name;
                string time;

                ///

                Regex regNum = new Regex(@"^(([0-9]{8})[_]).*");
                if (regNum.IsMatch(files[i].Name))// 已重命名
                {
                    if (sort1RadioButton.Checked)// 归类
                    {
                        string endFolder = Path.Combine(outTextBox.Text, files[i].Name.Substring(0, 6));// 年月文件夹
                        if (!Directory.Exists(endFolder)) Directory.CreateDirectory(endFolder);
                        files[i].MoveTo(Path.Combine(endFolder, files[i].Name));
                        continue;
                    }
                    else if (sort2RadioButton.Checked)
                    {
                        string endFolder = Path.Combine(files[i].DirectoryName, files[i].Name.Substring(0, 6));// 年月文件夹
                        DirectoryInfo dirF = Directory.GetParent(dir.FullName);

                        if (dir.Name == dirF.Name)// 如果多重同名目录，放到上一层
                        {
                            files[i].MoveTo(Path.Combine(dirF.FullName, files[i].Name));
                            continue;
                        }

                        if (dir.Name == files[i].Name.Substring(0, 6)) continue;// 如果取到的文件夹名与自身文件夹名相同则跳过

                        if (!Directory.Exists(endFolder)) Directory.CreateDirectory(endFolder);
                        files[i].MoveTo(Path.Combine(endFolder, files[i].Name));
                        continue;
                    }
                    else continue;
                }

                ///

                try// 访问权限捕捉
                {
                    switch (extension)
                    {
                        case ".jpg":
                        case ".jpeg":
                        case ".arw":
                            {
                                time = DateFunction.GetPhotoData(files[i].FullName);// 获取照片拍摄时间 // 返回空白，即此方法没有获取到有效信息
                                break;
                            }
                        case ".mp4":
                        case ".mov":
                        case ".avi":
                            {
                                time = DateFunction.GetMediaData(files[i].FullName);// 获取媒体拍摄时间 // 返回空白，即此方法没有获取到有效信息
                                break;
                            }
                        default:
                            {
                                time = "";
                                if (MessageBox.Show("当前文件：" + files[i].FullName + "不支持获取拍摄时间\r\n\r\n是否继续归类其他文件？", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK) break;
                                else return;
                            }
                    }
                    if (time == "") time = string.Format("{0:yyyyMMdd}", files[i].LastWriteTime);// 获取文件最后写入时间
                    if (time == null)// 返回了null值，表示时间格式化时失败了
                    {
                        e.Cancel = true;
                        return;
                    }

                    if (sort1RadioButton.Checked)// 归类
                    {
                        string endFolder = Path.Combine(outTextBox.Text, time.Substring(0, 6));// 年月文件夹
                        if (!Directory.Exists(endFolder)) Directory.CreateDirectory(endFolder);
                        files[i].MoveTo(Path.Combine(endFolder, time + "_" + folddrName + "_" + fileName + extension));
                    }
                    else if (sort2RadioButton.Checked)
                    {
                        string endFolder = Path.Combine(files[i].DirectoryName, time.Substring(0, 6));// 年月文件夹
                        if (!Directory.Exists(endFolder)) Directory.CreateDirectory(endFolder);
                        files[i].MoveTo(Path.Combine(endFolder, time + "_" + folddrName + "_" + fileName + extension));
                    }
                    else files[i].MoveTo(Path.Combine(files[i].DirectoryName, time + "_" + folddrName + "_" + fileName + extension));
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

        /// <summary>
        /// 
        /// </summary>
        private void sort1RadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (sort1RadioButton.Checked) outTextBox.Visible = outButton.Visible = true;
            else outTextBox.Visible = outButton.Visible = false;
        }

        ///

    }
}
