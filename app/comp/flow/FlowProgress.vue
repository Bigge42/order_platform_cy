<template>
  <div class="flow-progress" :style="{ height: `${getContainerHeight()}px` }">
    <!-- 主线暂时隐藏，因为蛇形布局不需要直线 -->
    <!-- <div class="flow-main-line"></div> -->
    
    <!-- 节点 -->
    <template v-for="(step, idx) in steps" :key="step.key">
      <div
        class="flow-node"
        :class="getNodeClass(step, idx)"
        :style="getNodeStyle(idx)"
      >
        <div class="node-content">
          <span class="node-title">{{ step.name }}</span>
          <!-- <span class="node-percent">({{ step.percent }}%)</span> -->
        </div>
      </div>
      
      <!-- 箭头，最后一个节点不显示 -->
      <template v-if="idx < steps.length - 1">
        <div
          class="flow-arrow"
          :style="getArrowStyle(idx)"
          :class="getArrowClass(idx)"
        >
          <svg
            viewBox="0 0 100 100"
            preserveAspectRatio="none"
            class="arrow-svg"
          >
            <!-- 水平箭头 -->
            <template v-if="getArrowDirection(idx) !== 'vertical'">
              <!-- 向右箭头 -->
              <template v-if="getArrowDirection(idx) === 'right'">
                <line
                  x1="10" y1="50" x2="60" y2="50"
                  :stroke="isArrowActive(idx) ? '#33845b' : '#78848E'"
                  stroke-width="36"
                />
                <polygon
                  points="60,0 100,50 60,100"
                  :fill="isArrowActive(idx) ? '#33845b' : '#78848E'"
                />
              </template>
              <!-- 向左箭头 -->
              <template v-else>
                <line
                  x1="90" y1="50" x2="40" y2="50"
                  :stroke="isArrowActive(idx) ? '#33845b' : '#78848E'"
                  stroke-width="36"
                />
                <polygon
                  points="40,0 0,50 40,100"
                  :fill="isArrowActive(idx) ? '#33845b' : '#78848E'"
                />
              </template>
            </template>
            
            <!-- 垂直箭头 -->
            <template v-else>
              <line
                x1="50" y1="10" x2="50" y2="60"
                :stroke="isArrowActive(idx) ? '#33845b' : '#78848E'"
                stroke-width="20"
              />
              <polygon
                points="20,60 50,100 80,60"
                :fill="isArrowActive(idx) ? '#33845b' : '#78848E'"
              />
            </template>
          </svg>
        </div>
      </template>
    </template>
  </div>
</template>

<script setup>
import { computed } from 'vue'

const props = defineProps({
  steps: {
    type: Array,
    required: true,
  },
  currentKey: {
    type: String,
    required: true
  }
})

const COLS = 3 // 每行3个节点
const NODE_WIDTH = 110
const NODE_HEIGHT = 40  // 调整节点高度
const ROW_HEIGHT = 74   // 调整行高适应新的箭头尺寸



// 获取节点的CSS类
const getNodeClass = (step, idx) => {
  if (step.percent < 100) {
    // 未达到100%的节点（包含0%）：显示绿色
    return 'is-completed'
  } else {
    // 其他节点：默认样式
    return ''
  }
}

// 获取容器高度
const getContainerHeight = () => {
  const rows = Math.ceil(props.steps.length / COLS)
  return rows * ROW_HEIGHT + 40 // 额外40px padding
}

// 获取节点在蛇形布局中的坐标
const getNodePosition = (idx) => {
  const row = Math.floor(idx / COLS)
  const col = idx % COLS
  
  // 奇数行（从0开始）需要反向排列
  const isOddRow = row % 2 === 1
  const actualCol = isOddRow ? (COLS - 1 - col) : col
  
  // 确保节点完整显示，增加足够边距
  const leftMargin = 16 // 足够的左边距确保节点不被截断
  const rightMargin = 16 // 足够的右边距确保节点不被截断
  const availableWidth = 100 - leftMargin - rightMargin
  const colWidth = availableWidth / (COLS - 1)
  
  return {
    x: leftMargin + (actualCol * colWidth), // 确保节点完整显示
    y: row * ROW_HEIGHT + 20 // 像素
  }
}

