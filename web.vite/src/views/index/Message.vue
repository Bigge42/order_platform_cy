<template>
  <div>
    <el-dropdown trigger="hover">
      <div class="notification">
        <el-badge
          :value="msgCount"
          :max="99"
          :is-dot="msgCount <= 0"
          class="item"
          :offset="[3, -3]"
          :badge-style="
            msgCount > 0
              ? 'font-size: 10px; width: 18px; height: 18px;background-color: #ff1b0b;'
              : ''
          "
        >
          <el-icon size="15">
            <Bell />
          </el-icon>
        </el-badge>
      </div>
      <template #dropdown>
        <el-tabs v-model="activeName" class="msg-tabs" @tab-click="handleClick">
          <el-tab-pane name="urge">
            <template #label>
              <span class="custom-tabs-label">
                {{ $ts('催单') }}
                <el-badge
                  v-if="urgeList.length > 0"
                  :value="urgeList.length"
                  :max="99"
                  class="tab-badge"
                />
              </span>
            </template>
            <message-list :list="urgeList" type="urgent" @itemClick="itemClick"></message-list>
          </el-tab-pane>
          <el-tab-pane name="negotiate">
            <template #label>
              <span class="custom-tabs-label">
                {{ $ts('协商') }}
                <el-badge
                  v-if="negotiateList.length > 0"
                  :value="negotiateList.length"
                  :max="99"
                  class="tab-badge"
                />
              </span>
            </template>
            <message-list
              :list="negotiateList"
              type="negotiate"
              @itemClick="itemClick"
            ></message-list>
          </el-tab-pane>
          <el-tab-pane name="msg">
            <template #label>
              <span class="custom-tabs-label">
                {{ $ts('消息通知') }}
                <el-badge
                  v-if="unreadMsgCount > 0"
                  :value="unreadMsgCount"
                  :max="99"
                  class="tab-badge"
                />
              </span>
            </template>
            <div class="message-content-wrapper">
              <message-content :list="list" @itemClick="itemClick"></message-content>
            </div>
          </el-tab-pane>
          <!-- 隐藏审批流程tab -->
          <!-- <el-tab-pane label="审批流程" name="audit">
            <div class="message-content-wrapper">
              <message-content :list="auditList" @itemClick="itemClick"></message-content>
            </div>
          </el-tab-pane> -->
          <!-- 隐藏未读消息tab -->
          <!-- <el-tab-pane label="未读消息" name="unread">
            <template #label>
              <span class="custom-tabs-label">
                <el-badge
                  :value="msgCount"
                  :show-zero="false"
                  :offset="[-2, 4]"
                  badge-style="background-color: #ff1b0b;width: 18px; "
                >
                  未读消息<el-icon size="15"> </el-icon>
                </el-badge>
              </span>
            </template>
            <div class="message-content-wrapper">
              <message-content :list="unreadList" @itemClick="itemClick"></message-content>
            </div>
          </el-tab-pane> -->
        </el-tabs>
      </template>
    </el-dropdown>
    <ViewGridAudit ref="auditRef"></ViewGridAudit>
    <vol-box :lazy="true" v-model="model" title="消息" :width="700" :padding="10">
      <div v-html="htmlContent"></div>
      <template #footer
        ><div style="text-align: center">
          <el-button type="default" size="small" icon="Close" @click="model = false">{{
            $ts('关闭')
          }}</el-button>
        </div>
      </template>
    </vol-box>

    <!-- 消息抽屉 -->
    <MessageBoard ref="messageBoardRef" @submit="handleMessageBoardSubmit" />
  </div>
</template>

<script setup>
import MessageContent from './MessageContent.vue'
import MessageList from './MessageList.vue'
import MessageBoard from '@/comp/message-board/index.vue'
import ViewGridAudit from '@/components/basic/ViewGrid/ViewGridAudit.vue'
import { ref, getCurrentInstance, onMounted } from 'vue'
import { useStore } from 'vuex'
import http from '@/api/http'

