# AI写作平台软件架构设计（百万并发版）

## 1. 架构概述

### 1.1 设计目标
- **高并发**：支持百万级并发用户访问
- **高可用**：99.99%可用性，多地域部署
- **可扩展**：水平扩展能力，弹性伸缩
- **高性能**：低延迟响应，快速数据处理
- **安全性**：全方位安全防护，合规性保障

### 1.2 技术栈选型
| 组件 | 技术选型 | 选型理由 |
|------|----------|----------|
| **客户端** | Flutter (Android/iOS) + React (Web) | 跨平台开发，代码复用，高性能渲染 |
| **管理后台** | React + Vite + TypeScript + Ant Design | 开发体验优，构建速度快，组件丰富 |
| **服务端** | C# .NET 8 WebAPI Core | 高性能，高并发支持，微软官方维护 |
| **数据库** | MySQL 8.0 + Vitess(分片) | 成熟稳定，分片方案完善，社区活跃 |
| **缓存** | Redis Cluster 7.0 | 高性能内存数据库，集群支持好 |
| **消息队列** | Apache Kafka | 高吞吐，分布式，持久化 |
| **API网关** | Kong/Nginx + Ocelot | 高性能网关，.NET生态集成 |
| **服务发现** | Consul | 服务注册与发现，健康检查 |
| **容器编排** | Kubernetes | 行业标准，自动伸缩，服务治理 |
| **监控告警** | Prometheus + Grafana + ELK | 全链路监控，日志分析 |
| **对象存储** | MinIO | S3兼容，自建可控 |

## 2. 整体架构图

```
┌─────────────────────────────────────────────────────────────────────────┐
│                              客户端层                                     │
│  ┌────────────┐  ┌────────────┐  ┌────────────┐  ┌────────────┐        │
│  │  Flutter   │  │  Flutter   │  │   React    │  │   React    │        │
│  │   (iOS)    │  │ (Android)  │  │   (Web)    │  │  (Admin)   │        │
│  └────────────┘  └────────────┘  └────────────┘  └────────────┘        │
└─────────────────────────────────────────────────────────────────────────┘
                                    │ HTTPS/WebSocket
┌─────────────────────────────────────────────────────────────────────────┐
│                            CDN & 负载均衡层                               │
│  ┌──────────────────────────────────────────────────────────────────┐   │
│  │                    Cloudflare / 阿里云SLB                          │   │
│  │          全球加速 + DDoS防护 + WAF + SSL卸载                        │   │
│  └──────────────────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────────────┘
                                    │
┌─────────────────────────────────────────────────────────────────────────┐
│                              API网关层                                   │
│  ┌──────────────────────────────────────────────────────────────────┐   │
│  │                        Kong集群 (3节点)                            │   │
│  │  路由分发 │ 限流熔断 │ 认证鉴权 │ 日志记录 │ 缓存代理 │ 监控指标        │   │
│  └──────────────────────────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────────────────────────┘
                                    │
┌─────────────────────────────────────────────────────────────────────────┐
│                          业务服务层（微服务架构）                          │
│  ┌──────────┐  ┌──────────┐  ┌──────────┐  ┌──────────┐  ┌──────────┐  │
│  │  用户服务  │  │ 作品服务  │  │ 社区服务  │  │ AI服务   │  │ 支付服务  │  │
│  │ (.NET 8) │  │ (.NET 8) │  │ (.NET 8) │  │(.NET 8) │  │ (.NET 8) │  │
│  └──────────┘  └──────────┘  └──────────┘  └──────────┘  └──────────┘  │
│  ┌──────────┐  ┌──────────┐  ┌──────────┐  ┌──────────┐  ┌──────────┐  │
│  │ 好友服务  │  │ 聊天服务  │  │ 通知服务  │  │ 文件服务  │  │ 搜索服务  │  │
│  │ (.NET 8) │  │ (.NET 8) │  │ (.NET 8) │  │ (.NET 8) │  │(.NET 8) │  │
│  └──────────┘  └──────────┘  └──────────┘  └──────────┘  └──────────┘  │
└─────────────────────────────────────────────────────────────────────────┘
                                    │
┌─────────────────────────────────────────────────────────────────────────┐
│                           中间件与数据层                                  │
│  ┌──────────┐  ┌──────────┐  ┌──────────┐  ┌──────────┐  ┌──────────┐  │
│  │ MySQL    │  │ Redis    │  │  Kafka   │  │  MinIO   │  │ Consul   │  │
│  │ Cluster  │  │ Cluster  │  │ Cluster  │  │ Cluster  │  │ Cluster  │  │
│  │ (分库分表)│  │(6主6从)  │  │ (3节点)  │  │ (4节点)  │  │ (3节点)  │  │
│  └──────────┘  └──────────┘  └──────────┘  └──────────┘  └──────────┘  │
└─────────────────────────────────────────────────────────────────────────┘
```

## 3. 客户端架构设计

### 3.1 Flutter移动端架构
```
lib/
├── main.dart                    # 应用入口
├── core/                        # 核心层
│   ├── config/                  # 配置管理
│   ├── constants/               # 常量定义
│   ├── di/                      # 依赖注入
│   ├── network/                 # 网络封装
│   │   ├── api_client.dart      # API客户端
│   │   ├── interceptors/        # 拦截器
│   │   └── response_handler.dart# 响应处理
│   └── utils/                   # 工具类
├── data/                        # 数据层
│   ├── models/                  # 数据模型
│   ├── repositories/            # 数据仓库
│   └── datasources/             # 数据源（本地/远程）
├── domain/                      # 领域层
│   ├── entities/                # 领域实体
│   ├── repositories/            # 仓库接口
│   └── usecases/                # 用例
└── presentation/                # 表现层
    ├── pages/                   # 页面
    ├── widgets/                 # 组件
    ├── blocs/                   # 业务逻辑组件（BLoC）
    └── theme/                   # 主题样式
```

#### 关键特性
- **状态管理**：使用BLoC（Business Logic Component）模式
- **网络层**：Dio + 连接池 + 重试机制
- **本地缓存**：Hive/SQLite + LRU策略
- **图片加载**：CachedNetworkImage + 内存缓存
- **推送通知**：Firebase Cloud Messaging

#### 3.1.1 详细架构设计
**架构模式**：Clean Architecture + BLoC
- **表现层 (Presentation Layer)**：UI组件、页面、BLoC状态管理
- **领域层 (Domain Layer)**：业务逻辑、用例、实体
- **数据层 (Data Layer)**：数据仓库、数据源、模型

**技术栈选型**：
| 组件 | 技术选型 | 选型理由 |
|------|----------|----------|
| **Flutter版本** | 3.19+ | 空安全、性能优化、最新功能 |
| **状态管理** | flutter_bloc 8.1+ | 响应式状态管理、测试友好、社区成熟 |
| **网络请求** | Dio 5.3+ | 强大拦截器、连接池、文件上传 |
| **本地存储** | Hive 2.2+ | 高性能NoSQL、零依赖、类型安全 |
| **路由管理** | go_router 12.0+ | 声明式路由、深度链接、路由守卫 |
| **依赖注入** | get_it 7.6+ | 简单可靠、类型安全、配合Injectable |
| **JSON序列化** | json_serializable 6.7+ | 代码生成、类型安全、高性能 |
| **图片加载** | cached_network_image 3.3+ | 内存/磁盘缓存、加载占位符 |
| **推送通知** | firebase_messaging 14.7+ | Firebase生态、跨平台支持 |
| **国际化** | flutter_localizations | 官方支持、Material/Cupertino适配 |
| **主题管理** | flex_color_scheme 7.3+ | 丰富主题配置、动态主题切换 |

**网络层设计**：
```dart
// 核心组件
1. ApiClient：统一API客户端，封装Dio实例
2. RequestInterceptor：请求拦截器（添加Token、设置Header）
3. ResponseInterceptor：响应拦截器（错误处理、日志记录）
4. RetryInterceptor：重试拦截器（网络异常自动重试）
5. ConnectivityInterceptor：网络连接检查拦截器
6. ApiResponse<T>：统一响应包装类（data, error, status）
7. ApiException：自定义API异常类

// 配置项
- 连接超时：30秒
- 接收超时：60秒
- 连接池：最大5个连接，保持活跃60秒
- 重试机制：最多3次，指数退避
```

**状态管理方案**：
```dart
// BLoC模式结构
- Event：用户交互或系统事件
- State：界面状态（loading, success, error, empty）
- Bloc：业务逻辑处理（mapEventToState）

// 状态分类
1. 页面级状态：使用Cubit（简单状态）
2. 复杂业务流：使用Bloc（多事件处理）
3. 全局状态：使用Repository + Stream
4. 表单状态：使用Reactive Forms

// 最佳实践
- 每个功能模块独立的BLoC目录
- 状态不可变性保障
- 事件与状态一对一映射
- 使用Equatable简化状态比较
```

**本地存储策略**：
```dart
// 多级存储体系
1. 内存缓存：使用MemoryCache（LRU策略，最大100条）
2. 本地缓存：Hive NoSQL数据库（用户数据、配置信息）
3. 文件存储：path_provider + 加密存储（敏感信息）
4. 首选项：shared_preferences（简单配置项）

// 数据加密
- 敏感数据使用AES-256加密
- 密钥通过Keychain/Keystore安全存储
- 数据库文件加密（Hive加密盒）
```

**依赖注入架构**：
```dart
// 分层注入
1. 数据层注入：ApiClient、Repository实现
2. 领域层注入：UseCase、Service
3. 表现层注入：BLoC、Controller

// 注入方式
- 环境区分：开发、测试、生产环境配置
- 懒加载：单例模式 + 懒加载初始化
- 作用域：请求级别、页面级别、全局级别
```

**测试策略**：
```dart
// 测试金字塔
1. 单元测试（70%）：BLoC、UseCase、Repository
2. 组件测试（20%）：Widget、页面交互
3. 集成测试（10%）：端到端流程测试

// 测试工具
- flutter_test：官方测试框架
- mockito / mocktail：Mock工具
- bloc_test：BLoC测试工具
- integration_test：集成测试
```

**性能优化措施**：
```dart
// 渲染优化
1. 列表优化：ListView.builder + itemExtent
2. 图片优化：缓存策略 + 分辨率适配
3. 构建优化：const构造函数 + 避免重建
4. 内存优化：Dispose释放资源 + 图片缓存清理

// 包体积优化
- 使用--split-debug-info减少符号表
- 移除未使用资源（图片、字体）
- 代码混淆与压缩
- 按需加载功能模块
```

**打包发布流程**：
```dart
// 环境配置
- 开发环境：调试模式、热重载、日志详细
- 测试环境：Mock API、测试数据
- 生产环境：代码混淆、性能优化

// 发布渠道
1. Android：Google Play + 国内应用商店
2. iOS：App Store
3. 企业分发：内部测试版本

// 自动化流程
- Flutter CI/CD：GitHub Actions
- 自动构建：flutter build apk/ipa
- 代码检查：flutter analyze
- 测试执行：flutter test
```

#### 3.1.2 核心模块设计
**用户认证模块**：
```dart
// 功能要点
1. 登录/注册（手机号、邮箱、第三方）
2. Token管理（自动刷新、安全存储）
3. 会话管理（自动登录、登出清理）
4. 权限验证（角色权限、功能权限）

// 安全措施
- JWT Token自动刷新机制
- 生物识别认证（指纹、面部）
- 设备绑定与异常登录检测
```

**写作编辑器模块**：
```dart
// 编辑器特性
1. 富文本编辑：加粗、斜体、标题、列表
2. AI辅助：实时建议、语法检查、风格优化
3. 实时保存：自动保存草稿、版本管理
4. 多格式导出：Markdown、PDF、Word

// 技术实现
- Flutter Quill富文本编辑器
- 自定义插件扩展
- 实时协作（Operational Transformation）
```

**社区互动模块**：
```dart
// 社交功能
1. 作品展示：瀑布流、分类筛选、搜索
2. 互动功能：点赞、评论、收藏、分享
3. 实时通知：WebSocket推送
4. 打赏系统：支付集成、余额管理

// 性能优化
- 图片懒加载 + 渐进式加载
- 列表分页 + 下拉刷新
- 评论缓存 + 本地存储
```

**实时聊天模块**：
```dart
// 聊天功能
1. 一对一聊天：文字、图片、文件
2. 消息状态：已发送、已送达、已读
3. 历史记录：本地存储 + 云端同步
4. 推送通知：离线消息提醒

// 技术方案
- WebSocket长连接
- 消息队列保证可靠投递
- 本地数据库存储历史
```

#### 3.1.3 跨平台适配
**Android特定优化**：
```dart
// 平台特性
1. 后台服务：消息推送、数据同步
2. 权限管理：运行时权限申请
3. 深色主题：Material Design 3
4. 手势导航：全面屏手势支持
```

**iOS特定优化**：
```dart
// 平台特性
1. UI规范：Cupertino设计语言
2. 隐私保护：ATT框架、隐私清单
3. 灵动岛：实时活动更新
4. 小组件：桌面交互组件
```

**响应式设计**：
```dart
// 适配方案
1. 屏幕适配：基于dp/sp的尺寸系统
2. 横竖屏：自适应布局切换
3. 平板优化：多栏布局、分屏支持
4. 折叠屏：铰链角度检测、布局调整
```

#### 3.1.4 监控与运维
**应用监控**：
```dart
// 监控指标
1. 性能监控：FPS、内存、CPU、启动时间
2. 错误监控：崩溃报告、异常捕获
3. 业务监控：关键路径转化率、用户行为
4. 网络监控：API成功率、响应时间

// 监控工具
- Firebase Crashlytics：崩溃报告
- Firebase Performance：性能监控
- Sentry：错误追踪
- 自定义埋点：业务指标
```

**热更新方案**：
```dart
// 更新策略
1. 应用商店更新：主要版本
2. 热修复：小范围Bug修复
3. 动态配置：服务端控制功能开关
4. A/B测试：功能灰度发布

// 技术实现
- 代码推送：Google Play即时应用
- 资源热更新：Assets动态加载
- 配置中心：远程配置管理
```

### 3.2 React Web端架构
```
src/
├── main.tsx                     # 应用入口
├── App.tsx                      # 根组件
├── core/                        # 核心模块
│   ├── api/                     # API封装
│   ├── config/                  # 配置管理
│   ├── constants/               # 常量定义
│   ├── hooks/                   # 自定义Hooks
│   ├── types/                   # TypeScript类型
│   └── utils/                   # 工具函数
├── features/                    # 功能模块
│   ├── auth/                    # 认证模块
│   ├── writing/                 # 写作模块
│   ├── community/               # 社区模块
│   ├── payment/                 # 支付模块
│   └── profile/                 # 个人中心
├── components/                  # 公共组件
│   ├── common/                  # 通用组件
│   ├── layout/                  # 布局组件
│   └── ui/                      # UI组件
├── store/                       # 状态管理
│   ├── slices/                  # Redux切片
│   └── index.ts                 # Store配置
└── styles/                      # 样式文件
```

#### 关键特性
- **状态管理**：Redux Toolkit + RTK Query
- **路由**：React Router v6 + 懒加载
- **UI组件库**：Ant Design 5.x
- **构建工具**：Vite + SWC（超快构建）
- **代码分割**：动态导入 + 预加载

