<template>
	<view v-if="flowInit" class="audit-model-content">
		<view style="background: #ffff;">
			<u-tabs :list="list" @click="tabsClick"></u-tabs>
		</view>

		<view v-show="currentIndex == 0 || !hasFlow" class="audit-content">
			<view class="fx-right" v-if="isCurrentUser || !hasFlow">
				<view v-if="!hasFlow">
					<u-alert :title="$ts('当前选中【$ts(' + rowLen + ')】条记录待审核')" type="success" :closable="false" />
				</view>
				<view class="rd" style="display: flex;margin-bottom: 20rpx;">
					<text>{{$ts('审批')}}：</text>
					<u-radio-group style="margin-left:15px" v-model="auditParam.value">
						<view style="margin-right: 30rpx;" v-for="item in auditParam.data" :key="item.value">
							<u-radio :name="item.value" :label="item.text">
							</u-radio>
						</view>
					</u-radio-group>
				</view>
				<view style="border: 1px solid #d0d0d0;border-radius: 6rpx;">
					<u--textarea border="none" :height="100" v-model="auditParam.reason" :placeholder="$ts('请输入审批内容')">
					</u--textarea>

				</view>

				<view class="btn">
					<u-button type="primary" shape="circle" @click="auditClick">审批</u-button>
				</view>
			</view>
			<view class="fx-left v-steps-list" v-if="hasFlow">
				<!-- <u-viewider text="流程信息" :hairline="true"></u-viewider>
 -->
				<view class="v-steps">
					<view v-for="(item, index) in workFlowSteps" :key="index">
						<view class="step-item" :class="{'step-item-ad':item.auditId||item.stepAttrType=='start'}"
							v-if="item.stepAttrType == 'start'">
							<!-- 	<view class="left-item">
								<view>流程开始</view>
								<view class="left-date">{{ item.createDate }}</view>
							</view> -->
							<view class="right-item">
								<view class="step-line"></view>
								<i class="step-circle"></i>
								<view class="step-title">
									{{ item.stepName }}
								</view>
								<view class="step-text">{{$ts('发起人')}}：{{ item.creator }}</view>
							</view>
						</view>
						<view class="step-item" v-else-if="item.stepAttrType == 'end'">
							<!-- 	<view class="left-item">
								<view>流程结束</view>
							</view> -->
							<view class="right-item">
								<view class="step-line"></view>
								<i class="step-circle"></i>
								<view class="step-title">
									{{ item.stepName }}
								</view>
							</view>
						</view>
						<view v-else :class="{ 'step-current': item.isCurrent }" class="step-item">

							<view class="right-item">

								<view class="step-line"></view>
								<i class="step-circle"></i>
								<view class="step-title">
									{{ item.stepName }}
								</view>
								<view v-if="item.auditList" class="step-table">
									<view class="step-table-item">
										<view class="step-table-user step-cell">审批人</view>
										<view class="step-table-status step-cell">状态</view>
										<view class="step-table-date step-cell">审批时间</view>
									</view>
									<view class="step-table-item" v-for="(data, aindex) in item.auditList"
										:key="aindex">
										<view class="step-table-user step-cell">
											{{ data.auditor }}
										</view>
										<view class="step-table-status step-cell">
											{{ data.auditStatus ? getAuditStatus(data.auditStatus) : "" }}
										</view>
										<view class="step-table-date step-cell">
											{{ data.auditDate || "" }}
										</view>
									</view>
									<view class="step-table-text">
										审批意见： {{ item.remark || "-" }}
									</view>
								</view>
								<view v-else>
									<view class="step-text">{{$ts('审批人')}}：{{ item.auditor }}</view>
									<view class="step-text">
										{{$ts('状态')}}： {{ getAuditStatus(item.auditStatus) }}
									</view>
									<view class="step-text">
										{{$ts('审批时间')}}： {{ item.auditDate ||$ts('待审批') }}
									</view>
									<view class="step-text">{{$ts('备注')}}： {{ item.remark || '-' }}</view>
								</view>
							</view>
						</view>
					</view>
				</view>
			</view>

		</view>

		<view v-show="currentIndex===1">
			<view class="detail-item">
				<view class="f-item " v-for="(data, key) in formData" :key="key">
					<view class="f-name">{{ $ts(data.name)}}</view>
					<view class="f-value">
						<view class="form-file" v-if='data.formType == "file" || data.formType == "excel"'>
							<text class="file-item" @click="fileClick(file)"
								v-for="(file, findex) in getFilePath(data.value)" :key="findex">{{ file.name
				            }}</text>
						</view>
						<view class="form-img" v-else-if='data.formType == "img"'>
							<u--image @click="imgClick(file, data, findex)" style="float:left;margin-left:5px;"
								width="40px" height="40px" radius="4px"
								v-for="(file, findex) in getFilePath(data.value)" :key="findex" :src="file.path">
							</u--image>
						</view>
						<view class="form-editor" v-else-if='data.formType == "editor"'>
							<u-parse :content="data.value"></u-parse>
						</view>
						<view v-else>
							{{ data.value }}
						</view>
					</view>
				</view>
			</view>

			<view class="audit-detail-table" v-if="formDetail.name && formDetail.data && formDetail.data.length">
				<view class="audit-detail-title">{{ $ts(formDetail.name) }}</view>
				<view class="audit-detail-list">
					<view class="detail-item" v-for="(detailItem,index) in formDetail.data" :key="index">
						<view class="f-item" v-for="(data, key) in detailItem" :key="key">
							<view class="f-name">{{$ts(data.name)}}</view>
							<view class="f-value">
								<!-- {{data.value}} -->
								<view class="form-file" v-if='data.formType == "file" || data.formType == "excel"'>
									<text @click="fileClick(file)" v-for="(file, findex) in getFilePath(data.value)"
										:key="findex">{{ file.name}}</text>
								</view>
								<view class="form-img" v-else-if='data.formType == "img"'>
									<u--image @click="imgClick(file, data, findex)" style="float:left;margin-left:5px;"
										width="40px" height="40px" radius="4px"
										v-for="(file, findex) in getFilePath(data.value)" :key="findex"
										:src="file.path"></u--image>
								</view>
								<view v-else-if='data.formType == "editor"' v-html="data.value">
								</view>
								<view v-else>
									{{ data.value||'' }}
								</view>
							</view>
						</view>
					</view>
					<!-- <view>
						<view v-for="(value, key) in formDetail.data[0]" :key="key">
							{{ key }}
						</view>
					</view>
					<view v-for="(data, aindex) in formDetail.data" :key="aindex">
						<view v-for="(value, key) in data" :key="key">
							{{ value }}
						</view>
					</view> -->
				</view>
			</view>
		</view>
		<view v-show="currentIndex ===2">
			<vol-table titleField="auditDate" :tableData="tableData" :columns="columns" :pagination-hide="true"
				:load-key="false" :text-inline="false" :ck="false"></vol-table>
		</view>
	</view>

