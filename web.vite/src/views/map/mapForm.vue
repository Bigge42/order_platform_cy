<template>
    <div class="map-form">
        <div class="map-container">
            <div class="left-tree">

                <div class="title">
                    <div class="border">
                    </div>
                    <div class="text">已选地图
                    <div class="small-text">点击右侧地图可选点或输入直接搜索.....</div>
                    </div>
                </div>
                <VolForm :label-width="60" ref="form" label-position="left" :loadKey="true" :formFields="formFields"
                    :formRules="formRules">
                </VolForm>
                <div style="text-align: center; width: 100%">
                    <el-button type="primary" plain @click="getForm">获取表单</el-button>
                    <el-button type="success" plain @click="reset">重置表单</el-button>
                </div>
            </div>
            <div :id="id" ref="map" class="map"></div>
            <div class="search-input">
                <div>鼠标在地图上单击获取坐标</div>
                <div>
                    坐标x:<input type="text" v-model="lng" />
                    <span style="padding-left: 17px">
                        坐标y：<input type="text" v-model="lat" />
                    </span>
                </div>
                <div>城 市：{{ detailAddress }}</div>
                <div style="position: relative;">
                    <labe>搜 索:</labe> <input placeholder="请输入关键字" v-model="searchText" />
                    <div class="search-list" v-show="list.length">
                        <div @click="itemClick(item)" v-for="(item, index) in list" :key="index">
                            {{ item.district }}
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</template>
<script>
import AMapLoader from "@amap/amap-jsapi-loader";
import VolForm from "@/components/basic/VolForm.vue";
export default {
    components: {
        VolForm
    },
    data() {
        return {
            searchText: "",
            list: [],
            autoComplete: "",
            geocoder: null,
            map: null,
            marker: null,
            markerList: [],
            id: Math.random(),
            detailAddress: "",
            lng: 116.245202,
            lat: 39.736968,
            formFields: {
                country: "",
                province: "",
                district: "",
                lng: "",
                lat: "",
                township: "",
                formattedAddress: "",
            },
            formRules: [
                [{ type: "text", title: "国家", readonly: true, field: "country" }],
                [{ type: "text", title: "省份", readonly: true, field: "province" }],
                [{ type: "text", title: "市县", readonly: true, field: "district" }],
                [{ type: "text", title: "城镇", readonly: true, field: "township" }],
                [{ type: "text", title: "经度", readonly: true, field: "lng" }],
                [{ type: "text", title: "纬度", readonly: true, field: "lat" }],
                [{ type: "textarea", title: "地址", readonly: true, field: "formattedAddress" }],
            ]
        }
    },
    watch: {
        searchText(newText, oldText) {
            if (!newText) {
                this.list = [];
                return;
            }
            this.autoComplete.search(newText, (status, result) => {
                if (typeof result == 'string') {
                    this.list = [{ district: result }];
                    return;
                }
                //  console.log(result)
                this.list = result;
            });
        }
    },
    //地图示例：https://lbs.amap.com/demo/jsapi-v2/example/map-lifecycle/map-show
    mounted() {
        //申请key与密钥：
        //点添加key，选择Web端(JS API)
        //https://console.amap.com/dev/key/app
        window._AMapSecurityConfig = {
            securityJsCode: 'da3f57a26fd79f4eacf2cf9c2d3b6d60' //申请的【安全密钥】
        };
        AMapLoader.load({
            key: 'f732063d2f6ecfa336f6cb9c63a045fb',//申请的[Key  ]
            version: '1.4.15',// "1.4.15", // 指定要加载的 JSAPI 的版本，缺省时默认为 1.4.15
            plugins: [], // 需要使用的的插件列表，如比例尺'AMap.Scale'等
            AMapUI: {
                // 是否加载 AMapUI，缺省不加载
                version: "1.1", // AMapUI 缺省 1.1
                plugins: [], // 需要加载的 AMapUI ui插件
            },
            Loca: {
                // 是否加载 Loca， 缺省不加载
                version: "1.3.2", // Loca 版本，缺省 1.3.2
            },
        })
            .then((AMap) => {
                this.$nextTick(() => this.initMap(AMap));
            })
            .catch((e) => {
                console.error(e);
            });


    },
    methods: {
        getForm() {
            this.$refs.form.validate(() => {
                this.$message.error(JSON.stringify(this.formFields));
            });
        },
        reset() {
            this.$refs.form.reset();
            this.$message.error("表单已重置");
        },
        initMap(AMap) {

            this.map = new AMap.Map(this.$refs.map, {
                zoom: 8,
                center: [this.lng, this.lat]
            });

            //AMap.Autocomplete这里版本不同可能是AMap.AutoComplete
            AMap.plugin(["AMap.Geolocation", "AMap.Driving", 'AMap.PlaceSearch', 'AMap.Autocomplete', 'AMap.Geocoder'], () => { });


            this.autoComplete = new AMap.Autocomplete({});

            this.geocoder = new AMap.Geocoder({
                city: '010', //城市设为北京，默认：“全国”
                radius: 100 //范围，默认：500
            });

            this.marker = new AMap.Marker();
            //地图点击事件
            this.map.on('click', (e) => {
                this.lng = e.lnglat.lng;
                this.lat = e.lnglat.lat;
                this.regeoCode(true);
            });
            if (this.lng) {
                this.regeoCode(false);
            }
        },
        regeoCode(isClick) {
            if (!this.lng) {
                return;
            }
            this.map.add(this.marker);
            let lnglat = [this.lng, this.lat];
            this.marker.setPosition(lnglat);
            this.geocoder.getAddress(lnglat, (status, result) => {
                if (status === 'complete' && result.regeocode) {
                    this.detailAddress = result.regeocode.formattedAddress;
                    Object.assign(this.formFields,
                        result.regeocode.addressComponent,
                        {
                            lng: this.lng,
                            lat: this.lat,
                            formattedAddress: result.regeocode.formattedAddress
                        })
                    this.setCenter(isClick);
                } else {
                    console.log('根据经纬度查询地址失败' + JSON.stringify(result));
                    this.$message.error('定位异常：' + JSON.stringify(result))
                }
            });
        },
        setLanglat(lng, lat, address) {
            if (!lng) {
                return;
            }
            this.lng = lng;
            this.lat = lat;
            this.regeoCode(false);
        },
        setCenter(isClick) {
            if (!isClick) {
                this.map.setCenter([this.lng, this.lat]); //设置地图中心点
            }
        }
    },
};
</script>
  
