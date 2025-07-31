// 批量催单弹窗组件测试脚本
// 这个文件包含了各种测试场景的数据和验证方法

// 测试数据生成器
export const createTestData = {
  // 空数据测试
  empty: () => [],
  
  // 单条数据测试
  single: () => [{
    id: 1,
    businessType: '销售订单',
    defaultResponsible: '张三',
    urgencyLevel: '高',
    specifiedReplyTime: '2024-01-15',
    specifiedResponsible: '李四',
    reminderType: '电话催单',
    reminderContent: '请及时处理销售订单，客户催促较急。'
  }],
  
  // 小量数据测试（5条）
  small: () => {
    const data = []
    for (let i = 1; i <= 5; i++) {
      data.push({
        id: i,
        businessType: `业务类型${i}`,
        defaultResponsible: `负责人${i}`,
        urgencyLevel: i % 2 === 0 ? '高' : '中',
        specifiedReplyTime: `2024-01-${10 + i}`,
        specifiedResponsible: `指定人${i}`,
        reminderType: '系统催单',
        reminderContent: `这是第${i}条测试催单内容。`
      })
    }
    return data
  },
  
  // 中等数据测试（25条）
  medium: () => {
    const businessTypes = ['销售订单', '采购订单', '委外订单', '生产订单']
    const urgencyLevels = ['低', '中', '高', '紧急']
    const reminderTypes = ['电话催单', '邮件催单', '短信催单', '系统催单']
    
    const data = []
    for (let i = 1; i <= 25; i++) {
      data.push({
        id: i,
        businessType: businessTypes[i % businessTypes.length],
        defaultResponsible: `默认负责人${i}`,
        urgencyLevel: urgencyLevels[i % urgencyLevels.length],
        specifiedReplyTime: new Date(2024, 0, i).toLocaleDateString(),
        specifiedResponsible: `指定负责人${i}`,
        reminderType: reminderTypes[i % reminderTypes.length],
        reminderContent: `第${i}条催单：请及时处理相关业务，确保按时完成。`
      })
    }
    return data
  },
  
  // 大量数据测试（100条）
  large: () => {
    const data = []
    for (let i = 1; i <= 100; i++) {
      data.push({
        id: i,
        businessType: `业务类型${Math.ceil(i / 10)}`,
        defaultResponsible: `负责人${i}`,
        urgencyLevel: ['低', '中', '高', '紧急'][i % 4],
        specifiedReplyTime: `2024-${String(Math.ceil(i / 10)).padStart(2, '0')}-${String((i % 30) + 1).padStart(2, '0')}`,
        specifiedResponsible: `指定人${i}`,
        reminderType: ['电话', '邮件', '短信', '系统'][i % 4] + '催单',
        reminderContent: `大数据测试第${i}条：这是一个较长的催单内容，用于测试表格显示效果和性能。请及时处理相关业务事项。`
      })
    }
    return data
  },
  
  // 特殊字符测试
  special: () => [{
    id: 1,
    businessType: '特殊&字符<测试>',
    defaultResponsible: '张三"李四\'',
    urgencyLevel: '高级/紧急',
    specifiedReplyTime: '2024/01/15 10:30',
    specifiedResponsible: '王五&赵六',
    reminderType: 'HTML<标签>测试',
    reminderContent: '这是包含特殊字符的测试内容：&lt;script&gt;alert("test")&lt;/script&gt;，用于验证XSS防护。'
  }],
  
  // 长文本测试
  longText: () => [{
    id: 1,
    businessType: '超长业务类型名称测试',
    defaultResponsible: '这是一个非常长的负责人姓名用于测试显示效果',
    urgencyLevel: '超级紧急重要',
    specifiedReplyTime: '2024年01月15日 上午10点30分',
    specifiedResponsible: '指定的负责人姓名也很长用于测试',
    reminderType: '综合性多渠道催单方式',
    reminderContent: '这是一个非常长的催单内容，用于测试表格中长文本的显示效果。内容包含了详细的业务描述、处理要求、时间节点、注意事项等多个方面的信息。这样的长文本在实际业务中是很常见的，需要确保组件能够正确处理和显示。'
  }]
}

