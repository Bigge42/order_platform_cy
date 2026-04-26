import { defineConfig } from 'vite'
import uni from '@dcloudio/vite-plugin-uni'
import path from 'node:path'
import { fileURLToPath } from 'node:url'

const rootDir = fileURLToPath(new URL('.', import.meta.url))

export default defineConfig({
  resolve: {
    alias: {
      '@': rootDir,
      uni_modules: path.resolve(rootDir, 'uni_modules')
    }
  },
  plugins: [uni()]
})
