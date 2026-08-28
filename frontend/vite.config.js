import react from '@vitejs/plugin-react'
import { defineConfig } from 'vite'

// The gateway (http://localhost:8080) has no CORS policy configured, and
// that's ApiGateway's call to make, not something this frontend should
// change on its own. Proxying through Vite's dev server sidesteps the
// problem entirely: the browser only ever talks to this same origin
// (http://localhost:5173), and Vite forwards matching requests to the
// gateway server-to-server, where CORS doesn't apply.
const GATEWAY_URL = 'http://localhost:8080'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    proxy: {
      '/api': { target: GATEWAY_URL, changeOrigin: true },
      '/sprints': { target: GATEWAY_URL, changeOrigin: true },
      '/projects': { target: GATEWAY_URL, changeOrigin: true },
      '/attachments': { target: GATEWAY_URL, changeOrigin: true },
    },
  },
})
