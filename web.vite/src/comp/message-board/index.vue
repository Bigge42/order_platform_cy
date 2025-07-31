<template>
  <el-drawer
    v-model="visible"
    title="留言板"
    :close-on-click-modal="true"
    :close-on-press-escape="true"
    :size="600"
    direction="rtl"
    @closed="handleDrawerClosed"
  >
    <div class="reply-stats">
      <div class="stat-item">
        <el-icon class="stat-icon success"><Check /></el-icon>
        <span class="stat-text">{{ replyStats.replied }} 已回复</span>
      </div>
      <div class="stat-item">
        <el-icon class="stat-icon warning"><Clock /></el-icon>
        <span class="stat-text">{{ replyStats.pending }} 待回复</span>
      </div>
      <div class="total-count">共 {{ totalMessages }} 条</div>
    </div>
    <div class="message-board-content">
      <!-- 业务筛选标签 -->
      <div class="filter-section">
        <div class="filter-tags">
          <el-tag
            v-for="business in businesses"
            :key="business.key"
            :type="currentBusiness === business.key ? 'primary' : 'info'"
            :effect="currentBusiness === business.key ? 'dark' : 'plain'"
            class="filter-tag"
            @click="switchBusiness(business.key)"
          >
            {{ business.label }} ({{ business.count }})
          </el-tag>
        </div>
      </div>
      <div class="stats-section">
        <div class="message-types">
          <div
            class="type-item"
            :class="{ active: currentType === 'urgent' }"
            @click="switchType('urgent')"
          >
            <div class="type-label">催单</div>
            <div class="type-count">{{ urgentCount }}</div>
          </div>
          <div
            class="type-item"
            :class="{ active: currentType === 'negotiate' }"
            @click="switchType('negotiate')"
          >
            <div class="type-label">协商</div>
            <div class="type-count">{{ negotiateCount }}</div>
          </div>
        </div>
      </div>

      <!-- 消息列表 -->
      <div class="messages-section">
        <!-- Loading状态 - 只在消息列表区域显示 -->
        <div v-if="loading" class="message-loading-container">
          <el-skeleton :rows="6" animated />
        </div>

        <!-- 消息列表内容 -->
        <div v-else class="message-list" ref="messageListRef">
          <div v-for="message in filteredMessages" :key="message.id" class="message-item">
            <div class="message-header">
              <div class="user-info">
                <span class="username">{{ message.username }}</span>
              </div>
              <div class="message-meta">
                <el-tag
                  :type="getMessageTypeColor(message.type)"
                  size="small"
                  class="message-type-tag"
                >
                  {{ getMessageTypeLabel(message.type) }}
                </el-tag>
                <el-tag
                  :type="message.isReplied ? 'success' : 'info'"
                  size="small"
                  class="reply-status-tag"
                >
                  {{ message.isReplied ? '已回复' : '未回复' }}
                </el-tag>
                <!-- 显示紧急程度 -->
                <el-tag
                  v-if="message.rawData.urgencyLevel"
                  type="danger"
                  size="small"
                  class="urgency-level-tag"
                >
                  {{ message.rawData.urgencyLevel }}
                </el-tag>
                <!-- 显示协商类型 -->
                <el-tag
                  v-if="message.rawData.negotiationType"
                  type="warning"
                  size="small"
                  class="negotiation-type-tag"
                >
                  {{ getNegotiationTypeLabel(message.rawData.negotiationType) }}
                </el-tag>
                <!-- 显示催单状态 -->
                <el-tag
                  v-if="message.type === 'urgent' && message.rawData.urgentStatus"
                  :type="getUrgentStatusColor(message.rawData.urgentStatus)"
                  size="small"
                  class="urgent-status-tag"
                >
                  {{ message.rawData.urgentStatus }}
                </el-tag>
                <!-- 显示协商状态 -->
                <el-tag
                  v-if="message.type === 'negotiate' && message.rawData.negotiationStatus"
                  :type="getNegotiationStatusColor(message.rawData.negotiationStatus)"
                  size="small"
                  class="negotiation-status-tag"
                >
                  {{ message.rawData.negotiationStatus }}
                </el-tag>
              </div>
            </div>

            <div class="message-content">
              <!-- 只有当内容有值时才显示 -->
              <div v-if="message.content" class="message-main-content">
                <strong>{{ message.type === 'urgent' ? '催单内容：' : '协商内容：' }}</strong
                >{{ message.content }}
              </div>
              <!-- 显示协商原因 -->
              <div v-if="message.rawData.negotiationReason" class="message-reason">
                <strong>协商原因：</strong
                >{{ getNegotiationReasonLabel(message.rawData.negotiationReason) }}
              </div>
              <!-- 显示备注 -->
              <div v-if="message.rawData.remarks" class="message-remarks">
                <strong>备注：</strong>{{ message.rawData.remarks }}
              </div>
            </div>

            <div class="message-footer">
              <div class="message-times">
                <span class="message-time"
                  >{{ message.type === 'urgent' ? '催单时间：' : '协商时间：'
                  }}{{ formatTime(message.rawData.createDate) }}</span
                >
                <span v-if="message.time !== message.rawData.createDate" class="message-time">
                  更新时间：{{ formatTime(message.time) }}
                </span>
              </div>
              <!-- 协商和催单类型显示回复按钮 -->
              <el-button
                v-if="message.canReply"
                type="primary"
                size="small"
                plain
                class="reply-btn"
                @click="openReplyDialog(message)"
              >
                回复
              </el-button>
            </div>

            <!-- 回复列表 -->
            <div v-if="message.replies && message.replies.length > 0" class="replies-section">
              <div class="replies-header">
                <div class="replies-title-wrapper" @click="toggleReplies(message.id)">
                  <span class="replies-title">回复</span>
                  <span class="replies-count">({{ message.replies.length }})</span>
                  <el-icon
                    class="collapse-icon"
                    :class="{ collapsed: !isRepliesExpanded(message.id) }"
                  >
                    <ArrowDown />
                  </el-icon>
                </div>
              </div>
              <div v-show="isRepliesExpanded(message.id)" class="replies-list">
                <div v-for="reply in message.replies" :key="reply.replyID" class="reply-item">
                  <div class="reply-header">
                    <span class="reply-username">{{ reply.replyPersonName || reply.creator }}</span>
                    <span class="reply-business-type">{{
                      getBusinessTypeLabel(message.businessType)
                    }}</span>
                    <span v-if="reply.replyPersonPhone" class="reply-phone">{{
                      reply.replyPersonPhone
                    }}</span>
                    <span class="reply-time"
                      >回复时间：{{ formatTime(reply.replyTime || reply.createDate) }}</span
                    >
                  </div>
                  <div class="reply-content">
                    <div class="reply-main-content">
                      <strong>回复内容：</strong>{{ reply.replyContent }}
                    </div>
                    <div v-if="reply.replyProgress" class="reply-progress">
                      <strong>回复进度：</strong>{{ reply.replyProgress }}
                    </div>
                    <div v-if="reply.replyDeliveryDate" class="reply-delivery">
                      <strong>回复交期：</strong>{{ formatTime(reply.replyDeliveryDate) }}
                    </div>
                    <div v-if="reply.remarks" class="reply-remarks">
                      <strong>备注：</strong>{{ reply.remarks }}
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- 底部回复区域 - 仅在选中留言类型时显示 -->
      <div v-if="currentType === 'message'" class="reply-section">
        <div class="quick-reply-label">快捷回复</div>
        <div class="reply-actions">
          <el-button
            type="primary"
            size="small"
            plain
            class="quick-reply-btn"
            @click="handleProcessed"
          >
            已处理
          </el-button>
          <el-button type="info" size="small" plain class="quick-reply-btn" @click="handleNeedTime">
            需要时间
          </el-button>
          <el-button
            type="warning"
            size="small"
            plain
            class="quick-reply-btn"
            @click="handleTransfer"
          >
            转交他人
          </el-button>
        </div>

        <div class="reply-input">
          <el-input
            v-model="replyText"
            type="textarea"
            :rows="3"
            placeholder="输入回复内容..."
            class="reply-textarea"
            @keydown="handleKeydown"
          />
          <div class="input-actions">
            <el-button
              type="primary"
              size="small"
              @click="handleSendReply"
              :disabled="!replyText.trim()"
            >
              发送
            </el-button>
          </div>
        </div>

        <div class="input-hint">Enter 发送 • Shift+Enter 换行</div>
      </div>
    </div>

    <!-- 催单回复弹窗 -->
    <ReminderReplyDialog
      ref="reminderReplyDialogRef"
      v-model="reminderReplyVisible"
      :message-id="selectedMessage?.id"
      @submit="handleReminderReply"
    />

    <!-- 协商回复弹窗 -->
    <NegotiationReplyDialog
      ref="negotiationReplyDialogRef"
      v-model="negotiationReplyVisible"
      :data="selectedMessage"
      @confirm="handleNegotiationReply"
      @cancel="() => (negotiationReplyVisible = false)"
    />
  </el-drawer>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { Check, Clock, ArrowDown } from '@element-plus/icons-vue'
