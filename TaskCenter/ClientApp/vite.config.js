import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import VueDevTools from 'vite-plugin-vue-devtools'
import { resolve } from 'path'


export default defineConfig(({ command }) => ({
  // Path-agnostic deployment: build with a relative base so asset URLs resolve
  // against the <base href> the server injects at runtime (from Request.PathBase).
  // This lets the same build run under any IIS sub-path (/TaskCenter/, /Foo/, or
  // the site root) with no rebuild. Dev server stays at root for the /api proxy.
  base: command === 'build' ? './' : '/',
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
