<template></template>
<script>
export default {
    components: {
    },
    data() {
        return {
            show: false
        };
    },
    created() {
        if (!this.$route.query.access_token) { 
            return;
        }
        const access_token =this.$route.query.access_token;
        this.http.post("api/auth/validationToken", { token: access_token }, true).then(x => {
            if (!x.status) {
                this.$message.error(x.message)
                return
            }
            this.$store.commit('setUserInfo', x);
            window.location.href = '/'
        })
    }
};
</script>