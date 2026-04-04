一、本地 Windows 脚本
路径：D:\Project\PaperSystemV1\deploy\windows\
1. publish-all.bat（打包所有微服务）
bat
@echo off
chcp 65001
echo ==============================================
echo          .NET 微服务一键打包 Linux-x64
echo ==============================================

set CONFIG=Release
set RUNTIME=linux-x64

echo 正在打包 UserService...
dotnet publish../../src/Services/UserService/UserService.csproj -c %CONFIG% -r %RUNTIME% --self-contained true /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true /p:DebugType=None -o../../src/Publish/UserService/

echo.
echo ==============================================
echo  ✅ 打包完成：src/Publish/UserService
echo ==============================================
pause
2. deploy-to-server.bat（上传到服务器）
bat
@echo off
chcp 65001
echo ==============================================
echo           上传到 Ubuntu 服务器
echo ==============================================

set SERVER=root@8.136.202.27
set DEST=/www/wwwroot/PaperServer

echo 上传 UserService...
scp -r../../src/Publish/UserService %SERVER%:%DEST%/

echo.
echo ==============================================
echo  ✅ 上传完成
echo ==============================================
pause
3. build-web.bat（打包前端）
bat
@echo off
chcp 65001
cd../../src/Web
npm install
npm run build
echo.
echo ✅ 前端打包完成：dist/
pause
二、服务器 Ubuntu 脚本
放到服务器：/www/wwwroot/PaperServer/
1. start-all.sh（一键启动）
bash
运行
#!/bin/bash
BASE="/www/wwwroot/PaperServer"

echo "============================================="
echo "          PaperSystem 服务启动"
echo "============================================="

# 安装依赖
apt update >/dev/null 2>&1
apt install -y libicu-dev libssl-dev redis-server >/dev/null 2>&1

# 启动 UserService
echo "启动 UserService..."
cd "$BASE/UserService"
pkill -f "UserService"
sleep 1
chmod +x UserService
nohup ./UserService > nohup.out 2>&1 &

echo ""
echo "✅ 启动完成"
echo "查看日志: tail -f UserService/nohup.out"
echo ""
2. stop-all.sh（一键停止）
bash
运行
#!/bin/bash
echo "============================================="
echo "          停止 PaperSystem 服务"
echo "============================================="

pkill -f "UserService"
echo "✅ 服务已停止"
echo ""
3. restart-all.sh（一键重启）
bash
运行
#!/bin/bash
echo "============================================="
echo "          重启服务"
echo "============================================="

./stop-all.sh
sleep 2
./start-all.sh
4. update.sh（一键更新重启）
bash
运行
#!/bin/bash
echo "============================================="
echo "          更新并重启服务"
echo "============================================="

./stop-all.sh
sleep 1
./start-all.sh

echo ""
echo "🎉 更新完成，服务已重启"
echo ""
三、Nginx 配置
文件路径：/etc/nginx/sites-available/papersystem.conf
nginx
server {
    listen 80;
    server_name 8.136.202.27;

    # 前端 Web
    location / {
        root /www/wwwroot/PaperServer/Web;
        index index.html;
        try_files $uri $uri/ /index.html;
    }

    # 后端 API 代理
    location /api {
        proxy_pass http://127.0.0.1:5100;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_connect_timeout 60s;
        proxy_send_timeout 60s;
        proxy_read_timeout 60s;
    }
}
启用 Nginx：
bash
运行
ln -s /etc/nginx/sites-available/papersystem.conf /etc/nginx/sites-enabled/
nginx -t
systemctl restart nginx
四、开机自启 systemd 服务
文件路径：/etc/systemd/system/papersystem.service
ini
[Unit]
Description=PaperSystem 微服务
After=network.target redis-server.service

[Service]
Type=forking
User=root
WorkingDirectory=/www/wwwroot/PaperServer
ExecStart=/www/wwwroot/PaperServer/start-all.sh
Restart=always
RestartSec=5

[Install]
WantedBy=multi-user.target
启用开机自启：
bash
运行
systemctl daemon-reload
systemctl enable papersystem
systemctl start papersystem
五、服务器一键赋权（必须执行一次）
bash
运行
cd /www/wwwroot/PaperServer
chmod +x *.sh
六、最终你只需要这 4 条命令
bash
运行
./start-all.sh      # 启动
./stop-all.sh       # 停止
./restart-all.sh    # 重启
./update.sh         # 更新版本后一键重启
七、前端部署
把你前端 dist 目录上传到服务器：
plaintext
/www/wwwroot/PaperServer/Web/
如果你需要，我可以再给你一个安卓 APK 打包说明 + 接口地址配置文档。







如何使用这个部署包？
这个部署包支持哪些版本的.NET？
部署包中包含哪些文件？
