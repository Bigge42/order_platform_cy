<template>
  <div class="bom-query-container">
    <!-- 查询区域 -->
    <div class="query-header">
     
      <div class="query-input">
        <el-input
          v-model="materialCode"
          placeholder="请输入物料编码"
          clearable
          @keyup.enter="handleQuery"
          style="width: 400px"
        >
          <template #prepend>物料编码</template>
        </el-input>
        <el-button type="primary" @click="handleQuery" :loading="loading">
          查询
        </el-button>
      </div>
    </div>

    <!-- 主体内容区域 -->
    <div class="bom-content">
      <!-- 左侧BOM树 -->
      <div class="bom-left" v-show="!bomTreeCollapsed">
        <div class="bom-tree-title">
          BOM结构
          <el-button
            text
            :icon="ArrowLeft"
            @click="bomTreeCollapsed = true"
            title="收起BOM树"
            class="collapse-btn"
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
            @node-click="handleNodeClick"
            class="bom-tree"
          >
            <template #default="{ data }">
              <span class="custom-tree-node">
                <span class="tree-label">
                  {{ data.number }} / {{ data.name }}
                </span>
              </span>
            </template>
          </el-tree>
          <el-empty
            v-else
            description="请输入物料编码进行查询"
            :image-size="100"
          ></el-empty>
        </el-scrollbar>
      </div>

      <!-- BOM树收起后的展开按钮 -->
      <div class="bom-left-collapsed" v-show="bomTreeCollapsed">
        <el-button
          text
          :icon="ArrowRight"
          @click="bomTreeCollapsed = false"
          title="展开BOM树"
          class="expand-btn"
        />
      </div>

      <!-- 右侧物料信息和图纸 -->
      <div class="bom-right">
        <!-- 物料信息 -->
        <div class="material-info" v-show="!materialInfoCollapsed">
          <div class="info-title">
            物料信息
            <span v-if="currentMaterial" style="margin-left: 20px; font-weight: normal; font-size: 14px;">
              {{ currentMaterial.number || currentMaterial.materialCode }}
            </span>
            <el-button
              text
              :icon="ArrowUp"
              @click="materialInfoCollapsed = true"
              title="收起物料信息"
              class="collapse-btn"
              style="float: right;"
            />
          </div>
          <el-descriptions :column="4" border size="small">
            <el-descriptions-item label="物料名称" :span="1">
              {{ currentMaterial?.name || currentMaterial?.materialName || '' }}
            </el-descriptions-item>
            <el-descriptions-item label="公称通径" :span="1">
              {{ currentMaterial?.nominalDiameter || '' }}
            </el-descriptions-item>
            <el-descriptions-item label="公称压力" :span="1">
              {{ currentMaterial?.nominalPressure || '' }}
            </el-descriptions-item>
            <el-descriptions-item label="CV" :span="1">
              {{ currentMaterial?.cv || '' }}
            </el-descriptions-item>

            <el-descriptions-item label="法兰标准" :span="1">
              {{ currentMaterial?.flangeStandard || '' }}
            </el-descriptions-item>

            <el-descriptions-item label="法兰密封面型式" :span="1">
              {{ currentMaterial?.flangeSealType || '' }}
            </el-descriptions-item>
            <el-descriptions-item label="阀体材质" :span="1">
              {{ currentMaterial?.bodyMaterial || '' }}
            </el-descriptions-item>

            <el-descriptions-item label="阀内件材质" :span="1">
              {{ currentMaterial?.trimMaterial || '' }}
            </el-descriptions-item>
          
             <el-descriptions-item label="流量特性" :span="1">
              {{ currentMaterial?.flowCharacteristic||'' }}
            </el-descriptions-item>
            <el-descriptions-item label="填料形式" :span="1">
              {{ currentMaterial?.packingForm||'' }}
            </el-descriptions-item>
            <el-descriptions-item label="法兰连接方式" :span="1">
              {{ currentMaterial?.flangeConnection|| '' }}
            </el-descriptions-item>
            <el-descriptions-item label="执行机构型号" :span="1">
              {{ currentMaterial?.actuatorModel || '' }}
            </el-descriptions-item>
            <el-descriptions-item label="执行机构行程" :span="1">
              {{ currentMaterial?.actuatorStroke|| '' }}
            </el-descriptions-item>
            <el-descriptions-item label="图号" :span="1">
              {{ currentMaterial?.drawingNo || '' }}
            </el-descriptions-item>
            <el-descriptions-item label="材质" :span="1">
              {{ currentMaterial?.material|| '' }}
            </el-descriptions-item>
            <el-descriptions-item label="TC发布人" :span="1">
              {{ currentMaterial?.tcReleaser ||'' }}
            </el-descriptions-item>
          </el-descriptions>
        </div>

        <!-- 物料信息收起后的展开按钮 -->
        <div class="material-info-collapsed" v-show="materialInfoCollapsed">
          <el-button
            text
            :icon="ArrowDown"
            @click="materialInfoCollapsed = false"
            title="展开物料信息"
            class="expand-btn"
          >
            物料信息
          </el-button>
        </div>

        <!-- 图纸预览 -->
        <div class="drawing-preview">
          <div class="info-title">
            图纸
              <span v-if="currentMaterial" style="margin-left: 20px; font-weight: normal; font-size: 14px;">
              {{ currentMaterial.drawingNo }}
            </span>
             <span v-if="currentMaterial" style="margin-left: 20px; font-weight: normal; font-size: 14px;">
              {{ currentMaterial?.nominalDiameter }}
            </span>
            <el-button
              text
              :icon="FullScreen"
              @click="toggleFullScreen"
              title="全屏查看图纸"
              class="fullscreen-btn"
              style="float: right;"
            />
          </div>
          <div class="drawing-content" v-loading="drawingLoading" ref="drawingContentRef">
            <!-- 图纸iframe -->
            <iframe
              v-if="drawingUrl"
              :src="drawingUrl + '#toolbar=0&navpanes=0&scrollbar=0'"
              frameborder="0"
              class="drawing-iframe"
              ref="drawingIframeRef"
            ></iframe>
            <!-- 错误提示 -->
            <el-empty
              v-else-if="drawingError"
              :image-size="100"
            >
              <template #description>
                <div class="drawing-error">
                  <el-icon :size="20" color="#f56c6c" style="margin-bottom: 8px;">
                    <WarningFilled />
                  </el-icon>
                  <div class="error-message">{{ drawingError }}</div>
                </div>
              </template>
            </el-empty>
            <!-- 暂无图纸 -->
            <el-empty
              v-else
              description="暂无图纸"
              :image-size="100"
            ></el-empty>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, getCurrentInstance, nextTick } from 'vue'
