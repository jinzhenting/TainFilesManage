using Shell32;
using System;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace TainFilesManage
{
    public static class DateFunction
    {

        /// <summary>
        /// 获取Exif信息
        /// </summary>
        /// <param name="imagePath">文件位置</param>
        /// <returns></returns>
        public static PropertyItem[] GetExifProperties(string imagePath)
        {
            FileStream fileStream = new FileStream(imagePath, FileMode.Open, FileAccess.Read);
            try
            {
                System.Drawing.Image image = System.Drawing.Image.FromStream(fileStream, true, false);// 通过指定的数据流来创建Image
                PropertyItem[] propertyItem = image.PropertyItems;
                fileStream.Close();
                return propertyItem;
            }
            catch (ArgumentException)
            {
                MessageBox.Show("文件：" + imagePath + "编码错误，已跳过处理", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
            finally
            {
                fileStream.Close();
            }
        }

        /// <summary>
        /// 获取图片拍摄时间
        /// </summary>
        /// <param name="parr">Exif信息</param>
        /// <returns></returns>
        public static string GetImageDate(PropertyItem[] parr)
        {
            if (parr == null) return "";
            Encoding ascii = Encoding.ASCII;
            foreach (PropertyItem propertyItem in parr) if (propertyItem.Id == 0x0132) return DateFormat(ascii.GetString(propertyItem.Value));// 遍历图像文件元数据，检索所有属性// 如果是PropertyTagDateTime，则返回该属性所对应的值
            return "";// 没有EXIF信息
        }

        /// <summary>
        /// 获取照片拍摄时间
        /// </summary>
        /// <param name="imagePath">文件路径</param>
        /// <returns></returns>
        public static string GetPhotoData(string imagePath)
        {
            var shell = new ShellClass();
            var dir = shell.NameSpace(Path.GetDirectoryName(imagePath));
            var item = dir.ParseName(Path.GetFileName(imagePath));
            if (dir.GetDetailsOf(item, 12) != "") return DateFormat(dir.GetDetailsOf(item, 12));// 12 为照片拍摄时间
            else return "";
        }

        /// <summary>
        /// 获取媒体拍摄时间
        /// </summary>
        /// <param name="imagePath">文件路径</param>
        /// <returns></returns>
        public static string GetMediaData(string imagePath)
        {
            string date = "";
            MediaInfo MI = new MediaInfo();
            MI.Open(imagePath);
            date = MI.Get(StreamKind.Video, 0, "Encoded_Date");// 视频编码UTC时间，要转换为北京时间
            MI.Close();
            if (date != "")
            {
                DateTime mediaUtcTime = Convert.ToDateTime(date.Replace("UTC ",""));
                DateTime newTime = mediaUtcTime.AddHours(8);
                return DateFormat(newTime.ToString());
            }
            else return "";
        }

        /// <summary>
        /// 格式化日期yyyyMMdd
        /// </summary>
        /// <param name="date">日期</param>
        /// <returns></returns>
        public static string DateFormat(string date)
        {
            string regex = @"([0-9]{4}).*?([0-9]{1,2}).*?([0-9]{1,2}).*?\s.*";// 
            Match match = Regex.Match(date, regex);
            if (match.Groups[0].Value == "" || match.Groups[0] == null)
            {
                Console.WriteLine(date);
                MessageBox.Show("时间格式 " + date.Replace(@"/", "-") + " 不标准，可能是系统时间格式设置造成的，请尝试修改", "提示", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
                return null;
            }
            string year_temp = match.Groups[1].Value;// 年转换
            string month_temp = Months(match.Groups[2].Value);// 月转换
            string day_temp = Day(match.Groups[3].Value);// 日转换
            return year_temp + month_temp + day_temp;
        }

        /// <summary>
        /// 日转换
        /// </summary>
        /// <param name="day">日</param>
        /// <returns></returns>
        private static string Day(string day)
        {
            string[] days = { "0", "1", "2", "3", "4", "5", "6", "7", "8", "9" };
            string[] new_days = { "00", "01", "02", "03", "04", "05", "06", "07", "08", "09" };
            for (int i = 0; i < days.Length; i++)
            {
                if (day == days[i])
                {
                    day = new_days[i];
                    break;
                }
            }
            return day;
        }

        /// <summary>
        /// 月份转换
        /// </summary>
        /// <param name="month">月份</param>
        /// <returns></returns>
        private static string Months(string month)
        {
            string[] months = { "1", "2", "3", "4", "5", "6", "7", "8", "9" };
            string[] new_months = { "01", "02", "03", "04", "05", "06", "07", "08", "09" };
            for (int i = 0; i < months.Length; i++)
            {
                if (month == months[i])
                {
                    month = new_months[i];
                    break;
                }
            }
            return month;
        }
    }

}