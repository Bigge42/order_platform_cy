# AI小助手平台智能体应用集成需求方案

> 版本：v1.0 | 日期：2026-04-15 | 状态：需求原型

---

## 一、项目概述

### 1.1 背景

基于当前协同平台（OCP）框架，集成 AI小助手平台搭建的各类 AI 智能体应用。PC 端协同平台负责智能体管理与权限控制，手机端 H5 提供智能体应用列表展示和对话交互能力，按 PC 端角色权限控制可见应用范围。

### 1.2 目标

- 在 OCP 协同平台中集成 AI小助手平台的智能体应用，实现 AI 能力的统一接入
- PC 端提供管理后台，支持应用管理、角色授权
- 手机端 H5 提供对话入口，支持流式输出、Markdown 渲染、表格、图表等多种内容展示
- 统一权限控制，安全隔离 API 密钥，前端不直接接触 AI小助手平台

### 1.3 技术栈

| 端 | 框架 | UI 库 | 目录 |
|---|---|---|---|
| PC 端 | Vue 3 + Vite | Element Plus | `web.vite/` |
| 手机端 | uni-app (Vue 3) | uview-plus | `app/` |
| 后端 | .NET 8 / ASP.NET Core | — | `api/` |
| AI 平台 | AI小助手平台（自部署） | — | `http://10.11.10.101` |

### 1.4 整体架构

> **📷 截图 1-1：系统整体架构图**
>
> 原型来源：`AI小助手平台智能体应用原型交互图.html` → 「架构图」Tab 页

**关键设计原则：**

- 前端不直接调用 AI小助手平台 API，统一经后端转发（安全隔离 AppKey）
- 权限复用现有角色体系，通过「角色-AI应用」授权表控制可见范围
- AI小助手平台用户与 OCP 用户默认以员工号直接映射，无需额外映射表
- SSE 流式输出通过后端透传，支持实时对话体验

---

## 二、PC 端功能设计

### 2.1 AI 应用管理

**菜单位置：** 系统管理 → AI 应用管理

**页面路由：** `/sys/ai-app`

**功能描述：** 管理 AI小助手平台的智能体应用信息，支持从 AI小助手平台同步或手动维护。

#### 2.1.1 列表页

> **📷 截图 2-1：PC端 - AI应用管理列表页**
>
> 原型来源：`AI小助手平台智能体应用原型交互图.html` → 「AI应用管理」Tab 页

**功能点：**

| 功能 | 说明 |
|------|------|
| 从AI小助手平台同步 | 一键从 AI小助手平台拉取应用列表，已存在的更新基本信息，新增的自动创建 |
| 新增 | 手动新增 AI 应用，填写名称、类型、AppID、AppKey 等 |
| 删除 | 批量或单条删除（逻辑删除） |
| 搜索 | 按应用名称模糊搜索 |
| 编辑 | 修改应用配置信息 |

**列表字段：** 勾选框、应用名称、应用类型（对话型/Agent/Workflow）、平台 AppID、应用图标、描述、状态（启用/停用）、操作（编辑/删除）

#### 2.1.2 新增/编辑弹窗

> **📷 截图 2-2：PC端 - 新增AI应用弹窗**
>
> 原型来源：`AI小助手平台智能体应用原型交互图.html` → 「AI应用管理」Tab 页 → 点击「新增」按钮

**表单字段：**

| 字段 | 类型 | 必填 | 说明 |
|------|------|------|------|
| 应用名称 | 文本输入 | 是 | 应用的显示名称 |
| 应用类型 | 下拉选择 | 是 | 对话型(Chat) / Agent / Workflow |
| 平台 AppID | 文本输入 | 是 | AI小助手平台应用ID |
| AppKey | 密码输入 | 是 | AI小助手平台 AppKey（加密存储） |
| 应用描述 | 文本域 | 否 | 应用功能简介 |
| 应用图标 | 图标选择/上传 | 否 | 预设图标或自定义上传 |
| 排序号 | 数字输入 | 否 | 列表排序，默认 0 |
| 状态 | 单选 | 是 | 启用 / 停用 |

#### 2.1.3 从 AI小助手平台同步弹窗

> **📷 截图 2-3：PC端 - 从AI小助手平台同步应用弹窗**
>
> 原型来源：`AI小助手平台智能体应用原型交互图.html` → 「AI应用管理」Tab 页 → 点击「从AI小助手平台同步」按钮