#### 3.2.1 详细架构设计
**架构模式**：Clean Architecture + 分层设计
- **表现层 (Presentation Layer)**：React组件、页面、路由
- **领域层 (Domain Layer)**：业务逻辑、实体、服务接口
- **应用层 (Application Layer)**：用例、状态管理、API调用
- **基础设施层 (Infrastructure Layer)**：API客户端、本地存储、第三方集成

**项目结构说明**：
```
src/
├── main.tsx                     # 应用入口
├── App.tsx                      # 根组件
├── core/                        # 核心模块
│   ├── api/                     # API封装
│   │   ├── client.ts            # Axios实例配置
│   │   ├── endpoints/           # API端点定义
│   │   └── interceptors/        # 请求响应拦截器
│   ├── config/                  # 配置管理
│   │   ├── app.config.ts        # 应用配置
│   │   └── feature-flags.ts     # 功能开关
│   ├── constants/               # 常量定义
│   │   ├── routes.ts            # 路由常量
│   │   └── storage-keys.ts      # 本地存储键名
│   ├── hooks/                   # 自定义Hooks
│   │   ├── useAuth.ts           # 认证Hook
│   │   ├── useDebounce.ts       # 防抖Hook
│   │   └── useLocalStorage.ts   # 本地存储Hook
│   ├── types/                   # TypeScript类型
│   │   ├── api.types.ts         # API响应类型
│   │   ├── entities.ts          # 实体类型
│   │   └── store.types.ts       # 状态管理类型
│   └── utils/                   # 工具函数
│       ├── formatters.ts        # 格式化工具
│       ├── validators.ts        # 验证工具
│       └── error-handler.ts     # 错误处理工具
├── features/                    # 功能模块
│   ├── auth/                    # 认证模块
│   │   ├── components/          # 认证相关组件
│   │   ├── hooks/               # 认证Hooks
│   │   ├── services/            # 认证服务
│   │   ├── store/               # 认证状态
│   │   └── types/               # 认证类型
│   ├── writing/                 # 写作模块
│   │   ├── components/          # 编辑器组件
│   │   ├── hooks/               # 写作Hooks
│   │   ├── services/            # 写作服务
│   │   ├── store/               # 写作状态
│   │   └── types/               # 写作类型
│   ├── community/               # 社区模块
│   ├── payment/                 # 支付模块
│   └── profile/                 # 个人中心
├── components/                  # 公共组件
│   ├── common/                  # 通用组件
│   │   ├── LoadingSpinner.tsx   # 加载指示器
│   │   ├── ErrorBoundary.tsx    # 错误边界
│   │   └── PageHeader.tsx       # 页面标题
│   ├── layout/                  # 布局组件
│   │   ├── MainLayout.tsx       # 主布局
│   │   ├── AuthLayout.tsx       # 认证布局
│   │   └── AdminLayout.tsx      # 管理后台布局
│   └── ui/                      # UI组件
│       ├── buttons/             # 按钮组件
│       ├── forms/               # 表单组件
│       └── modals/              # 模态框组件
├── store/                       # 状态管理
│   ├── slices/                  # Redux切片
│   │   ├── authSlice.ts         # 认证状态切片
│   │   ├── userSlice.ts         # 用户状态切片
│   │   └── writingSlice.ts      # 写作状态切片
│   ├── api/                     # RTK Query API
│   │   ├── authApi.ts           # 认证API
│   │   ├── userApi.ts           # 用户API
│   │   └── writingApi.ts        # 写作API
│   └── index.ts                 # Store配置
└── styles/                      # 样式文件
    ├── themes/                  # 主题配置
    │   ├── light-theme.ts       # 明亮主题
    │   ├── dark-theme.ts        # 深色主题
    │   └── index.ts             # 主题导出
    ├── globals.css              # 全局样式
    └── antd-custom.less         # Ant Design定制样式
```

**模块化设计原则**：
1. **功能模块化**：每个业务功能独立成模块，包含完整的功能闭环
2. **组件原子化**：遵循原子设计理念，从原子组件到页面模板
3. **依赖单向性**：高层模块不依赖低层模块，通过接口抽象
4. **测试友好**：纯函数组件，易于单元测试和集成测试

**技术栈选型**：
| 组件 | 技术选型 | 版本 | 选型理由 |
|------|----------|------|----------|
| **React框架** | React 18+ | 18.2.0+ | 并发特性、自动批处理、Suspense SSR |
| **TypeScript** | TypeScript 5+ | 5.0.0+ | 类型安全、代码可维护性、编辑器支持 |
| **构建工具** | Vite 5+ | 5.0.0+ | 极速启动、HMR、原生ESM支持 |
| **编译器** | SWC | 1.3.0+ | Rust编写，比Babel快20倍 |
| **路由管理** | React Router v6 | 6.20.0+ | 嵌套路由、数据加载、声明式API |
| **状态管理** | Redux Toolkit + RTK Query | 1.9.0+ | 官方推荐、简化Redux、内置缓存 |
| **UI组件库** | Ant Design 5.x | 5.12.0+ | 企业级组件、主题定制、国际化 |
| **CSS方案** | CSS Modules + Less | - | 局部作用域、Ant Design集成 |
| **HTTP客户端** | Axios | 1.6.0+ | 拦截器、请求取消、浏览器兼容 |
| **表单处理** | React Hook Form | 7.48.0+ | 高性能、非受控组件、验证集成 |
| **数据可视化** | ECharts React | 2.0.0+ | 丰富图表、性能优化、中国开发 |
| **富文本编辑器** | Quill.js + React Quill | 2.0.0+ | 模块化、API丰富、社区活跃 |
| **代码质量** | ESLint + Prettier + Husky | - | 代码规范、自动格式化、Git钩子 |
| **测试框架** | Vitest + React Testing Library | 1.0.0+ | Vite集成、Jest兼容、组件测试 |
| **打包分析** | rollup-plugin-visualizer | 5.10.0+ | 包大小分析、依赖可视化 |
| **错误监控** | Sentry React | 7.80.0+ | 错误追踪、性能监控、源码映射 |

**开发环境配置**：
```json
// package.json scripts部分
{
  "scripts": {
    "dev": "vite",                              // 开发服务器
    "build": "tsc && vite build",               // 生产构建
    "preview": "vite preview",                  // 构建预览
    "lint": "eslint src --ext ts,tsx",          // 代码检查
    "lint:fix": "eslint src --ext ts,tsx --fix",// 自动修复
    "format": "prettier --write \"src/**/*.{ts,tsx,css,less}\"", // 代码格式化
    "test": "vitest",                           // 运行测试
    "test:coverage": "vitest run --coverage",   // 覆盖率测试
    "analyze": "npm run build && vite-bundle-analyzer" // 打包分析
  }
}
```

**构建配置优化**：
```typescript
// vite.config.ts
import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react-swc'
import { visualizer } from 'rollup-plugin-visualizer'

export default defineConfig({
  plugins: [
    react({
      tsDecorators: true, // 支持TypeScript装饰器
    }),
    visualizer({          // 打包分析
      open: true,
      filename: 'dist/stats.html',
    }),
  ],
  resolve: {
    alias: {
      '@': '/src',        // 路径别名
    },
  },
  build: {
    target: 'es2020',     // 目标ES版本
    minify: 'terser',     // 代码压缩
    rollupOptions: {
      output: {
        manualChunks: {   // 手动代码分割
          vendor: ['react', 'react-dom', 'react-router-dom'],
          ui: ['antd', '@ant-design/icons'],
          charts: ['echarts', 'echarts-for-react'],
        },
        chunkFileNames: 'assets/[name]-[hash].js',
        entryFileNames: 'assets/[name]-[hash].js',
        assetFileNames: 'assets/[name]-[hash].[ext]',
      },
    },
    cssCodeSplit: true,   // CSS代码分割
    sourcemap: false,     // 生产环境关闭sourcemap
  },
  server: {
    port: 3000,           // 开发服务器端口
    open: true,           // 自动打开浏览器
    proxy: {              // API代理
      '/api': {
        target: 'http://localhost:5000',
        changeOrigin: true,
      },
    },
  },
})
```

**状态管理方案**：

**Redux Toolkit + RTK Query架构**：
```typescript
// store/index.ts - Store配置
import { configureStore } from '@reduxjs/toolkit'
import { setupListeners } from '@reduxjs/toolkit/query'
import { authApi } from './api/authApi'
import { userApi } from './api/userApi'
import { writingApi } from './api/writingApi'
import authReducer from './slices/authSlice'
import userReducer from './slices/userSlice'
import writingReducer from './slices/writingSlice'

export const store = configureStore({
  reducer: {
    [authApi.reducerPath]: authApi.reducer,
    [userApi.reducerPath]: userApi.reducer,
    [writingApi.reducerPath]: writingApi.reducer,
    auth: authReducer,
    user: userReducer,
    writing: writingReducer,
  },
  middleware: (getDefaultMiddleware) =>
    getDefaultMiddleware().concat(
      authApi.middleware,
      userApi.middleware,
      writingApi.middleware
    ),
  devTools: process.env.NODE_ENV !== 'production',
})

setupListeners(store.dispatch)

export type RootState = ReturnType<typeof store.getState>
export type AppDispatch = typeof store.dispatch
```

**RTK Query API设计**：
```typescript
// store/api/authApi.ts - 认证API
import { createApi, fetchBaseQuery } from '@reduxjs/toolkit/query/react'

export const authApi = createApi({
  reducerPath: 'authApi',
  baseQuery: fetchBaseQuery({
    baseUrl: '/api/v1',
    prepareHeaders: (headers, { getState }) => {
      const token = (getState() as RootState).auth.token
      if (token) {
        headers.set('Authorization', `Bearer ${token}`)
      }
      return headers
    },
  }),
  tagTypes: ['Auth', 'User'],
  endpoints: (builder) => ({
    login: builder.mutation<LoginResponse, LoginRequest>({
      query: (credentials) => ({
        url: '/auth/login',
        method: 'POST',
        body: credentials,
      }),
      invalidatesTags: ['Auth'],
    }),
    register: builder.mutation<RegisterResponse, RegisterRequest>({
      query: (userData) => ({
        url: '/auth/register',
        method: 'POST',
        body: userData,
      }),
    }),
    logout: builder.mutation<void, void>({
      query: () => ({
        url: '/auth/logout',
        method: 'POST',
      }),
      invalidatesTags: ['Auth'],
    }),
    getProfile: builder.query<UserProfile, void>({
      query: () => '/auth/profile',
      providesTags: ['User'],
    }),
    refreshToken: builder.mutation<RefreshTokenResponse, void>({
      query: () => ({
        url: '/auth/refresh',
        method: 'POST',
      }),
    }),
  }),
})

export const {
  useLoginMutation,
  useRegisterMutation,
  useLogoutMutation,
  useGetProfileQuery,
  useRefreshTokenMutation,
} = authApi
```

**状态切片设计**：
```typescript
// store/slices/authSlice.ts - 认证状态切片
import { createSlice, PayloadAction } from '@reduxjs/toolkit'

interface AuthState {
  token: string | null
  refreshToken: string | null
  isAuthenticated: boolean
  isLoading: boolean
  error: string | null
  user: UserProfile | null
}

const initialState: AuthState = {
  token: localStorage.getItem('token'),
  refreshToken: localStorage.getItem('refreshToken'),
  isAuthenticated: !!localStorage.getItem('token'),
  isLoading: false,
  error: null,
  user: null,
}

const authSlice = createSlice({
  name: 'auth',
  initialState,
  reducers: {
    setCredentials: (state, action: PayloadAction<{ token: string; refreshToken: string }>) => {
      const { token, refreshToken } = action.payload
      state.token = token
      state.refreshToken = refreshToken
      state.isAuthenticated = true
      localStorage.setItem('token', token)
      localStorage.setItem('refreshToken', refreshToken)
    },
    clearCredentials: (state) => {
      state.token = null
      state.refreshToken = null
      state.isAuthenticated = false
      state.user = null
      localStorage.removeItem('token')
      localStorage.removeItem('refreshToken')
    },
    setUser: (state, action: PayloadAction<UserProfile>) => {
      state.user = action.payload
    },
    setLoading: (state, action: PayloadAction<boolean>) => {
      state.isLoading = action.payload
    },
    setError: (state, action: PayloadAction<string | null>) => {
      state.error = action.payload
    },
  },
  extraReducers: (builder) => {
    builder
      .addMatcher(authApi.endpoints.login.matchPending, (state) => {
        state.isLoading = true
        state.error = null
      })
      .addMatcher(authApi.endpoints.login.matchFulfilled, (state, action) => {
        state.isLoading = false
        state.isAuthenticated = true
        state.token = action.payload.token
        state.refreshToken = action.payload.refreshToken
      })
      .addMatcher(authApi.endpoints.login.matchRejected, (state, action) => {
        state.isLoading = false
        state.error = action.error.message || '登录失败'
      })
  },
})

export const { setCredentials, clearCredentials, setUser, setLoading, setError } = authSlice.actions
export default authSlice.reducer
```

**自定义Hook封装**：
```typescript
// hooks/useAuth.ts - 认证Hook
import { useCallback } from 'react'
import { useDispatch, useSelector } from 'react-redux'
import { useLoginMutation, useLogoutMutation, useGetProfileQuery } from '../store/api/authApi'
import { clearCredentials, setCredentials, setUser } from '../store/slices/authSlice'
import type { RootState, AppDispatch } from '../store'

export const useAuth = () => {
  const dispatch = useDispatch<AppDispatch>()
  const [loginMutation] = useLoginMutation()
  const [logoutMutation] = useLogoutMutation()
  const { data: profile, refetch: refetchProfile } = useGetProfileQuery()
  
  const authState = useSelector((state: RootState) => state.auth)
  
  const login = useCallback(async (credentials: LoginRequest) => {
    try {
      const response = await loginMutation(credentials).unwrap()
      dispatch(setCredentials(response))
      return response
    } catch (error) {
      throw error
    }
  }, [dispatch, loginMutation])
  
  const logout = useCallback(async () => {
    try {
      await logoutMutation().unwrap()
    } finally {
      dispatch(clearCredentials())
    }
  }, [dispatch, logoutMutation])
  
  const updateProfile = useCallback((profile: UserProfile) => {
    dispatch(setUser(profile))
  }, [dispatch])
  
  return {
    ...authState,
    profile,
    login,
    logout,
    updateProfile,
    refetchProfile,
  }
}
```

**状态管理最佳实践**：
1. **单一数据源**：所有应用状态集中存储在Redux Store中
2. **状态不可变性**：使用Immer自动处理不可变更新
3. **选择器优化**：使用Reselect或createSelector记忆化选择器
4. **异步状态**：RTK Query自动管理加载、成功、错误状态
5. **本地状态**：简单的组件内部状态使用useState，复杂业务状态用Redux
6. **持久化策略**：
   - Token等敏感信息：localStorage + 加密
   - 用户偏好：localStorage + 自动同步
   - 表单草稿：SessionStorage + 自动保存
7. **性能优化**：
   - 组件按需订阅状态变化
   - 批量更新避免重复渲染
   - 使用React.memo + useMemo + useCallback

