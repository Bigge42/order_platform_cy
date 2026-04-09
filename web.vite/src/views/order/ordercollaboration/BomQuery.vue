<template>
  <div class="bom-query-container">
    <div class="query-header">
      <div class="query-input">
        <el-input
          v-model="materialCode"
          placeholder="请输入物料编码"
          clearable
          class="material-code-input"
          @keyup.enter="handleQuery"
        >
          <template #prepend>物料编码</template>
        </el-input>
        <el-button type="primary" :loading="loading" @click="handleQuery">
          查询
        </el-button>
      </div>
    </div>

    <div class="bom-content">
      <div v-show="!bomTreeCollapsed" class="bom-left" :style="{ width: `${bomTreeWidth}px` }">
        <div class="bom-tree-title">
          <span>BOM结构</span>
          <el-button
            text
            :icon="ArrowLeft"
            class="collapse-btn"
            title="收起BOM结构"
            @click="bomTreeCollapsed = true"
          />
        </div>
        <el-scrollbar class="bom-tree-scrollbar">
          <el-tree
            v-if="hasData"
            ref="treeRef"
            :data="bomTreeData"
            :props="treeProps"
            node-key="entryId"
            :default-expand-all="true"
            :highlight-current="true"
            class="bom-tree"
            @node-click="handleNodeClick"
          >
            <template #default="{ data }">
              <span class="custom-tree-node">
                <span class="tree-label">
                  {{ data.number }} / {{ data.name }} / {{ data.requiredQtyDisplay || '--' }}
                </span>
              </span>
            </template>
          </el-tree>
          <el-empty
            v-else
            description="请输入物料编码后查询BOM结构"
            :image-size="100"
          />
        </el-scrollbar>
      </div>

      <div
        v-show="!bomTreeCollapsed"
        class="panel-resizer"
        title="拖动调整BOM结构宽度"
        @mousedown.prevent="startBomTreeResize"
      />

      <div v-show="bomTreeCollapsed" class="bom-left-collapsed">
        <el-button
          text
          :icon="ArrowRight"
          class="expand-btn"
          title="展开BOM结构"
          @click="bomTreeCollapsed = false"
        />
      </div>

      <div class="bom-right">
        <div v-show="!materialInfoCollapsed" class="material-info">
          <div class="info-title">
            <div class="info-title-main">
              <span>物料信息</span>
              <span v-if="currentMaterialCode" class="info-subtitle">
                {{ currentMaterialCode }}
              </span>
            </div>
            <div class="info-actions">
              <el-button
                v-if="hasExportPermission"
                link
                type="primary"
                :disabled="!currentMaterialCode"
                :loading="exportingCurrentMaterial"
                @click="exportCurrentMaterial"
              >
                导出当前物料
              </el-button>
              <el-button
                v-if="hasExportPermission"
                link
                type="primary"
                :disabled="!hasData"
                :loading="exportingBomMaterials"
                @click="exportBomMaterials"
              >
                导出BOM全部物料
              </el-button>
              <el-button
                text
                :icon="ArrowUp"
                class="collapse-btn"
                title="收起物料信息"
                @click="materialInfoCollapsed = true"
              />
            </div>
          </div>

          <div class="material-info-grid">
            <div
              v-for="field in materialFields"
              :key="field.label"
              class="material-field-card"
              :style="{ width: `${field.width || 220}px` }"
            >
              <div class="material-field-label">{{ field.label }}</div>
              <div
                class="material-field-value"
                :title="getMaterialFieldValue(field) || '--'"
              >
                {{ getMaterialFieldValue(field) || '--' }}
              </div>
            </div>
          </div>
        </div>

        <div v-show="materialInfoCollapsed" class="material-info-collapsed">
          <el-button
            text
            :icon="ArrowDown"
            class="expand-btn"
            title="展开物料信息"
            @click="materialInfoCollapsed = false"
          >
            物料信息
          </el-button>
        </div>

        <div class="drawing-preview">
          <div class="info-title">
            <div class="info-title-main">
              <span>图纸</span>
              <span v-if="currentMaterial?.drawingNo" class="info-subtitle">
                {{ currentMaterial.drawingNo }}
              </span>
              <span v-if="currentMaterial?.nominalDiameter" class="info-subtitle">
                {{ currentMaterial.nominalDiameter }}
              </span>
            </div>
            <el-button
              text
              :icon="FullScreen"
              class="fullscreen-btn"
              title="全屏查看图纸"
              @click="toggleFullScreen"
            />
          </div>

          <div
            ref="drawingContentRef"
            v-loading="drawingLoading"
            class="drawing-content"
          >
            <div v-if="isFullscreen" class="fullscreen-exit-btn">
              <el-button
                type="danger"
                :icon="Close"
                size="large"
                round
                @click="exitFullScreen"
              >
                退出全屏（ESC）
              </el-button>
            </div>

            <iframe
              v-if="drawingUrl"
              ref="drawingIframeRef"
              :src="`${drawingUrl}#toolbar=0&navpanes=0&scrollbar=0`"
              frameborder="0"
              class="drawing-iframe"
            />

            <el-empty v-else-if="drawingError" :image-size="100">
              <template #description>
                <div class="drawing-error">
                  <el-icon :size="20" color="#f56c6c" style="margin-bottom: 8px">
                    <WarningFilled />
                  </el-icon>
                  <div class="error-message">{{ drawingError }}</div>
                </div>
              </template>
            </el-empty>

            <el-empty v-else description="暂无图纸" :image-size="100" />
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { computed, getCurrentInstance, nextTick, onMounted, onUnmounted, ref } from 'vue'
import {
  WarningFilled,
  FullScreen,
  ArrowLeft,
  ArrowRight,
  ArrowUp,
  ArrowDown,
  Close
} from '@element-plus/icons-vue'

