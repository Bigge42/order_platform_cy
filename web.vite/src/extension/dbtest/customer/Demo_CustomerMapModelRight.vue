<template>
  <div>
   <div><el-alert show-icon	 type="warning" title="点击地图即可自动选点给表单设置值" :closable="false"></el-alert></div>
    <div style="width: 500px;height:370px;" :id="id" ref="map" class="map"></div>
  </div>
</template>
<script>
import AMapLoader from "@amap/amap-jsapi-loader";
export default {
  data() {
    return {
      id: Math.random(),
      list: [],
      autoComplete: "",
      geocoder: null,
      map: null,
      marker: null,
      markerList: [],
      detailAddress: "",
      lng: 107.031461,
      lat: 27.935342
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
      // if (this.lng) {
      //   this.regeoCode(false);
      // }
      //this.setPosition();
    },
    regeoCode(isClick) {
      if (!this.lng) {
        return;
      }
      this.map.add(this.marker);
      let lnglat = [this.lng, this.lat];
      this.marker.setPosition(lnglat);
      //地图点击事件
      this.geocoder.getAddress(lnglat, (status, result) => {
        if (status === 'complete' && result.regeocode) {
          //获取生成页面对象
          this.$emit('parentCall', $parnet => {
            //给左边表单设置值
            $parnet.editFormFields.Province = result.regeocode.addressComponent.province;
            $parnet.editFormFields.City = result.regeocode.addressComponent.city;
            $parnet.editFormFields.County = result.regeocode.addressComponent.district;
            $parnet.editFormFields.DetailAddress = result.regeocode.formattedAddress;

            $parnet.editFormFields.Remark = this.lng + " , " + this.lat;
          })
          console.log(result.regeocode)

          this.setCenter(isClick);
        } else {
          console.log('根据经纬度查询地址失败' + JSON.stringify(result));
          //  this.$message.error('定位异常：' + JSON.stringify(result))
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
    setPosition(){
      this.map.setCenter([100.211149 , 25.583954]); //设置地图中心点
    },
    setCenter(isClick) {
      if (!isClick) {
        this.map.setCenter([this.lng, this.lat]); //设置地图中心点
      }
    }
  },
};
</script>