**同步逻辑：**

- 调用 AI小助手平台 API 获取全部应用列表
- 列表中标注每个应用的状态：「已存在」或「新增」
- 勾选需要同步的应用，点击确认同步
- 已存在的应用将更新基本信息（名称、类型、描述），AppKey 不会覆盖
- 新增的应用自动创建记录

#### 2.1.4 数据模型

```sql
-- AI应用表
CREATE TABLE Sys_AIApp (
    Id            BIGINT PRIMARY KEY IDENTITY,
    AppName       NVARCHAR(100) NOT NULL,      -- 应用名称
    AppType       NVARCHAR(20)  NOT NULL,       -- 应用类型: chat/agent/workflow
    PlatformAppId VARCHAR(64)   NOT NULL,       -- AI小助手平台应用ID
    PlatformAppKey VARCHAR(128) NOT NULL,       -- AI小助手平台AppKey（加密存储）
    Description   NVARCHAR(500),                -- 应用描述
    Icon          NVARCHAR(200),                -- 图标URL
    SortNo        INT DEFAULT 0,                -- 排序号
    Status        INT DEFAULT 1,                -- 状态: 1启用 0停用
    CreateDate    DATETIME DEFAULT GETDATE(),
    Creator       NVARCHAR(50),
    ModifyDate    DATETIME,
    Modifier      NVARCHAR(50)
);
```

---

### 2.2 角色-AI 应用授权管理

**菜单位置：** 系统管理 → 角色管理 → 编辑角色 → AI 应用授权 Tab

**功能描述：** 在现有角色编辑页面增加一个 Tab 页，配置该角色可访问的 AI 应用。

#### 2.2.1 角色编辑页 - AI 应用授权 Tab

> **📷 截图 2-4：PC端 - 角色AI应用授权配置页**
>
> 原型来源：`AI小助手平台智能体应用原型交互图.html` → 「角色授权」Tab 页

**交互说明：**

- 在现有角色编辑页面新增「AI应用授权」Tab 页
- 使用穿梭框（Transfer）组件，左侧为「可用应用」，右侧为「已授权应用」
- 支持全选、单选、移入、移出操作
- 保存后立即生效

#### 2.2.2 数据模型

```sql
-- 角色-AI应用授权表
CREATE TABLE Sys_RoleAIApp (
    Id            BIGINT PRIMARY KEY IDENTITY,
    RoleId        INT NOT NULL,                 -- 角色ID (关联 Sys_Role)
    AIAppId       BIGINT NOT NULL,              -- AI应用ID (关联 Sys_AIApp)
    CreateDate    DATETIME DEFAULT GETDATE(),
    Creator       NVARCHAR(50),
    UNIQUE (RoleId, AIAppId)
);
```

---

### 2.3 AI小助手平台 API 转发设计

**设计原则：** 后端作为代理，前端不直接接触 AI小助手平台的 AppKey 和 API 地址。

#### 2.3.1 转发接口设计

```
后端统一转发路由：POST /api/AI/{action}

前端请求 → OCP 后端 → 根据应用ID查 AppKey → 注入 Authorization → 转发 AI小助手平台 API → 返回
```

| OCP 接口 | 转发到 AI小助手平台 API | 说明 |
|---------|----------------|------|
| `POST /api/AI/SendMessage` | `POST /v1/chat-messages` | 发送对话消息 |
| `POST /api/AI/StopMessage` | `POST /v1/chat-messages/:task_id/stop` | 停止响应 |
| `GET /api/AI/Conversations` | `GET /v1/conversations` | 获取会话列表 |
| `GET /api/AI/Messages` | `GET /v1/messages` | 获取会话历史 |
| `DELETE /api/AI/Conversation` | `DELETE /v1/conversations/:id` | 删除会话 |
| `POST /api/AI/RenameConversation` | `POST /v1/conversations/:id/name` | 会话重命名 |
| `POST /api/AI/MessageFeedback` | `POST /v1/messages/:id/feedbacks` | 消息反馈 |
| `GET /api/AI/SuggestedQuestions` | `GET /v1/messages/:id/suggested` | 建议问题 |
| `GET /api/AI/ConversationVariables` | `GET /v1/conversations/:id/variables` | 对话变量 |
| `POST /api/AI/UpdateVariables` | `PATCH /v1/conversations/:id/variables` | 更新变量 |
| `GET /api/AI/AppInfo` | `GET /v1/info` | 应用信息 |
| `GET /api/AI/AppParameters` | `GET /v1/parameters` | 应用参数 |
| `GET /api/AI/AppMeta` | `GET /v1/meta` | 应用Meta |
| `POST /api/AI/FileUpload` | `POST /v1/files/upload` | 文件上传 |
| `GET /api/AI/FilePreview` | `GET /v1/files/:id/preview` | 文件预览 |

