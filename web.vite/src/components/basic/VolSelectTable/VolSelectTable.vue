<template>
  <div ref="selectRef" class="input-select content">
    <el-popover
      :visible="show"
      popper-style="padding:0"
      :offset="4"
      :show-arrow="false"
      :teleported="true"
      :width="width"
      trigger="contextmenu"
      placement="bottom-start"
    >
      <template #reference>
        <el-input
          ref="input"
          @keypress="onKeyPress"
          @input="onInput"
          :placeholder="$ts('请输入')"
          :size="size"
          :clearable="false"
          :readonly="inputReadonly"
          :disabled="readonly"
          @click.stop="inputClick"
          v-model="val"
          suffix-icon="Search"
        >
        </el-input>
      </template>
      <div
        class="table"
        @click.stop
        :style="{
          height:
            height + (paginationHide ? 0 : 37) + (searchForm.length ? 45 : 0) + 'px',
        }"
      >
        <div class="input-search" v-if="searchForm.length || !single">
          <!-- <label>{{ $ts('搜索') }}：</label> -->
          <div class="serach-item" v-for="(item, index) in searchForm" :key="index">
            <el-select
              style="width: 120px"
              v-if="item.data"
              v-model="item.value"
              filterable
              :placeholder="$ts(item.placeholder || item.title)"
              :multiple="item.type == 'selectList' ? true : false"
              :allow-create="item.autocomplete"
              @change="item.onChange"
              clearable
            >
              <el-option
                v-show="!item.hidden"
                :disabled="item.disabled"
                v-for="item in getSelectData(item)"
                :key="item.key"
                :label="$ts(item.value)"
                :value="item.key"
              >
              </el-option>
            </el-select>
            <el-input
              style="width: 120px"
              v-else
              v-model="item.value"
              :placeholder="$ts(item.placeholder || item.title)"
            ></el-input>
          </div>
          <el-button @click="searchClick" type="primary" v-if="searchForm.length" plain
            ><i class="el-icon-search"></i>{{ $ts("查询") }}</el-button
          >
          <el-button
            v-if="!single"
            style="margin-left: 3px"
            @click="confirmBtnClick"
            type="primary"
            plain
            ><i class="el-icon-check"></i> {{ $ts("确定") }}</el-button
          >
          <!-- <el-button @click="searchResetClick" type="success" plain><i class="el-icon-refresh"></i>{{ $ts('重置')
          }}</el-button> -->
        </div>
        <vol-table
          v-if="lazy"
          ref="table"
          @loadBefore="loadTableBefore"
          @loadAfter="loadAfter"
          @rowClick="rowClick"
          :url="url"
          :load-key="true"
          :columnIndex="false"
          :tableData="tableData"
          :columns="columns"
          :pagination="pagination"
          :height="height"
          :single="single"
          :pagination-hide="paginationHide"
          :defaultLoadPage="defaultLoadPage"
          :ck="ck"
          :textInline="textInline"
          :pagerCount="5"
        ></vol-table>
      </div>
    </el-popover>
  </div>
