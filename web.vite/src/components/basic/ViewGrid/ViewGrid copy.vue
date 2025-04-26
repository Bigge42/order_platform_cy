<template>
  <div
    class="layout-container"
    :class="{ 'layout-container-padding': $global.gridPadding || padding }"
  >
    <a :href="exportHref" ref="export"></a>
    <!--开启懒加载2020.12.06 -->
    <vol-box
      :on-model-close="closeCustomModel"
      v-model="viewModel"
      :width="500"
      :padding="0"
      :lazy="true"
      title="设置"
    >
      <template #content>
        <custom-column :view-columns="viewColumns"></custom-column>
      </template>
      <template #footer>
        <div style="text-align: center">
          <el-button type="default" size="small" @click="closeCustomModel"
            ><i class="el-icon-close"></i>{{ $ts("取消") }}</el-button
          >
          <el-button type="success" size="small" @click="initViewColumns(true)"
            ><i class="el-icon-refresh"></i>{{ $ts("重置") }}</el-button
          >
          <el-button type="primary" size="small" @click="saveColumnConfig"
            ><i class="el-icon-check"></i>{{ $ts("确定") }}</el-button
          >
        </div>
      </template>
    </vol-box>
    <ViewGridAudit
      @auditClick="saveAudit"
      @flowLoadAfter="flowLoadAfter"
      @signAfter="signAfter"
      :option="table"
      ref="audit"
    >
    </ViewGridAudit>

    <!--导入excel功能-->
    <!--2020.10.31添加导入前的方法-->
    <!--开启懒加载2020.12.06 -->
    <!-- 2022.01.08增加明细表导入判断 -->
    <vol-box
      v-if="upload.url"
      v-model="upload.excel"
      :width="600"
      :lazy="true"
      :title="(boxModel ? detailOptions.cnName : table.cnName) + '-导入'"
    >
      <UploadExcel
        ref="upload_excel"
        @importExcelAfter="importExcelAfter"
        :importExcelBefore="importExcelBefore"
        :url="upload.url"
        :template="upload.template"
        :desc="importDesc"
      ></UploadExcel>
    </vol-box>
    <!--头部自定义组件-->
    <component
      :is="dynamicComponent.gridHeader"
      ref="gridHeader"
      @parentCall="parentCall"
    ></component>
    <!--主界面查询与table表单布局-->
    <div class="view-container">
      <!-- 2020.09.11增加固定查询表单 -->
      <!--查询条件-->
      <div class="grid-search">
        <div ref="customSearchRef">
          <vol-custom-search
            @customFilterClick="customFilterClick"
            @customFilterChange="customFilterChange"
            v-if="showCustom"
            show-filter
            :cache-key="table.name"
            ref="customSearch"
            :options="columns"
          >
          </vol-custom-search>
        </div>
        <div
          ref="fiexdSearchBox"
          :class="[fiexdSearchForm ? 'fiexd-search-box' : 'search-box']"
          v-show="searchBoxShow"
        >
          <!-- 2020.09.13增加formFileds拼写错误兼容处理 -->
          <vol-form
            v-if="searchFormOptions.length"
            ref="searchForm"
            :load-key="false"
            :label-width="labelWidth"
            :formRules="searchFormOptions"
            :formFields="searchFormFields"
            :label-position="labelPosition"
            :select2Count="select2Count"
          >
            <template #footer>
              <div v-if="!fiexdSearchForm" class="form-closex">
                <el-button size="small" type="primary" plain @click="search">
                  <i class="el-icon-search" />{{ $ts("查询") }}
                </el-button>

                <el-button size="small" type="success" plain @click="resetSearch">
                  <i class="el-icon-refresh-right" />{{ $ts("重置") }}
                </el-button>
                <el-button size="small" plain @click="searchBoxShow = !searchBoxShow">
                  <i class="el-icon-switch-button" />{{ $ts("关闭") }}
                </el-button>
              </div>
            </template>
          </vol-form>
          <div v-if="fiexdSearchForm" class="fs-line"></div>
        </div>
        <div class="view-header">
          <div class="desc-text">
            <i class="el-icon-s-grid" />
            <span>{{ $ts(table.cnName) }}</span>
          </div>
          <!-- <view-grid-expand
                          :render="btn.render"
                          :item="btn"
                          v-if="btn.render"
                        ></view-grid-expand> -->
          <view-grid-expand
            :render="gridRender.h"
            :item="gridRender.data"
          ></view-grid-expand>
          <div class="notice">
            <div v-if="text" v-html="text"></div>
            <a class="text" :title="extend.text">{{ extend.text }}</a>
          </div>
          <!--快速查询字段-->
          <div class="search-line" v-if="!fiexdSearchForm && !searchBoxShow">
            <QuickSearch
              ref="quickSearch"
              v-if="singleSearch"
              :searchFormOptions="searchFormOptions"
              :searchFormFields="searchFormFields"
              :select2Count="select2Count"
              :label-width="labelWidth"
              :queryFields="queryFields"
              @tiggerPress="quickSearchKeyPress"
            ></QuickSearch>
          </div>
          <!--操作按钮组-->
          <!-- 2020.11.29增加查询界面hidden属性 -->

          <div class="btn-group">
            <template
              :key="bIndex"
              v-for="(btn, bIndex) in buttons.slice(0, maxBtnLength)"
            >
              <template v-if="btn.data">
                <el-dropdown size="small" :split-button="false">
                  <el-button
                    :color="btn.color"
                    :dark="false"
                    :type="btn.type"
                    :plain="btn.plain"
                  >
                    {{ $ts(btn.name) }}
                    <i class="el-icon-arrow-down el-icon--right"></i
                  ></el-button>
                  <template #dropdown>
                    <el-dropdown-menu>
                      <el-dropdown-item v-for="(item, index) in btn.data" :key="index">
                        <div @click="onClick(item.onClick)">
                          <i :class="item.icon"></i>
                          {{ $ts(item.name) }}
                        </div>
                      </el-dropdown-item>
                    </el-dropdown-menu>
                  </template>
                </el-dropdown>
              </template>

              <view-grid-expand
                :render="btn.render"
                :item="btn"
                v-else-if="btn.render"
              ></view-grid-expand>
              <el-button
                v-else
                :type="btn.type"
                size="small"
                :color="btn.color"
                :dark="false"
                :class="btn.class"
                :plain="btn.plain"
                v-show="!btn.hidden"
                :disabled="btn.readonly || btn.disabled"
                @click="onClick(btn.onClick)"
              >
                <i :class="btn.icon"></i> {{ $ts(btn.name) }}
              </el-button>
            </template>

            <el-dropdown
              size="small"
              @click="changeDropdown"
              v-if="buttons.length > maxBtnLength"
            >
              <el-button type="primary" plain size="small" class="more-btn">
                {{ $ts("更多") }}<i class="el-icon-arrow-down el-icon--right"></i>
              </el-button>
              <template #dropdown>
                <el-dropdown-menu>
                  <template
                    v-for="(item, dIndex) in buttons.slice(maxBtnLength, buttons.length)"
                    :key="dIndex"
                  >
                    <el-dropdown-item
                      @click="changeDropdown(item.name)"
                      :name="item.name"
                    >
                      <div v-show="!item.hidden">
                        <i :class="item.icon"></i>
                        {{ $ts(item.name) }}
                      </div>
                    </el-dropdown-item>
                  </template>
                </el-dropdown-menu>
              </template>
            </el-dropdown>
            <!-- <el-button
              class="setting-btn"
              type="default"
              style="padding: 0px 10px;font-wei"
              size="small"
              :plain="true"
              v-if="showCustom"
              @click="showCustomModel"
            >
              <i class="bi-filter-left"></i>
            </el-button> -->
            <el-button
              class="setting-btn"
              type="default"
              style="padding-left: 9px !important; padding-right: 10px !important"
              size="small"
              :plain="true"
              v-if="showCustom"
              @click="$refs.customSearch.show()"
            >
              <img style="height: 15px" src="./filter.svg" alt="filter" />
            </el-button>

            <!-- <el-button
              class="setting-btn"
              type="default"
              style="padding: 0px 10px;font-wei"
              size="small"
              :plain="true"
              v-if="showCustom"
              @click="$refs.customSearch.show()"
            >
              <i class="bi-filter-left"></i>
            </el-button> -->
          </div>
        </div>

        <!-- 分割位置 -->
        <vol-box
          v-if="boxInit"
          v-model="boxModel"
          :title="boxOptions.title"
          :width="boxOptions.width"
          :height="boxOptions.height"
          :modal="boxOptions.modal"
          :draggable="boxOptions.draggable"
          :padding="0"
          :on-model-close="onGridModelClose"
          @fullscreen="fullscreen"
        >
          <!--明细头部自定义组件-->
          <template #content>
            <div class="vol-edit-box">
              <div class="vol-edit-content">
                <component
                  :is="dynamicComponent.modelHeader"
                  ref="modelHeader"
                  @parentCall="parentCall"
                ></component>
                <!-- <div v-show="isBoxAudit" class="flow-step">
                <div v-for="(item, index) in workFlowSteps" :key="index">
                  {{ item.stepName }}
                </div>
              </div> -->
                <div class="item form-item" style="padding-top: 10px">
                  <vol-form
                    ref="form"
                    :editor="editor"
                    :load-key="false"
                    :label-width="boxOptions.labelWidth"
                    :formRules="editFormOptions"
                    :formFields="editFormFields"
                    :select2Count="select2Count"
                    :label-position="labelPosition"
                    @tabClick="editFormTabClick"
                  ></vol-form>
                </div>
                <!--明细body自定义组件-->
                <component
                  :is="dynamicComponent.modelBody"
                  ref="modelBody"
                  @parentCall="parentCall"
                ></component>
                <div
                  v-show="hasDetail"
                  v-if="detail.columns && detail.columns.length > 0"
                  class="grid-detail table-item item"
                >
                  <div class="toolbar">
                    <div class="title form-text">
                      <span>
                        <i class="el-icon-menu" />
                        {{ $ts(detail.cnName) }}
                      </span>
                    </div>
                    <!--明细表格按钮-->
                    <div class="btns detail-btns" v-show="!isBoxAudit">
                      <template
                        v-for="(btn, bIndex) in detailOptions.buttons"
                        :key="bIndex"
                      >
                        <view-grid-expand
                          :render="btn.render"
                          :item="btn"
                          v-if="btn.render"
                        ></view-grid-expand>
                        <el-button
                          v-else
                          :plain="btn.plain"
                          v-show="!(typeof btn.hidden == 'boolean' && btn.hidden)"
                          @click="onClick(btn.onClick)"
                          size="small"
                          ><span :style="{ color: btn.color }"
                            ><i :class="btn.icon"></i>{{ $ts(btn.name) }}</span
                          ></el-button
                        >
                      </template>
                    </div>
                  </div>
                  <vol-table
                    ref="detail"
                    @loadBefore="loadInternalDetailTableBefore"
                    @loadAfter="loadDetailTableAfter"
                    @rowChange="detailRowOnChange"
                    @rowClick="detailRowOnClick"
                    :url="detailOptions.url"
                    :load-key="true"
                    :index="true"
                    :tableData="detailOptions.data"
                    :columns="detailOptions.columns"
                    :pagination="detailOptions.pagination"
                    :height="detailOptions.height"
                    :single="detailOptions.single"
                    :pagination-hide="detailOptions.paginationHide"
                    :defaultLoadPage="detailOptions.load"
                    :beginEdit="detailOptions.beginEdit"
                    :endEditBefore="detailOptions.endEditBefore"
                    :endEditAfter="detailOptions.endEditAfter"
                    :column-index="detailOptions.columnIndex"
                    :ck="detailOptions.ck"
                    :text-inline="detailOptions.textInline"
                    :select2Count="select2Count"
                    :selectable="detailSelectable"
                    :spanMethod="detailSpanMethod"
                    :sortable="detailOptions.sortable"
                    @onSortEnd="detailOnSortEnd"
                  ></vol-table>
                </div>

                <div
                  class="details"
                  :class="{ 'details-horizontal': multiple.horizontal }"
                >
                  <!-- 多表明细2023.07.02 -->
                  <detail-table
                    ref="details"
                    :class="{ 'detail-item-horizontal': multiple.horizontal }"
                    :style="{
                      width: multiple.horizontal ? multiple.leftWidth + 'px' : null,
                      flex: multiple.leftWidth ? '' : 1,
                    }"
                    @loadBefore="loadInternalDetailTableBefore"
                    @loadAfter="loadDetailTableAfter"
                    @rowChange="detailRowOnChange"
                    @rowClick="detailRowOnClick"
                    @tabsClick="tabsClick"
                    v-if="details.length"
                    :main-table="table.url"
                    :height="detailHeight"
                    :data="details"
                    @onSortEnd="detailOnSortEnd"
                    :selectable="detailSelectable"
                    :showTabs="showTabs"
                  >
                  </detail-table>

                  <!-- 三级明细2023.09.17 -->
                  <detail-table
                    :class="{ 'detail-item-horizontal': multiple.horizontal }"
                    :style="{
                      width: multiple.horizontal ? multiple.rightWidth + 'px' : null,
                      flex: multiple.rightWidth ? '' : 1,
                    }"
                    ref="subDetails"
                    v-if="subDetails.length"
                    @loadBefore="loadSubInternalDetailTableBefore"
                    @loadAfter="loadSubDetailTableAfter"
                    @rowClick="detailRowOnClick"
                    :main-table="table.url"
                    :height="multiple.horizontal ? detailHeight : 200"
                    @tabsClick="tabsClick"
                    :data="subDetails"
                    @rowChange="detailRowOnChange"
                    @onSortEnd="detailOnSortEnd"
                    :selectable="detailSelectable"
                  >
                  </detail-table>
                </div>

                <!--明细footer自定义组件-->
                <component
                  :is="dynamicComponent.modelFooter"
                  ref="modelFooter"
                  @parentCall="parentCall"
                ></component>
              </div>
              <div class="vol-edit-box-right">
                <component
                  :is="dynamicComponent.modelRight"
                  ref="modelRight"
                  @parentCall="parentCall"
                ></component>
              </div>
            </div>
          </template>
          <template #footer>
            <div style="text-align: center" v-show="isBoxAudit">
              <el-button
                size="small"
                type="primary"
                plain
                @click="onGridModelClose(false)"
              >
                <i class="el-icon-close">{{ $ts("关闭") }}</i>
              </el-button>
              <el-button
                size="small"
                type="primary"
                v-show="auditParam.showViewButton"
                @click="auditParam.model = true"
              >
                <i class="el-icon-view">{{ $ts("审批") }}</i>
              </el-button>
            </div>
            <div v-show="!isBoxAudit">
              <el-button
                v-for="(btn, bIndex) in boxButtons"
                :key="bIndex"
                :type="btn.type"
                size="small"
                :plain="btn.plain"
                v-show="!(typeof btn.hidden == 'boolean' && btn.hidden)"
                :disabled="btn.hasOwnProperty('disabled') && !!btn.disabled"
                @click="onClick(btn.onClick)"
              >
                <i :class="btn.icon"></i>{{ $ts(btn.name) }}
              </el-button>
              <el-button
                size="small"
                type="primary"
                plain
                @click="onGridModelClose(false)"
              >
                <i class="el-icon-close">{{ $ts("关闭") }}</i>
              </el-button>
            </div>
          </template>
        </vol-box>
      </div>
      <!--body自定义组件-->
      <div class="grid-body">
        <component
          :is="dynamicComponent.gridBody"
          ref="gridBody"
          @parentCall="parentCall"
        ></component>
      </div>

      <!--table表格-->
      <div class="grid-container">
        <!-- 2021.05.02增加树形结构 rowKey -->
        <vol-table
          ref="table"
          :single="single"
          :rowKey="rowKey"
          :loadTreeChildren="loadTreeTableChildren"
          @loadBefore="loadTableBefore"
          @loadAfter="loadTableAfter"
          @rowChange="rowOnChange"
          @rowClick="rowOnClick"
          @rowDbClick="rowOnDbClick"
          @selectionChange="selectionOnChange"
          :tableData="[]"
          :linkView="linkData"
          :columns="columns"
          :pagination="pagination"
          :height="height"
          :max-height="tableMaxHeight"
          :pagination-hide="paginationHide"
          :url="url"
          :load-key="false"
          :defaultLoadPage="load"
          :double-edit="doubleEdit"
          :index="true"
          :beginEdit="tableBeginEdit"
          :endEditBefore="tableEndEditBefore"
          :column-index="columnIndex"
          :text-inline="textInline"
          :ck="ck"
          :select2Count="select2Count"
          :selectable="selectable"
          :lazy="lazy"
          :defaultExpandAll="defaultExpandAll"
          :rowParentField="rowParentField"
          :expandRowKeys="expandRowKeys"
          :dragPosition="dragPosition"
          :spanMethod="spanMethod"
          :reserveSelection="reserveSelection"
          :sortable="sortable"
          @onSortEnd="onSortEnd"
          :extraHeight="extraHeight"
        ></vol-table>
      </div>
    </div>

    <!--footer自定义组件-->
    <component
      :is="dynamicComponent.gridFooter"
      ref="gridFooter"
      @parentCall="parentCall"
    ></component>
  </div>

  <ViewGridPrint ref="print"></ViewGridPrint>
