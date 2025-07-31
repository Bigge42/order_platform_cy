import { createRouter, createWebHistory, createWebHashHistory } from 'vue-router'
import viewgird from './viewGird'
import store from '../store/index'
import redirect from './redirect'
import example from './example'
const routes = [
  {
    path: '/',
    name: 'Index',
    component: () => import('@/views/Index.vue'),
    redirect: '/home',
    children: [
      ...viewgird,
      ...redirect,
      ...example,
      {
        path: '/home',
        name: 'home',
        component: () => import('@/views/Home.vue')
      }, {
        path: '/UserInfo',
        name: 'UserInfo',
        component: () => import('@/views/sys/UserInfo.vue')
      },
      {
        path: '/sysMenu',
        name: 'sysMenu',
        component: () => import('@/views/sys/system/Sys_Menu.vue')
      }, {
        path: '/coder',
        name: 'coder',
        component: () => import('@/views/builder/coder.vue')
      },
      {
        path: '/fastCodeGenerator',
        name: 'fastCodeGenerator',
        component: () => import('@/views/builder/fastCodeGenerator.vue')
      },
      {
        path: '/formDraggable',  //表单设计
        name: 'formDraggable',
        component: () => import('@/views/formDraggable/formDraggable.vue')
      },
      {
        path: '/formSubmit',  //表单提交页面
        name: 'formSubmit',
        component: () => import('@/views/formDraggable/FormSubmit.vue'),
        meta:{
          keepAlive:false
        }
      },
      {
        path: '/formCollectionResultTree',  //显示收集的数据表单
        name: 'formCollectionResultTree',
        component: () => import('@/views/formDraggable/FormCollectionResultTree.vue'),
        meta:{
          keepAlive:false
        }
      },
      {
        path: '/signalR',  //消息推送
        name: 'signalR',
        component: () => import('@/views/signalR/Index.vue'),
        meta:{
          keepAlive:false
        }
      },
      {
        path: '/db',  //消息推送
        name: 'db',
        component: () => import('@/views/db/db.vue'),
        meta:{
          keepAlive:false
        }
      },
      {
        path: '/message',  //消息推送
        name: 'message',
        component: () => import('@/views/message/message.vue'),
        meta:{
          keepAlive:false
        }
      },
      {
        path: '/form',  //表单示例
        name: 'form',
        component: () => import('@/views/form/form.vue'),
        meta:{
          keepAlive:true
        }
      },
      {
        path: '/form2',  //表单示例
        name: 'form2',
        component: () => import('@/views/form/form2.vue'),
        meta:{
          keepAlive:true
        }
      },
      {
        path: '/formGroup',  //表单分组示例
        name: 'formGroup',
        component: () => import('@/views/form/formGroup.vue'),
        meta:{
          keepAlive:true
        }
      },
      // {
      //   path: '/table/:id',   //table示例
      //   name: 'table',
      //   component: () => import('@/views/table/table.vue')
      // },
      {
        path: '/table',   //table示例
        name: 'table',
        component: () => import('@/views/table/table.vue')
      },
      {
        path: '/chart',   //table示例
        name: 'chart',
        component: () => import('@/views/chart/chart.vue'),
      },
      {
        path: '/chart2',   //table示例
        name: 'chart2',
        component: () => import('@/views/chart/chart2.vue'),
      },
      {
        path: '/chart3',   //table示例
        name: 'chart3',
        component: () => import('@/views/chart/chart3.vue'),
      },
      {
        path: '/map',   //table示例
        name: 'map',
        component: () => import('@/views/map/map.vue'),
      },
      {
        path: '/mapForm',   //table示例
        name: 'mapForm',
        component: () => import('@/views/map/mapForm.vue'),
      },
      {
        path: '/report',   //table示例
        name: 'report',
        component: () => import('@/views/report/report.vue'),
        meta:{
          dynamic:true
          //keepAlive:false
        }
      },
      {
        path: '/monitor',   //table示例
        name: 'monitor',
        component: () => import('@/views/sys/monitor.vue'),
        meta:{
          dynamic:true
          //keepAlive:false
        }
      },
      {
        path: '/gridLayout',   //table示例
        name: 'gridLayout',
        component: () => import('@/views/gridLayout/index.vue'),
        meta:{
          dynamic:true
          //keepAlive:false
        }
      }
    ]
  },
  {
    path: '/login',
    name: 'login',
    component: () => import('@/views/Login.vue'),
    meta:{
        anonymous:true
      }
  },
  {
    path: '/sso-login',
    name: 'ssoLogin',
    component: () => import('@/views/SsoLogin.vue'),
    meta:{
        anonymous:true
      }
  },
  {
    path: '/print',  //打印
    name: 'print',
    component: () => import('@/views/print/print.vue'),
    meta:{
      keepAlive:false
    }
  },  
  {
    path: '/auth',  
    name: 'auth',
    component: () => import('@/views/auth/auth.vue'),
    meta:{
      anonymous:true
    }
  },
]

const router = createRouter({
  history: createWebHashHistory(), //createWebHistory(process.env.BASE_URL),
  routes
})


router.beforeEach((to, from, next) => {
  if (to.matched.length == 0) return next({ path: '/404' });
  //2020.06.03增加路由切换时加载提示
  store.dispatch("onLoading", true);
  if ((to.hasOwnProperty('meta') && to.meta.anonymous) || store.getters.isLogin() || to.path == '/login') {
    return next();
  }

  next({ path: '/login', query: { redirect: Math.random() } });
})
router.afterEach((to, from) => {
  store.dispatch("onLoading", false);
})
router.onError((error) => {
  // const targetPath = router.currentRoute.value.matched;
  try {
    console.log(error.message);
    if (process.env.NODE_ENV == 'development') {
      //alert(error.message)
    }
    localStorage.setItem("route_error", error.message)
  } catch (e) {

  }
 // window.location.href = '/'
});
export default router
