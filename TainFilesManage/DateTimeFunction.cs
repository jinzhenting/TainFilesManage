using System;
using System.Drawing.Imaging;
using System.IO;
using System.Text;

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
            System.Drawing.Image image = System.Drawing.Image.FromStream(fileStream, true, false);// 通过指定的数据流来创建Image
            PropertyItem[] propertyItem = image.PropertyItems;
            fileStream.Close();
            return propertyItem;
        }

        /// <summary>
        /// 获取照片拍摄时间
        /// </summary>
        /// <param name="parr">Exif信息</param>
        /// <returns></returns>
        public static string GetTakePicDateTime(PropertyItem[] parr)
        {
            Encoding ascii = Encoding.ASCII;
            foreach (PropertyItem propertyItem in parr)
            {
                if (propertyItem.Id == 0x0132)
                {
                    //DateTime dataTime = Convert.ToDateTime(ascii.GetString(propertyItem.Value).Replace(":","/"));
                    //return dataTime.ToString("yyyy-MM-dd HH:mm:ss");
                    return ascii.GetString(propertyItem.Value).Substring(0, 10).Replace(":", "-");
                }
            }// 遍历图像文件元数据，检索所有属性// 如果是PropertyTagDateTime，则返回该属性所对应的值
            return "N-A";// 若没有相关的EXIF信息则返回N-A
        }
    }
}
