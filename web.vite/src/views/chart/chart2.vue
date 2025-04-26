<template>
    <div class="home-container">
        <el-scrollbar style="height: 100%;">
            <!--下面订单部分-->
            <div class="h-top margin-item">
                <div class="item" :class="'item' + (index + 1)" v-for="(item, index) in topData" :key="index"
                    @click="centerClick(item)" :style="{ background: item.bg }">
                    <div class="h-top-item-top">
                        <div class="left">{{ item.samllTextLeft }}</div>
                        <div class="right" :style="{ background: item.samllTextRightBg }">
                            {{ item.samllTextRight }}
                        </div>
                    </div>
                    <div class="h-top-item-bottom">
                        {{ item.name }}
                    </div>
                </div>
            </div>
            <!--中件图表-->
            <div class="h-chart margin-item">
                <div id="chart1" class="h-chart-left"></div>
                <div id="chart2" class="h-chart-right"></div>
            </div>

            <!---->
            <div class="h-bottom margin-item">
                <div class="title">处理事项</div>
                <div class="grid-list">
                    <div class="item" :class="'item' + (index + 1)" v-for="(item, index) in topData" :key="index">
                        <div class="h-bottom-txt">
                            {{ item.name }}
                        </div>
                        <div class="h-bottom-qty">
                            {{ (item.qty + '').replace(/\B(?=(\d{3})+(?!\d))/g, ",") }}
                        </div>
                    </div>
                </div>
            </div>
        </el-scrollbar>
    </div>
</template>
<script>
import * as echarts from 'echarts';

import options from './chart2Ops.js';
import { ref, onMounted, onUnmounted, getCurrentInstance } from 'vue';
var $chart1, $chart2;
export default {
    components: {},
    data() {
        return {

            topData: [
                {
                    name: '待处理事项',
                    bg: 'rgb(0,115,182)',
                    url: '',
                    samllTextLeft: '待办事项',
                    samllTextRight: '订单',
                    samllTextRightBg: 'rgb(22,186,153)',
                    path: '/newOrder',
                    qty: 1000
                },
                {
                    name: '已处理事项',
                    bg: 'rgb(48 148 209)',
                    url: '',
                    samllTextLeft: '已办事项',
                    samllTextRight: '订单',
                    samllTextRightBg: 'rgb(15 119 182)',
                    path: '/newOrder',
                    qty: 2200
                },
                {
                    name: '待回复消息',
                    bg: 'rgb(105,102,172)',
                    url: '',
                    samllTextLeft: '待办事项',
                    samllTextRight: '消息',
                    samllTextRightBg: 'rgb(44,60,82)',
                    path: '/newOrder',
                    qty: 2800
                },
                {
                    name: '已回复消息',
                    bg: 'rgb(25,190,160)',
                    url: '',
                    samllTextLeft: '待办事项',
                    samllTextRight: '工具',
                    samllTextRightBg: 'rgb(44,60,82)',
                    qty: 1500
                },
                {
                    name: '待审批事项',
                    bg: 'rgb(48 148 209)',
                    url: '',
                    samllTextLeft: '待办事项',
                    samllTextRight: '订单',
                    samllTextRightBg: 'rgb(15 119 182)',
                    path: '/newOrder',
                    qty: 1800
                },
                {
                    name: '已审批事项',
                    bg: 'rgb(105,102,172)',
                    url: '',
                    samllTextLeft: '待办事项',
                    samllTextRight: '消息',
                    samllTextRightBg: 'rgb(44,60,82)',
                    path: '/newOrder',
                    qty: 1200
                }
            ],
        };
    },
    setup() {
        const { proxy } = getCurrentInstance();
        onMounted(() => {
            let dateArr = new Array(10).fill(0).map((x, i) => {
                let date = proxy.base.getDate();
                return proxy.base.addDays(date, i * -1)
            })
            options.xAxis.data = dateArr;

            $chart1 = echarts.init(document.getElementById('chart1'));
            $chart1.setOption(options);

            // 第二个图
            $chart2 = echarts.init(document.getElementById('chart2'));
            options.series[0].type = "bar";
            options.series[0].barWidth = 4;
            options.series[1].type = "bar";
            options.series[1].barWidth = 4;
            options.grid.right=20;
            options.grid.left=40;
            options.xAxis.boundaryGap = true;
            options.xAxis.data=dateArr.map(x=>{return x.substr(5,10).replace('-','.')})
            //   console.log(JSON.stringify(options))
            $chart2.setOption(options);
        });
        onUnmounted(() => {
            $chart1 && $chart1.dispose();
            $chart2 && $chart2.dispose();
        });
        return {};
    },
    methods: {
        centerClick(item) {
            if (!item.path) {
                return;
            }
        },
        toOrder(item) {
        }
    },
    created() {

    },
    destroyed() {
        $chart = null;
    }
};
</script>
<style lang="less" scoped>
.home-container {
    position: absolute;
    height: 100%;
    width: 100%;

    background: #f3f7fb;

    .margin-item {
        margin: 20px;
    }

    .h-top-item {
        height: 100px;
        background: #ffff;
        display: flex;

        .item {
            background: #ffff;
            margin-right: 20px;
            cursor: pointer;
            flex: 1;
            background: #ffff;
            display: flex;

            .h-top-item-left {
                color: white;
                margin: 10px;
                width: 80px;
                height: 80px;
                text-align: center;
                padding-top: 20px;
                border-radius: 10px;
            }

            .h-top-item-right {
                padding-left: 5px;

                .number {
                    font-size: 35px;
                    padding: 17px 0 0px 0;
                }

                .name {
                    font-size: 14px;
                    color: #747474;
                    padding-left: 2px;
                }
            }
        }

        .item:last-child {
            margin-right: 0;
        }
    }

    .h3 {
        padding: 7px 20px;
        font-weight: 500;
        background: #fff;
        border-bottom: 1px dotted #d4d4d4;
    }
}

