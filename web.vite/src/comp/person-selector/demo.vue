<template>
  <div class="demo-container">
    <h2>人员选择组件演示</h2>
    
    <el-button type="primary" @click="showPersonSelector">
      打开人员选择器
    </el-button>
    
    <div v-if="selectedPersons.length > 0" class="selected-result">
      <h3>已选择的人员：</h3>
      <ul>
        <li v-for="person in selectedPersons" :key="person.id">
          {{ person.name }}（{{ person.deptName }}）
        </li>
      </ul>
    </div>
    
    <!-- 人员选择器组件 -->
    <PersonSelector
      v-model="visible"
      :selectedPersonIds="selectedPersonIds"
      @confirm="handleConfirm"
      @cancel="handleCancel"
    />
  </div>
</template>

<script setup>
import { ref } from 'vue'
import PersonSelector from './index.vue'

const visible = ref(false)
const selectedPersons = ref([])
const selectedPersonIds = ref([])

const showPersonSelector = () => {
  visible.value = true
}

const handleConfirm = (persons) => {
  selectedPersons.value = persons
  selectedPersonIds.value = persons.map(p => p.id)
  console.log('选择的人员:', persons)
}

const handleCancel = () => {
  console.log('取消选择')
}
</script>

<style scoped>
.demo-container {
  padding: 20px;
}

.selected-result {
  margin-top: 20px;
  padding: 16px;
  background-color: #f5f7fa;
  border-radius: 4px;
}

.selected-result h3 {
  margin-top: 0;
  color: #303133;
}

.selected-result ul {
  margin: 0;
  padding-left: 20px;
}

.selected-result li {
  margin-bottom: 4px;
  color: #606266;
}
</style> 