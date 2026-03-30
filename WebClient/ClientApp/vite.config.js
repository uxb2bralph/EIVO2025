import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import VueDevTools from 'vite-plugin-vue-devtools'
import { resolve } from 'path'


export default defineConfig({
  base: './',
  plugins: [vue(), VueDevTools()],
  root: '.',
  resolve: {
    alias: {
      '@': resolve(__dirname, 'src')
    }
  },
  server: {
    port: 5173,
    proxy: {
      '/api': {
        target: 'http://localhost:5274',
        changeOrigin: true,
        secure: false,
      }
    }
  },
  build: {
    sourcemap: true
  }
})
