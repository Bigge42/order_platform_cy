<template>
  <CompDialog
    v-model="visible"
    title="选择人员"
    :width="1000"
    :height="600"
    icon="el-icon-user"
    @update:modelValue="handleClose"
  >
    <template #content>
      <div class="person-selector-container">
        <!-- 左侧组织树区域 -->
        <div class="left-panel">
          <div class="panel-header">
            <h4>部门</h4>
            <el-input
              v-model="orgSearchText"
              placeholder="搜索部门"
              size="small"
              clearable
              class="search-input"
            >
              <template #prefix>
                <el-icon><Search /></el-icon>
              </template>
            </el-input>
          </div>
          <div class="tree-container">
            <div v-if="orgLoading" class="loading-container">
              <el-icon class="is-loading">
                <Loading />
              </el-icon>
              <span>加载部门数据中...</span>
            </div>
            <el-tree
              v-else
              ref="treeRef"
              :data="filteredOrgs"
              :props="treeProps"
              node-key="id"
              :default-expanded-keys="defaultExpandedKeys"
              :current-node-key="selectedOrgId"
              :highlight-current="true"
              :expand-on-click-node="false"
              :filter-node-method="filterNode"
              @node-click="handleNodeClick"
              class="org-tree"
            >
              <template #default="{ node, data }">
                <div class="tree-node-content">
                  <el-icon class="node-icon">
                    <component :is="getNodeIcon(data)" />
                  </el-icon>
                  <span class="node-label">{{ node.label }}</span>
                </div>
              </template>
            </el-tree>
          </div>
        </div>

        <!-- 中间人员列表区域 -->
        <div class="middle-panel">
          <div class="panel-header">
            <h4>人员</h4>
            <el-input
              v-model="personSearchText"
              placeholder="搜索人员"
              size="small"
              clearable
              class="search-input"
            >
              <template #prefix>
                <el-icon><Search /></el-icon>
              </template>
            </el-input>
          </div>
          <div class="person-list-container">
            <div v-if="personLoading" class="loading-container">
              <el-icon class="is-loading">
                <Loading />
              </el-icon>
              <span>加载人员数据中...</span>
            </div>
            <div v-else-if="filteredPersons.length === 0" class="empty-person">
              <el-icon><User /></el-icon>
              <span>{{ selectedOrgId === 'company' ? '请选择部门查看人员' : '该部门暂无人员' }}</span>
            </div>
            <div 
              v-else
              v-for="person in filteredPersons" 
              :key="person.id"
              class="person-item"
              :class="{ 'selected': selectedPersonId === person.id }"
              @click="selectPerson(person)"
            >
              <div class="person-avatar">
                <el-icon :style="{ color: person.avatarColor, fontSize: '18px' }">
                  <Avatar />
                </el-icon>
              </div>
              <div class="person-info">
                <span class="person-name">{{ person.name }}</span>
              </div>
              <div v-if="selectedPersonId === person.id" class="selected-indicator">
                <el-icon color="#67c23a"><Check /></el-icon>
              </div>
            </div>
          </div>
        </div>

        <!-- 右侧已选人员区域 -->
        <div class="right-panel">
          <div class="panel-header">
            <h4>已选择人员</h4>
          </div>
          <div class="selected-person-list">
            <div 
              v-if="selectedPerson" 
              class="selected-person-item"
            >
              <div class="person-avatar">
                <el-icon :style="{ color: selectedPerson.avatarColor, fontSize: '16px' }">
                  <Avatar />
                </el-icon>
              </div>
              <div class="person-detail">
                <span class="person-name">{{ selectedPerson.name }}</span>
              </div>
              <el-button 
                type="danger"
                link
                size="small" 
                @click="clearSelection"
                class="remove-btn"
              >
                <el-icon><Close /></el-icon>
              </el-button>
            </div>
            <div v-else class="empty-selection">
              <el-icon><User /></el-icon>
              <span>暂未选择人员</span>
            </div>
          </div>
        </div>
      </div>
    </template>

    <template #footer>
      <div class="dialog-footer">
        <el-button @click="handleCancel">取消</el-button>
        <el-button type="primary" @click="handleConfirm">确定</el-button>
      </div>
    </template>
  </CompDialog>
</template>