const { proxy } = getCurrentInstance()

const excelMimeType =
  'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet'
const exportAuthorizedUsers = ['cyadmin', '002166', '013675', '004356']

const materialFields = [
  { label: '物料名称', keys: ['materialName', 'name'], width: 320 },
  { label: '公称通径', keys: ['nominalDiameter'], width: 220 },
  { label: '公称压力', keys: ['nominalPressure'], width: 220 },
  { label: 'CV', keys: ['cv', 'CV'], width: 180 },
  { label: '法兰标准', keys: ['flangeStandard'], width: 240 },
  { label: '法兰密封面形式', keys: ['flangeSealType'], width: 280 },
  { label: '阀体材质', keys: ['bodyMaterial'], width: 220 },
  { label: '阀内件材质', keys: ['trimMaterial'], width: 220 },
  { label: '流量特性', keys: ['flowCharacteristic'], width: 240 },
  { label: '填料形式', keys: ['packingForm'], width: 220 },
  { label: '法兰连接方式', keys: ['flangeConnection'], width: 260 },
  { label: '执行机构型号', keys: ['actuatorModel'], width: 260 },
  { label: '执行机构行程', keys: ['actuatorStroke'], width: 220 },
  { label: '图号', keys: ['drawingNo'], width: 220 },
  { label: '材质', keys: ['material', 'innerMaterial'], width: 220 },
  { label: 'TC发布人', keys: ['tcReleaser', 'TCReleaser'], width: 220 }
]

const materialCode = ref('')
const loading = ref(false)
const drawingLoading = ref(false)
const hasData = ref(false)
const bomTreeData = ref([])
const currentMaterial = ref(null)
const drawingUrl = ref('')
const drawingError = ref('')
const treeRef = ref(null)
const drawingContentRef = ref(null)
const drawingIframeRef = ref(null)

const bomTreeCollapsed = ref(false)
const materialInfoCollapsed = ref(false)
const isFullscreen = ref(false)
const exportingCurrentMaterial = ref(false)
const exportingBomMaterials = ref(false)
const bomTreeWidth = ref(320)

const treeProps = {
  children: 'children',
  label: 'label'
}

const currentMaterialCode = computed(() => {
  return currentMaterial.value?.number || currentMaterial.value?.materialCode || ''
})

const currentLoginName = computed(() => {
  const loginName = proxy?.$store?.getters?.getLoginName?.()
  if (loginName) {
    return String(loginName).trim()
  }

  const userInfo = proxy?.$store?.getters?.getUserInfo?.() || {}
  return String(userInfo?.userName || '').trim()
})

const hasExportPermission = computed(() => {
  const loginName = currentLoginName.value
  if (!loginName) {
    return false
  }

  return exportAuthorizedUsers.some(
    (user) => user.toLowerCase() === loginName.toLowerCase()
  )
})

