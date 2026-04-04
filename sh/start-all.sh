#!/bin/bash
BASE="/www/wwwroot/PaperServer"

echo "============================================="
echo "      一键启动所有 PaperSystem 微服务
echo "============================================="

# 安装依赖
apt update >/dev/null 2>&1
apt install -y libicu-dev libssl-dev redis-server >/dev/null 2>&1

# 自动启动所有服务
for service_dir in "$BASE"/*/; do
    service_name=$(basename "$service_dir")
    exe_path="$service_dir$service_name"

    if [ -f "$exe_path" ]; then
        echo "启动 $service_name ..."

        cd "$service_dir"
        pkill -f "$service_name"
        sleep 1
        chmod +x "$service_name"
        nohup ./"$service_name" > nohup.out 2>&1 &

        echo "✅ $service_name 启动成功"
        echo
    fi
done

echo "🎉 所有服务启动完成！"