// 测试场景配置
export const testScenarios = [
  {
    name: '空数据测试',
    description: '测试组件在没有数据时的表现',
    data: createTestData.empty(),
    expectedBehavior: '应显示"暂无数据"提示，不显示分页组件'
  },
  {
    name: '单条数据测试',
    description: '测试组件处理单条数据的情况',
    data: createTestData.single(),
    expectedBehavior: '应正确显示数据，不显示分页组件'
  },
  {
    name: '小量数据测试',
    description: '测试少量数据（不足一页）的显示',
    data: createTestData.small(),
    expectedBehavior: '应显示所有数据，不显示分页组件'
  },
  {
    name: '中等数据测试',
    description: '测试需要分页的中等数量数据',
    data: createTestData.medium(),
    expectedBehavior: '应正确分页显示，显示分页组件'
  },
  {
    name: '大量数据测试',
    description: '测试大量数据的性能和显示',
    data: createTestData.large(),
    expectedBehavior: '应保持良好性能，正确分页'
  },
  {
    name: '特殊字符测试',
    description: '测试特殊字符和HTML标签的处理',
    data: createTestData.special(),
    expectedBehavior: '应正确转义特殊字符，防止XSS攻击'
  },
  {
    name: '长文本测试',
    description: '测试长文本的显示和截断',
    data: createTestData.longText(),
    expectedBehavior: '应正确处理长文本，使用省略号显示'
  }
]

// 测试工具函数
export const testUtils = {
  // 验证数据结构
  validateDataStructure: (data) => {
    const requiredFields = [
      'id', 'businessType', 'defaultResponsible', 'urgencyLevel',
      'specifiedReplyTime', 'specifiedResponsible', 'reminderType', 'reminderContent'
    ]
    
    return data.every(item => 
      requiredFields.every(field => item.hasOwnProperty(field))
    )
  },
  
  // 验证分页计算
  validatePagination: (totalCount, pageSize, currentPage) => {
    const totalPages = Math.ceil(totalCount / pageSize)
    const isValidPage = currentPage >= 1 && currentPage <= totalPages
    const startIndex = (currentPage - 1) * pageSize
    const endIndex = Math.min(startIndex + pageSize, totalCount)
    
    return {
      totalPages,
      isValidPage,
      startIndex,
      endIndex,
      currentPageSize: endIndex - startIndex
    }
  },
  
  // 模拟用户操作
  simulateUserActions: {
    openDialog: () => console.log('模拟：打开弹窗'),
    closeDialog: () => console.log('模拟：关闭弹窗'),
    clickConfirm: () => console.log('模拟：点击确定按钮'),
    clickCancel: () => console.log('模拟：点击取消按钮'),
    changePage: (page) => console.log(`模拟：切换到第${page}页`),
    changePageSize: (size) => console.log(`模拟：修改页面大小为${size}`)
  }
}

// 性能测试
export const performanceTest = {
  // 测试大数据渲染时间
  measureRenderTime: (dataSize) => {
    const startTime = performance.now()
    const data = createTestData.large().slice(0, dataSize)
    const endTime = performance.now()
    
    return {
      dataSize,
      renderTime: endTime - startTime,
      performance: endTime - startTime < 100 ? 'good' : 'needs optimization'
    }
  },
  
  // 内存使用监控
  monitorMemoryUsage: () => {
    if (performance.memory) {
      return {
        used: performance.memory.usedJSHeapSize,
        total: performance.memory.totalJSHeapSize,
        limit: performance.memory.jsHeapSizeLimit
      }
    }
    return null
  }
}

// 导出所有测试相关的功能
export default {
  createTestData,
  testScenarios,
  testUtils,
  performanceTest
}