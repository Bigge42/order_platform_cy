/**
 * 预警功能工具类 - 简化版
 * 后端已经处理预警判断逻辑并在数据中添加ShouldAlert字段
 * 前端只需根据ShouldAlert字段应用样式即可
 */

/**
 * 默认预警样式
 */
const DEFAULT_ALERT_STYLE = {
  backgroundColor: '#fffbe6',
  color: '#000'
}

/**
 * 应用预警样式到表格列
 * @param {Array} columns - 表格列配置
 * @param {Object} customStyle - 自定义预警样式(可选)
 */
export function applyAlertWarningStyle(columns, customStyle = null) {
  if (!columns || !Array.isArray(columns)) {
    console.warn('columns参数无效')
    return
  }

  const alertStyle = customStyle || DEFAULT_ALERT_STYLE

  /**
   * 预警单元格样式函数
   */
  const warningCellStyle = (row) => {
    if (row && row.ShouldAlert === true) {
      return alertStyle
    }
    return null
  }

  // 为每个列添加cellStyle
  columns.forEach((col) => {
    if (col.cellStyle) {
      // 如果已有cellStyle,则合并
      const originalCellStyle = col.cellStyle
      col.cellStyle = (row, rowIndex, columnIndex, tableData) => {
        const originalStyle = originalCellStyle(row, rowIndex, columnIndex, tableData)
        const warningStyle = warningCellStyle(row)
        
        // 预警样式优先级更高
        if (warningStyle) {
          return { ...originalStyle, ...warningStyle }
        }
        return originalStyle
      }
    } else {
      // 没有cellStyle,直接设置
      col.cellStyle = warningCellStyle
    }
  })

  console.log('预警样式已应用到表格列')
}

/**
 * 清除表格列的预警样式
 * @param {Array} columns - 表格列配置
 */
export function clearAlertWarningStyle(columns) {
  if (!columns || !Array.isArray(columns)) {
    return
  }

  columns.forEach((col) => {
    if (col.cellStyle) {
      delete col.cellStyle
    }
  })

  console.log('预警样式已清除')
}