import { ElMessage } from 'element-plus'
import ReminderReplyDialog from '@/comp/reminder-dialog/reply.vue'
import NegotiationReplyDialog from '@/comp/negotiation-dialog/reply.vue'
import http from '@/api/http'

// 控制抽屉显示隐藏
const visible = ref(false)

// 加载状态
const loading = ref(false)

// 当前选中的消息类型
const currentType = ref('urgent')

// 当前选中的业务
const currentBusiness = ref('ALL')

// 回复内容
const replyText = ref('')

// 消息列表引用
const messageListRef = ref(null)

// 选中的消息
const selectedMessage = ref(null)

// 催单回复弹窗
const reminderReplyDialogRef = ref(null)
const reminderReplyVisible = ref(false)

// 协商回复弹窗
const negotiationReplyDialogRef = ref(null)
const negotiationReplyVisible = ref(false)

// 回复折叠状态管理 - 默认展开
const repliesExpandedState = ref({})

// 统计数据
const stats = ref({
  urgent: 0,
  negotiate: 0,
  message: 0
})

// 回复统计
const replyStats = ref({
  replied: 0,
  pending: 0
})

// 业务列表
const businesses = ref([
  { key: 'ALL', label: '全部', count: 0 },
  { key: 'PO', label: '采购', count: 0 },
  { key: 'SO', label: '销售', count: 0 },
  { key: 'JS', label: '技术', count: 0 },
  { key: 'JH', label: '计划', count: 0 },
  { key: 'ZP', label: '装配', count: 0 },
  { key: 'JG', label: '金工', count: 0 },
  { key: 'WW', label: '委外', count: 0 },
  { key: 'BJ', label: '部件', count: 0 }
])

