using gridreport;
using System.Web;

namespace HDPro.Core.common
{
    public class ReportGenerateInfo
    {
        public string ContentType;          //HTTP响应ContentType 
        public string ExtFileBame;          //默认扩展文件名
        public bool IsGRD;                  //是否生成为 Grid++Report 报表文档格式
        public ExportType ExportType;     //导出的数据格式类型
        public ExportImageType ImageType; //导出的图像格式类型

        public void Build(string ExportTypeText, string ImageTypeText)
        {
            ExtFileBame = ExportTypeText;
            ContentType = "application/";
            IsGRD = (ExportTypeText == "grd" || ExportTypeText == "grp");

            if (IsGRD)
            {
                //ContentType += ExportTypeText; //application/grd
                ContentType += "octet-stream"; //application/octet-stream
            }
            else
            {
                switch (ExportTypeText)
                {
                    case "xls":
                        ExportType = ExportType.XLS;
                        ContentType += "x-xls"; //application/vnd.ms-excel application/x-xls
                        break;
                    case "csv":
                        ExportType = ExportType.CSV;
                        ContentType += "vnd.ms-excel"; //application/vnd.ms-excel application/x-xls
                        break;
                    case "txt":
                        ExportType = ExportType.TXT;
                        ContentType = "text/plain"; //text/plain
                        break;
                    case "rtf":
                        ExportType = ExportType.RTF;
                        ContentType += "rtf"; //application/rtf
                        break;
                    case "img":
                        ExportType = ExportType.IMG;
                        //ContentType 要在后面根据图像格式来确定
                        break;
                    default:
                        ExtFileBame = "pdf"; //"type"参数如没有设置，保证 ExtFileBame 被设置为"pdf"
                        ExportType = ExportType.PDF;
                        ContentType += "pdf";
                        break;
                }

                //导出图像处理
                if (ExportType == ExportType.IMG)
                {
                    ExtFileBame = ImageTypeText;
                    switch (ImageTypeText)
                    {
                        case "bmp":
                            ImageType = ExportImageType.BMP;
                            ContentType += "x-bmp";
                            break;
                        case "jpg":
                            ImageType = ExportImageType.JPEG;
                            ContentType += "x-jpg";
                            break;
                        case "tif":
                            ImageType = ExportImageType.TIFF;
                            ContentType = "image/tiff";
                            break;
                        default:
                            ExtFileBame = "png";
                            ImageType = ExportImageType.PNG;
                            ContentType += "x-png";
                            break;
                    }
                }
            }
        }


    }
}
