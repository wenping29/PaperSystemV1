#!/bin/bash
BASE="/www/wwwroot/PaperServer"

echo "============================================="
echo "         实时查看所有服务日志
echo "============================================="

tail -f "$BASE"/*/nohup.out