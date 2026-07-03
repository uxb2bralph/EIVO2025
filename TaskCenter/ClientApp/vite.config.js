import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import VueDevTools from 'vite-plugin-vue-devtools'
import { resolve } from 'path'


export default defineConfig(({ command }) => ({
  base: './',
  plugins: [vue(), ...(command === 'serve' ? [VueDevTools()] : [])],
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
        target: 'http://localhost:5050',
        changeOrigin: true,
        secure: false,
      }
    }
  },
  build: {
    sourcemap: true,
    outDir: '../wwwroot',
    emptyOutDir: true
  }
}))
