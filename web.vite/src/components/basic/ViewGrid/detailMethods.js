//从表方法
let detailMethods = {
  //查询从表前先做内部处理
  loadInternalDetailTableBefore(param, callBack, table, item) {
    //加载明细表数据之前,需要设定查询的主表的ID
    //每次只要加载明细表格数据就重置删除明细的值
    if (this.detailOptions.delKeys.length > 0) {
      this.detailOptions.delKeys = [];
    }
    let key = this.table.key;
    if (this.currentRow && this.currentRow.hasOwnProperty(key)) {
      param.value = this.currentRow[key];
    }
    if (this.isMultiple) {
      if (!param.tableName) {
        (param.value = this.currentRow[this.table.key]),
          (param.tableName = table);
      }
    }
    return this.loadDetailTableBefore(param, callBack, table, item);
  },
  loadDetailTableBefore(param, callBack, table, item) {
    //明细查询前
    //新建时禁止加载明细
    if (this.currentAction == this.const.ADD) {
      callBack(false);
      return false;
    }
    let status = this.searchDetailBefore(param, table, item);
    callBack(status);
  },
  loadDetailTableAfter(data, callBack, table, item) {
    //明细查询后
    let status = this.searchDetailAfter(data, table, item);
    //2024.05.23增加复制数据功能后清空明细表的主键值
    if (this.currentAction == 'Add') {
      data.forEach(row => {
        row[this.table.key] = undefined;
        if (!item) {
          row[this.detailOptions.key] = undefined;
        } else {
          row[item.key] = undefined;
        }
      })
    }
    callBack(status);
  },

  //三级明细查询前2023.09.17
  loadSubInternalDetailTableBefore(param, callBack, table, item) {
    //获取二级明细表配置
    if (item) {
      param.table = item.table;
      param.detailTable = item.secondTable;
      param.tableName = item.table;
      let detailRows = this.getTable(item.secondTable).getSelected();
      if (!detailRows || detailRows.length == 0) {
        callBack(false);
        return;
      }
      param.value = detailRows[0][item.secondKey];
    }
    let rows;
    if (!param.value) {
      //获取三级表当前选中的明细行数据,并且设置二级明细表的id作为查询条件
      let secondKey = item.secondKey;
      //获取二级明细表选中的行
      // rows = this.getTableRef(item.secondTable).getSelected();
      rows = this.getCurrentDetailSelectRows(item.secondTable);
      if (!rows) {
        callBack(false);
        return;
      }
      param.value = rows[0][secondKey];
    }

    //三级明细查询前
    //新建时禁止加载明细
    if (this.currentAction == this.const.ADD) {
      callBack(false);
      return false;
    }
    if (!this.searchSubDetailBefore) {
      callBack(true);
      return;
    }
    let status = this.searchSubDetailBefore(param, table, item);
    callBack(status);
  },
  getCurrentDetailSelectRows(table) {
    let rows = this.getTableRef(table).getSelected();
    if (!rows.length) {
      this.$message.error(
        this.$ts('请选中明细表数据') +
        ':【' +
        this.$ts(this.getTableName(table)) +
        '】'
      );
      return null;
    }
    return rows;
  },
  //获取二级或者三级明细表ref对象
  getTableRef(table) {
    //获取明细表
    if (this.details.length) {
      //获取一对多的table表格
      let _obj =
        this.$refs.details.$refs[table] ||
        (this.$refs.subDetails && this.$refs.subDetails.$refs[table]);
      if (_obj) {
        return _obj[0];
      }
    } else {
      return this.$refs.detail;
    }
  },
  //获取二级或者三级明细表中文名称
  getTableName(table) {
    //获取明细表
    if (!this.details.length) {
      return '未配置二级或三级明细表';
    }

    let ops = this.details.find((x) => {
      return x.table == table;
    });
    if (ops) {
      return ops.cnName;
    }

    return this.subDetails.find((x) => {
      return x.table == table;
    }).cnName;
  },
  //三级明细查询后2023.09.17
  loadSubDetailTableAfter(rows, callBack, table, item) {
    item.keys = [];
    //给二级明细表设置值
    let row = this.getTableRef(item.secondTable).getSelected()[0];
    row[item.table] = rows;

    if (!this.searchSubDetailAfter) {
      callBack(true);
      return;
    }
    //三级明细查询后
    let status = this.searchSubDetailAfter(rows, table, item);
    callBack(status);
  },
  // detailRowOnChange(row,table) {
  //   this.detailRowChange(row);
  // },
  detailRowChange(row, table) {
    // if (this.currentAction == 'Add') {
    //   return;
    // }
    // //二级明细表点击选中行事件
    // if (
    //   this.subDetails.length &&
    //   this.subDetails.some((x) => {
    //     return x.secondTable == table;
    //   })
    // ) {
    //   if (row[table]) {
    //     return;
    //   }
    // }
    //checkbox选中行事件
  },
  detailRowOnClick({ row, column, event, item }) {
    //明细表点击行事件2020.11.07
    this.detailRowClick({ row, column, event, item });
  },
  detailRowClick({ row, column, event, item }) { },
  resetDetailTable(row, isAdd) {
    //重置三级明细
    if (this.subDetails.length) {
      this.subDetails.forEach((x) => {
        x.delKeys = [];
        let tableRef = this.getTableRef(x.table);
        tableRef && tableRef.reset();
      });
    }
    //新建或编辑时重置明细表
    //一对多
    if (this.isMultiple) {
      this.$nextTick(() => {
        if (this.$refs.details) {
          this.details.forEach((item) => {
            if (item.columns) {
              let refTable = this.getTable(item.table);
              refTable && refTable.reset();
              if (refTable && !isAdd) {
                // refTable.load();
                this.detailTableLoad(row, refTable, item.table);
              }
            }
          });
        }
      });
      return;
    }

    //编辑和查看明细时重置从表数据
    if (!this.detailOptions.columns || this.detailOptions.columns.length == 0) {
      return;
    }
    //let key = this.table.key;
    // let query = { value: row ? row[key] : this.currentRow[key] };
    this.$nextTick(() => {
      if (this.$refs.detail) {
        this.$refs.detail.reset();
        //this.$refs.detail.load(query);
        this.detailTableLoad(row, this.$refs.detail);
      }
    });
  },
  detailTableLoad(row, refTable, table) {
    //一对多明细表加载数据
    if (refTable) {
      let query = {
        value: row ? row[this.table.key] : this.currentRow[this.table.key],
        tableName: table
      };
      refTable.load(query);
    }
  },
  //从后面加载从表数据
  refreshRow() {
    this.resetDetailTable();
  },
  //二级明细表添加行
  addSecondRow(table, item, index) {
    let _row = this.addDetailRow(table, item, index);
    if (item.detail && item.detail.table) {
      _row[item.detail.table] = [];
    }
    this.getTable(table).addRow(_row);
  },
  //获取二级明细表默认数据
  addDetailRow(table, item, index) {
    return {};
  },
  //二级明细选中行事件
  detailRowOnChange(row, item) {
    //获取三级明细子表
    if (!item || !item.detail) {
      return;
    }
    this.detailRowChange(row, item);
    let table = item.detail.table;
    //三级表对象
    let subRef = this.getTableRef(table);
    if (!subRef) {
      return;
    }

    //三级明细表数据
    let subRows = row[table];
    if (subRows) {
      subRef.rowData = subRows;
      return;
    }
    if (!subRows) {
      subRows = [];
      row[table] = subRows;
    }
    subRef.rowData = subRows;
    //处于编辑状态时没有明细表数据，从后台加载数据
    if (this.currentAction != 'Add' && !subRows.length) {
      //从接口添加数据
      subRef.load(null, true);
    }
  },
  //三级明细表添加行
  addSubRow(table, item, index, newRows) {
    let rows = this.getCurrentDetailSelectRows(item.secondTable);
    if (!rows) {
      return;
    }
    if (newRows) {
      if (!Array.isArray(newRows)) {
        newRows = [newRows];
      }
    } else {
      //默认默认的添加行数据
      newRows = [this.addDetailRow(table, item, index)];
    }
    //给二级明细添加表数据
    if (!rows[0][table]) {
      rows[0][table] = newRows;
    } else {
      rows[0][table].push(...newRows);
    }
    //三级明细表数据指向二级明细表行数据
    this.getTableRef(item.table || table).rowData = rows[0][table];
  },
  addRow() {
    //单表明细表添加行
    this.$refs.detail.addRow({});
    this.$refs.detail.edit.rowIndex = -1;
    this.updateDetailTableSummaryTotal();
  },
  getTable(table) {
    // //获取一对多的table表格
    // let _obj = this.$refs.details.$refs[table]; //[0];
    // if (_obj) {
    //   return _obj[0];
    // }
    return this.getTableRef(table);
  },
  delSubRow(table, item, index) {
    this.delRow(table, item, index, true);
  },
  delRowAfter(rows, table, item, index) {
    return true;
  },
  delRow(table, item, index, isSubDetail) {
    //一对多明细的添加行

    let rows;
    if (isSubDetail) {
      rows = this.getTable(table).getSelected();
    } else if (typeof table == 'string' && this.isMultiple) {
      rows = this.getTable(table).getSelected();
    } else {
      rows = this.$refs.detail.getSelected();
    }

    if (!rows || rows.length == 0) {
      return this.$message.error(this.$ts('请选择要删除的行!'));
    }
    if (!this.delDetailRow(rows, table)) {
      return false;
    }
    if (this.delRowBefore && !this.delRowBefore(rows, table, item, index)) {
      return;
    }
    let tigger = false;
    this.$confirm(this.$ts('确认要删除选择的数据吗?'), this.$ts('警告'), {
      confirmButtonText: this.$ts('确定'),
      cancelButtonText: this.$ts('取消'),
      type: 'warning',
      center: true
    }).then(() => {
      if (tigger) return;
      tigger = true;
      //二、三级删除行
      if (this.isMultiple || isSubDetail) {
        let refDetail = this.getTable(table);
        refDetail.delRow();
        //记录删除的明细
        rows.forEach((x) => {
          if (x.hasOwnProperty(item.key) && x[item.key]) {
            item.delKeys.push(x[item.key]);
          }
        });
        if (!this.delRowAfter(rows, table, item, index)) {
          return;
        }
        return;
      }
      rows = this.$refs.detail.delRow();
      if (!this.delRowAfter(rows, table, item, index)) {
        return;
      }
      let key = this.detailOptions.key;
      //记录删除的行数据
      rows.forEach((x) => {
        if (x.hasOwnProperty(key) && x[key]) {
          this.detailOptions.delKeys.push(x[key]);
        }
      });
      this.updateDetailTableSummaryTotal();
    });
  },
  updateDetailTableSummaryTotal(table) {
    //2021.09.25增加明细表删除、修改时重新计算行数与汇总
    //2021.12.12增加明细表判断(强制刷新合计时会用到)
    if (this.isMultiple) {
      this.details.forEach((c) => {
        if (!table || c.table === table) {
          let tableRef = this.getTable(c.table);
          tableRef.paginations.total = tableRef.rowData.length;
          //重新设置合计
          if (tableRef.summary) {
            tableRef.columns.forEach((column) => {
              if (column.summary) {
                tableRef.getInputSummaries(null, null, null, column);
              }
            });
          }
        }
      });
      return;
    }
    if (!this.$refs.detail) {
      return;
    }
    //删除或新增行时重新设置显示的总行数
    this.$refs.detail.paginations.total = this.$refs.detail.rowData.length;
    //重新设置合计
    if (this.$refs.detail.summary) {
      this.$refs.detail.columns.forEach((column) => {
        if (column.summary) {
          this.$refs.detail.getInputSummaries(null, null, null, column);
        }
      });
    }
  },
  detailSelectable(row, index) {
    //明细表CheckBox 是否可以勾选
    return true;
  },
  tabsClick(table) {
    let obj = this.details.find((x) => {
      return x.table == table;
    });
    this.detailTabsClick && this.detailTabsClick(table)
    //设置三级明细表选中
    if (obj) {
      if (this.$refs.subDetails) {
        if (obj.detail) {
          this.$refs.subDetails.setTable(obj.detail.table);
        } else {
          this.$refs.subDetails.setTable("");
        }
      }
      return;
    }

    obj = this.subDetails.find((x) => {
      return x.table == table;
    });
    //设置二级明细表选中
    if (obj) {
      this.$refs.details.setTable(obj.secondTable);
    }
  }
};

export default detailMethods;
