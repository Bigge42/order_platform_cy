<template>
    <div class="home-board" v-if="show">
        <div class="home-header">
            <div class="home-header-left" style="min-width: 400px;">
                <!-- <div style="width: 100%;"> -->
                <!-- <left-right-swiper-scroll> -->
                <div class="img-item" @click="toPage(item.url)" v-for="(item, index) in menu" :key="index">
                    <div :class="currentId == item.url ? 'active' : ''" class="img-item-title">{{ item.menuName }}
                    </div>
                    <img :src="currentId == item.url ? '/static/btn-02-ac.png' : '/static/btn-02.png'">
                </div>
                <!-- </left-right-swiper-scroll> -->
                <!-- </div> -->
                <!-- <div class="img-item" @click="toPage(path2)">
                    <div class="img-item-title">设备管理</div>
                    <img style="height: 40px;" src="/static/btn-02.png">
                </div> -->
            </div>
            <div class="home-title">{{ projectName }}</div>
            <div class="home-header-right">
                <n-dropdown v-if="serviceName && options.length" key-field="id" label-field="name" trigger="hover"
                    :options="options" @select="handleSelect">
                    <n-button text style="font-size: 13px;color:aqua">{{ serviceName }}
                        <svg width="13" height="13" xmlns="http://www.w3.org/2000/svg"
                            xmlns:xlink="http://www.w3.org/1999/xlink" viewBox="0 10 16 4">
                            <path
                                d="M15.88 9.29L12 13.17L8.12 9.29a.996.996 0 1 0-1.41 1.41l4.59 4.59c.39.39 1.02.39 1.41 0l4.59-4.59a.996.996 0 0 0 0-1.41c-.39-.38-1.03-.39-1.42 0z"
                                fill="currentColor"></path>
                        </svg></n-button>

                </n-dropdown>

                <div class="home-header-right-content" v-if="menu.length">
                    <div class="date">{{ date }}</div>

                    <button @click="tologin" class="full-btn"><svg xmlns="http://www.w3.org/2000/svg" width="13" height="13"
                            style="margin-right: 5px;top:1px;position: relative;" xmlns:xlink="http://www.w3.org/1999/xlink"
                            viewBox="0 0 1024 1024">
                            <path
                                d="M868 732h-70.3c-4.8 0-9.3 2.1-12.3 5.8c-7 8.5-14.5 16.7-22.4 24.5a353.84 353.84 0 0 1-112.7 75.9A352.8 352.8 0 0 1 512.4 866c-47.9 0-94.3-9.4-137.9-27.8a353.84 353.84 0 0 1-112.7-75.9a353.28 353.28 0 0 1-76-112.5C167.3 606.2 158 559.9 158 512s9.4-94.2 27.8-137.8c17.8-42.1 43.4-80 76-112.5s70.5-58.1 112.7-75.9c43.6-18.4 90-27.8 137.9-27.8c47.9 0 94.3 9.3 137.9 27.8c42.2 17.8 80.1 43.4 112.7 75.9c7.9 7.9 15.3 16.1 22.4 24.5c3 3.7 7.6 5.8 12.3 5.8H868c6.3 0 10.2-7 6.7-12.3C798 160.5 663.8 81.6 511.3 82C271.7 82.6 79.6 277.1 82 516.4C84.4 751.9 276.2 942 512.4 942c152.1 0 285.7-78.8 362.3-197.7c3.4-5.3-.4-12.3-6.7-12.3zm88.9-226.3L815 393.7c-5.3-4.2-13-.4-13 6.3v76H488c-4.4 0-8 3.6-8 8v56c0 4.4 3.6 8 8 8h314v76c0 6.7 7.8 10.5 13 6.3l141.9-112a8 8 0 0 0 0-12.6z"
                                fill="currentColor"></path>
                        </svg>注销</button>
                    <button @click="toFull" class="full-btn">
                        <svg xmlns="http://www.w3.org/2000/svg" width="13" height="13"
                            xmlns:xlink="http://www.w3.org/1999/xlink" viewBox="0 4 16 4">
                            <g fill="none">
                                <path
                                    d="M3.75 3a.75.75 0 0 0-.75.75V5.5a.5.5 0 0 1-1 0V3.75C2 2.784 2.784 2 3.75 2H5.5a.5.5 0 0 1 0 1H3.75zM10 2.5a.5.5 0 0 1 .5-.5h1.75c.966 0 1.75.784 1.75 1.75V5.5a.5.5 0 0 1-1 0V3.75a.75.75 0 0 0-.75-.75H10.5a.5.5 0 0 1-.5-.5zM2.5 10a.5.5 0 0 1 .5.5v1.75c0 .414.336.75.75.75H5.5a.5.5 0 0 1 0 1H3.75A1.75 1.75 0 0 1 2 12.25V10.5a.5.5 0 0 1 .5-.5zm11 0a.5.5 0 0 1 .5.5v1.75A1.75 1.75 0 0 1 12.25 14H10.5a.5.5 0 0 1 0-1h1.75a.75.75 0 0 0 .75-.75V10.5a.5.5 0 0 1 .5-.5z"
                                    fill="currentColor"></path>
                            </g>
                        </svg>
                        {{ fullscreen ? '退出' : '全屏' }}</button>
                </div>
            </div>
            <div class="move-line"></div>
            <img class="home-header-bg" src="/static/header-04.png" />

        </div>
        <router-view></router-view>
        <div class="home-board-bg"></div>
    </div>