import { WarningFilled, FullScreen, ArrowLeft, ArrowRight, ArrowUp, ArrowDown } from '@element-plus/icons-vue'

const { proxy } = getCurrentInstance()

// 数据定义
const materialCode = ref('')
const loading = ref(false)
const drawingLoading = ref(false)
const hasData = ref(false)
const bomTreeData = ref([])
const currentMaterial = ref(null)
const drawingUrl = ref('')
const drawingError = ref('') // 图纸加载错误信息
const treeRef = ref(null)
const drawingContentRef = ref(null) // 图纸容器ref
const drawingIframeRef = ref(null) // 图纸iframe ref

// 折叠状态
const bomTreeCollapsed = ref(false) // BOM树折叠状态
const materialInfoCollapsed = ref(false) // 物料信息折叠状态

// 树形结构配置
const treeProps = {
  children: 'children',
  label: 'label'
}

// 查询BOM
const handleQuery = async () => {
  if (!materialCode.value) {
    proxy.$message.warning('请输入物料编码')
    return
  }

  loading.value = true
  try {
    const result = await proxy.http.get(
      `api/BomQuery/ExpandBom?materialNumber=${materialCode.value}`
    )

    if (result.status && result.data) {
      // 构建树形结构
      bomTreeData.value = buildBomTree(result.data)
      hasData.value = true

      // 默认选中根节点
      nextTick(() => {
        if (bomTreeData.value.length > 0) {
          treeRef.value.setCurrentKey(bomTreeData.value[0].entryId)
          handleNodeClick(bomTreeData.value[0])
        }
      })
    } else {
      proxy.$message.error(result.message || 'BOM查询失败')
      hasData.value = false
    }
  } catch (error) {
    console.error('BOM查询失败:', error)
    proxy.$message.error('BOM查询失败')
    hasData.value = false
  } finally {
    loading.value = false
  }
}

