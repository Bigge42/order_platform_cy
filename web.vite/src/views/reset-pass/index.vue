<template>
  <vol-box
    v-model="visible"
    title="忘记密码"
    :width="450"
    icon="el-icon-lock"
    :on-model-close="handleClose"
    :show-full="false"
  >
    <template #content>
      <el-steps :active="currentStep" finish-status="success" align-center>
        <el-step title="身份认证"></el-step>
        <el-step title="重置密码"></el-step>
      </el-steps>

      <div style="margin-top: 30px">
        <!-- 第一步：身份认证 -->
        <div v-show="currentStep === 0">
          <el-form :model="forgotForm" label-width="80px">
            <el-form-item label="邮箱">
              <el-input
                v-model="forgotForm.email"
                placeholder="请输入邮箱"
                type="email"
                @blur="handleEmailBlur"
                :class="{ 'email-error': emailError }"
              ></el-input>
              <div v-if="emailError" class="error-message">{{ emailError }}</div>
            </el-form-item>
            <el-form-item label="验证码">
              <el-input v-model="forgotForm.verifyCode" placeholder="请输入验证码">
                <template #append>
                  <el-button @click="sendVerifyCode" :disabled="sendCodeDisabled">
                    {{ sendCodeText }}
                  </el-button>
                </template>
              </el-input>
            </el-form-item>
          </el-form>
        </div>

        <!-- 第二步：重置密码 -->
        <div v-show="currentStep === 1">
          <el-form :model="forgotForm" label-width="80px">
            <el-form-item label="新密码">
              <el-input
                v-model="forgotForm.newPassword"
                type="password"
                placeholder="请输入新密码"
              ></el-input>
            </el-form-item>
            <el-form-item label="确认密码">
              <el-input
                v-model="forgotForm.confirmPassword"
                type="password"
                placeholder="请确认新密码"
              ></el-input>
            </el-form-item>
          </el-form>
        </div>
      </div>
    </template>

    <template #footer>
      <el-button v-if="currentStep === 1" @click="handlePrevStep">返回上一步</el-button>
      <el-button @click="handleClose">取消</el-button>
      <el-button type="primary" @click="handleNext">
        {{ currentStep === 0 ? '下一步' : '确认' }}
      </el-button>
    </template>
  </vol-box>
</template>

<script>
import { defineComponent, ref, reactive, getCurrentInstance } from 'vue'
import VolBox from '@/components/basic/VolBox.vue'

export default defineComponent({
  name: 'ResetPassword',
  components: {
    VolBox
  },
  props: {
    modelValue: {
      type: Boolean,
      default: false
    }
  },
  emits: ['update:modelValue'],
  setup(props, { emit }) {
    const visible = ref(props.modelValue)
    const currentStep = ref(0)
    const sendCodeDisabled = ref(false)
    const sendCodeText = ref('发送验证码')
    const emailError = ref('')

    const forgotForm = reactive({
      email: '',
      verifyCode: '',
      newPassword: '',
      confirmPassword: ''
    })

    let appContext = getCurrentInstance().appContext
    let $message = appContext.config.globalProperties.$message
    let $ts = appContext.config.globalProperties.$ts

    // 邮箱格式校验
    const validateEmail = (email) => {
      const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/
      return emailRegex.test(email)
    }

    // 处理邮箱失焦事件
    const handleEmailBlur = () => {
      if (forgotForm.email && !validateEmail(forgotForm.email)) {
        emailError.value = $ts('请输入正确的邮箱格式')
      } else {
        emailError.value = ''
      }
    }

    // 监听props变化
    const updateVisible = (val) => {
      visible.value = val
    }

    // 发送验证码
    const sendVerifyCode = () => {
      if (!forgotForm.email) {
        $message.error($ts('请输入邮箱'))
        return
      }
      if (!validateEmail(forgotForm.email)) {
        $message.error($ts('请输入正确的邮箱格式'))
        return
      }
      sendCodeDisabled.value = true
      let countdown = 60
      sendCodeText.value = `${countdown}s`
      const timer = setInterval(() => {
        countdown--
        if (countdown > 0) {
          sendCodeText.value = `${countdown}s`
        } else {
          clearInterval(timer)
          sendCodeDisabled.value = false
          sendCodeText.value = '发送验证码'
        }
      }, 1000)
    }

    // 处理下一步
    const handleNext = () => {
      if (currentStep.value === 0) {
        if (!forgotForm.email) {
          $message.error($ts('请输入邮箱'))
          return
        }
        if (!validateEmail(forgotForm.email)) {
          $message.error($ts('请输入正确的邮箱格式'))
          return
        }
        if (!forgotForm.verifyCode) {
          $message.error($ts('请输入验证码'))
          return
        }
        // 切换到第二步时清空验证码，保留邮箱
        forgotForm.verifyCode = ''
        // 确保密码字段为空
        forgotForm.newPassword = ''
        forgotForm.confirmPassword = ''
        currentStep.value = 1
      } else {
        if (!forgotForm.newPassword) {
          $message.error($ts('请输入新密码'))
          return
        }
        if (!forgotForm.confirmPassword) {
          $message.error($ts('请确认新密码'))
          return
        }
        if (forgotForm.newPassword !== forgotForm.confirmPassword) {
          $message.error($ts('两次密码输入不一致'))
          return
        }
        $message.success($ts('密码重置成功'))
        handleClose()
      }
    }

    // 返回上一步
    const handlePrevStep = () => {
      // 清空密码字段
      forgotForm.newPassword = ''
      forgotForm.confirmPassword = ''
      currentStep.value = 0
    }

    // 关闭弹窗
    const handleClose = () => {
      // 重置所有数据
      currentStep.value = 0
      Object.assign(forgotForm, {
        email: '',
        verifyCode: '',
        newPassword: '',
        confirmPassword: ''
      })
      sendCodeDisabled.value = false
      sendCodeText.value = '发送验证码'

      visible.value = false
      emit('update:modelValue', false)
      return true
    }

    // 监听外部传入的visible变化
    const { modelValue } = props
    if (modelValue !== visible.value) {
      visible.value = modelValue
    }

    return {
      visible,
      currentStep,
      sendCodeDisabled,
      sendCodeText,
      forgotForm,
      sendVerifyCode,
      handleNext,
      handlePrevStep,
      handleClose,
      updateVisible,
      emailError,
      handleEmailBlur
    }
  },
  watch: {
    modelValue(val) {
      this.visible = val
    },
    visible(val) {
      this.$emit('update:modelValue', val)
    }
  }
})
</script>

<style lang="less" scoped>
::v-deep(.el-steps) {
  .el-step__head.is-process {
    color: var(--el-color-primary);
    border-color: var(--el-color-primary);
  }

  .el-step__head.is-process .el-step__icon {
    background-color: var(--el-color-primary);
    border-color: var(--el-color-primary);
    color: #fff;
  }

  .el-step__title.is-process {
    color: var(--el-color-primary);
    font-weight: 600;
  }

  .el-step__head.is-finish {
    color: var(--el-color-primary);
    border-color: var(--el-color-primary);
  }

  .el-step__head.is-finish .el-step__icon {
    background-color: var(--el-color-primary);
    border-color: var(--el-color-primary);
    color: #fff;
  }

  .el-step__title.is-finish {
    color: var(--el-color-primary);
  }
}

.email-error ::v-deep(.el-input__wrapper) {
  border-color: var(--el-color-danger) !important;
  box-shadow: 0 0 0 1px var(--el-color-danger) inset !important;
}

.error-message {
  color: var(--el-color-danger);
  font-size: 12px;
  margin-top: 4px;
  line-height: 1;
}
</style>
