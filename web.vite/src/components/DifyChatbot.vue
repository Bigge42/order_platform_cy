<template>
  <div id="dify-chatbot-container"></div>
</template>

<script>
export default {
  name: 'DifyChatbot',
  mounted() {
    if (typeof window === 'undefined') {
      return
    }

    window.difyChatbotConfig = {
      token: 'k6oPyg1OBBa6TlXs',
      baseUrl: 'http://10.11.10.101',
      inputs: {},
      systemVariables: {},
      userVariables: {},
    }

    const scriptId = 'k6oPyg1OBBa6TlXs'
    const existingScript = document.getElementById(scriptId)

    if (!existingScript) {
      const script = document.createElement('script')
      script.src = 'http://10.11.10.101/embed.min.js'
      script.id = scriptId
      script.defer = true
      document.body.appendChild(script)
    }
  },
}
</script>

<style>
#dify-chatbot-bubble-button {
  background-color: #1c64f2 !important;
}

#dify-chatbot-bubble-window {
  width: 24rem !important;
  height: 40rem !important;
}
</style>