</template>

<script lang="jsx">
const _const = {
  EDIT: "update",
  ADD: "Add",
  VIEW: "view",
  PAGE: "getPageData",
  AUDIT: "audit",
  DEL: "del",
  EXPORT: "Export", //导出操作返回加密后的路径
  DOWNLOAD: "DownLoadFile", //导出文件
  DOWNLOADTEMPLATE: "DownLoadTemplate", //下载导入模板
  IMPORT: "Import", //导入(导入表的Excel功能)
  UPLOAD: "Upload", //上传文件
};
import ViewGridExpand from "./ViewGridExpand";
import Empty from "@/components/basic/Empty.vue";
import VolTable from "@/components/basic/VolTable.vue";
import VolForm from "@/components/basic/VolForm.vue";
import { defineAsyncComponent, defineComponent, ref, shallowRef, toRaw } from "vue";
import VolCustomSearch from "@/components/basic/VolCustomSearch/VolCustomSearch.vue";
import ViewGridDetails from "./ViewGridDetails.vue";
var vueParam = {
  components: {
    ViewGridExpand,
    "vol-form": VolForm,
    "vol-table": VolTable,
    VolBox: defineAsyncComponent(() => import("@/components/basic/VolBox.vue")),
    QuickSearch: defineAsyncComponent(() => import("@/components/basic/QuickSearch.vue")),
    Audit: defineAsyncComponent(() => import("@/components/basic/Audit.vue")),
    UploadExcel: defineAsyncComponent(() => import("@/components/basic/UploadExcel.vue")),
    "custom-column": defineAsyncComponent(() => import("./ViewGridCustomColumn.vue")),
    "vol-header": defineAsyncComponent(() => import("./../VolHeader.vue")),
    ViewGridAudit: defineAsyncComponent(() => import("./ViewGridAudit.vue")),
    ViewGridPrint: defineAsyncComponent(() => import("./ViewGridPrint.vue")),
    "detail-table": ViewGridDetails,
    "vol-custom-search": VolCustomSearch,
    //  defineAsyncComponent(() =>
    //   import('@/components/basic/VolCustomSearch/VolCustomSearch.vue'))
  },
  emits: ["parentCall"],
  props: {},
  setup(props) {
    console.log("setup");
    //2021.07.17调整扩展组件组件
    const dynamicCom = {
      gridHeader: Empty,
      gridBody: Empty,
      gridFooter: Empty,
      modelHeader: Empty,
      modelBody: Empty,
      modelRight: Empty,
      modelFooter: Empty,
    };
    //合并扩展组件
    if (props.extend.components) {
      for (const key in props.extend.components) {
        if (props.extend.components[key]) {
          dynamicCom[key] = toRaw(props.extend.components[key]);
        }
      }
    }
    const dynamicComponent = shallowRef(dynamicCom);
    return { dynamicComponent };
  },
  data() {
    return {
      isBoxAudit: false,
      formFieldsType: [],
      workFlowSteps: [],
      //树形结构的主键字段，如果设置值默认会开启树形table；注意rowKey字段的值必须是唯一（2021.05.02）
      rowKey: undefined,
      fiexdSearchForm: false, //2020.09.011是否固定查询表单，true查询表单将固定显示在表单的最上面
      _inited: false,
      doubleEdit: false, //2021.03.19是否开启查询界面表格双击编辑
      single: false, //表是否单选
      const: _const, //增删改查导入导出等对应的action
      boxInit: false, //新建或编辑的弹出框初化状态，默认不做初始化，点击新建或编辑才初始化弹出框
      searchBoxShow: false, //高级查询(界面查询后的下拉框点击触发)
      singleSearch: {}, //快速查询字段
      exportHref: "",
      currentAction: _const.ADD, //当新建或编辑时，记录当前的状态:如当前操作是新建
      currentRow: {}, //当前编辑或查看数据的行
      closable: false,
      boxModel: false, //弹出新建、编辑框
      width: 700, //弹出框查看表数据结构
      labelWidth: 90, //高级查询的标签宽度
      viewModel: false, //查看表结构的弹出框
      viewColumns: [], //查看表结构的列数据
      viewColumnsClone: [],
      showCustom: true, //是否显示自定义配置列按钮2022.05.27
      // viewData: [], //查看表结构信息
      maxBtnLength: 8, //界面按钮最多显示的个数，超过的数量都显示在更多中
      buttons: [], //查询界面按钮  如需要其他操作按钮，可在表对应的.js中添加(如:Sys_User.js中buttons添加其他按钮)
      splitButtons: [],
      uploadfiled: [], //上传文件图片的字段
      boxButtons: [], //弹出框按钮 如需要其他操作按钮，可在表对应的.js中添加
      dicKeys: [], //当前界面所有的下拉框字典编号及数据源
      hasKeyField: [], //有字典数据源的字段
      keyValueType: { _dinit: false },
      url: "", //界面表查询的数据源的url
      hasDetail: false, //是否有从表(明细)表格数据
      initActivated: false,
      load: true, //是否默认加载表数据
      activatedLoad: false, //页面触发actived时是否刷新页面数据
      summary: false, //查询界面table是否显示合计
      //需要从远程绑定数据源的字典编号,如果字典数据源的查询结果较多，请在onInit中将字典编号添加进来
      //只对自定sql有效
      remoteKeys: [],
      columnIndex: false, //2020.11.01是否显示行号
      ck: true, //2020.11.01是否显示checkbox
      continueAdd: false, //2021.04.11新建时是否可以连续新建操作
      continueAddName: "保存后继续添加", //2021.04.11按钮名称
      // detailUrl: "",
      detailOptions: {
        paginationHide: false, //明细表隐藏分页
        dragPosition: "", //明细表格可拖动位置，顶部拖动top,底部bottom
        //弹出框从表(明细)对象
        //从表配置
        buttons: [], //弹出框从表表格操作按钮,目前有删除行，添加行，刷新操作，如需要其他操作按钮，可在表对应的.js中添加
        cnName: "", //从表名称
        key: "", //从表主键名
        data: [], //数据源
        columns: [], //从表列信息
        edit: true, //明细是否可以编辑
        single: false, //明细表是否单选
        load: false, //
        delKeys: [], //当编辑时删除当前明细的行主键值
        url: "", //从表加载数据的url
        pagination: { total: 0, size: 100, sortName: "" }, //从表分页配置数据
        height: 0, //默认从表高度
        textInline: true, //明细表行内容显示在一行上，如果需要换行显示，请设置为false
        doubleEdit: true, //使用双击编辑
        clickEdit: false, //是否开启点击单元格编辑，点击其他行时结束编辑
        currentReadonly: false, //当前用户没有编辑或新建权限时，表单只读(可用于判断用户是否有编辑或新建权限)
        //开启编辑时
        beginEdit: (row, column, index) => {
          return true;
        },
        //结束编辑前
        endEditBefore: (row, column, index) => {
          return true;
        },
        //结束编辑后
        endEditAfter: (row, column, index) => {
          return true;
        },
        columnIndex: false, //2020.11.01明细是否显示行号
        ck: true, //2020.11.01明细是否显示checkbox
        sortable: false, //表格是否可以拖拽排序2024.10.06
      },
      auditParam: {
        //审核对象
        rows: 0, //当前选中审核的行数
        model: false, //审核弹出框
        value: -1, //审核结果
        status: -1,
        reason: "", //审核原因
        height: 500,
        showViewButton: true,
        auditHis: [],
        showAction: false, //是否显示审批操作(当前节点为用户审批时显示)
        //审核选项(可自行再添加)
        data: [
          { text: "通过", value: 1 },
          { text: "拒绝", value: 2 },
          { text: "驳回", value: 3 },
        ],
      },
      upload: {
        //导入上传excel对象
        excel: false, //导入的弹出框是否显示
        url: "", //导入的路径,如果没有值，则不渲染导入功能
        template: {
          //下载模板对象
          url: "", //下载模板路径
          fileName: "", //模板下载的中文名
        },
        init: false, //是否有导入权限，有才渲染导入组件
      },
      height: 0, //表高度
      tableHeight: 0, //查询页面table的高度
      tableMaxHeight: 0, //查询页面table的最大高度
      textInline: true, //table内容超出后是否不换行2020.01.16
      pagination: { total: 0, size: 30, sortName: "" }, //从分页配置数据
      boxOptions: {
        title: "", //弹出框显示的标题2022.08.01
        saveClose: true,
        labelWidth: 100,
        height: 0,
        width: 0,
        summary: false, //弹出框明细table是否显示合计
        draggable: false, //2022.09.12弹出框拖动功能
        modal: true, //2022.09.12弹出框背景遮罩层
      }, //saveClose新建或编辑成功后是否关闭弹出框//弹出框的标签宽度labelWidth
      editor: {
        uploadImgUrl: "", //上传路径
        upload: null, //上传方法
      },
      numberFields: [],
      //2022.09.26增加自定义导出文件名
      downloadFileName: null,
      select2Count: 1500, //超出500数量显示select2组件
      newTabEdit: false, //新窗口编辑
      isMultiple: false, //是否多明细表
      detailHeight: 300, //明细表的高度
      hiddenFields: [],
      text: "",
      subDetails: [], //三级明细表
      lazy: true, //树形表格是否默认延迟加载
      defaultExpandAll: false, //树形表格是否展开所有
      expandRowKeys: [], //默认展开的节点
      paginationHide: false, //是否隐藏分页
      rowParentField: "", //树形表格父级id
      importDesc: "", //导入excel弹出框的描述
      multiple: {
        horizontal: false, //一对多水平显示(二级表与三级表是否左右结构显示)
        leftWidth: 0, //左边二级表宽度(默认不用设置)
        rightWidth: 0, //右边三级表宽度(默认不用设置)
      },
      dragPosition: "", //表格可拖动位置，顶部拖动top,底部bottom
      labelPosition: "", //编辑表单标签文字显示位置:left / top（默认是top，或者在main.js全局配置）
      queryFields: [], //快捷查询字段2024.01.18增加多个快捷查询字段
      isCopyClick: false, //当前是否点击的复制行操作
      padding: false, //是否使用padding间距
      gridRender: {
        h: (h, {}) => {
          return "";
        },
        data: {},
      },
      submitChangeRows: true, //只提交变更的明细表数据2024.08.30
      reserveSelection: false, //分页或者刷新表格数据后是否保留复选框选择状态，2024.09.10
      sortable: false, //表格是否可以拖拽排序2024.10.06
      extraHeight: 0,
      showTabs: true, //二级表是否以为tabs显示，为false时会从上住下顺序显示
    };
  },
  methods: {
    initGridExtension() {
      if (!this.$grid) {
        return;
      }

      Object.keys(this.$grid).forEach((key) => {
        const fn = this.$grid[key];
        if (typeof fn == "function") {
          fn.call(this);
        }
      });
    },
  },
  activated() {
    this.initFlowQuery();
    //2020.06.25增加activated方法
    this.onActivated && this.onActivated();
    if (!this._inited) {
      this._inited = true;
      return;
    }
    if (this.activatedLoad) {
      this.refresh();
    }
  },
  mounted() {
    this.mounted();
    // this.$refs.searchForm.forEach()
  },
  unmounted() {
    this.destroyed();
  },
  created: function () {
    console.log("created");
    if (this.$global.fixedSearch) {
      this.maxBtnLength = 12;
      // this.fixedSearchForm=true;
      this.setFixedSearchForm(true);
    }
    //合并自定义业务扩展方法
    Object.assign(this, this.extend.methods);
    //如果没有指定排序字段，则用主键作为默认排序字段
    this.pagination.sortName = this.table.sortName || this.table.key;
    this.newTabEdit = this.table.newTabEdit;

    this.initBoxButtons(); //初始化弹出框与明细表格按钮
    this.initAuditColumn();
    this.initEditTable();
    this.onInit(); //初始化前，如果需要做其他处理在扩展方法中覆盖此方法
    this.getButtons();
    //初始化自定义表格列
    this.initViewColumns();
    //初始编辑框等数据
    this.initBoxHeightWidth();
    this.initDicKeys(); //初始下框数据源
    this.onInited(); //初始化后，如果需要做其他处理在扩展方法中覆盖此方法

    this.initGridExtension();
  },
  beforeUpdate: function () {},
  updated: function () {},
};

