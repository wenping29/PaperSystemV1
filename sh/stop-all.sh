#!/bin/bash
echo "============================================="
echo "          停止 PaperSystem 服务"
echo "============================================="

pkill -f "UserService"
echo "✅ 服务已停止"
echo ""