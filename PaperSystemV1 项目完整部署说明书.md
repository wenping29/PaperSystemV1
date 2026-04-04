# PaperSystemV1 项目完整部署说明书

# 一、文档说明

本文档用于指导 PaperSystemV1 项目（微服务 \+ Web 端 \+ 安卓端）在 Ubuntu 服务器上的本地化部署、运维操作，包含全套流程、脚本使用、常见问题排查，适用于开发、测试及生产环境，全程一键化操作，无需复杂配置。

项目架构：\.NET 微服务（多服务）\+ Vite 前端 Web \+ Android 移动端 \+ Nginx 代理 \+ MySQL 数据库 \+ Redis 缓存

服务器环境：Ubuntu（任意版本，推荐 20\.04/22\.04）

本地环境：Windows 10/11（用于打包微服务、前端、安卓 APK）

核心地址：服务器 IP 8\.136\.202\.27，前端/Web 访问 80 端口，微服务统一通过 Nginx 代理访问

# 二、前期准备

## 2\.1 本地 Windows 准备

- 安装 \.NET SDK（版本与项目一致，推荐 \.NET 8）

- 安装 Node\.js（用于前端打包，推荐 v16\+）

- 安装 Android Studio（配置 Android SDK，用于安卓打包，无需打开 Studio，仅需配置环境）

- 确保项目路径正确：D:\\Project\\PaperSystemV1（脚本默认路径，不可修改）

- 配置服务器 SSH 连接（本地可通过 scp 上传文件，需提前开启服务器 SSH 服务）

## 2\.2 Ubuntu 服务器准备

- 确保服务器能正常访问外网（用于安装依赖、下载软件）

- 创建项目目录：mkdir \-p /www/wwwroot/PaperServer（脚本默认目录，不可修改）

- 安装基础依赖：apt update \&amp;\&amp; apt install \-y openssh\-server nginx mysql\-server redis\-server（后续脚本会自动补装，提前安装可节省时间）

- 配置 MySQL 数据库：创建 PaperSystem 数据库，设置用户名（root）、密码（自行修改），确保微服务能正常连接

## 2\.3 项目文件准备

将全套部署脚本（本地 Windows 脚本 \+ 服务器脚本）按以下路径放置，确保文件完整：

```plain text
D:\Project\PaperSystemV1\
├── deploy\                  # 部署脚本目录
│   ├── windows\             # 本地 Windows 打包/上传脚本
│   │   ├── publish-all.bat  # 微服务批量打包
│   │   ├── deploy-to-server.bat # 微服务上传服务器
│   │   ├── build-web.bat    # 前端打包
│   │   └── build-apk.bat    # 安卓APK打包
├── src\
│   ├── Services\            # 微服务目录（每个子目录对应一个服务）
│   ├── Web\                 # 前端 Vite 项目
│   ├── Mobile\              # 安卓项目
│   └── Publish\             # 微服务打包输出目录
├── apk_output\              # 安卓APK输出目录（自动生成）
/www/wwwroot/PaperServer/    # 服务器项目目录
├── start-all.sh             # 一键启动所有服务
├── stop-all.sh              # 一键停止所有服务
├── restart-all.sh           # 一键重启所有服务
├── update.sh                # 一键更新服务
├── status.sh                # 检查服务在线状态
├── logs.sh                  # 查看所有服务日志
├── backup-db.sh             # 数据库自动备份
├── clean.sh                 # 清理日志/缓存
└── Web/                     # 前端 dist 目录（上传后放置）
```

# 三、本地打包流程（Windows 操作）

所有操作均在 D:\\Project\\PaperSystemV1\\deploy\\windows\\ 目录下执行，双击对应脚本即可，无需手动输入命令。

## 3\.1 微服务打包（publish\-all\.bat）

### 功能

自动扫描 src/Services 下所有微服务，批量打包为 Linux\-x64 独立单文件（无依赖，无需在服务器安装 \.NET 运行时），输出到 src/Publish 目录。

### 操作步骤

1. 双击 publish\-all\.bat，脚本自动执行清理、构建、打包流程。

2. 等待打包完成，提示“所有微服务打包完成”即可。

3. 检查 src/Publish 目录，每个微服务对应一个子目录，目录内包含可直接在 Linux 运行的执行文件。

### 注意事项

确保每个微服务目录下有 【服务名\.csproj】 文件（如 UserService/UserService\.csproj），否则脚本无法识别。

## 3\.2 前端 Web 打包（build\-web\.bat）

### 功能

自动安装前端依赖、打包 Vite 项目，生成 dist 目录（生产环境前端文件）。

### 操作步骤

1. 双击 build\-web\.bat，脚本自动进入 src/Web 目录，执行 npm install 和 npm run build。