#### 2.3.2 转发流程（流式输出）

```
手机端                    OCP 后端                         AI小助手平台
  │                         │                              │
  │── SSE /api/AI/SendMsg──▶│                              │
  │                         │── POST /v1/chat-messages ──▶│
  │                         │   (response_mode: streaming) │
  │                         │◀── SSE event: message ──────│
  │◀── SSE event: message ──│                              │
  │                         │◀── SSE event: message ──────│
  │◀── SSE event: message ──│                              │
  │                         │◀── SSE event: message_end ──│
  │◀── SSE event: end ──────│                              │
```

---

## 三、手机端 H5 功能设计

### 3.1 整体导航规划

在现有 uni-app TabBar 中新增「AI」Tab 页：

| 原有 TabBar | 新增后 TabBar |
|------------|--------------|
| 菜单 / 流程 / 报表 / 我的 | 菜单 / 流程 / **AI** / 报表 / 我的 |

---

### 3.2 AI 应用列表页

**页面路由：** `pages/ai/index`

> **📷 截图 3-1：手机端 - AI应用列表页**
>
> 原型来源：`AI小助手平台智能体应用原型交互图.html` → 「应用列表」Tab 页

**页面结构：**

| 区域 | 说明 |
|------|------|
| 顶部导航栏 | 标题「AI 助手」 |
| 搜索栏 | 按应用名称模糊搜索 |
| 我的应用 | 宫格卡片展示用户有权限的 AI 应用（图标 + 名称 + 描述 + 在线状态） |
| 最近对话 | 展示最近的对话记录（应用图标 + 应用名 + 最后消息预览 + 时间） |
| 底部 TabBar | 菜单 / 流程 / AI(选中) / 报表 / 我的 |

**应用卡片组件：**

| 元素 | 说明 |
|------|------|
| 图标 | 应用图标，支持 Emoji 或自定义图片 |
| 名称 | 应用名称，最多显示 6 字 |
| 描述 | 一句话描述，最多 2 行 |
| 状态指示 | 绿色圆点 + "在线" 文字 |

---

### 3.3 对话会话列表页

**页面路由：** `pages/ai/conversations`

**进入方式：** 点击应用卡片进入该应用的会话列表

> **📷 截图 3-2：手机端 - 会话列表页**
>
> 原型来源：`AI小助手平台智能体应用原型交互图.html` → 「会话列表」Tab 页

**页面结构：**

| 区域 | 说明 |
|------|------|
| 顶部导航栏 | 返回按钮 + 应用名称 + 「新对话」按钮 |
| 会话列表 | 卡片列表，每个卡片显示：会话标题、最后消息预览、时间、更多操作(···) |
| 更多操作 | 点击 ··· 弹出底部菜单：重命名会话 / 删除会话 |

---

### 3.4 对话界面

**页面路由：** `pages/ai/chat`

#### 3.4.1 基础对话 - 流式输出

> **📷 截图 3-3：手机端 - 对话界面（流式输出中）**
>
> 原型来源：`AI小助手平台智能体应用原型交互图.html` → 「对话界面」Tab 页 → 左侧手机

**交互说明：**

- AI 欢迎消息在新会话创建时自动显示
- 用户发送消息后，AI 回复采用 SSE 流式输出，文字逐字显示
- 流式输出期间显示光标闪烁动画
- 输入区域切换为「正在回复中...」+ 停止按钮

#### 3.4.2 完整对话 - Markdown + 表格 + 建议问题

> **📷 截图 3-4：手机端 - 对话界面（Markdown + 表格 + 建议问题）**
>
> 原型来源：`AI小助手平台智能体应用原型交互图.html` → 「对话界面」Tab 页 → 右侧手机

**消息渲染能力：**

