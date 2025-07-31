<template>
    <div>
        <el-popover ref="lcMultipleInputRef" placement="bottom-start" :title="modelValue?.label" :width="460"
            trigger="click" :close-delay=10 @before-leave="togglePopover" @after-enter="handleAfterLeave"
            :disabled="disabled" v-model="visible">
            <div class="popSelect">
                <el-row>
                    <label class="el-form-item__label">比较符：</label>
                    <el-select v-model="displayType" placeholder="请选择" class="textareaOperator" :teleported="false"
                        @change="operatorChange">
                        <el-option v-for="(operator, index) in operatorList" :label="operator.label" :key="index"
                            :value="operator.displayType"></el-option>
                    </el-select>
                    <el-button style="margin-left:8px;" type="info" plain @click="clear">清空</el-button>
                    <el-button style="margin-left:8px;" type="primary" @click="Confirm">确定</el-button>
                </el-row>
                <el-row style="margin-top:15px;">
                    <label class="el-form-item__label">比较值：</label>
                    <el-input :clearable="true" v-model="fieldRawValue" :placeholder="placeholder" type="textarea"
                        :disabled="InputDisabled" ref="textareaInputRef" class="textareaInput" :rows=6
                        :show-word-limit="true" :maxlength="maxLength" @blur="InputBlur" />
                </el-row>
            </div>
            <template #reference>
                <div class="lc-multiple-input" :id="lcmultipleinputId" :class="size" @click="togglePopover">
                    <div v-if="fieldValueList.length > 0" class="lc-multiple-input-tag">
                        <span style="display:inline-block;">
                            <el-tag type="danger" :size="size">{{ displayTypeLable }}</el-tag>
                        </span>
                        <span class="lc-multiple-input-content-tag">
                            <el-tag type="info" :size="size">{{ fieldValueList[0] }}</el-tag>
                        </span>
                        <el-tag type="success" :size="size" class="ele-tag-round">
                            共{{ fieldValueList.length > 99 ? '99+' : fieldValueList.length }}项
                        </el-tag>
                    </div>
                    <div v-else class="placeholder">
                        {{ placeholder }}
                    </div>
                </div>

            </template>
        </el-popover>
    </div>
</template>
<script setup>
//import { on, off } from 'element-plus/src/utils/dom';
//import { generateId } from 'element-plus/src/utils/util';
import { ref, onMounted, reactive, computed, nextTick, watch } from 'vue';

const textareaInputRef = ref();
const emit = defineEmits(['update:modelValue']);
const lcMultipleInputRef = ref();

const props = defineProps({
    placeholder: { type: String, default: '请输入' },
    value: {
        type: Object,
        default: () => {
            return {
                value: null,
                displayType: 'selectlist',
                label: null,
                operatorList: [
                    { displayType: 'equal', label: '等于' },
                    { displayType: 'selectlist', label: '批量' },
                    { displayType: 'likeStart', label: '左包含' },
                    { displayType: 'likeEnd', label: '右包含' },
                    { displayType: 'like', label: '模糊' },
                    { displayType: 'EMPTY', label: '空' },
                    { displayType: 'NOT_EMPTY', label: '不空' },
                ]
            }
        },
        required: true
    },
    modelValue: {
        type: Object,
        default: () => {
            return {
                value: null,
                displayType: 'selectlist',
                label: null,
                operatorList: [
                    { displayType: 'equal', label: '等于' },
                    { displayType: 'selectlist', label: '批量' },
                    { displayType: 'likeStart', label: '左包含' },
                    { displayType: 'likeEnd', label: '右包含' },
                    { displayType: 'like', label: '模糊' },
                    { displayType: 'EMPTY', label: '空' },
                    { displayType: 'NOT_EMPTY', label: '不空' },
                ]
            }
        },
        required: true
    },
    disabled: { type: Boolean, defalut: false },
    //返回值格式，可以是带英文逗号的字符串或数组；
    valueType: {
        type: [String, Array],
        default: 'String', //String Array
        required: false,
    },
    size: { type: String, default: 'medium' },
    maxLength: { type: Number, default: 1000 }

})


const fieldRawValue = ref(null);
const fieldValueList = ref([]);
const fieldValue = ref(null);
const visible = ref(false);
const loading = ref(false);
const displayType = ref(props.modelValue?.displayType);
const operatorList = ref([]);
operatorList.value = props.modelValue?.operatorList ?? [
    { displayType: 'equal', label: '等于' },
    { displayType: 'selectlist', label: '批量' },
    { displayType: 'likeStart', label: '左包含' },
    { displayType: 'likeEnd', label: '右包含' },
    { displayType: 'like', label: '模糊' },
    { displayType: 'EMPTY', label: '空' },
    { displayType: 'NOT_EMPTY', label: '不空' },
];

const InputDisabled = ref(false);


onMounted(() => {
    //on(document, 'click', this.handleDocumentClick);
    //console.log(props.modelValue)
    // console.log('=====');
    fieldRawValue.value = props.modelValue?.value;
    valueInit();
    //handleDocumentClick()
})

