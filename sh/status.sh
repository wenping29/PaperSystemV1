#!/bin/bash
BASE="/www/wwwroot/PaperServer"

echo "============================================="
echo "         服务状态检查
echo "============================================="

for service_dir in "$BASE"/*/; do
    service_name=$(basename "$service_dir")
    pid=$(pgrep -f "$service_name")

    if [ -n "$pid" ]; then
        echo "✅ $service_name 运行中 PID：$pid"
    else
        echo "❌ $service 未运行"
    fi
done

echo ""