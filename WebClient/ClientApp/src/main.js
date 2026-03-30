// import { createApp } from 'vue'
// import App from './App.vue'

// createApp(App).mount('#app')
import { createApp } from 'vue'
import App from './App.vue'
import { router } from './router/index'

// Bootstrap CSS and JS
import 'bootstrap/dist/css/bootstrap.min.css'
import 'bootstrap/dist/js/bootstrap.bundle.min.js'

createApp(App).use(router).mount('#app')