</template>
<script>
import VolTable from "@/components/basic/VolTable.vue";
import {
  defineComponent,
  getCurrentInstance,
  reactive,
  ref,
  watch,
  watchEffect,
  onMounted,
  computed,
  onBeforeUnmount,
} from "vue";
export default {
  name: "vol-select-table",
  components: {
    "vol-table": VolTable,
  },
  props: {
    modelValue: false,
    readonly: {
      type: Boolean,
      default: false,
    },
    inputReadonly: {
      type: Boolean,
      default: false,
    },
    columns: {
      type: Array,
      default: () => {
        return [];
      },
    },
    tableData: {
      type: Array,
      default: () => {
        return [];
      },
    },
    url: {
      type: String,
      default: "",
    },
    loadBefore: {
      typeof: Function,
      default: (param, callback) => {
        //callback(true)
      },
    },
    loadAfter: {
      typeof: Function,
      default: (rows, callback, result) => {
        // callback(true)
      },
    },
    pagination: {
      type: Object,
      default: function () {
        return { total: 0, size: 10, sortName: "" };
      },
    },
    paginationHide: {
      //是否默认隐藏分页
      type: Boolean,
      default: true,
    },
    textInline: {
      //表格内容超出是否换行
      type: Boolean,
      default: true,
    },
    defaultLoadPage: {
      //是否默认加载数据
      type: Boolean,
      default: true,
    },
    single: {
      //是否单选
      type: Boolean,
      default: true,
    },
    height: {
      //表格高度
      type: Number,
      default: 200,
    },
    width: {
      type: Number,
      default: 500,
    },
    onSelect: {
      type: Function,
      default: (rows) => {},
    },
    size: {
      type: String, //large / default / small
      default: "default",
    },
    field: {
      type: String,
      default: "",
    },
    inputChangeLoadData: {
      type: Boolean,
      default: true,
    },
  },
  setup(props, context) {
    const { proxy } = getCurrentInstance();
    const val = ref(null);
    val.value = props.modelValue;

    const ck = ref(false);
    ck.value = !props.single;

    const show = ref(false);
    //搜索
    const getTableData = (row) => {};

    const rowClick = ({ row, column, event }) => {
      if (props.single) {
        props.onSelect([row]);
        show.value = false;
      }
    };
    let isClick = true;
    const inputClick = () => {
      if (!props.url) {
        proxy.$message.error(
          "未配置url属性"
        );
        return;
      }
      currentIndex = -1;
      if (!show.value) {
        searchClick();
      }
      show.value = true;
      lazy.value = true;
      isClick = true;
    };
    const lazy = ref(false);

    const table = ref(null);

    const valChange = () => {
      context.emit("update:modelValue", val.value);
      // if (isClick) {
      //   isClick = false;
      //   return;
      // }
      if (lazy.value && show.value) {
        searchClick();
      }
    };

    const searchClick = () => {
      table.value && table.value.load({ loadType: 2 }, true);
    };

    const searchResetClick = () => {
      searchForm.forEach((x) => {
        if (Array.isArray(x.value)) {
          x.value = [];
        } else {
          x.value = null;
        }
      });
    };

    watch(
      () => props.modelValue,
      (newVal, oldVal) => {
        if (newVal && !lazy.value) {
          // lazy.value = true;
        }
        val.value = newVal;
      }
    );
    watch(
      () => val.value,
      (newVal, oldVal) => {
        valChange(newVal);
      }
    );

    const getSelectData = (item) => {
      return props.columns.find((x) => {
        return x.field == item.field;
      }).bind.data;
    };

    const searchForm = reactive([]);
    props.columns.forEach((c) => {
      if (
        c.search &&
        searchForm.length < 5 &&
        (c.type != "date" || c.type != "datetime")
      ) {
        let data = {
          field: c.field,
          title: c.title,
          type: c.type,
          // data: c.bind ? c.bind.data : null,
          // data: c.bind ? c.bind.data : null,
          value: null,
          placeholder: c.placeholder,
        };
        if (c.bind && c.bind.data) {
          data.data = [];
        }
        searchForm.push(data);
      }
    });

    const loadTableBefore = (params, callback) => {
      currentIndex = -1;
      if (searchForm.length) {
        let wheres = searchForm
          .filter((x) => {
            return x.value || x.value + "" === "0";
          })
          .map((x) => {
            return {
              name: x.field,
              value: x.value,
              displayType: x.data ? "" : "like",
            };
          });
        if (
          !props.inputReadonly &&
          !searchForm.some((x) => {
            return x.field == props.field;
          })
        ) {
          wheres.push({ name: props.field, value: val.value, displayType: "like" });
        }

        params.wheres.push(...wheres);
      }
      //点击时第一次不走自定义查询
      // if (isClick) {
      //   callback(true);
      //   isClick=false;
      //   return;
      // }
      proxy.loadBefore(params, callback);
    };

    const selectRef = ref(null);
    const handleClickOutside = (event) => {
      if (selectRef.value && !selectRef.value.contains(event.target)) {
        show.value = false;
      }
    };

    const confirmBtnClick = () => {
      let _rows = table.value.getSelected();
      if (_rows && _rows.length) {
        props.onSelect(_rows);
        show.value = false;
      }
    };
    let currentIndex = -1;
    const handleClickKeydown = (e) => {
      if (!show.value) {
        return;
      }
      if (e.keyCode == 13) {
        let row = table.value.rowData[currentIndex];
        if (row) {
          props.onSelect([row]);
          show.value = false;
        }
        return;
      }
      if (e.keyCode == 38 || e.keyCode == 40) {
        e.preventDefault();
        //向上
        if (e.keyCode == 38) {
          if (currentIndex > 0) {
            currentIndex--;
            if (currentIndex >= table.value.rowData.length) {
              currentIndex = table.value.rowData.length - 2;
              if (currentIndex < 0) {
                currentIndex = 0;
              }
            }
            table.value.$refs.table.setCurrentRow(table.value.rowData[currentIndex]);
          }
        } else {
          currentIndex++;
          if (currentIndex < 0) {
            currentIndex = 0;
          }
          if (currentIndex < table.value.rowData.length) {
            table.value.$refs.table.setCurrentRow(table.value.rowData[currentIndex]);
          }
        }
        table.value.$refs.table.setScrollTop((currentIndex - 1) * 37.8);
      }
    };
    onMounted(() => {
      document.addEventListener("click", handleClickOutside);
      document.addEventListener("keydown", handleClickKeydown);
    });

    onBeforeUnmount(() => {
      // 组件销毁前移除事件监听器
      document.removeEventListener("click", handleClickOutside);
      document.removeEventListener("keydown", handleClickKeydown);
    });

    const input = ref(null);
    const focus = () => {
      input.value.focus();
      setTimeout(() => {
        currentIndex = -1;
        show.value = true;
        lazy.value = true;
        isClick = true;
      }, 50);
    };
    const onInput = () => {
      context.emit("onKeyPress", val.value, {});
    };
    const onKeyPress = ($event) => {
      context.emit("onKeyPress", val.value, $event);
      if ($event.keyCode == 13) {
        table.value && table.value.load({ loadType: 1 }, true);
      }
    };
    return {
      show,
      lazy,
      val,
      table,
      getTableData,
      rowClick,
      inputClick,
      selectRef,
      handleClickOutside,
      searchForm,
      loadTableBefore,
      ck,
      searchResetClick,
      searchClick,
      handleClickKeydown,
      focus,
      input,
      confirmBtnClick,
      onInput,
      onKeyPress,
      getSelectData,
    };
  },
};
</script>
<style lang="less" scoped>
.input-search {
  display: flex;
  height: 40px;
  padding: 4px 3px;
}

.serach-item {
  margin-right: 4px;
  // display: flex;
}

.table ::v-deep(.cell) {
  padding: 0 12px !important;
  font-size: 12px !important;
}

.table ::v-deep(.el-table__cell) {
  padding: 5px 0 !important;
}

::v-deep(.el-popover) {
  padding: 0 !important;
}

::v-deep(.el-pagination__jump) {
  display: none;
}

::v-deep(.el-pagination) {
  * {
    font-size: 13px;
  }

  // .el-pager li:nth-child(n + 4) {
  //  display: none;
  //  }
}
</style>
