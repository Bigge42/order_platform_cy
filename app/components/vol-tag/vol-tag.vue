<template>
	<view class="vol-tag-content">
	   <!--slot为text时小程序上无动态刷新 -->
		<view @click="click" :class="['vol-tag-'+type]" class="vol-tag">
		</view>
	</view>
</template>

<script setup>
	import {
		ref,
		defineProps,
		defineEmits,
	} from 'vue'
	const props = defineProps({
		type: {
			type: String,
			default: "primary"
		},
		text: ""
	})

	const emit = defineEmits(['click'])
	const click = () => {
		emit('click')
	}
</script>

<style scoped>
	
</style>
