<template>
  <div class="sso-login-container">
    <div class="loading-wrapper">
      <div class="loading-text">{{ $ts('正在登录中,请稍候...') }}</div>
      <div class="loading-spinner"></div>
    </div>
  </div>
</template>

<script>
import { defineComponent, onMounted, getCurrentInstance } from 'vue';
import { useRouter, useRoute } from 'vue-router';
import { ssoApi } from '@/api/sso.js';
import { SSOUtils } from '@/utils/sso.js';
import store from '../store/index';

export default defineComponent({
  name: 'SsoLogin',
  setup() {
    const router = useRouter();
    const route = useRoute();
    const appContext = getCurrentInstance().appContext;
    const $message = appContext.config.globalProperties.$message;
    const $ts = appContext.config.globalProperties.$ts;

    const handleSsoLogin = async () => {
      try {
        // 检查浏览器支持
        if (!SSOUtils.isSupported()) {
          $message.error($ts('当前浏览器不支持单点登录功能'));
          router.push('/login');
          return;
        }

        // 获取授权码
        const code = SSOUtils.getCodeFromUrl();
        
        if (!code) {
          $message.error($ts('单点登录失败，缺少授权码'));
          setTimeout(() => {
            router.push('/login');
          }, 1500);
          return;
        }

        // 验证状态参数（如果有的话）
        const state = route.query.state || new URLSearchParams(window.location.search).get('state');
        if (state && !SSOUtils.validateState(state)) {
          $message.error($ts('单点登录验证失败，可能存在安全风险'));
          router.push('/login');
          return;
        }

        // 清理URL中的SSO参数
        SSOUtils.cleanSSOParams();

        // 调用SSO API完成登录
        const result = await ssoApi.ssoLogin(code);
        
        if (!result.status) {
          $message.error(result.message || $ts('单点登录失败'));
          setTimeout(() => {
            router.push('/login');
          }, 1500);
          return;
        }

        // 登录成功，保存用户信息并跳转
        $message.success($ts('登录成功,正在跳转!'));
        store.commit('setUserInfo', result.data);
        
        // 跳转到首页或指定页面
        const redirectPath = route.query.redirect || '/';
        router.push({ path: redirectPath });
        
      } catch (error) {
        SSOUtils.handleError(error, (message) => {
          $message.error($ts(message));
        });
        setTimeout(() => {
          router.push('/login');
        }, 1500);
      }
    }

    onMounted(() => {
      handleSsoLogin();
    });

    return {};
  }
});
</script>

<style lang="less" scoped>
.sso-login-container {
  display: flex;
  justify-content: center;
  align-items: center;
  height: 100vh;
  background: linear-gradient(135deg, #1a3a63, #2a4c7a);
}

.loading-wrapper {
  display: flex;
  flex-direction: column;
  align-items: center;
  padding: 30px;
  background-color: white;
  border-radius: 12px;
  box-shadow: 0 8px 24px rgba(0, 0, 0, 0.12);
}

.loading-text {
  color: #2a4c7a;
  font-size: 18px;
  margin-bottom: 20px;
}

.loading-spinner {
  width: 50px;
  height: 50px;
  border: 5px solid #f3f3f3;
  border-top: 5px solid #3498db;
  border-radius: 50%;
  animation: spin 1s linear infinite;
}

@keyframes spin {
  0% { transform: rotate(0deg); }
  100% { transform: rotate(360deg); }
}
</style> 