// 存储业务数据
const businessData = ref({
  businessType: '',
  businessKey: ''
})

// 原始催单数据
const urgentOrders = ref([])

// 原始协商数据
const negotiations = ref([])

// 转换后的消息列表
const messages = ref([])

// 字典数据
const negotiationReasonOptions = ref([])
const negotiationTypeOptions = ref([])
const businessTypeOptions = ref([])

// 获取协商原因字典数据
const loadNegotiationReasonOptions = async () => {
  try {
    const params = {
      page: 1,
      rows: 100,
      sort: 'OrderNo,CreateDate',
      order: 'desc',
      wheres: '[]',
      value: 110,
      tableName: null,
      isCopyClick: false
    }

    const result = await http.post('api/Sys_Dictionary/getDetailPage', params, false)

    if (result && result.rows) {
      negotiationReasonOptions.value = result.rows
        .filter((item) => item.Enable === 1)
        .map((item) => ({
          value: item.DicValue,
          label: item.DicName
        }))
    }
  } catch (error) {
    console.error('获取协商原因选项失败:', error)
  }
}

// 获取协商类型字典数据
const loadNegotiationTypeOptions = async () => {
  try {
    const params = {
      page: 1,
      rows: 100,
      sort: 'OrderNo,CreateDate',
      order: 'desc',
      wheres: '[]',
      value: 111, // 假设协商类型的字典值是111
      tableName: null,
      isCopyClick: false
    }

    const result = await http.post('api/Sys_Dictionary/getDetailPage', params, false)

    if (result && result.rows) {
      negotiationTypeOptions.value = result.rows
        .filter((item) => item.Enable === 1)
        .map((item) => ({
          value: item.DicValue,
          label: item.DicName
        }))
    }
  } catch (error) {
    console.error('获取协商类型选项失败:', error)
  }
}

// 获取业务类型字典数据
const loadBusinessTypeOptions = async () => {
  try {
    const params = {
      page: 1,
      rows: 100,
      sort: 'OrderNo,CreateDate',
      order: 'desc',
      wheres: '[]',
      value: 109,
      tableName: null,
      isCopyClick: false
    }

    const result = await http.post('api/Sys_Dictionary/getDetailPage', params, false)

    if (result && result.rows) {
      businessTypeOptions.value = result.rows
        .filter((item) => item.Enable === 1)
        .map((item) => ({
          value: item.DicValue,
          label: item.DicName
        }))
    }
  } catch (error) {
    console.error('获取业务类型选项失败:', error)
  }
}

// 根据值获取协商原因标签
const getNegotiationReasonLabel = (value) => {
  if (!value || !negotiationReasonOptions.value.length) {
    return value || ''
  }
  const option = negotiationReasonOptions.value.find((item) => item.value === value)
  return option ? option.label : value
}

// 根据值获取协商类型标签
const getNegotiationTypeLabel = (value) => {
  if (!value || !negotiationTypeOptions.value.length) {
    return value || ''
  }
  const option = negotiationTypeOptions.value.find((item) => item.value === value)
  return option ? option.label : value
}

// 根据值获取业务类型标签
const getBusinessTypeLabel = (value) => {
  if (!value) {
    return ''
  }

  // 先从预设的业务列表中查找
  const business = businesses.value.find((b) => b.key === value)
  if (business) {
    return business.label
  }

  // 再从字典数据中查找
  if (businessTypeOptions.value.length) {
    const option = businessTypeOptions.value.find((item) => item.value === value)
    if (option) {
      return option.label
    }
  }

  return value
}