| 内容类型 | 渲染方式 | 说明 |
|---------|---------|------|
| 纯文本 | 直接渲染 | 支持换行、特殊字符 |
| Markdown | `marked` + `highlight.js` | 标题、加粗、列表、引用、代码块 |
| 表格 | HTML Table | 支持横向滚动，响应式适配 |
| 图片 | Image 组件 | 点击全屏预览（`uni.previewImage`） |
| 图表 | ECharts | 识别 `chart` 代码块，转换为图表渲染 |
| 文件 | 文件卡片 | 显示文件名、类型图标、大小 |

**建议问题：** AI 回复结束后，自动显示建议的下一轮问题按钮（胶囊样式，点击自动发送）

#### 3.4.3 输入区域设计

| 状态 | 说明 |
|------|------|
| 默认状态 | 附件按钮(📎) + 输入框 + 发送按钮(▶) |
| 输入中 | 输入框显示文字，发送按钮高亮 |
| AI回复中 | 输入框替换为「正在回复中...」，发送按钮替换为停止按钮(⏹) |
| 文件上传 | 输入框上方显示文件预览缩略图，支持移除 |

#### 3.4.4 消息操作

长按消息气泡弹出操作菜单：

| 操作 | 说明 |
|------|------|
| 👍 有帮助 | 正面反馈（调用消息反馈接口） |
| 👎 无帮助 | 负面反馈 |
| 📋 复制文本 | 复制消息文本内容 |
| 🔄 重新生成 | 仅 AI 消息可用，重新生成回复 |

#### 3.4.5 会话管理（右上角菜单）

点击右上角按钮弹出底部菜单：

| 操作 | 说明 |
|------|------|
| 📝 重命名会话 | 修改当前会话标题 |
| 🗑 删除会话 | 删除当前会话及历史消息 |
| 📋 新建对话 | 创建新的对话会话 |
| 📜 历史会话 | 跳转到会话列表页 |

---

### 3.5 图表渲染展示

#### 3.5.1 折线图 & 环形图

> **📷 截图 3-5：手机端 - 图表渲染（折线图 + 环形图）**
>
> 原型来源：`AI小助手平台智能体应用原型交互图.html` → 「渲染展示」Tab 页 → 左侧手机

#### 3.5.2 柱状图 & 雷达图

> **📷 截图 3-6：手机端 - 图表渲染（柱状图 + 雷达图）**
>
> 原型来源：`AI小助手平台智能体应用原型交互图.html` → 「渲染展示」Tab 页 → 右侧手机

**图表类型支持：**

| 图表类型 | ECharts 类型 | 适用场景 |
|---------|-------------|---------|
| 折线图 (line) | `line` | 趋势分析，如产量趋势、销售趋势 |
| 柱状图 (bar) | `bar` | 对比分析，如产线产量对比 |
| 饼图 (pie) | `pie` | 占比分析，如订单状态分布 |
| 环形图 (doughnut) | `pie` (带内半径) | 占比分析（带中心数值） |
| 雷达图 (radar) | `radar` | 多维评估，如供应商评估 |

---

### 3.6 页面与组件清单

#### 新增页面

| 文件路径 | 说明 |
|---------|------|
| `app/pages/ai/index.vue` | AI 应用列表（Tab 页） |
| `app/pages/ai/conversations.vue` | 会话列表页 |
| `app/pages/ai/chat.vue` | 对话页 |

#### 新增组件

| 文件路径 | 说明 |
|---------|------|
| `app/components/ai/app-card.vue` | 应用卡片组件 |
| `app/components/ai/conversation-item.vue` | 会话列表项组件 |
| `app/components/ai/chat-message.vue` | 消息气泡组件（总入口） |
| `app/components/ai/msg-text.vue` | 文本消息渲染 |
| `app/components/ai/msg-markdown.vue` | Markdown 消息渲染 |
| `app/components/ai/msg-table.vue` | 表格消息渲染 |
| `app/components/ai/msg-image.vue` | 图片消息渲染 |
| `app/components/ai/msg-chart.vue` | 图表消息渲染（ECharts） |
| `app/components/ai/msg-file.vue` | 文件卡片组件 |
| `app/components/ai/chat-input.vue` | 输入区域组件 |
| `app/components/ai/suggested-questions.vue` | 建议问题组件 |
| `app/components/ai/streaming-cursor.vue` | 流式输出光标动画 |

#### 新增工具/服务

