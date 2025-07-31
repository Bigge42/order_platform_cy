import { ref } from 'vue'

export function useFormValidation() {
  const batchFormRef = ref()



  // 表单验证
  const validateBatchForm = async () => {
    try {
      if (!batchFormRef.value) {
        throw new Error('表单引用不存在')
      }
      await batchFormRef.value.validate()
      return true
    } catch (error) {
      console.error('表单验证失败:', error)
      return false
    }
  }

  // 清除表单验证状态
  const clearFormValidation = () => {
    if (batchFormRef.value) {
      batchFormRef.value.clearValidate()
    }
  }

  // 清除指定字段的验证状态
  const clearFieldValidation = (index, fieldName) => {
    if (batchFormRef.value) {
      const fieldProp = `tableData.${index}.${fieldName}`
      batchFormRef.value.clearValidate(fieldProp)
    }
  }



  return {
    batchFormRef,
    validateBatchForm,
    clearFormValidation,
    clearFieldValidation
  }
} 