<style lang="less" scoped>
.map-form {
    display: flex;
    align-items: center;
    background: #f3f7fb;
    width: 100%;
    height: 100%;
    position: absolute;
    top: 0;
    right: 0;
    bottom: 0;
}

.map-container {
    border: 1px solid #ededed;
    width: 1000px;
    // height: 100%;
    height: 450px;
    position: relative;
    left: 0;
    right: 0;
    margin: 0 auto;
    display: flex;
}

.map {
    flex: 1;
    height: 100%;
    padding: 10px;
    background: #ffff;
}

.left-tree {
    background: #fff;
    height: 100%;
    width: 400px;
    padding-right: 10px;
    display: flex;
    flex-direction: column;
    border-right: 1px solid #eee;
}

input {
    border: 1px solid #ddd;
    font-size: 12px;
    padding: 4px;
    border-radius: 3px;
    outline: none;
    width: 100px;
}

.search-input {
    display: flex;
    flex-direction: column;
    z-index: 9999;
    top: 0;
    font-size: 12px;
    position: absolute;
    right: 0;
    margin: 8px;
    background: #f9f9f9d4;
    padding: 10px;
    border: 1px solid #cacaca;
    border-radius: 4px;

    >div {
        margin-bottom: 6px;
    }
}

.search-list {
    position: absolute;
    background: #fff;
    font-size: 11px;
    min-height: 100px;
    left: 36px;
    top: 26px;
    border-radius: 4px;
    border: 1px solid #dadada;
    box-shadow: 5px 5px 6px #c5c3c38f;

    div {
        padding: 6px 10px;
        font-size: 12px;
    }

    div:hover {
        background: #ececec;
        cursor: pointer;
    }
}

.title {
    display: flex;
    margin-bottom: -4px;
    line-height: 20px;
    margin-top: 5px;
    padding-bottom: 5px;
    padding: 10px;
    border-bottom: 0;
    .small-text{
        font-size: 12px;
        color: #808080;
        font-weight: 500;
    }

    .border {
        height: 15px;
        background: rgb(33, 150, 243);
        width: 5px;
        border-radius: 10px;
        top: 2px;
        position: relative;
    }

    .text {
        padding-left: 6px;
        font-weight: bold;
        font-size: 15px;
    }
}
</style>

