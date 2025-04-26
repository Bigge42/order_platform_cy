<template>
  <div>
    <div v-if="showFilter">
      <vol-custom-filter
        show-border
        @delFilter="delViewFilter"
        @clickFilter="clickViewFilter"
        ref="customFilterViewRef"
        @filterInit="filterInit"
        :cache-key="cacheKey"
      >
        <el-button
          class="setting-btn"
          type="default"
          style="padding: 0 !important"
          size="small"
          link
          @click="model = true"
        >
          <img style="height: 15px" src="../ViewGrid/filter.svg" alt="filter" />
        </el-button>
      </vol-custom-filter>
    </div>
    <vol-box
      :lazy="true"
      v-model="model"
      title="自定义查询"
      :width="width"
      :height="height"
      :padding="0"
    >
      <div class="custom-search">
        <div class="custom-content">
          <div class="custom-fields">
            <div class="custom-title">
              <i class="el-icon-edit-outline"></i> {{ $ts("字段") }}
            </div>
            <el-scrollbar style="height: 100%">
              <vol-custom-fields
                :cacheKey="cacheKey"
                :checkEnd="checkEnd"
                :fieldOptions="fieldOptions"
                @end="end"
              ></vol-custom-fields>
            </el-scrollbar>
          </div>
          <!-- 中间表单 -->
          <div class="custom-form">
            <div class="custom-title custom-form-title">
              <div><i class="el-icon-receiving"></i> {{ $ts("查询表单") }}</div>
              <!-- <el-button type="primary" size="small" link @click="model = false"
                >保存配置</el-button
              > -->
              <div class="custom-form-title-input">
                <label>{{ $ts("模板名称") }}：</label>
                <el-input
                  v-model="templateName"
                  style="width: 160px"
                  :placeholder="$ts('模板名称')"
                ></el-input>

                <el-button @click="addClick" plain type="primary"
                  ><i class="el-icon-plus"></i> 新建</el-button
                >
                <el-button @click="delClick" plain type="warning"
                  ><i class="el-icon-delete"></i> 删除</el-button
                >
                <el-button plain type="default" @click="saveClick"
                  ><i class="el-icon-check"></i> 保存</el-button
                >
              </div>
            </div>
            <div class="custom-form-content">
              <el-scrollbar style="height: 100%">
                <el-form label-position="top">
                  <draggable
                    class="draggable-container"
                    v-model="formOptions"
                    @end="formEnd"
                    animation="300"
                    :move="onMove"
                    style="min-height: 300px"
                    group="componentsGroup"
                  >
                    <transition-group>
                      <div
                        class="vol-custom-form-item"
                        :class="{ actived: index === currentIndex }"
                        @click="itemClick(item, index)"
                        :style="{ width: (item.width || 25) + '%' }"
                        v-for="(item, index) in formOptions"
                        :key="index"
                      >
                        <i class="el-icon-delete" @click.stop="removeItem(index)"> </i>
                        <el-form-item
                          :required="item.required"
                          label-position="top"
                          style="width: 100%"
                          :label="$ts(item.title)"
                        >
                          <el-col>
                            <el-date-picker
                              clearable
                              style="width: 100%"
                              v-if="
                                ['date', 'datetime', 'month'].indexOf(item.type) != -1
                              "
                              v-model="item.value"
                              :type="item.type + (item.range ? 'range' : '')"
                              :placeholder="$ts('请选择')"
                              :value-format="getDateFormat(item)"
                            >
                            </el-date-picker>

                            <el-select
                              v-else-if="
                                ['select', 'selectList'].indexOf(item.type) != -1
                              "
                              v-model="item.value"
                              filterable
                              :multiple="item.type != 'select'"
                              :placeholder="$ts('请选择')"
                              @change="(val) => {}"
                              clearable
                            >
                              <el-option
                                v-for="item2 in item.data"
                                :key="item2.key"
                                :label="$ts(item2.value)"
                                :value="item2.key"
                              >
                              </el-option>
                            </el-select>
                            <div v-else-if="isNubmer(item.type)">
                              <div v-if="item.range" class="input-range">
                                <el-input-number
                                  v-model="item.value[0]"
                                  class="mx-4"
                                  controls-position="right"
                                />
                                <span>-</span>
                                <el-input-number
                                  v-model="item.value[1]"
                                  class="mx-4"
                                  controls-position="right"
                                />
                              </div>
                              <div v-else>
                                <el-input-number
                                  style="width: 100%"
                                  placeholder="请输入"
                                  v-model="item.value"
                                  class="mx-4"
                                  controls-position="right"
                                />
                              </div>
                            </div>
                            <el-input
                              v-else
                              placeholder="请输入内容"
                              v-model="item.value"
                              :disabled="item.readonly"
                              size="default"
                            ></el-input>
                          </el-col>
                        </el-form-item>
                      </div>
                    </transition-group>
                  </draggable>
                </el-form>

                <div class="custom-empty" v-show="!formOptions.length">
                  <div>
                    <div><i class="el-icon-receiving"></i></div>
                    {{ $ts("请将左边组件拖入此容器中") }}
                  </div>
                </div>
              </el-scrollbar>
            </div>
          </div>
          <div class="custom-attr">
            <div class="custom-title">
              <i class="el-icon-paperclip"></i> {{ $ts("属性配置") }}
            </div>
            <el-scrollbar style="height: 100%; padding: 10px">
              <div class="attr-item">
                <div class="attr-item-title">字段名称</div>
                <el-input
                  v-model="currentItem.title"
                  :placeholder="$ts('字段名称')"
                  disabled
                />
              </div>
              <div class="attr-item" v-show="!isDate() && !isNubmer()">
                <div class="attr-item-title">查询类型</div>

                <el-radio-group
                  @change="selectChange"
                  v-if="currentItem.type == 'select' || currentItem.type == 'selectList'"
                  v-model="currentItem.searchType"
                >
                  <!-- v-model="currentItem.value"有问题？ -->
                  <el-radio value="select">{{ "单选" }}</el-radio>
                  <el-radio value="selectList">{{ "多选" }}</el-radio>
                </el-radio-group>

                <el-select
                  v-else
                  style="width: 100%"
                  size="default"
                  v-model="currentItem.searchType"
                  :multiple="false"
                  :placeholder="$ts('请选择')"
                  clearable
                >
                  <el-option
                    v-for="item in searchTypeList"
                    :key="item.key"
                    :label="$ts(item.value)"
                    :value="item.key"
                  >
                  </el-option>
                </el-select>
              </div>
              <div class="attr-item" v-if="isDate()">
                <div class="attr-item-title">日期类型</div>
                <!-- 日期 -->
                <div>
                  <el-radio-group v-model="currentItem.type" @change="rangeChange">
                    <el-radio value="datetime" style="margin-right: 10px">{{
                      "datetime"
                    }}</el-radio>
                    <el-radio value="date" style="margin-right: 10px">{{
                      "date"
                    }}</el-radio>
                    <el-radio value="month">{{ "年月" }}</el-radio>
                  </el-radio-group>
                </div>
              </div>
              <div class="attr-item" v-if="isDate() || isNubmer()">
                <div class="attr-item-title">
                  {{ isDate() ? "日期范围查询" : "数字范围查询" }}
                </div>

                <el-radio-group v-model="currentItem.range" @change="rangeChange">
                  <el-radio :value="1">{{ "是" }}</el-radio>
                  <el-radio :value="0">{{ "否" }}</el-radio>
                </el-radio-group>
              </div>
              <div class="attr-item">
                <div class="attr-item-title">查询默认值</div>
                <!-- 日期 -->
                <div v-if="isDate()">
                  <div style="display: flex">
                    <el-date-picker
                      clearable
                      :start-placeholder="$ts('开始时间')"
                      :unlink-panels="true"
                      :end-placeholder="$ts('结束时间')"
                      v-model="currentItem.value"
                      :type="currentItem.type + (currentItem.range ? 'range' : '')"
                      :value-format="getDateFormat(currentItem)"
                      :shortcuts="getShortcuts()"
                    >
                    </el-date-picker>
                  </div>
                </div>
                <!-- <div      v-else-if="['select', 'selectList'].indexOf(currentItem.type) != -1">

                 {{JSON.stringify(currentItem)}}
                </div> -->
                <el-select
                  v-else-if="['select', 'selectList'].indexOf(currentItem.type) != -1"
                  v-model="currentItem.value"
                  filterable
                  :multiple="false"
                  :placeholder="$ts('请选择')"
                  @change="(val) => {}"
                  clearable
                >
                  <el-option
                    v-for="item in currentItem.data"
                    :key="item.key"
                    :label="$ts(item.value)"
                    :value="item.key"
                  >
                  </el-option>
                </el-select>
                <div v-else-if="isNubmer()">
                  <div v-if="currentItem.range" class="input-range">
                    <el-input-number
                      v-model="currentItem.value[0]"
                      class="mx-4"
                      controls-position="right"
                    />
                    <span>-</span>
                    <el-input-number
                      v-model="currentItem.value[1]"
                      class="mx-4"
                      controls-position="right"
                    />
                  </div>
                  <div v-else>
                    <el-input-number
                      style="width: 100%"
                      placeholder="请输入"
                      v-model="currentItem.value"
                      class="mx-4"
                      controls-position="right"
                    />
                  </div>
                </div>
                <el-input v-else v-model="currentItem.value" />
              </div>
              <div class="attr-item">
                <div class="attr-item-title">标签宽度</div>
                <el-select v-model="currentItem.width" :placeholder="$ts('请选择')">
                  <el-option
                    v-for="item in formWidth"
                    :key="item.key"
                    :label="item.value"
                    :value="item.key"
                  >
                  </el-option>
                </el-select>
                <!-- <el-slider
                  style="width: 95%"
                  :min="15"
                  v-model="currentItem.width"
                  :step="5"
                  show-stops
                >
                </el-slider> -->
              </div>
            </el-scrollbar>
          </div>
        </div>
      </div>
      <template #footer>
        <div class="filter-bottom">
          <div class="filter-left">
            <vol-custom-filter
              @delFilter="delFilter"
              @clickFilter="clickFilter"
              ref="customFilterRef"
              :cache-key="cacheKey"
            ></vol-custom-filter>
          </div>
          <div class="filter-add-btn">
            <el-button @click="addClick" link
              ><i class="add-filter el-icon-plus">添加</i></el-button
            >
          </div>
          <div>
            <el-button type="primary" plain size="small" @click="model = false"
              >关闭</el-button
            >
            <!-- <el-button type="default" size="small" @click="model = false">关闭</el-button> -->
          </div>
        </div>
      </template>
    </vol-box>
  </div>