import props from "./props.js";
import methods from "./methods.jsx";

//合并属性
vueParam.props = Object.assign(vueParam.props, props());
//合并方法
vueParam.methods = Object.assign(vueParam.methods, methods, props().extend.methods);
export default defineComponent(vueParam);
</script>
<style lang="less" scoped>
@import "./ViewGrid.less";
</style>
<style lang="less" scoped>
.btn-group ::v-deep(.el-dropdown .el-button:focus-visible) {
  outline: 0px !important;
  outline-offset: 1px;
}
.vertical-center-modal ::v-deep(.srcoll-content) {
  padding: 0;
}

.view-model-content {
  background: #eee;
}
</style>
<style lang="less" scoped>
// .grid-search {
//   position: relative;

//   .search-box {
//     background: #fefefe;
//     margin-top: 33px;
//     border: 1px solid #eae8e8;
//     position: absolute;
//     z-index: 999;
//     left: 15px;
//     right: 15px;
//     padding: 25px 20px;
//     padding-bottom: 0;
//     border-top: 0;
//     box-shadow: 0 7px 18px -12px #bdc0bb;
//   }
// }
.form-item ::v-deep(.form-tabs) {
  margin-top: -10px;
}
.search-line ::v-deep(.vol-form-item) {
  margin-top: 4px !important;
}
</style>
