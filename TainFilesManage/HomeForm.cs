﻿using System;
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
                sortBackgroundWorker.ReportProgress(Percents.Get(i, files.Length), files[i].FullName);// 进度传出

                ///

                bool rename = Rename(files[i].Name);// 是否重命名
                bool sort = !noSortRadioButton.Checked;// 是否归类
                if (!rename && !sort) continue;// 

                #region 关键元素
                string inPath = files[i].FullName;// 旧路径
                string inName = Path.GetFileNameWithoutExtension(files[i].Name).Replace("mmexport", "").Replace("wx_camera_", "");// 不包含扩展名的文件名
                string inExtension = Path.GetExtension(files[i].Name);// 旧扩展名
                string outPath = "";// 新路径
                string outName = "";// 不包含扩展名的文件名
                string outExtension = inExtension.ToLower();// 新扩展名
                string specifyFolder = outTextBox.Text;// 指定目标文件夹路径
                string originalFolder = files[i].DirectoryName;// 自身文件夹路径
                string originalFolderName = new DirectoryInfo(originalFolder).Name;// 自身文件夹名字
                string yyyyMM = "";// 年月关键字
                #endregion 关键元素

                try
                {
                    #region 获取年月关键字
                    if (!rename) yyyyMM = files[i].Name.Substring(0, 6);// 不须重命名时，通过文件名获取年月关键字
                    else // 需要重命名时，通过读取文件信息获得年月关键字
                    {
                        switch (outExtension) // 扩展名检测 
                        {
                            case ".jpg":// 照片类，必须是相机生成的才有拍摄日期，软件生成的没有
                            case ".jpeg":
                            case ".arw":
                                {
                                    yyyyMM = DateFunction.GetPhotoData(inPath);// 获取照片拍摄时间 // 返回空白，即此方法没有获取到有效信息
                                    break;
                                }
                            case ".mkv":// Matroska // 多媒体类，MediaInfo支持的格式，读取编码日期
                            case ".mka":// Matroska
                            case ".mks":// Matroska
                            case ".ogg":// Ogg 
                            case ".ogm":// Ogg
                            case ".avi":// Riff
                            case ".wav":// Riff
                            case ".mpeg":// Mpeg 1&2
                            case ".mpg":// Mpeg 1&2
                            case ".vob":// Mpeg 1&2
                            case ".mp4":// Mpeg 4 container
                            case ".mpgv":// Mpeg video specific
                            case ".mpv":// Mpeg video specific
                            case ".m1v":// Mpeg video specific
                            case ".m2v":// Mpeg video specific
                            case ".mp2":// Mpeg audio specific
                            case ".mp3":// Mpeg audio specific
                            case ".asf":// Windows Media
                            case ".wma":// Windows Media
                            case ".wmv":// Windows Media
                            case ".qt":// Quicktime 
                            case ".mov":// Quicktime 
                            case ".rm":// Real
                            case ".rmvb":// Real
                            case ".ra":// Real
                            case ".ifo":// DVD-Video
                            case ".ac3":// AC3 
                            case ".dts":// DTS 
                            case ".aac":// AAC  
                            case ".ape":// Monkey's Audio 
                            case ".mac":// Monkey's Audio 
                            case ".flac":// Flac 
                            case ".dat":// CDXA, like Video-CD
                            case ".aiff":// Apple/SGI
                            case ".aifc":// Apple/SGI
                            case ".au":// Sun/NeXT 
                            case ".iff":// Amiga IFF/SVX8/SV16
                            case ".paf":// Ensoniq PARIS
                            case ".sd2":// Sound Designer 2
                            case ".irca":// Berkeley/IRCAM/CARL
                            case ".w64":// SoundFoundry WAVE 64
                            case ".mat":// Matlab 
                            case ".pvf":// Portable Voice format
                            case ".xi":// FastTracker2 Extanded
                            case ".sds":// Midi Sample dump Format
                            case ".avr":// Audio Visual Research
                                {
                                    yyyyMM = DateFunction.GetMediaData(inPath);// 获取媒体拍摄时间 // 返回空白，即此方法没有获取到有效信息
                                    break;
                                }
                            default:
                                {
                                    yyyyMM = "";
                                    if (MessageBox.Show("当前文件：" + inPath + "不支持获取拍摄时间\r\n\r\n是以最后的修改时间归类？", "提示", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK) break;
                                    else continue;
                                }
                        }
                        if (yyyyMM == null)// 如果遍历完文件信息后，yyyyMM为null，表示通过读取文件信息获得年月关键字，时间格式化成yyyyMM时失败了
                        {
                            DialogResult msg = MessageBox.Show("当前文件：" + inPath + "获取最后的修改时间失败\r\n\r\n点击 “是” 以文件的最后修改时间归类此文件其他文件\r\n\r\n点击 “否” 跳过些文件，继续归类其他文件\r\n\r\n点击 “取消” 不再处理其他文件", "提示", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                            if (msg == DialogResult.Yes) { }
                            else if (msg == DialogResult.No) break;
                            else
                            {
                                e.Cancel = true;
                                return;
                            }
                        }
                    }
                    if (yyyyMM == "") yyyyMM = string.Format("{0:yyyyMMdd}", files[i].LastWriteTime);// 为空，表示通过读取文件信息获得年月关键字未成功 // 通过文件最后写入时间获得年月关键字
                    #endregion 获取年月关键字

                    #region 重命名
                    if (rename)// 须重命名时
                    {
                        outName = yyyyMM + "_" + originalFolderName + "_" + inName + outExtension;
                        outPath = Path.Combine(originalFolder, outName);
                        files[i].MoveTo(outPath);
                        inPath = outPath;// 重命名后，将新路径赋绘inPath，不然后面归类时找不到文件
                    }
                    else outName = inName + outExtension;// 不须重命名时
                    #endregion 重命名

                    #region 归类
                    if (sort)// 不须归类时跳过
                    {
                        if (yyyyMM == originalFolderName) continue;// 自身文件夹名与年月关键字相同时不归类
                        if (specifyRadioButton.Checked)// 归类到指定文件夹
                        {
                            outPath = Path.Combine(specifyFolder, yyyyMM, outName);
                            Sort(inPath, outPath);
                            continue;
                        }
                        if (originalRadioButton.Checked)// 在自身文件夹中归类
                        {
                            /// 以后增加父级文件夹同名检测
                            outPath = Path.Combine(originalFolder, yyyyMM, outName);
                            Sort(inPath, outPath);
                            continue;
                        }
                    }
                    #endregion 获取年月关键字
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

            ///

        }

        /// <summary>
        /// 是否已重命名
        /// </summary>
        /// <param name="fileName">文件名</param>
        /// <returns></returns>
        private bool Rename(string fileName)
        {
            Regex regNum = new Regex(@"^(([0-9]{6})[_]).*");// 检测文件是否已重命名
            if (regNum.IsMatch(fileName)) return false;// 不须重命名
            else return true;
        }

        /// <summary>
        /// 文件归类
        /// </summary>
        /// <param name="inPath">旧路径</param>
        /// <param name="outPath">新路径</param>
        private void Sort(string inPath, string outPath)
        {
            if (inPath.ToLower() == outPath.ToLower()) return;// 相同位置时跳过
            if (File.Exists(outPath))// 如果新文件已存在同名文件
            {
                FileInfo inFile = new FileInfo(inPath);// 旧文件
                FileInfo outFile = new FileInfo(outPath);// 新文件
                if (inFile.LastWriteTime == outFile.LastWriteTime && inFile.Length == outFile.Length) File.Delete(inPath);// 最后修改时间和文件大小一样，即文件相同，则删除旧文件
                else File.Move(inPath, GetNewPathForDupes(outPath));// 如不文件不同，则生成版本号再移动到新位置
                return;
            }
            string newFolder = Path.GetDirectoryName(outPath);// 新位置文件夹
            if (!Directory.Exists(newFolder)) Directory.CreateDirectory(newFolder);// 如果新位置文件夹不存在，则创建
            File.Move(inPath, outPath);
        }

        /// <summary>
        /// 文件名增加版本（_x）后缀
        /// </summary>
        /// <param name="name">传入文件名</param>
        /// <returns>传出文件名</returns>
        private string GetNewPathForDupes(string name)
        {
            string directory = Path.GetDirectoryName(name);// 目录
            string fileName = Path.GetFileNameWithoutExtension(name);// 文件名
            string extension = Path.GetExtension(name);// 扩展名
            int counter = 1;
            string newFullName;
            do
            {
                string newFilename = fileName + "_" + counter.ToString() + extension;
                newFullName = Path.Combine(directory, newFilename);
                counter++;
            } while (File.Exists(newFullName));
            return newFullName;
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
            if (specifyRadioButton.Checked) outTextBox.Visible = outButton.Visible = true;
            else outTextBox.Visible = outButton.Visible = false;
        }

        ///

    }
}