const valueInit = () => {

    if (![null, '', undefined].includes(fieldRawValue.value)) {
        let formatFieldRawValue = fieldRawValue.value.replace(/\r\n/g, ",").trim();
        formatFieldRawValue = formatFieldRawValue.replace(/\n/g, ",").trim();
        fieldValueList.value = formatFieldRawValue.split(',');
    } else {
        fieldValueList.value = '';
    }


}
const InputBlur = () => {
    valueInit();
    updateValue();
    togglePopover();
}
const updateValue = () => {
    if (props.valueType == 'String') {
        fieldValue.value = fieldValueList.value.toString();
    } else {
        fieldValue.value = fieldValueList.value;
    }

    //合并作用主要是为合并最初传来的其他附加属性；
    let modelValue = Object.assign(props.value, {
        displayType: displayType.value,
        value: fieldValue.value
    });
    //console.log(modelValue);
    emit('update:modelValue', modelValue)
}
const togglePopover = () => {
    visible.value = !visible.value;
    if (visible.value) {
        nextTick(() => {
            textareaInputRef?.value?.focus();
        })
    }
}

const handleDocumentClick = (e) => {
    //弹出层之外的点击则关闭
    //if (!this.$el || this.$el.contains(e.target)) return;
    updateValue();
    visible.value = false;
}
const clear = () => {
    fieldRawValue.value = '';
    fieldValueList.value = [];
    updateValue();
}

const displayTypeLable = computed(() => {
    //return props.modelValue.operatorList.find(d => d.displayType == displayType.value).label;
    return operatorList.value.find(d => d.displayType == displayType.value).label;

})

// const fieldRawValue=computed(()=>{
//     return props.modelValue.value;
// })

const lcmultipleinputId = computed(() => {
    // return `lc-multiple-input-${generateId()}`;
    return 'lc-multiple-input-id';
})


watch(
    () => props.modelValue,
    (modelValue) => {
        fieldRawValue.value = modelValue?.value;
        //console.log('watch modelValue')
        valueInit();
    }
);

const handleAfterLeave = () => {
    // console.log('handleAfterLeave');
    nextTick(() => {
        //textareaInputRef.value.focus();
    })
}


const Confirm = () => {
    // nextTick(() => {
    //     textareaInputRef.value.blur();
    //     visible.value = false;
    // })
    //InputBlur();
    //visible.value = false;
    lcMultipleInputRef.value.hide();
}

const operatorChange = (value) => {
    console.log(value);
    if (['EMPTY', 'NOT_EMPTY'].includes(value)) {
        InputDisabled.value = true;
        fieldRawValue.value = value;
        fieldValueList.value = [value];
        updateValue();
    } else {
        InputDisabled.value = false;
    }
}

// watch(
//     (props.modelValue),
//     (newValue, oldvalue) => {
//         console.log(newValue);
//         fieldRawValue.value = newValue.value;
//         valueInit();
//     }
// );
watch(
    visible,
    (newValue, oldvalue) => {
        if (newValue == true) {
            textareaInputRef?.value?.focus();
        }
    }
);


</script>

<script>
export default {
    name: 'HdMultipleInput'
};
</script>

<style lang="less" scoped>
.textareaOperator {
    display: inline-block;
    background-color: var(--background-color-base) !important;
    width: 40% !important;
}

.textareaOperator ::v-deep(.el-input) {
    background-color: var(--background-color-base);
}

.textareaOperator ::v-deep(input) {
    background-color: var(--background-color-base) !important;
}

.textareaOperator ::v-deep(.el-input__suffix) {
    right: 0px !important;
}

.textareaInput {
    width: 80%;
}

.textareaInput ::v-deep(textarea) {
    // border-top-left-radius: 0 !important;
    // border-bottom-left-radius: 0 !important;
}

.lc-multiple-input {
    width: 100%;
    display: inline-flex;
    position: relative;
    background-color: #fff;
    height: 30px;
    border: var(--el-border);
    border-radius: var(--el-border-radius-base);
}

.lc-multiple-input.medium {
    height: 36px;
    line-height: 36px;
}

.lc-multiple-input.small {
    height: 32px;
    line-height: 32px;
}

.lc-multiple-input.mini {
    height: 28px;
    line-height: 28px;
}

.lc-multiple-input:hover {
    border: 1px solid var(--el-border-color-hover);
    border-radius: var(--el-border-radius-base);
    background: var(--el-input-hover-bg);
    box-shadow: var(--el-input-hover-shadow);

}

.lc-multiple-input:focus {
    border: 1px solid #409eff;
}

.lc-multiple-input-tag {
    margin-left: 5px;
    width: 100%;
    line-height: 27px !important;
    display: flex;
    align-items: center;
    justify-content: space-between;
}

.lc-multiple-input-tag>span:first-child {
    flex-shrink: 0;
    /* 防止收缩 */
    white-space: nowrap;
}

.lc-multiple-input-tag>span:last-child {
    flex-shrink: 0;
    /* 防止收缩 */
    white-space: nowrap;
}

.lc-multiple-input-content-tag {
    display: flex;
    flex: 1;
    overflow: hidden;
    margin: 0 5px;
    text-overflow: ellipsis;
    white-space: nowrap;
    align-items: center;
    min-width: 0;
    /* 确保flex子项可以收缩 */
}

.placeholder {
    padding: 0px 15px;
    color: var(--el-text-color-placeholder);
    vertical-align: middle;
}
</style>