// 构建BOM树形结构
const buildBomTree = (bomList) => {
  if (!bomList || bomList.length === 0) return []

  // 创建映射表
  const map = new Map()
  const roots = []

  // 第一遍遍历：创建所有节点
  bomList.forEach((item) => {
    map.set(item.entryId, {
      ...item,
      label: `${item.number} / ${item.name}`,
      children: []
    })
  })

  // 第二遍遍历：建立父子关系
  bomList.forEach((item) => {
    const node = map.get(item.entryId)
    if (item.parentEntryId && map.has(item.parentEntryId)) {
      // 有父节点，添加到父节点的children中
      const parent = map.get(item.parentEntryId)
      parent.children.push(node)
    } else {
      // 没有父节点或父节点不存在，作为根节点
      roots.push(node)
    }
  })

  return roots
}

// 树节点点击事件
const handleNodeClick = async (data) => {
  // 先显示BOM数据中的基本信息
  currentMaterial.value = data

  // 异步加载完整的物料信息
  loadMaterialInfo(data.number)

  // 加载图纸
  await loadDrawing(data.number)
}

// 加载物料完整信息
const loadMaterialInfo = async (materialCode) => {
  try {
    const result = await proxy.http.get(
      `api/BomQuery/GetMaterial?materialCode=${materialCode}`
    )

    if (result.status && result.data) {
      // 合并物料信息到当前物料对象
      currentMaterial.value = {
        ...currentMaterial.value,
        ...result.data
      }
    }
  } catch (error) {
    console.error('物料信息加载失败:', error)
  }
}

// 加载图纸
const loadDrawing = async (materialCode) => {
  drawingLoading.value = true
  drawingUrl.value = ''
  drawingError.value = '' // 清空之前的错误信息

  try {
    const result = await proxy.http.get(
      `api/BomQuery/GetDrawing?materialCode=${materialCode}`
    )

    console.log('图纸接口返回:', result)

    if (result.success && result.data && result.data.previewUrl) {
      drawingUrl.value = result.data.previewUrl
      drawingError.value = ''
      console.log('设置图纸URL:', drawingUrl.value)
    } else {
      drawingUrl.value = ''
      // 设置错误信息
      drawingError.value = result.message || '未找到图纸'
      console.log('图纸加载失败:', drawingError.value)
    }
  } catch (error) {
    console.error('图纸加载失败:', error)
    drawingUrl.value = ''
    drawingError.value = error.message || '图纸加载异常'
  } finally {
    drawingLoading.value = false
  }
}