| 文件路径 | 说明 |
|---------|------|
| `app/utils/ai/sse-client.js` | SSE 流式请求封装 |
| `app/utils/ai/message-parser.js` | 消息内容解析（识别文本/表格/图表/图片） |
| `app/utils/ai/chart-converter.js` | 图表 JSON → ECharts Option 转换器 |

---

## 四、接口设计

### 4.1 PC 端管理接口

#### AI 应用管理

| 接口 | 说明 |
|------|------|
| `POST /api/Sys/AIApp/GetPageData` | 分页查询应用列表 |
| `POST /api/Sys/AIApp/Add` | 新增应用 |
| `POST /api/Sys/AIApp/Update` | 更新应用 |
| `POST /api/Sys/AIApp/Delete` | 删除应用 |
| `POST /api/Sys/AIApp/SyncFromPlatform` | 从 AI小助手平台同步应用 |

#### 角色授权管理

| 接口 | 说明 |
|------|------|
| `GET /api/Sys/RoleAIApp/GetByRole?roleId=` | 获取角色已授权应用 |
| `POST /api/Sys/RoleAIApp/Save` | 保存角色应用授权 |

### 4.2 手机端业务接口

#### 应用与会话

| 接口 | 说明 |
|------|------|
| `GET /api/AI/Apps` | 获取当前用户可用应用列表（按权限过滤） |
| `GET /api/AI/AppInfo?appId=` | 获取应用详情 |
| `GET /api/AI/AppParameters?appId=` | 获取应用参数 |
| `GET /api/AI/AppMeta?appId=` | 获取应用 Meta |
| `GET /api/AI/Conversations?appId=` | 获取会话列表 |
| `GET /api/AI/Messages?conversationId=` | 获取会话消息历史 |
| `DELETE /api/AI/Conversation/:id` | 删除会话 |
| `POST /api/AI/RenameConversation` | 重命名会话 |

#### 对话核心

```
POST /api/AI/SendMessage                -- 发送消息（支持 SSE 流式）
  请求体：
  {
    "appId": "应用ID",
    "query": "用户输入内容",
    "conversationId": "会话ID（新会话为空）",
    "files": [{ "type": "image", "transferMethod": "local_file", "uploadFileId": "xxx" }],
    "inputs": {}
  }
  响应：SSE 流式事件

POST /api/AI/StopMessage                -- 停止响应
  请求体：{ "appId": "应用ID", "taskId": "任务ID" }
```

#### 消息互动

| 接口 | 说明 |
|------|------|
| `POST /api/AI/MessageFeedback` | 消息反馈（点赞/踩），body: `{ appId, messageId, rating: "like/dislike/null" }` |
| `GET /api/AI/SuggestedQuestions?appId=&messageId=` | 获取建议问题 |
| `GET /api/AI/Feedbacks?appId=` | 获取应用消息反馈列表 |

#### 对话变量

| 接口 | 说明 |
|------|------|
| `GET /api/AI/ConversationVariables?appId=&conversationId=` | 获取变量 |
| `POST /api/AI/UpdateVariables` | 更新变量 |

#### 文件

| 接口 | 说明 |
|------|------|
| `POST /api/AI/FileUpload` | 文件上传 |
| `GET /api/AI/FilePreview/:fileId` | 文件预览 |

### 4.3 SSE 流式事件格式

| 事件类型 | 说明 | 前端处理 |
|---------|------|---------|
| `message` | 消息流片段 | 追加文本到气泡 |
| `message_end` | 消息结束 | 结束 loading，渲染完整内容 |
| `message_file` | 文件输出 | 渲染图片/文件卡片 |
| `tts_message` | 语音消息 | 音频播放（预留） |
| `message_replace` | 内容审核替换 | 替换当前消息内容 |
| `error` | 错误 | 显示错误提示 |
| `ping` | 心跳 | 忽略 |

**流式消息处理流程：**

```
收到 SSE 事件
    │
    ├── message → 追加 answer 到当前消息气泡
    │              ↓
    │         检测内容类型：
    │           ├── 纯文本 → 直接渲染
    │           ├── Markdown → 实时解析渲染
    │           ├── 包含表格标记 → 表格组件渲染
    │           ├── 包含图表JSON → 解析后 ECharts 渲染
    │           └── 包含图片URL → 图片组件渲染
    │
    ├── message_end → 最终渲染 + 显示建议问题
    │
    ├── message_file → 插入文件/图片卡片
    │
    ├── message_replace → 替换消息内容
    │
    └── error → Toast 错误提示
```

