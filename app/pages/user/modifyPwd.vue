<template>
	<view class="md-form">
		<view class="form-content">
			<vol-form ref="form" :form-options="formOptions" :formFields="formFields">
			</vol-form>
			<view style="margin: 20rpx 10rpx">
				<vol-button size="large" shape="circle" @click="submit" type="primary">
					{{$ts('修改密码')}}
				</vol-button>
			</view>
		</view>
	</view>
</template>

<script setup>
	import {
		ref,
		reactive,
		getCurrentInstance
	} from 'vue';

	const form = ref();
	const {
		proxy
	} = getCurrentInstance();
	const formFields = reactive({
		oldPwd: "",
		newPwd: ""
	})
	const formOptions = reactive([{
		title: "旧密码",
		field: "oldPwd",
		type: "password",
		required: true
	}, {
		title: "新密码",
		field: "newPwd",
		type: "password",
		required: true
	}])

	const submit = () => {
		if (!form.value.validate()) {
			return;
		}

		let url = "api/user/modifyPwd?oldPwd=" +
			formFields.oldPwd +
			"&newPwd=" +
			formFields.newPwd;
		proxy.http.post(url, {}, true).then(x => {
			proxy.$toast(x.message);
			if (!x.status) {
				return;
			};
		});
	}
</script>

<style lang="less" scoped>
	.md-form {
		background: #f7f7f7;
		height: 100%;
		padding: 20rpx 20rpx 0 20rpx;
	}

	.form-content {
		border-radius: 10rpx;
		padding: 20rpx;
		background: #fff;
	}
</style>