2. 打包完成后，dist 目录生成在 src/Web/dist 下。

### 注意事项

若前端依赖安装失败，可手动进入 src/Web 目录，执行 npm install 后再重新运行脚本。

## 3\.3 安卓 APK 打包（build\-apk\.bat）

### 功能

自动清理安卓项目、构建 Release 版本 APK，输出到 apk\_output 目录，可直接用于安装、测试，配置签名后可上架。

### 前置配置（必须做）

在 src/Mobile/app/build\.gradle 中配置签名信息（无签名 APK 无法覆盖安装、无法上架）：

```gradle
android {
    signingConfigs {
        release {
            keyAlias '你的签名别名'       // 自行设置（如 papersystem）
            keyPassword '你的签名密码'     // 自行设置
            storeFile file('../你的签名文件.jks') // 签名文件路径（放在 Mobile 目录下）
            storePassword '你的仓库密码'   // 与签名密码一致或自行设置
        }
    }

    buildTypes {
        release {
            signingConfig signingConfigs.release // 关联签名配置
            minifyEnabled false
            proguardFiles getDefaultProguardFile('proguard-android-optimize.txt'), 'proguard-rules.pro'
        }
    }
}
```

### 操作步骤

1. 双击 build\-apk\.bat，脚本自动进入安卓项目目录，执行清理、构建、打包。

2. 打包完成后，APK 文件输出到 D:\\Project\\PaperSystemV1\\apk\_output 目录。

### 注意事项

- 确保 Android SDK 环境配置正确，脚本可自动识别 gradlew 命令。

- 若无签名文件，可通过 Android Studio 生成 \.jks 签名文件，放置在 src/Mobile 目录下。

# 四、服务器部署流程（Ubuntu 操作）

所有操作均在服务器 /www/wwwroot/PaperServer/ 目录下执行，先完成文件上传，再执行部署命令。

## 4\.1 上传文件到服务器

### 方法1：使用本地脚本自动上传（推荐）

1. 在 Windows 本地，双击 deploy\-to\-server\.bat。

2. 脚本自动将 src/Publish 下所有微服务上传到服务器 /www/wwwroot/PaperServer/ 目录。

3. 手动上传前端 dist 目录：将 src/Web/dist 下所有文件，上传到服务器 /www/wwwroot/PaperServer/Web/ 目录（可通过 scp、Xftp 等工具）。

### 方法2：手动上传（备用）

通过 Xftp、WinSCP 等工具，将以下文件上传到对应目录：

- 微服务：src/Publish 下所有子目录 → 服务器 /www/wwwroot/PaperServer/

- 前端：src/Web/dist 所有文件 → 服务器 /www/wwwroot/PaperServer/Web/

- 服务器脚本：所有 \.sh 脚本 → 服务器 /www/wwwroot/PaperServer/

## 4\.2 脚本赋权（仅执行一次）

进入服务器项目目录，给所有 \.sh 脚本赋予执行权限：

```bash
cd /www/wwwroot/PaperServer
chmod +x *.sh
```

## 4\.3 Nginx 代理配置（仅执行一次）

### 配置文件创建

创建 Nginx 配置文件：

```bash
nano /etc/nginx/sites-available/papersystem.conf
```

粘贴以下内容（无需修改，适配当前项目）：

```nginx
server {
    listen 80;
    server_name 8.136.202.27;

    # 前端 Web 访问（根路径）
    location / {
        root /www/wwwroot/PaperServer/Web;
        index index.html;
        try_files $uri $uri/ /index.html;
    }

    # 微服务 API 代理（/api 路径）
    location /api {
        proxy_pass http://127.0.0.1:5100; # 微服务统一端口（若多服务需调整）
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_connect_timeout 60s;
        proxy_send_timeout 60s;
        proxy_read_timeout 60s;
    }
}
```

### 启用配置并重启 Nginx

```bash
ln -s /etc/nginx/sites-available/papersystem.conf /etc/nginx/sites-enabled/
nginx -t # 检查配置是否正确
systemctl restart nginx # 重启 Nginx
```

## 4\.4 开机自启配置（仅执行一次）

配置系统级开机自启，确保服务器重启后，所有服务自动启动。

### 创建服务文件

```bash
nano /etc/systemd/system/papersystem.service
```

粘贴以下内容：

```ini
[Unit]
Description=PaperSystem 微服务集群
After=network.target redis-server.service mysql.service

[Service]
Type=forking
User=root
WorkingDirectory=/www/wwwroot/PaperServer
ExecStart=/www/wwwroot/PaperServer/start-all.sh
Restart=always
RestartSec=5 # 服务崩溃后，5秒自动重启

[Install]
WantedBy=multi-user.target
```