<script setup>
import { ref, computed, watch, onMounted, nextTick } from 'vue'
import { Search, Avatar, Close, OfficeBuilding, Sell, User, Cpu, DataLine, Goods, Check, Loading } from '@element-plus/icons-vue'
import CompDialog from '@/comp/dialog/index.vue'
import http from '@/api/http'

// Props
const props = defineProps({
  modelValue: {
    type: Boolean,
    default: true
  },
  selectedPersonId: {
    type: String,
    default: ''
  }
})

// Emits
const emit = defineEmits(['update:modelValue', 'confirm', 'cancel'])

// 响应式数据
const visible = ref(false)
const orgSearchText = ref('')
const personSearchText = ref('')
const selectedOrgId = ref('company')
const selectedPersonId = ref('')
const treeRef = ref()
const orgLoading = ref(false)
const personLoading = ref(false)

// Tree 组件配置
const treeProps = {
  children: 'children',
  label: 'name'
}

const defaultExpandedKeys = ['company']

// 组织数据
const orgList = ref([])

// 人员数据
const personList = ref([])

// 头像颜色池
const avatarColors = ['#f56c6c', '#67c23a', '#409eff', '#e6a23c', '#909399', '#f78989', '#95d475', '#73b9ff', '#f2ca73', '#b1b8c3']

// 计算属性
const filteredOrgs = computed(() => {
  return orgList.value
})

const filteredPersons = computed(() => {
  let persons = personList.value
  
  // 根据搜索文本过滤
  if (personSearchText.value) {
    persons = persons.filter(person => 
      person.name.toLowerCase().includes(personSearchText.value.toLowerCase())
    )
  }
  
  return persons
})

const selectedPerson = computed(() => {
  return personList.value.find(person => person.id === selectedPersonId.value)
})

// API方法
const fetchDepartmentTree = async () => {
  try {
    orgLoading.value = true
    const response = await http.post('api/Sys_Department/getDepartmentTree', {}, '加载部门数据...')
    
    if (response && response.status !== false) {
      // 根据实际API返回的数据结构进行处理
      // 假设API返回的是数组格式的部门数据
      const departments = response.data || response
      
      // 处理数据格式，确保符合树组件的要求
      if (Array.isArray(departments) && departments.length > 0) {
        orgList.value = formatDepartmentData(departments)
      } else {
        // 如果没有数据，使用默认结构
        orgList.value = [{
          id: 'company',
          name: '公司',
          type: 'company',
          children: []
        }]
      }
    } else {
      console.error('获取部门数据失败:', response.message || '未知错误')
      // 出错时使用默认数据
      orgList.value = [{
        id: 'company',
        name: '公司',
        type: 'company',
        children: []
      }]
    }
  } catch (error) {
    console.error('调用部门接口异常:', error)
    // 出错时使用默认数据
    orgList.value = [{
      id: 'company',
      name: '公司',
      type: 'company',
      children: []
    }]
  } finally {
    orgLoading.value = false
  }
}

// 格式化部门数据，将API返回的数据转换为树形结构
const formatDepartmentData = (departments) => {
  // 根据API实际返回的数据结构进行格式化
  // 这里假设API返回的是扁平化的部门数据，需要转换为树形结构
  
  if (!departments || !Array.isArray(departments)) {
    return []
  }
  
  // 如果API返回的已经是树形结构，直接使用
  if (departments.length > 0 && departments[0].children !== undefined) {
    return departments.map(dept => ({
      id: dept.id || dept.Id || dept.departmentId,
      name: dept.name || dept.Name || dept.departmentName,
      type: dept.type || 'department',
      children: dept.children || []
    }))
  }
  
  // 如果是扁平化数据，需要构建树形结构
  const deptMap = new Map()
  const rootNodes = []
  
  // 第一遍遍历，创建所有节点
  departments.forEach(dept => {
    const node = {
      id: dept.id || dept.Id || dept.departmentId,
      name: dept.name || dept.Name || dept.departmentName,
      type: dept.type || 'department',
      parentId: dept.parentId || dept.ParentId,
      children: []
    }
    deptMap.set(node.id, node)
  })
  
  // 第二遍遍历，建立父子关系
  deptMap.forEach(node => {
    if (node.parentId && deptMap.has(node.parentId)) {
      deptMap.get(node.parentId).children.push(node)
    } else {
      rootNodes.push(node)
    }
  })
  
  return rootNodes.length > 0 ? rootNodes : [{
    id: 'company',
    name: '公司',
    type: 'company',
    children: Array.from(deptMap.values())
  }]
}

