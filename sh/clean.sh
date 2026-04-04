#!/bin/bash
BASE="/www/wwwroot/PaperServer"

echo "============================================="
echo "         自动清理日志与缓存
echo "============================================="

# 清空所有服务日志
for log in "$BASE"/*/nohup.out; do
    > $log
    echo "✅ 清空日志：$log"
done

# 删除7天前的系统日志
find /var/log -name "*.log" -mtime +7 -delete >/dev/null 2>&1

echo ""
echo "🎉 清理完成！"
echo ""