<template>
  <div v-if="cardData" class="go-items-list-card">
    <n-card hoverable size="small">
      <div class="list-content">
        <!-- 顶部按钮 -->
        <!-- {{JSON.stringify(cardData)}} -->
        <div class="list-content-top">
          <mac-os-control-btn class="top-btn" :hidden="['remove']" @close="deleteHandle"
            @resize="resizeHandle"></mac-os-control-btn>

          <div class="list-content-top-input">
            <span style="font-size: 12px;margin: 9px;color: #b0a7a7;">显示排序 </span>
            <n-popover trigger="hover">
              <template #trigger>
                <n-input-number style="width: 100px;" :onBlur="onBlur" placeholder="排序号" :show-button="false"
                  v-model:value="cardData.orderNo" clearable />
              </template>
              <span>大屏页面按值从大到小显示</span>
            </n-popover>
          </div>
        </div>
        <!-- 中间 -->
        <div class="list-content-img" @click="resizeHandle">
          <n-image object-fit="contain" height="180" preview-disabled
            :src="`${cardData.image}?time=${new Date().getTime()}`" :alt="cardData.title"
            :fallback-src="requireErrorImg()"></n-image>
        </div>
      </div>
      <template #action>
        <div> <n-text>
            <!-- <n-popover trigger="hover">
              <template #trigger>
                <n-input style="width: 100%;margin-bottom: 10px;" :on-blur="onBlur" placeholder="大屏名称" :show-button="false"
                  v-model:value="cardData.title"  />
              </template>
              <span>大屏页面按值从大到小显示</span>
            </n-popover> -->
            <n-input style="width: 100%;margin-bottom: 10px;" :on-blur="onBlur" placeholder="大屏名称" :show-button="false"
              v-model:value="cardData.title" />
            <!-- {{ cardData.title || cardData.id || '未命名' }} -->
            <div style="font-size: 13px;color: #a5a3a3; margin: -8px 0 5px 0;"> 大屏ID: {{ cardData.id }}</div>
          </n-text>
        </div>
        <div class="go-flex-items-center list-footer" justify="space-between">

          <!-- 工具 -->
          <div class="go-flex-items-center list-footer-ri">


            <n-space>
              <n-text>
                <n-badge class="go-animation-twinkle" dot :color="cardData.release ? '#34c749' : '#fcbc40'"></n-badge>
                {{
                  cardData.release
                  ? $t('project.release')
                  : $t('project.unreleased')
                }}
              </n-text>
              <n-button size="small" @click="copyClick">
                复制
              </n-button>
              <template v-for="item in fnBtnList" :key="item.key">
                <template v-if="item.key === 'select'">
                  <n-dropdown trigger="hover" placement="bottom" :options="selectOptions" :show-arrow="true"
                    @select="handleSelect">
                    <n-button size="small">
                      <template #icon>
                        <component :is="item.icon"></component>
                      </template>
                    </n-button>
                  </n-dropdown>
                </template>

                <n-tooltip v-else placement="bottom" trigger="hover">
                  <template #trigger>
                    <n-button size="small" @click="handleSelect(item.key)">
                      <template #icon>
                        <component :is="item.icon"></component>
                      </template>
                    </n-button>
                  </template>
                  <component :is="item.label"></component>
                </n-tooltip>
              </template>
            </n-space>
            <!-- end -->
          </div>
        </div>
      </template>
    </n-card>
  </div>
</template>

<script setup lang="ts">
import { reactive, ref, PropType } from 'vue'
import { renderIcon, renderLang, requireErrorImg } from '@/utils'
import { icon } from '@/plugins'
import { MacOsControlBtn } from '@/components/Tips/MacOsControlBtn'
import { Chartype } from '../../index.d'
import { http } from '@/api/http'
const {
  EllipsisHorizontalCircleSharpIcon,
  CopyIcon,
  TrashIcon,
  PencilIcon,
  DownloadIcon,
  BrowsersOutlineIcon,
  HammerIcon,
  SendIcon
} = icon.ionicons5

const emit = defineEmits(['preview', 'delete', 'resize', 'edit', 'release', 'copy'])

const props = defineProps({
  cardData: Object as PropType<Chartype>
})

const fnBtnList = reactive([
  {
    label: renderLang('global.r_edit'),
    key: 'edit',
    icon: renderIcon(HammerIcon)
  },
  {
    lable: renderLang('global.r_more'),
    key: 'select',
    icon: renderIcon(EllipsisHorizontalCircleSharpIcon)
  }
])

const selectOptions = ref([
  {
    label: renderLang('global.r_preview'),
    key: 'preview',
    icon: renderIcon(BrowsersOutlineIcon)
  },
  {
    label: props.cardData?.release
      ? renderLang('global.r_unpublish')
      : renderLang('global.r_publish'),
    key: 'release',
    icon: renderIcon(SendIcon)
  },
  {
    label: renderLang('global.r_delete'),
    key: 'delete',
    icon: renderIcon(TrashIcon)
  }
])

const handleSelect = (key: string) => {
  switch (key) {
    case 'preview':
      previewHandle()
      break
    case 'delete':
      deleteHandle()
      break
    case 'release':
      releaseHandle()
      break
    case 'edit':
      editHandle()
      break
  }
}
import { updateOrderNoProjectApi, copyProjectApi } from '@/api/path'
const onBlur = () => {
  if (props.cardData) {
    //@ts-ignore
    props.cardData.projectName = props.cardData.title;
  }
  updateOrderNoProjectApi(props.cardData).then(x => {

  })
}

const copyClick = () => {
  copyProjectApi(props.cardData).then(x => {
    emit('copy', props.cardData)
  })
}
// 预览处理
const previewHandle = () => {
  emit('preview', props.cardData)
}

// 删除处理
const deleteHandle = () => {
  emit('delete', props.cardData)
}

// 编辑处理
const editHandle = () => {
  emit('edit', props.cardData)
}

// 编辑处理
const releaseHandle = () => {
  emit('release', props.cardData)
}

// 放大处理
const resizeHandle = () => {
  emit('resize', props.cardData)
}



</script>

<style lang="scss" scoped>
$contentHeight: 180px;

@include go('items-list-card') {
  position: relative;
  border-radius: $--border-radius-base;
  border: 1px solid rgba(0, 0, 0, 0);
  @extend .go-transition;

  &:hover {
    @include hover-border-color('hover-border-color');
  }

  .list-content {
    margin-top: 20px;
    margin-bottom: 5px;
    cursor: pointer;
    border-radius: $--border-radius-base;
    @include background-image('background-point');
    @extend .go-point-bg;

    &-top {
      position: absolute;
      top: 10px;
      left: 10px;
      height: 22px;
    }

    &-img {
      height: $contentHeight;
      @extend .go-flex-center;
      @extend .go-border-radius;

      @include deep() {
        img {
          @extend .go-border-radius;
        }
      }
    }
  }

  .list-footer {
    flex-wrap: nowrap;
    justify-content: right;
    line-height: 30px;

    &-ri {
      justify-content: flex-end;
      min-width: 180px;
    }
  }
}

.list-content-top {
  display: flex;
  width: 100%;
  align-items: center;
}

.list-content-top-input {
  flex: 1;
  display: flex;
  justify-content: right;
  padding-right: 26px;
}
</style>
