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
      // This collides character-for-character with the page route of the
      // same name (/projects/:id/sprints), so the proxy has to tell an
      // XHR/fetch call apart from a full-page navigation (e.g. a hard
      // refresh or a pasted URL landing here) - otherwise it swallows the
      // page request too and React Router never gets a chance to render
      // it. Bypass lets Vite fall through to its SPA index.html for
      // anything asking for text/html. (WorkPackage has no such collision:
      // its real endpoint is /api/workpackage/project/{id}, already
      // covered by the plain /api rule above - /projects/:id/work-packages
      // is only ever a page route, so it needs no proxy entry at all.)
      '^/projects/[^/]+/sprints': {
        target: GATEWAY_URL,
        changeOrigin: true,
        bypass: (req) => (req.headers.accept?.includes('text/html') ? req.url : undefined),
      },
      '/attachments': { target: GATEWAY_URL, changeOrigin: true },
    },
  },
})
