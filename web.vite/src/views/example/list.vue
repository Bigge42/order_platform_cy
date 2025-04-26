<template>
  <div class="list-container">
    <el-scrollbar style="height: 100%">
      <div class="search-content">
        <label>开始日期：</label>
        <el-date-picker
          style="width: 150px"
          v-model="searchValue.beginDate"
          placeholder="开始日期"
          value-format="YYYY-MM-DD"
        ></el-date-picker>
        <label class="mg-item">结束日期：</label>
        <el-date-picker
          style="width: 150px"
          v-model="searchValue.endDate"
          value-format="YYYY-MM-DD"
          placeholder="结束日期"
        ></el-date-picker>
        <el-button type="primary" plain icon="Search" @click="getData(true)">搜索</el-button>
      </div>
      <div class="list-content">
        <div
          class="list-item"
          @click="itemClick(item, index)"
          v-for="(item, index) in list"
          :key="index"
        >
          <div class="production-name">
            {{ item.工序名称 }}
          </div>
          <div class="content">
            <div class="content-left">
              <div class="data">
                {{ item.生产任务数 || 0 }}
                <div class="data-name">生产任务数</div>
              </div>
            </div>
            <div class="content-right">
              <div class="content-right-item">
                <div class="txt">计划数</div>
                <span class="content-right-value">{{ item.计划数 }}</span>
              </div>
              <div class="content-right-item">
                <div class="txt">良品数</div>
                <span class="content-right-value">{{ item.良品数 }}</span>
              </div>
              <div class="content-right-item">
                <div class="txt">不良品数</div>
                <span class="content-right-value">{{ item.不良品数 }}</span>
              </div>
            </div>
          </div>
          <div class="content-bottom">
            <div class="num-01">进度:</div>
            <div class="progress" :class="{ success: item.完成进度 >= 100 }">
              <el-progress :stroke-width="5" :percentage="item.完成进度" />
            </div>
          </div>
        </div>
      </div>
    </el-scrollbar>
  </div>
</template>
<script setup>
import {
  defineComponent,
  ref,
  reactive,
  toRefs,
  getCurrentInstance,
  onMounted,
  onUnmounted
} from 'vue'
import { useRouter, useRoute } from 'vue-router'
const { proxy } = getCurrentInstance()

const searchValue = reactive({
  beginDate: null,
  endDate: null
})

const dateNow = proxy.base.getDate()
searchValue.beginDate = proxy.base.addDays(dateNow, -90)
searchValue.endDate = dateNow

const list = reactive([])

list.push(
  ...[
    { 工序名称: '装配', 生产任务数: 100, 计划数: 90, 良品数: 85, 不良品数: 5, 完成进度: 100 },
    { 工序名称: '加工', 生产任务数: 120, 计划数: 110, 良品数: 105, 不良品数: 5, 完成进度: 60 },
    { 工序名称: '包装', 生产任务数: 80, 计划数: 70, 良品数: 65, 不良品数: 5, 完成进度: 92.86 },
    { 工序名称: '清洗', 生产任务数: 90, 计划数: 80, 良品数: 75, 不良品数: 5, 完成进度: 33.75 },
    { 工序名称: '焊接', 生产任务数: 110, 计划数: 100, 良品数: 95, 不良品数: 5, 完成进度: 50 },
    { 工序名称: '涂装', 生产任务数: 85, 计划数: 75, 良品数: 70, 不良品数: 5, 完成进度: 82.35 },
    { 工序名称: '质检', 生产任务数: 130, 计划数: 120, 良品数: 115, 不良品数: 5, 完成进度: 100 },
    { 工序名称: '打磨', 生产任务数: 95, 计划数: 85, 良品数: 80, 不良品数: 5, 完成进度: 84.21 },
    { 工序名称: '分拣', 生产任务数: 105, 计划数: 95, 良品数: 90, 不良品数: 5, 完成进度: 55.71 },
    { 工序名称: '喷漆', 生产任务数: 120, 计划数: 110, 良品数: 105, 不良品数: 5, 完成进度: 77.5 },
    { 工序名称: '组装', 生产任务数: 100, 计划数: 90, 良品数: 85, 不良品数: 5, 完成进度: 25 },
    { 工序名称: '清洗', 生产任务数: 105, 计划数: 95, 良品数: 90, 不良品数: 5, 完成进度: 15.71 },
    { 工序名称: '去油', 生产任务数: 125, 计划数: 115, 良品数: 110, 不良品数: 5, 完成进度: 100 },
    { 工序名称: '酸洗', 生产任务数: 130, 计划数: 120, 良品数: 115, 不良品数: 5, 完成进度: 78.46 },
    { 工序名称: '绕线', 生产任务数: 140, 计划数: 130, 良品数: 125, 不良品数: 5, 完成进度: 89.29 }
  ]
)

