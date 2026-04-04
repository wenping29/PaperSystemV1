#!/bin/bash

# 基础路径
BASE_PATH="/www/wwwroot/PaperServer"

# 定义要启动的微服务列表
# 格式：服务名=端口
declare -A SERVICES=(
  ["UserService"]=5100
  ["WritingService"]=5101
  ["AIService"]=5109
  ["CommunityService"]=5102
  ["ChatService"]=5103
  ["FileService"]=5104
  ["FriendshipService"]=5105
  ["NotificationService"]=5106
  ["PaymentService"]=5107
  ["SearchService"]=5108

)

echo "==================================="
echo "  批量启动 .NET 微服务（无依赖版）"
echo "==================================="

# 1. 安装必备依赖（防止 Aborted）
echo "→ 安装系统依赖库..."
apt update > /dev/null 2>&1
apt install -y libicu-dev libssl-dev > /dev/null 2>&1

# 2. 遍历启动所有服务
for service in "${!SERVICES[@]}"; do
  port=${SERVICES[$service]}
  path="$BASE_PATH/publish/$service"
  
  echo ""
  echo "==================================="
  echo "启动服务：$service  端口：$port"
  echo "路径：$path"
  echo "==================================="

  # 进入目录
  cd "$path" || { echo "→ 目录不存在，跳过"; continue; }

  # 杀死旧进程
  echo "→ 杀死旧的 $service 进程..."
  pkill -f "./$service" > /dev/null 2>&1
  sleep 1

  # 赋权
  echo "→ 设置执行权限..."
  chmod 777 "./$service"
  chmod +x "./$service"

  # 后台启动
  echo "→ 后台启动 $service ..."
  nohup "./$service" > "$service.log" 2>&1 &

  echo "✅ $service 启动完成！"
done

echo ""
echo "==================================="
echo "🎉 所有微服务已批量启动完成！"
echo "==================================="
echo "查看日志：cat 服务名.log"
echo "停止所有：pkill -f Service"
echo ""