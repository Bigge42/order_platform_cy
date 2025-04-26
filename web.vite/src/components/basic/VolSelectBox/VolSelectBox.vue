<template>
  <div style="">
    <!-- <el-button @click="visible = !visible;">click</el-button> -->
    <!-- <el-popover :visible="visible" placement="bottom-start" popper-style="max-width:800px" width="auto" trigger="click">
      <template #reference>
        <div class="select-wrapper">
          <el-input prefix-icon="Search" :readonly="readonly" v-model="val" @click="showPopover" :clearable="false">
          </el-input>
          <div @click="clearClick" class="select-clear"> <el-icon>
              <CircleClose />
            </el-icon></div>
        </div>
    
      </template> -->
    <div class="select-table">
      <div class="select-wrapper">

        <el-input style="width: 100%;height: 34px;"  :size="size" v-model="val" :readonly="readonly" :placeholder="$ts('请选择')"
          class="input-with-select">
          <!-- <template #prepend>
            <el-button :icon="Search" />
          </template> -->
          <template #append>
            <el-button @click="showPopover" link> <i style="font-size: 16px;" class="el-icon-search"></i></el-button>

          </template>
        </el-input>

        <!-- <el-input prefix-icon="Search" :readonly="readonly" v-model="val" @click="showPopover" :clearable="false">
        </el-input>
        <div @click="clearClick" class="select-clear"> <el-icon>
            <CircleClose />
          </el-icon></div> -->
      </div>
      <el-dialog class="vol-dialog" v-model="visible" :title="$ts('选择数据')" :width="width">
        <div style="max-height: 500px;">
          <div class="input-search" v-if="searchForm.length">
            <label>{{ $ts('搜索') }}：</label>
            <div class="serach-item" v-for="(item, index) in searchForm" :key="index">
              <!-- <span class="label">{{ $ts(item.title) }}:</span> -->
<!-- 
              -->
              <el-select style="width: 160px;" v-if="item.data" v-model="item.value" filterable
               :placeholder="$ts(item.placeholder || item.title)"
               :multiple="item.type == 'selectList' ? true : false" 
                :allow-create="item.autocomplete" @change="item.onChange" clearable>
                <el-option v-show="!item.hidden" :disabled="item.disabled" v-for="item in item.data" :key="item.key"
                  :label="$ts(item.value)" :value="item.key">
                </el-option>
              </el-select>
              <el-input style="width: 160px;" v-else v-model="item.value" :placeholder="$ts(item.title)"></el-input>
            </div>
            <el-button @click="searchClick" type="primary" plain><i class="el-icon-search"></i>{{ $ts('查询') }}</el-button>
            <el-button @click="searchResetClick" type="success" plain><i class="el-icon-refresh"></i>{{ $ts('重置')
            }}</el-button>
          </div>
          <vol-table v-if="lazy" ref="table" @loadBefore="loadTableBefore" @loadAfter="loadTableAfter" @rowClick="rowClick"
            :url="url" :load-key="true" :index="true" :tableData="tableData" :columns="columns" :pagination="pagination"
            :height="height" :single="single" :pagination-hide="paginationHide" :defaultLoadPage="defaultLoadPage"
            :ck="ck" :text-inline="textInline"></vol-table>
          <div v-if="!single" style="text-align: center;margin-top: 10px;">
            <el-button size="small" @click="visible = false">{{ $ts('关闭') }}</el-button>
            <el-button type="primary" size="small" @click="confirm">{{ $ts('确定') }}</el-button>
          </div>
        </div>
      </el-dialog>

    </div>
    <!-- </el-popover> -->
  </div>
</template>

