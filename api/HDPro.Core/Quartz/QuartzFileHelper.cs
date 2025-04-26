﻿using System;
using System.Collections.Generic;
using System.Text;
using HDPro.Core.Configuration;
using HDPro.Core.Extensions;
using HDPro.Core.Utilities;

namespace HDPro.Core.Quartz
{
  public static  class QuartzFileHelper
    {
        public static void OK(string message)
        {
            Write(message, "log");
        }

        public static void Error(string message)
        {
            Write(message, "error");
        }

        private static void Write(string message,string folder)
        {
            try
            {
                string fileName = DateTime.Now.ToString("yyyy-MM-dd");
                string path = $"{AppSetting.CurrentPath}\\quartz\\{folder}\\".ReplacePath();
                FileHelper.WriteFile(path, $"{fileName}.txt", message, true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"文件写入异常{message},{ex.Message + ex.StackTrace}");
            }
        }
    }
}
