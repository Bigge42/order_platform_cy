using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HDPro.Core.common;
using HDPro.Core.Dapper;
using HDPro.Core.DBManager;
using HDPro.Core.EFDbContext;
using HDPro.Core.Extensions;
using HDPro.Core.Services;
using HDPro.Entity.DomainModels;

namespace HDPro.Core.Controllers.Basic
{
    public class ReportBaseController : VolController
    {

        //private readonly IWebHostEnvironment _hostingEnvironment;

        public ReportBaseController()
        {

        }

        private ReportOption _reportOptions = null;
        protected BaseDbContext DbContext { get; set; }

        protected ISqlDapper DapperContext { get; set; }
        protected ReportOption ReportOptions
        {
            get
            {
                if (_reportOptions == null)
                {
                    string code = HttpContext.Request.Query["code"];
                    _reportOptions = DBServerProvider.DbContext.Set<Sys_ReportOptions>().Where(x => x.ReportCode == code)
                                    .Select(s => new ReportOption()
                                    {
                                        ReportOptionsId = s.ReportOptionsId,
                                        ReportCode = s.ReportCode,
                                        DbService = s.DbService,
                                        Sql = s.Options,
                                        ParentId = s.ParentId,
                                        ReportName = s.ReportName,
                                        ReportType = s.ReportType,
                                        FilePath = s.FilePath,

                                    }).ToList().FirstOrDefault();
                    if (_reportOptions == null)
                    {
                        Console.Write($"模板[{code}]不存在");
                    }
                    else
                    {
                        DbContext = HttpContext.RequestServices.GetService(DbRelativeCache.GetDbContextType(_reportOptions.DbService)) as BaseDbContext;

                        DapperContext = DBServerProvider.GetSqlDapperWidthDbService(_reportOptions.DbService);
                    }
                }
                return _reportOptions;
            }
        }

        protected object Data = null;
        [HttpGet, HttpPost, Route("getTemplateData")]
        public virtual IActionResult GetTemplateData(string code)
        {
            if (ReportOptions == null)
            {
                return Error("模板不存在");
            }
            string filePath = ReportOptions.FilePath.MapPath(false);
            string text = System.IO.File.ReadAllText(filePath);

            Data = GetData(code);
            if (Data != null)
            {
                return Success(null, new { text, data = Data });
            }
            if (Data == null && !string.IsNullOrEmpty(ReportOptions.Sql))
            {
                Data = DapperContext.QueryList<object>(ReportOptions.Sql, new { });
            }
            return Success(null, new { text, data = new { Table = Data } });
        }

        protected virtual object GetData(string code)
        {
            return null;
        }

        [HttpGet, HttpPost, Route("download")]
        public virtual IActionResult Download([FromBody] FileParameter fileParameter, string code)
        {
            //try
            //{
            //    FileData file;
            //    if (StringExtension._windows)
            //    {
            //        file = GetWindowsFileData(fileParameter, code);
            //    }
            //    else
            //    {
            //        file = GetLinuxFileData(fileParameter, code);
            //    }
            //    if (fileParameter.open == "inline") return File(file.FileContent, file.ContentType);

            //    return File(file.FileContent, file.ContentType, file.FileName);
            //}
            //catch (Exception ex)
            //{
            //    string msg = $"报表导出异常：" + ex.Message + ex.StackTrace;
            //    Console.WriteLine(msg);
            //    Logger.Error(msg);
            //    return Content("error");
            //}
            return Content("ok");
        }

        private FileData GetWindowsFileData(FileParameter fileParameter, string code)
        {
            var _hostingEnvironment = HttpContext.GetService<IWebHostEnvironment>();
            var _reportGenerator = new ReportGeneratorWindows(_hostingEnvironment);
            _reportGenerator.InitReport();
            _reportGenerator.LoadReport(ReportOptions.FilePath);
            string reportDataText = GetData(code).Serialize();
            _reportGenerator.LoadReportData(reportDataText);
            var file = _reportGenerator.Generate(fileParameter);
            return file;
        }

        private FileData GetLinuxFileData(FileParameter fileParameter, string code)
        {
            var _hostingEnvironment = HttpContext.GetService<IWebHostEnvironment>();
            var _reportGenerator = new ReportGeneratorLinux(_hostingEnvironment);
            _reportGenerator.InitReport();
            _reportGenerator.LoadReport(ReportOptions.FilePath);
            string reportDataText = GetData(code).Serialize();
            _reportGenerator.LoadReportData(reportDataText);
            var file = _reportGenerator.Generate(fileParameter);
            return file;
        }
    }

    public class ReportOption
    {

        [Key]
        [Display(Name = "ReportOptionsId")]
        [Column(TypeName = "uniqueidentifier")]
        [Editable(true)]
        [Required(AllowEmptyStrings = false)]
        public Guid ReportOptionsId { get; set; }

        /// <summary>
        ///报表名称
        /// </summary>
        [Display(Name = "报表名称")]
        [MaxLength(100)]
        [Column(TypeName = "nvarchar(100)")]
        [Editable(true)]
        [Required(AllowEmptyStrings = false)]
        public string ReportName { get; set; }

        /// <summary>
        ///报表编码
        /// </summary>
        [Display(Name = "报表编码")]
        [MaxLength(100)]
        [Column(TypeName = "nvarchar(100)")]
        [Editable(true)]
        [Required(AllowEmptyStrings = false)]
        public string ReportCode { get; set; }

        /// <summary>
        ///所在数据库
        /// </summary>
        [Display(Name = "所在数据库")]
        [MaxLength(100)]
        [Column(TypeName = "nvarchar(100)")]
        [Editable(true)]
        public string DbService { get; set; }

        /// <summary>
        ///报表类型
        /// </summary>
        [Display(Name = "报表类型")]
        [MaxLength(100)]
        [Column(TypeName = "varchar(100)")]
        [Editable(true)]
        public string ReportType { get; set; }

        /// <summary>
        ///父级id
        /// </summary>
        [Display(Name = "父级id")]
        [Column(TypeName = "uniqueidentifier")]
        [Editable(true)]
        public Guid? ParentId { get; set; }

        /// <summary>
        ///模板文件
        /// </summary>
        [Display(Name = "模板文件")]
        [MaxLength(2000)]
        [Column(TypeName = "nvarchar(2000)")]
        [Editable(true)]
        [Required(AllowEmptyStrings = false)]
        public string FilePath { get; set; }

        /// <summary>
        ///数据源sql
        /// </summary>
        [Display(Name = "数据源sql")]
        [MaxLength(2000)]
        [Column(TypeName = "nvarchar(2000)")]
        [Editable(true)]
        public string Sql { get; set; }


    }
}
