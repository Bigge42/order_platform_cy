<template>
  <div class="container">
    <div class="main-content">
      <div class="header">
        调节阀客户合同交付节点监控
      </div>
      
      <!-- 筛选区域 -->
      <div class="filter-section">
        <div class="filter-item">
          <div class="filter-label">销售单据号：</div>
          <el-select v-model="selectedOrder" placeholder="所有" class="filter-control">
            <el-option label="所有" value="所有"></el-option>
          </el-select>
        </div>
        <div class="filter-item">
          <div class="filter-label">回复交付期(年份)：</div>
          <el-select v-model="selectedYear" class="filter-control">
            <el-option label="2025" value="2025"></el-option>
          </el-select>
        </div>
        <div class="filter-item">
          <div class="filter-label">回复交付期(月份)：</div>
          <div style="display: flex; gap: 5px;">
            <div 
              v-for="month in months" 
              :key="month"
              :class="['month-item', selectedMonth === month ? 'month-selected' : '']"
              @click="selectedMonth = month"
            >{{ month }}</div>
          </div>
        </div>
      </div>
      
      <!-- 统计区域 -->
      <div class="stats-section">
        <div class="stats-container">
          <div class="stats-label">售</div>
          <div class="stats-count">{{ productCodeData.totalTasks }} 份</div>
        </div>
        <div>订单交付时长 (回复交付日期-合同签日期) = </div>
        <div class="stats-value">{{ deliveryDays }}</div>
        <div>天</div>
      </div>
      
      <!-- 流程节点 - 第一行 -->
      <div class="workflow-section">
        <div class="workflow-row">
          <WorkflowNode 
            v-for="(node, index) in firstRowNodes" 
            :key="node.title"
            :node-data="node"
            :has-right-arrow="index < firstRowNodes.length - 1"
          />
        </div>
        <div class="arrow-bottom">›››</div>
      </div>
      
      <!-- 流程节点 - 第二行 -->
      <div class="workflow-section">
        <div class="workflow-row">
          <WorkflowNode 
            v-for="(node, index) in secondRowNodes" 
            :key="node.title"
            :node-data="node"
            :has-left-arrow="index < secondRowNodes.length - 1"
          />
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import {
  ref,
  reactive,
  getCurrentInstance,
  onMounted,
} from "vue";
import WorkflowNode from "../components/WorkflowNode.vue";

export default {
  components: {
    WorkflowNode
  },
  setup(props) {
    const { proxy } = getCurrentInstance();
    const deliveryDays = ref(0);
    const selectedOrder = ref("所有");
    const selectedYear = ref("2025");
    const selectedMonth = ref(4);
    const months = [1, 2, 3, 4, 5, 6, 7, 8, 9, 11, 12];

    // 合同评审数据
    const contractReviewData = reactive({
      rate: "20.00%",
      totalTasks: 0,
      pendingTasks: 0,
      overdueIncomplete: 0,
      overdueComplete: 4,
      standardRatio: "6.67%",
      standardDays: 0
    });

    // 产品出码数据
    const productCodeData = reactive({
      rate: "17.89%",
      totalTasks: 738,
      pendingTasks: 0,
      overdueIncomplete: 0,
      overdueComplete: 348,
      standardRatio: "1.67%",
      standardDays: 0
    });

    // 产品设计数据
    const productDesignData = reactive({
      rate: "7.45%",
      totalTasks: 738,
      pendingTasks: 0,
      overdueIncomplete: 1,
      overdueComplete: 628,
      standardRatio: "3.33%",
      standardDays: 0
    });

    // 合同录入数据
    const contractEntryData = reactive({
      rate: "100%",
      totalTasks: 738,
      pendingTasks: 0,
      overdueIncomplete: 0,
      overdueComplete: 0,
      standardRatio: "3.33%",
      standardDays: 0
    });

    // 装配完工数据
    const assemblyData = reactive({
      rate: "29.71%",
      totalTasks: 589,
      pendingTasks: 48,
      overdueIncomplete: 37,
      overdueComplete: 280,
      standardRatio: "5.00%",
      standardDays: 0
    });

    // 物流配送数据
    const logisticsData = reactive({
      rate: "77.91%",
      totalTasks: 611,
      pendingTasks: 17,
      overdueIncomplete: 5,
      overdueComplete: 51,
      standardRatio: "3.33%",
      standardDays: 0
    });

    // 物料准备数据
    const materialData = reactive({
      rate: "56.17%",
      totalTasks: 721,
      pendingTasks: 73,
      overdueIncomplete: 27,
      overdueComplete: 177,
      standardRatio: "73.34%",
      standardDays: 0
    });

    // 计划确认数据
    const planConfirmData = reactive({
      rate: "96.79%",
      totalTasks: 717,
      pendingTasks: 0,
      overdueIncomplete: 0,
      overdueComplete: 7,
      standardRatio: "3.33%",
      standardDays: 0
    });

    // 流程节点数据 - 第一行
    const firstRowNodes = [
      {
        title: "合同评审",
        ...contractReviewData,
        pendingLabel: "待评审"
      },
      {
        title: "产品出码",
        ...productCodeData,
        pendingLabel: "待出码"
      },
      {
        title: "产品设计",
        ...productDesignData,
        pendingLabel: "待设计"
      },
      {
        title: "合同录入",
        ...contractEntryData,
        pendingLabel: "待录入"
      }
    ];

    // 流程节点数据 - 第二行
    const secondRowNodes = [
      {
        title: "装配完工",
        ...assemblyData,
        pendingLabel: "待完工"
      },
      {
        title: "物流配送",
        ...logisticsData,
        pendingLabel: "待配送"
      },
      {
        title: "物料准备",
        ...materialData,
        pendingLabel: "待准备"
      },
      {
        title: "计划确认",
        ...planConfirmData,
        pendingLabel: "待确认"
      }
    ];

    onMounted(() => {
      // 可以在这里加载数据
    });

    return {
      deliveryDays,
      selectedOrder,
      selectedYear,
      selectedMonth,
      months,
      contractReviewData,
      productCodeData,
      productDesignData,
      contractEntryData,
      assemblyData,
      logisticsData,
      materialData,
      planConfirmData,
      firstRowNodes,
      secondRowNodes
    };
  },
};
</script>