### 启用开机自启

```bash
systemctl daemon-reload
systemctl enable papersystem # 设置开机自启
systemctl start papersystem # 启动服务
```

## 4\.5 启动所有服务

执行以下命令，一键启动所有微服务、Redis 等依赖：

```bash
cd /www/wwwroot/PaperServer
./start-all.sh
```

启动成功后，会提示每个服务的启动状态，显示“所有服务启动完成”即可。

# 五、服务器运维操作（常用命令）

所有命令均在 /www/wwwroot/PaperServer/ 目录下执行，一键操作，无需复杂输入。

## 5\.1 服务启停相关

|命令|功能|
|---|---|
|\./start\-all\.sh|一键启动所有微服务、安装依赖（Redis 等）|
|\./stop\-all\.sh|一键停止所有微服务|
|\./restart\-all\.sh|一键重启所有微服务（先停止，再启动）|
|\./update\.sh|更新服务（上传新版本后，执行此命令，自动停止→启动）|

## 5\.2 服务状态与日志

|命令|功能|
|---|---|
|\./status\.sh|检查所有微服务是否在线，显示运行中的 PID|
|\./logs\.sh|实时查看所有微服务的日志（nohup\.out），按 Ctrl\+C 退出|
|tail \-f 服务目录/nohup\.out|查看单个服务的日志（如 tail \-f UserService/nohup\.out）|

## 5\.3 数据库与清理

|命令|功能|
|---|---|
|\./backup\-db\.sh|自动备份 MySQL 数据库（PaperSystem），压缩存储，保留7天备份，自动清理旧备份|
|\./clean\.sh|清空所有微服务日志、删除7天前的系统日志，清理缓存|

# 六、常见问题排查

## 6\.1 微服务启动失败

- 现象：\./status\.sh 显示服务未运行，logs\.sh 报错“Address already in use”

- 原因：端口被占用

- 解决：执行 pkill \-f 服务名（如 pkill \-f UserService），再重新启动 \./start\-all\.sh

- 现象：logs\.sh 报错“缺少依赖”

- 原因：未安装 libicu\-dev、libssl\-dev 等依赖

- 解决：执行 apt install \-y libicu\-dev libssl\-dev，再重启服务

- 现象：logs\.sh 报错“Redis 连接失败”

- 原因：Redis 未启动或配置错误

- 解决：执行 systemctl start redis\-server，检查 appsettings\.json 中 Redis 连接字符串

## 6\.2 前端访问失败

- 现象：浏览器访问 8\.136\.202\.27 空白或 404

- 原因：前端文件未上传、Nginx 配置错误

- 解决：重新上传前端 dist 文件到 /www/wwwroot/PaperServer/Web/，执行 nginx \-t 检查配置，重启 Nginx

## 6\.3 接口访问报错（500/404）

- 现象：前端调用 /api 接口报错 500

- 原因：微服务未启动、JWT 配置错误、数据库连接失败

- 解决：1\. 用 \./status\.sh 检查服务是否在线；2\. 查看服务日志（logs\.sh），排查 JWT 密钥、数据库连接字符串

- 现象：接口报错 401（未授权）

- 原因：前端未携带 Token 或 Token 过期

- 解决：重新调用 /api/v1/auth/login 获取 Token，确保请求头携带 Authorization: Bearer Token

## 6\.4 安卓 APK 打包失败

- 现象：脚本报错“找不到 gradlew”

- 原因：Android SDK 环境未配置

- 解决：打开 Android Studio，配置 SDK 路径，确保 gradlew 命令可正常执行

- 现象：打包成功但无法安装

- 原因：未配置签名或签名错误

- 解决：检查 build\.gradle 中的签名配置，确保签名文件路径、密码正确

# 七、补充说明

- 备份文件：数据库备份默认存储在 /www/wwwroot/PaperServer/backups 目录，保留7天，可手动复制备份到其他位置。

- 日志文件：每个微服务的日志存储在自身目录下的 nohup\.out，可定期执行 \./clean\.sh 清理，避免占用过多磁盘空间。

- 版本更新：微服务更新时，本地重新打包后，执行 deploy\-to\-server\.bat 上传，再在服务器执行 \./update\.sh 即可完成更新，无需手动停止服务。

- 安全建议：生产环境中，修改 MySQL 密码、JWT 密钥，关闭服务器不必要的端口，定期备份数据库。

# 八、联系方式（可选）

若遇到无法解决的问题，可参考以下方式排查：

- 查看服务日志：\./logs\.sh

- 检查 Nginx 日志：tail \-f /var/log/nginx/error\.log

- 检查 MySQL 日志：tail \-f /var/log/mysql/error\.log

> （注：文档部分内容可能由 AI 生成）
