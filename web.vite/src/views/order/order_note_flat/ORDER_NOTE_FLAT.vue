<!--
 *Author：jxx
 *Date：{Date}
 *Contact：461857658@qq.com
 *业务请在@/extension/order/order_note_flat/ORDER_NOTE_FLAT.jsx或ORDER_NOTE_FLAT.vue文件编写
 *新版本支持vue或【表.jsx]文件编写业务
-->
<template>
  <view-grid
    ref="grid"
    :columns="columns"
    :detail="detail"
    :details="details"
    :editFormFields="editFormFields"
    :editFormOptions="editFormOptions"
    :searchFormFields="searchFormFields"
    :searchFormOptions="searchFormOptions"
    :table="table"
    :extend="extend"
    :onInit="onInit"
    :onInited="onInited"
    :searchBefore="searchBefore"
    :searchAfter="searchAfter"
    :addBefore="addBefore"
    :updateBefore="updateBefore"
    :rowClick="rowClick"
    :modelOpenBefore="modelOpenBefore"
    :modelOpenAfter="modelOpenAfter"
  >
    <template #gridHeader></template>
  </view-grid>

  <!-- 备注拆分弹窗 -->
  <el-dialog
    v-model="splitDialogVisible"
    title="备注拆分"
    width="920px"
    top="8vh"
    class="note-split-dialog-wrapper"
    :close-on-click-modal="false"
    :before-close="handleSplitDialogBeforeClose"
    @closed="handleSplitDialogClosed"
  >
    <div
      class="note-split-dialog"
      v-loading="splitDialogFetching"
      element-loading-text="加载备注中..."
    >
      <!-- 当前用户显示区 -->
      <div class="note-split-dialog-header">
        <span class="note-split-dialog-header__label">当前用户：</span>
        <span class="note-split-dialog-header__value">{{ userDisplayName }}</span>
        <span style="margin: 0 8px; color:#c0c4cc;">|</span>
        <span class="note-split-dialog-header__label">账户用户名：</span>
        <span class="note-split-dialog-header__value">{{ userAccount }}</span>
      </div>

      <!-- 原始备注（只读） -->
      <el-form label-width="140px" class="note-split-section" :model="splitDialogRemark">
        <el-form-item label="原始备注">
          <el-input
            type="textarea"
            :autosize="{ minRows: 2, maxRows: 6 }"
            v-model="splitDialogRemark.remark_raw"
            readonly
          />
        </el-form-item>
        <el-form-item label="内部信息传递">
          <el-input
            type="textarea"
            :autosize="{ minRows: 2, maxRows: 6 }"
            v-model="splitDialogRemark.internal_note"
            readonly
          />
        </el-form-item>
      </el-form>

      <el-divider />

      <!-- 可编辑备注区 -->
      <el-form label-width="180px" class="note-split-section" :model="splitDialogForm">
        <el-form-item label="阀体及执行机构装配备注">
          <el-input
            type="textarea"
            :autosize="{ minRows: 2, maxRows: 6 }"
            v-model="splitDialogForm.note_body_actuator"
            placeholder="请输入阀体及执行机构装配备注"
          />
        </el-form-item>
        <el-form-item label="附件安装及调试备注">
          <el-input
            type="textarea"
            :autosize="{ minRows: 2, maxRows: 6 }"
            v-model="splitDialogForm.note_accessory_debug"
            placeholder="请输入附件安装及调试备注"
          />
        </el-form-item>
        <el-form-item label="强压泄漏试验备注">
          <el-input
            type="textarea"
            :autosize="{ minRows: 2, maxRows: 6 }"
            v-model="splitDialogForm.note_pressure_leak"
            placeholder="请输入强压泄漏试验备注"
          />
        </el-form-item>
        <el-form-item label="装箱备注">
          <el-input
            type="textarea"
            :autosize="{ minRows: 2, maxRows: 6 }"
            v-model="splitDialogForm.note_packing"
            placeholder="请输入装箱备注"
          />
        </el-form-item>
      </el-form>
    </div>

    <template #footer>
      <span class="dialog-footer">
        <el-button @click="handleSplitDialogCancel" :disabled="splitDialogSubmitting">取消</el-button>
        <el-button type="primary" @click="handleSplitDialogSubmit" :loading="splitDialogSubmitting">
          确认
        </el-button>
      </span>
    </template>
  </el-dialog>
</template>

<script setup lang="jsx">
import extend from "@/extension/order/order_note_flat/ORDER_NOTE_FLAT.jsx";
import viewOptions from "./ORDER_NOTE_FLAT/options.js";
import { ref, reactive, getCurrentInstance, computed } from "vue";
import { useStore } from "vuex";

const grid = ref(null);
const { proxy } = getCurrentInstance();
const store = useStore();

// http请求，proxy.http.post/get
const {
  table,
  editFormFields,
  editFormOptions,
  searchFormFields,
  searchFormOptions,
  columns,
  detail,
  details,
} = reactive(viewOptions());

