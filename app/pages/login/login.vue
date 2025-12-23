<template>
	<view class="login-container">
		<view class="login-bg fx">
			<view class="login-title">
				<view class="login-title-txt">
					你好！欢迎登录
				</view>
				<view class="login-title-small">
					Welcome to login
				</view>

			</view>
			<image class="login-bg-img" src="/static/login-bg.png"></image> -->
		</view>

		<view style="padding: 40rpx 60rpx 0 60rpx;">
			<view class="login-form">
				<view class="login-item">
					<image class="login-item-img" src="/static/account.png"></image>
					<u--input v-model="userInfo.userName" :placeholder="$ts('请输入帐号')" border="none" clearable
						fontSize="30rpx" placeholderStyle="fontSize:'30rpx'"></u--input>
				</view>
				<view class="login-item">
					<image class="login-item-img login-item-pwd" src="/static/pwd.png"></image>
					<u--input v-model="userInfo.password" type="password" :placeholder="$ts('请输入密码')" border="none"
						fontSize="30rpx" placeholderStyle="fontSize:'30rpx'" clearable></u--input>
				</view>
				<view class="login-item login-code">
					<image class="login-item-img" src="/static/yzm.png"></image>
					<u--input v-model="userInfo.verificationCode" :placeholder="$ts('请输入验证码')" border="none" clearable
						fontSize="30rpx" placeholderStyle="fontSize:'30rpx'"> </u--input>
					<image class="img" @click="getVierificationCode" :src="codeSrc"></image>
				</view>
			</view>

			<!-- <view class="agreement">
				<u-checkbox-group>
					<u-checkbox @change="ckChange" :size="13" labelSize="26rpx" label="我已同意并阅读" :checked="ck">
					</u-checkbox>
				</u-checkbox-group>
				<text @click="toAgreement" class="link">《用户服务协议》</text>与
				<text @click="toPolicy" class="link">《隐私政策》</text>
			</view> -->
			<view class="login-btn">
				<u-button @click="loginClick" shape="circle" :loading="loading" :loadingText="loading?'登录中..':''"
					:customStyle="{'box-shadow': '3px 3px 8px #5babff80'}" size="large" type="primary" text="登录">
				</u-button>
			</view>
		</view>
		<!-- <u-divider text="其他方式登陆"></u-divider>
		<view class="login-other">
			<button type="default" class="login-wechat-btn" @getphonenumber="getPhoneNumber" open-type="getPhoneNumber">
				<u-image width="110rpx" height="110rpx" src="@/static/share-icon-wechat.png"></u-image>
			</button>
		</view> -->
	</view>
</template>
<script setup>
	import {
		ref,
		getCurrentInstance,
		defineExpose
	} from "vue"
	const ck = ref(false);
	const loading = ref(false);
	const codeSrc = ref('');

	const userInfo = ref({
		userName: "",
		password: "",
		UUID: "",
		verificationCode: ""
	})
	const {
		proxy
	} = getCurrentInstance();

	const toPolicy = () => {
		uni.navigateTo({
			url: "/pages/policy/policy"
		})
	}
	const toAgreement = () => {
		uni.navigateTo({
			url: "/pages/agreement/agreement"
		})
	}
	//微信手机号登录
	const getPhoneNumber = (e) => {
		proxy.$toast('小程序正在申请审核中...')
		return;
		uni.login({
			provider: 'weixin',
			success: (loginRes) => {
				const params = {};
				params.code = loginRes.code;
				params.phoneDetail = {
					code: e.detail.code,
					encryptedData: e.detail.encryptedData,
					iv: e.detail.iv
				}
				proxy.http.post('api/user/loginWX', params, true).then(result => {
					if (!result.status) {
						proxy.$toast(result.message)
						return;
					}
					//console.log(result)
					proxy.$store.commit("setUserInfo", result.data);
					uni.switchTab({
						url: "/pages/home/home"
					})
				})
			}
		})
	}

	const getVierificationCode = () => {
		proxy.http.get("api/User/getVierificationCode").then(x => {
			codeSrc.value = "data:image/png;base64," + x.img;
			userInfo.value.UUID = x.uuid;
		});
	}
	
	const initTenancy=async()=>{
		await proxy.http.post('api/menu/getTreeMenu', {}, false).then(result => {
			if (!result.service || !result.service.length) {
				return;
			}
			let serviceId = uni.getStorageSync('serviceId')
			if (!serviceId || !result.service.some(x => {
					return x.id == serviceId
				})) {
				serviceId = result.service[0].id;
				uni.setStorageSync('serviceId', serviceId)
			}
		})
	}

	const loginClick = async () => {

		if (!ck.value) {
			//return proxy.$toast(proxy.$ts("请先阅读同意隐藏政策"));
		}

		if (proxy.base.isEmpty(userInfo.value.userName))
			return proxy.$toast(proxy.$ts("请输入用户名"));
		if (proxy.base.isEmpty(userInfo.value.password))
			return proxy.$toast(proxy.$ts("请输入密码"));
		if (proxy.base.isEmpty(userInfo.value.verificationCode))
			return proxy.$toast(proxy.$ts("请输入验证码"));
		userInfo.value.userName = userInfo.value.userName.trim();
		userInfo.value.password = userInfo.value.password.trim();
		userInfo.value.verificationCode = userInfo.value.verificationCode.trim();
		loading.value = true;
		proxy.http
			.post("api/user/login", userInfo.value, proxy.$ts('正在登录') + "....")
			.then(async (result) => {
				if (!result.status) {
					loading.value = false;
					getVierificationCode();
					return proxy.$toast(result.message);
				}
				proxy.$store.commit("setUserInfo", result.data);	
				await initTenancy();
				proxy.$toast(proxy.$ts("登录成功"));
				uni.switchTab({
					url: "/pages/home/home"
				})
			});
	}
	getVierificationCode();
	const ckChange = () => {
		ck.value = !ck.value;
	}
	defineExpose({})
</script>
<style lang="less" scoped>
	.login-container {
		height: auto;

		.login-item {
			padding: 12rpx 0;
			border-bottom: 1px solid #f6f6f6;
			margin-bottom: 20rpx;
			display: flex;
			align-items: center;

			.login-item-img {
				margin-right: 50rpx;
				height: 32rpx;
				width: 32rpx;
			}

			.login-item-pwd {
				height: 36rpx;
				width: 36rpx;
			}
		}

		.login-code {

			display: flex;

			.img {
				width: 120rpx;
				height: 54rpx;
			}
		}

		.login-btn {
			margin: 30rpx 0 0rpx 0;
		}
	}

	.agreement {
		padding: 24rpx 0rpx;
		font-size: 24rpx;
		color: #727272;
		display: flex;
		align-items: center;

		.link {
			color: #337aff;
			font-size: 24rpx;
		}
	}

	.login-bg {
		position: relative;
		width: 100%;
		height: 420rpx;
		display: flex;
		align-items: center;

		.login-title {
			z-index: 99;
			color: #fff;
			font-style: italic;
			padding: 60rpx 100rpx 0 100rpx;

			.login-title-txt {
				font-weight: bolder;
				font-size: 50rpx;
			}

			.logi-title-small {
				font-size: 34rpx;
			}
		}

		.login-bg-img {
			width: 100%;
			height: 100%;
			position: absolute;
			z-index: 0;
		}
	}

	.login-wechat-btn {
		position: relative;
		width: 60px;
		background-color: none;
		border: none;
		height: 60px;
		// border-radius: 50%;
		padding: 0;
	}

	.login-wechat-btn:after {
		border: none !important;
	}
</style>
