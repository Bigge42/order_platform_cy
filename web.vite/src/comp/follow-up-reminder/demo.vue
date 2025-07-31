<template>
  <div class="demo-container">
    <h2>催单弹窗组件演示</h2>
    
    <el-button type="primary" @click="showFollowUpDialog">
      打开催单弹窗
    </el-button>
    
    <div v-if="lastSentData.length > 0" class="result-display">
      <h3>最后一次发送的数据：</h3>
      <el-table :data="lastSentData" stripe style="width: 100%">
        <el-table-column prop="contractNo" label="合同号" width="150" />
        <el-table-column prop="planTrackNo" label="计划跟踪号" width="150" />
        <el-table-column prop="business" label="业务" width="120" />
        <el-table-column prop="defaultResponsible" label="默认负责人" width="120" />
        <el-table-column label="指定负责人" width="200">
          <template #default="{ row }">
            <el-tag v-for="person in row.assignedPersons" :key="person.id" size="small" type="info">
              {{ person.name }}
            </el-tag>
          </template>
        </el-table-column>
        <el-table-column prop="urgentLevel" label="紧急等级" width="120" />
        <el-table-column prop="replyDays" label="回复天数" width="100" />
        <el-table-column prop="messageContent" label="消息内容" show-overflow-tooltip />
      </el-table>
    </div>
    
    <!-- 催单弹窗组件 -->
    <FollowUpReminder
      v-model="dialogVisible"
      :data="mockData"
      @send="handleSend"
    />
  </div>
</template>

<script setup>
import { ref } from 'vue'
import FollowUpReminder from './index.vue'

const dialogVisible = ref(false)
const lastSentData = ref([])

// 模拟数据
const mockData = ref([
  {
    id: 1,
    contractNo: '3264846521',
    planTrackNo: '计划跟踪号1',
    business: 'sales',
    defaultResponsible: '张三',
    assignedPersons: [],
    urgentLevel: 'A',
    replyDays: 3,
    messageContent: '这是一条消息内容这是一条消息内容这是一条消息内容这是一条消息内容'
  },
  {
    id: 2,
    contractNo: '3264846522',
    planTrackNo: '计划跟踪号2',
    business: 'purchase',
    defaultResponsible: '李四',
    assignedPersons: [],
    urgentLevel: 'B',
    replyDays: 1,
    messageContent: ''
  },
  {
    id: 3,
    contractNo: '3264846523',
    planTrackNo: '计划跟踪号3',
    business: '',
    defaultResponsible: '王五',
    assignedPersons: [],
    urgentLevel: '',
    replyDays: null,
    messageContent: ''
  },
  // 添加更多数据用于测试分页
  ...Array.from({ length: 50 }, (_, index) => ({
    id: index + 4,
    contractNo: `3264846${524 + index}`,
    planTrackNo: `计划跟踪号${index + 4}`,
    business: '',
    defaultResponsible: `用户${index + 4}`,
    assignedPersons: [],
    urgentLevel: '',
    replyDays: null,
    messageContent: ''
  }))
])

const showFollowUpDialog = () => {
  dialogVisible.value = true
}

const handleSend = (selectedData) => {
  lastSentData.value = selectedData
  console.log('发送催单数据:', selectedData)
  
  // 模拟API调用
  console.log('发送的数据包含以下信息：')
  selectedData.forEach((item, index) => {
    console.log(`${index + 1}. 合同号：${item.contractNo}`)
    console.log(`   计划跟踪号：${item.planTrackNo}`)
    console.log(`   业务：${item.business}`)
    console.log(`   默认负责人：${item.defaultResponsible}`)
    console.log(`   指定负责人：${item.assignedPersons.map(p => p.name).join(', ') || '无'}`)
    console.log(`   紧急等级：${item.urgentLevel}`)
    console.log(`   回复天数：${item.replyDays || '未设置'}`)
    console.log(`   消息内容：${item.messageContent}`)
    console.log('---')
  })
  
  // 这里可以调用API发送数据
  // api.sendFollowUpReminder(selectedData)
}
</script>

<style scoped>
.demo-container {
  padding: 20px;
}

.result-display {
  margin-top: 20px;
  padding: 16px;
  background-color: #f5f7fa;
  border-radius: 4px;
}

.result-display h3 {
  margin-top: 0;
  color: #303133;
}
</style> 