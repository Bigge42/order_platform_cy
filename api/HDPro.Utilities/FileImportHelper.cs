using Microsoft.AspNetCore.Http;

namespace MT.BOM.Utilities
{
    public class FileImportHelper
    {
        public static string UploadExcelFile(IFormFile file)
        {
            string filePath = $"FileUpload/{DateTime.Now:yyyyMMddHHmmss}";
            string directoryName = $"{System.AppDomain.CurrentDomain.BaseDirectory}/{filePath}";
           
            if (!Directory.Exists(directoryName))
            {
                Directory.CreateDirectory(directoryName);
            }
            string fileName = Path.GetFileName(file.FileName);

            string fullFileName = string.Format("{0}/{1}", directoryName, fileName);
            if (System.IO.File.Exists(fullFileName))
            {
                System.IO.File.Delete(fullFileName);
            }
            //将流写入文件
            using (Stream stream = file.OpenReadStream())
            {
                // 把 Stream 转换成 byte[]
                byte[] bytes = new byte[stream.Length];
                stream.Read(bytes, 0, bytes.Length);
                // 设置当前流的位置为流的开始
                stream.Seek(0, SeekOrigin.Begin);
                // 把 byte[] 写入文件
                FileStream fs = new FileStream(fullFileName, FileMode.Create);
                BinaryWriter bw = new BinaryWriter(fs);
                bw.Write(bytes);
                bw.Close();
                fs.Close();
            }

            return $"{filePath}/{fileName}"; 
        }
        public static string GetFullFileName(string filePath)
        {
            return $"{System.AppDomain.CurrentDomain.BaseDirectory}/{filePath}";
        }
    }
}