const { proxy } = getCurrentInstance()
const store = useStore()
const htmlContent = ref('')
const model = ref(false)
const activeName = ref('urge')
const msgCount = ref(0)
const list = ref([])
const auditCount = ref(0)
const auditList = ref([])
const unreadList = ref([])
const urgeList = ref([])
const negotiateList = ref([])
const unreadMsgCount = ref(0)

// 消息抽屉引用
const messageBoardRef = ref(null)

// 获取当前登录用户userName（真正的登录账号）
const getCurrentUser = () => {
  return store.getters.getLoginName()
}

proxy.base.setItem('pushMessage', () => {
  // msgCount.value = msgCount.value + 1
  getList()
  getUrgeList()
  getNegotiateList()
})

//获取所有消息
const getList = () => {
  proxy.http.post('api/Sys_Notification/getList', {}, false).then((res) => {
    list.value = res.list
    // msgCount.value = res.total || 0
    // auditCount.value = res.auditTotal || 0

    // 计算未读消息数量
    updateUnreadMsgCount()
    // 修改msgCount逻辑：催单数量+协商数量+未读消息通知数量
    updateMsgCount()
  })
}

// 计算未读消息数量
const updateUnreadMsgCount = () => {
  unreadMsgCount.value = list.value.filter((item) => item.isRead !== 1).length
}

// 更新消息总数
const updateMsgCount = () => {
  msgCount.value = urgeList.value.length + negotiateList.value.length + unreadMsgCount.value
}

// 获取催单数据
const getUrgeList = async () => {
  if (sessionStorage.getItem('order.dev-model.stop-message') === '1') {
    return
  }
  try {
    const currentUser = getCurrentUser()

    // 如果未获取到当前登录人信息，则不查询数据
    if (!currentUser) {
      console.warn('未获取到当前登录人信息，跳过催单查询')
      urgeList.value = []
      return
    }

    // 准备查询条件
    const urgeWhereConditions = [
      {
        name: 'AssignedResPerson',
        value: currentUser,
        displayType: '='
      },
      {
        name: 'UrgentStatus',
        value: '催单中',
        displayType: '='
      }
    ]

    const urgeResponse = await http.post(
      'api/OCP_UrgentOrder/getPageData',
      {
        wheres: JSON.stringify(urgeWhereConditions),
        pageIndex: 1,
        pageSize: 10000
      },
      false
    )

    // 处理催单响应数据
    if (urgeResponse && urgeResponse.status === 0 && urgeResponse.rows) {
      urgeList.value = urgeResponse.rows
    } else {
      urgeList.value = []
    }

    // 更新消息总数
    updateMsgCount()
  } catch (error) {
    console.error('获取催单消息失败:', error)
    urgeList.value = []
    updateMsgCount()
  }
}

// 获取协商数据
const getNegotiateList = async () => {
  if (sessionStorage.getItem('order.dev-model.stop-message') === '1') {
    return
  }
  try {
    const currentUser = getCurrentUser()

    // 如果未获取到当前登录人信息，则不查询数据
    if (!currentUser) {
      console.warn('未获取到当前登录人信息，跳过协商查询')
      negotiateList.value = []
      return
    }

    // 准备查询条件
    const negotiateWhereConditions = [
      {
        name: 'AssignedResPerson',
        value: currentUser,
        displayType: '='
      },
      {
        name: 'NegotiationStatus',
        value: '协商中',
        displayType: '='
      }
    ]

    const negotiateResponse = await http.post(
      'api/OCP_Negotiation/getPageData',
      {
        wheres: JSON.stringify(negotiateWhereConditions),
        pageIndex: 1,
        pageSize: 10000
      },
      false
    )

    // 处理协商响应数据
    if (negotiateResponse && negotiateResponse.status === 0 && negotiateResponse.rows) {
      negotiateList.value = negotiateResponse.rows
    } else {
      negotiateList.value = []
    }

    // 更新消息总数
    updateMsgCount()
  } catch (error) {
    console.error('获取协商消息失败:', error)
    negotiateList.value = []
    updateMsgCount()
  }
}