// ============== 用户信息（姓名 / 账号 / 展示名） =================
const userInfoRaw = computed(() => {
  const getter = store.getters.getUserInfo;
  return (typeof getter === "function" ? getter() : getter) || {};
});
const userAccount = computed(() => userInfoRaw.value.loginName || userInfoRaw.value.userName || "");
const userRealName = computed(
  () => userInfoRaw.value.userTrueName || userInfoRaw.value.realName || ""
);
const userDisplayName = computed(() => {
  const n = userRealName.value;
  const a = userAccount.value;
  if (n && a && n !== a) return `${n}（${a}）`;
  return n || a || "未知用户";
});

// ============== 弹窗状态 =================
let gridRef; // 对应[表.jsx]文件中this.使用方式一样
const splitDialogVisible = ref(false);
const splitDialogFetching = ref(false);
const splitDialogSubmitting = ref(false);
const splitDialogRemark = reactive({
  remark_raw: "",
  internal_note: "",
});
const splitDialogForm = reactive({
  note_body_actuator: "",
  note_accessory_debug: "",
  note_pressure_leak: "",
  note_packing: "",
});
const splitDialogSourceId = ref(null);
const activeRowRef = ref(null);

const resetSplitDialog = () => {
  splitDialogRemark.remark_raw = "";
  splitDialogRemark.internal_note = "";
  splitDialogForm.note_body_actuator = "";
  splitDialogForm.note_accessory_debug = "";
  splitDialogForm.note_pressure_leak = "";
  splitDialogForm.note_packing = "";
  splitDialogSourceId.value = null;
  activeRowRef.value = null;
};

const handleSplitDialogBeforeClose = (done) => {
  if (splitDialogSubmitting.value) return;
  done();
};
const handleSplitDialogClosed = () => resetSplitDialog();
const handleSplitDialogCancel = () => {
  if (!splitDialogSubmitting.value) splitDialogVisible.value = false;
};

// ============== 保存：按后端新文档携带 modified_by =================
const handleSplitDialogSubmit = async () => {
  if (!splitDialogSourceId.value) {
    proxy.$message.error("缺少 source_entry_id，无法保存");
    return;
  }
  // modified_by 按接口文档为“张三”这类真实姓名，若无姓名则回退到账号
  const modifiedBy = userRealName.value || userAccount.value || "系统用户";

  splitDialogSubmitting.value = true;
  try {
    const payload = {
      source_entry_id: splitDialogSourceId.value,
      note_body_actuator: splitDialogForm.note_body_actuator,
      note_accessory_debug: splitDialogForm.note_accessory_debug,
      note_pressure_leak: splitDialogForm.note_pressure_leak,
      note_packing: splitDialogForm.note_packing,
      modified_by: modifiedBy, // ★ 新增必填
    };

    const res = await proxy.http.post("api/ORDER_NOTE_FLAT/updateNoteDetails", payload);
    if (res && (res.status || res.success)) {
      proxy.$message.success(res.message || res.msg || "保存成功");
      // 同步更新表格当前行
      if (activeRowRef.value) {
        activeRowRef.value.note_body_actuator = splitDialogForm.note_body_actuator;
        activeRowRef.value.note_accessory_debug = splitDialogForm.note_accessory_debug;
        activeRowRef.value.note_pressure_leak = splitDialogForm.note_pressure_leak;
        activeRowRef.value.note_packing = splitDialogForm.note_packing;
      }
      splitDialogVisible.value = false;
      gridRef && gridRef.refresh && gridRef.refresh();
    } else {
      proxy.$message.error(res?.message || res?.msg || "保存失败");
    }
  } catch (error) {
    console.error("updateNoteDetails error", error);
    proxy.$message.error("保存失败，请稍后重试");
  } finally {
    splitDialogSubmitting.value = false;
  }
};

// ============== 打开弹窗并拉取只读备注 =================
const openSplitDialog = async (row) => {
  if (!row || !row.source_entry_id) {
    proxy.$message.error("未找到 source_entry_id");
    return;
  }
  splitDialogVisible.value = true;
  splitDialogFetching.value = true;
  splitDialogSourceId.value = row.source_entry_id;
  activeRowRef.value = row;

  // 先用表格中的快照回填，再尝试请求详情覆盖
  splitDialogRemark.remark_raw = row.remark_raw || "";
  splitDialogRemark.internal_note = row.internal_note || "";
  splitDialogForm.note_body_actuator = row.note_body_actuator || "";
  splitDialogForm.note_accessory_debug = row.note_accessory_debug || "";
  splitDialogForm.note_pressure_leak = row.note_pressure_leak || "";
  splitDialogForm.note_packing = row.note_packing || "";

  try {
    const res = await proxy.http.get(`api/ORDER_NOTE_FLAT/${row.source_entry_id}/note`, {}, false);
    if (res && res.status && res.data) {
      splitDialogRemark.remark_raw = res.data.remark_raw || "";
      splitDialogRemark.internal_note = res.data.internal_note || "";
    } else if (res && !res.status) {
      proxy.$message.error(res.msg || "获取备注失败");
    }
  } catch (error) {
    console.error("fetch note error", error);
    proxy.$message.error("获取备注失败，请稍后重试");
  } finally {
    splitDialogFetching.value = false;
  }
};

