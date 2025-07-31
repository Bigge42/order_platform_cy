<template>
  <svg width="32" height="24" viewBox="0 0 32 24" fill="none" xmlns="http://www.w3.org/2000/svg">
    <path
      :stroke="active ? '#67C23A' : '#C0C4CC'"
      stroke-width="3"
      stroke-linecap="round"
      stroke-linejoin="round"
      d="M4 12h22m0 0l-6-6m6 6l-6 6"
    />
  </svg>
</template>

<script setup>
const props = defineProps({
  active: {
    type: Boolean,
    default: false
  }
})
</script> 