import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react-swc'
import { visualizer } from 'rollup-plugin-visualizer'
import path from 'path'

export default defineConfig({
  plugins: [
    react({
      tsDecorators: true,
    }),
    visualizer({
      open: true,
      filename: 'dist/stats.html',
    }),
  ],
  resolve: {
    alias: {
      '@': path.resolve(__dirname, './src'),
      '@components': path.resolve(__dirname, './src/components'),
      '@core': path.resolve(__dirname, './src/core'),
      '@features': path.resolve(__dirname, './src/features'),
      '@store': path.resolve(__dirname, './src/store'),
      '@styles': path.resolve(__dirname, './src/styles'),
    },
  },
  build: {
    target: 'es2020',
    minify: 'terser',
    terserOptions: {
      compress: {
        drop_console: true,
        drop_debugger: true,
        pure_funcs: ['console.log'],
      },
    },
    assetsInlineLimit: 4096,
    rollupOptions: {
      output: {
        manualChunks: {
          'react-vendor': ['react', 'react-dom', 'react-router-dom'],
          'redux-vendor': ['@reduxjs/toolkit', 'react-redux'],
          'antd-vendor': ['antd', '@ant-design/icons'],
          'echarts-vendor': ['echarts', 'echarts-for-react'],
          'utils-vendor': ['dayjs', 'lodash-es', 'axios'],
        },
        chunkFileNames: 'assets/[name]-[hash].js',
        entryFileNames: 'assets/[name]-[hash].js',
        assetFileNames: 'assets/[name]-[hash].[ext]',
      },
    },
    cssCodeSplit: true,
    sourcemap: false,
  },
  optimizeDeps: {
    include: [
      'react',
      'react-dom',
      'react-router-dom',
      '@reduxjs/toolkit',
      'antd',
      'echarts',
    ],
  },
  server: {
    port: 3000,
    open: true,
    proxy: {
        '/api/v1/auth': {
        target: 'http://8.136.202.27:5100',
        changeOrigin: true,
        secure: false,
      },
      '/api/v1/users': {
        target: 'http://8.136.202.27:5100',  // 认证后端
        changeOrigin: true,
        secure: false,
      },
      '/api/v1/activity-logs': {
        target: 'http://8.136.202.27:5100',  // 认证后端
        changeOrigin: true,
        secure: false,
      },
      '/api/v1/works': {
        target: 'http://8.136.202.27:5101',  // 认证后端
        changeOrigin: true,
        secure: false,
      },
      '/api/v1/comments': {
        target: 'http://8.136.202.27:5102',  // 认证后端
        changeOrigin: true,
        secure: false,
      },
    },
  },
})