import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'

// https://vitejs.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    proxy: {
      '/api': {
        target: 'https://localhost:7148', // Your ASP.NET API URL
        changeOrigin: true,
        secure: false, // Accept self-signed SSL (like dev cert)
        rewrite: (path) => path.replace(/^\/api/, '/api'),
      },
    },
    port: 5173,
  },
});