<style lang="less" scoped>
.container {
  display: flex;
  width: 100%;
  height: 100%;
}

// 添加不同主题色下的样式
// 深色主题
.vol-theme-dark-aside {
  background-color: #001529;
  color: #fff;
  
  .header {
    background-color: #021d37;
  }
  
  .stats-section {
    background-color: #021d37;
  }

  .month-selected {
    background-color: #021d37;
    border-color: #021d37;
  }
  
  .filter-section {
    box-shadow: 0 2px 5px rgba(0, 21, 41, 0.3);
  }
  
  .el-select .el-input.is-focus .el-input__inner {
    border-color: #021d37;
  }

  .arrow-bottom {
    color: #021d37;
  }

  .stats-label {
    background: #fff;
    color: #021d37;
  }
}

// 红色主题
.vol-theme-red-aside {
  .header {
    background-color: rgb(237, 64, 20);
  }
  
  .stats-section {
    background-color: rgb(237, 64, 20);
  }
  
  .month-selected {
    background-color: rgb(237, 64, 20);
    border-color: rgb(237, 64, 20);
  }
  
  .filter-section {
    box-shadow: 0 2px 5px rgba(237, 64, 20, 0.1);
  }
  
  .el-select .el-input.is-focus .el-input__inner {
    border-color: rgb(237, 64, 20);
  }

  .arrow-bottom {
    color: rgb(237, 64, 20);
  }

  .stats-label {
    background: #fff;
    color: rgb(237, 64, 20);
  }
}

// 橙色主题
.vol-theme-orange-aside {
  .header {
    background-color: rgb(255, 153, 0);
  }
  
  .stats-section {
    background-color: rgb(255, 153, 0);
  }

  .month-selected {
    background-color: rgb(255, 153, 0);
    border-color: rgb(255, 153, 0);
  }
  
  .filter-section {
    box-shadow: 0 2px 5px rgba(255, 153, 0, 0.1);
  }
  
  .el-select .el-input.is-focus .el-input__inner {
    border-color: rgb(255, 153, 0);
  }

  .arrow-bottom {
    color: rgb(255, 153, 0);
  }

  .stats-label {
    background: #fff;
    color: rgb(255, 153, 0);
  }
}

