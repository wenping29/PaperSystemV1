# 多数据库配置指南

本项目支持多种数据库，通过配置文件可以轻松切换。

## 支持的数据库

- ✅ **MySQL** (默认)
- ✅ **SQLite** (开发环境推荐)
- ✅ **Oracle**
- ⏳ **PostgreSQL** (待实现)
- ⏳ **SQL Server** (待实现)

## 配置方式

### 1. 设置数据库类型

在 `appsettings.json` 中配置：

```json
{
  "Database": {
    "Provider": "MySql",
    "EnableDetailedErrors": true,
    "EnableSensitiveDataLogging": false
  }
}
```

`Provider` 的可选值：
- `MySql` - MySQL 数据库
- `Sqlite` - SQLite 本地文件数据库
- `Oracle` - Oracle 数据库

### 2. 配置连接字符串

在 `ConnectionStrings` 部分配置对应数据库的连接字符串。

---

## 各数据库配置示例

### MySQL 配置

```json
{
  "Database": {
    "Provider": "MySql"
  },
  "ConnectionStrings": {
    "UserDatabase": "Server=localhost;Port=3306;Database=writing_platform_user;Uid=root;Pwd=your_password;",
    "WritingDatabase": "Server=localhost;Port=3306;Database=writing_platform_writing;Uid=root;Pwd=your_password;",
    "ChatDatabase": "Server=localhost;Port=3306;Database=writing_platform_chat;Uid=root;Pwd=your_password;",
    "FriendshipDatabase": "Server=localhost;Port=3306;Database=writing_platform_friendship;Uid=root;Pwd=your_password;",
    "FileDatabase": "Server=localhost;Port=3306;Database=writing_platform_file;Uid=root;Pwd=your_password;",
    "AIDatabase": "Server=localhost;Port=3306;Database=writing_platform_ai;Uid=root;Pwd=your_password;"
  }
}
```

### SQLite 配置

SQLite 是本地文件数据库，适合开发环境：

```json
{
  "Database": {
    "Provider": "Sqlite",
    "EnableDetailedErrors": true,
    "EnableSensitiveDataLogging": true
  },
  "ConnectionStrings": {
    "UserDatabase": "Data Source=Data/writing_platform_user.db",
    "WritingDatabase": "Data Source=Data/writing_platform_writing.db",
    "ChatDatabase": "Data Source=Data/writing_platform_chat.db",
    "FriendshipDatabase": "Data Source=Data/writing_platform_friendship.db",
    "FileDatabase": "Data Source=Data/writing_platform_file.db",
    "AIDatabase": "Data Source=Data/writing_platform_ai.db"
  }
}
```

数据库文件会自动创建在项目的 `bin/Debug/net8.0/Data/` 目录中。

### Oracle 配置

```json
{
  "Database": {
    "Provider": "Oracle"
  },
  "ConnectionStrings": {
    "UserDatabase": "User Id=writing_user;Password=YourPassword123;Data Source=localhost:1521/ORCLPDB1;",
    "WritingDatabase": "User Id=writing_writing;Password=YourPassword123;Data Source=localhost:1521/ORCLPDB1;",
    "ChatDatabase": "User Id=writing_chat;Password=YourPassword123;Data Source=localhost:1521/ORCLPDB1;",
    "FriendshipDatabase": "User Id=writing_friendship;Password=YourPassword123;Data Source=localhost:1521/ORCLPDB1;",
    "FileDatabase": "User Id=writing_file;Password=YourPassword123;Data Source=localhost:1521/ORCLPDB1;",
    "AIDatabase": "User Id=writing_ai;Password=YourPassword123;Data Source=localhost:1521/ORCLPDB1;"
  }
}
```

---

## 环境变量切换

也可以通过环境变量来指定使用哪个配置文件：

### Windows
```cmd
set ASPNETCORE_ENVIRONMENT=Development
dotnet run
```

### Linux/Mac
```bash
export ASPNETCORE_ENVIRONMENT=Development
dotnet run
```

### PowerShell
```powershell
$env:ASPNETCORE_ENVIRONMENT = "Development"
dotnet run
```

---

## 数据库迁移

### 创建迁移

首次使用或修改模型时，需要为每个 DbContext 创建迁移：

```bash
# 为 UserDbContext 创建迁移
dotnet ef migrations add InitialCreate -c UserDbContext -o Migrations/User

# 为 WritingDbContext 创建迁移
dotnet ef migrations add InitialCreate -c WritingDbContext -o Migrations/Writing
```

### 应用迁移

```bash
# 应用迁移到数据库
dotnet ef database update -c UserDbContext
dotnet ef database update -c WritingDbContext
```

---

## 验证配置

启动应用后，访问 `/health` 可以查看当前使用的数据库类型和连接状态：

```json
{
  "status": "Healthy",
  "databaseProvider": "MySql",
  "checks": [
    {
      "name": "UserDatabase",
      "status": "Healthy",
      "duration": 123.45
    },
    ...
  ]
}
```

Swagger UI 中也会显示当前使用的数据库类型。

---

## 注意事项

1. **首次使用 SQLite** 时，会自动在 `bin/Debug/net8.0/Data/` 目录下创建数据库文件
2. **Oracle 驱动** 可能需要额外安装 Oracle 客户端库
3. **连接字符串** 中的密码等敏感信息，生产环境应使用环境变量或机密管理
4. **迁移文件** 应该提交到源代码控制，因为迁移是与模型相关的