// 组件挂载时获取初始数据
onMounted(() => {
  getList()
  getUrgeList()
  getNegotiateList()
})
const handleClick = (item) => {
  if (item.paneName == 'urge') {
    getUrgeList()
  } else if (item.paneName == 'negotiate') {
    getNegotiateList()
  } else if (item.paneName == 'msg') {
    getList()
  }
  // 注释审批流程接口调用
  // else if (item.paneName == 'audit') {
  //   proxy.http.post('api/Sys_Notification/getAuditList', {}, false).then((res) => {
  //     auditList.value = res.list
  //   })
  // }
  // 注释未读消息接口调用
  // else if (item.paneName == 'unread') {
  //   proxy.http.post('api/Sys_Notification/getUnreadList', {}, false).then((res) => {
  //     unreadList.value = res.list
  //   })
  // }
}

const auditRef = ref()

const itemClick = (item, isRead) => {
  const isAudit = item.notificationType == '审批'
  const isUrge = item.notificationType == '催单' || item.UrgentStatus == '催单中'
  const isNegotiate = item.notificationType == '协商' || item.NegotiationStatus == '协商中'

  if (!isRead) {
    // 重新计算未读消息数量和消息总数
    updateUnreadMsgCount()
    updateMsgCount()
    // if (isAudit) {
    //   auditCount.value = auditCount - 1
    //   if (auditCount.value < 0) {
    //     auditCount.value = 0
    //   }
    // }
  }
  if (item.LinkUrl) {
    window.open(item.LinkUrl, '_blank')
    return
  }
  //审批跳转
  if (isAudit) {
    item.WorkTable = item.tableName
    item.WorkTableKey = item.tableKey

    auditRef.value.open([item], true)
    // proxy.$tabs.open({ text: '我的审批', path: '/Sys_WorkFlowTable' })
    return
  }

  // 催单和协商处理
  if (isUrge || isNegotiate) {
    // 获取业务类型和业务键值
    const businessType = item.BusinessType || ''
    const businessKey = item.BusinessKey || ''

    // 打开消息抽屉
    if (businessType && businessKey) {
      if (isUrge) {
        // 催单类型，默认选中催单tab
        messageBoardRef.value?.open(businessType, businessKey, 'urgent')
      } else if (isNegotiate) {
        // 协商类型，默认选中协商tab
        messageBoardRef.value?.open(businessType, businessKey, 'negotiate')
      }
      return
    }
  }

  if (item.notificationType == '通知' && item.notificationContent) {
    model.value = true
    htmlContent.value = item.notificationContent
    return
  }
}

// 处理消息抽屉提交回调（可选）
const handleMessageBoardSubmit = (submitData) => {
  // 根据提交类型刷新对应的数据
  if (submitData.type === 'urgent') {
    // 如果是催单，刷新催单数据
    getUrgeList()
  } else if (submitData.type === 'negotiate') {
    // 如果是协商，刷新协商数据
    getNegotiateList()
  }
}
</script>
<style scoped lang="less">
.notification {
  outline: none;
  color: #000;
}
.msg-tabs {
  width: 300px;
}

::v-deep(.el-tabs__header) {
  margin: 0;
}
::v-deep(.el-tabs__content) {
  min-height: 400px;
  max-height: 400px;
  overflow: hidden;
}
::v-deep(.el-tabs__nav) {
  width: 100%;
  padding: 0 10px;
}
::v-deep(.el-tabs__item) {
  padding: 0 6px;
  flex: 1;
}

.custom-tabs-label {
  position: relative;
  display: inline-block;
}

.tab-badge {
  margin-left: 8px;
}

::v-deep(.tab-badge .el-badge__content) {
  background-color: #ff1b0b;
  font-size: 10px;
  min-width: 16px;
  height: 16px;
  line-height: 16px;
  border-radius: 8px;
}

.message-content-wrapper {
  height: 400px;
  overflow: hidden;
}

.message-content-wrapper ::v-deep(.el-scrollbar) {
  height: 100% !important;
  max-height: 400px !important;
}

.message-content-wrapper ::v-deep(.el-scrollbar__view) {
  height: 100%;
}
</style>