**缓存策略配置**：
```typescript
// RTK Query缓存配置
export const writingApi = createApi({
  reducerPath: 'writingApi',
  baseQuery: fetchBaseQuery({ baseUrl: '/api/v1' }),
  tagTypes: ['Works', 'Templates', 'Drafts'],
  keepUnusedDataFor: 60, // 未使用数据保留60秒
  refetchOnMountOrArgChange: 30, // 30秒后重新获取
  refetchOnFocus: true, // 窗口聚焦时重新获取
  refetchOnReconnect: true, // 网络重连时重新获取
  endpoints: (builder) => ({
    getWorks: builder.query<Work[], PaginationParams>({
      query: (params) => ({ url: '/works', params }),
      providesTags: (result) =>
        result
          ? [...result.map(({ id }) => ({ type: 'Works' as const, id })), 'Works']
          : ['Works'],
      transformResponse: (response: ApiResponse<Work[]>) => response.data,
    }),
    // ...其他endpoints
  }),
})
```

**网络层设计**：

**Axios实例配置与拦截器**：
```typescript
// core/api/client.ts - Axios实例配置
import axios, { AxiosError, AxiosRequestConfig, AxiosResponse, InternalAxiosRequestConfig } from 'axios'
import { store } from '../../store'
import { clearCredentials, setCredentials } from '../../store/slices/authSlice'
import { refreshToken } from '../../store/api/authApi'

// 创建axios实例
const axiosInstance = axios.create({
  baseURL: import.meta.env.VITE_API_BASE_URL || '/api',
  timeout: 30000, // 30秒超时
  headers: {
    'Content-Type': 'application/json',
    'Accept': 'application/json',
  },
})

// 请求拦截器
axiosInstance.interceptors.request.use(
  (config: InternalAxiosRequestConfig) => {
    const token = store.getState().auth.token
    
    if (token) {
      config.headers.Authorization = `Bearer ${token}`
    }
    
    // 添加请求ID用于追踪
    config.headers['X-Request-ID'] = crypto.randomUUID()
    
    // 开发环境添加调试头
    if (import.meta.env.DEV) {
      config.headers['X-Debug-Mode'] = 'true'
    }
    
    return config
  },
  (error) => {
    return Promise.reject(error)
  }
)

// 响应拦截器
axiosInstance.interceptors.response.use(
  (response: AxiosResponse) => {
    // 统一响应格式处理
    const { data } = response
    if (data?.code !== undefined && data.code !== 0) {
      return Promise.reject(new Error(data.message || '请求失败'))
    }
    return data?.data ?? data
  },
  async (error: AxiosError) => {
    const originalRequest = error.config as InternalAxiosRequestConfig & { _retry?: boolean }
    
    // 401错误且不是刷新token请求，尝试刷新token
    if (error.response?.status === 401 && !originalRequest._retry && !originalRequest.url?.includes('/auth/refresh')) {
      originalRequest._retry = true
      
      try {
        // 调用刷新token接口
        const refreshResult = await store.dispatch(refreshToken()).unwrap()
        
        // 更新store中的token
        store.dispatch(setCredentials(refreshResult))
        
        // 重试原始请求
        originalRequest.headers.Authorization = `Bearer ${refreshResult.token}`
        return axiosInstance(originalRequest)
      } catch (refreshError) {
        // 刷新token失败，清空认证状态并跳转到登录页
        store.dispatch(clearCredentials())
        window.location.href = '/login'
        return Promise.reject(refreshError)
      }
    }
    
    // 其他错误处理
    const errorMessage = getErrorMessage(error)
    console.error('API请求错误:', errorMessage)
    
    // 统一错误处理
    if (error.response?.status === 403) {
      // 权限不足
      console.warn('权限不足，访问被拒绝')
    } else if (error.response?.status === 429) {
      // 请求过于频繁
      console.warn('请求过于频繁，请稍后重试')
    } else if (!error.response) {
      // 网络错误
      console.warn('网络连接失败，请检查网络设置')
    }
    
    return Promise.reject(error)
  }
)

// 错误消息提取函数
function getErrorMessage(error: AxiosError): string {
  if (error.response?.data) {
    const data = error.response.data as any
    return data.message || data.error || '请求失败'
  }
  if (error.request) {
    return '网络请求失败，请检查网络连接'
  }
  return error.message || '未知错误'
}

export default axiosInstance
```

**API服务层封装**：
```typescript
// core/api/endpoints/authEndpoints.ts - 认证API端点
import axiosInstance from '../client'

export interface LoginRequest {
  username: string
  password: string
}

export interface LoginResponse {
  token: string
  refreshToken: string
  user: UserProfile
}

export interface RegisterRequest {
  username: string
  email: string
  password: string
}

export interface RegisterResponse {
  message: string
  userId: number
}

export const authApi = {
  // 登录
  login: async (credentials: LoginRequest): Promise<LoginResponse> => {
    const response = await axiosInstance.post<ApiResponse<LoginResponse>>('/auth/login', credentials)
    return response.data
  },
  
  // 注册
  register: async (userData: RegisterRequest): Promise<RegisterResponse> => {
    const response = await axiosInstance.post<ApiResponse<RegisterResponse>>('/auth/register', userData)
    return response.data
  },
  
  // 登出
  logout: async (): Promise<void> => {
    await axiosInstance.post('/auth/logout')
  },
  
  // 获取用户资料
  getProfile: async (): Promise<UserProfile> => {
    const response = await axiosInstance.get<ApiResponse<UserProfile>>('/auth/profile')
    return response.data
  },
  
  // 刷新token
  refreshToken: async (): Promise<LoginResponse> => {
    const response = await axiosInstance.post<ApiResponse<LoginResponse>>('/auth/refresh')
    return response.data
  },
}

// 统一的API响应类型
export interface ApiResponse<T = any> {
  code: number
  message: string
  data: T
  timestamp: number
}

// 分页参数类型
export interface PaginationParams {
  page?: number
  pageSize?: number
  sortBy?: string
  sortOrder?: 'asc' | 'desc'
}

// 分页响应类型
export interface PaginatedResponse<T> {
  items: T[]
  total: number
  page: number
  pageSize: number
  totalPages: number
}
```

**WebSocket连接管理**：
```typescript
// core/api/websocket.ts - WebSocket客户端
class WebSocketClient {
  private ws: WebSocket | null = null
  private reconnectAttempts = 0
  private maxReconnectAttempts = 5
  private reconnectDelay = 1000
  private messageHandlers: Map<string, Function[]> = new Map()
  private connectionPromise: Promise<void> | null = null
  
  constructor(private url: string) {}
  
  // 建立连接
  async connect(): Promise<void> {
    if (this.connectionPromise) {
      return this.connectionPromise
    }
    
    this.connectionPromise = new Promise((resolve, reject) => {
      try {
        const token = localStorage.getItem('token')
        const wsUrl = `${this.url}?token=${encodeURIComponent(token || '')}`
        
        this.ws = new WebSocket(wsUrl)
        
        this.ws.onopen = () => {
          console.log('WebSocket连接已建立')
          this.reconnectAttempts = 0
          resolve()
        }
        
        this.ws.onclose = (event) => {
          console.log('WebSocket连接关闭:', event.code, event.reason)
          this.ws = null
          this.connectionPromise = null
          
          // 自动重连
          if (this.reconnectAttempts < this.maxReconnectAttempts) {
            setTimeout(() => {
              this.reconnectAttempts++
              console.log(`尝试重连... (${this.reconnectAttempts}/${this.maxReconnectAttempts})`)
              this.connect()
            }, this.reconnectDelay * this.reconnectAttempts)
          }
        }
        
        this.ws.onerror = (error) => {
          console.error('WebSocket错误:', error)
          reject(error)
        }
        
        this.ws.onmessage = (event) => {
          try {
            const message = JSON.parse(event.data)
            this.handleMessage(message)
          } catch (error) {
            console.error('消息解析失败:', error)
          }
        }
      } catch (error) {
        reject(error)
      }
    })
    
    return this.connectionPromise
  }
  
  // 发送消息
  send(type: string, data?: any): boolean {
    if (!this.ws || this.ws.readyState !== WebSocket.OPEN) {
      console.warn('WebSocket未连接，无法发送消息')
      return false
    }
    
    try {
      const message = JSON.stringify({ type, data, timestamp: Date.now() })
      this.ws.send(message)
      return true
    } catch (error) {
      console.error('发送消息失败:', error)
      return false
    }
  }
  
  // 订阅消息
  subscribe(type: string, handler: Function): () => void {
    if (!this.messageHandlers.has(type)) {
      this.messageHandlers.set(type, [])
    }
    
    const handlers = this.messageHandlers.get(type)!
    handlers.push(handler)
    
    // 返回取消订阅函数
    return () => {
      const index = handlers.indexOf(handler)
      if (index > -1) {
        handlers.splice(index, 1)
      }
    }
  }
  
  // 处理消息
  private handleMessage(message: any) {
    const { type, data } = message
    const handlers = this.messageHandlers.get(type)
    
    if (handlers) {
      handlers.forEach(handler => {
        try {
          handler(data)
        } catch (error) {
          console.error(`消息处理器错误 (${type}):`, error)
        }
      })
    }
  }
  
  // 断开连接
  disconnect(): void {
    if (this.ws) {
      this.ws.close(1000, '正常关闭')
      this.ws = null
    }
    this.connectionPromise = null
    this.messageHandlers.clear()
  }
  
  // 获取连接状态
  get isConnected(): boolean {
    return this.ws?.readyState === WebSocket.OPEN
  }
}

// 全局WebSocket实例
export const chatWebSocket = new WebSocketClient(import.meta.env.VITE_WS_URL || 'ws://localhost:5000/ws/chat')
export const notificationWebSocket = new WebSocketClient(import.meta.env.VITE_WS_URL || 'ws://localhost:5000/ws/notifications')
```

**网络请求最佳实践**：
1. **请求重试机制**：重要请求自动重试，指数退避策略
2. **请求取消**：页面卸载时取消未完成的请求
3. **请求去重**：相同请求合并，避免重复请求
4. **离线处理**：网络异常时缓存请求，恢复后自动重试
5. **性能监控**：记录请求耗时，识别性能瓶颈
6. **安全防护**：CSRF Token、XSS防护、请求签名

**性能优化策略**：
```typescript
// 请求缓存策略
const cache = new Map<string, { data: any; timestamp: number; ttl: number }>()

export const cachedRequest = async <T>(
  key: string,
  requestFn: () => Promise<T>,
  ttl: number = 300000 // 默认5分钟
): Promise<T> => {
  const cached = cache.get(key)
  const now = Date.now()
  
  // 检查缓存是否有效
  if (cached && now - cached.timestamp < cached.ttl) {
    console.log(`缓存命中: ${key}`)
    return cached.data as T
  }
  
  // 执行请求并缓存结果
  console.log(`缓存未命中: ${key}`)
  try {
    const data = await requestFn()
    cache.set(key, { data, timestamp: now, ttl })
    return data
  } catch (error) {
    // 请求失败时，如果缓存存在且未过期，返回缓存数据
    if (cached && now - cached.timestamp < cached.ttl * 2) {
      console.warn(`请求失败，返回缓存数据: ${key}`)
      return cached.data as T
    }
    throw error
  }
}

// 请求队列管理（防止并发请求过多）
class RequestQueue {
  private queue: Array<() => Promise<any>> = []
  private concurrent = 0
  private maxConcurrent = 6
  
  async add<T>(requestFn: () => Promise<T>): Promise<T> {
    return new Promise((resolve, reject) => {
      const task = async () => {
        try {
          const result = await requestFn()
          resolve(result)
        } catch (error) {
          reject(error)
        } finally {
          this.concurrent--
          this.processQueue()
        }
      }
      
      this.queue.push(task)
      this.processQueue()
    })
  }
  
  private processQueue() {
    while (this.queue.length > 0 && this.concurrent < this.maxConcurrent) {
      const task = this.queue.shift()
      if (task) {
        this.concurrent++
        task()
      }
    }
  }
}

export const requestQueue = new RequestQueue()
```

**性能优化措施**：

**渲染性能优化**：
```typescript
// 1. 组件懒加载
const HomePage = lazy(() => import('@/pages/HomePage'))
const WritingEditor = lazy(() => import('@/features/writing/components/WritingEditor'))
const UserProfile = lazy(() => import('@/features/profile/components/UserProfile'))

// 2. 图片懒加载组件
const LazyImage: React.FC<LazyImageProps> = ({ src, alt, width, height }) => {
  const [isLoaded, setIsLoaded] = useState(false)
  const imgRef = useRef<HTMLImageElement>(null)

  useEffect(() => {
    if (!imgRef.current) return

    const observer = new IntersectionObserver(
      (entries) => {
        entries.forEach((entry) => {
          if (entry.isIntersecting) {
            const img = entry.target as HTMLImageElement
            img.src = img.dataset.src || ''
            observer.unobserve(img)
          }
        })
      },
      { rootMargin: '50px' }
    )

    observer.observe(imgRef.current)
    return () => observer.disconnect()
  }, [])

  return (
    <img
      ref={imgRef}
      data-src={src}
      alt={alt}
      width={width}
      height={height}
      className={`lazy-image ${isLoaded ? 'loaded' : 'loading'}`}
      onLoad={() => setIsLoaded(true)}
      loading="lazy"
    />
  )
}

// 3. 虚拟列表组件
const VirtualList: React.FC<VirtualListProps> = ({ items, itemHeight, renderItem }) => {
  const containerRef = useRef<HTMLDivElement>(null)
  const [visibleRange, setVisibleRange] = useState({ start: 0, end: 20 })

  useEffect(() => {
    const container = containerRef.current
    if (!container) return

    const handleScroll = () => {
      const scrollTop = container.scrollTop
      const visibleHeight = container.clientHeight
      const start = Math.floor(scrollTop / itemHeight)
      const end = Math.ceil((scrollTop + visibleHeight) / itemHeight)

      setVisibleRange({ start, end })
    }

    container.addEventListener('scroll', handleScroll)
    handleScroll() // 初始计算

    return () => container.removeEventListener('scroll', handleScroll)
  }, [itemHeight])

  const totalHeight = items.length * itemHeight
  const visibleItems = items.slice(visibleRange.start, visibleRange.end)

  return (
    <div ref={containerRef} className="virtual-list-container" style={{ height: '100%', overflow: 'auto' }}>
      <div style={{ height: totalHeight, position: 'relative' }}>
        {visibleItems.map((item, index) => {
          const actualIndex = visibleRange.start + index
          return (
            <div
              key={item.id}
              style={{
                position: 'absolute',
                top: actualIndex * itemHeight,
                height: itemHeight,
                width: '100%',
              }}
            >
              {renderItem(item, actualIndex)}
            </div>
          )
        })}
      </div>
    </div>
  )
}
```

**包体积优化**：
```typescript
// 1. 动态导入第三方库
const loadECharts = async () => {
  const echarts = await import('echarts/core')
  const { BarChart, LineChart, PieChart } = await import('echarts/charts')
  const { GridComponent, TooltipComponent, LegendComponent } = await import('echarts/components')
  const { CanvasRenderer } = await import('echarts/renderers')
  
  echarts.use([
    BarChart,
    LineChart,
    PieChart,
    GridComponent,
    TooltipComponent,
    LegendComponent,
    CanvasRenderer,
  ])
  
  return echarts
}

// 2. 按需加载Ant Design组件
export { Button, Input, Form, Table } from 'antd'

// 3. 代码分割配置 (vite.config.ts)
export default defineConfig({
  build: {
    rollupOptions: {
      output: {
        manualChunks: {
          'react-vendor': ['react', 'react-dom', 'react-router-dom'],
          'redux-vendor': ['@reduxjs/toolkit', 'react-redux'],
          'antd-vendor': ['antd', '@ant-design/icons'],
          'echarts-vendor': ['echarts', 'echarts-for-react'],
          'utils-vendor': ['dayjs', 'lodash-es', 'axios'],
        },
      },
    },
  },
})
```

