<template>
    <div>
        <button ref="btn" @click="show">test</button>
    </div>
</template>
<script>
export default {
    created() {
     //   window.open('http://localhost:9200/#/chart/preview/16568795341767680','_blank')
      //  window.close();
       // http://localhost:9200/#/chart/preview/16568795341767680'
      //  this.$router.push({ path: '/chart/preview/16568795341767680' })
    },
    methods: {
        show(){
            window.open('http://localhost:9200/#/chart/preview/16568795341767680','_blank')
        }
    },
    mounted() {
        this.$refs.btn.click();
    },
}
</script>