</template>
<script>
// https://www.daidaibg.com/bigscreen-vue3/#/index
//https://docs.apipost.cn/preview/5aa85d10a59d66ce/ddb813732007ad2b?target_id=84dbc5b0-158f-4bcb-8f74-793ac604ada3
const week = new Array(
    '星期一',
    '星期二',
    '星期三',
    '星期四',
    '星期五',
    '星期六',
    '星期日'
);

import { useSystemStore } from '@/store/modules/systemStore/systemStore'
import { SystemStoreUserInfoEnum, SystemStoreEnum } from '@/store/modules/systemStore/systemStore.d'
import { getToken } from '@/api/path'
import { fetchPathByName, routerTurnByPath, openNewWindow, previewPath, getLocalStorage } from '@/utils'
import { StorageEnum } from '@/enums/storageEnum'
import ScrollTabs from './ScrollTabs.vue'
import options from '@/options.js'
export default {
    components: {
        'left-right-swiper-scroll': ScrollTabs
    },

    data() {
        return {
            //  serviceName:"",
            options: [],
            fullscreen: false,
            show: false,
            date: '',
            menu: [],
            currentId: '',
            projectName: ""
            // path1: '/chart/preview/16564609255818240',
            // path2: '/chart/preview/16568795534623744'
        }
    },
    created() {
        this.projectName = options.title;
    },
    mounted() {
        // this.$router.push({ path: '/chart/preview/16568795341767680' })
        // return;
        this.init() 
    },
    watch: {
        $route(to, from) {
            if (this.show && this.$route.query.key) {
                this.init();
            }
        }
    },
    methods: {
        handleSelect(key) {
            localStorage.setItem('serviceId', key);
            window.location.reload();
        },
        async init() {
            if (this.$route.query.key) {
                const systemStore = useSystemStore()
                await getToken(this.$route.query.key).then(res => {
                    //return
                    if (!res.status) {
                        window.$message.error(res.msg)
                    }
                    //  console.log(res)

                    const { tokenValue, tokenName } = res.data.token
                    const { nickname, username, id } = res.data.userinfo

                    // 存储到 pinia 
                    systemStore.setItem(SystemStoreEnum.USER_INFO, {
                        [SystemStoreUserInfoEnum.USER_TOKEN]: tokenValue,
                        [SystemStoreUserInfoEnum.TOKEN_NAME]: tokenName,
                        [SystemStoreUserInfoEnum.USER_ID]: id,
                        [SystemStoreUserInfoEnum.USER_NAME]: username,
                        [SystemStoreUserInfoEnum.NICK_NAME]: nickname,
                        [SystemStoreUserInfoEnum.MENU]: res.data.menu,
                    })
                    localStorage.setItem('service', JSON.stringify(res.data.service))

                    // window['$message'].success(t('login.login_success'))
                });

                // if (this.$router.push({ path: this.$route.query.path })) {

                // }
                // return;
            }
          
            const service = localStorage.getItem('service');
            let hasId = false;
            if (service) {
                try {
                    this.options = JSON.parse(service);
                    if (this.options.length) {
                        
                        let id = localStorage.getItem('serviceId');
                        let item = this.options.find(x => { return x.id == id });
                        if (!item) {
                            id = this.options[0].id;
                            item=this.options[0];
                        }
                        this.serviceName = item.name;
                        hasId = true;
                        localStorage.setItem('serviceId', id)
                    }
                } catch (error) {
                    console.log(error)
                }
            }
            if (!hasId) {
                localStorage.removeItem('serviceId')
            }



            // this.show = true;
            // return;
            let info = (getLocalStorage(StorageEnum.GO_SYSTEM_STORE) || {}).userInfo;

            if (!info) {
                //  this.$router.push({path:"/login"});
                return;
            }
            if (!info || !info.menu || !info.menu.length) {
                info = info || { menu: [] };
                info.menu = []
            }
            if (!info.menu.some(x => { return x.url == this.$route.params.id }) && !this.$route.query.key) {
                this.show = true;
                this.currentId = this.$route.params.id;
                return;
                // window['$message'].error(('没有数据查看权限'))
                // return;
            }
            console.log(info)
            this.menu = info.menu;
            if (this.$route.query.path) {
                this.$router.push({ path: this.$route.query.path })
                // this.show = true;
                setTimeout(() => { this.show = true; }, 500)
                return;
            }

            this.show = true;
            console.log(this.$route.query)
            this.showTime();
            setInterval(() => { this.showTime() }, 1000);
            let url;
            if (this.$route.query.key || (this.$route.params.id && !this.menu.some(x => { return x.url == this.$route.params.id }))) {
                this.currentId = this.menu[0].url;
                this.toPage(this.menu[0].url);
            }
            //  else if(this.$route.query.key) {
            //     this.currentId = this.menu[0].url;
            //     this.toPage(this.menu[0].url);
            // }

        },
        tologin() {
            window.close();
        },
        // toPage1() {
        //     this.toPage(this.path1);
        //     //  window.open(this.path1)
        // },
        toPage(path) {
            this.currentId = path;
            console.log(path);
            this.$router.push({ path: "/home/index" })
            setTimeout(() => {
                this.$router.push({ path: '/chart/preview/' + path })
                document.body
            }, 50)
        },
        toFull() {
            this.full = !this.full;
            if (this.full) {
                this.handleFullScreen();
            } else {
                this.onExitScreen();
            }
        },
        //退出全屏 
        onExitScreen() {
            let element = document.documentElement;  // 将整个页面设为全屏  
            if (document.exitFullscreen) {
                document.exitFullscreen();  // W3C  
            } else if (document.mozCancelFullScreen) {  // Firefox  
                document.mozCancelFullScreen();
            } else if (document.webkitExitFullscreen) {  // Chrome, Safari and Opera  
                document.webkitExitFullscreen();
            } else if (document.msExitFullscreen) {  // IE/Edge  
                document.msExitFullscreen();
            }
        },
        //进入全屏
        handleFullScreen() {
            // 检查浏览器是否支持 Full Screen API  
            let element = document.documentElement;  // 将整个页面设为全屏  

            if (element.requestFullscreen) {
                element.requestFullscreen();  // W3C  
            } else if (element.mozRequestFullScreen) {  // Firefox  
                element.mozRequestFullScreen();
            } else if (element.webkitRequestFullscreen) {  // Chrome, Safari and Opera  
                element.webkitRequestFullscreen();
            } else if (element.msRequestFullscreen) {  // IE/Edge  
                element.msRequestFullscreen();
            }
            this.fullscreen = !this.fullscreen;
        },
        showTime() {
            let date = new Date();
            let year = date.getFullYear();
            let month = date.getMonth() + 1;
            let day = date.getDate();
            let hour = date.getHours();
            let minutes = date.getMinutes();
            let second = date.getSeconds();

            this.date =
                year +
                '.' +
                (month < 10 ? '0' + month : month) +
                '.' +
                (day < 10 ? '0' + day : day) + //202.08.08修复日期天数小于10时添加0
                '' +
                ' ' +
                (hour < 10 ? '0' + hour : hour) +
                ':' +
                (minutes < 10 ? '0' + minutes : minutes) +
                ':' +
                (second < 10 ? '0' + second : second) +
                ' ' + //2020.08.30修复首页日期星期天不显示的问题
                (week[date.getDay() - 1] || week[6]);
        }
    }
}
</script>
<style scoped >
.home-board {}

