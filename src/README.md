# Writing Platform - 后端项目

基于C# .NET 8 WebAPI Core的微服务架构，支持百万级并发。

## 项目结构

```
src/
├── WritingPlatform.sln                    # 解决方案文件
├── Services/                              # 微服务项目
│   ├── UserService/          # 用户服务
│   ├── WritingService/       # 作品服务
│   ├── CommunityService/     # 社区服务
│   ├── AIService/            # AI服务
│   ├── PaymentService/       # 支付服务
│   ├── ChatService/          # 聊天服务
│   ├── FileService/          # 文件服务
│   ├── SearchService/        # 搜索服务
│   ├── FriendshipService/    # 好友服务
│   └── NotificationService/  # 通知服务
├── Shared/                               # 共享类库
│   ├── Shared.Core/      # 核心共享库
│   ├── Shared.Models/    # 共享数据模型
│   └── Shared.Infrastructure/ # 共享基础设施
└── Tests/                                # 测试项目
    └── UserService.Tests/ # 用户服务测试
```

## 技术栈

- **框架**: .NET 8 WebAPI Core
- **数据库**: MySQL 8.0 + Entity Framework Core
- **缓存**: Redis Cluster + StackExchange.Redis
- **认证**: JWT + OAuth2.0
- **消息队列**: Apache Kafka (待集成)
- **API网关**: Kong/Nginx
- **容器化**: Docker + Kubernetes
- **监控**: Prometheus + Grafana + ELK

## 快速开始

### 前提条件

