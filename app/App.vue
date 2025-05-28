<script>
	export default {
		onLaunch: function() {
			console.log('App Launch')
		},
		onShow: function() {
			console.log('App Show')
		},
		onHide: function() {
			console.log('App Hide')
		}
	}
</script>
<style lang="scss">
	/* 注意要写在第一行，同时给style标签加入lang="scss"属性 */
	@import "@/uni_modules/uview-plus/index.scss";
</style>
<style lang="less">
	@import "@/components/vol.less"; 
</style>
<style>
	/*每个页面公共css */
	.uni-page-body,
	uni-app,
	uni-page,
	page,
	uni-page-wrapper,
	uni-page-body {
		height: 100%;
	} 

	
</style>
