# PaperSystem 管理后台

基于 React + Vite + TypeScript + Ant Design 构建的现代化管理后台系统。

## 技术栈

- **框架**: React 18
- **构建工具**: Vite 5
- **语言**: TypeScript 5
- **UI组件库**: Ant Design 5
- **路由**: React Router v6
- **状态管理**: Zustand
- **HTTP客户端**: Axios
- **图表**: ECharts
- **日期处理**: Day.js

## 功能模块

- [x] 用户认证（登录/登出）
- [x] 仪表板（Dashboard）
- [x] 用户管理
- [ ] 作品管理
- [ ] 评论管理
- [ ] 消息管理
- [ ] 支付管理
- [ ] 系统设置

## 快速开始

### 安装依赖

```bash
npm install
```

### 开发环境运行

```bash
npm run dev
```

开发服务器将在 `http://localhost:3001` 启动

### 生产环境构建

```bash
npm run build
```

### 预览构建结果

```bash
npm run preview
```

## 环境变量

在项目根目录创建 `.env.development` 和 `.env.production` 文件：

```env
# .env.development
VITE_API_BASE_URL=/api/v1
VITE_APP_TITLE=PaperSystem 管理后台
```

```env
# .env.production
VITE_API_BASE_URL=/api/v1
VITE_APP_TITLE=PaperSystem 管理后台
```

## 项目结构

```
apps/admin/
├── public/                 # 静态资源
├── src/
│   ├── components/         # 公共组件
│   │   ├── Layout/         # 布局组件
│   │   └── ProtectedRoute.tsx  # 路由守卫
│   ├── core/               # 核心模块
│   │   ├── api/            # API层
│   │   │   ├── endpoints/  # API端点
│   │   │   └── client.ts   # Axios配置
│   │   ├── config/         # 配置
│   │   ├── constants/      # 常量
│   │   ├── types/          # TypeScript类型
│   │   └── utils/          # 工具函数
│   ├── pages/              # 页面组件
│   │   ├── Login/          # 登录页
│   │   ├── Dashboard/      # 仪表板
│   │   ├── Users/          # 用户管理
│   │   ├── Works/          # 作品管理
│   │   ├── Comments/       # 评论管理
│   │   ├── Messages/       # 消息管理
│   │   ├── Payments/       # 支付管理
│   │   └── Settings/       # 系统设置
│   ├── router/             # 路由配置
│   ├── store/              # 状态管理
│   ├── App.tsx             # 根组件
│   ├── main.tsx            # 应用入口
│   └── index.css           # 全局样式
├── index.html              # HTML模板
├── vite.config.ts          # Vite配置
├── tsconfig.json           # TypeScript配置
└── package.json
```

## 开发规范

- 使用 TypeScript 进行类型检查
- 遵循 React Hooks 最佳实践
- 使用 Ant Design 组件库
- API 调用统一放在 `core/api/` 目录
- 状态管理使用 Zustand

## 后端API

后端服务基于 .NET 8 WebAPI 构建，API 文档请参考后端项目。

主要API端点：

- `POST /api/v1/auth/login` - 用户登录
- `GET /api/v1/auth/me` - 获取当前用户信息
- `GET /api/v1/users` - 获取用户列表
- `GET /api/v1/users/{id}` - 获取用户详情
- `PUT /api/v1/users/{id}` - 更新用户信息
- `DELETE /api/v1/users/{id}` - 删除用户
- `PUT /api/v1/users/{id}/role` - 更新用户角色
