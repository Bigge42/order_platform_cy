<template>
  <div class="workflow-node">
    <div class="node-header">
      <div>按时完成率：</div>
      <div class="completion-rate">{{ nodeData.rate }}</div>
    </div>
    <div class="node-title">{{ nodeData.title }}</div>
    <div class="task-stats">
      <div class="task-row">
        <div class="task-label">总任务：</div>
        <div class="task-value">{{ nodeData.totalTasks }}</div>
      </div>
      <div class="task-row">
        <div class="task-label">{{ nodeData.pendingLabel }}：</div>
        <div class="task-value">{{ nodeData.pendingTasks }}</div>
      </div>
      <div class="task-row">
        <div class="task-label">超期未完成：</div>
        <div class="task-value">{{ nodeData.overdueIncomplete }}</div>
      </div>
      <div class="task-row">
        <div class="task-label">超期已完成：</div>
        <div class="task-value">{{ nodeData.overdueComplete }}</div>
      </div>
    </div>
    <div class="standard-metric">
      <div class="standard-ratio">标准配比：{{ nodeData.standardRatio }}</div>
      <div class="standard-time">标准时长：<span class="value">{{ nodeData.standardDays }}</span> 天</div>
    </div>
    <div v-if="hasRightArrow" class="arrow right"></div>
    <div v-if="hasLeftArrow" class="arrow left"></div>
  </div>
</template>

<script>
import { defineComponent } from 'vue';

export default defineComponent({
  name: 'WorkflowNode',
  props: {
    nodeData: {
      type: Object,
      required: true
    },
    hasRightArrow: {
      type: Boolean,
      default: false
    },
    hasLeftArrow: {
      type: Boolean,
      default: false
    }
  }
});
</script>

<style lang="less" scoped>
.workflow-node {
  background-color: #fff;
  border-radius: 5px;
  padding: 15px 15px 10px 15px;
  position: relative;
  box-shadow: 0 2px 5px rgba(0,0,0,0.3);
  display: flex;
  flex-direction: column;
}

.node-header {
  background-color: rgb(45, 140, 240); // 默认使用蓝色主题色
  color: white;
  padding: 10px;
  border-radius: 5px 5px 0 0;
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin: -15px -15px 15px -15px;
}

.node-title {
  writing-mode: vertical-lr;
  text-orientation: upright;
  font-size: 16px;
  font-weight: bold;
  padding: 5px;
  background-color: rgb(45, 140, 240); // 默认使用蓝色主题色
  color: white;
  border-radius: 5px;
  height: 120px;
  display: flex;
  align-items: center;
  justify-content: center;
  position: absolute;
  left: -20px;
  top: 65px;
  z-index: 2;
}

.completion-rate {
  font-size: 14px;
}

.task-stats {
  border: 1px solid rgb(45, 140, 240); // 默认使用蓝色主题色
  border-radius: 5px;
  padding: 10px;
  flex-grow: 1;
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.task-row {
  display: flex;
  justify-content: space-between;
  font-size: 14px;
}

.task-label {
  color: #666;
}

.task-value {
  font-weight: bold;
  color: rgb(45, 140, 240); // 默认使用蓝色主题色
}

.task-value.highlight {
  color: #ff6b6b;
}

.standard-metric {
  margin-top: 10px;
  text-align: center;
  font-size: 14px;
  padding-bottom: 5px;
}

.standard-ratio {
  font-weight: bold;
  margin-bottom: 3px;
}

.standard-time {
  color: #666;
  white-space: nowrap;
  font-size: 14px;
}

.standard-time .value {
  color: #ff6b6b;
  font-weight: bold;
}

/* 箭头 */
.arrow {
  position: absolute;
  top: 50%;
  right: -40px;
  transform: translateY(-50%);
  width: 30px;
  height: 20px;
  color: rgb(45, 140, 240); // 默认使用蓝色主题色
  font-size: 24px;
  z-index: 1;

  &:after {
    display: block;
    content: '›››';
    animation: arrow 2s infinite;
  }
}

.arrow.left::after {
  transform: rotate(180deg);
  transform-origin: center;
}

@keyframes arrow {
  0% { opacity: 0.3; }
  50% { opacity: 1; }
  100% { opacity: 0.3; }
}


// 添加不同主题色下的样式
// 深色主题
.vol-theme-dark-aside {
  .workflow-node {
    .node-header {
      background-color: #021d37;
    }
    
    .node-title {
      background-color: #021d37;
    }
    
    .task-value {
      color: #021d37;
    }
  }
  
  .arrow:after {
    color: #021d37;
  }
  
  .task-stats {
    border-color: #021d37;
  }
}

// 红色主题
.vol-theme-red-aside {
  .workflow-node {
    .node-header {
      background-color: rgb(237, 64, 20);
    }
    
    .node-title {
      background-color: rgb(237, 64, 20);
    }
    
    .task-value {
      color: rgb(237, 64, 20);
    }
  }
  
  .arrow:after {
    color: rgb(237, 64, 20);
  }
  
  .task-stats {
    border-color: rgb(237, 64, 20);
  }
}

// 橙色主题
.vol-theme-orange-aside {
  .workflow-node {
    .node-header {
      background-color: rgb(255, 153, 0);
    }
    
    .node-title {
      background-color: rgb(255, 153, 0);
    }
    
    .task-value {
      color: rgb(255, 153, 0);
    }
  }
  
  .arrow:after {
    color: rgb(255, 153, 0);
  }
  
  .task-stats {
    border-color: rgb(255, 153, 0);
  }
}

// 橙色渐变主题
.vol-theme-gradient_orange-aside {
  .workflow-node {
    .node-header {
      background: linear-gradient(to bottom, #fdb890 60%, #fae6aa);
    }
    
    .node-title {
      background: linear-gradient(to bottom, #fdb890 60%, #fae6aa);
    }
    
    .task-value {
      color: #fdb890;
    }
  }
  
  .arrow:after {
    color: #fdb890;
  }
  
  .task-stats {
    border-color: #fdb890;
  }
}

// 绿色主题
.vol-theme-green-aside {
  .workflow-node {
    .node-header {
      background-color: rgb(25, 190, 107);
    }
    
    .node-title {
      background-color: rgb(25, 190, 107);
    }
    
    .task-value {
      color: rgb(25, 190, 107);
    }
  }
  
  .arrow:after {
    color: rgb(25, 190, 107);
  }
  
  .task-stats {
    border-color: rgb(25, 190, 107);
  }
}

// 蓝色主题
.vol-theme-blue-aside {
  .workflow-node {
    .node-header {
      background-color: rgb(45, 140, 240);
    }
    
    .node-title {
      background-color: rgb(45, 140, 240);
    }
    
    .task-value {
      color: rgb(45, 140, 240);
    }
  }
  
  .arrow:after {
    color: rgb(45, 140, 240);
  }
  
  .task-stats {
    border-color: rgb(45, 140, 240);
  }
}

// 白色主题
.vol-theme-white-aside {
  background-color: #fff;

  .workflow-node {
    .node-header {
      background-color: rgb(45, 140, 240);
    }
    
    .node-title {
      background-color: rgb(45, 140, 240);
    }
    
    .task-value {
      color: rgb(45, 140, 240);
    }
  }
  
  .arrow:after {
    color: rgb(45, 140, 240);
  }
  
  .task-stats {
    border-color: rgb(45, 140, 240);
  }
}


</style> 