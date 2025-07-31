
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Newtonsoft.Json.Linq;
using ServiceStack.IO;
using ServiceStack;
using NPOI.HPSF;
using Newtonsoft.Json;

namespace HDPro.Utilities
{
    public class HDLogHelper
    {
        /// <summary>
        /// 创建文件目录
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="fileName"></param>
        public static void CreateDirectory(string filePath)
        {
            if (!Directory.Exists(filePath))//验证路径是否存在
            {
                Directory.CreateDirectory(filePath);
            }
        }
        public static void Log(string fileName, string strLog, string directory = "", bool truncate = false)
        {
            Log(DateTime.Now, fileName, strLog, directory, truncate);
        }

        /// <summary>
        /// 写日志返回虚拟路径(覆盖)
        /// 如：\2023-04-20\QuotedPrice\HB0081-2304012144.log
        /// </summary>
        /// <param name="fileName">文件名</param>
        /// <param name="fileSuffixes">后缀</param>
        /// <param name="strLog">内容</param>
        /// <param name="directory">存放文件夹</param>
        /// <returns></returns>
        public static string QuotedPriceLog(string fileName, string fileSuffixes, string strLog, string directory = "")
        {
            Tuple<string, string> pathTuple = CreateDefaultLogDirectory(DateTime.Now, fileName, fileSuffixes, directory);
            string sFileName = pathTuple.Item1;
            string virtualDirectory = pathTuple.Item2;
            File.WriteAllText(sFileName, strLog, Encoding.UTF8);
            return virtualDirectory;
        }
        /// <summary>
        /// 写日志返回虚拟路径(覆盖)
        /// 如：\2023-04-20\QuotedPrice\HB0081-2304012144.log
        /// </summary>
        /// <param name="fileName">文件名</param>
        /// <param name="fileSuffixes">后缀</param>
        /// <param name="strLog">内容</param>
        /// <param name="directory">存放文件夹</param>
        /// <returns></returns>
        public static string LogReturnVirtualDirectory(string fileName, string fileSuffixes, string strLog, string directory = "")
        {
            Tuple<string, string> pathTuple = CreateDefaultLogDirectory(DateTime.Now, fileName, fileSuffixes, directory);
            string sFileName = pathTuple.Item1;
            string virtualDirectory = pathTuple.Item2;
            File.WriteAllText(sFileName, strLog, Encoding.UTF8);
            return virtualDirectory;
        }

        private static string Log(DateTime dateTime, string fileName, string strLog, string directory = "", bool truncate = false)
        {
            Tuple<string, string> pathTuple = CreateDefaultLogDirectory(dateTime, fileName, ".log", directory);
            string sFileName = pathTuple.Item1;
            string virtualDirectory = pathTuple.Item2;
            FileMode fileModel;
            if (File.Exists(sFileName))
            {
                fileModel = truncate ? FileMode.Truncate : FileMode.Append;
            }
            else
            {
                fileModel = FileMode.Create;
            }
            using (FileStream fs = new FileStream(sFileName, fileModel, FileAccess.Write, FileShare.ReadWrite))
            {
                using (StreamWriter sw = new StreamWriter(fs, Encoding.UTF8))
                {
                    sw.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + ":" + strLog);

                    sw.Close();
                }
                fs.Close();
            }
            return virtualDirectory;
        }

