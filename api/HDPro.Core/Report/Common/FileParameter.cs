namespace HDPro.Core.common
{
    public class FileParameter
    {

        //report=1a&data=Customer&type=pdf&open=inline
        /// <summary>
        /// 报表模板
        /// </summary>
        public string report { get; set; }
        /// <summary>
        /// 报表数据
        /// </summary>
        public string data { get; set; }
        /// <summary>
        /// 文件类型
        /// </summary>
        public string type { get; set; }
        /// <summary>
        /// 下载还是浏览器内联打开
        /// </summary>
        public string open { get; set; }
        /// <summary>
        /// 图片类型
        /// </summary>
        public string img { get; set; }
        /// <summary>
        /// 下载文件名
        /// </summary>
        public string filename { get; set; }

    }

}
