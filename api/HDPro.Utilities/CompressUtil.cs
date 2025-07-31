using SharpCompress.Common;
using SharpCompress.Readers;
using SharpCompress.Writers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HDPro.Utilities
{
    public class CompressUtil
    {


        /// <summary>
        /// 压缩文件/文件夹
        /// </summary>
        /// <param name="filePath">需要压缩的文件/文件夹路径</param>
        /// <param name="zipPath">压缩文件路径（zip后缀）</param>
        /// <param name="filterExtenList">需要过滤的文件后缀名</param>
        public static void CompressionFile(string filePath, string zipPath, List<string> filterExtenList = null)
        {
            try
            {
                using (var zip = File.Create(zipPath))
                {
                    //设置编码格式防止乱码
                    var option = new WriterOptions(CompressionType.Deflate)
                    {
                        ArchiveEncoding = new SharpCompress.Common.ArchiveEncoding()
                        {
                            Default = Encoding.UTF8
                        }
                    };
                    using (var zipWriter = WriterFactory.Open(zip, ArchiveType.Zip, option))
                    {
                        if (Directory.Exists(filePath))
                        {
                            //添加文件夹
                            zipWriter.WriteAll(filePath, "*",
                              (path) => filterExtenList == null ? true : !filterExtenList.Any(d => Path.GetExtension(path).Contains(d, StringComparison.OrdinalIgnoreCase)), SearchOption.AllDirectories);
                        }
                        else if (File.Exists(filePath))
                        {
                            zipWriter.Write(Path.GetFileName(filePath), filePath);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogUtil.Error("解压异常", ex);
            }
        }


        /// <summary>
        /// 解压文件
        /// </summary>
        /// <param name="zipPath">压缩文件路径</param>
        /// <param name="dirPath">解压到文件夹路径</param>
        /// <param name="password">密码</param>
        public static void DeCompressionFile(string zipPath, string dirPath, string password = "")
        {
            if (!File.Exists(zipPath))
            {
                throw new ArgumentNullException("zipPath压缩文件不存在");
            }
            Directory.CreateDirectory(dirPath);
            try
            {
                using (Stream stream = File.OpenRead(zipPath))
                {
                    //设置编码格式防止乱码(文件内容和文件名)
                    var option = new ReaderOptions()
                    {
                        ArchiveEncoding = new SharpCompress.Common.ArchiveEncoding()
                        {
                            Default = Encoding.UTF8
                        }
                    };
                    if (!string.IsNullOrWhiteSpace(password))
                    {
                        option.Password = password;
                    }

                    var reader = ReaderFactory.Open(stream, option);
                    while (reader.MoveToNextEntry())
                    {
                        if (reader.Entry.IsDirectory)
                        {
                            Directory.CreateDirectory(Path.Combine(dirPath, reader.Entry.Key));
                        }
                        else
                        {
                            //创建父级目录，防止Entry文件,解压时由于目录不存在报异常
                            var file = Path.Combine(dirPath, reader.Entry.Key);
                            Directory.CreateDirectory(Path.GetDirectoryName(file));
                            reader.WriteEntryToFile(file);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogUtil.Error("解压异常",ex);
            }
        }


    }
}