// 获取指定部门的人员数据
const fetchPersonsByDepartment = async (departmentId) => {
  try {
    personLoading.value = true
    personList.value = [] // 清空当前人员列表
    
    if (!departmentId || departmentId === 'company') {
      // 如果选择的是公司根节点，不加载人员数据
      return
    }
    
    const response = await http.post('api/Sys_User/getUsersByDepartment', {}, '加载人员数据...', {
      params: {
        departmentId: departmentId
      }
    })
    
    if (response && response.success) {
      const users = response.data
      
      if (Array.isArray(users)) {
        // 处理人员数据，添加头像颜色
        personList.value = users.map((user, index) => ({
          id: user.userName, // 使用userName作为唯一标识
          name: user.userTrueName, // 显示格式：只显示userTrueName
          userName: user.userName,
          userTrueName: user.userTrueName,
          phoneNo: user.phoneNo,
          email: user.email,
          userId: user.userId,
          deptId: departmentId, // 使用传入的部门ID
          avatarColor: avatarColors[index % avatarColors.length]
        }))
      }
    } else {
      console.error('获取人员数据失败:', response.message || '未知错误')
      personList.value = []
    }
  } catch (error) {
    console.error('调用人员接口异常:', error)
    personList.value = []
  } finally {
    personLoading.value = false
  }
}

// 方法
const getNodeIcon = (data) => {
  const iconMap = {
    company: OfficeBuilding,
    department: User
  }
  
  // 根据部门名称返回不同图标
  if (data.name.includes('销售')) return Sell
  if (data.name.includes('研发')) return Cpu
  if (data.name.includes('市场')) return DataLine
  if (data.name.includes('生产')) return Goods
  
  return iconMap[data.type] || User
}

const filterNode = (value, data) => {
  if (!value) return true
  return data.name.toLowerCase().includes(value.toLowerCase())
}

const handleNodeClick = async (data) => {
  selectedOrgId.value = data.id
  selectedPersonId.value = '' // 清空当前选中的人员
  
  // 获取选中部门的人员数据
  await fetchPersonsByDepartment(data.id)
}

const selectPerson = (person) => {
  selectedPersonId.value = person.id
}

const clearSelection = () => {
  selectedPersonId.value = ''
}

const handleClose = (value) => {
  visible.value = value
  emit('update:modelValue', value)
}

const handleCancel = () => {
  visible.value = false
  emit('update:modelValue', false)
  emit('cancel')
}

const handleConfirm = () => {
  const selectedData = selectedPerson.value ? {
    id: selectedPerson.value.id, // userName
    name: selectedPerson.value.name, // userTrueName
    userName: selectedPerson.value.userName,
    userTrueName: selectedPerson.value.userTrueName,
    phoneNo: selectedPerson.value.phoneNo,
    email: selectedPerson.value.email,
    userId: selectedPerson.value.userId,
    deptId: selectedPerson.value.deptId
  } : null
  
  emit('confirm', selectedData)
  visible.value = false
  emit('update:modelValue', false)
}

// 监听搜索文本变化，触发树组件过滤
watch(orgSearchText, (val) => {
  if (treeRef.value) {
    treeRef.value.filter(val)
  }
})

// 监听 props 变化
watch(() => props.modelValue, (newVal) => {
  visible.value = newVal
})

watch(() => props.selectedPersonId, (newVal) => {
  selectedPersonId.value = newVal || ''
}, { immediate: true })

// 初始化
onMounted(async () => {
  // 默认选中公司
  selectedOrgId.value = 'company'
  
  // 获取部门数据
  await fetchDepartmentTree()
})
</script>

<style scoped>
.person-selector-container {
  display: flex;
  height: 100%;
  gap: 16px;
}

.left-panel, .middle-panel, .right-panel {
  display: flex;
  flex-direction: column;
  border: 1px solid #e4e7ed;
  border-radius: 6px;
  background-color: #fff;
}

.left-panel {
  width: 280px;
}