1. **.NET 8 SDK**: [下载地址](https://dotnet.microsoft.com/download/dotnet/8.0)
2. **MySQL 8.0+**: 本地安装或使用Docker
3. **Redis 7.0+**: 本地安装或使用Docker
4. **IDE推荐**: Visual Studio 2022+ 或 JetBrains Rider

### 运行UserService示例

1. **配置数据库连接**
   - 修改 `Services/UserService/appsettings.json` 中的连接字符串
   - 确保MySQL服务运行在localhost:3306

2. **运行服务**
   ```bash
   cd src/Services/UserService
   dotnet run
   ```

3. **访问API文档**
   - 打开浏览器访问: http://localhost:5000/api-docs
   - 健康检查: http://localhost:5000/health

### 运行所有服务

1. **还原解决方案包**
   ```bash
   cd src
   dotnet restore
   ```

2. **构建解决方案**
   ```bash
   dotnet build
   ```

3. **运行测试**
   ```bash
   dotnet test
   ```

## 服务端口配置

每个服务使用不同的端口以避免冲突：

| 服务 | 端口 | 说明 |
|------|------|------|
| UserService | 5000 | 用户管理、认证 |
| WritingService | 5001 | 作品管理、写作 |
| CommunityService | 5002 | 社区、评论、点赞 |
| AIService | 5003 | AI写作辅助、评分 |
| PaymentService | 5004 | 支付、打赏 |
| ChatService | 5005 | 实时聊天 |
| FileService | 5006 | 文件上传、存储 |
| SearchService | 5007 | 全文搜索 |
| FriendshipService | 5008 | 好友关系 |
| NotificationService | 5009 | 通知推送 |

## 数据库设计

### 分库分表策略
- **垂直分库**: 按业务领域分库（用户库、内容库、支付库、系统库）
- **水平分表**: 按ID哈希分表（用户表分1024片，作品表分256片）

### 数据库迁移
```bash
# 为UserService创建迁移
cd Services/UserService
dotnet ef migrations add InitialCreate --context UserDbContext
dotnet ef database update --context UserDbContext
```

## 缓存策略

### 多级缓存架构
1. **L1缓存**: 内存缓存（5-30秒，命中率≈40%）
2. **L2缓存**: Redis集群（5分钟-24小时，命中率≈50%）
3. **数据库**: MySQL（最终数据源）

### 缓存击穿/穿透/雪崩防护
- **布隆过滤器**: 防止缓存穿透
- **互斥锁**: 防止缓存击穿
- **随机过期时间**: 防止缓存雪崩

## 性能优化

### .NET 8 高性能配置
- Kestrel配置支持10,000并发连接
- 连接池优化（最小10，最大500连接）
- 响应压缩（Brotli + Gzip）
- 请求限流（每分钟100请求）

### 数据库优化
- 查询拆分（SplitQuery）
- 批量操作（MaxBatchSize: 100）
- 重试机制（最大5次重试）
- 连接复用（ConnectionPooling）

## 部署方案

### 开发环境
```bash
# 使用Docker Compose启动基础设施
docker-compose -f ../docker-compose.dev.yml up -d
```

### 生产环境
- **容器编排**: Kubernetes
- **服务网格**: Istio（可选）
- **监控**: Prometheus + Grafana
- **日志**: ELK Stack

## 开发指南

### 添加新服务
1. 创建新的WebAPI项目
   ```bash
   cd src/Services
   dotnet new webapi -n NewService --framework net8.0 --no-https
   ```

2. 添加到解决方案
   ```bash
   cd ..
   dotnet sln add Services/NewService
   ```

3. 添加必要的NuGet包
   ```bash
   cd Services/NewService
   dotnet add package Pomelo.EntityFrameworkCore.MySql
   dotnet add package Microsoft.Extensions.Caching.StackExchangeRedis
   dotnet add package AutoMapper.Extensions.Microsoft.DependencyInjection
   ```

4. 参考UserService或WritingService实现

### 添加共享代码
- **业务逻辑**: 放在 `Shared/Shared.Core`
- **数据模型**: 放在 `Shared/Shared.Models`
- **通用工具**: 放在 `Shared/Shared.Infrastructure`

## 测试

### 单元测试
```bash
# 运行所有测试
cd src
dotnet test

# 运行特定测试项目
dotnet test Tests/UserService.Tests
```

### 集成测试
- 使用Testcontainers进行数据库集成测试
- 使用WebApplicationFactory进行API集成测试

## 监控与调试

### 健康检查端点
- `/health`: 完整健康检查
- `/health/ready`: 就绪检查（数据库、缓存）
- `/health/live`: 存活检查

### 日志级别
- Development环境: Information级别
- Production环境: Warning级别
- 数据库命令: Warning级别（避免日志过多）

## 下一步

### 待完成事项
1. [ ] 实现所有服务的完整业务逻辑
2. [ ] 添加API网关（Kong/Nginx）
3. [ ] 配置消息队列（Kafka）
4. [ ] 实现分布式追踪（OpenTelemetry）
5. [ ] 配置CI/CD流水线
6. [ ] 添加压力测试脚本
7. [ ] 完善安全配置（JWT、OAuth2.0）
8. [ ] 实现数据库分片（Vitess）

### 扩展功能
1. [ ] 实时协作编辑（WebSocket + OT算法）
2. [ ] AI模型集成（GPT、ERNIE等）
3. [ ] 支付系统集成（微信/支付宝）
4. [ ] 搜索优化（Elasticsearch）
5. [ ] 推荐算法（协同过滤）

## 常见问题

### 端口冲突
修改 `Program.cs` 中的 `ListenAnyIP` 端口号，并更新对应的 `appsettings.json` 配置。

### 数据库连接失败
1. 检查MySQL服务是否运行
2. 验证连接字符串中的用户名/密码
3. 确认防火墙是否允许3306端口

### Redis连接失败
1. 检查Redis服务是否运行
2. 验证连接字符串格式
3. 确认Redis配置允许远程连接

### 依赖包还原失败
1. 检查网络连接
2. 清理NuGet缓存
   ```bash
   dotnet nuget locals all --clear
   ```
3. 重新还原
   ```bash
   dotnet restore --force
   ```

## 贡献指南

1. Fork项目
2. 创建功能分支
3. 提交更改
4. 推送到分支
5. 创建Pull Request

## 许可证

MIT License

## 联系方式

如有问题，请查看项目文档或提交Issue。

---
*最后更新: 2026-03-31*