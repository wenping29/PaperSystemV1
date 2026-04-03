import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react-swc'
import path from 'path'

export default defineConfig({
  plugins: [
    react({
      tsDecorators: true,
    }),
  ],
  resolve: {
    alias: {
      '@': path.resolve(__dirname, './src'),
    },
  },
  build: {
    target: 'es2020',
    minify: 'terser',
    terserOptions: {
      compress: {
        drop_console: true,
        drop_debugger: true,
      },
    },
    rollupOptions: {
      output: {
        manualChunks: {
          'react-vendor': ['react', 'react-dom', 'react-router-dom'],
          'redux-vendor': ['@reduxjs/toolkit', 'react-redux'],
          'antd-vendor': ['antd', '@ant-design/icons'],
          'charts-vendor': ['echarts', 'echarts-for-react'],
          'utils-vendor': ['dayjs', 'axios'],
        },
        chunkFileNames: 'assets/[name]-[hash].js',
        entryFileNames: 'assets/[name]-[hash].js',
        assetFileNames: 'assets/[name]-[hash].[ext]',
      },
    },
    cssCodeSplit: true,
    sourcemap: false,
  },
  server: {
    port: 3001,
    open: true,
    proxy: {
      '/api/v1/auth': {
        target: 'http://localhost:5000',
        changeOrigin: true,
      },
      '/api/v1/users': {
        target: 'http://localhost:5000',  // 认证后端
        changeOrigin: true,
        secure: false,
      },
      '/api/v1/activity-logs': {
        target: 'http://localhost:5000',  // 认证后端
        changeOrigin: true,
        secure: false,
      },
      '/api/v1/works': {
        target: 'http://localhost:5001',  // 认证后端
        changeOrigin: true,
        secure: false,
      },
    },
  },
})