// 回复折叠状态管理
const toggleReplies = (messageId) => {
  repliesExpandedState.value[messageId] = !repliesExpandedState.value[messageId]
}

const isRepliesExpanded = (messageId) => {
  // 默认展开，如果没有设置状态，返回true
  return repliesExpandedState.value[messageId] !== false
}

// 计算总消息数
const totalMessages = computed(() => {
  return stats.value.urgent + stats.value.negotiate + stats.value.message
})

// 计算当前催单消息数量（按业务类型过滤）
const urgentCount = computed(() => {
  let filtered = messages.value.filter((msg) => msg.type === 'urgent')

  // 按业务类型过滤
  if (currentBusiness.value !== 'ALL') {
    filtered = filtered.filter((msg) => msg.businessType === currentBusiness.value)
  }

  return filtered.length
})

// 计算当前协商消息数量（按业务类型过滤）
const negotiateCount = computed(() => {
  let filtered = messages.value.filter((msg) => msg.type === 'negotiate')

  // 按业务类型过滤
  if (currentBusiness.value !== 'ALL') {
    filtered = filtered.filter((msg) => msg.businessType === currentBusiness.value)
  }

  return filtered.length
})

// 过滤后的消息列表
const filteredMessages = computed(() => {
  let filtered = messages.value

  // 按消息类型过滤
  if (currentType.value !== 'all') {
    filtered = filtered.filter((msg) => msg.type === currentType.value)
  }

  // 按业务过滤
  if (currentBusiness.value !== 'ALL') {
    filtered = filtered.filter((msg) => msg.businessType === currentBusiness.value)
  }

  return filtered
})

// 获取业务协同数据
const fetchBusinessOrderCollaboration = async () => {
  if (!businessData.value.businessType || !businessData.value.businessKey) {
    return
  }

  loading.value = true

  try {
    const response = await http.get(
      `/api/OCP_UrgentOrder/GetBusinessOrderCollaboration?businessType=${encodeURIComponent(
        businessData.value.businessType
      )}&businessKey=${encodeURIComponent(businessData.value.businessKey)}`
    )

    if (response && response.status) {
      const data = response.data

      // 保存原始数据
      urgentOrders.value = data.urgentOrders || []
      negotiations.value = data.negotiations || []

      // 转换为统一的消息格式
      const urgentMessages = urgentOrders.value.map((order) => ({
        id: `urgent_${order.urgentOrderID}`,
        type: 'urgent',
        businessType: order.businessType,
        content: order.urgentContent,
        username: order.creator || order.assignedResPersonName || '系统',
        time: order.createDate,
        isReplied: order.replies && order.replies.length > 0,
        canReply: order.canReply,
        replies: order.replies || [],
        rawData: order
      }))

      const negotiationMessages = negotiations.value.map((negotiation) => ({
        id: `negotiate_${negotiation.negotiationID}`,
        type: 'negotiate',
        businessType: negotiation.businessType,
        content: negotiation.negotiationContent,
        username: negotiation.creator || negotiation.assignedResPersonName || '系统',
        time: negotiation.createDate,
        isReplied: negotiation.replies && negotiation.replies.length > 0,
        canReply: negotiation.canReply,
        replies: negotiation.replies || [],
        rawData: negotiation
      }))

      // 合并消息列表
      messages.value = [...urgentMessages, ...negotiationMessages].sort((a, b) => {
        return new Date(b.time) - new Date(a.time)
      })

      // 更新统计数据
      if (data.summary) {
        stats.value.urgent = data.summary.totalUrgentOrders || 0
        stats.value.negotiate = data.summary.totalNegotiations || 0
        stats.value.message = 0 // 留言功能暂未实现

        replyStats.value.replied =
          (data.summary.repliedUrgentOrders || 0) + (data.summary.repliedNegotiations || 0)
        replyStats.value.pending =
          (data.summary.pendingUrgentOrders || 0) + (data.summary.pendingNegotiations || 0)
      }

      // 更新业务统计
      updateBusinessCounts()
    }
  } catch (error) {
    console.error('获取业务协同数据失败:', error)
    ElMessage.error('获取数据失败，请稍后重试')
  } finally {
    loading.value = false
  }
}

// 更新业务类型统计
const updateBusinessCounts = () => {
  // 重置计数
  businesses.value.forEach((business) => {
    business.count = 0
  })

  // 统计各业务类型的消息数
  messages.value.forEach((msg) => {
    const business = businesses.value.find((b) => b.key === msg.businessType)
    if (business) {
      business.count++
    }
  })

  // 更新全部的计数
  const allBusiness = businesses.value.find((b) => b.key === 'ALL')
  if (allBusiness) {
    allBusiness.count = messages.value.length
  }
}

