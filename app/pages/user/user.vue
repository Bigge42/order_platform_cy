<template>
	<view class="user-container">
		<view class="user-info">
			<!-- 	<image class="home-bg-img" src="/static/home-bg.png"></image> -->
			<image class="u-img" @error="onHeaderError()" :src="userInfo.img"></image>
			<view class="u-text">
				<view class="username">{{userInfo.userName}},{{getTimeText()}}</view>
				<view class="small-text">
					<vol-tenancy title="选择租户"></vol-tenancy>
				</view>
			</view>
			<view class="u-icon-setting" @click="toUserInfo">
				<u-icon name="arrow-right" color="rgb(227 227 227)" size="18"></u-icon>
			</view>
		</view>
		<view class="u-menu-list">
			<view class="u-menu-item" @click="itemClick(item)" v-for="(item,index) in menu" :key="index">
				<view class="u-menu-icon">
					<image style="width:70rpx;height: 70rpx;" :src="item.icon"></image>
					<!-- 	<u-icon :name="item.icon" color="#303133" size="15"></u-icon> -->
				</view>
				<view class="u-menu-text">{{$ts(item.name)}}</view>
				<view class="u-menu-icon-rigth">
					<u-icon name="arrow-right" color="#bebebe" size="15"></u-icon>
				</view>
			</view>
		</view>
		<vol-updater ref="updater" @getVersion="getVersion" :home="false"></vol-updater>
	</view>
</template>

<script>
	export default {
		data() {
			return {
				height: 50,
				userInfo: {
					img: "/static/imgs/head.png",
					userName: "请登陆"
				},
				menu: [{
						name: "修改密码",
						path: '',
						icon: '/static/icon/1.png',
						color: '',
						path: "/pages/user/modifyPwd"
					}, {
						name: "扫一扫",
						path: '/pages/user/about/about',
						icon: '/static/icon/27.png',
						color: '',
						click: () => {
							uni.scanCode({
								success(res) {
									console.log(res)
								}
							})
						}
					},
					{
						icon: '/static/icon/54.png',
						path: "/pages/language/language",
						name: "语言设置" //"Chinese" //"Chinese(中文)"
					},
					{
						icon: '/static/icon/53.png',
						version: "",
						ck: true,
						name: "检查更新",
						click: () => {
							this.$refs.updater.checkVersion();
						}
					},
					{
						name: "退出登录",
						path: '/pages/login/login',
						icon: '/static/icon/52.png',
						color: ''
					},
				]
			}
		},
		onLoad() {
			this.isInited = true;
			var _this = this;
			// // 获取手机状态栏高度
			// uni.getSystemInfo({
			// 	success: function(data) {
			// 		// 将其赋值给this
			// 		_this.height = data.statusBarHeight;
			// 	}
			// })
		},
		onShow() {
			this.setTabLang();
			this.getUserInfo();
		},
		methods: {
			getVersion(version) {
				try {
					this.list.forEach(item => {
						if (item.value == 'ck') {
							item.version = version;
						}
					})
				} catch (e) {
					console.log(e)
					//TODO handle the exception
				}
			},
			setTabLang() {
				this.$tsTabBar();
			},
			getUserInfo() {
				this.http.post("api/user/getCurrentUserInfo", {}).then(x => {
					//x.data.gender = x.data.gender;
					//  this.$refs.form.reset(x.data);
					this.userInfo.img = this.http.ipAddress + x.data.headImageUrl
					//this.userInfo.createDate = x.data.createDate;
					this.userInfo.userName = x.data.userTrueName;
					//this.userInfo.phoneNo = x.data.phoneNo;
					//this.userInfo.email = x.data.email;
				});
			},
			getTimeText() {
				let timeNow = new Date();
				// 获取当前小时
				let hours = timeNow.getHours();
				// 设置默认文字
				let text = ``;
				// 判断当前时间段
				if (hours >= 0 && hours <= 10) {
					text = `早上好`;
				} else if (hours > 10 && hours <= 14) {
					text = `中午好`;
				} else if (hours > 14 && hours <= 18) {
					text = `下午好`;
				} else if (hours > 18 && hours <= 24) {
					text = `晚上好`;
				}
				return text;
			},
			onHeaderError() {
				this.userInfo.img = "/static/imgs/head.png";
			},
			test() {
				this.http.get("api/menu/getTreeMenu", {}, false).then(result => {
					console.log(result)
				})
			},
			toUserInfo() {
				uni.navigateTo({
					url: '/pages/user/modifyPwd/modifyPwd'
				})
			},
			itemClick(item) {
				if (item.click) {
					item.click();
					return;
				}
				if (!item.path) {
					return;
				}
				if (item.path == "/pages/login/login") {
					this.$store.commit("clearUserInfo");
					uni.reLaunch({
						url: item.path
					})
					return;
				}
				uni.navigateTo({
					url: item.path
				})

			}
		}
	}
</script>

<style lang="less" scoped>
	.user-container {
		height: 100%;
		background: #fbfbfb;
	}

	.user-info {
		// height: 220rpx;
		display: flex;
		align-items: center;
		padding: 150rpx 40rpx 60rpx 60rpx;
		background-image: linear-gradient(135deg, #26bcff 10%, #078ef9);
		position: relative;
		border-radius: 10rpx;

		.u-img {
			width: 150rpx;
			height: 150rpx;
			border-radius: 50%;
			border: 2rpx solid #FFFFFF;
		}

		.u-text {
			flex: 1;
			color: #FFFFFF;
			padding: 26rpx 30rpx;
		}

		.username {
			font-weight: bolder;
			font-family: 黑体;
		}

		.small-text {
			font-size: 24rpx;
			padding-top: 10rpx;
		}

		.u-icon-setting {
			width: 30rpx;
			color: #FFFFFF;
		}
	}

	.u-menu-list {
		background: #FFFFFF;
		margin: 20rpx;
		border-radius: 5rpx;
		border: 1px solid #f7f7f7;

		.u-menu-item {
			display: flex;
			padding: 10rpx 30rpx;
			border-bottom: 1px solid #f7f7f7;
			align-items: center;

			.u-menu-icon {
				padding-top: 8rpx;
				padding-right: 20rpx;
			}

			.u-menu-text {
				flex: 1;
				color: #5e5e5e;
			}

			.u-menu-icon-rigth {
				width: 30rpx;
			}
		}
	}

	.home-bg-img {
		position: absolute;
		width: 100%;
	}
</style>
