<template>
    <div style="width: 100%; height: 100%" class="map-container">
        <div class="left-tree">
            <div class="tree-name">
                地区列表
            </div>
            <el-scrollbar style="flex: 1;"> <el-tree :data="treeData" @node-click="handleNodeClick" />
            </el-scrollbar>
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
</template>
<script>
import AMapLoader from "@amap/amap-jsapi-loader";
export default {

    data() {
        return {
            searchText: "",
            list: [],
            treeData: [],
            autoComplete: "",
            geocoder: null,
            map: null,
            marker: null,
            markerList: [],
            id: Math.random(),
            detailAddress: "",
            lng: 107.031461,
            lat: 27.935342
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
        handleNodeClick(item) {
            this.lat = item.lat;
            this.lng = item.lng;
            let lnglat = [this.lng, this.lat];
            this.marker.setPosition(lnglat);
            this.regeoCode(false);
        },
        itemClick(item) {

        },
        initMarker(list) {
            list = list.filter(x => { return x.level == 1 })
            for (let index = 0; index < list.length; index++) {
                const item = list[index];
                var markerContent =
                    '' +
                    '<div class="custom-content-marker"><div class="h-info"><div class="text">' +
                    item.name +
                    '</div><div class="value">' +
                    (~~(Math.random() * 1000)) +
                    '万</div></div></div>';

                var marker = new AMap.Marker({
                    id: item.id * 1,
                    position: [item.lng, item.lat],
                    content: markerContent,
                    offset: new AMap.Pixel(-13, -30),
                    index: index
                });
                marker.on('click', (e) => {
                    var item = list.find(x => { return x.id * 1 == e.target.De.id })
                    this.$message.success('点击了:' + item.name)
                    // this.map.setCenter(item.lng, item.lat);
                    // this.map.setZoom(13);
                    // this.map.remove(this.markerList);
                });
                this.markerList.push(marker);
                this.map.add(marker);
            }
        },
        initData() {
            this.http.get("api/Sys_Region/getMapList", {}, true).then(x => {
                let treeData = this.base.convertTree(x, (node, data, isRoot) => {
                    node.label = node.name;
                });
                this.treeData = treeData;
                this.initMarker(treeData);
            })
        },
        initMap(AMap) {

            this.map = new AMap.Map(this.$refs.map, {
                zoom: 5.5,
                center: [this.lng, this.lat]
            });

            //AMap.Autocomplete这里版本不同可能是AMap.AutoComplete
            AMap.plugin(["AMap.Geolocation", "AMap.Driving", 'AMap.PlaceSearch', 'AMap.Autocomplete', 'AMap.Geocoder'], () => { });


            try {
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

            } catch (error) {
               console.log(error)
            }
            this.initData();
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
.map-container {
    width: 100%;
    height: 100%;
    position: absolute;
    top: 0;
    right: 0;
    bottom: 0;
    z-index: 999;
    display: flex;
}

.map {
    flex: 1;
    height: 100%;
}

.left-tree {
    height: 100%;
    width: 200px;
    display: flex;
    flex-direction: column;
    border-right: 1px solid #eee;

    .tree-name {
        border: 1px solid #eee;
        padding: 10px;
        font-weight: bolder;
        background: #66b1ff0f;
        font-size: 14px;
    }

    ::v-deep(.el-scrollbar__thumb) {
        width: 0px !important;
        right: 0;
    }
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
</style>
<style>
.h-info {
    color: #fff;
    width: 80px;
    height: 80px;
    background: #03a9f4cc;
    border-radius: 50%;
    text-align: center;
    flex-direction: column;
    display: flex;
    align-items: center;
    justify-content: center;
    font-size: 13px;
}

.h-info .text {
    font-size: 12px;
}
</style>
