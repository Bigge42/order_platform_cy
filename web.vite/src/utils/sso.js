/**
 * SSO单点登录工具类
 */
export class SSOUtils {
  /**
   * 检查当前URL是否包含SSO回调参数
   * @returns {boolean} 是否为SSO回调
   */
  static isSSOCallback() {
    const urlParams = new URLSearchParams(window.location.search);
    const hashParams = new URLSearchParams(window.location.hash.split('?')[1]);
    return urlParams.has('code') || hashParams.has('code');
  }

  /**
   * 从URL中提取授权码
   * @returns {string|null} 授权码
   */
  static getCodeFromUrl() {
    const urlParams = new URLSearchParams(window.location.search);
    const hashParams = new URLSearchParams(window.location.hash.split('?')[1]);
    return urlParams.get('code') || hashParams.get('code');
  }

  /**
   * 清理URL中的SSO参数
   */
  static cleanSSOParams() {
    const url = new URL(window.location);
    url.searchParams.delete('code');
    url.searchParams.delete('state');
    
    // 清理hash中的参数
    if (url.hash.includes('?')) {
      const [hashPath, hashQuery] = url.hash.split('?');
      const hashParams = new URLSearchParams(hashQuery);
      hashParams.delete('code');
      hashParams.delete('state');
      
      const newHashQuery = hashParams.toString();
      url.hash = newHashQuery ? `${hashPath}?${newHashQuery}` : hashPath;
    }
    
    window.history.replaceState({}, document.title, url.toString());
  }

  /**
   * 生成SSO状态参数（用于防止CSRF攻击）
   * @returns {string} 状态参数
   */
  static generateState() {
    return Math.random().toString(36).substring(2, 15) + 
           Math.random().toString(36).substring(2, 15);
  }

  /**
   * 保存SSO状态到本地存储
   * @param {string} state 状态参数
   */
  static saveState(state) {
    localStorage.setItem('sso_state', state);
  }

  /**
   * 验证SSO状态参数
   * @param {string} state 接收到的状态参数
   * @returns {boolean} 是否验证通过
   */
  static validateState(state) {
    const savedState = localStorage.getItem('sso_state');
    localStorage.removeItem('sso_state');
    return savedState === state;
  }

  /**
   * 构建SSO登录URL（如果需要前端构建的话）
   * @param {Object} config SSO配置
   * @param {string} config.serverUrl SSO服务器地址
   * @param {string} config.clientId 客户端ID
   * @param {string} config.redirectUri 回调地址
   * @param {string} config.authorizeEndpoint 授权端点
   * @returns {string} SSO登录URL
   */
  static buildAuthUrl(config) {
    const state = this.generateState();
    this.saveState(state);
    
    const params = new URLSearchParams({
      client_id: config.clientId,
      response_type: 'code',
      redirect_uri: config.redirectUri,
      state: state
    });
    
    return `${config.serverUrl.replace(/\/$/, '')}${config.authorizeEndpoint}?${params.toString()}`;
  }

  /**
   * 检查是否支持SSO登录
   * @returns {boolean} 是否支持
   */
  static isSupported() {
    // 检查浏览器是否支持必要的API
    return !!(window.URLSearchParams && window.localStorage && window.history);
  }

  /**
   * 处理SSO登录错误
   * @param {Error} error 错误对象
   * @param {Function} messageHandler 消息处理函数
   */
  static handleError(error, messageHandler) {
    console.error('SSO登录错误:', error);
    
    let errorMessage = '单点登录失败';
    
    if (error.message) {
      if (error.message.includes('network') || error.message.includes('fetch')) {
        errorMessage = '网络连接失败，请检查网络设置';
      } else if (error.message.includes('timeout')) {
        errorMessage = '请求超时，请重试';
      } else if (error.message.includes('unauthorized')) {
        errorMessage = '认证失败，请重新登录';
      } else {
        errorMessage = `登录失败: ${error.message}`;
      }
    }
    
    if (messageHandler && typeof messageHandler === 'function') {
      messageHandler(errorMessage);
    }
  }
}

export default SSOUtils 