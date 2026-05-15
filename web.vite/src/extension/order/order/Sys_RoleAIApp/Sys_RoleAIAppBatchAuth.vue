<template>
  <div>
    <vol-box v-model="roleModel" title="按角色授权智能体" :width="980" :height="620" :padding="16">
      <div class="batch-auth">
        <div class="selector-row">
          <span class="selector-label">角色</span>
          <el-select
            v-model="roleId"
            filterable
            clearable
            placeholder="请选择角色"
            class="selector"
            @change="loadAIAppsByRole"
          >
            <el-option v-for="item in roles" :key="item.key" :label="item.label" :value="item.key" />
          </el-select>
        </div>
        <el-transfer
          v-model="selectedAIAppIds"
          v-loading="loading"
          filterable
          :data="aiApps"
          :titles="['可选智能体', '已授权智能体']"
        />
      </div>
      <template #footer>
        <div class="dialog-footer">
          <el-button @click="roleModel = false">关闭</el-button>
          <el-button type="primary" :loading="saving" @click="saveByRole">保存</el-button>
        </div>
      </template>
    </vol-box>

    <vol-box v-model="aiAppModel" title="按智能体授权角色" :width="980" :height="620" :padding="16">
      <div class="batch-auth">
        <div class="selector-row">
          <span class="selector-label">智能体</span>
          <el-select
            v-model="aiAppId"
            filterable
            clearable
            placeholder="请选择智能体"
            class="selector"
            @change="loadRolesByAIApp"
          >
            <el-option v-for="item in aiApps" :key="item.key" :label="item.label" :value="item.key" />
          </el-select>
        </div>
        <el-transfer
          v-model="selectedRoleIds"
          v-loading="loading"
          filterable
          :data="roles"
          :titles="['可选角色', '已授权角色']"
        />
      </div>
      <template #footer>
        <div class="dialog-footer">
          <el-button @click="aiAppModel = false">关闭</el-button>
          <el-button type="primary" :loading="saving" @click="saveByAIApp">保存</el-button>
        </div>
      </template>
    </vol-box>
  </div>
</template>

<script setup>
import VolBox from '@/components/basic/VolBox.vue'
import { getCurrentInstance, ref } from 'vue'

const emit = defineEmits(['parentCall'])
const { proxy } = getCurrentInstance()

const roleModel = ref(false)
const aiAppModel = ref(false)
const loading = ref(false)
const saving = ref(false)
const loaded = ref(false)
const gridRef = ref(null)

const roles = ref([])
const aiApps = ref([])
const roleId = ref(null)
const aiAppId = ref(null)
const selectedAIAppIds = ref([])
const selectedRoleIds = ref([])

emit('parentCall', ($grid) => {
  gridRef.value = $grid
})

const normalizeTransferItem = (item = {}) => {
  const type = item.appType ? ` (${item.appType})` : ''
  return {
    key: item.key,
    label: `${item.label || ''}${type}`,
    disabled: false
  }
}

const loadOptions = async () => {
  if (loaded.value) return
  loading.value = true
  try {
    const result = await proxy.http.get('api/Sys_RoleAIApp/GetBatchOptions', {}, true)
    if (!result.status) {
      proxy.$message.error(result.message)
      return
    }
    const data = result.data || {}
    roles.value = (data.roles || []).map((x) => ({
      key: x.key,
      label: x.label,
      disabled: false
    }))
    aiApps.value = (data.apps || []).map(normalizeTransferItem)
    loaded.value = true
  } finally {
    loading.value = false
  }
}

const openByRole = async () => {
  roleModel.value = true
  selectedAIAppIds.value = []
  await loadOptions()
  if (roleId.value) {
    await loadAIAppsByRole()
  }
}

const openByAIApp = async () => {
  aiAppModel.value = true
  selectedRoleIds.value = []
  await loadOptions()
  if (aiAppId.value) {
    await loadRolesByAIApp()
  }
}

const loadAIAppsByRole = async () => {
  selectedAIAppIds.value = []
  if (!roleId.value) return
  loading.value = true
  try {
    const result = await proxy.http.get(
      `api/Sys_RoleAIApp/GetAIAppsByRole?roleId=${roleId.value}`,
      {},
      true
    )
    if (!result.status) {
      proxy.$message.error(result.message)
      return
    }
    selectedAIAppIds.value = result.data || []
  } finally {
    loading.value = false
  }
}

const loadRolesByAIApp = async () => {
  selectedRoleIds.value = []
  if (!aiAppId.value) return
  loading.value = true
  try {
    const result = await proxy.http.get(
      `api/Sys_RoleAIApp/GetRolesByAIApp?aiAppId=${aiAppId.value}`,
      {},
      true
    )
    if (!result.status) {
      proxy.$message.error(result.message)
      return
    }
    selectedRoleIds.value = result.data || []
  } finally {
    loading.value = false
  }
}

const refreshGrid = () => {
  if (gridRef.value && typeof gridRef.value.search === 'function') {
    gridRef.value.search()
  }
}

const saveByRole = async () => {
  if (!roleId.value) {
    return proxy.$message.warning('请选择角色')
  }
  saving.value = true
  try {
    const result = await proxy.http.post(
      'api/Sys_RoleAIApp/SaveAIAppsByRole',
      {
        roleId: roleId.value,
        aiAppIds: selectedAIAppIds.value
      },
      true
    )
    proxy.$message[result.status ? 'success' : 'error'](result.message)
    if (result.status) {
      roleModel.value = false
      refreshGrid()
    }
  } finally {
    saving.value = false
  }
}

const saveByAIApp = async () => {
  if (!aiAppId.value) {
    return proxy.$message.warning('请选择智能体')
  }
  saving.value = true
  try {
    const result = await proxy.http.post(
      'api/Sys_RoleAIApp/SaveRolesByAIApp',
      {
        aiAppId: aiAppId.value,
        roleIds: selectedRoleIds.value
      },
      true
    )
    proxy.$message[result.status ? 'success' : 'error'](result.message)
    if (result.status) {
      aiAppModel.value = false
      refreshGrid()
    }
  } finally {
    saving.value = false
  }
}

defineExpose({
  openByRole,
  openByAIApp
})
</script>

<style scoped>
.batch-auth {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.selector-row {
  display: flex;
  align-items: center;
  gap: 12px;
}

.selector-label {
  width: 56px;
  color: #303133;
  font-weight: 600;
}

.selector {
  width: 420px;
}

.dialog-footer {
  text-align: center;
}

:deep(.el-transfer) {
  display: flex;
  align-items: center;
  justify-content: center;
}

:deep(.el-transfer-panel) {
  width: 360px;
}
</style>
