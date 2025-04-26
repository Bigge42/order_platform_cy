import type { App } from 'vue'
import { createRouter, createWebHashHistory, RouteRecordRaw } from 'vue-router'
import { createRouterGuards } from './router-guards'
import { PageEnum } from '@/enums/pageEnum'
import { HttpErrorPage, LoginRoute, ReloadRoute, RedirectRoute } from '@/router/base'
import { Layout } from '@/router/constant'

import modules from '@/router/modules'

const RootRoute: Array<RouteRecordRaw> = [
  {
    path: '/',
    name: 'Root',
    redirect:'/home',// PageEnum.BASE_HOME,
    component: Layout,
    meta: {
      title: 'Root',
    },
    children: [
      {
        path: '/home',
        name: "home",
        component: () => import('@/views/home/index.vue') ,
        //redirect
        meta: {
          title:""
        },
        children:[{
          path: '/home/index',
          name: "homeindex",
          component: () => import('@/views/home/redirect.vue') ,
        }, modules.previewRoutes]
      },
      ...HttpErrorPage,
      ...RedirectRoute,
      modules.projectRoutes,
      modules.chartRoutes,
     // modules.previewRoutes,
      modules.editRoutes,
      {
        path: '/auth/index',
        name: "authindex",
        component: () => import('@/views/auth/index.vue') ,
      }
    ]
  }
]


export const constantRouter: any[] = [LoginRoute, ...RootRoute, ReloadRoute];

const router = createRouter({
  history: createWebHashHistory(''),
  routes: constantRouter,
  strict: true,
})

export function setupRouter(app: App) {
  app.use(router);
  // 创建路由守卫
  createRouterGuards(router)
}
export default router
