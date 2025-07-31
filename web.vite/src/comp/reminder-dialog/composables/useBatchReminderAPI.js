import { ref, reactive, getCurrentInstance } from 'vue'
import { ElMessage } from 'element-plus'


export function useBatchReminderAPI() {
  const { proxy } = getCurrentInstance()

  // API状态管理
  const apiState = reactive({
    submitting: false,
    error: null,
    result: null
  })

  // 业务类型选项
  const businessTypeOptions = ref([])
  const businessTypeLoading = ref(false)

  // 默认负责人状态
  const defaultResponsibleState = reactive({
    loading: false,
    error: null,
    completed: false,
    abortController: null
  })

  // 重置API状态
  const resetAPIState = (scope = 'all') => {
    const resetters = {
      api: () => Object.assign(apiState, { submitting: false, error: null, result: null }),
      businessType: () => { businessTypeOptions.value = []; businessTypeLoading.value = false },
      defaultResponsible: () => Object.assign(defaultResponsibleState, { loading: false, error: null, completed: false }),
      all: () => {
        resetters.api()
        resetters.businessType()
        resetters.defaultResponsible()
      }
    }
    resetters[scope]?.()
  }

  // 获取业务类型字典
  const fetchBusinessTypeOptions = async (businessType) => {
    if (!businessType) return

    businessTypeLoading.value = true
    try {
      const params = {
        page: 1,
        rows: 30,
        sort: 'OrderNo,CreateDate',
        order: 'desc',
        wheres: '[]',
        value: 109,
        tableName: null,
        isCopyClick: false
      }

      const result = await proxy.http.post('api/Sys_Dictionary/getDetailPage', params, false)
      
      if (result?.rows) {
        businessTypeOptions.value = result.rows
          .filter(item => item.Enable === 1)
          .map(item => ({ value: item.DicValue, label: item.DicName }))
      }
    } catch (error) {
      console.error('获取业务类型字典失败:', error)
      businessTypeOptions.value = []
    } finally {
      businessTypeLoading.value = false
    }
  }

  // 获取默认负责人
  const fetchDefaultResponsible = async (data, businessType) => {
    if (!data?.length) return

    defaultResponsibleState.loading = true
    try {
      // 直接从传入数据中取businessKey
      const businessItems = data
        .map(row => {
          const businessKey = String(row.businessKey || '')
          const item = {
            businessType: String(businessType || ''),
            businessKey
          }
          
          // 如果businessType是JS，则添加materialCode字段
          if (String(businessType).toUpperCase() === 'JS') {
            item.materialCode = String(row.materialCode || '')
          }
          
          return item
        })
        .filter(item => item.businessKey) // 过滤掉没有businessKey的项

      if (!businessItems.length) {
        console.log('没有有效的businessKey，跳过默认负责人获取')
        return
      }

      const response = await proxy.http.post(
        '/api/OCP_UrgentOrder/BatchGetDefaultResponsible',
        { businessItems },
        false
      )

      // 检查响应状态和数据结构
      if (!response) {
        console.warn('默认负责人API响应为空')
        return
      }

      if (!response.status) {
        console.warn('默认负责人API调用失败:', response.message || '未知错误')
        return
      }

      if (!response.data?.responsibleInfos || !Array.isArray(response.data.responsibleInfos)) {
        console.warn('默认负责人API响应数据格式不正确:', response.data)
        return
      }

      // 创建businessKey到负责人信息的映射
      const responsibleMap = new Map(
        response.data.responsibleInfos.map(info => [info.businessKey, info])
      )
      
      console.log('创建负责人映射:', {
        总数: response.data.responsibleInfos.length,
        找到的: response.data.responsibleInfos.filter(info => info.found).length,
        映射键: Array.from(responsibleMap.keys())
      })
      
      return responsibleMap
    } catch (error) {
      console.error('获取默认负责人失败:', error)
      defaultResponsibleState.error = error.message
    } finally {
      defaultResponsibleState.loading = false
    }
  }

  // 数据转换
  const transformDataForAPI = (tableData, originalData, businessType) => {
    return tableData.map((row, index) => {
      const originalRow = row._originalData || originalData[index] || {}
      
      return {
        urgentType: row.urgentType || 0,
        businessType: businessType || '',
        businessKey: originalRow.businessKey,
        billNo: originalRow.billNo,
        seq: originalRow.seq,
        urgencyLevel: row.urgencyLevel,
        defaultResPerson: row.defaultResPerson,
        defaultResPersonName: row.defaultResPersonName,
        assignedReplyTime: row.assignedReplyTime,
        timeUnit: row.timeUnit,
        urgentContent: row.urgentContent,
        assignedResPerson: row.assignedResPerson,
        assignedResPersonName: row.assignedResPersonName
      }
    })
  }







  // 调用批量催单接口
  const callBatchReminderAPI = async (apiData) => {
    try {
      const response = await proxy.http.post('/api/OCP_UrgentOrder/BatchAdd', apiData, false)
      return response
    } catch (error) {
      console.error('批量催单接口调用失败:', error)
      throw error
    }
  }



  // 解析API响应
  const parseAPIResponse = (response) => {
    try {
      if (!response) throw new Error('接口响应为空')

      const { status, message, data } = response
      
      return {
        success: Boolean(status),
        message: message || '',
        successCount: data?.successCount || 0,
        failedCount: data?.failedCount || 0,
        failedItems: data?.failedItems || []
      }
    } catch (error) {
      console.error('解析接口响应失败:', error)
      return {
        success: false,
        message: '响应数据解析失败',
        successCount: 0,
        failedCount: 0,
        failedItems: []
      }
    }
  }

  // 处理API错误
  const handleAPIError = (error) => {
    let errorMessage = '操作失败'
    
    if (error.response) {
      errorMessage = error.response.data?.message || '服务器错误'
    } else if (error.request) {
      errorMessage = '网络连接失败，请检查网络设置'
    } else {
      errorMessage = error.message || '未知错误'
    }

    ElMessage.error(errorMessage)
    apiState.error = errorMessage
    return errorMessage
  }

  // 显示成功消息
  const showSuccessMessage = (result) => {
    const message = `批量添加完成，成功 ${result.successCount} 条${result.failedCount > 0 ? `，失败 ${result.failedCount} 条` : ''}`
    ElMessage.success(message)
    
    if (result.failedCount > 0) {
      setTimeout(() => {
        ElMessage.warning(`有 ${result.failedCount} 条记录处理失败，请查看控制台了解详情`)
      }, 1000)
    }
  }

  // 执行批量催单
  const executeBatchReminder = async (tableData, originalData, businessType) => {
    try {
      apiState.submitting = true
      apiState.error = null

      const apiData = transformDataForAPI(tableData, originalData, businessType)
      const response = await callBatchReminderAPI(apiData)
      const result = parseAPIResponse(response)
      
      apiState.result = result

      if (result.success) {
        showSuccessMessage(result)
        return result
      } else {
        throw new Error(result.message || '批量催单失败')
      }
    } catch (error) {
      handleAPIError(error)
      throw error
    } finally {
      apiState.submitting = false
    }
  }

  return {
    // 状态
    apiState,
    businessTypeOptions,
    businessTypeLoading,
    defaultResponsibleState,
    
    // 方法
    resetAPIState,
    fetchBusinessTypeOptions,
    fetchDefaultResponsible,
    executeBatchReminder,
    transformDataForAPI
  }
} 