const getData = (showMsg) => {
  ////从后台获取数据
  const url = `api/表名/getData?beginDate=${searchValue.beginDate}&endDate=${searchValue.endDate}`
  // proxy.http.get(url,{},true).then((result) => {
     if(showMsg){
        proxy.$message.success('查询成功')
     }
       
  //   list.splice(0)//清空数据    
  //   list.push(
  //     ...result.map((c) => {
  //       return { name: c.value, table: c.key, type: '审批' }
  //     })
  //   )
  // })
}
getData()

onMounted(() => {})
</script>
<style lang="less" scoped>
.list-container {
  position: absolute;
  height: 100%;
  width: 100%;
  background: #f3f7fb;
}
.list-content {
  height: 100%;
  padding: 15px 15px 0 15px;
  display: grid;
  -moz-column-gap: 12px;
  column-gap: 12px;
  //上面汇总显示的数量
  grid-template-columns: repeat(5, auto);
}

.list-item {
  position: relative;
  cursor: pointer;
  display: flex;
  flex-direction: column;
  justify-content: center;
  background: #ffffff;
  border-radius: 5px;
  height: 120px;
  overflow: hidden;
  border: 1px solid #eee;
  box-shadow: 4px 5px 10px 1px #f7f7f7;
  margin-bottom: 12px;
  .content {
    position: relative;

    display: flex;
    align-items: center;
    .content-left {
      flex: 1;
      color: #1d252f;
      padding: 0 20px;

      .el-icon-warning-outline {
        margin-right: 5px;
      }
    }
    .content-right {
      font-size: 13px;
      padding-right: 20px;
      .content-right-item {
        display: flex;
        align-items: center;
        line-height: 20px;
        .txt {
          width: 70px;
          text-align: right;
          color: #7d7d7d;
          padding-right: 8px;
        }
      }
    }
    .name {
      color: #7d7b7b;
      font-size: 14px;
      font-weight: 400;
      padding-bottom: 5px;
    }

    .data {
      font-size: 19px;
      font-family: Source Han Sans CN, sans-serif;
      color: #505050;
      font-weight: bold;
      letter-spacing: 1px;
      padding-top: 15px;
      .data-name {
        color: #272626;
        font-weight: 500;
        font-size: 14px;
      }
    }
  }
}
.content-bottom {
  align-items: center;
  display: flex;
  margin-top: 6px;
  padding: 10px 4px 0 20px;
  border-top: 1px solid #eee;
  ::v-deep(.el-progress-bar__inner) {
    background-image: linear-gradient(to right, #0fc8fe, #4768ff);
  }
  .success ::v-deep(.el-progress-bar__inner) {
    background-image: linear-gradient(to right, #64b804, #00b707);
  }

  .num-01 {
    margin-right: 5px;
    font-size: 13px;
    color: #837e7e;
  }
  .progress {
    flex: 1;
  }
}
.production-name {
    position: absolute;
    top: 0;
    left: 0;
    font-size: 12px;
    padding: 2px 11px 3px;
    background: #cee9ff69;
    border-bottom-right-radius: 9px;
    color: #028fff;
    border-top-right-radius: 5px;
}

.search-content {
  margin: 15px 15px 0 15px;
  background: #fff;
  padding: 13px;
  label {
    font-size: 14px;
    color: #6a6565;
  }
  .mg-item,
  button {
    margin-left: 20px;
  }
}
</style>