// 切换消息类型
const switchType = (type) => {
  currentType.value = type
}

// 切换业务
const switchBusiness = (business) => {
  currentBusiness.value = business
}

// 获取消息类型颜色
const getMessageTypeColor = (type) => {
  const colorMap = {
    urgent: 'danger',
    negotiate: 'warning',
    message: 'info'
  }
  return colorMap[type] || 'info'
}

// 获取消息类型标签
const getMessageTypeLabel = (type) => {
  const labelMap = {
    urgent: '催单',
    negotiate: '协商',
    message: '留言'
  }
  return labelMap[type] || '留言'
}

// 获取催单状态颜色
const getUrgentStatusColor = (status) => {
  if (!status) return 'info'

  const statusLower = status.toLowerCase()
  if (statusLower.includes('待') || statusLower.includes('pending')) {
    return 'warning'
  } else if (
    statusLower.includes('已') ||
    statusLower.includes('完成') ||
    statusLower.includes('处理') ||
    statusLower.includes('complete')
  ) {
    return 'success'
  } else if (
    statusLower.includes('超时') ||
    statusLower.includes('逾期') ||
    statusLower.includes('overdue')
  ) {
    return 'danger'
  }
  return 'info'
}

// 获取协商状态颜色
const getNegotiationStatusColor = (status) => {
  if (!status) return 'info'

  const statusLower = status.toLowerCase()
  if (statusLower.includes('待') || statusLower.includes('pending')) {
    return 'warning'
  } else if (
    statusLower.includes('已协商') ||
    statusLower.includes('同意') ||
    statusLower.includes('agreed') ||
    statusLower.includes('完成')
  ) {
    return 'success'
  } else if (
    statusLower.includes('拒绝') ||
    statusLower.includes('rejected') ||
    statusLower.includes('失败')
  ) {
    return 'danger'
  }
  return 'info'
}

// 格式化时间
const formatTime = (timeStr) => {
  if (!timeStr) return ''

  try {
    const date = new Date(timeStr)
    return date.toLocaleString('zh-CN', {
      year: 'numeric',
      month: '2-digit',
      day: '2-digit',
      hour: '2-digit',
      minute: '2-digit'
    })
  } catch (error) {
    return timeStr
  }
}

// 处理快捷回复 - 已处理
const handleProcessed = () => {
  sendQuickReply('已处理，问题已解决。')
}

// 处理快捷回复 - 需要时间
const handleNeedTime = () => {
  sendQuickReply('收到，需要时间处理，稍后回复。')
}

// 处理快捷回复 - 转交他人
const handleTransfer = () => {
  sendQuickReply('已转交给相关同事处理。')
}

// 发送快捷回复的通用方法
const sendQuickReply = (content) => {
  const newMessage = {
    id: Date.now(),
    username: '当前用户',
    dept: '系统',
    type: 'message',
    content: content,
    time: new Date().toLocaleString('zh-CN', {
      year: 'numeric',
      month: '2-digit',
      day: '2-digit',
      hour: '2-digit',
      minute: '2-digit'
    }),
    isReplied: true,
    BusinessType: 'ALL'
  }

  messages.value.unshift(newMessage)

  // 滚动到顶部显示新消息
  setTimeout(() => {
    if (messageListRef.value) {
      messageListRef.value.scrollTop = 0
    }
  }, 100)

  // 触发提交回调
  emit('submit', {
    type: 'quickReply',
    data: { content: content },
    response: { status: true, message: '快捷回复成功' }
  })
}

// 发送回复
const handleSendReply = () => {
  if (!replyText.value.trim()) {
    return
  }

  // 模拟发送回复
  const newMessage = {
    id: Date.now(),
    username: '当前用户',
    dept: '系统',
    type: 'message',
    content: replyText.value,
    time: new Date().toLocaleString('zh-CN', {
      year: 'numeric',
      month: '2-digit',
      day: '2-digit',
      hour: '2-digit',
      minute: '2-digit'
    }),
    isReplied: true,
    BusinessType: 'ALL'
  }

  messages.value.unshift(newMessage)
  const replyContent = replyText.value
  replyText.value = ''

  // 滚动到顶部显示新消息
  setTimeout(() => {
    if (messageListRef.value) {
      messageListRef.value.scrollTop = 0
    }
  }, 100)

  // 触发提交回调
  emit('submit', {
    type: 'message',
    data: { content: replyContent },
    response: { status: true, message: '发送成功' }
  })
}

// 处理键盘事件
const handleKeydown = (event) => {
  if (event.key === 'Enter' && !event.shiftKey) {
    event.preventDefault()
    handleSendReply()
  }
}

