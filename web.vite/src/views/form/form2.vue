<template>
  <div class="container">
    <div class="content">
      <div class="header">
        <el-alert
          type="success"
          show-icon
          title="动态表单包括：实时计算、倒计时、区间、表单按钮、事件处理等各种自定义render熏染任何内容。。。"
        >
        </el-alert>
      </div>
      <VolForm ref="form" :loadKey="true" :formFields="formFields" :formRules="formRules">
        <div style="text-align: center; width: 100%; margin-top: 10px">
          <el-button type="primary" plain @click="getForm">获取表单</el-button>
          <el-button type="success" plain @click="reset">重置表单</el-button>
          <el-button type="info" plain @click="setReadonlyStaus(true)">全部只读</el-button>
          <el-button type="primary" plain @click="setReadonlyStaus(false)">取消只读</el-button>
        </div>
      </VolForm>
    </div>
  </div>
</template>
<script lang="jsx">
import VolForm from '@/components/basic/VolForm.vue'
import VolHeader from '@/components/basic/VolHeader.vue'
export default {
  props: {
    showBtn: { type: Boolean, default: true }
  },
  components: {
    VolForm,
    VolHeader
  },
  methods: {
    setReadonlyStaus(status) {
      this.formRules.forEach((rules) => {
        rules.forEach((option) => {
          option.readonly = status
        })
      })
    },
    getForm() {
      this.$refs.form.validate(() => {
        this.$message.error(JSON.stringify(this.formFields))
      })
    },
    reset() {
      //重置表单，重置时可指定重置的值，如果没有指定重置的值，默认全部清空
      let data = { decimalVal: 100, numberVal: 100 }
      this.$refs.form.reset(data)
      this.$message.error('表单已重置')
    },
    popover() {
      this.$message.success('点击了提示')
    }
  },
  data() {
    return {
      formFields: {
        qty: 10,
        price: 8,
        total: null,
        total2: null,
        total3: null,

        custom1: null,
        custom2: null,
        custom3: null,
        custom4: null
      },
      formRules: [
        //表单配置formRules数据格式是二维数组，表示每个标签所在位置:[[{},{}]]
        [
          {
            colSize: 12,
            render: (h) => {
              return (
                <div
                  style={{
                    display: 'flex',
                    'margin-bottom': '-4px',
                    'line-height': '20px',
                    'margin-top': '5px',
                    'padding-bottom': '5px',
                    'border-bottom': '1px solid  rgb(227 227 227)'
                  }}
                >
                  <div style="height: 15px;background: #2196F3;width: 5px;border-radius: 10px;top: 2px;position: relative;"></div>
                  <div style="padding-left: 6px; font-weight: bold; font-size: 15px;">
                    实时计算,只需要简单几行代码实现render表单数量*金额输入自动计算
                  </div>
                </div>
              )
            }
          }
        ],
        [
          {
            type: 'decimal',
            title: '数量',
            required: true,
            placeholder: '输入框。。。',
            field: 'qty',
            onKeyPress: () => {
              //给实时计算3字段设置值
              this.formFields.total3 = (this.formFields.qty || 0) * (this.formFields.price || 0)
            }
          },
          {
            type: 'decimal',
            title: '单价',
            required: true,
            field: 'price',
            onKeyPress: () => {
              //给实时计算3字段设置值
              this.formFields.total3 = (this.formFields.qty || 0) * (this.formFields.price || 0)
            }
          },
          {
            type: 'decimal',
            title: '总价(实时计算)',
            field: 'total',
            render: (h, {}) => {
              return (
                <el-input
                  disabled
                  value={(this.formFields.qty || 0) * (this.formFields.price || 0)}
                />
              )
            }
          },
          {
            type: 'decimal',
            title: '总价(实时计算2)',
            field: 'total2',
            render: (h, {}) => {
              return (
                <div style="display:flex;align-items: center;">
                  <div style="color:rgb(104 98 98)">
                    {(this.formFields.qty || 0) * (this.formFields.price || 0)}
                  </div>
                  <el-popover
                    placement="top-start"
                    title="提示"
                    width="200"
                    trigger="hover"
                    content="输入框自计算"
                  >
                    {{
                      reference: (() => {
                        return h(
                          <i
                            style="color:rgb(6 118 169);font-size:12px;margin-left:15px"
                            onClick={() => {
                              this.$message.success('提示信息')
                            }}
                            class="el-icon-warning-outline"
                          ></i>
                        )
                      })
                    }}
                  </el-popover>
                </div>
              )
            }
          },
          {
            type: 'text',
            title: '总价(实时计算3)',
            field: 'total3',
            readonly: true
          }
        ],
        [
          {
            colSize: 12,
            render: (h) => {
              return (
                <div
                  style={{
                    display: 'flex',
                    'margin-bottom': '-4px',
                    'line-height': '20px',
                    'margin-top': '5px',
                    'padding-bottom': '5px',
                    'border-bottom': '1px solid  rgb(227 227 227)'
                  }}
                >
                  <div style="height: 15px;background: #2196F3;width: 5px;border-radius: 10px;top: 2px;position: relative;"></div>
                  <div style="padding-left: 6px; font-weight: bold; font-size: 15px;">
                    自定义显示
                  </div>
                </div>
              )
            }
          }
        ],
        [
          {
            title: '自定义额外属性',
            // required: true,
            field: 'custom1',
            type: 'text',
            btnValue: '发送短信',
            extra: {
              render: (h, {}) => {
                let countdown = 10
                let item
                this.formRules.forEach((c) => {
                  c.forEach((x) => {
                    if (x.field == 'custom1') {
                      item = x
                    }
                  })
                })
                return (
                  // <el-input disabled value={item.btnValue} />
                  <div style="display:flex;height: 100%;">
                    <el-button
                      type="primary"
                      link
                      onClick={() => {
                        this.$message.success('点击了按钮')
                      }}
                    >
                      <i class="el-icon-search">选择</i>
                    </el-button>
                    <el-button
                      type="primary"
                      style="margin-left:0"
                      link
                      onClick={() => {
                        item.btnValue = countdown + '(秒)'
                        countdown--
                        //设置倒计时
                        var timer = setInterval(function () {
                          if (countdown > 0) {
                            item.btnValue = countdown + '(秒)'
                            countdown--
                          } else {
                            //给倒计时按钮设置值
                            item.btnValue = '发送短信'
                            countdown = 10
                            clearInterval(timer)
                          }
                        }, 1000)
                      }}
                    >
                      <i class="el-icon-message">{item.btnValue}</i>
                    </el-button>
                  </div>
                )
              }
            }
          },
          {
            title: '自定义提示',
            // required: true,
            field: 'custom2',
            type: 'text',
            extra: {
              render: (h, {}) => {
                return (
                  <el-popover
                    placement="top-start"
                    title="提示"
                    width="200"
                    trigger="hover"
                    content="还没想好"
                  >
                    {{
                      reference: (() => {
                        return h(
                          <i
                            style="color:rgb(6 118 169);font-size:12px;"
                            onClick={() => {
                              this.$message.success('提示信息')
                            }}
                            class="el-icon-warning-outline"
                          >
                            提示
                          </i>
                        )
                      })
                    }}
                  </el-popover>
                )
              }
            }
          },
          {
            title: '自定义按钮',
            // required: true,
            field: 'custom3',
            type: 'text',
            extra: {
              render: (h, {}) => {
                return (
                  <el-button
                    onClick={() => {
                      this.$message.success('click')
                    }}
                  >
                    {
                      <i
                        onClick={() => {
                          this.$message.success('查询')
                        }}
                        class="el-icon-search"
                      >
                        提示
                      </i>
                    }
                  </el-button>
                )
              }
            }
          },
          {
            title: '拾色器',
            // required: true,
            field: 'custom4',
            type: 'text',
            readonly: true,
            extra: {
              render: (h, {}) => {
                return (
                  <el-color-picker
                    onChange={(value) => {
                      this.formFields.custom4 = value
                    }}
                    value={this.formFields.custom4}
                  />
                )
              }
            }
          }
        ]
      ]
    }
  }
}
</script>
<style lang="less" scoped>
.container {
  padding: 10px;
  position: absolute;
  width: 100%;
  height: 100%;
  background: #f3f7fc;

  .content {
    border-radius: 5px;
    padding: 10px 0;
    height: 100%;
    background: #fff;
  }

  .header {
    margin: 0 13px 13px 13px;
  }
}
</style>