</template>
<script>
	let currentOption = {}
	export default {
		props: {
			data: {
				type: Object,
				default: () => {
					return {
						table: "",
						tableKey: ""
					}
				}
			}
		},
		data() {
			return {
				flowInit: false,
				currentIndex: 0,
				list: [{
					name: this.$ts('审批')
				}, {
					name: this.$ts('详细信息'),
				}, {
					name: this.$ts('审批记录')
				}],
				height: 550,
				width: 1000,
				workFlowSteps: [],
				hasFlow: false,
				formData: {},
				formDetail: {
					name: "",
					data: []
				},
				isFlow: false,

				auditParam: {
					//审核对象
					rows: 0, //当前选中审核的行数
					model: false, //审核弹出框
					value: -1, //审核结果
					reason: '', //审核原因
					status: [0, 2], //审核中的数据
					data: [{
							text: '通过',
							value: 1
						},
						{
							text: '拒绝',
							value: 3
						},
						{
							text: '驳回',
							value: 4
						}
					],
				},
				tableData: [],
				columns: [{
						title: '节点',
						field: 'stepName',
						width: 100
					},
					{
						title: '审批人',
						field: 'auditor',
						width: 80
					},
					{
						title: '审批结果',
						field: 'auditStatus',
						width: 70,
						bind: {
							data: []
						}
					},
					{
						title: '审批时间',
						field: 'auditDate',
						width: 145
					},
					{
						title: '备注',
						field: 'remark',
						width: 120
					}
				],
				isCurrentUser: null,
				activeName: "audit",
				auditDic: [],
				rowLen: 0,
				currentRows: []
			}
		},
		methods: {
			tabsClick(item) {
				console.log(item)
				this.currentIndex = item.index;
				this.list[item.index].inited = true;
			},
			getAuditStatus(key) {
				return (this.auditDic.find(x => {
					return x.key === key + ''
				}) || {
					value: key
				}).value || '待审批';
			},


			auditClick() {
				if (this.auditParam.value == -1) {
					this.$toast('请选择审批项');
					return;
				}


				//我的流程中点击审批
				//保存审核
				let keys = this.currentRows.map(x => {
					return x[currentOption.key]
				});
				let url =
					`api/${currentOption.table}/audit?auditReason=${this.auditParam.reason}&auditStatus=${this.auditParam.value}`

				// this.$emit('onAudit', '');
				// return;
				this.http.post(url, keys, '审核中....').then((x) => {
					this.$toast(x.message || '审批成功');
					if (!x.status) {
						return;
					}
					this.isCurrentUser = false;
					this.$emit('onAudit', x);
				});
			},
			getAuditInfo(option) {
				const table = option.table;
				const url = `api/Sys_WorkFlow/getSteps?tableName=${table}`
				let ids = this.currentRows.map(x => {
					return x[option.key]
				});
				this.http.post(url, ids, true).then(result => {
					if (!result.status) {
						this.$toast(result.message);
						return;
					}

					this.flowInit = true;
					this.hasFlow = !!(result.list || []).length;
					if (!this.hasFlow) {

						let auditStatus = Object.keys(this.currentRows[0]).find(x => {
							return x.toLowerCase() === 'auditstatus'
						});

						let checkStatus = this.currentRows.every((x) => {
							return this.auditParam.status.some(c => {
								return c === x[auditStatus] || !x[auditStatus]
							})
						});
						if (!checkStatus) {
							this.$toast('只能选择待审批或审核中的数据');
							return;
						}
						this.rowLen = this.currentRows.length;

						this.isCurrentUser = true;
						//没有审批流程的数据只显示
						return;
					}


					if (!this.auditDic.length) {
						this.auditDic.push(...(result.auditDic || []))
						this.columns.forEach(item => {
							if (item.field == 'auditStatus') {
								item.bind.data = this.auditDic;
							}
						})
					}
					this.isCurrentUser = result.list.some(x => {
						return x.isCurrentUser
					})
					this.workFlowSteps.length = 0;
					this.workFlowSteps.push(...result.list);
					this.tableData.length = 0;
					result.logs.forEach(x => {
						if (x.stepAttrType = 'start') {
							x.stepName = '进入流程';
							x.auditDate = x.createDate;
						}
					})
					this.tableData.push(...result.logs)
					this.formData = result.form.data || {}
					//console.log(this.formData)
					this.formDetail = result.form.detail || {
						name: "",
						data: []
					};
					//this.formData.push(...(result.data || []))
				})
			},
			initFlow(rows, flow) {
				this.isFlow = !!flow;
				this.currentRows = rows;
				this.activeName = 'audit'
				this.auditParam.reason = '';
				this.auditParam.value = -1;

				if (flow) {
					currentOption = {
						table: rows[0].WorkTable,
						key: "WorkTableKey" // rows[0].WorkTableKey
					}
				}
				this.getAuditInfo(currentOption);

			},
			getFilePath(pathSring) {
				if (!pathSring) return [];
				let filePath = pathSring.replace(/\\/g, "/").split(",");
				let fileInfo = [];
				for (let index = 0; index < filePath.length; index++) {
					let path = filePath[index];
					if (path.indexOf(".") != -1) {
						let splitFile = path.split("/");
						if (splitFile.length > 0) {
							fileInfo.push({
								name: splitFile[splitFile.length - 1],
								path: this.http.ipAddress + path,
							});
						}
					}
				}
				return fileInfo;
			},
			fileClick(file) {

			},
			imgClick(file, data, index) {
				let fileInfo = this.getFilePath(data.value);
				let imgs = fileInfo.map(x => {
					return x.path
				});
				uni.previewImage({
					current: index,
					urls: imgs,
					longPressActions: {}
				});
			}
		},
		created() {
			let rows = [{
				WorkTable: this.data.workTable,
				WorkTableKey: this.data.tableKey
			}]
			this.initFlow(rows, true);
		},
		mounted() {
			// uni.getSystemInfo({
			// 	success: (resu) => {
			// 		var view = uni.createSelectorQuery().in(this).select(".audit-model-content");
			// 		view.boundingClientRect().exec(res => {
			// 			this.height = resu.windowHeight;
			// 			console.log(this.height)
			// 		})
			// 	}
			// })
		}
	};
