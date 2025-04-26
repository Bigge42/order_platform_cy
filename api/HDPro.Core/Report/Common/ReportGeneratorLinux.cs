using System;
using Microsoft.AspNetCore.Http;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using System.Runtime.InteropServices;
using System.IO;
using HDPro.Core.Extensions;
using gridreport;
using HDPro.Core.Report.PInvokeWindows;
using E2IMGOption = gridreport.E2IMGOption;

namespace HDPro.Core.common
{
    public class ReportGeneratorLinux
    {

        private ReportLinux report;
        public ReportLinux ReportLinux => report;

        private readonly IWebHostEnvironment _hostingEnvironment;

        public ReportGeneratorLinux(IWebHostEnvironment hostingEnvironment)
        {
              _hostingEnvironment = hostingEnvironment;
        }

        /// <summary>
        /// 初始化报表对象
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public ReportLinux InitReport()
        {
            report = new ReportLinux();
            if (report?.NativeHandle == null) throw new Exception("Create report failed.");
            return report;
        }

        /// <summary>
        /// 加载模板
        /// </summary>
        /// <param name="reportID"></param>
        /// <exception cref="Exception"></exception>
        public void LoadReport(string reportID)
        {
            string reportPathFile = Path.Combine(_hostingEnvironment.ContentRootPath, reportID).ReplacePath();
            if (!File.Exists(reportPathFile)) throw new Exception("模板文件不存在:" + reportPathFile);
            bool success = report.LoadFromFile(reportPathFile);
            if (!success) throw new Exception(string.Format("载入报表模板 '{0}' 失败！", reportPathFile));
            Console.WriteLine(string.Format("载入报表模板 '{0}' 成功！", reportPathFile));
        }

        /// <summary>
        /// 加载报表数据
        /// </summary>
        /// <param name="DataText"></param>
        /// <exception cref="Exception"></exception>
        public void LoadReportData(string DataText)
        {
            bool success = report.LoadDataFromXML(DataText);
            if (!success)   throw new Exception(string.Format("载入报表数据:\r\n '{0}' \r\n失败！", DataText));
        }
        public FileData Generate(FileParameter fileParameter)
        {
            return Generate(fileParameter.type, fileParameter.filename, fileParameter.img);
        }
        private FileData Generate(string TypeText, string FileName, string ImageTypeText)
        {
            //确定导出数据类型及数据的ContentType
            ReportGenerateInfo GenerateInfo = new ReportGenerateInfo();
            GenerateInfo.Build(TypeText, ImageTypeText);

            gridreport.BinaryObject ResultDataObject;
            if (GenerateInfo.IsGRD)
            {
               // Core.Report.PInvokeWindows.Report.GenerateDocumentData();
                ResultDataObject = report.GenerateDocumentData();
            }
            else
            {
                gridreport.ExportOption ExportOption = report.PrepareExport(GenerateInfo.ExportType);

                if (GenerateInfo.ExportType == ExportType.IMG)
                {
                    E2IMGOption E2IMGOption = ExportOption.AsE2IMGOption;
                    E2IMGOption.ImageType = GenerateInfo.ImageType;
                    E2IMGOption.AllInOne = true; //所有页产生在一个图像文件中
                    //E2IMGOption.VertGap = 20;    //页之间设置20个像素的间距
                }

                ResultDataObject = report.ExportToBinaryObject();
                report.UnprepareExport();
            }



            //如果参数中没指定文件名，则用报表模板中的“标题”属性设置一个默认文件名
            if (string.IsNullOrWhiteSpace(FileName))
            {
                FileName = report.Title;
                if (string.IsNullOrWhiteSpace(FileName)) FileName = "gridreport";
                FileName += "." + GenerateInfo.ExtFileBame;
            }
            if (ResultDataObject == null || ResultDataObject.DataSize <= 0) throw new Exception("未产生报表数据");


            byte[] managedArray = new byte[ResultDataObject.DataSize];
            Marshal.Copy(ResultDataObject.DataBuf, managedArray, 0, ResultDataObject.DataSize);

            FileData data = new FileData { FileContent = managedArray, FileName = FileName, ContentType = GenerateInfo.ContentType };
            return data;
        }
    }
}