</template>
<script lang="jsx" setup>
import { VueDraggableNext as draggable } from "vue-draggable-next";
import VolBox from "@/components/basic/VolBox.vue";
import VolCustomFields from "./VolCustomFields.vue";
import VolCustomFilter from "./VolCustomFilter.vue";
import cache from "./cache";
import { defineComponent, ref, reactive, getCurrentInstance, computed } from "vue";

const props = defineProps({
  options: {
    type: Array,
    default: () => {
      return [];
    },
  },
  cacheKey: {
    type: String,
    default: "",
  },
  showFilter: {
    type: Boolean,
    default: false,
  },
});
const isDate = (type) => {
  return ["date", "datetime", "month"].indexOf(type || currentItem.value.type) != -1;
};
const isNubmer = (type) => {
  return ["decimal", "number", "int"].indexOf(type || currentItem.value.type) != -1;
};
const getShortcuts = () => {
  const endDate = new Date();
  return [
    { name: "今天", value: 0 },
    { name: "昨天", value: 1 },
    { name: "近三天", value: 2 },
    { name: "近一周", value: 6 },
    { name: "近一月", m: true, value: 1 },
    { name: "近三月", m: true, value: 3 },
    { name: "近半年", m: true, value: 6 },
    { name: "近一年", m: true, value: 12 },
  ].map((x) => {
    return {
      text: proxy.$ts(x.name),
      value: () => {
        currentItem.value.rangeValue = x.value;
        try {
          const start = new Date();
          if (x.m) {
            start.setMonth(start.getMonth() - x.value);
            return [start.getTime(), endDate];
          }
          start.setTime(start.getTime() - 3600 * 1000 * 24 * x.value);
          return [start, endDate];
        } catch (error) {
          console.log(error);
        }
      },
    };
  });
};