**内存优化**：
```typescript
// 1. 清理副作用
const useInterval = (callback: () => void, delay: number | null) => {
  const savedCallback = useRef<() => void>()

  useEffect(() => {
    savedCallback.current = callback
  }, [callback])

  useEffect(() => {
    if (delay === null) return

    const tick = () => savedCallback.current?.()
    const id = setInterval(tick, delay)
    return () => clearInterval(id)
  }, [delay])
}

// 2. 防抖与节流
const useDebounce = <T>(value: T, delay: number): T => {
  const [debouncedValue, setDebouncedValue] = useState<T>(value)

  useEffect(() => {
    const timer = setTimeout(() => setDebouncedValue(value), delay)
    return () => clearTimeout(timer)
  }, [value, delay])

  return debouncedValue
}

const useThrottle = <T>(value: T, limit: number): T => {
  const [throttledValue, setThrottledValue] = useState<T>(value)
  const lastRan = useRef<number>(Date.now())

  useEffect(() => {
    const handler = setTimeout(() => {
      if (Date.now() - lastRan.current >= limit) {
        setThrottledValue(value)
        lastRan.current = Date.now()
      }
    }, limit - (Date.now() - lastRan.current))

    return () => clearTimeout(handler)
  }, [value, limit])

  return throttledValue
}
```

**构建优化**：
```typescript
// vite.config.ts - 构建优化配置
export default defineConfig({
  build: {
    // 启用 terser 压缩
    minify: 'terser',
    terserOptions: {
      compress: {
        drop_console: true,           // 移除console
        drop_debugger: true,          // 移除debugger
        pure_funcs: ['console.log'],  // 移除特定函数调用
      },
    },
    
    // 资源压缩
    assetsInlineLimit: 4096,          // 小于4KB的图片转为base64
    
    // 输出配置
    rollupOptions: {
      output: {
        // 文件名添加hash
        entryFileNames: 'assets/[name]-[hash].js',
        chunkFileNames: 'assets/[name]-[hash].js',
        assetFileNames: 'assets/[name]-[hash].[ext]',
        
        // 优化分包
        manualChunks(id) {
          if (id.includes('node_modules')) {
            if (id.includes('react')) {
              return 'vendor-react'
            }
            if (id.includes('antd')) {
              return 'vendor-antd'
            }
            if (id.includes('echarts')) {
              return 'vendor-charts'
            }
            return 'vendor-others'
          }
        },
      },
    },
  },
  
  // 预构建依赖
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
})
```

**性能监控**：
```typescript
// 性能监控工具
class PerformanceMonitor {
  private metrics: Map<string, number> = new Map()
  private observers: PerformanceObserver[] = []
  
  constructor() {
    this.setupObservers()
  }
  
  private setupObservers() {
    // 监控长任务
    const longTaskObserver = new PerformanceObserver((list) => {
      for (const entry of list.getEntries()) {
        if (entry.duration > 50) {
          console.warn('长任务检测:', entry)
          this.reportMetric('long_task', entry.duration)
        }
      }
    })
    longTaskObserver.observe({ entryTypes: ['longtask'] })
    this.observers.push(longTaskObserver)
    
    // 监控布局偏移
    const layoutShiftObserver = new PerformanceObserver((list) => {
      for (const entry of list.getEntries()) {
        console.warn('布局偏移:', entry)
        this.reportMetric('layout_shift', (entry as any).value)
      }
    })
    layoutShiftObserver.observe({ entryTypes: ['layout-shift'] })
    this.observers.push(layoutShiftObserver)
  }
  
  // 测量函数执行时间
  measure(name: string, fn: () => any): any {
    const start = performance.now()
    const result = fn()
    const duration = performance.now() - start
    
    this.reportMetric(name, duration)
    
    if (duration > 100) {
      console.warn(`函数 ${name} 执行时间过长: ${duration.toFixed(2)}ms`)
    }
    
    return result
  }
  
  private reportMetric(name: string, value: number) {
    this.metrics.set(name, (this.metrics.get(name) || 0) + 1)
    
    // 上报到监控系统
    if (typeof window.gtag === 'function') {
      window.gtag('event', 'performance_metric', {
        metric_name: name,
        metric_value: value,
      })
    }
  }
  
  // 获取性能报告
  getReport() {
    return {
      metrics: Object.fromEntries(this.metrics),
      navigation: performance.getEntriesByType('navigation')[0],
      paint: performance.getEntriesByType('paint'),
      resource: performance.getEntriesByType('resource'),
    }
  }
  
  // 清理
  disconnect() {
    this.observers.forEach(observer => observer.disconnect())
  }
}

// 全局性能监控实例
export const perfMonitor = new PerformanceMonitor()

// React性能监控高阶组件
export function withPerformanceMonitor<P extends object>(
  WrappedComponent: React.ComponentType<P>,
  componentName: string
) {
  return function WithPerformanceMonitor(props: P) {
    const mountedRef = useRef(false)
    
    useEffect(() => {
      if (!mountedRef.current) {
        const mountTime = performance.now()
        perfMonitor.reportMetric(`${componentName}_mount`, mountTime)
        mountedRef.current = true
        
        return () => {
          const unmountTime = performance.now()
          perfMonitor.reportMetric(`${componentName}_unmount`, unmountTime)
        }
      }
    }, [])
    
    return <WrappedComponent {...props} />
  }
}
```

**缓存策略**：
```typescript
// 多级缓存系统
class CacheManager {
  private memoryCache = new Map<string, { data: any; expiry: number }>()
  private localStoragePrefix = 'app_cache_'
  
  // 内存缓存（短期）
  setMemory(key: string, data: any, ttl: number = 300000): void {
    this.memoryCache.set(key, {
      data,
      expiry: Date.now() + ttl,
    })
  }
  
  getMemory<T>(key: string): T | null {
    const cached = this.memoryCache.get(key)
    if (!cached || Date.now() > cached.expiry) {
      this.memoryCache.delete(key)
      return null
    }
    return cached.data as T
  }
  
  // 本地存储缓存（长期）
  setLocal(key: string, data: any, ttl: number = 86400000): void {
    try {
      const cacheKey = `${this.localStoragePrefix}${key}`
      const cacheData = {
        data,
        expiry: Date.now() + ttl,
        version: '1.0',
      }
      localStorage.setItem(cacheKey, JSON.stringify(cacheData))
    } catch (error) {
      console.warn('本地存储缓存失败:', error)
    }
  }
  
  getLocal<T>(key: string): T | null {
    try {
      const cacheKey = `${this.localStoragePrefix}${key}`
      const cached = localStorage.getItem(cacheKey)
      if (!cached) return null
      
      const parsed = JSON.parse(cached)
      if (Date.now() > parsed.expiry) {
        localStorage.removeItem(cacheKey)
        return null
      }
      
      return parsed.data as T
    } catch (error) {
      console.warn('本地存储读取失败:', error)
      return null
    }
  }
  
  // 清理过期缓存
  cleanup(): void {
    // 清理内存缓存
    const now = Date.now()
    for (const [key, value] of this.memoryCache.entries()) {
      if (now > value.expiry) {
        this.memoryCache.delete(key)
      }
    }
    
    // 清理本地存储缓存
    for (let i = 0; i < localStorage.length; i++) {
      const key = localStorage.key(i)
      if (key?.startsWith(this.localStoragePrefix)) {
        try {
          const cached = localStorage.getItem(key)
          if (cached) {
            const parsed = JSON.parse(cached)
            if (now > parsed.expiry) {
              localStorage.removeItem(key)
            }
          }
        } catch (error) {
          // 忽略无效数据
        }
      }
    }
  }
}

export const cacheManager = new CacheManager()
```

**最佳实践总结**：
1. **首屏加载优化**：
   - 代码分割 + 懒加载
   - 资源预加载 + 预连接
   - 关键CSS内联
   - 图片优化（WebP格式 + 尺寸适配）

2. **运行时性能**：
   - 虚拟列表处理大数据
   - 防抖节流避免重复渲染
   - 组件记忆化（React.memo, useMemo, useCallback）
   - 状态管理优化（选择性订阅）

3. **包体积控制**：
   - 按需导入第三方库
   - 移除未使用代码（Tree Shaking）
   - 压缩混淆代码
   - 使用现代ES语法

4. **缓存策略**：
   - HTTP缓存头配置
   - Service Worker离线缓存
   - 内存缓存热点数据
   - 本地存储用户数据

5. **监控与告警**：
   - 关键性能指标监控（FCP, LCP, FID, CLS）
   - 错误收集与上报
   - 用户行为分析
   - 自动化性能测试

**测试策略**：

**测试金字塔与工具链**：
```
测试金字塔
┌─────────────────────────────────┐
│       端到端测试 (10%)           │
│  ┌───────────────────────────┐  │
│  │     集成测试 (20%)         │  │
│  │  ┌─────────────────────┐  │  │
│  │  │   单元测试 (70%)     │  │  │
│  │  └─────────────────────┘  │  │
│  └───────────────────────────┘  │
└─────────────────────────────────┘

工具链：
├── 单元测试：Vitest + React Testing Library
├── 集成测试：Vitest + MSW (Mock Service Worker)
├── 端到端测试：Playwright
└── 测试覆盖率：Vitest coverage + Istanbul
```

**单元测试配置**：
```typescript
// vitest.config.ts
import { defineConfig } from 'vitest/config'
import react from '@vitejs/plugin-react-swc'

export default defineConfig({
  plugins: [react()],
  test: {
    globals: true,
    environment: 'jsdom',
    setupFiles: ['./src/test/setup.ts'],
    include: ['src/**/*.{test,spec}.{ts,tsx}'],
    exclude: ['node_modules', 'dist'],
    coverage: {
      provider: 'istanbul',
      reporter: ['text', 'json', 'html'],
      exclude: [
        'coverage/**',
        'dist/**',
        '**/*.d.ts',
        '**/*.config.*',
        '**/test/**',
        '**/*.{test,spec}.{ts,tsx}',
      ],
      thresholds: {
        statements: 80,
        branches: 75,
        functions: 80,
        lines: 80,
      },
    },
  },
})
```

**组件测试示例**：
```typescript
// components/Button.test.tsx
import { render, screen, fireEvent } from '@testing-library/react'
import { Button } from './Button'

describe('Button组件', () => {
  test('渲染正确文本', () => {
    render(<Button>点击我</Button>)
    expect(screen.getByText('点击我')).toBeInTheDocument()
  })
  
  test('点击事件触发', () => {
    const handleClick = vi.fn()
    render(<Button onClick={handleClick}>点击我</Button>)
    
    fireEvent.click(screen.getByText('点击我'))
    expect(handleClick).toHaveBeenCalledTimes(1)
  })
  
  test('禁用状态', () => {
    render(<Button disabled>禁用按钮</Button>)
    expect(screen.getByText('禁用按钮')).toBeDisabled()
  })
  
  test('加载状态', () => {
    render(<Button loading>加载中</Button>)
    expect(screen.getByRole('button')).toHaveAttribute('aria-busy', 'true')
  })
})
```

**Hook测试示例**：
```typescript
// hooks/useAuth.test.ts
import { renderHook, act, waitFor } from '@testing-library/react'
import { Provider } from 'react-redux'
import { configureStore } from '@reduxjs/toolkit'
import { useAuth } from './useAuth'
import { authApi } from '../store/api/authApi'
import authReducer from '../store/slices/authSlice'

const createTestStore = () => {
  return configureStore({
    reducer: {
      [authApi.reducerPath]: authApi.reducer,
      auth: authReducer,
    },
    middleware: (getDefaultMiddleware) =>
      getDefaultMiddleware().concat(authApi.middleware),
  })
}

describe('useAuth Hook', () => {
  let store: ReturnType<typeof createTestStore>
  
  beforeEach(() => {
    store = createTestStore()
  })
  
  test('登录功能', async () => {
    const wrapper = ({ children }: { children: React.ReactNode }) => (
      <Provider store={store}>{children}</Provider>
    )
    
    const { result } = renderHook(() => useAuth(), { wrapper })
    
    // 模拟登录
    await act(async () => {
      await result.current.login({
        username: 'testuser',
        password: 'password123',
      })
    })
    
    expect(result.current.isAuthenticated).toBe(true)
    expect(result.current.token).toBeTruthy()
  })
  
  test('登出功能', async () => {
    const wrapper = ({ children }: { children: React.ReactNode }) => (
      <Provider store={store}>{children}</Provider>
    )
    
    const { result } = renderHook(() => useAuth(), { wrapper })
    
    // 先登录
    await act(async () => {
      await result.current.login({
        username: 'testuser',
        password: 'password123',
      })
    })
    
    // 再登出
    await act(async () => {
      await result.current.logout()
    })
    
    expect(result.current.isAuthenticated).toBe(false)
    expect(result.current.token).toBeNull()
  })
})
```

**API Mocking策略**：
```typescript
// test/mocks/handlers.ts
import { http, HttpResponse } from 'msw'

export const handlers = [
  // 登录接口
  http.post('/api/v1/auth/login', async ({ request }) => {
    const { username, password } = await request.json() as any
    
    if (username === 'testuser' && password === 'password123') {
      return HttpResponse.json({
        code: 0,
        message: '登录成功',
        data: {
          token: 'mock-jwt-token',
          refreshToken: 'mock-refresh-token',
          user: {
            id: 1,
            username: 'testuser',
            email: 'test@example.com',
          },
        },
      })
    }
    
    return HttpResponse.json(
      {
        code: 401,
        message: '用户名或密码错误',
      },
      { status: 401 }
    )
  }),
  
  // 获取用户资料接口
  http.get('/api/v1/auth/profile', ({ request }) => {
    const token = request.headers.get('Authorization')
    
    if (token === 'Bearer mock-jwt-token') {
      return HttpResponse.json({
        code: 0,
        message: '成功',
        data: {
          id: 1,
          username: 'testuser',
          email: 'test@example.com',
          avatar: 'https://example.com/avatar.jpg',
        },
      })
    }
    
    return HttpResponse.json(
      {
        code: 401,
        message: '未授权',
      },
      { status: 401 }
    )
  }),
  
  // 获取作品列表接口
  http.get('/api/v1/works', () => {
    return HttpResponse.json({
      code: 0,
      message: '成功',
      data: {
        items: [
          { id: 1, title: '作品1', content: '内容1' },
          { id: 2, title: '作品2', content: '内容2' },
          { id: 3, title: '作品3', content: '内容3' },
        ],
        total: 3,
        page: 1,
        pageSize: 10,
        totalPages: 1,
      },
    })
  }),
]

// test/setup.ts
import { beforeAll, afterEach, afterAll } from 'vitest'
import { setupServer } from 'msw/node'
import { handlers } from './mocks/handlers'

const server = setupServer(...handlers)

beforeAll(() => server.listen({ onUnhandledRequest: 'error' }))
afterEach(() => server.resetHandlers())
afterAll(() => server.close())
```