---

## 五、图表数据转换规范

### 5.1 约定格式

AI小助手平台智能体返回的图表数据统一使用 JSON 代码块包裹，前端通过识别特定标记进行渲染：

````
```chart
{
  "type": "line",
  "title": "近7日产量趋势",
  "xAxis": ["周一","周二","周三","周四","周五","周六","周日"],
  "series": [
    { "name": "A线", "data": [120,132,101,134,90,230,210] },
    { "name": "B线", "data": [220,182,191,234,290,330,310] }
  ]
}
```
````

### 5.2 各图表类型 ECharts Option 转换

| 源 type | ECharts 类型 | 转换要点 |
|---------|-------------|---------|
| `line` | `line` | xAxis + series[].data，支持多系列 |
| `bar` | `bar` | xAxis + series[].data，水平条形图设 yAxis |
| `pie` | `pie` | series[0].data 转为 [{name,value}] 格式 |
| `doughnut` | `pie` (radius: ['40%','70%']) | 同饼图但设内外半径 |
| `radar` | `radar` | indicator 从 xAxis 提取，series.data 对应 |

### 5.3 移动端适配要点

- 图表容器固定高度 `250px`，宽度自适应
- 支持「全屏查看」按钮横屏展示
- Legend 在图表下方水平排列，超出换行
- 关闭动画（移动端性能优先）
- dataZoom 使用 `inside` 类型（触摸滑动）

---

## 六、权限流转设计

> **📷 截图 6-1：权限流转时序图**
>
> 原型来源：`AI小助手平台智能体应用原型交互图.html` → 「权限流转」Tab 页

**权限流转步骤：**

| 步骤 | 操作 | 参与方 |
|------|------|--------|
| 1 | 管理员在 PC 端创建 AI 应用（从 AI小助手平台同步或手动添加，包含 AppKey） | PC 端 → 后端 |
| 2 | 管理员在角色管理中配置 AI 应用授权（穿梭框操作） | PC 端 → 后端 |
| 3 | 用户使用 OCP 账号登录手机端 H5，获取 JWT Token | 手机端 → 后端 |
| 4 | 后端查询用户角色 → 获取角色已授权的 AI 应用 → 仅返回用户可见应用 | 后端 → 手机端 |
| 5 | OCP 与 AI小助手平台统一使用员工号作为用户标识，后端直接以员工号作为 user 参数 | 后端 → AI小助手平台 |
| 6 | 用户发起对话：手机端发送消息 → 后端校验权限 → 查 AppKey → 注入员工号 → 转发到 AI小助手平台 | 手机端 → 后端 → AI小助手平台 |
| 7 | AI小助手平台返回 SSE 流 → 后端透传 → 手机端实时渲染文本/表格/图表 | AI小助手平台 → 后端 → 手机端 |

---

## 七、技术要点与风险

### 7.1 关键技术点

| 技术点 | 方案 | 备注 |
|-------|------|------|
| SSE 流式 | uni-app 内使用 `uni.request` + 分块处理，或 EventSource polyfill | H5 环境原生支持 EventSource |
| Markdown 渲染 | `marked` + `highlight.js` 或 `markdown-it` | 需移动端样式适配 |
| ECharts 移动端 | `echarts` + `uni-canvas` 或 H5 直接用 DOM 版本 | H5 环境可直接用 echarts |
| 图片预览 | `uni.previewImage` | 原生支持 |
| 文件上传 | `uni.chooseFile` / `uni.chooseImage` | 需处理文件大小限制 |

### 7.2 风险与对策

| 风险 | 影响 | 对策 |
|------|------|------|
| SSE 在低版本浏览器兼容性 | 流式输出无法使用 | 降级为轮询模式，或使用 fetch + ReadableStream |
| AI小助手平台 API 变更 | 接口不兼容 | 后端转发层做版本适配，前端无感知 |
| 大量并发对话性能 | 后端 SSE 连接数过多 | 后端使用异步流处理，设置连接超时 |
| 图表数据格式不统一 | 渲染异常 | AI小助手平台智能体 Prompt 中约束输出格式 |
| AppKey 安全 | 密钥泄露 | 仅存后端，加密存储，前端永远不获取 |

