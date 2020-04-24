using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
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
            DirectoryInfo directorys = new DirectoryInfo(inTextBox.Text);// 遍历文件夹
            FileInfo[] files = directorys.GetFiles("*.jpg", SearchOption.AllDirectories);
            foreach (FileInfo file in files)
            {
                string time = DateTimeFunction.GetTakePicDateTime(DateTimeFunction.GetExifProperties(file.FullName));
                file.MoveTo(Path.Combine(file.DirectoryName, ""+time+""+ file.Name));
            }
            MessageBox.Show("OK");
        }
    }
}
