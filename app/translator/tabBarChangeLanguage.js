//底部导航文字切换
export default function() {
	if (!this.useLang || !this.$ts) {
		return;
	}
	uni.setTabBarItem({
		index: 0,
		text: this.$ts("首页")
	})
	uni.setTabBarItem({
		index: 1,
		text: this.$ts("菜单")
	})
	uni.setTabBarItem({
		index: 2,
		text: this.$ts("审批流程")
	})
	uni.setTabBarItem({
		index: 3,
		text: this.$ts("消息")
	})
	uni.setTabBarItem({
		index: 4,
		text: this.$ts("我的")
	})
}
