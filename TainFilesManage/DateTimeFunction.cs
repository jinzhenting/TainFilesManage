using Shell32;
using System;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace TainFilesManage
{
    public static class DateTimeFunction
    {

        /// <summary>
        /// 获取Exif信息
        /// </summary>
        /// <param name="fileName">文件位置</param>
        /// <returns></returns>
        public static PropertyItem[] GetExifProperties(string fileName)
        {
            FileStream fileStream = new FileStream(fileName, FileMode.Open, FileAccess.Read);
            try
            {
                System.Drawing.Image image = System.Drawing.Image.FromStream(fileStream, true, false);// 通过指定的数据流来创建Image
                PropertyItem[] propertyItem = image.PropertyItems;
                fileStream.Close();
                return propertyItem;
            }
            catch (ArgumentException)
            {
                MessageBox.Show("文件：" + fileName + "编码错误，已跳过处理", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
            finally
            {
                fileStream.Close();
            }
        }

        /// <summary>
        /// 获取照片拍摄时间
        /// </summary>
        /// <param name="parr">Exif信息</param>
        /// <returns></returns>
        public static string GetTakePicDateTime(PropertyItem[] parr)
        {
            if (parr == null) return "";
            Encoding ascii = Encoding.ASCII;
            foreach (PropertyItem propertyItem in parr)// 遍历图像文件元数据，检索所有属性// 如果是PropertyTagDateTime，则返回该属性所对应的值
            {
                if (propertyItem.Id == 0x0132) return ascii.GetString(propertyItem.Value).Substring(0, 10).Replace(":", "");
            }
            return "";// 若没有相关的EXIF信息则返回N-A
        }


        public static string GetTakeMediaDataTime(string path)
        {
            var shell = new ShellClass();
            var dir = shell.NameSpace(Path.GetDirectoryName(path));
            var item = dir.ParseName(Path.GetFileName(path));
            try
            {
                MessageBox.Show(dir.GetDetailsOf(item, 12));
                if (dir.GetDetailsOf(item, 12) != "")
                {
                    DateTime time = DateTime.ParseExact(dir.GetDetailsOf(item, 12).Replace("\","")."yyyyMMdd",System.Globalization.CultureInfo.CurrentCulture);
                    return string.Format("{0:yyyyMMdd}", time);// 12 为照片拍摄时间}
                }
                return "";
            }
            catch (FormatException ex)
            {
                MessageBox.Show(ex.ToString());
                MessageBox.Show("系统时间格式不标准，请先设置再重新处理", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
                return "";
            }
        }
    }

}