const formWidth = reactive([
  { key: 20, value: "20%" },
  { key: 25, value: "25%" },
  { key: 33.333, value: "33%" },
  // { key: 40, value: '40%' },
  { key: 50, value: "50%" },
  { key: 60, value: "60%" },
  { key: 80, value: "80%" },
  { key: 100, value: "100%" },
]);
const emit = defineEmits(["customFilterClick", "customFilterChange"]);
const height = document.body.clientHeight * 0.75;
let width = document.body.clientWidth * 0.92;
if (width > 1400) {
  width = 1400;
}

const { proxy } = getCurrentInstance();
const model = ref(false);

const getFormData = (c) => {
  let obj = {};
  if (c.bind) {
    obj.type = "select";
    obj.data = c.bind.data;
    obj.dataKey = c.bind.key;
  } else {
    obj.type = c.type;
    obj.data = c.data;
    obj.dataKey = c.dataKey;
  }
  if (obj.data) {
    obj.icon = "el-icon-arrow-down";
  } else if (obj.type == "date" || obj.type == "datetime" || obj.type == "month") {
    obj.icon = "el-icon-date";
    obj.range = 1;
    obj.value = [null, null];
  } else if (obj.type == "int" || obj.type == "number" || obj.type == "decimal") {
    obj.icon = "el-icon-aim";
    obj.range = 1;
    obj.value = [null, null];
  } else {
    obj.icon = "el-icon-document";
  }
  return obj;
};

