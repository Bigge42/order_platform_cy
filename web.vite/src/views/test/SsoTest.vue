<template>
  <div class="sso-test-container">
    <el-card class="test-card">
      <template #header>
        <div class="card-header">
          <h2>SSO单点登录功能测试</h2>
        </div>
      </template>
      
      <div class="test-section">
        <h3>1. 配置检查</h3>
        <el-descriptions :column="2" border>
          <el-descriptions-item label="SSO状态">
            <el-tag :type="ssoStatus.enabled ? 'success' : 'danger'">
              {{ ssoStatus.enabled ? '已启用' : '未启用' }}
            </el-tag>
          </el-descriptions-item>
          <el-descriptions-item label="浏览器支持">
            <el-tag :type="ssoStatus.browserSupported ? 'success' : 'warning'">
              {{ ssoStatus.browserSupported ? '支持' : '不支持' }}
            </el-tag>
          </el-descriptions-item>
          <el-descriptions-item label="当前URL">
            {{ currentUrl }}
          </el-descriptions-item>
          <el-descriptions-item label="是否SSO回调">
            <el-tag :type="ssoStatus.isCallback ? 'info' : 'default'">
              {{ ssoStatus.isCallback ? '是' : '否' }}
            </el-tag>
          </el-descriptions-item>
        </el-descriptions>
      </div>

      <div class="test-section">
        <h3>2. 功能测试</h3>
        <div class="test-buttons">
          <el-button 
            type="primary" 
            @click="testGetAuthUrl"
            :loading="loading.authUrl"
          >
            测试获取授权URL
          </el-button>
          
          <el-button 
            type="success" 
            @click="testSsoLogin"
            :loading="loading.ssoLogin"
          >
            测试SSO登录
          </el-button>
          
          <el-button 
            type="info" 
            @click="testUrlUtils"
          >
            测试URL工具
          </el-button>
          
          <el-button 
            type="warning" 
            @click="clearTestData"
          >
            清理测试数据
          </el-button>
        </div>
      </div>

      <div class="test-section" v-if="testResults.length > 0">
        <h3>3. 测试结果</h3>
        <div class="test-results">
          <div 
            v-for="(result, index) in testResults" 
            :key="index"
            class="test-result-item"
            :class="result.type"
          >
            <div class="result-header">
              <span class="result-title">{{ result.title }}</span>
              <span class="result-time">{{ result.time }}</span>
            </div>
            <div class="result-content">
              <pre>{{ result.content }}</pre>
            </div>
          </div>
        </div>
      </div>

      <div class="test-section">
        <h3>4. 使用说明</h3>
        <el-alert
          title="测试步骤"
          type="info"
          :closable="false"
        >
          <ol>
            <li>首先检查SSO配置是否正确</li>
            <li>点击"测试获取授权URL"验证后端接口</li>
            <li>点击"测试SSO登录"模拟完整登录流程</li>
            <li>查看测试结果中的详细信息</li>
          </ol>
        </el-alert>
      </div>
    </el-card>
  </div>
</template>

<script>
import { defineComponent, ref, reactive, onMounted, getCurrentInstance } from 'vue';
import { ssoApi } from '@/api/sso.js';
import { SSOUtils } from '@/utils/sso.js';