<script>
import { defineComponent, getCurrentInstance, reactive, ref, watch, watchEffect } from 'vue';
import VolTable from "@/components/basic/VolTable.vue";
export default defineComponent({
  props: {
    modelValue: false,
    readonly: {
      type: Boolean,
      default: true
    },
    columns: {
      type: Array,
      default: () => {
        return []
      }
    },
    tableData: {
      type: Array,
      default: () => {
        return []
      }
    },
    url: {
      type: String,
      default: ""
    },
    loadBefore: {
      typeof: Function,
      default: (param, callback) => {
        //callback(true)
      }
    },
    loadAfter: {
      typeof: Function,
      default: (rows, callback, result) => {
        // callback(true)
      }
    },
    pagination: {
      type: Object,
      default: function () {
        return { total: 0, size: 30, sortName: "" };
      },
    },

    paginationHide: {//是否默认隐藏分页
      type: Boolean,
      default: false
    },
    textInline: {//表格内容超出是否换行
      type: Boolean,
      default: true
    },
    defaultLoadPage: { //是否默认加载数据
      type: Boolean,
      default: true
    },
    single: { //是否单选
      type: Boolean,
      default: true
    },
    height: { //表格高度
      type: Number,
      default: 420
    },
    onSelect: {
      type: Function,
      default: (rows) => {
      }
    },
    size: {
      type: String, //large / default / small
      default: "default",
    },
  },
  components: {
    'vol-table': VolTable
  },
  setup(props, context) {
    const val = ref(null);
    val.value = props.modelValue;
    const visible = ref(false);
    const showPopover = () => {
      visible.value = !visible.value;
      if (visible.value && table.value) {
        table.value.load()
      }
    }
    const clearClick = () => {
      val.value = null;
      valChange();
    }
    const valChange = () => {
      context.emit("update:modelValue", val.value);
    }
    const ck = ref(false);
    ck.value = !props.single;

    const width = ref(500);

    let w = 100;

    const searchForm = reactive([]);

    props.columns.forEach(c => {
      if (!c.hidden) {
        w += c.width || 100;
      }
      if (c.search && searchForm.length < 5 && (c.type != 'date' || c.type != 'datetime')) {
        let data = {
          field: c.field,
          title: c.title,
          type: c.type,
          data: c.bind ? c.bind.data : null,
          value: null
        }
        if (c.bind && c.bind.data) {
          data.data = c.bind.data;
        }
        searchForm.push(data)
      }
    })
    if (w > 1200) {
      w = 1200
    } else if (w < 200) {
      w = 300;
    }
    width.value = w + 200;
    watch(
      () => props.modelValue,
      (newVal, oldVal) => {
        if (newVal && !lazy.value) {
          lazy.value = true;
        }
        val.value = newVal;
      }
    );


    watch(
      () => val.value,
      (newVal, oldVal) => {
        valChange(newVal)
      }
    );

    const rowClick = ({ row, column, event }) => {
      if (props.single) {
        props.onSelect([row]);
        visible.value = false;
      }
    }

    const table = ref(null)
    const { proxy } = getCurrentInstance();
    const confirm = () => {
      // proxy.$message.error('111')
      // return;
      let rows = table.value.getSelected();
      if (!rows.length) {
        proxy.$message.error(proxy.$ts('请选择数据'))
        return
      }
      props.onSelect(rows);
      visible.value = false;
    }
    const lazy = ref(false);

    const searchClick = () => {
      table.value.load(null, true)
    }

    const searchResetClick = () => {
      searchForm.forEach(x => {
        if (Array.isArray(x.value)) {
          x.value = []
        } else {
          x.value = null;
        }
      })
    }
    const loadTableBefore = (params, callback) => {

      if (searchForm.length) {
        let wheres = searchForm.filter(x=>{return x.value||x.value+''==='0'}).map(x => {
          return {
            name: x.field,
            value: x.value,
            displayType: x.data ? '' : 'like'
          }
        })
        params.wheres.push(...wheres);
      }
      proxy.loadBefore(params, callback)
    }
    const loadTableAfter=(params, callback,result)=>{
      proxy.loadAfter(params, callback,result)
    }

    return {
      visible,
      showPopover,
      clearClick,
      val,
      ck,
      rowClick,
      confirm,
      lazy,
      width,
      table,
      searchForm,
      searchClick,
      loadTableBefore,
      searchResetClick,
      loadTableAfter
    }
  },
  data() {
  },
  methods: {
  }
})

</script>
<style scoped lang="less">
.select-wrapper {
  position: relative;

  .select-clear {
    visibility: hidden;
    cursor: pointer;
    position: absolute;
    right: 7px;
    top: 2px;
    background: #fffe;
    bottom: 2px;
    justify-content: center;
    align-items: center;
    display: flex;
    font-size: 14px;
    color: #aaa4a4;
  }
}

.select-wrapper:hover {
  .select-clear {
    visibility: unset;
  }
}

.select-table ::v-deep(.el-dialog__body) {
  padding: 10px !important;
}

.select-table ::v-deep(.el-dialog) {
  margin: auto;
}

.select-table ::v-deep(.el-overlay-dialog) {
  display: flex !important;
}

.select-table ::v-deep(.el-dialog__title) {
  font-size: 14px;
}

.select-table ::v-deep(.el-input-group__append) {
  width: 36px;
}

.input-search {
  display: flex;
  height: 35px;
  margin-bottom: 8px;
}

.serach-item {
  margin-right: 18px;
  // display: flex;
}
</style>