.h-top {
    height: 100px;
    // background: #ffff;
    display: flex;

    .item {
        border-radius: 5px;
        flex: 1;
        padding: 18px;
        margin-right: 20px;
        display: flex;
        flex-direction: column;
        justify-content: center;

        .h-top-item-top {
            display: flex;
            align-items: center;
            margin-bottom: 8px;

            .left {
                flex: 1;
                color: #cdcdcd;
                font-size: 13px;
            }

            .right {
                max-width: 60px;
                text-align: center;
                font-size: 12px;
                color: #cdcdcd;
                padding: 2px 6px;
                border-radius: 4px;
            }
        }

        .h-top-item-bottom {
            font-size: 14px;
            color: #ffff;
            letter-spacing: 2px;
        }
    }

    .item:hover {
        cursor: pointer;
    }

    .item:last-child {
        margin-right: 0;
    }
}


.h-chart {
    height: 300px;

    display: flex;

    .h-chart-left {
        margin-right: 20px;
        flex: 1;
        background: #ffff;
        padding: 20px;
    }

    .h-chart-right {
        padding: 20px;
        width: 420px;
        background: #ffff;
    }
}

.h-bottom {


    background: #ffff;


    .grid-list {
        padding:20px 20px 30px  20px;
        display: grid;
        -moz-column-gap: 12px;
        column-gap: 20px;
        grid-template-columns: repeat(6, auto);
    }
    .title{
        padding: 20px 0 0 20px;
        font-weight: bolder;
    }

    .h-bottom-txt {
        color: #a5a2a2;
        font-size: 13px;
    }

    .h-bottom-qty {
        margin-top: 10px;
        font-size: 19px;
        font-family: Source Han Sans CN, sans-serif;
        color: #505050;
        font-weight: bold;
        letter-spacing: 1px;
    }
}
</style>
  