**集成测试示例**：
```typescript
// features/auth/LoginForm.integration.test.tsx
import { render, screen, fireEvent, waitFor } from '@testing-library/react'
import { Provider } from 'react-redux'
import { configureStore } from '@reduxjs/toolkit'
import { BrowserRouter } from 'react-router-dom'
import LoginForm from './LoginForm'
import { authApi } from '../../store/api/authApi'
import authReducer from '../../store/slices/authSlice'

describe('登录表单集成测试', () => {
  const renderWithProviders = (ui: React.ReactElement) => {
    const store = configureStore({
      reducer: {
        [authApi.reducerPath]: authApi.reducer,
        auth: authReducer,
      },
      middleware: (getDefaultMiddleware) =>
        getDefaultMiddleware().concat(authApi.middleware),
    })
    
    return render(
      <Provider store={store}>
        <BrowserRouter>{ui}</BrowserRouter>
      </Provider>
    )
  }
  
  test('完整登录流程', async () => {
    renderWithProviders(<LoginForm />)
    
    // 填写表单
    const usernameInput = screen.getByLabelText(/用户名/i)
    const passwordInput = screen.getByLabelText(/密码/i)
    const submitButton = screen.getByRole('button', { name: /登录/i })
    
    fireEvent.change(usernameInput, { target: { value: 'testuser' } })
    fireEvent.change(passwordInput, { target: { value: 'password123' } })
    
    // 提交表单
    fireEvent.click(submitButton)
    
    // 验证登录成功后的跳转
    await waitFor(() => {
      expect(window.location.pathname).toBe('/dashboard')
    })
  })
  
  test('登录失败提示', async () => {
    // 模拟登录失败
    server.use(
      http.post('/api/v1/auth/login', () => {
        return HttpResponse.json(
          {
            code: 401,
            message: '用户名或密码错误',
          },
          { status: 401 }
        )
      })
    )
    
    renderWithProviders(<LoginForm />)
    
    // 填写错误凭证
    const usernameInput = screen.getByLabelText(/用户名/i)
    const passwordInput = screen.getByLabelText(/密码/i)
    const submitButton = screen.getByRole('button', { name: /登录/i })
    
    fireEvent.change(usernameInput, { target: { value: 'wronguser' } })
    fireEvent.change(passwordInput, { target: { value: 'wrongpass' } })
    fireEvent.click(submitButton)
    
    // 验证错误提示
    await waitFor(() => {
      expect(screen.getByText(/用户名或密码错误/i)).toBeInTheDocument()
    })
  })
})
```

**端到端测试配置**：
```typescript
// playwright.config.ts
import { defineConfig, devices } from '@playwright/test'

export default defineConfig({
  testDir: './e2e',
  fullyParallel: true,
  forbidOnly: !!process.env.CI,
  retries: process.env.CI ? 2 : 0,
  workers: process.env.CI ? 1 : undefined,
  reporter: [
    ['html', { outputFolder: 'playwright-report' }],
    ['json', { outputFile: 'playwright-results.json' }],
  ],
  use: {
    baseURL: 'http://localhost:3000',
    trace: 'on-first-retry',
    screenshot: 'only-on-failure',
    video: 'retain-on-failure',
  },
  projects: [
    {
      name: 'chromium',
      use: { ...devices['Desktop Chrome'] },
    },
    {
      name: 'firefox',
      use: { ...devices['Desktop Firefox'] },
    },
    {
      name: 'webkit',
      use: { ...devices['Desktop Safari'] },
    },
    {
      name: 'Mobile Chrome',
      use: { ...devices['Pixel 5'] },
    },
    {
      name: 'Mobile Safari',
      use: { ...devices['iPhone 12'] },
    },
  ],
  webServer: {
    command: 'npm run preview',
    url: 'http://localhost:3000',
    reuseExistingServer: !process.env.CI,
  },
})
```

**端到端测试示例**：
```typescript
// e2e/auth.spec.ts
import { test, expect } from '@playwright/test'

test.describe('认证流程', () => {
  test('用户登录', async ({ page }) => {
    // 访问登录页
    await page.goto('/login')
    
    // 填写登录表单
    await page.fill('input[name="username"]', 'testuser')
    await page.fill('input[name="password"]', 'password123')
    
    // 点击登录按钮
    await page.click('button[type="submit"]')
    
    // 验证登录成功跳转
    await page.waitForURL('/dashboard')
    await expect(page).toHaveURL('/dashboard')
    await expect(page.locator('text=欢迎回来')).toBeVisible()
  })
  
  test('用户注册', async ({ page }) => {
    // 访问注册页
    await page.goto('/register')
    
    // 填写注册表单
    await page.fill('input[name="username"]', 'newuser')
    await page.fill('input[name="email"]', 'newuser@example.com')
    await page.fill('input[name="password"]', 'Password123!')
    await page.fill('input[name="confirmPassword"]', 'Password123!')
    
    // 同意协议
    await page.check('input[name="agreeTerms"]')
    
    // 点击注册按钮
    await page.click('button[type="submit"]')
    
    // 验证注册成功
    await page.waitForURL('/register/success')
    await expect(page.locator('text=注册成功')).toBeVisible()
  })
  
  test('登出功能', async ({ page }) => {
    // 先登录
    await page.goto('/login')
    await page.fill('input[name="username"]', 'testuser')
    await page.fill('input[name="password"]', 'password123')
    await page.click('button[type="submit"]')
    await page.waitForURL('/dashboard')
    
    // 点击登出
    await page.click('button:has-text("登出")')
    
    // 验证跳转到登录页
    await page.waitForURL('/login')
    await expect(page).toHaveURL('/login')
  })
})
```

**测试最佳实践**：
1. **测试金字塔原则**：
   - 70%单元测试：业务逻辑、工具函数、纯组件
   - 20%集成测试：组件组合、API交互、路由
   - 10%端到端测试：关键用户流程

2. **测试数据管理**：
   - 使用Factory模式创建测试数据
   - 避免硬编码，使用动态数据
   - 测试前后清理数据

3. **测试性能优化**：
   - 并行运行测试
   - 使用模拟数据减少外部依赖
   - 避免不必要的渲染

4. **持续集成集成**：
   - 提交前自动运行测试
   - 合并前通过所有测试
   - 测试失败阻止部署

**测试环境配置**：
```json
// package.json scripts
{
  "scripts": {
    "test": "vitest",
    "test:unit": "vitest run --config vitest.unit.config.ts",
    "test:integration": "vitest run --config vitest.integration.config.ts",
    "test:e2e": "playwright test",
    "test:coverage": "vitest run --coverage",
    "test:watch": "vitest",
    "test:ci": "npm run test:unit && npm run test:integration && npm run test:e2e",
    "test:update": "vitest -u"
  }
}
```

**核心模块设计**：

**认证模块 (Authentication Module)**：
```typescript
// features/auth/components/LoginForm.tsx
import { useState } from 'react'
import { Form, Input, Button, Checkbox, message } from 'antd'
import { LockOutlined, UserOutlined } from '@ant-design/icons'
import { useAuth } from '../hooks/useAuth'

const LoginForm: React.FC = () => {
  const [loading, setLoading] = useState(false)
  const { login } = useAuth()

  const onFinish = async (values: any) => {
    setLoading(true)
    try {
      await login({
        username: values.username,
        password: values.password,
      })
      message.success('登录成功')
      // 路由跳转由useAuth内部处理
    } catch (error) {
      message.error('登录失败，请检查用户名和密码')
    } finally {
      setLoading(false)
    }
  }

  return (
    <Form
      name="login"
      initialValues={{ remember: true }}
      onFinish={onFinish}
      size="large"
    >
      <Form.Item
        name="username"
        rules={[
          { required: true, message: '请输入用户名' },
          { min: 3, max: 20, message: '用户名长度为3-20个字符' },
        ]}
      >
        <Input
          prefix={<UserOutlined />}
          placeholder="用户名"
          autoComplete="username"
        />
      </Form.Item>

      <Form.Item
        name="password"
        rules={[
          { required: true, message: '请输入密码' },
          { min: 6, max: 32, message: '密码长度为6-32个字符' },
        ]}
      >
        <Input.Password
          prefix={<LockOutlined />}
          placeholder="密码"
          autoComplete="current-password"
        />
      </Form.Item>

      <Form.Item>
        <Form.Item name="remember" valuePropName="checked" noStyle>
          <Checkbox>记住我</Checkbox>
        </Form.Item>
        <a href="/forgot-password" style={{ float: 'right' }}>
          忘记密码？
        </a>
      </Form.Item>

      <Form.Item>
        <Button
          type="primary"
          htmlType="submit"
          loading={loading}
          block
        >
          登录
        </Button>
      </Form.Item>
    </Form>
  )
}

export default LoginForm
```

**路由守卫与权限控制**：
```typescript
// core/components/PrivateRoute.tsx
import { Navigate, useLocation } from 'react-router-dom'
import { useAuth } from '@/hooks/useAuth'

interface PrivateRouteProps {
  children: React.ReactNode
  requiredRoles?: string[]
}

const PrivateRoute: React.FC<PrivateRouteProps> = ({ 
  children, 
  requiredRoles = [] 
}) => {
  const { isAuthenticated, user, isLoading } = useAuth()
  const location = useLocation()

  if (isLoading) {
    return <div>加载中...</div>
  }

  if (!isAuthenticated) {
    // 重定向到登录页，并保存当前路径以便登录后返回
    return <Navigate to="/login" state={{ from: location }} replace />
  }

  // 检查角色权限
  if (requiredRoles.length > 0 && user) {
    const hasRole = requiredRoles.some(role => 
      user.roles?.includes(role)
    )
    
    if (!hasRole) {
      return <Navigate to="/unauthorized" replace />
    }
  }

  return <>{children}</>
}

// 路由配置示例
const routes = [
  {
    path: '/',
    element: <MainLayout />,
    children: [
      {
        path: 'dashboard',
        element: (
          <PrivateRoute requiredRoles={['user', 'admin']}>
            <DashboardPage />
          </PrivateRoute>
        ),
      },
      {
        path: 'admin',
        element: (
          <PrivateRoute requiredRoles={['admin']}>
            <AdminLayout />
          </PrivateRoute>
        ),
        children: [
          { path: 'users', element: <UserManagement /> },
          { path: 'works', element: <WorkManagement /> },
        ],
      },
    ],
  },
]
```

**写作编辑器模块 (Writing Editor Module)**：
```typescript
// features/writing/components/WritingEditor.tsx
import { useState, useCallback, useEffect } from 'react'
import ReactQuill from 'react-quill'
import 'react-quill/dist/quill.snow.css'
import { 
  Button, 
  Space, 
  Input, 
  Select, 
  message, 
  Card,
  Typography 
} from 'antd'
import { 
  SaveOutlined, 
  EyeOutlined, 
  DownloadOutlined,
  BulbOutlined 
} from '@ant-design/icons'
import { useDebounce } from '@/hooks/useDebounce'
import AIAssistant from './AIAssistant'

const { Title } = Typography
const { TextArea } = Input

interface WritingEditorProps {
  initialContent?: string
  workId?: number
  onSave?: (content: string, metadata: WorkMetadata) => Promise<void>
}

const WritingEditor: React.FC<WritingEditorProps> = ({
  initialContent = '',
  workId,
  onSave,
}) => {
  const [title, setTitle] = useState('')
  const [content, setContent] = useState(initialContent)
  const [category, setCategory] = useState('')
  const [tags, setTags] = useState<string[]>([])
  const [isSaving, setIsSaving] = useState(false)
  const [wordCount, setWordCount] = useState(0)
  const [showAIAssistant, setShowAIAssistant] = useState(false)

  // 自动保存（防抖）
  const debouncedContent = useDebounce(content, 5000)
  
  useEffect(() => {
    // 计算字数
    const text = content.replace(/<[^>]*>/g, '')
    const words = text.trim().split(/\s+/).filter(word => word.length > 0)
    setWordCount(words.length)
    
    // 自动保存逻辑
    if (debouncedContent && debouncedContent !== initialContent) {
      autoSave()
    }
  }, [debouncedContent])

  const autoSave = useCallback(async () => {
    if (!workId || !onSave) return
    
    try {
      await onSave(content, {
        title,
        category,
        tags,
        wordCount,
        lastModified: new Date().toISOString(),
      })
      console.log('自动保存成功')
    } catch (error) {
      console.error('自动保存失败:', error)
    }
  }, [content, title, category, tags, wordCount, workId, onSave])

  const handleManualSave = async () => {
    if (!onSave) return
    
    setIsSaving(true)
    try {
      await onSave(content, {
        title,
        category,
        tags,
        wordCount,
        lastModified: new Date().toISOString(),
      })
      message.success('保存成功')
    } catch (error) {
      message.error('保存失败')
    } finally {
      setIsSaving(false)
    }
  }

  const handleAISuggestion = useCallback((suggestion: string) => {
    // 插入AI建议到编辑器
    setContent(prev => prev + '\n' + suggestion)
    message.success('AI建议已插入')
  }, [])

  // 编辑器模块配置
  const modules = {
    toolbar: [
      [{ header: [1, 2, 3, false] }],
      ['bold', 'italic', 'underline', 'strike'],
      [{ list: 'ordered' }, { list: 'bullet' }],
      [{ indent: '-1' }, { indent: '+1' }],
      ['link', 'image', 'video'],
      ['clean'],
      ['code-block'],
    ],
  }

  return (
    <Card>
      <Space direction="vertical" style={{ width: '100%' }}>
        {/* 标题和基本信息 */}
        <Space>
          <Input
            placeholder="作品标题"
            value={title}
            onChange={(e) => setTitle(e.target.value)}
            style={{ width: 300 }}
          />
          <Select
            placeholder="选择分类"
            value={category}
            onChange={setCategory}
            style={{ width: 150 }}
            options={[
              { label: '小说', value: 'fiction' },
              { label: '散文', value: 'prose' },
              { label: '诗歌', value: 'poetry' },
              { label: '学术', value: 'academic' },
            ]}
          />
        </Space>

        {/* 编辑器 */}
        <ReactQuill
          theme="snow"
          value={content}
          onChange={setContent}
          modules={modules}
          style={{ height: '500px', marginBottom: '50px' }}
        />

        {/* 工具栏 */}
        <Space>
          <Button
            type="primary"
            icon={<SaveOutlined />}
            loading={isSaving}
            onClick={handleManualSave}
          >
            保存
          </Button>
          <Button icon={<EyeOutlined />}>预览</Button>
          <Button icon={<DownloadOutlined />}>导出</Button>
          <Button 
            icon={<BulbOutlined />}
            onClick={() => setShowAIAssistant(!showAIAssistant)}
          >
            AI助手
          </Button>
          
          {/* 统计信息 */}
          <span style={{ marginLeft: 'auto' }}>
            字数: {wordCount} | 字符数: {content.length}
          </span>
        </Space>

        {/* AI助手面板 */}
        {showAIAssistant && (
          <AIAssistant
            content={content}
            onSuggestion={handleAISuggestion}
          />
        )}
      </Space>
    </Card>
  )
}

export default WritingEditor
```