export default defineComponent({
  name: 'SsoTest',
  setup() {
    const appContext = getCurrentInstance().appContext;
    const $message = appContext.config.globalProperties.$message;

    const currentUrl = ref(window.location.href);
    const testResults = ref([]);
    
    const loading = reactive({
      authUrl: false,
      ssoLogin: false
    });

    const ssoStatus = reactive({
      enabled: true, // 假设已启用，实际可以通过API检查
      browserSupported: SSOUtils.isSupported(),
      isCallback: SSOUtils.isSSOCallback()
    });

    // 添加测试结果
    const addTestResult = (title, content, type = 'info') => {
      testResults.value.unshift({
        title,
        content: typeof content === 'object' ? JSON.stringify(content, null, 2) : content,
        type,
        time: new Date().toLocaleTimeString()
      });
    };

    // 测试获取授权URL
    const testGetAuthUrl = async () => {
      loading.authUrl = true;
      try {
        addTestResult('开始测试', '正在获取SSO授权URL...', 'info');
        
        const result = await ssoApi.getAuthUrl();
        
        if (result.status) {
          addTestResult('获取授权URL成功', {
            url: result.data,
            message: '授权URL获取成功，可以用于SSO登录跳转'
          }, 'success');
          $message.success('授权URL获取成功');
        } else {
          addTestResult('获取授权URL失败', {
            error: result.message,
            details: '请检查后端SSO配置'
          }, 'error');
          $message.error(result.message || '获取授权URL失败');
        }
      } catch (error) {
        addTestResult('获取授权URL异常', {
          error: error.message,
          stack: error.stack
        }, 'error');
        $message.error('请求异常：' + error.message);
      } finally {
        loading.authUrl = false;
      }
    };

    // 测试SSO登录
    const testSsoLogin = async () => {
      loading.ssoLogin = true;
      try {
        // 模拟授权码
        const mockCode = 'test_authorization_code_' + Date.now();
        
        addTestResult('开始测试', `正在测试SSO登录，使用模拟授权码: ${mockCode}`, 'info');
        
        const result = await ssoApi.ssoLogin(mockCode);
        
        if (result.status) {
          addTestResult('SSO登录测试成功', {
            data: result.data,
            message: '注意：这是使用模拟授权码的测试，实际使用时需要真实的授权码'
          }, 'success');
          $message.success('SSO登录测试成功（模拟）');
        } else {
          addTestResult('SSO登录测试失败', {
            error: result.message,
            details: '这是预期的结果，因为使用了模拟授权码'
          }, 'warning');
          $message.warning('SSO登录测试失败（预期结果）');
        }
      } catch (error) {
        addTestResult('SSO登录测试异常', {
          error: error.message,
          details: '这可能是预期的结果，因为使用了模拟授权码'
        }, 'warning');
        $message.warning('SSO登录测试异常（可能是预期结果）');
      } finally {
        loading.ssoLogin = false;
      }
    };

    // 测试URL工具
    const testUrlUtils = () => {
      const testData = {
        '当前URL': window.location.href,
        '是否SSO回调': SSOUtils.isSSOCallback(),
        '提取的授权码': SSOUtils.getCodeFromUrl(),
        '浏览器支持': SSOUtils.isSupported(),
        '生成的状态参数': SSOUtils.generateState()
      };

      addTestResult('URL工具测试', testData, 'info');
      $message.info('URL工具测试完成，请查看结果');
    };

    // 清理测试数据
    const clearTestData = () => {
      testResults.value = [];
      localStorage.removeItem('sso_state');
      SSOUtils.cleanSSOParams();
      $message.success('测试数据已清理');
    };

    onMounted(() => {
      addTestResult('页面初始化', {
        message: 'SSO测试页面已加载',
        timestamp: new Date().toISOString(),
        userAgent: navigator.userAgent
      }, 'info');
    });

    return {
      currentUrl,
      testResults,
      loading,
      ssoStatus,
      testGetAuthUrl,
      testSsoLogin,
      testUrlUtils,
      clearTestData
    };
  }
});
</script>

<style lang="less" scoped>
.sso-test-container {
  padding: 20px;
  max-width: 1200px;
  margin: 0 auto;
}

.test-card {
  .card-header {
    text-align: center;
    
    h2 {
      margin: 0;
      color: #409eff;
    }
  }
}

.test-section {
  margin-bottom: 30px;
  
  h3 {
    color: #303133;
    border-bottom: 2px solid #e4e7ed;
    padding-bottom: 10px;
    margin-bottom: 20px;
  }
}

.test-buttons {
  display: flex;
  gap: 15px;
  flex-wrap: wrap;
  
  .el-button {
    min-width: 150px;
  }
}

.test-results {
  max-height: 500px;
  overflow-y: auto;
  border: 1px solid #e4e7ed;
  border-radius: 4px;
}

.test-result-item {
  padding: 15px;
  border-bottom: 1px solid #f0f0f0;
  
  &:last-child {
    border-bottom: none;
  }
  
  &.success {
    background-color: #f0f9ff;
    border-left: 4px solid #67c23a;
  }
  
  &.error {
    background-color: #fef0f0;
    border-left: 4px solid #f56c6c;
  }
  
  &.warning {
    background-color: #fdf6ec;
    border-left: 4px solid #e6a23c;
  }
  
  &.info {
    background-color: #f4f4f5;
    border-left: 4px solid #909399;
  }
}

.result-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 10px;
  
  .result-title {
    font-weight: bold;
    color: #303133;
  }
  
  .result-time {
    font-size: 12px;
    color: #909399;
  }
}

.result-content {
  pre {
    background-color: #f8f8f8;
    padding: 10px;
    border-radius: 4px;
    font-size: 12px;
    line-height: 1.4;
    margin: 0;
    white-space: pre-wrap;
    word-break: break-all;
  }
}

@media (max-width: 768px) {
  .test-buttons {
    flex-direction: column;
    
    .el-button {
      width: 100%;
    }
  }
}
</style> 