// 暴露给父组件的方法
const open = (businessType, businessKey, defaultType = 'urgent') => {
  // 设置业务数据
  businessData.value = {
    businessType: businessType || '',
    businessKey: businessKey || ''
  }

  // 设置默认选中的消息类型
  currentType.value = defaultType

  // 重置折叠状态（打开新的消息板时，所有回复都默认展开）
  repliesExpandedState.value = {}

  visible.value = true

  // 打开后加载数据
  if (businessType && businessKey) {
    fetchBusinessOrderCollaboration()
  }
}

const close = () => {
  visible.value = false
}

// 重置数据到初始值
const resetData = () => {
  // 重置消息相关数据
  messages.value = []
  urgentOrders.value = []
  negotiations.value = []

  // 重置统计数据
  stats.value = {
    urgent: 0,
    negotiate: 0,
    message: 0
  }

  replyStats.value = {
    replied: 0,
    pending: 0
  }

  // 重置业务统计
  businesses.value.forEach((business) => {
    business.count = 0
  })

  // 重置筛选状态
  currentType.value = 'urgent'
  currentBusiness.value = 'ALL'

  // 重置业务数据
  businessData.value = {
    businessType: '',
    businessKey: ''
  }

  // 重置回复相关状态
  replyText.value = ''
  selectedMessage.value = null

  // 重置回复折叠状态
  repliesExpandedState.value = {}

  // 重置加载状态
  loading.value = false

  // 关闭所有弹窗
  reminderReplyVisible.value = false
  negotiationReplyVisible.value = false
}

// 处理抽屉关闭事件
const handleDrawerClosed = () => {
  resetData()
}

// 定义事件
const emit = defineEmits(['submit'])

// 向父组件暴露方法
defineExpose({
  open,
  close,
  resetData
})

// 打开回复弹窗
const openReplyDialog = (message) => {
  // 根据消息类型打开不同的弹窗
  if (message.type === 'urgent') {
    // 催单类型使用催单回复弹窗
    selectedMessage.value = {
      ...message,
      id: message.rawData.urgentOrderID
    }
    reminderReplyVisible.value = true
  } else if (message.type === 'negotiate') {
    // 协商类型使用协商回复弹窗
    // 为协商类型添加必要的字段
    const rawData = message.rawData || {}
    selectedMessage.value = {
      ...message,
      ...rawData,
      NegotiationID: rawData.negotiationID,
      NegotiationDate: rawData.negotiationDate || new Date().toISOString()
    }
    negotiationReplyVisible.value = true
  }
}

// 处理催单回复提交
const handleReminderReply = async (formData) => {
  try {
    // 构建催单回复请求数据 - 按照接口文档格式
    const requestData = {
      urgentOrderID: selectedMessage.value.rawData.urgentOrderID,
      replyContent: formData.replyContent || null,
      replyProgress: formData.replyProgress || null,
      replyDeliveryDate: formData.replyDeliveryDate || null,
      remarks: formData.remarks || null
    }

    // 调用催单回复API - 使用新的AddReply接口
    const response = await http.post('/api/OCP_UrgentOrderReply/AddReply', requestData)

    if (response && response.status) {
      ElMessage.success(response.message || '回复成功')

      // 刷新数据
      await fetchBusinessOrderCollaboration()

      reminderReplyVisible.value = false

      // 触发提交回调
      emit('submit', {
        type: 'urgent',
        data: requestData,
        response: response
      })
    } else {
      throw new Error(response?.message || '回复失败')
    }
  } catch (error) {
    console.error('催单回复失败:', error)
    ElMessage.error(error.message || '回复失败，请重试')
  }
}

// 处理协商回复提交
const handleNegotiationReply = async (formData) => {
  try {
    // 构建协商回复请求数据 - 按照接口文档格式
    const requestData = {
      negotiationID: selectedMessage.value.rawData.negotiationID,
      replyContent: formData.replyContent || null,
      replyDeliveryDate: formData.replyDeliveryDate || null,
      negotiationStatus: formData.negotiationStatus || null
    }

    // 调用协商回复API - 使用新的AddReply接口
    const response = await http.post('/api/OCP_NegotiationReply/AddReply', requestData)

    if (response && response.status) {
      const status = formData.negotiationStatus === '已同意' ? '同意' : '拒绝'
      ElMessage.success(`已${status}协商请求`)

      // 刷新数据
      await fetchBusinessOrderCollaboration()

      negotiationReplyVisible.value = false

      // 触发提交回调
      emit('submit', {
        type: 'negotiate',
        data: requestData,
        response: response
      })
    } else {
      throw new Error(response?.message || '回复失败')
    }
  } catch (error) {
    console.error('协商回复失败:', error)
    ElMessage.error(error.message || '回复失败，请重试')
  }
}

