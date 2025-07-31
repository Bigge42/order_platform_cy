import * as signalR from '@microsoft/signalr';
import { ElNotification } from 'element-plus';
import { ElMessageBox } from 'element-plus';
import store from '@/store/index';
export default function(http, receive) {
  let connection;
  http.post('api/user/GetCurrentUserInfo').then((result) => {
    // 确保store中有loginName字段
    let user = store.getters.getUserInfo();
    if (user && !user.loginName) {
      user.loginName = result.data.userName;  // 设置真正的登录账号
      store.commit("setUserInfo", user);
    }
    
    connection = new signalR.HubConnectionBuilder()
      .withAutomaticReconnect()
      .withUrl(`${http.ipAddress}hub/message?userName=${result.data.userName}`,{
        //withCredentials: true // 启用凭证模式
       // accessTokenFactory: () => store.getters.getToken()
    })
      //.withUrl(`${http.ipAddress}message`)
      .build();

    connection.start().catch((err) => console.log(err.message));
    //自动重连成功后的处理
    connection.onreconnected((connectionId) => {
      console.log(connectionId);
    });
    connection.on('ReceiveHomePageMessage', function(data) {
      switch (data.code) {
        case '-1':
          showLogoutMessage(data);
          return;
        default:
          ElNotification.success({
            title:'消息' ,
            message: data.title,
            type: 'warning'
          });
          store.getters.data().pushMessage(data)
          receive && receive(data);
          break;
      }
    });
  });
}
//强制用户下线
function showLogoutMessage(data) {
  store.commit('clearUserInfo', '');
  const timerId = setTimeout(() => {
    clearTimeout(timerId);
    window.location.href = '/';
  }, 15000);
  ElMessageBox.confirm(data.message, '警告', {
    center: true,
    showCancelButton: false,
    closeOnClickModal: false,
    closeOnPressEscape: false,
    showClose: false
  }).then(() => {
    clearTimeout(timerId);
    window.location.href = '/';
  });
}
