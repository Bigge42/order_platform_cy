<template>
  <el-scrollbar :max-height="450">
    <div
      class="msg-item"
      :class="{ 'item-read': item.isRead == 1 }"
      @click="itemClick(item)"
      v-for="(item, index) in list"
      :key="index"
    >
      <div class="title">
        {{ item.notificationTitle }}
      </div>
      <div class="desc" v-html="item.notificationContent"></div>
      <div class="bottom">
        <div class="tag">
          <el-tag :type="item.notificationType == '审批' ? 'success' : item.tag" size="small">{{
            item.notificationType || '系统'
          }}</el-tag>
        </div>
        <div class="tag">{{ item.creator || '' }}</div>
        <div class="date">{{ item.createDate || '' }}</div>
      </div>
    </div>
    <vol-empty v-show="!list.length"></vol-empty>
  </el-scrollbar>
</template>
<script setup>
import VolEmpty from '@/components/basic/VolEmpty.vue'
import { ref, getCurrentInstance } from 'vue'
const { proxy } = getCurrentInstance()

const props = defineProps({
  list: {
    type: Array,
    default: () => {
      return []
    }
  }
})
const emit = defineEmits(['itemClick'])
const itemClick = (item) => {
  emit('itemClick', item, item.isRead)
  if (item.isRead) {
    return
  }
  item.isRead = 1
  const url = 'api/Sys_Notification/read?id=' + item.id
  proxy.http.get(url, {}, false).then((x) => {})
}
</script>
<style scoped lang="less">
.msg-item {
  border-bottom: 1px solid #eee;
  padding: 10px;
  .title {
    font-weight: bolder;
    font-size: 13px;
    color: #000;
  }
  .desc {
    margin-top: 5px;
    margin-bottom: 5px;
    line-height: 1.3;
    font-size: 12px;
    color: #676565;
    display: -webkit-box;
    -webkit-box-orient: vertical;
    -webkit-line-clamp: 2;
    overflow: hidden;
    text-overflow: ellipsis;
    word-wrap: break-word;
    ::v-deep(> p:first-child) {
      margin: 0;
    }
  }
  .bottom {
    display: flex;
    margin-top: 5px;
    font-size: 12px;
    color: #676565;
  }
  .tag {
    flex: 1;
    padding-right: 5px;
  }
}
.item-read {
  .title {
    color: #979797;
  }
  .desc {
    color: #939191;
  }
}
.msg-item:hover {
  cursor: pointer;
  background: #f9f9f9;
}
</style>