// ============== 列定义增强：按钮列 / 字段顺序 / 样式 =================
const ensureFieldOrder = () => {
  const internalIndex = columns.findIndex((col) => col.field === "internal_note");
  const selectionIndex = columns.findIndex((col) => col.field === "selection_guid");
  if (internalIndex > -1 && selectionIndex > -1 && internalIndex !== selectionIndex) {
    [columns[internalIndex], columns[selectionIndex]] = [
      columns[selectionIndex],
      columns[internalIndex],
    ];
  }
};

const ensureSplitActionColumn = () => {
  const actionField = "__note_split_action__";
  const existed = columns.find((c) => c.field === actionField);
  if (existed) return;

  columns.push({
    field: actionField,
    title: "操作",
    width: 120,
    align: "center",
    fixed: "right",
    render: (h, { row }) => (
      <el-button type="primary" link size="small" onClick={() => openSplitDialog(row)}>
        备注拆分
      </el-button>
    ),
  });
};

const normalizeBoolean = (value) => {
  if (value === true || value === "true") return true;
  if (value === false || value === "false") return false;
  if (typeof value === "number") return value !== 0;
  if (typeof value === "string") {
    const lowered = value.trim().toLowerCase();
    if (["1", "是", "y", "yes", "true", "t"].includes(lowered)) return true;
    if (["0", "否", "n", "no", "false", "f"].includes(lowered)) return false;
  }
  return Boolean(value);
};

const ensureBzChangedStyling = () => {
  const column = columns.find((col) => col.field === "bz_changed");
  if (!column || column.__noteSplitStyled) return;

  const originalRender = column.render;
  column.render = (h, ctx) => {
    const { row } = ctx;
    const changed = normalizeBoolean(row?.bz_changed);
    const classNames = ["bz-changed-flag", changed ? "is-true" : "is-false"];
    const content = originalRender
      ? originalRender(h, ctx)
      : row?.bz_changed === undefined || row?.bz_changed === null
        ? ""
        : changed
          ? "是"
          : "否";
    return <span class={classNames.join(" ")}>{content}</span>;
  };
  column.align = column.align || "center";
  column.__noteSplitStyled = true;
};

// 初始增强
ensureFieldOrder();
ensureSplitActionColumn();
ensureBzChangedStyling();

// ============== view-grid 生命周期回调 =================
const onInit = async ($vm) => {
  gridRef = $vm;
};
const onInited = async () => {
  // 这里不用再重复插入“备注拆分”列，已在 ensureSplitActionColumn 完成
};
const searchBefore = async () => true;
const searchAfter = async () => true;
const addBefore = async () => true;
const updateBefore = async () => true;
const rowClick = () => {};
const modelOpenBefore = async () => true;
const modelOpenAfter = () => {};

// 对外暴露
defineExpose({});
</script>

<style scoped lang="scss">
.note-split-dialog-wrapper :deep(.el-dialog) {
  border-radius: 18px;
  overflow: hidden;
}
.note-split-dialog-wrapper :deep(.el-dialog__body) {
  padding: 40px 56px 0;
  background-color: #f5f7fa;
}
.note-split-dialog-wrapper :deep(.el-dialog__footer) {
  padding: 0 56px 40px;
  background-color: #f5f7fa;
}

.note-split-dialog {
  display: flex;
  flex-direction: column;
  gap: 24px;
  padding: 36px 40px;
  background-color: #ffffff;
  border-radius: 16px;
  box-shadow: 0 10px 28px rgba(31, 45, 61, 0.08);
  border: 1px solid #e4e7ed;
}

.note-split-section {
  width: 100%;
}
.note-split-section :deep(.el-form-item) {
  margin-bottom: 20px;
}

.note-split-dialog-header {
  display: flex;
  justify-content: flex-end;
  align-items: center;
  gap: 6px;
  font-size: 14px;
  color: #606266;
  margin-bottom: 12px;
}
.note-split-dialog-header__value {
  font-weight: 600;
  color: #303133;
}

.dialog-footer {
  display: flex;
  justify-content: flex-end;
  gap: 12px;
}

/* bz_changed 渲染样式 */
:deep(.bz-changed-flag) {
  display: inline-flex;
  align-items: center;
  padding: 0 8px;
  line-height: 24px;
  border-radius: 12px;
  font-weight: 500;
}
:deep(.bz-changed-flag.is-false) {
  background-color: #dff6e2;
  color: #2f8f3a;
}
:deep(.bz-changed-flag.is-true) {
  background-color: #fde4e4;
  color: #d14343;
}
:deep(.el-table__row:has(.bz-changed-flag.is-false) > td) {
  background-color: #f3fbf4;
}
:deep(.el-table__row:has(.bz-changed-flag.is-true) > td) {
  background-color: #fef3f3;
}
</style>