const getFormItem = (c) => {
  return Object.assign(
    {
      field: c.field,
      title: c.title,
      // type: getType(c),
      // data: c.data,
      // dataKey: c.dataKey,
      range: c.range,
      icon: "el-icon-document",
      searchType: null, //查询类型
      value: null, //默认值
    },
    getFormData(c)
  );
};
const fieldOptions = reactive([]);
if (props.options.length) {
  if (Array.isArray(props.options[0])) {
    props.options.forEach((ops) => {
      fieldOptions
        .filter((c) => {
          return !c.hidden && !c.render;
        })
        .push(
          ...ops.map((c) => {
            return getFormItem(c);
          })
        );
    });
  } else {
    fieldOptions.push(
      ...props.options
        .filter((c) => {
          return !c.hidden && !c.render;
        })
        .map((c) => {
          return getFormItem(c);
        })
    );
  }
}

const formOptions = ref([
  // { title: "11222" },
  // { title: "11" },
  // { title: "11" },
  // { title: "11" },
  // { title: "11" },
]);

const searchTypeList = computed(() => {
  let types = [];

  if (currentItem.value.data) {
    return [
      { key: "select", value: "单选" },
      { key: "selectList", value: "多选" },
    ];
  }

  switch (currentItem.value.type) {
    case "date":
    case "datetime":
    case "number":
    case "decimal":
    case "int":
      types = [
        { key: "=", value: "等于" },
        { key: ">", value: "大于" },
        { key: ">=", value: "大于等于" },
        { key: "<", value: "小于" },
        { key: "<=", value: "小于等于" },
        // { key: "range", value: "范围查询" },
      ];
      break;
    default:
      types = [
        { key: "=", value: "等于" },
        { key: "!=", value: "不等于" },
        { key: "like", value: "%模糊查询%" },
        { key: "likeStart", value: "模糊查询%" },
        { key: "likeEnd", value: "%模糊查询" },
      ];
      break;
  }
  return types;
});