**社区模块 (Community Module)**：
```typescript
// features/community/components/WorkList.tsx
import { useState, useEffect } from 'react'
import { 
  Card, 
  Row, 
  Col, 
  Pagination, 
  Spin,
  Empty,
  Tag,
  Space,
  Dropdown,
  MenuProps 
} from 'antd'
import { 
  LikeOutlined, 
  MessageOutlined, 
  StarOutlined,
  EyeOutlined,
  MoreOutlined 
} from '@ant-design/icons'
import VirtualList from '@/components/VirtualList'
import { useGetWorksQuery } from '@/store/api/writingApi'

const WorkList: React.FC = () => {
  const [currentPage, setCurrentPage] = useState(1)
  const [pageSize, setPageSize] = useState(12)
  const [sortBy, setSortBy] = useState('latest')
  const [category, setCategory] = useState<string>('all')

  const { data, isLoading, isFetching } = useGetWorksQuery({
    page: currentPage,
    pageSize,
    sortBy,
    category: category !== 'all' ? category : undefined,
  })

  const handleWorkClick = (workId: number) => {
    window.open(`/works/${workId}`, '_blank')
  }

  const sortOptions: MenuProps['items'] = [
    { key: 'latest', label: '最新发布' },
    { key: 'hot', label: '热门作品' },
    { key: 'recommended', label: '编辑推荐' },
  ]

  const categoryOptions = [
    { label: '全部', value: 'all' },
    { label: '小说', value: 'fiction' },
    { label: '散文', value: 'prose' },
    { label: '诗歌', value: 'poetry' },
    { label: '学术', value: 'academic' },
  ]

  if (isLoading) {
    return (
      <div style={{ textAlign: 'center', padding: '100px' }}>
        <Spin size="large" />
      </div>
    )
  }

  if (!data?.items?.length) {
    return <Empty description="暂无作品" />
  }

  return (
    <div>
      {/* 筛选工具栏 */}
      <Space style={{ marginBottom: 16 }}>
        <Dropdown
          menu={{
            items: sortOptions,
            onClick: ({ key }) => setSortBy(key),
            selectedKeys: [sortBy],
          }}
        >
          <a onClick={(e) => e.preventDefault()}>
            排序方式
          </a>
        </Dropdown>
        
        <Space>
          {categoryOptions.map((option) => (
            <Tag
              key={option.value}
              color={category === option.value ? 'blue' : 'default'}
              onClick={() => setCategory(option.value)}
              style={{ cursor: 'pointer' }}
            >
              {option.label}
            </Tag>
          ))}
        </Space>
      </Space>

      {/* 作品列表 */}
      <Row gutter={[16, 16]}>
        {data.items.map((work) => (
          <Col key={work.id} xs={24} sm={12} md={8} lg={6}>
            <Card
              hoverable
              onClick={() => handleWorkClick(work.id)}
              cover={
                <div style={{ 
                  height: '150px', 
                  backgroundColor: '#f5f5f5',
                  display: 'flex',
                  alignItems: 'center',
                  justifyContent: 'center',
                  color: '#999',
                }}>
                  {work.coverImage ? (
                    <img 
                      src={work.coverImage} 
                      alt={work.title}
                      style={{ width: '100%', height: '100%', objectFit: 'cover' }}
                    />
                  ) : (
                    '无封面'
                  )}
                </div>
              }
            >
              <Card.Meta
                title={
                  <div style={{ 
                    whiteSpace: 'nowrap',
                    overflow: 'hidden',
                    textOverflow: 'ellipsis',
                  }}>
                    {work.title}
                  </div>
                }
                description={
                  <div style={{ 
                    color: '#666',
                    fontSize: '12px',
                    height: '40px',
                    overflow: 'hidden',
                    textOverflow: 'ellipsis',
                    display: '-webkit-box',
                    WebkitLineClamp: 2,
                    WebkitBoxOrient: 'vertical',
                  }}>
                    {work.summary || '暂无简介'}
                  </div>
                }
              />
              
              {/* 作者信息 */}
              <div style={{ marginTop: '12px', fontSize: '12px', color: '#999' }}>
                {work.author?.username}
              </div>
              
              {/* 统计信息 */}
              <Space style={{ marginTop: '12px', fontSize: '12px' }}>
                <span>
                  <EyeOutlined /> {work.viewCount || 0}
                </span>
                <span>
                  <LikeOutlined /> {work.likeCount || 0}
                </span>
                <span>
                  <MessageOutlined /> {work.commentCount || 0}
                </span>
                <span>
                  <StarOutlined /> {work.favoriteCount || 0}
                </span>
              </Space>
              
              {/* 标签 */}
              <div style={{ marginTop: '8px' }}>
                {work.tags?.slice(0, 3).map((tag) => (
                  <Tag key={tag} size="small" style={{ marginBottom: '4px' }}>
                    {tag}
                  </Tag>
                ))}
              </div>
            </Card>
          </Col>
        ))}
      </Row>

      {/* 分页 */}
      {data.totalPages > 1 && (
        <div style={{ textAlign: 'center', marginTop: '24px' }}>
          <Pagination
            current={currentPage}
            pageSize={pageSize}
            total={data.total}
            onChange={setCurrentPage}
            onShowSizeChange={(current, size) => setPageSize(size)}
            showSizeChanger
            showQuickJumper
            showTotal={(total) => `共 ${total} 条作品`}
          />
        </div>
      )}
    </div>
  )
}

export default WorkList
```

**实时聊天模块 (Real-time Chat Module)**：
```typescript
// features/chat/components/ChatWindow.tsx
import { useState, useRef, useEffect } from 'react'
import { 
  Input, 
  Button, 
  List, 
  Avatar, 
  Card, 
  Space,
  Popover,
  Tooltip,
  Upload,
  message 
} from 'antd'
import { 
  SendOutlined, 
  PaperClipOutlined,
  SmileOutlined,
  PictureOutlined,
  FileOutlined 
} from '@ant-design/icons'
import Picker from 'emoji-picker-react'
import { chatWebSocket } from '@/core/api/websocket'
import { useAuth } from '@/hooks/useAuth'

interface Message {
  id: string
  senderId: number
  senderName: string
  senderAvatar: string
  content: string
  timestamp: number
  type: 'text' | 'image' | 'file'
  status: 'sending' | 'sent' | 'delivered' | 'read'
}

const ChatWindow: React.FC<{ conversationId: number }> = ({ conversationId }) => {
  const [messages, setMessages] = useState<Message[]>([])
  const [inputText, setInputText] = useState('')
  const [showEmojiPicker, setShowEmojiPicker] = useState(false)
  const [isConnected, setIsConnected] = useState(false)
  const messagesEndRef = useRef<HTMLDivElement>(null)
  const { user } = useAuth()

  // 滚动到底部
  const scrollToBottom = () => {
    messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' })
  }

  // 连接WebSocket
  useEffect(() => {
    const connect = async () => {
      try {
        await chatWebSocket.connect()
        setIsConnected(true)
        
        // 订阅消息
        const unsubscribe = chatWebSocket.subscribe('new_message', (data) => {
          setMessages(prev => [...prev, data])
          scrollToBottom()
        })
        
        // 加载历史消息
        loadHistoryMessages()
        
        return unsubscribe
      } catch (error) {
        console.error('WebSocket连接失败:', error)
        message.error('聊天连接失败')
      }
    }
    
    const cleanup = connect()
    
    return () => {
      cleanup?.then(fn => fn?.())
      chatWebSocket.disconnect()
    }
  }, [conversationId])

  // 加载历史消息
  const loadHistoryMessages = async () => {
    // 调用API加载历史消息
    // const history = await chatApi.getMessages(conversationId)
    // setMessages(history)
  }

  // 发送消息
  const sendMessage = () => {
    if (!inputText.trim() || !isConnected) return
    
    const message: Message = {
      id: Date.now().toString(),
      senderId: user!.id,
      senderName: user!.username,
      senderAvatar: user!.avatar || '',
      content: inputText.trim(),
      timestamp: Date.now(),
      type: 'text',
      status: 'sending',
    }
    
    // 添加到本地列表
    setMessages(prev => [...prev, message])
    setInputText('')
    scrollToBottom()
    
    // 发送到服务器
    const sent = chatWebSocket.send('send_message', {
      conversationId,
      message: inputText.trim(),
    })
    
    if (sent) {
      // 更新消息状态为已发送
      setTimeout(() => {
        setMessages(prev =>
          prev.map(msg =>
            msg.id === message.id
              ? { ...msg, status: 'sent' }
              : msg
          )
        )
      }, 100)
    }
  }

  // 发送图片
  const sendImage = (file: File) => {
    // 上传图片并发送
    const reader = new FileReader()
    reader.onload = (e) => {
      const imageMessage: Message = {
        id: Date.now().toString(),
        senderId: user!.id,
        senderName: user!.username,
        senderAvatar: user!.avatar || '',
        content: e.target!.result as string,
        timestamp: Date.now(),
        type: 'image',
        status: 'sending',
      }
      
      setMessages(prev => [...prev, imageMessage])
      scrollToBottom()
      
      // 发送到服务器
      chatWebSocket.send('send_image', {
        conversationId,
        imageData: e.target!.result,
        fileName: file.name,
      })
    }
    reader.readAsDataURL(file)
  }

  // 选择表情
  const onEmojiClick = (emojiObject: any) => {
    setInputText(prev => prev + emojiObject.emoji)
    setShowEmojiPicker(false)
  }

  // 处理键盘事件
  const handleKeyPress = (e: React.KeyboardEvent) => {
    if (e.key === 'Enter' && !e.shiftKey) {
      e.preventDefault()
      sendMessage()
    }
  }

  return (
    <Card 
      title="聊天" 
      style={{ height: '600px', display: 'flex', flexDirection: 'column' }}
    >
      {/* 消息列表 */}
      <div style={{ flex: 1, overflow: 'auto', padding: '16px' }}>
        <List
          dataSource={messages}
          renderItem={(msg) => (
            <List.Item 
              style={{ 
                justifyContent: msg.senderId === user?.id ? 'flex-end' : 'flex-start',
                padding: '8px 0',
              }}
            >
              <Space align="start">
                {msg.senderId !== user?.id && (
                  <Avatar src={msg.senderAvatar} size="small">
                    {msg.senderName[0]}
                  </Avatar>
                )}
                
                <div style={{ 
                  maxWidth: '60%',
                  backgroundColor: msg.senderId === user?.id ? '#1890ff' : '#f5f5f5',
                  color: msg.senderId === user?.id ? '#fff' : '#000',
                  padding: '8px 12px',
                  borderRadius: '12px',
                  wordBreak: 'break-word',
                }}>
                  {msg.type === 'text' ? (
                    <div>{msg.content}</div>
                  ) : msg.type === 'image' ? (
                    <img 
                      src={msg.content} 
                      alt="图片" 
                      style={{ maxWidth: '200px', maxHeight: '200px' }}
                    />
                  ) : (
                    <div>
                      <FileOutlined /> 文件
                    </div>
                  )}
                  
                  <div style={{ 
                    fontSize: '10px', 
                    opacity: 0.7,
                    textAlign: 'right',
                    marginTop: '4px',
                  }}>
                    {new Date(msg.timestamp).toLocaleTimeString()}
                  </div>
                </div>
                
                {msg.senderId === user?.id && (
                  <Avatar src={msg.senderAvatar} size="small">
                    {msg.senderName[0]}
                  </Avatar>
                )}
              </Space>
            </List.Item>
          )}
        />
        <div ref={messagesEndRef} />
      </div>

      {/* 输入区域 */}
      <Space.Compact style={{ width: '100%', padding: '16px' }}>
        {/* 附件按钮 */}
        <Popover
          content={
            <Space direction="vertical">
              <Upload
                showUploadList={false}
                beforeUpload={(file) => {
                  sendImage(file)
                  return false
                }}
              >
                <Button icon={<PictureOutlined />}>图片</Button>
              </Upload>
              <Button icon={<FileOutlined />}>文件</Button>
            </Space>
          }
          trigger="click"
        >
          <Button icon={<PaperClipOutlined />} />
        </Popover>

        {/* 表情按钮 */}
        <Popover
          content={
            <Picker onEmojiClick={onEmojiClick} />
          }
          open={showEmojiPicker}
          onOpenChange={setShowEmojiPicker}
          trigger="click"
        >
          <Button icon={<SmileOutlined />} />
        </Popover>

        {/* 消息输入框 */}
        <Input.TextArea
          value={inputText}
          onChange={(e) => setInputText(e.target.value)}
          onKeyPress={handleKeyPress}
          placeholder="输入消息..."
          autoSize={{ minRows: 1, maxRows: 4 }}
          style={{ flex: 1 }}
        />

        {/* 发送按钮 */}
        <Button
          type="primary"
          icon={<SendOutlined />}
          onClick={sendMessage}
          disabled={!inputText.trim() || !isConnected}
        >
          发送
        </Button>
      </Space.Compact>
    </Card>
  )
}

export default ChatWindow
```