const sanitizeFileName = (value) => {
  return String(value || '').replace(/[\\/:*?"<>|]+/g, '_')
}

const getTimestamp = () => {
  const now = new Date()
  const pad = (value) => `${value}`.padStart(2, '0')
  return `${now.getFullYear()}${pad(now.getMonth() + 1)}${pad(now.getDate())}${pad(
    now.getHours()
  )}${pad(now.getMinutes())}${pad(now.getSeconds())}`
}

const downloadBlobFile = (content, fileName) => {
  const blob =
    content instanceof Blob ? content : new Blob([content], { type: excelMimeType })
  const url = URL.createObjectURL(blob)
  const link = document.createElement('a')
  link.href = url
  link.download = fileName
  link.style.display = 'none'
  document.body.appendChild(link)
  link.click()
  URL.revokeObjectURL(url)
  document.body.removeChild(link)
}

const resolveExportBlob = async (content) => {
  const blob =
    content instanceof Blob ? content : new Blob([content], { type: excelMimeType })

  if (blob.type?.includes('application/json')) {
    const text = await blob.text()
    let message = '导出失败'
    try {
      const result = JSON.parse(text)
      message = result?.message || result?.Message || message
    } catch (error) {
      console.error('解析导出错误信息失败:', error)
    }
    throw new Error(message)
  }

  return blob
}

const getMaterialFieldValue = (field) => {
  const material = currentMaterial.value || {}
  for (const key of field.keys) {
    const value = material[key]
    if (value !== undefined && value !== null && String(value).trim() !== '') {
      return String(value)
    }
  }
  return ''
}

const loadQueryFallback = async (selectedMaterialCode) => {
  hasData.value = false
  bomTreeData.value = []
  currentMaterial.value = {
    number: selectedMaterialCode,
    materialCode: selectedMaterialCode
  }

  await Promise.all([loadMaterialInfo(selectedMaterialCode), loadDrawing(selectedMaterialCode)])
}

const handleQuery = async () => {
  const queryCode = materialCode.value.trim()
  if (!queryCode) {
    proxy.$message.warning('请输入物料编码')
    return
  }

  loading.value = true
  try {
    const result = await proxy.http.get(
      `api/BomQuery/ExpandBom?materialNumber=${encodeURIComponent(queryCode)}`
    )

    if (result.status && result.data) {
      bomTreeData.value = buildBomTree(result.data)
      hasData.value = bomTreeData.value.length > 0
      materialCode.value = queryCode

      await nextTick()
      if (bomTreeData.value.length > 0 && treeRef.value) {
        treeRef.value.setCurrentKey(bomTreeData.value[0].entryId)
        handleNodeClick(bomTreeData.value[0])
        return
      }
    }

    await loadQueryFallback(queryCode)
    proxy.$message.warning(result.message || 'BOM查询失败，已尝试加载图纸')
  } catch (error) {
    await loadQueryFallback(queryCode)
    console.error('BOM查询失败:', error)
    proxy.$message.error('BOM查询失败，已尝试加载图纸')
  } finally {
    loading.value = false
  }
}

const buildBomTree = (bomList) => {
  if (!Array.isArray(bomList) || bomList.length === 0) {
    return []
  }

  const map = new Map()
  const roots = []

  const formatRequiredQty = (numerator, denominator) => {
    const numeratorValue = Number(numerator)
    const denominatorValue = Number(denominator)

    if (!Number.isFinite(numeratorValue)) {
      return ''
    }
    if (!Number.isFinite(denominatorValue) || denominatorValue === 0) {
      return `${numeratorValue}`
    }

    const qty = numeratorValue / denominatorValue
    if (!Number.isFinite(qty)) {
      return ''
    }

    if (Number.isInteger(qty)) {
      return `${qty}`
    }

    return qty.toFixed(6).replace(/\.?0+$/, '')
  }

  const formatRequiredQtyDisplay = (numerator, denominator, unitName) => {
    const qtyText = formatRequiredQty(numerator, denominator)
    if (!qtyText) {
      return ''
    }

    return unitName ? `${qtyText} ${unitName}` : qtyText
  }

  bomList.forEach((item) => {
    map.set(item.entryId, {
      ...item,
      label: `${item.number} / ${item.name}`,
      requiredQtyText: formatRequiredQty(item.numerator, item.denominator),
      requiredQtyDisplay: formatRequiredQtyDisplay(item.numerator, item.denominator, item.unitName),
      children: []
    })
  })

  bomList.forEach((item) => {
    const node = map.get(item.entryId)
    if (item.parentEntryId && map.has(item.parentEntryId)) {
      map.get(item.parentEntryId).children.push(node)
    } else {
      roots.push(node)
    }
  })

  return roots
}

const handleNodeClick = async (data) => {
  currentMaterial.value = data
  loadMaterialInfo(data.number)
  await loadDrawing(data.number)
}

const loadMaterialInfo = async (selectedMaterialCode) => {
  try {
    const result = await proxy.http.get(
      `api/BomQuery/GetMaterial?materialCode=${encodeURIComponent(selectedMaterialCode)}`
    )

    if (result.status && result.data) {
      currentMaterial.value = {
        ...currentMaterial.value,
        ...result.data
      }
    }
  } catch (error) {
    console.error('物料信息加载失败:', error)
  }
}

const loadDrawing = async (selectedMaterialCode) => {
  drawingLoading.value = true
  drawingUrl.value = ''
  drawingError.value = ''

  try {
    const result = await proxy.http.get(
      `api/BomQuery/GetDrawing?materialCode=${encodeURIComponent(selectedMaterialCode)}`
    )

    if (result.success && result.data?.previewUrl) {
      drawingUrl.value = result.data.previewUrl
      drawingError.value = ''
      return
    }

    drawingError.value = result.message || '未找到图纸'
  } catch (error) {
    console.error('图纸加载失败:', error)
    drawingError.value = error.message || '图纸加载异常'
  } finally {
    drawingLoading.value = false
  }
}

const exportCurrentMaterial = async () => {
  if (!hasExportPermission.value) {
    proxy.$message.error('当前账号无导出权限')
    return
  }

  const code = currentMaterialCode.value.trim()
  if (!code) {
    proxy.$message.warning('请先选择需要导出的物料')
    return
  }

  exportingCurrentMaterial.value = true
  try {
    const content = await proxy.http.post(
      'api/BomQuery/ExportCurrentMaterialInfo',
      { materialCode: code },
      '正在导出当前物料信息...',
      { responseType: 'blob' }
    )
    const blob = await resolveExportBlob(content)

    downloadBlobFile(
      blob,
      `${sanitizeFileName(code)}_物料信息_${getTimestamp()}.xlsx`
    )
    proxy.$message.success('当前物料导出完成')
  } catch (error) {
    console.error('导出当前物料失败:', error)
    proxy.$message.error(error?.message || '导出当前物料失败')
  } finally {
    exportingCurrentMaterial.value = false
  }
}

const exportBomMaterials = async () => {
  if (!hasExportPermission.value) {
    proxy.$message.error('当前账号无导出权限')
    return
  }

  const queryCode = materialCode.value.trim()
  if (!queryCode) {
    proxy.$message.warning('请先查询BOM结构')
    return
  }
  if (!hasData.value) {
    proxy.$message.warning('当前没有可导出的BOM物料')
    return
  }

  exportingBomMaterials.value = true
  try {
    const content = await proxy.http.post(
      'api/BomQuery/ExportBomMaterialInfo',
      { materialNumber: queryCode },
      '正在导出BOM全部物料信息...',
      { responseType: 'blob' }
    )
    const blob = await resolveExportBlob(content)

    downloadBlobFile(
      blob,
      `${sanitizeFileName(queryCode)}_BOM物料信息_${getTimestamp()}.xlsx`
    )
    proxy.$message.success('BOM全部物料导出完成')
  } catch (error) {
    console.error('导出BOM全部物料失败:', error)
    proxy.$message.error(error?.message || '导出BOM全部物料失败')
  } finally {
    exportingBomMaterials.value = false
  }
}

const toggleFullScreen = () => {
  const element = drawingContentRef.value

  if (!element) {
    proxy.$message.warning('图纸容器未找到')
    return
  }

  if (!document.fullscreenElement) {
    if (element.requestFullscreen) {
      element.requestFullscreen()
    } else if (element.webkitRequestFullscreen) {
      element.webkitRequestFullscreen()
    } else if (element.mozRequestFullScreen) {
      element.mozRequestFullScreen()
    } else if (element.msRequestFullscreen) {
      element.msRequestFullscreen()
    }
  } else {
    exitFullScreen()
  }
}

const exitFullScreen = () => {
  if (document.exitFullscreen) {
    document.exitFullscreen()
  } else if (document.webkitExitFullscreen) {
    document.webkitExitFullscreen()
  } else if (document.mozCancelFullScreen) {
    document.mozCancelFullScreen()
  } else if (document.msExitFullscreen) {
    document.msExitFullscreen()
  }
}

const handleFullscreenChange = () => {
  isFullscreen.value = !!document.fullscreenElement
}

const clamp = (value, min, max) => {
  return Math.min(max, Math.max(min, value))
}

let stopBomTreeResize = null

const startBomTreeResize = (event) => {
  if (bomTreeCollapsed.value) {
    return
  }

  const startX = event.clientX
  const startWidth = bomTreeWidth.value

  const handleMouseMove = (moveEvent) => {
    const nextWidth = clamp(startWidth + moveEvent.clientX - startX, 240, 560)
    bomTreeWidth.value = nextWidth
  }

  const handleMouseUp = () => {
    document.removeEventListener('mousemove', handleMouseMove)
    document.removeEventListener('mouseup', handleMouseUp)
    stopBomTreeResize = null
  }

  stopBomTreeResize = handleMouseUp
  document.addEventListener('mousemove', handleMouseMove)
  document.addEventListener('mouseup', handleMouseUp)
}

onMounted(() => {
  document.addEventListener('fullscreenchange', handleFullscreenChange)
  document.addEventListener('webkitfullscreenchange', handleFullscreenChange)
  document.addEventListener('mozfullscreenchange', handleFullscreenChange)
  document.addEventListener('msfullscreenchange', handleFullscreenChange)
})

onUnmounted(() => {
  document.removeEventListener('fullscreenchange', handleFullscreenChange)
  document.removeEventListener('webkitfullscreenchange', handleFullscreenChange)
  document.removeEventListener('mozfullscreenchange', handleFullscreenChange)
  document.removeEventListener('msfullscreenchange', handleFullscreenChange)
  stopBomTreeResize && stopBomTreeResize()
})
</script>

<style scoped lang="scss">
.bom-query-container {
  position: absolute;
  inset: 0;
  display: flex;
  flex-direction: column;
  padding: 12px;
  background: #f5f7fa;

  .query-header {
    margin-bottom: 12px;
    padding: 8px 12px;
    background: #fff;
    border: 1px solid #dcdfe6;
    border-radius: 4px;
    flex-shrink: 0;

    .query-input {
      display: flex;
      gap: 12px;
      align-items: center;
    }

    .material-code-input {
      width: 400px;
    }
  }

  .bom-content {
    flex: 1;
    display: flex;
    overflow: hidden;
    min-height: 0;

    .bom-left {
      display: flex;
      flex-direction: column;
      border: 1px solid #dcdfe6;
      border-radius: 4px;
      overflow: hidden;
      background: #fff;
      flex-shrink: 0;

      .bom-tree-title {
        padding: 6px 10px;
        background: #f5f7fa;
        border-bottom: 1px solid #dcdfe6;
        display: flex;
        align-items: center;
        justify-content: space-between;
        font-size: 16px;
        font-weight: 700;

        .collapse-btn {
          padding: 4px;
          font-size: 16px;
        }
      }

      .bom-tree-scrollbar {
        flex: 1;
        height: 0;
      }

      .bom-tree {
        display: inline-block;
        min-width: 100%;
      }

      .custom-tree-node {
        flex: 1;
        display: flex;
        align-items: center;
        justify-content: space-between;
        padding-right: 8px;
        font-size: 14px;

        .tree-label {
          white-space: nowrap;
        }
      }

      :deep(.el-tree-node__content) {
        white-space: nowrap;
      }
    }

    .panel-resizer {
      width: 12px;
      cursor: col-resize;
      flex-shrink: 0;
      position: relative;

      &::before {
        content: '';
        position: absolute;
        top: 0;
        bottom: 0;
        left: 5px;
        width: 2px;
        border-radius: 1px;
        background: #dcdfe6;
        transition: background-color 0.2s ease;
      }

      &:hover::before {
        background: #409eff;
      }
    }

    .bom-left-collapsed {
      width: 40px;
      margin-right: 12px;
      display: flex;
      align-items: center;
      justify-content: center;
      border: 1px solid #dcdfe6;
      border-radius: 4px;
      background: #fff;
      flex-shrink: 0;

      .expand-btn {
        padding: 8px;
        font-size: 20px;
        color: #409eff;

        &:hover {
          color: #66b1ff;
        }
      }
    }

    .bom-right {
      flex: 1;
      min-width: 0;
      display: flex;
      flex-direction: column;
      overflow: hidden;

      .material-info {
        margin-left: 12px;
        margin-bottom: 8px;
        border: 1px solid #dcdfe6;
        border-radius: 4px;
        background: #fff;
        flex-shrink: 0;
        overflow: hidden;

        .material-info-grid {
          display: flex;
          flex-wrap: wrap;
          gap: 8px;
          padding: 8px;
          overflow: auto;
        }

        .material-field-card {
          min-width: 180px;
          max-width: 100%;
          padding: 8px 10px;
          border: 1px solid #ebeef5;
          border-radius: 4px;
          background: #fff;
          box-sizing: border-box;
          overflow: auto;
          resize: horizontal;
          flex: 0 0 auto;
        }

        .material-field-label {
          font-size: 12px;
          color: #909399;
          line-height: 1.4;
        }

        .material-field-value {
          margin-top: 6px;
          font-size: 13px;
          color: #303133;
          line-height: 1.6;
          word-break: break-all;
          white-space: pre-wrap;
          min-height: 22px;
        }
      }

      .material-info-collapsed {
        margin-left: 12px;
        margin-bottom: 8px;
        padding: 8px;
        border: 1px solid #dcdfe6;
        border-radius: 4px;
        background: #fff;
        text-align: center;
        flex-shrink: 0;

        .expand-btn {
          color: #409eff;
          font-size: 14px;

          &:hover {
            color: #66b1ff;
          }
        }
      }

      .drawing-preview {
        flex: 1;
        min-height: 0;
        margin-left: 12px;
        display: flex;
        flex-direction: column;
        border: 1px solid #dcdfe6;
        border-radius: 4px;
        background: #fff;
        overflow: hidden;

        .drawing-content {
          position: relative;
          flex: 1;
          min-height: 0;
          display: flex;
          align-items: stretch;
          justify-content: stretch;
          overflow: hidden;
          background: #fff;

          .fullscreen-exit-btn {
            position: absolute;
            top: 20px;
            right: 20px;
            z-index: 9999;

            .el-button {
              padding: 12px 24px;
              font-size: 16px;
              box-shadow: 0 2px 12px 0 rgba(0, 0, 0, 0.3);
            }
          }

          .drawing-iframe {
            position: absolute;
            inset: 0;
            width: 100%;
            height: 100%;
            border: none;
          }

          :deep(.el-empty) {
            margin: auto;
            padding: 20px;
          }

          .drawing-error {
            display: flex;
            flex-direction: column;
            align-items: center;
            justify-content: center;
            color: #606266;

            .error-message {
              margin-top: 4px;
              font-size: 14px;
              color: #f56c6c;
              line-height: 1.5;
              text-align: center;
            }
          }

          &:fullscreen,
          &:-webkit-full-screen,
          &:-moz-full-screen,
          &:-ms-fullscreen {
            background: #000;

            .drawing-iframe {
              background: #fff;
            }
          }
        }
      }
    }
  }
}

.info-title {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 12px;
  margin: 0;
  padding: 6px 10px;
  border-bottom: 1px solid #dcdfe6;
  background: #f5f7fa;
  font-size: 16px;
  font-weight: 700;

  .info-title-main {
    min-width: 0;
    display: flex;
    align-items: center;
    gap: 12px;
    overflow: hidden;
  }

  .info-subtitle {
    font-size: 14px;
    font-weight: 400;
    color: #606266;
    white-space: nowrap;
    overflow: hidden;
    text-overflow: ellipsis;
  }

  .info-actions {
    display: flex;
    align-items: center;
    gap: 8px;
    flex-shrink: 0;
  }

  .collapse-btn,
  .fullscreen-btn {
    padding: 4px;
    font-size: 16px;
    color: #409eff;

    &:hover {
      color: #66b1ff;
    }
  }
}

:deep(.el-tree) {
  .el-tree-node__content {
    height: 36px;

    &:hover {
      background-color: #f5f7fa;
    }
  }

  .is-current > .el-tree-node__content {
    background-color: #e6f7ff;
    color: #409eff;
  }
}

@media (max-width: 1200px) {
  .bom-query-container {
    .query-header {
      .query-input {
        flex-wrap: wrap;
      }

      .material-code-input {
        width: 100%;
      }
    }

    .bom-content {
      .bom-right {
        .material-info,
        .material-info-collapsed,
        .drawing-preview {
          margin-left: 8px;
        }

        .material-info {
          .material-field-card {
            width: 100% !important;
            resize: none;
          }
        }
      }
    }
  }
}
</style>