const currentIndex = ref(-1);
const currentItem = ref({ option: {}, width: 25, index: -1 });
const itemClick = (item, index) => {
  currentItem.value = item;
  if (!currentItem.value.width) {
    currentItem.value.width = 25;
  }
  currentIndex.value = index;
};

const removeItem = (index) => {
  formOptions.value.splice(index, 1);
  currentIndex.value = -1;
};
const formEnd = (field) => {};

let timeMsg = false;
const checkEnd = (field) => {
  if (
    formOptions.value.some((c) => {
      return c.field == field;
    })
  ) {
    if (!timeMsg) {
      timeMsg = true;
      setTimeout(() => {
        timeMsg = false;
      }, 1000);
      proxy.$message.error("字段已存在");
    }

    return false;
  }
  return true;
};
const onMove = () => {};

const end = (ops) => {
  currentIndex.value = formOptions.value.findIndex((x) => {
    return x.field == ops.field;
  });
  currentItem.value = ops;
};

const getDateFormat = (item) => {
  if (item.format) {
    return item.format;
  }
  if (item.type == "month") {
    return "YYYY-MM";
  }

  return item.type == "date" ? "YYYY-MM-DD" : "YYYY-MM-DD HH:mm:ss";
};

const saveClick = () => {
  if (!templateName.value) {
    proxy.$message.error(proxy.$ts("请输入模板名称"));
    return;
  }
  if (!formOptions.value.length) {
    proxy.$message.error(proxy.$ts("请输查询模板字段"));
    return;
  }

  cache.setData(props.cacheKey, templateName.value, formOptions.value);
  proxy.$message.success(proxy.$ts("保存成功"));

  filterInit(formOptions.value);
};

const delClick = () => {
  formOptions.value.splice(0);
  cache.removeData(props.cacheKey, templateName.value);
  templateName.value = null;
  proxy.$message.success(proxy.$ts("删除成功"));
};

const addClick = () => {
  templateName.value = null;
  formOptions.value.splice(0);
};
const delFilter = (name) => {
  //最下面模板名称删除
  //删除
  if (templateName.value == name) {
    templateName.value = null;
    formOptions.value.splice(0);
    // cache.removeData(props.cacheKey,name)
  }
};
const delViewFilter = (name, fitlers) => {
  delFilter(name);
  // emit('customFilterChange', fitlers)
  filterInit(fitlers);
};
const filterInit = (fitlers) => {
  emit("customFilterChange", fitlers);
};

//重新绑定数据源
const initSelectData = (data) => {
  data.forEach((c) => {
    if (c.data) {
      let ops = fieldOptions.find((x) => {
        return x.field == c.field;
      });
      if (ops) {
        c.data = ops.data;
      }
    }
  });
  //
};
const clickFilter = (name) => {
  //最下面模板名称点击
  templateName.value = name;

  let data = cache.getData(props.cacheKey, name);
  data = data.map((c) => {
    //这里还要处理下默认数据源
    return Object.assign({}, c);
  });
  initSelectData(data);
  formOptions.value.splice(0);
  formOptions.value.push(...data);
};

//查询界面点击过滤器名称
const clickViewFilter = (name) => {
  let data = cache.getData(props.cacheKey, name);
  if (!data.length) {
    console.log(`未获取到:【${name}】配置信息`);
    return;
  }
  initSelectData(data);
  //生成表单信息
  const options = cache.getFormOptions(data);
  emit("customFilterClick", options);
  // emit('customFilterClick', [[{ title: 'test', field: '1' }]])
};

const rangeChange = () => {
  if (currentItem.value.range) {
    currentItem.value.value = [null, null];
  } else {
    currentItem.value.value = null;
  }
};
const selectChange = () => {
  if (currentItem.value.type == "selectList") {
    currentItem.value.value = [];
  } else {
    currentItem.value.value = null;
  }
};

const customFilterRef = ref(null);
const templateName = ref(null);
const show = () => {
  formOptions.value.splice(0);
  templateName.value = null;
  model.value = true;
};

defineExpose({ formOptions, show });
</script>
<style lang="less" scoped>
@import "./index.less";
</style>