// 全屏切换
const toggleFullScreen = () => {
  const element = drawingContentRef.value

  if (!element) {
    proxy.$message.warning('图纸容器未找到')
    return
  }

  if (!document.fullscreenElement) {
    // 进入全屏
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
    // 退出全屏
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
}


</script>

<style scoped lang="scss">
.bom-query-container {
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  display: flex;
  flex-direction: column;
  background: #f5f7fa;
  padding: 12px;

  .query-header {
    margin-bottom: 12px;
    padding: 8px 12px;
    background: #fff;
    border: 1px solid #dcdfe6;
    border-radius: 4px;
    flex-shrink: 0;

    .query-title {
      font-size: 14px;
      font-weight: bold;
      margin-bottom: 8px;
      color: #303133;
    }

    .query-input {
      display: flex;
      gap: 12px;
      align-items: center;
    }
  }

  .bom-content {
    flex: 1;
    display: flex;
    gap: 12px;
    overflow: hidden;
    background: transparent;
    min-height: 0;

    .bom-left {
      width: 300px;
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
        font-weight: bold;
        font-size: 16px;
        border-bottom: 1px solid #dcdfe6;
        flex-shrink: 0;
        display: flex;
        align-items: center;
        justify-content: space-between;

        .collapse-btn {
          padding: 4px;
          font-size: 16px;
        }
      }

      .bom-tree-scrollbar {
        flex: 1;
        height: 0; // 配合flex: 1使用，确保滚动条正常工作
      }

      .bom-tree {
        display: inline-block; // 关键：让树的宽度由内容决定
        min-width: 100%; // 至少占满容器宽度
      }

      .custom-tree-node {
        flex: 1;
        display: flex;
        align-items: center;
        justify-content: space-between;
        font-size: 14px;
        padding-right: 8px;

        .tree-label {
          white-space: nowrap; // 不换行，保持单行显示
        }
      }

      // 允许树节点横向滚动
      :deep(.el-tree-node__content) {
        white-space: nowrap; // 不换行
      }
    }

    // BOM树收起后的展开按钮
    .bom-left-collapsed {
      width: 40px;
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
      display: flex;
      flex-direction: column;
      overflow: hidden;

      .material-info {
        margin-bottom: 8px;
        border: 1px solid #dcdfe6;
        border-radius: 4px;
        padding: 0;
        background: #fff;
        flex-shrink: 0;

        .info-title {
          font-size: 16px;
          font-weight: bold;
          padding: 6px 10px;
          background: #f5f7fa;
          border-bottom: 1px solid #dcdfe6;
          margin: 0;
          display: flex;
          align-items: center;
          justify-content: space-between;

          .collapse-btn {
            padding: 4px;
            font-size: 16px;
          }
        }

        :deep(.el-descriptions) {
          padding: 8px;
        }

        :deep(.el-descriptions__label) {
          font-size: 12px;
          padding: 4px 8px;
        }

        :deep(.el-descriptions__content) {
          font-size: 12px;
          padding: 4px 8px;
        }

        :deep(.el-descriptions__cell) {
          padding: 4px 8px;
        }
      }

      // 物料信息收起后的展开按钮
      .material-info-collapsed {
        margin-bottom: 8px;
        border: 1px solid #dcdfe6;
        border-radius: 4px;
        background: #fff;
        padding: 8px;
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
        display: flex;
        flex-direction: column;
        border: 1px solid #dcdfe6;
        border-radius: 4px;
        padding: 0;
        background: #fff;
        overflow: hidden;
        min-height: 0;

        .info-title {
          font-size: 16px;
          font-weight: bold;
          padding: 6px 10px;
          background: #f5f7fa;
          border-bottom: 1px solid #dcdfe6;
          margin: 0;
          flex-shrink: 0;
          display: flex;
          align-items: center;
          justify-content: space-between;

          .fullscreen-btn {
            padding: 4px;
            font-size: 16px;
            color: #409eff;

            &:hover {
              color: #66b1ff;
            }
          }
        }

        .drawing-content {
          flex: 1;
          overflow: hidden;
          display: flex;
          align-items: stretch;
          justify-content: stretch;
          background: #fff;
          padding: 0;
          min-height: 0;
          position: relative;

          .drawing-iframe {
            position: absolute;
            top: 0;
            left: 0;
            width: 100%;
            height: 100%;
            border: none;
          }

          :deep(.el-empty) {
            padding: 20px;
            margin: auto;
          }

          .drawing-error {
            display: flex;
            flex-direction: column;
            align-items: center;
            justify-content: center;
            color: #606266;

            .error-message {
              font-size: 14px;
              color: #f56c6c;
              margin-top: 4px;
              text-align: center;
              line-height: 1.5;
            }
          }

          // 全屏模式下的样式
          &:fullscreen {
            background: #000;

            .drawing-iframe {
              background: #fff;
            }
          }

          &:-webkit-full-screen {
            background: #000;

            .drawing-iframe {
              background: #fff;
            }
          }

          &:-moz-full-screen {
            background: #000;

            .drawing-iframe {
              background: #fff;
            }
          }

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

  .empty-state {
    flex: 1;
    display: flex;
    align-items: center;
    justify-content: center;
  }
}

// 树形结构样式优化
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
</style>


