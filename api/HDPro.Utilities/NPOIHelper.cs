using Npoi.Mapper;
using NPOI.SS.Formula.Functions;
using NPOI.SS.UserModel;
using NPOI.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HDPro.Utilities
{
    public class NPOIHelper
    {
        protected void CheckColumnNameInSheetHead(Mapper npoiMapper, List<string> result, string sheetName, List<string> columnNames)
        {
            IEnumerable<RowInfo<dynamic>> objs = npoiMapper.TakeDynamicWithColumnType(getColumnType: _ => typeof(string), sheetName);
            var obj = objs?.FirstOrDefault();
            if (obj != null)
            {
                List<string> columns = new List<string>();
                foreach (var columnName in columnNames)
                {
                    if (!CommonUtils.IsPropertyExist(obj.Value, columnName))
                    {
                        columns.Add(columnName);
                    }
                }
                if (columns.Count > 0)
                {
                    result.Add($"在【{sheetName}】sheet中未找到以下表头字段：【{string.Join("】，【", columns)}】");
                }
            }
        }
        /// <summary>
        /// NPOI 列转行
        /// </summary>
        /// <param name="sheet">sheet页签名</param>
        /// <param name="rowKey">行（比如：模型）</param>
        /// <param name="columnKey">列（比如：结构）</param>
        /// <param name="valKey">值（行列的交集）</param>
        /// <returns></returns>
        public static List<dynamic> Column2Row(ISheet sheet, string rowKey, string columnKey, string valKey)
        {
            IRow headerRow = sheet.GetRow(0);
            int cellCount = headerRow.LastCellNum;
            List<dynamic> dynamicList = new List<dynamic>();
            List<string> columnNames = new List<string>();
            for (int c = 1; c <= cellCount; c++)
            {
                var cellValue = Convert.ToString(headerRow?.GetCell(c));
                if (string.IsNullOrEmpty(cellValue))
                {
                    continue;
                }
                columnNames.Add(cellValue);
            }
            for (int r = 1; r <= sheet.LastRowNum; r++)
            {
                var row = sheet.GetRow(r);
                string rowValue = Convert.ToString(row?.GetCell(0));
                if (string.IsNullOrEmpty(rowValue))
                {
                    continue;
                }
                for (int c = 1; c <= cellCount; c++)
                {
                    var cellValue = Convert.ToString(row?.GetCell(c));
                    if (string.IsNullOrEmpty(cellValue))
                    {
                        continue;
                    }
                    dynamic p = new ExpandoObject();
                    var dic = p as IDictionary<string, object>;
                    dic.Add(rowKey, rowValue);
                    dic.Add(columnKey, columnNames[c - 1]);
                    dic.Add(valKey, cellValue);
                    dynamicList.Add(p);
                }
            }
            return dynamicList;
        }

        /// <summary>
        /// NPOI 列转行
        /// </summary>
        /// <param name="sheet">sheet页签名</param>
        /// <param name="rowKey">行（比如：模型）</param>
        /// <param name="columnKey">列（比如：结构）</param>
        /// <param name="valKey">值（行列的交集）</param>
        ///  <param name="ParentColumnKey">父列</param>
        /// <returns></returns>
        public static List<dynamic> Column2RowWithParent(ISheet sheet, string rowKey, string columnKey, string valKey, string ParentModelNameKey = "ParentModelName")
        {
            IRow headerRow = sheet.GetRow(1);
            int cellCount = headerRow.LastCellNum;
            List<dynamic> dynamicList = new List<dynamic>();
            List<string> columnNames = new List<string>();
            for (int c = 1; c <= cellCount; c++)
            {
                var cellValue = Convert.ToString(headerRow?.GetCell(c));
                if (string.IsNullOrEmpty(cellValue))
                {
                    continue;
                }
                columnNames.Add(cellValue);
            }

            IRow parentRow = sheet.GetRow(0);
            List<string> ParentColumnNames = new List<string>();
            for (int c = 1; c <= cellCount; c++)
            {
                var cellValue = Convert.ToString(parentRow?.GetCell(c));
                if (string.IsNullOrEmpty(cellValue))
                {
                    continue;
                }
                ParentColumnNames.Add(cellValue);
            }

            for (int r = 1; r <= sheet.LastRowNum; r++)
            {
                var row = sheet.GetRow(r);
                string rowValue = Convert.ToString(row?.GetCell(0));
                if (string.IsNullOrEmpty(rowValue))
                {
                    continue;
                }
                for (int c = 1; c <= cellCount; c++)
                {
                    var cellValue = Convert.ToString(row?.GetCell(c));
                    if (string.IsNullOrEmpty(cellValue))
                    {
                        continue;
                    }
                    dynamic p = new ExpandoObject();
                    var dic = p as IDictionary<string, object>;
                    dic.Add(rowKey, rowValue);
                    dic.Add(columnKey, columnNames[c - 1]);
                    dic.Add(valKey, cellValue);
                    dic.Add(ParentModelNameKey, ParentColumnNames[c - 1]);
                    dynamicList.Add(p);
                }
            }
            return dynamicList;
        }
        /// <summary>
        /// NPOI 列转行
        /// </summary>
        /// <param name="sheet">sheet页签名</param>
        /// <param name="rowKey">行（比如：模型）</param>
        /// <param name="columnKey">列（比如：结构）</param>
        /// <param name="valKey">值（行列的交集）</param>
        ///  <param name="ParentColumnKey">父列</param>
        /// <returns></returns>
        public static List<dynamic> Column2RowWithColumnParent(ISheet sheet, string rowKey, string columnKey, string valKey, string ParentModelNameKey = "ParentModelName")
        {
            IRow headerRow = sheet.GetRow(0);
            int cellCount = headerRow.LastCellNum;
            List<dynamic> dynamicList = new List<dynamic>();
            List<string> columnNames = new List<string>();
            for (int c = 1; c <= cellCount; c++)
            {
                var cellValue = Convert.ToString(headerRow?.GetCell(c));
                if (string.IsNullOrEmpty(cellValue))
                {
                    continue;
                }
                columnNames.Add(cellValue);
            }

            for (int r = 1; r <= sheet.LastRowNum; r++)
            {
                var row = sheet.GetRow(r);
                string rowValue = Convert.ToString(row?.GetCell(1));
                if (string.IsNullOrEmpty(rowValue))
                {
                    continue;
                }
                var parentCellValue = Convert.ToString(row?.GetCell(0));

                for (int c = 2; c <= cellCount; c++)
                {
                    var cellValue = Convert.ToString(row?.GetCell(c));
                    if (string.IsNullOrEmpty(cellValue))
                    {
                        continue;
                    }
                    dynamic p = new ExpandoObject();
                    var dic = p as IDictionary<string, object>;
                    dic.Add(rowKey, rowValue);
                    dic.Add(columnKey, columnNames[c - 1]);
                    dic.Add(valKey, cellValue);
                    dic.Add(ParentModelNameKey, parentCellValue);
                    dynamicList.Add(p);
                }
            }
            return dynamicList;
        }

        /// <summary>
        /// NPOI 列转行
        /// </summary>
        /// <param name="sheet">sheet页签名</param>
        /// <param name="rowKey">行（比如：模型）</param>
        /// <param name="columnKey">列（比如：结构）</param>
        /// <param name="valKey">值（行列的交集）</param>
        ///  <param name="ParentColumnKey">父列</param>
        /// <returns></returns>
        public static List<dynamic> Column2RowWithMultipleAttr(ISheet sheet, List<string> attrKeys, string valKey)
        {
            IRow headerRow = sheet.GetRow(0);
            int cellCount = headerRow.LastCellNum;
            List<dynamic> dynamicList = new List<dynamic>();
            List<string> columnNames = new List<string>();
            for (int c = attrKeys.Count - 1; c < cellCount; c++)
            {
                var cellValue = Convert.ToString(headerRow?.GetCell(c));
                if (string.IsNullOrEmpty(cellValue))
                {
                    continue;
                }
                columnNames.Add(cellValue);
            }
            var tempAttrs = attrKeys.ToArray();
            for (int r = 1; r <= sheet.LastRowNum; r++)
            {
                var row = sheet.GetRow(r);
                var firstcellValue = Convert.ToString(row?.GetCell(2));
                if (string.IsNullOrEmpty(firstcellValue))
                {
                    continue;
                }

                for (int c = attrKeys.Count - 1; c < cellCount; c++)
                {
                    var cellValue = Convert.ToString(row?.GetCell(c));
                    dynamic p = new ExpandoObject();
                    var dic = p as IDictionary<string, object>;
                    (p, dic) = initRowAttrs(attrKeys, row);

                    if (string.IsNullOrEmpty(cellValue) && (c - attrKeys.Count + 1) < columnNames.Count)
                    {
                        dic.Add(tempAttrs[attrKeys.Count - 1], columnNames[c - attrKeys.Count + 1]);
                        continue;
                    }
                    if ((c - attrKeys.Count + 1) < columnNames.Count)
                    {
                        dic.Add(tempAttrs[attrKeys.Count - 1], columnNames[c - attrKeys.Count + 1]);
                        dic.Add(valKey, cellValue);
                        dynamicList.Add(p);
                    }
                }
            }
            return dynamicList;
        }

        protected static (ExpandoObject, IDictionary<string, object>) initRowAttrs(List<string> attrKeys, IRow row)
        {
            var tempAttrs = attrKeys.ToArray();
            dynamic p = new ExpandoObject();
            var dic = p as IDictionary<string, object>;
            for (int j = 0; j < attrKeys.Count - 1; j++)
            {
                var cellValue = Convert.ToString(row?.GetCell(j));
                if (string.IsNullOrEmpty(cellValue))
                {
                    continue;
                }
                dic.Add(tempAttrs[j], cellValue);
            }
            return (p, dic);
        }
    }
}