.home-board-bg {
    position: absolute;
    width: 100%;
    height: 100%;
    z-index: 0;
    background: url(/static/pageBg.png);
    background-size: 100% 100%;
    top: 0;
}

.home-header {
    position: relative;
    left: 0;
    right: 0;
    margin: 0 auto;
    height: 60px;
    display: flex;


    bottom: 0.01rem;
    /* border-bottom: 1px solid #0b89e7; */
    box-shadow: 0 0 0.04rem rgba(11, 137, 231, .6);
    z-index: 1;
    /* color: var(--font-color); */
    height: 3rem;
    width: 100%;
    position: relative;
    line-height: 3.5rem;
}

.home-header>div {
    z-index: 99;
}

.home-header-bg {
    position: absolute;
    left: 0;
    right: 0;
    margin: 0 auto;
    top: 0;
    z-index: 0;
    height: 3.84rem;
    width: 100%;
}

.home-title {
    text-align: center;
    font-size: 28px;
    font-weight: 900;
    letter-spacing: 6px;
    /* display: flex; */
    line-height: 3.7rem;
    /* flex: 1; */
    -webkit-background-clip: text;
    color: transparent;
    background-image: linear-gradient(92deg, #0072ff 0%, #00eaff 48.8525390625%, #01aaff 100%);
    margin: 0 50px;
    min-width: 250px;
    text-align: center;
}

.home-header-left,
.home-header-right {
    flex: 1;
    font-size: .8rem;
    display: flex;
    justify-content: right;
    align-items: center;
}

.home-header-left {
    display: flex;
    justify-content: left;
    /* padding-left: 40px; */
}

.img-item {
    cursor: pointer;
    display: flex;
    position: relative;
    width: 133px;
    justify-content: center;
    align-items: center;
}

.img-item img {
    position: absolute;
    /* width: 100%; */
    height: 36px;
    width: 125px;
    top: 11px;
}

.img-item-title {
    width: 100%;
    z-index: 9999;
    text-align: center;
    color: #03A9F4;
    font-size: 13px;
    padding-right: 18px;
}

.img-item-title.active {
    color: #18ddf5;
    font-weight: bolder;
}

.home-header-right-content {
    display: flex;
    justify-content: right;
    margin-left: 1.5rem;
}

.full-btn {
    font-size: 13px;
    color: #18ddf5;
    background: none;
    margin-right: 15px;
    border: none;
    cursor: pointer;
}

.date {
    padding-right: 1rem;
    color: var(--font-color);
}

.home-board ::v-deep(.go-preview.fit) {
    background: none !important;
    z-index: 99;
}

.move-line {
    width: 2.47rem;
    height: 0.38rem;
    position: absolute;
    bottom: -11px;
    height: 28px;
    width: 106px;
    background: url(/static/line-bg.png) no-repeat;
    background-size: 100% 100%;
    animation: move 4s ease-in-out infinite;

}

@keyframes move {
    0% {
        left: -2.47rem
    }

    to {
        left: 120%
    }
}
</style>