---

## 八、里程碑计划

| 阶段 | 时间 | 工作内容 |
|------|------|---------|
| 第1周 | Day 1-2 | 需求沟通 & 方案确认 |
| | Day 3-4 | PC端 AI应用管理页面 + 后端接口 |
| | Day 5 | PC端角色-AI应用授权 |
| 第2周 | Day 1-2 | 后端 AI小助手平台 API 转发层 |
| | Day 3-5 | 手机端应用列表 + 会话列表 |
| 第3周 | Day 1-2 | SSE 流式处理 + 基础对话 |
| | Day 3 | Markdown + 文本渲染 |
| | Day 4 | 表格 + 图片渲染 |
| | Day 5 | AI小助手平台接口联调（会话管理/反馈/建议问题） |
| 第4周 | Day 1-2 | 折线图/柱状图/条形图渲染 |
| | Day 3 | 饼图/环形图/雷达图渲染 |
| | Day 4 | 全流程联调 & BUG 修复 |
| | Day 5 | 优化 & 验收 |

---

## 附录

### 附录A：页面导航流程图

```
手机端 H5 导航流程：

[登录] → [首页TabBar]
              │
              ├── [菜单] ── 现有功能
              ├── [流程] ── 现有功能
              ├── [AI]   ── AI应用列表
              │              │
              │              ├── 点击应用卡片 → [会话列表]
              │              │                    │
              │              │                    ├── 点击会话 → [对话页面]
              │              │                    └── 新建对话 → [对话页面]
              │              │
              │              └── 点击最近对话 → [对话页面]
              │
              ├── [报表] ── 现有功能
              └── [我的] ── 现有功能
```

### 附录B：AI小助手平台 API 参考

| 接口 | Method | Path |
|------|--------|------|
| 发送对话消息 | POST | `/v1/chat-messages` |
| 停止响应 | POST | `/v1/chat-messages/:task_id/stop` |
| 获取会话列表 | GET | `/v1/conversations` |
| 获取会话消息 | GET | `/v1/messages` |
| 删除会话 | DELETE | `/v1/conversations/:conversation_id` |
| 会话重命名 | POST | `/v1/conversations/:conversation_id/name` |
| 消息反馈 | POST | `/v1/messages/:message_id/feedbacks` |
| 建议问题 | GET | `/v1/messages/:message_id/suggested` |
| 获取对话变量 | GET | `/v1/conversations/:conversation_id/variables` |
| 更新对话变量 | PATCH | `/v1/conversations/:conversation_id/variables` |
| 获取应用信息 | GET | `/v1/info` |
| 获取应用参数 | GET | `/v1/parameters` |
| 获取应用Meta | GET | `/v1/meta` |
| 文件上传 | POST | `/v1/files/upload` |
| 文件预览 | GET | `/v1/files/:file_id/preview` |

### 附录C：截图索引

| 编号 | 描述 | 原型来源 |
|------|------|---------|
| 截图 1-1 | 系统整体架构图 | 交互图 → 架构图 Tab |
| 截图 2-1 | PC端 AI应用管理列表页 | 交互图 → AI应用管理 Tab |
| 截图 2-2 | PC端 新增AI应用弹窗 | 交互图 → AI应用管理 Tab → 新增按钮 |
| 截图 2-3 | PC端 从AI小助手平台同步弹窗 | 交互图 → AI应用管理 Tab → 同步按钮 |
| 截图 2-4 | PC端 角色AI应用授权配置页 | 交互图 → 角色授权 Tab |
| 截图 3-1 | 手机端 AI应用列表页 | 交互图 → 应用列表 Tab |
| 截图 3-2 | 手机端 会话列表页 | 交互图 → 会话列表 Tab |
| 截图 3-3 | 手机端 对话界面（流式输出中） | 交互图 → 对话界面 Tab → 左侧 |
| 截图 3-4 | 手机端 对话界面（Markdown+表格+建议问题） | 交互图 → 对话界面 Tab → 右侧 |
| 截图 3-5 | 手机端 图表渲染（折线图+环形图） | 交互图 → 渲染展示 Tab → 左侧 |
| 截图 3-6 | 手机端 图表渲染（柱状图+雷达图） | 交互图 → 渲染展示 Tab → 右侧 |
| 截图 6-1 | 权限流转时序图 | 交互图 → 权限流转 Tab |