onMounted(() => {
  // 组件挂载后的初始化逻辑
  // 如果已有业务数据，则自动加载
  if (businessData.value.businessType && businessData.value.businessKey) {
    fetchBusinessOrderCollaboration()
  }

  // 加载字典数据
  loadNegotiationReasonOptions()
  loadNegotiationTypeOptions()
  loadBusinessTypeOptions()
})
</script>

<style scoped>
.message-board-content {
  display: flex;
  flex-direction: column;
  height: calc(100% - 42px);
  padding: 0;
}

/* 消息列表Loading状态样式 */
.message-loading-container {
  padding: 20px;
  height: 100%;
  display: flex;
  flex-direction: column;
  gap: 16px;
  background: #f8f9fa;
}

/* 顶部统计区域 */
.stats-section {
  padding: 12px 16px;
  border-bottom: 1px solid var(--el-border-color-light);
  background: #f8f9fa;
}

.message-types {
  display: flex;
  gap: 8px;
  margin-bottom: 0;
}

.type-item {
  flex: 1;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 4px;
  text-align: center;
  padding: 8px 10px;
  border-radius: 6px;
  background: white;
  cursor: pointer;
  transition: all 0.3s ease;
  border: 1px solid var(--el-border-color-light);
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
}

.type-item:hover {
  background: var(--el-color-primary-light-9);
  border-color: var(--el-color-primary-light-7);
}

.type-item.active {
  background: var(--el-color-primary);
  color: white;
  border-color: var(--el-color-primary);
  box-shadow: 0 2px 8px rgba(64, 158, 255, 0.3);
}

.type-label {
  font-size: 14px;
  font-weight: 500;
}

.type-count {
  font-size: 14px;
  font-weight: bold;
}

.reply-stats {
  display: flex;
  align-items: center;
  gap: 20px;
  font-size: 13px;
  padding: 10px 14px;
  background: white;
  border-radius: 6px;
  border: 1px solid var(--el-border-color-lighter);
}

.stat-item {
  display: flex;
  align-items: center;
  gap: 6px;
}

.stat-icon.success {
  color: var(--el-color-success);
}

.stat-icon.warning {
  color: var(--el-color-warning);
}

.stat-text {
  color: var(--el-text-color-regular);
  font-weight: 500;
}

.total-count {
  margin-left: auto;
  color: var(--el-text-color-secondary);
  font-weight: 500;
}

/* 筛选区域 */
.filter-section {
  padding: 16px 20px;
  border-bottom: 1px solid var(--el-border-color-light);
  background: white;
}

.filter-tags {
  display: flex;
  flex-wrap: wrap;
  gap: 10px;
}

.filter-tag {
  cursor: pointer;
  transition: all 0.3s ease;
  font-weight: 500;
}

.filter-tag:hover {
  transform: translateY(-1px);
}

/* 消息列表区域 */
.messages-section {
  flex: 1;
  overflow: hidden;
  background: #f8f9fa;
}

.message-list {
  height: 100%;
  overflow-y: auto;
  padding: 16px 20px;
}

.message-item {
  padding: 18px;
  border: 1px solid var(--el-border-color-lighter);
  border-radius: 10px;
  margin-bottom: 12px;
  background: white;
  transition: all 0.3s ease;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.08);
}

.message-item:hover {
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.12);
  transform: translateY(-1px);
  border-color: var(--el-color-primary-light-7);
}

.message-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 14px;
}

.user-info {
  display: flex;
  align-items: center;
  gap: 10px;
}

.username {
  font-weight: 600;
  color: var(--el-text-color-primary);
  font-size: 15px;
}

.dept-tag {
  font-size: 12px;
  color: var(--el-text-color-secondary);
  background: var(--el-color-info-light-9);
  padding: 3px 8px;
  border-radius: 12px;
  border: 1px solid var(--el-color-info-light-7);
}

.message-meta {
  display: flex;
  gap: 8px;
}

.message-type-tag,
.reply-status-tag {
  font-size: 12px;
  font-weight: 500;
}

.message-content {
  color: var(--el-text-color-regular);
  line-height: 1.7;
  margin-bottom: 14px;
  font-size: 14px;
}

.message-main-content {
  margin-bottom: 8px;
}

.message-reason,
.message-remarks {
  margin-top: 8px;
  font-size: 13px;
  color: var(--el-text-color-regular);
}