// 节点定位样式
const getNodeStyle = (idx) => {
  const pos = getNodePosition(idx)
  return {
    left: pos.x + '%',
    top: pos.y + 'px',
    transform: 'translateX(-50%)',
    position: 'absolute',
    zIndex: 2,
    width: NODE_WIDTH + 'px',
    height: NODE_HEIGHT + 'px'
  }
}

// 获取箭头方向和类型
const getArrowDirection = (idx) => {
  const currentPos = getNodePosition(idx)
  const nextPos = getNodePosition(idx + 1)
  
  if (Math.abs(currentPos.y - nextPos.y) < 10) {
    // 同一行，水平箭头
    return currentPos.x < nextPos.x ? 'right' : 'left'
  } else {
    // 不同行，垂直箭头
    return 'vertical'
  }
}

// 箭头定位样式
const getArrowStyle = (idx) => {
  const currentPos = getNodePosition(idx)
  const nextPos = getNodePosition(idx + 1)
  const direction = getArrowDirection(idx)
  
  if (direction === 'right') {
    // 水平向右
    const startX = currentPos.x + (NODE_WIDTH / 2 / window.innerWidth * 100)
    const endX = nextPos.x - (NODE_WIDTH / 2 / window.innerWidth * 100)
    const width = endX - startX
    
    return {
      position: 'absolute',
      left: startX + '%',
      top: (currentPos.y + NODE_HEIGHT / 2 - 6) + 'px',
      width: width + '%',
      height: '12px',
      zIndex: 1
    }
  } else if (direction === 'left') {
    // 水平向左
    const startX = currentPos.x - (NODE_WIDTH / 2 / window.innerWidth * 100)
    const endX = nextPos.x + (NODE_WIDTH / 2 / window.innerWidth * 100)
    const width = startX - endX
    
    return {
      position: 'absolute',
      left: endX + '%',
      top: (currentPos.y + NODE_HEIGHT / 2 - 6) + 'px',
      width: width + '%',
      height: '12px',
      zIndex: 1
    }
  } else {
    // 垂直向下
    return {
      position: 'absolute',
      left: currentPos.x + '%',
      top: (currentPos.y + NODE_HEIGHT + 5) + 'px',
      width: '24px',
      height: '24px',
      zIndex: 1,
      transform: 'translateX(-50%)'
    }
  }
}

// 箭头CSS类
const getArrowClass = (idx) => {
  const direction = getArrowDirection(idx)
  return `arrow-${direction}`
}

// 箭头是否激活（绿色）
const isArrowActive = (idx) => {
  const currentStep = props.steps[idx]
  return currentStep.percent >= 0 && currentStep.percent < 100
}
</script>

<style scoped>
.flow-progress {
  position: relative;
  width: 100%;
  margin: 24rpx 0;
  box-sizing: border-box;
  padding: 0 16px; /* 恢复合适的内边距 */
  overflow: hidden; /* 防止内容溢出 */
}

.flow-node {
  background: transparent;
  border: 2px solid #8C939D;
  border-radius: 8px;
  text-align: center;
  transition: border-color 0.2s, background 0.2s;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  font-size: 14px;
  box-sizing: border-box;
  padding: 4px;
  color: #606266;
}

.flow-node.is-completed {
  color: #33845b;
  font-weight: bold;
}

.flow-node.is-completed .node-percent {
  color: #33845b;
}

.node-content {
  display: flex;
  flex-direction: row;
  align-items: center;
  justify-content: center;
  box-sizing: border-box;
  width: 100%;
  height: 100%;
}

.node-title {
  font-size: 12px;
  line-height: 1.2;
  text-align: center;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.node-percent {
  font-size: 11px;
  color: #606266;
  text-align: center;
  white-space: nowrap;
  flex-shrink: 0;
}

.flow-arrow {
  pointer-events: none;
}

.arrow-svg {
  width: 100%;
  height: 100%;
  display: block;
}

.arrow-right .arrow-svg,
.arrow-left .arrow-svg {
  height: 12px;
}

.arrow-vertical .arrow-svg {
  width: 24px;
}
</style> 