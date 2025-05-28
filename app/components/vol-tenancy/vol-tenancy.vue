<template>
	<view  class="service" v-if="serviceId">
		<text class="s-name" @click="showService">{{serviceName}}</text>
		<u-icon name="arrow-down" color="#fff" size="14"></u-icon>
		<u-popup @touchmove.prevent :zIndex="999999" :show="model" @close="model=false">
			<view class="vol-action-sheet-select-container" :style="{'max-height':(maxHeight+'px')}">
				<view class="vol-action-sheet-select-title">
					{{title}}
					<view class="f-icon" @click="model=false">取消</view>
				</view>
				<view class="service-list vol-action-sheet-select-content">
					<view @click="itemClick(item)" :class="{active:serviceId===item.id}" class="item" v-for="(item,index) in service" :key="index">
						{{item.name}}
					</view>
				</view>
			</view>
		</u-popup>
	</view>
</template>

<script>
	export default {
		props: {
			title: {
				type: String,
				default: ""
			}
		},
		data() {
			return {
				model: false,
				serviceId: '',
				service: [],
				maxHeight: 600
			}
		},
		computed: {
			serviceName() {
				if (!this.serviceId) {
					return '';
				}
				return this.service.find(x => {
					return x.id == this.serviceId
				}).name;
			}
		},
		methods: {
			showService() {
				this.model = true;
			},
			itemClick(item) {
				this.serviceId = item.id;
				this.model=false;
				uni.setStorageSync('serviceId', item.id)
			}
		},
		created() {
			this.http.post('api/menu/getTreeMenu', {}, false).then(result => {
				if (!result.service || !result.service.length) {
					return;
				}
				this.service = result.service;
				let serviceId = uni.getStorageSync('serviceId')
				if (!serviceId || !result.service.some(x => {
						return x.id == serviceId
					})) {
					serviceId = result.service[0].id;
					uni.setStorageSync('serviceId', serviceId)
				}
				this.serviceId = serviceId;
			})
		},
		mounted() {
			uni.getSystemInfo({
				success: (res) => {
					this.maxHeight = res.screenHeight * 0.82;
				}
			});
		}
	}
</script>

<style scoped lang="less">
	.service {
		font-size: 28rpx;
		display: flex;
	}

	.s-name {
		margin-right: 10rpx;
	}

	.vol-action-sheet-select-container {
		min-height: 600rpx;
		display: flex;
		flex-direction: column;

		.vol-action-sheet-select-title {
			color: #272626ee;
			font-weight: bold;

			padding: 28rpx;
			text-align: center;
			position: relative;
			border-bottom: 1px solid rgb(246 246 246);

			.f-icon {
				position: absolute;
				right: 0rpx;
				top: 0;
				font-size: 28rpx;
				color: #2979FF;
				line-height: 49px;
				height: 47px;
				width: 80px;
				padding-right: 30rpx;
				text-align: right;
			}
		}

		.vol-action-sheet-select-content {
			flex: 1;
			height: 0;
			background: #fff;
			overflow: scroll;
			color: #4c4c4c;
			text-align: center;
			// padding: 20px;

			.item {
				//border-bottom: 1px solid #f5f5f5;
				padding: 20rpx;
			}
			.active{
				color: #2979FF;
			}
		}
	}
</style>