.middle-panel {
  flex: 1;
}

.right-panel {
  width: 280px;
}

.panel-header {
  padding: 16px;
  border-bottom: 1px solid #e4e7ed;
  background-color: #fafbfc;
  border-top-left-radius: 6px;
  border-top-right-radius: 6px;
}

.panel-header h4 {
  margin: 0 0 12px 0;
  font-size: 14px;
  font-weight: 600;
  color: #303133;
}

.search-input {
  --el-input-border-radius: 6px;
}

.search-input :deep(.el-input__wrapper) {
  border-radius: 6px;
  box-shadow: 0 0 0 1px #dcdfe6;
  transition: all 0.2s;
}

.search-input :deep(.el-input__wrapper:hover) {
  box-shadow: 0 0 0 1px #c0c4cc;
}

.search-input :deep(.el-input__wrapper.is-focus) {
  box-shadow: 0 0 0 1px #409eff;
}

.tree-container {
  flex: 1;
  overflow-y: auto;
  padding: 8px;
}

.org-tree {
  background-color: transparent;
}

.org-tree :deep(.el-tree-node__content) {
  height: 36px;
  border-radius: 4px;
  margin-bottom: 2px;
  transition: all 0.2s;
}

.org-tree :deep(.el-tree-node__content:hover) {
  background-color: #f0f9ff;
}

.org-tree :deep(.is-current > .el-tree-node__content) {
  background-color: #e1f3d8;
  color: #67c23a;
  font-weight: 500;
}

.tree-node-content {
  display: flex;
  align-items: center;
  width: 100%;
}

.node-icon {
  margin-right: 8px;
  font-size: 16px;
}

.node-label {
  font-size: 14px;
}

.person-list-container {
  flex: 1;
  overflow-y: auto;
  padding: 8px;
}

.person-item {
  display: flex;
  align-items: center;
  padding: 10px 12px;
  cursor: pointer;
  border-radius: 6px;
  margin-bottom: 4px;
  transition: all 0.2s;
  border: 1px solid transparent;
  position: relative;
}

.person-item:hover {
  background-color: #f0f9ff;
  border-color: #d9ecff;
}

.person-item.selected {
  background-color: #e1f3d8;
  border-color: #b3d8a0;
}

.person-avatar {
  margin-right: 12px;
}

.person-info {
  flex: 1;
}

.person-name {
  font-size: 14px;
  color: #303133;
  font-weight: 500;
}

.selected-indicator {
  margin-left: 8px;
}

.selected-person-list {
  flex: 1;
  overflow-y: auto;
  padding: 8px;
}

.selected-person-item {
  display: flex;
  align-items: center;
  padding: 10px 12px;
  border-radius: 6px;
  margin-bottom: 6px;
  background-color: #f0f9ff;
  border: 1px solid #d9ecff;
  transition: all 0.2s;
}

.selected-person-item:hover {
  background-color: #e1f3d8;
  border-color: #b3d8a0;
}

.person-detail {
  flex: 1;
  margin-left: 12px;
}

.person-detail .person-name {
  font-size: 14px;
  color: #303133;
  font-weight: 500;
  display: block;
}

.person-detail .person-info {
  font-size: 12px;
  color: #909399;
  margin-top: 2px;
  display: block;
}

.remove-btn {
  color: #f56c6c;
  padding: 4px;
  margin-left: 8px;
  border-radius: 4px;
  transition: all 0.2s;
}

.remove-btn:hover {
  color: #fff;
  background-color: #f56c6c;
}

.empty-selection {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 40px 20px;
  color: #909399;
  font-size: 14px;
}

.empty-selection .el-icon {
  font-size: 32px;
  margin-bottom: 8px;
}

.empty-person {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 40px 20px;
  color: #909399;
  font-size: 14px;
}

.empty-person .el-icon {
  font-size: 32px;
  margin-bottom: 8px;
}

.loading-container {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 40px 20px;
  color: #909399;
  font-size: 14px;
}

.loading-container .el-icon {
  font-size: 24px;
  margin-bottom: 8px;
}

.dialog-footer {
  display: flex;
  justify-content: flex-end;
  gap: 12px;
  padding: 16px 20px;
  border-top: 1px solid #e4e7ed;
  background-color: #fafbfc;
}
</style>