</script>

<style lang="less" scoped>
	// .audit-model-content {
	// 	position: absolute;
	// 	width: 100%;
	// 	height: 100%;
	// 	background: #f9f9f9;
	// 	overflow: auto;
	// }

	.step-item {
		background: #fff;
		display: flex;
	}

	.left-item {
		min-width: 180px;
		text-align: right;
		padding-right: 25px;
		padding-top: 8px;

		.left-date {
			font-size: 13px;
			padding-top: 7px;
			color: #6c6c6c;
		}
	}

	.right-item {
		cursor: pointer;
		position: relative;
		border-bottom: 1px solid #f3f3f3;
		padding: 5px 0 5px 5px;
		flex: 1;
	}

	.left-item,
	.right-item {
		padding-bottom: 10px;
	}

	.right-item:last-child {
		border-bottom: 0;
	}

	.step-line {
		top: 16px;
		left: -10px;
		width: 1px;
		height: 100%;
		position: absolute;
		background-color: #ebedf0;
	}

	.step-circle {
		position: absolute;
		top: 17px;
		left: -9px;
		z-index: 2;
		font-size: 12px;
		line-height: 1;
		transform: translate(-50%, -50%);
		width: 7px;
		height: 7px;
		background-color: #a1a1a1;
		border-radius: 50%;
	}

	.right-item::before {
		content: '';
	}

	.step-content {
		padding-top: 2px;
		font-size: 14px;
		color: #828282;
		line-height: 1.5;
	}

	.step-title {
		font-weight: bold;
		padding-top: 3px;
	}

	.step-text {
		font-size: 13px;
		color: #999999;
		padding-top: 6px;
	}

	.step-current {
		// * {
		// 	color: #2f95ff !important;
		// }

		.step-circle {
			background: #2f95ff !important;
		}

		// border-radius: 5px;
		// border: 1px solid #d6eaff;
		font-size: 13px;
		padding-top: 6px;
		// background-color: #eff7ffd9;
		color: black;
	}

	.audit-content {
		// background: #f9f9f9;
		padding: 10px;
		padding-top: 0;
		border-radius: 4px;
		// display: flex;

		.fx-left {
			margin-top: 20rpx;
			flex: 1;
			padding-left: 30rpx;

			.rd {
				display: flex;
				align-items: baseline;
			}
		}

		.fx-right {
			// width: 400px;

			.btn {
				margin-top: 10px;
				text-align: center;
			}
		}

	}

	.cell-item {
		font-weight: 500;
	}

	.desc-top {
		padding: 5px 10px 0 10px;
	}

	.step-item-ad {
		// * {
		// 	color: #9f9898 !important;
		// }
	}

	.f-item {
		padding: 20rpx;
		display: flex;
		color: rgb(48, 49, 51);
		font-size: 26rpx;
		border-bottom: 1px solid #f5f5f5;

		.f-name {
			width: 200rpx;
		}

		.f-value {
			flex: 1;
			text-align: right;
			width: 0;
			word-break: break-all;
			display: flex;
			justify-content: right;
		}
	}


	.step-table {
		margin-top: 16rpx;
		border-top: 1px solid #ebeef5;

	}

	.step-table-item {
		border-left: 1px solid #ebeef5;
		color: #9f9898;
		// padding: 8rpx;
		display: flex;
		font-size: 26rpx;

		.step-table-user,
		.step-table-status {
			width: 160rpx;
		}

		.step-table-date {
			flex: 1;
		}

		.step-cell {
			padding: 10rpx;
			border-bottom: 1px solid #ebeef5;
			border-right: 1px solid #ebeef5;
		}
	}

	.step-table-item:first-child {
		// font-weight: bold;
		color: #665f5f;
		background: #f5f7fa;
	}

	.step-table-text {
		margin-top: 14rpx;
		font-size: 26rpx;
		padding: 8rpx;
	}

	.audit-detail-title {
		font-weight: bold;
		margin: 15px 10px 10px 10px;
	}

	.detail-item {
		background: #fff;
		margin: 10px;
	}

	.v-steps-list {
		padding: 10px;
		background: #ffff;
		//margin-right: 10px;
	}

	.file-item {
		color: #0101ee;
		margin-right: 10px;
	}
</style>