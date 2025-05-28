<template>
	<view>
		<iframe :src="src" frameborder="0"></iframe>
	</view>
</template>
<script setup>
	import {
		onLoad
	} from '@dcloudio/uni-app'
	import {ref} from 'vue'
	const src = ref('')
	onLoad((options)=>{
		src.value=decodeURIComponent(options.src);
		//console.log(src.value)
	})
</script>
<style scoped>
	iframe{
		position: absolute;
		height: 100%;
		width: 100%;
		overflow: auto;
		border: 0;
	}
</style>