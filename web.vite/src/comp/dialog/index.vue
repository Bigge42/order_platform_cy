<template>
  <div class="comp-dialog">
    <el-dialog
      v-model="vmodel"
      :destroy-on-close="destroyOnClose"
      :close-on-click-modal="false"
      :close-on-press-escape="false"
      :width="width"
      :fullscreen="fullscreen"
      :draggable="draggable"
      :modal="modal"
      :before-close="handleClose"
      class="comp-flex-dialog"
      :style="dialogStyle"
    >
      <template #header>
        <i :class="icon"></i> {{ $ts(title) }}
        <slot name="header"></slot>
        <button
          v-if="showFullscreen && showFull"
          class="el-dialog__headerbtn"
          type="button"
          style="right: 35px; color: var(--el-color-info)"
          @click="handleFullScreen"
        >
          <i class="el-icon el-icon-full-screen"></i>
        </button>
      </template>
      
      <!-- 使用flex布局的内容区域 -->
      <div class="comp-dialog-content-wrapper" :style="{ padding: padding + 'px' }">
        <div v-if="inited" class="comp-dialog-content">
          <slot name="content"></slot>
          <slot></slot>
        </div>
      </div>
      
      <template #footer>
        <div class="dia-footer" v-if="footer">
          <slot name="footer"></slot>
          <el-button type="primary" v-if="!footer" size="mini" @click="handleClose()">
            <i class="el-icon-close"></i>{{ $ts("关闭") }}
          </el-button>
        </div>
      </template>
    </el-dialog>
  </div>
</template>

<script>
import { defineComponent, getCurrentInstance, ref, watch, computed } from "vue";

export default defineComponent({
  name: "VolDialogFlex",
  props: {
    modelValue: false,
    lazy: {
      //是否开启懒加载
      type: Boolean,
      default: false,
    },
    icon: {
      type: String,
      default: "el-icon-warning-outline",
    },
    title: {
      type: String,
      default: "基本信息",
    },
    height: {
      type: Number,
      default: 0,
    },
    width: {
      type: [Number, String],
      default: 650,
    },
    padding: {
      type: Number,
      default: 16,
    },
    modal: {
      //是否需要遮罩层
      type: Boolean,
      default: true,
    },
    draggable: {
      //启用可拖拽功能
      type: Boolean,
      default: false,
    },
    mask: {
      type: Boolean,
      default: true,
    },
    onModelClose: {
      //弹出框关闭事件
      type: Function,
      default: (iconClick) => {
        return true;
      },
    },
    footer: {
      //是否显示底部按钮
      type: Boolean,
      default: true,
    },
    full: {
      type: Boolean,
      default: false,
    },
    showFull: {
      type: Boolean,
      default: true,
    },
    destroyOnClose: {
      //当关闭 Dialog 时，销毁其中的元素
      type: Boolean,
      default: false,
    },
  },
  setup(props, context) {
    const inited = ref(true);
    const vmodel = ref(false);
    const footer = ref(false);
    
    vmodel.value = props.modelValue;
    footer.value = !!context.slots.footer;
    
    const handleClose = (done, iconClose) => {
      const result = props.onModelClose(!!iconClose);
      if (result === false) return;
      
      // 恢复默认状态 - 重置全屏状态为初始值
      fullscreen.value = props.full;
      
      vmodel.value = false;
      context.emit("update:modelValue", false);
      done?.();
    };
    
    watch(
      () => props.modelValue,
      (newVal) => {
        vmodel.value = newVal;
      }
    );
    
    const { proxy } = getCurrentInstance();
    const fullscreen = ref(false);
    const showFullscreen = ref(true);

    if (typeof proxy?.$global?.fullscreen === "boolean") {
      showFullscreen.value = proxy.$global.fullscreen;
    }
    fullscreen.value = props.full;

    const handleFullScreen = () => {
      fullscreen.value = !fullscreen.value;
      context.emit("fullscreen", fullscreen.value);
    };

    // 计算弹窗高度
    const dialogStyle = computed(() => {
      if (fullscreen.value) {
        return {};
      }
      
      if (props.height && props.height > 0) {
        return {
          height: `${props.height}px !important`
        };
      }
      
      return {
        height: '90vh !important'
      };
    });
    
    return {
      handleClose,
      inited,
      vmodel,
      footer,
      fullscreen,
      showFullscreen,
      handleFullScreen,
      dialogStyle,
    };
  },
});
</script>

<style lang="less" scoped>
.dia-footer {
  text-align: right;
  width: 100%;
  border-top: 1px solid #f1f1f1;
  padding: 6px 8px;
  flex-shrink: 0; /* 防止footer被压缩 */
}

.comp-dialog-content-wrapper {
  flex: 1;
  flex-direction: column;
  min-height: 0;
  overflow: hidden;
  height: 100%;
}

.comp-dialog-content {
  height: 100%;
}
</style>

<style lang="less">
.comp-dialog {
  .el-overlay-dialog {
    display: flex !important;
  }

  .el-dialog {
    margin: auto;
    border-top-left-radius: 4px;
    border-top-right-radius: 4px;
    display: flex;
    flex-direction: column;
    
    // 全屏时填满整个视口
    &.is-fullscreen {
      width: 100vw !important;
      height: 100vh !important;
      margin: 0 !important;
      border-radius: 0;
    }
  }

  .el-dialog__header {
    border-top-left-radius: 4px;
    border-top-right-radius: 4px;
    padding: 0px 13px;
    line-height: 53px;
    border-bottom: 1px solid #f1f1f1;
    height: 50px;
    color: rgb(79, 79, 79);
    font-weight: bold;
    font-size: 14px;
    margin: 0;
    flex-shrink: 0; /* 防止header被压缩 */
  }

  .el-dialog__body {
    padding: 0;
    height: calc(100% - 110px);
    overflow: hidden;
  }

  .el-dialog__footer {
    padding: 0;
    flex-shrink: 0; /* 防止footer被压缩 */
  }

  .el-dialog__headerbtn {
    top: 0;
    padding-top: 8px;
    height: 50px;
    width: 0;
    padding-right: 30px;
    padding-left: 5px;
  }
}
</style>