**支付模块 (Payment Module)**：
```typescript
// features/payment/components/DonationModal.tsx
import { useState } from 'react'
import { 
  Modal, 
  Form, 
  Input, 
  Radio, 
  Space, 
  Button, 
  message,
  Typography,
  Divider 
} from 'antd'
import { 
  WechatOutlined, 
  AlipayOutlined,
  CreditCardOutlined 
} from '@ant-design/icons'
import QRCode from 'qrcode.react'

const { Title, Text } = Typography

interface DonationModalProps {
  workId: number
  workTitle: string
  authorName: string
  open: boolean
  onClose: () => void
  onSuccess?: (transactionId: string) => void
}

const DonationModal: React.FC<DonationModalProps> = ({
  workId,
  workTitle,
  authorName,
  open,
  onClose,
  onSuccess,
}) => {
  const [form] = Form.useForm()
  const [paymentMethod, setPaymentMethod] = useState('wechat')
  const [amount, setAmount] = useState('')
  const [customAmount, setCustomAmount] = useState('')
  const [isPaying, setIsPaying] = useState(false)
  const [showQRCode, setShowQRCode] = useState(false)
  const [qrCodeUrl, setQrCodeUrl] = useState('')

  const presetAmounts = ['5', '10', '20', '50', '100']

  const handleAmountSelect = (value: string) => {
    setAmount(value)
    setCustomAmount('')
  }

  const handleCustomAmountChange = (value: string) => {
    setCustomAmount(value)
    if (value) {
      setAmount('')
    }
  }

  const getFinalAmount = () => {
    if (customAmount) {
      return parseFloat(customAmount) || 0
    }
    return parseFloat(amount) || 0
  }

  const handlePayment = async () => {
    const finalAmount = getFinalAmount()
    
    if (finalAmount <= 0) {
      message.error('请选择或输入打赏金额')
      return
    }

    if (finalAmount < 1) {
      message.error('打赏金额不能少于1元')
      return
    }

    if (finalAmount > 5000) {
      message.error('单笔打赏金额不能超过5000元')
      return
    }

    setIsPaying(true)

    try {
      // 调用支付API
      const response = await paymentApi.createDonation({
        workId,
        amount: finalAmount,
        paymentMethod,
        message: form.getFieldValue('message'),
      })

      if (paymentMethod === 'wechat' || paymentMethod === 'alipay') {
        // 显示二维码
        setQrCodeUrl(response.qrCodeUrl)
        setShowQRCode(true)
        
        // 轮询支付状态
        pollPaymentStatus(response.transactionId)
      } else {
        // 其他支付方式直接跳转
        window.open(response.paymentUrl, '_blank')
      }
    } catch (error) {
      message.error('支付请求失败')
      setIsPaying(false)
    }
  }

  const pollPaymentStatus = (transactionId: string) => {
    const interval = setInterval(async () => {
      try {
        const status = await paymentApi.checkPaymentStatus(transactionId)
        
        if (status === 'paid') {
          clearInterval(interval)
          message.success('支付成功！感谢您的支持！')
          setShowQRCode(false)
          setIsPaying(false)
          onClose()
          onSuccess?.(transactionId)
        } else if (status === 'failed' || status === 'cancelled') {
          clearInterval(interval)
          message.warning('支付已取消或失败')
          setShowQRCode(false)
          setIsPaying(false)
        }
      } catch (error) {
        console.error('查询支付状态失败:', error)
      }
    }, 2000)

    // 5分钟后自动停止轮询
    setTimeout(() => {
      clearInterval(interval)
      if (showQRCode) {
        message.warning('支付超时，请重试')
        setShowQRCode(false)
        setIsPaying(false)
      }
    }, 300000)
  }

  const paymentMethods = [
    {
      value: 'wechat',
      label: (
        <Space>
          <WechatOutlined style={{ color: '#07C160' }} />
          微信支付
        </Space>
      ),
    },
    {
      value: 'alipay',
      label: (
        <Space>
          <AlipayOutlined style={{ color: '#1677FF' }} />
          支付宝
        </Space>
      ),
    },
    {
      value: 'bankcard',
      label: (
        <Space>
          <CreditCardOutlined />
          银行卡
        </Space>
      ),
    },
  ]

  return (
    <Modal
      title="支持作者"
      open={open}
      onCancel={onClose}
      footer={null}
      width={500}
    >
      <div style={{ textAlign: 'center', marginBottom: 24 }}>
        <Title level={4}>{workTitle}</Title>
        <Text type="secondary">向 {authorName} 表示支持</Text>
      </div>

      {!showQRCode ? (
        <>
          {/* 金额选择 */}
          <div style={{ marginBottom: 24 }}>
            <Text strong style={{ display: 'block', marginBottom: 12 }}>
              选择打赏金额（元）
            </Text>
            <Space wrap>
              {presetAmounts.map((amt) => (
                <Button
                  key={amt}
                  type={amount === amt ? 'primary' : 'default'}
                  onClick={() => handleAmountSelect(amt)}
                >
                  {amt}
                </Button>
              ))}
            </Space>
            
            <div style={{ marginTop: 12 }}>
              <Input
                placeholder="自定义金额"
                value={customAmount}
                onChange={(e) => handleCustomAmountChange(e.target.value)}
                style={{ width: 150 }}
                suffix="元"
              />
            </div>
          </div>

          <Divider />

          {/* 支付方式 */}
          <div style={{ marginBottom: 24 }}>
            <Text strong style={{ display: 'block', marginBottom: 12 }}>
              选择支付方式
            </Text>
            <Radio.Group
              value={paymentMethod}
              onChange={(e) => setPaymentMethod(e.target.value)}
              style={{ width: '100%' }}
            >
              <Space direction="vertical" style={{ width: '100%' }}>
                {paymentMethods.map((method) => (
                  <Radio key={method.value} value={method.value} style={{ width: '100%' }}>
                    {method.label}
                  </Radio>
                ))}
              </Space>
            </Radio.Group>
          </div>

          {/* 留言 */}
          <Form form={form}>
            <Form.Item name="message">
              <Input.TextArea
                placeholder="给作者留言（可选）"
                maxLength={200}
                showCount
                rows={3}
              />
            </Form.Item>
          </Form>

          {/* 支付按钮 */}
          <Button
            type="primary"
            size="large"
            block
            loading={isPaying}
            onClick={handlePayment}
          >
            支付 {getFinalAmount().toFixed(2)} 元
          </Button>
        </>
      ) : (
        // 二维码显示
        <div style={{ textAlign: 'center', padding: '40px 0' }}>
          <Text strong style={{ display: 'block', marginBottom: 24 }}>
            请使用{paymentMethod === 'wechat' ? '微信' : '支付宝'}扫描二维码支付
          </Text>
          <div style={{ 
            display: 'inline-block', 
            padding: '20px', 
            backgroundColor: '#fff',
            borderRadius: '8px',
            boxShadow: '0 2px 8px rgba(0,0,0,0.1)',
          }}>
            <QRCode 
              value={qrCodeUrl} 
              size={200} 
              fgColor="#000000"
            />
          </div>
          <Text type="secondary" style={{ display: 'block', marginTop: 24 }}>
            支付完成后请勿关闭页面，支付结果将在支付成功后自动显示
          </Text>
        </div>
      )}
    </Modal>
  )
}

export default DonationModal
```

**总结**：
React Web端架构基于现代前端技术栈，采用模块化设计、状态集中管理、性能优化等最佳实践。核心模块包括：

1. **认证与授权**：JWT认证、路由守卫、角色权限控制
2. **写作编辑器**：富文本编辑、AI辅助、自动保存、多格式导出
3. **社区互动**：作品展示、分类筛选、点赞评论、实时推送
4. **实时聊天**：WebSocket连接、消息状态、文件传输、离线消息
5. **支付系统**：多支付方式、二维码支付、交易状态跟踪

每个模块都遵循统一的架构原则，确保代码的可维护性、可测试性和可扩展性。通过状态管理、网络层封装、性能优化等措施，保障了应用的高性能和高可用性。

```

## 4. 服务端架构设计（.NET 8）

### 4.1 微服务拆分策略

| 服务名称 | 职责范围 | 技术特性 |
|----------|----------|----------|
| **UserService** | 用户管理、认证授权、权限控制 | JWT认证、OAuth2.0、RBAC |
| **WritingService** | 作品管理、写作辅助、模板管理 | 实时协作、版本控制 |
| **CommunityService** | 作品广场、评论、点赞、收藏 | 实时推送、热门排行 |
| **AIService** | AI写作辅助、语法检查、评分 | GPU加速、模型管理 |
| **PaymentService** | 打赏支付、余额管理、提现 | 微信/支付宝集成、对账 |
| **ChatService** | 实时聊天、消息推送、文件传输 | WebSocket、消息队列 |
| **FileService** | 文件上传、存储管理、CDN分发 | 断点续传、图片处理 |
| **SearchService** | 全文搜索、推荐算法 | Elasticsearch集成 |
| **NotificationService** | 系统通知、邮件/SMS推送 | 模板引擎、多渠道 |

### 4.2 .NET 8 WebAPI项目结构
```
UserService/
├── UserService.csproj
├── Program.cs                          # 入口点
├── Properties/                         # 属性配置
│   └── launchSettings.json
├── Controllers/                        # API控制器
│   ├── UsersController.cs
│   ├── AuthController.cs
│   └── PermissionsController.cs
├── Services/                           # 业务服务
│   ├── Interfaces/                     # 服务接口
│   └── Implementations/                # 服务实现
├── Repositories/                       # 数据访问
│   ├── Interfaces/                     # 仓储接口
│   └── Implementations/                # 仓储实现
├── Models/                             # 数据模型
│   ├── Entities/                       # 数据库实体
│   ├── DTOs/                           # 数据传输对象
│   └── ViewModels/                     # 视图模型
├── Middlewares/                        # 中间件
│   ├── ExceptionMiddleware.cs          # 全局异常处理
│   ├── LoggingMiddleware.cs            # 请求日志
│   └── RateLimitMiddleware.cs          # 限流中间件
├── Filters/                            # 过滤器
│   ├── AuthorizationFilter.cs          # 授权过滤
│   └── ValidationFilter.cs             # 验证过滤
├── Validators/                         # 验证器
├── Extensions/                         # 扩展方法
├── Helpers/                            # 辅助类
├── Configurations/                     # 配置类
└── appsettings.json                    # 配置文件
```

### 4.3 高性能配置
```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

// 1. 性能优化配置
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Limits.MaxConcurrentConnections = 10000;
    serverOptions.Limits.MaxConcurrentUpgradedConnections = 10000;
    serverOptions.Limits.MaxRequestBodySize = 100 * 1024 * 1024; // 100MB
    serverOptions.ListenAnyIP(5000, listenOptions =>
    {
        listenOptions.Protocols = HttpProtocols.Http1AndHttp2AndHttp3;
    });
});

// 2. 依赖注入配置
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

// 3. 数据库连接池
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseMySql(
        builder.Configuration.GetConnectionString("Default"),
        new MySqlServerVersion(new Version(8, 0, 28)),
        mysqlOptions =>
        {
            mysqlOptions.EnableRetryOnFailure(
                maxRetryCount: 5,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null);
            mysqlOptions.CommandTimeout(30);
            mysqlOptions.MaxBatchSize(100);
            mysqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
        });
});

// 4. Redis分布式缓存
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
    options.InstanceName = "WritingPlatform:";
});

// 5. 响应压缩
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
});

// 6. 健康检查
builder.Services.AddHealthChecks()
    .AddMySql(builder.Configuration.GetConnectionString("Default"))
    .AddRedis(builder.Configuration.GetConnectionString("Redis"))
    .AddUrlGroup(new Uri("http://localhost:5000/health"), "API");

// 7. 限流策略
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.User.Identity?.Name ?? httpContext.Request.Headers.Host.ToString(),
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 100,
                QueueLimit = 50,
                Window = TimeSpan.FromMinutes(1)
            }));
});

var app = builder.Build();

// 中间件管道配置
app.UseResponseCompression();
app.UseRateLimiter();
app.UseHealthChecks("/health");
```

## 5. 数据库架构设计（MySQL）

### 5.1 分库分表策略
#### 垂直分库
```
writing_platform/
├── user_db/                    # 用户相关
│   ├── users                  # 用户表
│   ├── user_profiles          # 用户资料
│   ├── friendships            # 好友关系
│   └── user_sessions          # 用户会话
├── content_db/                 # 内容相关
│   ├── works                  # 作品表
│   ├── work_versions          # 作品版本
│   ├── comments               # 评论表
│   └── favorites              # 收藏表
├── payment_db/                 # 支付相关
│   ├── donations              # 打赏记录
│   ├── transactions           # 交易记录
│   ├── balances               # 余额表
│   └── withdrawals            # 提现记录
└── system_db/                  # 系统相关
    ├── notifications          # 通知表
    ├── system_logs            # 系统日志
    └── audit_logs             # 审计日志
```

#### 水平分表（Sharding）
- **用户表**：按 `user_id % 1024` 分1024个表
- **作品表**：按 `author_id % 256` 分256个表  
- **评论表**：按 `work_id % 512` 分512个表
- **聊天消息**：按时间范围分区（每月一个表）

### 5.2 数据库优化方案
#### 5.2.1 索引设计
```sql
-- 用户表复合索引
CREATE INDEX idx_users_email_status ON users(email, status);
CREATE INDEX idx_users_created_at ON users(created_at DESC);

-- 作品表复合索引
CREATE INDEX idx_works_author_status ON works(author_id, status, created_at);
CREATE INDEX idx_works_category_rating ON works(category, rating_score DESC);
CREATE INDEX idx_works_fulltext ON works(title, content) USING FULLTEXT;

-- 评论表索引
CREATE INDEX idx_comments_work_created ON comments(work_id, created_at DESC);
CREATE INDEX idx_comments_user_created ON comments(user_id, created_at DESC);
```

#### 5.2.2 读写分离
```
主库集群（3节点） → 同步复制 → 从库集群（6节点）
                    ↓
            读写分离中间件（ProxySQL）
                    ↓
           应用层（自动负载均衡）
```

#### 5.2.3 连接池配置
```csharp
// appsettings.json
{
  "ConnectionStrings": {
    "Default": "Server=mysql-master;Port=3306;Database=writing_platform;Uid=app_user;Pwd=StrongPassword123;Pooling=true;MinimumPoolSize=10;MaximumPoolSize=500;ConnectionIdleTimeout=300;ConnectionLifeTime=1800;"
  }
}
```

### 5.3 Vitess分片方案
```
┌─────────────────────────────────────┐
│           Vitess集群                │
│  ┌─────────┐  ┌─────────┐          │
│  │  Vtgate │  │  Vtgate │          │
│  │ (代理层)│  │ (代理层)│          │
│  └─────────┘  └─────────┘          │
│        │              │             │
│  ┌─────────┐  ┌─────────┐          │
│  │ Vtctld  │  │ Vtctld  │          │
│  │(控制层) │  │(控制层) │          │
│  └─────────┘  └─────────┘          │
│        │              │             │
│  ┌─────────┐  ┌─────────┐          │
│  │ Vttablet│  │ Vttablet│          │
│  │(数据层) │  │(数据层) │          │
│  └─────────┘  └─────────┘          │
│        │              │             │
│  ┌─────────┐  ┌─────────┐          │
│  │ MySQL   │  │ MySQL   │          │
│  │ Shard1  │  │ Shard2  │          │
│  └─────────┘  └─────────┘          │
└─────────────────────────────────────┘
```

## 6. 缓存架构设计（Redis）

### 6.1 Redis集群部署
```
Redis Cluster (6主6从，12节点)
├── 分片策略：CRC16哈希槽（16384个槽）
├── 主从复制：异步复制 + 故障自动转移
├── 持久化：RDB快照 + AOF追加
└── 内存策略：allkeys-lru + 最大内存64GB/节点
```

### 6.2 多级缓存设计
```
┌─────────────────────────────────────────┐
│           客户端请求                      │
└─────────────────────────────────────────┘
                    ↓
┌─────────────────────────────────────────┐
│      L1缓存：内存缓存（C# MemoryCache）    │
│      缓存时间：5-30秒，命中率≈40%         │
└─────────────────────────────────────────┘
                    ↓
┌─────────────────────────────────────────┐
│   L2缓存：Redis集群（热点数据）            │
│   缓存时间：5分钟-24小时，命中率≈50%       │
└─────────────────────────────────────────┘
                    ↓
┌─────────────────────────────────────────┐
│            MySQL数据库                   │
└─────────────────────────────────────────┘
```

### 6.3 缓存策略示例
```csharp
public class CachedUserRepository : IUserRepository
{
    private readonly IUserRepository _decorated;
    private readonly IDistributedCache _cache;
    private readonly ILogger<CachedUserRepository> _logger;
    
    public CachedUserRepository(IUserRepository decorated, 
        IDistributedCache cache, 
        ILogger<CachedUserRepository> logger)
    {
        _decorated = decorated;
        _cache = cache;
        _logger = logger;
    }
    
    public async Task<User> GetByIdAsync(long userId, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"user:{userId}";
        
        // 1. 尝试从缓存获取
        var cachedUser = await _cache.GetStringAsync(cacheKey, cancellationToken);
        if (!string.IsNullOrEmpty(cachedUser))
        {
            _logger.LogDebug("Cache hit for user {UserId}", userId);
            return JsonSerializer.Deserialize<User>(cachedUser);
        }
        
        // 2. 缓存未命中，查询数据库
        _logger.LogDebug("Cache miss for user {UserId}, querying database", userId);
        var user = await _decorated.GetByIdAsync(userId, cancellationToken);
        
        if (user != null)
        {
            // 3. 写入缓存（设置不同过期时间）
            var cacheOptions = new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(
                    userId % 100 == 0 ? 30 : 5) // 热门用户缓存更久
            };
            
            await _cache.SetStringAsync(
                cacheKey, 
                JsonSerializer.Serialize(user), 
                cacheOptions, 
                cancellationToken);
        }
        
