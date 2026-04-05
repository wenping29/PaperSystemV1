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
        //target: 'http://localhost:5100',
        target: 'http://8.136.202.27:5100',
        changeOrigin: true,
        secure: false,
      },
      '/api/v1/users': {
        //target: 'http://localhost:5100',
        target: 'http://8.136.202.27:5100',
        changeOrigin: true,
        secure: false,
      },
      '/api/v1/activity-logs': {
        //target: 'http://localhost:5100',  // 认证后端
        target: 'http://8.136.202.27:5100',
        changeOrigin: true,
        secure: false,
      },
      '/api/v1/works': {
        //target: 'http://localhost:5101',  // 认证后端
        target: 'http://8.136.202.27:5100',
        changeOrigin: true,
        secure: false,
      },
      '/api/v1/comments': {
        //target: 'http://localhost:5102',  // 认证后端
        target: 'http://8.136.202.27:5100',
        changeOrigin: true,
        secure: false,
      },
    },
  },
})
