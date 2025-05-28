<template>
	<view>
	</view>
</template>

<script>
	export default {
		props: {
			home: {
				type: Boolean,
				default: false
			}
		},
		name: "vol-updater",
		data() {
			return {

			};
		},
		methods: {
			download(url) {
				var dtask = plus.downloader.createDownload(url, {}, function(d, status) { //新建下载任务
					if (status == 200) { //当下载完成
						let path = plus.io.convertLocalFileSystemURL(d.filename);
						plus.runtime.install(path, {
							force: false
						}, () => {
							plus.runtime.restart();
						}, (error) => {
							uni.showToast({
								title: '安装失败',
								duration: 1500,
								icon: 'none'
							});
							console.log(error.message)
						});
						return;
					}
					uni.showToast({
						title: '更新失败',
						duration: 1500,
						icon: 'none'
					});

				})
				dtask.start();
				var prg = 0;
				var showLoading = plus.nativeUI.showWaiting("正在下载....");
				dtask.addEventListener('statechanged', function(task, status) { //添加下载任务事件监听器
					// 给下载任务设置一个监听 并根据状态 做操作
					switch (task.state) {
						case 1:
							showLoading.setTitle("正在下载");
							console.log('1111')
							break;
						case 2:
							showLoading.setTitle("已连接到服务器");
							console.log('222')
							break;
						case 3:
							prg = parseInt( //下载的进度
								(parseFloat(task.downloadedSize) / parseFloat(task.totalSize)) * 100
							);
							showLoading.setTitle("版本更新,正在下载" + prg + "% ");
							// console.log('3333')
							console.log("版本更新,正在下载" + prg + "% ")
							break;
						case 4:
							plus.nativeUI.closeWaiting(); //关闭系统提示框
							//下载完成
							break;
					}
				});
			},
			getVersion(version, isService) {
				//服务器当前版本被忽略过，不再提示
				if (this.home && isService && uni.getStorageSync('ingVersion') == version) {
					return -999;
				}
				for (var i = 0; i < 5; i++) {
					version = version.replace('.', '');
				}
				return version * 1;
			},
			updateAndroid(wgtinfo) {
				console.log(wgtinfo)
				this.http.get("api/app/getAndroidVersion?home=" + this.home, {}, false).then(result => {
					console.log(result)
					if (!result.status) {
						this.$emit('updaterLoad')
						return;
					}
					//version为manifest.json小版本号(应用版本名称)
					if (this.getVersion(result.version, true) <= this.getVersion(wgtinfo.version)) {
						if (!this.home) {
							this.$toast('当前已是最新版本')
						} else {
							this.$emit('updaterLoad')
						}
						return;
					}
					uni.showModal({
						title: "发现版本更新",
						content: result.desc || '发现版本,是否立即更新', //更新描述
						confirmText: '立即更新',
						cancelText: '稍后进行',
						success: sucRes => {
							if (!sucRes.confirm) {
								//首页点击忽略当前版本
								if (this.home) {
									uni.setStorageSync('ingVersion', result.version)
								}
								return;
							};
							this.download(result.url);
						}
					})
				})
			},
			updateIOS(wgtinfo) {
				this.http.get("api/app/getIOSVersion?home=" + this.home, {}, false).then(result => {
					if (!result.status) {
						this.$emit('updaterLoad')
						return;
					}
					console.log(wgtinfo.version)
					if (this.getVersion(result.version, true) <= this.getVersion(wgtinfo.version)) {
						if (!this.home) {
							this.$toast('当前已是最新版本')
						} else {
							this.$emit('updaterLoad')
						}

						return;
					}
					console.log('ios')

					uni.showModal({
						title: "发现版本更新",
						content: result.desc || '发现版本,是否立即更新', //更新描述
						confirmText: '立即更新',
						cancelText: '稍后进行',
						success: sucRes => {
							if (!sucRes.confirm) {
								//首页点击忽略当前版本
								if (this.home) {
									uni.setStorageSync('ingVersion', result.version)
								}
								return;
							};
							plus.runtime.launchApplication({
								action: result.url
								//`itms-apps://itunes.apple.com/cn/app/id${appleId}?mt=8`
							}, (e) => {
								this.$toast('Open system default browser failed: ' + e
									.message);
							});
						}
					})
					//let appleId = 111111111

				})
			},
			updateWechat() {
				const updateManager = uni.getUpdateManager();

				updateManager.onCheckForUpdate(function(res) {
					// 请求完新版本信息的回调
					console.log(res.hasUpdate);
				});

				updateManager.onUpdateReady(function(res) {
					uni.showModal({
						title: '更新提示',
						content: '新版本已经准备好，是否重启应用？',
						success(res) {
							if (res.confirm) {
								// 新的版本已经下载好，调用 applyUpdate 应用新版本并重启
								updateManager.applyUpdate();
							} else {

							}
						}
					});
				});
				updateManager.onUpdateFailed(function(res) {
					// 新的版本下载失败
				});
			},
			checkVersion(isUserGetVersion) {

				// // #ifdef MP-WEIXIN
				// !isUserGetVersion && this.updateWechat();
				// this.$emit('updaterLoad')
				// return;
				// // #endif

				//apk文件名不能带中文
				//#ifdef APP-PLUS
				let isAndroid;
				uni.getSystemInfo({
					success: (e) => {
						isAndroid = e.platform === 'android';
						plus.runtime.getProperty(plus.runtime.appid, (wgtinfo) => {
							if (isUserGetVersion) {
								this.$emit('getVersion', wgtinfo.version)
								return;
							}
							if (isAndroid) {
								this.updateAndroid(wgtinfo);
								return;
							}
							this.updateIOS(wgtinfo);
						})
					},
					fail(ex) {
						this.$toast(ex.messsage)
					}
				})
				return;
				//#endif
				//this.$emit('updaterLoad')
				if (!isUserGetVersion) {
					this.$toast('当前已经最新版本')
				}

			}
		},
		created() {
			//https://www.cnblogs.com/menxiaojin/p/13755555.html
			//https://developers.weixin.qq.com/miniprogram/dev/api/base/update/UpdateManager.html
			if (this.home) {
				this.checkVersion();
			} else {
				this.checkVersion(true);
			}
		}
	}
</script>

<style>

</style>