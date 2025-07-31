# 留言板组件

一个功能完整的留言板组件，支持消息分类、部门筛选、消息展示和回复功能。

## 功能特性

### 🎯 核心功能
- **消息分类**：支持催单、协商、留言三种消息类型
- **部门筛选**：支持按不同部门（销售、技术、计划、装配、金工、委外、外购）筛选消息
- **消息展示**：清晰展示消息内容、发送人、部门、时间等信息
- **回复功能**：支持文本回复，键盘快捷键操作
- **快捷回复**：预设常用回复内容，一键发送
- **快捷操作**：已处理、需要时间、转交他人等快捷按钮

### 📊 统计功能
- 实时显示各类型消息数量
- 已回复/待回复消息统计
- 总消息数量统计

### 🎨 用户体验
- 响应式设计，适配不同屏幕尺寸
- 流畅的动画效果和交互反馈
- 自定义滚动条样式
- 键盘快捷键支持（Enter发送，Shift+Enter换行）

## 使用方法

### 基础用法

```vue
<template>
  <div>
    <el-button @click="openMessageBoard">打开留言板</el-button>
    <MessageBoard ref="messageBoardRef" />
  </div>
</template>

<script setup>
import { ref } from 'vue'
import MessageBoard from '@/comp/message-board/index.vue'

const messageBoardRef = ref(null)

const openMessageBoard = () => {
  messageBoardRef.value?.open()
}
</script>
```

### API

#### 方法

| 方法名 | 说明 | 参数 |
|--------|------|------|
| open() | 打开留言板 | - |
| close() | 关闭留言板 | - |

#### 事件

组件内部处理所有交互事件，无需外部监听。

## 组件结构

```
message-board/
├── index.vue          # 主组件文件
├── demo.vue           # 演示页面
└── README.md          # 说明文档
```

## 数据结构

### 消息对象

```javascript
{
  id: 1,                    // 消息ID
  username: '张小明',        // 发送人姓名
  dept: '销售部',           // 部门名称
  type: 'urgent',           // 消息类型：urgent(催单) | negotiate(协商) | message(留言)
  content: '消息内容...',    // 消息内容
  time: '2024-06-01 14:30', // 发送时间
  isReplied: false,         // 是否已回复
  deptKey: 'sales'          // 部门键值，用于筛选
}
```

### 部门配置

```javascript
{
  key: 'sales',      // 部门键值
  label: '销售',     // 显示名称
  count: 15          // 消息数量
}
```

## 样式定制

组件使用Element Plus的CSS变量，可以通过修改CSS变量来定制主题：

```css
:root {
  --el-color-primary: #409EFF;
  --el-color-success: #67C23A;
  --el-color-warning: #E6A23C;
  --el-color-danger: #F56C6C;
}
```

## 技术栈

- **Vue 3.4+** - 使用Composition API
- **Element Plus 2.9+** - UI组件库
- **JavaScript** - 开发语言

## 浏览器兼容性

- Chrome 88+
- Firefox 78+
- Safari 14+
- Edge 88+

## 更新日志

### v1.0.0 (2024-06-01)
- ✨ 初始版本发布
- 🎯 支持消息分类和部门筛选
- 💬 实现回复功能
- 📊 添加统计功能
- 🎨 优化用户界面和交互体验

## 开发计划

### 待实现功能
- [ ] 消息搜索功能
- [ ] 消息导出功能
- [ ] 用户@提醒功能
- [ ] 消息已读/未读状态
- [ ] 实时消息推送
- [ ] 消息模板功能

### 优化计划
- [ ] 虚拟滚动优化大量消息性能
- [ ] 国际化支持
- [ ] 主题切换功能
- [ ] 移动端适配优化

## 贡献指南

1. Fork 项目
2. 创建功能分支 (`git checkout -b feature/AmazingFeature`)
3. 提交更改 (`git commit -m 'Add some AmazingFeature'`)
4. 推送到分支 (`git push origin feature/AmazingFeature`)
5. 打开 Pull Request

## 许可证

本项目采用 MIT 许可证 - 查看 [LICENSE](LICENSE) 文件了解详情。 