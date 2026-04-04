@echo off
chcp 65001
cd apps/admin
npm install
npm run build
echo.
echo ✅ 前端打包完成：dist/
pause