// 橙色渐变主题
.vol-theme-gradient_orange-aside {
  .header {
    background: linear-gradient(to bottom, #fdb890 60%, #fadaaa);
  }
  
  .stats-section {
    background: linear-gradient(to bottom, #fdb890 60%, #fae6aa);
  }
  
  .month-selected {
    background-color: #fdb890;
    border-color: #fdb890;
  }
  
  .filter-section {
    box-shadow: 0 2px 5px rgba(253, 184, 144, 0.1);
  }
  
  .el-select .el-input.is-focus .el-input__inner {
    border-color: #fdb890;
  }

  .arrow-bottom {
    color: #fdb890;
  }

  .stats-label {
    background: #fff;
    color: rgb(255, 153, 0);
  }
}

// 绿色主题
.vol-theme-green-aside {
  .header {
    background-color: rgb(25, 190, 107);
  }
  
  .stats-section {
    background-color: rgb(25, 190, 107);
  }
  
  .month-selected {
    background-color: rgb(25, 190, 107);
    border-color: rgb(25, 190, 107);
  }
  
  .filter-section {
    box-shadow: 0 2px 5px rgba(25, 190, 107, 0.1);
  }
  
  .el-select .el-input.is-focus .el-input__inner {
    border-color: rgb(25, 190, 107);
  }

  .arrow-bottom {
    color: rgb(25, 190, 107);
  }

  .stats-label {
    background: #fff;
    color: rgb(25, 190, 107);
  }
}

// 蓝色主题
.vol-theme-blue-aside {
  .header {
    background-color: rgb(45, 140, 240);
  }
  
  .stats-section {
    background-color: rgb(45, 140, 240);
  }
 
  .month-selected {
    background-color: rgb(45, 140, 240);
    border-color: rgb(45, 140, 240);
  }
  
  .filter-section {
    box-shadow: 0 2px 5px rgba(45, 140, 240, 0.1);
  }
  
  .el-select .el-input.is-focus .el-input__inner {
    border-color: rgb(45, 140, 240);
  }

  .arrow-bottom {
    color: rgb(45, 140, 240);
  }

  .stats-label {
    background: #fff;
    color: rgb(45, 140, 240);
  }
}

// 白色主题
.vol-theme-white-aside {
  background-color: #fff;
  
  .header {
    background-color: rgb(45, 140, 240);
  }
  
  .stats-section {
    background-color: rgb(45, 140, 240);
  }

  .month-selected {
    background-color: rgb(45, 140, 240);
    border-color: rgb(45, 140, 240);
  }
  
  .filter-section {
    box-shadow: 0 2px 5px rgba(45, 140, 240, 0.1);
  }
  
  .el-select .el-input.is-focus .el-input__inner {
    border-color: rgb(45, 140, 240);
  }

  .arrow-bottom {
    color: rgb(45, 140, 240);
  }

  .stats-label {
    background: #fff;
    color: rgb(45, 140, 240);
  }
}

/* 主要内容区 */
.main-content {
  flex: 1;
  padding: 20px;
  display: flex;
  flex-direction: column;
}

/* 标题 */
.header {
  text-align: center;
  padding: 15px 0;
  background-color: rgb(45, 140, 240); // 默认使用蓝色主题色
  color: white;
  font-size: 22px;
  font-weight: bold;
  margin-bottom: 20px;
  border-radius: 5px;
  position: relative;
}

.logo {
  position: absolute;
  right: 20px;
  top: 50%;
  transform: translateY(-50%);
  width: 50px;
  height: 30px;
  background-color: #ff6b6b;
  border-radius: 5px;
  display: flex;
  align-items: center;
  justify-content: center;
  color: white;
  font-weight: bold;
}

/* 筛选区域 */
.filter-section {
  display: flex;
  background-color: #fff;
  padding: 15px;
  border-radius: 5px;
  margin-bottom: 20px;
  box-shadow: 0 2px 5px rgba(45, 140, 240, 0.1); // 默认使用蓝色主题色
}

.filter-item {
  display: flex;
  align-items: center;
  margin-right: 20px;
}

.filter-label {
  margin-right: 10px;
  white-space: nowrap;
  font-size: 14px;
}

.filter-control {
  min-width: 120px;
}

.month-item {
  width: 40px;
  height: 40px;
  display: flex;
  align-items: center;
  justify-content: center;
  background-color: white;
  color: #333;
  margin: 0 5px 5px 0;
  cursor: pointer;
  border: 1px solid #ddd;
  border-radius: 3px;
  transition: all 0.3s ease;
}

.month-selected {
  background-color: rgb(45, 140, 240); // 默认使用蓝色主题色
  color: white;
  border-color: rgb(45, 140, 240); // 默认使用蓝色主题色
}

/* 统计区域 */
.stats-section {
  background-color: rgb(45, 140, 240); // 默认使用蓝色主题色
  color: white;
  padding: 15px;
  border-radius: 5px;
  margin-bottom: 20px;
  display: flex;
  align-items: center;
  justify-content: center;
  box-shadow: 0 2px 5px rgba(0,0,0,0.1);
  position: relative;

  .stats-container {
    display: flex;
    position: absolute;
    left: 20px;
    top: 50%;
    transform: translateY(-50%);
    align-items: center;
  }
}

.stats-label {
  background: #fff;
  border-radius: 50%;
  width: 32px;
  height: 32px;
  line-height: 32px;
  margin-right: 8px;
  text-align: center;
  color: rgb(45, 140, 240);
}

.stats-count {
  margin-right: 10px;
  font-weight: bold;
  font-size: 16px;
}

.stats-value {
  font-size: 24px;
  color: #ff6b6b;
  font-weight: bold;
  margin: 0 5px;
}

/* 流程节点区域 */
.workflow-section {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: 70px;
  margin-bottom: 50px;
  padding-left: 20px;
  position: relative;
}

.workflow-row {
  display: contents; 
}

.arrow-bottom {
  position: absolute;
  bottom: -40px;
  right: 10%;
  transform: rotate(90deg);
  font-size: 24px;
  animation: arrow 2s infinite;
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
  top: 50%;
  transform: translateY(-50%);
  z-index: 2;
}

.completion-rate {
  font-size: 14px;
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

@keyframes arrow {
  0% { opacity: 0.3; }
  50% { opacity: 1; }
  100% { opacity: 0.3; }
}


</style>