        return user;
    }
}
```

### 6.4 缓存击穿/穿透/雪崩防护
```csharp
// 1. 布隆过滤器防止缓存穿透
public class BloomFilterService
{
    private readonly IBloomFilter _bloomFilter;
    
    public bool MightContain(long userId) => _bloomFilter.Contains(userId);
    public void Add(long userId) => _bloomFilter.Add(userId);
}

// 2. 互斥锁防止缓存击穿
public async Task<User> GetUserWithLockAsync(long userId)
{
    var cacheKey = $"user:{userId}";
    var lockKey = $"lock:{cacheKey}";
    
    // 尝试获取分布式锁
    var lockAcquired = await _distributedLock.AcquireAsync(lockKey, TimeSpan.FromSeconds(5));
    
    if (!lockAcquired)
    {
        // 获取锁失败，返回空或重试
        return null;
    }
    
    try
    {
        // 双重检查
        var cachedUser = await _cache.GetStringAsync(cacheKey);
        if (!string.IsNullOrEmpty(cachedUser))
            return JsonSerializer.Deserialize<User>(cachedUser);
        
        // 查询数据库
        var user = await QueryDatabaseAsync(userId);
        
        if (user != null)
        {
            // 设置随机过期时间，防止雪崩
            var random = new Random();
            var expiration = TimeSpan.FromMinutes(5 + random.Next(0, 10));
            await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(user), 
                new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = expiration });
        }
        else
        {
            // 缓存空值，防止穿透
            await _cache.SetStringAsync(cacheKey, "NULL", 
                new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1) });
        }
        
        return user;
    }
    finally
    {
        await _distributedLock.ReleaseAsync(lockKey);
    }
}
```

## 7. 消息队列架构（Kafka）

### 7.1 Kafka集群部署
```
Kafka Cluster (3节点)
├── Zookeeper集群 (3节点，管理元数据)
├── Topic分区：根据业务量设置（如user_events: 16分区）
├── 副本因子：3（保证高可用）
├── 保留策略：7天（根据磁盘容量调整）
└── 监控：Kafka Manager + Prometheus
```

### 7.2 业务事件流设计
```csharp
// 事件定义
public abstract class DomainEvent
{
    public string EventId { get; } = Guid.NewGuid().ToString();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public string EventType => GetType().Name;
}

public class UserRegisteredEvent : DomainEvent
{
    public long UserId { get; }
    public string Username { get; }
    public string Email { get; }
    
    public UserRegisteredEvent(long userId, string username, string email)
    {
        UserId = userId;
        Username = username;
        Email = email;
    }
}

public class WorkPublishedEvent : DomainEvent
{
    public long WorkId { get; }
    public long AuthorId { get; }
    public string Title { get; }
    
    public WorkPublishedEvent(long workId, long authorId, string title)
    {
        WorkId = workId;
        AuthorId = authorId;
        Title = title;
    }
}

// 事件生产者
public class KafkaEventPublisher : IEventPublisher
{
    private readonly IProducer<Null, string> _producer;
    
    public async Task PublishAsync(DomainEvent @event)
    {
        var message = new Message<Null, string>
        {
            Value = JsonSerializer.Serialize(@event, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            })
        };
        
        await _producer.ProduceAsync($"domain_events_{@event.EventType}", message);
    }
}

// 事件消费者
public class NotificationEventConsumer : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var consumer = new ConsumerBuilder<Ignore, string>(_consumerConfig).Build();
        consumer.Subscribe("domain_events_UserRegisteredEvent");
        
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var consumeResult = consumer.Consume(stoppingToken);
                var @event = JsonSerializer.Deserialize<UserRegisteredEvent>(consumeResult.Message.Value);
                
                // 处理事件：发送欢迎邮件
                await _emailService.SendWelcomeEmailAsync(@event.Email, @event.Username);
                
                consumer.Commit(consumeResult);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing Kafka message");
            }
        }
    }
}
```

## 8. API网关设计（Kong）

### 8.1 Kong集群配置
```
Kong集群 (3节点 + PostgreSQL)
├── 控制平面：Kong Manager (UI管理)
├── 数据平面：Kong节点（处理流量）
├── 插件体系：认证、限流、日志、监控
└── 高可用：Keepalived + VIP
```

### 8.2 网关路由配置
```yaml
# kong.yaml
_format_version: "2.1"
services:
  - name: user-service
    url: http://user-service:5000
    routes:
      - name: user-routes
        paths: ["/api/v1/users", "/api/v1/auth"]
        strip_path: true
        methods: ["GET", "POST", "PUT", "DELETE"]
        
  - name: writing-service  
    url: http://writing-service:5001
    routes:
      - name: writing-routes
        paths: ["/api/v1/works", "/api/v1/templates"]
        strip_path: true
        
plugins:
  - name: rate-limiting
    service: user-service
    config:
      minute: 100
      hour: 1000
      policy: local
      
  - name: jwt
    service: user-service
    config:
      uri_param_names: ["token"]
      claims_to_verify: ["exp"]
      
  - name: cors
    config:
      origins: ["https://app.writing.com", "https://admin.writing.com"]
      methods: ["GET", "POST", "PUT", "DELETE", "OPTIONS"]
      headers: ["Accept", "Authorization", "Content-Type"]
      credentials: true
```

## 9. 监控与告警体系

### 9.1 监控架构
```
┌─────────────────────────────────────────────────────┐
│                应用层监控                            │
│  ┌─────────┐  ┌─────────┐  ┌─────────┐             │
│  │ .NET应用│  │  Redis  │  │ MySQL   │             │
│  │ 指标    │  │  指标   │  │  指标   │             │
│  └─────────┘  └─────────┘  └─────────┘             │
│          │            │            │                │
└──────────┼────────────┼────────────┼────────────────┘
           ▼            ▼            ▼
┌─────────────────────────────────────────────────────┐
│                Prometheus集群                        │
│  ┌─────────┐  ┌─────────┐  ┌─────────┐             │
│  │ 抓取器  │  │ 抓取器  │  │ 抓取器  │             │
│  └─────────┘  └─────────┘  └─────────┘             │
└─────────────────────────────────────────────────────┘
                           │
                           ▼
┌─────────────────────────────────────────────────────┐
│                Grafana可视化                         │
│  ┌─────────────────────────────────────────────┐   │
│  │         仪表盘：QPS、延迟、错误率              │   │
│  └─────────────────────────────────────────────┘   │
└─────────────────────────────────────────────────────┘
                           │
                           ▼
┌─────────────────────────────────────────────────────┐
│                Alertmanager告警                      │
│  ┌─────────┐  ┌─────────┐  ┌─────────┐             │
│  │ 邮件    │  │ 钉钉    │  │ Webhook │             │
│  └─────────┘  └─────────┘  └─────────┘             │
└─────────────────────────────────────────────────────┘
```

### 9.2 关键监控指标
```yaml
# prometheus.yml
global:
  scrape_interval: 15s
  evaluation_interval: 15s

rule_files:
  - "alert_rules.yml"

scrape_configs:
  - job_name: 'dotnet-apps'
    static_configs:
      - targets: ['user-service:5000', 'writing-service:5001']
    metrics_path: '/metrics'
    
  - job_name: 'mysql'
    static_configs:
      - targets: ['mysql-exporter:9104']
      
  - job_name: 'redis'
    static_configs:
      - targets: ['redis-exporter:9121']
      
  - job_name: 'kong'
    static_configs:
      - targets: ['kong:8001']
```

### 9.3 告警规则
```yaml
# alert_rules.yml
groups:
  - name: application_alerts
    rules:
      - alert: HighErrorRate
        expr: rate(http_requests_total{status=~"5.."}[5m]) / rate(http_requests_total[5m]) > 0.05
        for: 2m
        labels:
          severity: critical
        annotations:
          summary: "高错误率检测"
          description: "{{ $labels.service }} 错误率超过5% (当前值: {{ $value }})"
          
      - alert: HighLatency
        expr: histogram_quantile(0.95, rate(http_request_duration_seconds_bucket[5m])) > 1
        for: 5m
        labels:
          severity: warning
        annotations:
          summary: "高延迟检测"
          description: "{{ $labels.service }} P95延迟超过1秒 (当前值: {{ $value }}s)"
```

## 10. 部署与运维

### 10.1 Kubernetes部署配置
```yaml
# user-service-deployment.yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: user-service
spec:
  replicas: 6
  selector:
    matchLabels:
      app: user-service
  template:
    metadata:
      labels:
        app: user-service
    spec:
      containers:
      - name: user-service
        image: writingplatform/user-service:1.0.0
        ports:
        - containerPort: 5000
        resources:
          requests:
            memory: "512Mi"
            cpu: "250m"
          limits:
            memory: "1Gi"
            cpu: "500m"
        livenessProbe:
          httpGet:
            path: /health
            port: 5000
          initialDelaySeconds: 30
          periodSeconds: 10
        readinessProbe:
          httpGet:
            path: /ready
            port: 5000
          initialDelaySeconds: 5
          periodSeconds: 5
---
apiVersion: v1
kind: Service
metadata:
  name: user-service
spec:
  selector:
    app: user-service
  ports:
  - port: 80
    targetPort: 5000
  type: ClusterIP
```

### 10.2 HPA自动伸缩配置
```yaml
apiVersion: autoscaling/v2
kind: HorizontalPodAutoscaler
metadata:
  name: user-service-hpa
spec:
  scaleTargetRef:
    apiVersion: apps/v1
    kind: Deployment
    name: user-service
  minReplicas: 3
  maxReplicas: 20
  metrics:
  - type: Resource
    resource:
      name: cpu
      target:
        type: Utilization
        averageUtilization: 70
  - type: Resource
    resource:
      name: memory
      target:
        type: Utilization
        averageUtilization: 80
  - type: Pods
    pods:
      metric:
        name: http_requests_per_second
      target:
        type: AverageValue
        averageValue: 1000
```

### 10.3 CI/CD流水线
```yaml
# .github/workflows/deploy.yml
name: Deploy to Kubernetes

on:
  push:
    branches: [ main ]

jobs:
  build-and-deploy:
    runs-on: ubuntu-latest
    steps:
    - uses: actions/checkout@v3
    
    - name: Set up .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: '8.0.x'
        
    - name: Restore dependencies
      run: dotnet restore
      
    - name: Build
      run: dotnet build --configuration Release --no-restore
      
    - name: Test
      run: dotnet test --no-build --verbosity normal
      
    - name: Publish
      run: dotnet publish -c Release -o ./publish
      
    - name: Build Docker image
      run: |
        docker build -t ${{ secrets.DOCKER_USERNAME }}/user-service:${{ github.sha }} .
        docker push ${{ secrets.DOCKER_USERNAME }}/user-service:${{ github.sha }}
        
    - name: Deploy to Kubernetes
      uses: azure/k8s-deploy@v1
      with:
        namespace: writing-platform
        manifests: k8s/
        images: |
          ${{ secrets.DOCKER_USERNAME }}/user-service:${{ github.sha }}
        kubectl-version: 'latest'
```

## 11. 安全架构

### 11.1 安全防护层
```
1. 网络层安全
   ├── DDoS防护（Cloudflare/阿里云盾）
   ├── WAF（Web应用防火墙）
   └── VPC网络隔离

2. 应用层安全
   ├── JWT认证 + OAuth2.0
   ├── 输入验证 + XSS防护
   ├── SQL注入防护
   └── 文件上传安全

3. 数据层安全
   ├── 传输加密（TLS 1.3）
   ├── 静态加密（AES-256）
   ├── 数据脱敏
   └── 访问审计

4. 业务层安全
   ├── 交易风控
   ├── 反欺诈系统
   ├── 内容审核
   └── 权限控制（RBAC）
```

### 11.2 合规性要求
- **GDPR**：用户数据可携带权、删除权
- **网络安全法**：数据本地化存储
- **支付合规**：PCI DSS认证
- **内容合规**：内容审核机制

## 12. 性能测试方案

### 12.1 压测工具链
```
┌─────────────┐    ┌─────────────┐    ┌─────────────┐
│   JMeter    │───▶│  Gatling    │───▶│   k6.io     │
│ (脚本录制)   │    │(Scala DSL)  │    │(JavaScript) │
└─────────────┘    └─────────────┘    └─────────────┘
        │                 │                  │
        └─────────────────┼──────────────────┘
                          ▼
                ┌─────────────────┐
                │ 性能测试环境     │
                │ (隔离K8s集群)   │
                └─────────────────┘
```

### 12.2 压测场景
| 场景 | 并发用户 | 测试目标 | 通过标准 |
|------|----------|----------|----------|
| **登录场景** | 10万 | 登录接口TPS | TPS > 5000，P95 < 200ms |
| **作品发布** | 5万 | 发布接口TPS | TPS > 1000，P95 < 500ms |
| **作品浏览** | 50万 | 查询接口QPS | QPS > 10000，P95 < 100ms |
| **实时聊天** | 20万 | WebSocket连接 | 连接成功率 > 99.9% |
| **支付场景** | 1万 | 支付接口TPS | TPS > 500，P95 < 300ms |

## 13. 成本估算

### 13.1 基础设施成本（月度）
| 资源 | 规格 | 数量 | 月成本（估算） |
|------|------|------|----------------|
| **K8s节点** | 32核64GB | 20台 | $8,000 |
| **MySQL集群** | 16核32GB | 9台 | $4,500 |
| **Redis集群** | 8核16GB | 12台 | $2,400 |
| **Kafka集群** | 8核16GB | 3台 | $600 |
| **CDN流量** | 100TB | - | $2,000 |
| **对象存储** | 500TB | - | $1,000 |
| **监控服务** | - | - | $500 |
| **总计** | - | - | **$19,000** |

### 13.2 优化建议
1. **弹性伸缩**：根据流量自动调整资源
2. **预留实例**：购买1-3年预留实例节省30-50%
3. **混合云**：非核心业务使用公有云，核心业务自建
4. **成本监控**：建立成本预警机制

## 14. 总结

### 14.1 架构优势
1. **高性能**：微服务架构 + 多级缓存 + 异步处理
2. **高可用**：多副本 + 故障自动转移 + 多地域部署
3. **可扩展**：水平扩展能力，弹性伸缩
4. **易维护**：容器化部署，标准化运维

### 14.2 实施建议
1. **分阶段实施**：先核心功能，后扩展功能
2. **灰度发布**：新功能逐步放量，降低风险
3. **持续监控**：建立完善的监控告警体系
4. **容灾演练**：定期进行故障恢复演练

### 14.3 未来扩展
1. **AI能力增强**：集成更多AI模型，提升写作辅助效果
2. **国际化支持**：多语言界面，全球化部署
3. **区块链应用**：作品版权登记，交易透明化
4. **AR/VR体验**：沉浸式写作环境

---
*本架构设计基于百万并发目标，实际实施时可根据业务发展情况分阶段推进。*

*最后更新：2026-03-31*