        /// <summary>
        /// 创建默认日志文件夹返回（绝对路径，虚拟路径）
        /// </summary>
        /// <param name="dateTime"></param>
        /// <param name="fileName"></param>
        /// <param name="fileSuffixes">文件后缀：.log/.json</param>
        /// <param name="directory"></param>
        /// <returns></returns>
        private static Tuple<string, string> CreateDefaultLogDirectory(DateTime dateTime, string fileName, string fileSuffixes = ".log", string directory = "")
        {
            //todo 暂时写死
            string path = string.Format("{0}\\Logs\\{1}{2}",
                System.AppDomain.CurrentDomain.BaseDirectory,
                dateTime.ToString("yyyy-MM-dd"),
                string.IsNullOrWhiteSpace(directory) ? "" : ("\\" + directory));
            CreateDirectory(path);

            //文件的绝对路径
            string sFileName = fileName + fileSuffixes;
            sFileName = path + "\\" + sFileName;

            string virtualDirectory = string.Format("Logs\\{0}{1}",
                dateTime.ToString("yyyy-MM-dd"),
                string.IsNullOrWhiteSpace(directory) ? "" : ("\\" + directory));
            virtualDirectory = virtualDirectory + "\\" + fileName + fileSuffixes;

            return Tuple.Create(sFileName, virtualDirectory);
        }

        public static void Log(string fileName, List<string> strLogLst)
        {
            Log(DateTime.Now, fileName, strLogLst);
        }
        public static void Log(string fileName, JArray saveObjectArr)
        {
            List<string> strLogLst = new List<string>();
            foreach (var saveObject in saveObjectArr)
            {
                if (!Convert.ToBoolean(saveObject["Result"]["ResponseStatus"]["IsSuccess"].ToString()))
                {
                    string erroMesg = saveObject["Result"]["ResponseStatus"]["Errors"].ToString();
                    if (!string.IsNullOrWhiteSpace(erroMesg))
                    {
                        strLogLst.Add(erroMesg);
                    }
                }
            }
            if (strLogLst.Count > 0)
            {
                Log(fileName, strLogLst);
            }
        }
        public static void Log(DateTime dateTime, string fileName, List<string> strLogLst)
        {
            //todo 暂时写死
            string path = System.AppDomain.CurrentDomain.BaseDirectory + "\\Logs\\" + dateTime.ToString("yyyy-MM-dd");

            CreateDirectory(path);

            string sFileName = fileName + ".log";
            sFileName = path + "\\" + sFileName; //文件的绝对路径

            FileMode fileModel;
            if (File.Exists(sFileName))
            {
                fileModel = FileMode.Append;
            }
            else
            {
                fileModel = FileMode.Create;
            }
            using (FileStream fs = new FileStream(sFileName, fileModel, FileAccess.Write, FileShare.ReadWrite))
            {
                using (StreamWriter sw = new StreamWriter(fs, Encoding.UTF8))
                {
                    foreach (var strLog in strLogLst)
                    {
                        sw.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + ":" + strLog);
                    }
                    sw.Close();

                }
                fs.Close();
            }
        }

        /// <summary>
        /// 获取到本地的Json文件并且解析返回对应的json字符串
        /// </summary>
        /// <param name="filepath">文件路径</param>
        /// <returns></returns>
        public static string GetJsonFile(string filePath)
        {
            string jsonFilePath = $"{System.AppDomain.CurrentDomain.BaseDirectory}//{filePath}";
            string json = string.Empty;
            using (FileStream fs = new FileStream(jsonFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (StreamReader sr = new StreamReader(fs, Encoding.UTF8))
                {
                    json = sr.ReadToEnd().ToString();
                }
            }
            return json;
        }
     
        public static StringBuilder ReadLogFromFile( string filePath)
        {
            StringBuilder result = new StringBuilder();

           
            return result;
        }
        public static List<string> ReadLogFromFile(string dir, string fileName)
        {
            List<string> result = new List<string>();

            string path = System.AppDomain.CurrentDomain.BaseDirectory + "\\App_Data\\logs\\" + dir + "\\" + fileName + ".log";
            if (!File.Exists(path))
            {
                return null;
            }
            using (FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (StreamReader sr = new StreamReader(fs, Encoding.UTF8))
                {
                    String line;
                    while ((line = sr.ReadLine()) != null)
                    {
                        result.Add(line.ToString());
                    }
                    sr.Close();
                }
                fs.Close();
            }
            return result;
        }
    }
}