.message-footer {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.message-times {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.message-time {
  font-size: 12px;
  color: var(--el-text-color-secondary);
  font-weight: 500;
}

.urgency-level-tag,
.negotiation-type-tag,
.urgent-status-tag,
.negotiation-status-tag {
  margin-left: 4px;
  font-weight: 500;
}

.reply-btn {
  font-size: 12px !important;
  padding: 4px 12px !important;
  height: auto !important;
  border-radius: 12px !important;
  font-weight: 500 !important;
  transition: all 0.3s ease !important;
}

.reply-btn:hover {
  transform: translateY(-1px);
  box-shadow: 0 2px 6px rgba(64, 158, 255, 0.3);
}

/* 回复区域 */
.reply-section {
  padding: 20px;
  border-top: 1px solid var(--el-border-color-light);
  background: white;
}

.quick-reply-label {
  font-size: 13px;
  font-weight: 500;
  color: var(--el-text-color-secondary);
  margin-bottom: 12px;
  display: flex;
  align-items: center;
}

.reply-actions {
  display: flex;
  gap: 10px;
  margin-bottom: 12px;
}

.quick-reply-btn {
  border-radius: 16px !important;
  font-size: 12px !important;
  padding: 6px 12px !important;
  height: auto !important;
  font-weight: 500 !important;
  transition: all 0.3s ease !important;
}

.quick-reply-btn:hover {
  transform: translateY(-1px);
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.15);
}

.reply-input {
  position: relative;
}

.reply-textarea {
  margin-bottom: 10px;
}

.reply-textarea :deep(.el-textarea__inner) {
  border-radius: 8px;
  border: 1px solid var(--el-border-color);
  transition: all 0.3s ease;
  max-height: 200px;
  resize: vertical;
}

.reply-textarea :deep(.el-textarea__inner):focus {
  border-color: var(--el-color-primary);
  box-shadow: 0 0 0 2px rgba(64, 158, 255, 0.1);
}

.input-actions {
  display: flex;
  justify-content: flex-end;
  align-items: center;
}

.input-hint {
  font-size: 12px;
  color: var(--el-text-color-secondary);
  text-align: center;
  margin-top: 10px;
  font-style: italic;
}

/* 滚动条样式 */
.message-list::-webkit-scrollbar {
  width: 6px;
}

.message-list::-webkit-scrollbar-track {
  background: var(--el-bg-color-page);
  border-radius: 3px;
}

.message-list::-webkit-scrollbar-thumb {
  background: var(--el-border-color);
  border-radius: 3px;
}

.message-list::-webkit-scrollbar-thumb:hover {
  background: var(--el-border-color-dark);
}

/* 回复列表样式 */
.replies-section {
  margin-top: 16px;
  padding-top: 12px;
  border-top: 1px solid var(--el-border-color-lighter);
}

.replies-header {
  margin-bottom: 10px;
}

.replies-title-wrapper {
  display: flex;
  align-items: center;
  gap: 4px;
  cursor: pointer;
  user-select: none;
  transition: color 0.3s ease;
}

.replies-title-wrapper:hover {
  color: var(--el-color-primary);
}

.replies-title-wrapper:hover .replies-title,
.replies-title-wrapper:hover .replies-count,
.replies-title-wrapper:hover .collapse-icon {
  color: var(--el-color-primary);
}

.replies-title {
  font-size: 13px;
  font-weight: 600;
  color: var(--el-text-color-secondary);
}

.replies-count {
  font-size: 12px;
  color: var(--el-text-color-secondary);
}

.collapse-icon {
  font-size: 12px;
  color: var(--el-text-color-secondary);
  transition: transform 0.3s ease;
  transform: rotate(0deg); /* 默认展开，箭头向下 */
}

.collapse-icon.collapsed {
  transform: rotate(-90deg); /* 折叠时，箭头向右 */
}

.replies-list {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.reply-item {
  background: var(--el-bg-color-page);
  border-radius: 8px;
  padding: 12px;
  border: 1px solid var(--el-border-color-lighter);
  transition: all 0.3s ease;
}

.reply-item:hover {
  background: var(--el-color-primary-light-9);
  border-color: var(--el-color-primary-light-7);
}

.reply-header {
  display: flex;
  align-items: center;
  gap: 10px;
  margin-bottom: 8px;
  font-size: 12px;
}

.reply-username {
  font-weight: 600;
  color: var(--el-color-primary);
}

.reply-business-type {
  color: var(--el-text-color-secondary);
  background: white;
  padding: 1px 6px;
  border-radius: 6px;
  border: 1px solid var(--el-border-color-lighter);
}

.reply-phone {
  color: var(--el-text-color-secondary);
  margin-left: auto;
}

.reply-time {
  color: var(--el-text-color-secondary);
  margin-left: auto;
}

.reply-content {
  color: var(--el-text-color-regular);
  line-height: 1.5;
  font-size: 13px;
}

.reply-main-content,
.reply-progress,
.reply-delivery,
.reply-remarks {
  margin-top: 6px;
  padding: 4px 8px;
  border-radius: 4px;
  font-size: 12px;
}

.reply-main-content {
  margin-top: 0;
}
</style>
