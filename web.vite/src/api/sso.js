import http from './http'

/**
 * SSO单点登录相关API
 */
export const ssoApi = {
  /**
   * 获取SSO授权URL
   * @returns {Promise} 返回授权URL
   */
  getAuthUrl() {
    return http.get('/api/sso/auth-url', {}, '获取单点登录地址')
  },

  /**
   * SSO登录回调处理
   * @param {string} code 授权码
   * @returns {Promise} 返回登录结果
   */
  ssoLogin(code) {
    return http.post('/api/sso/ssologin', { code }, '正在登录中')
  }
}

export default ssoApi 