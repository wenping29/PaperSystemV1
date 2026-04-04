@echo off
chcp 65001
echo ======================================================
echo           批量上传所有微服务到 Ubuntu 服务器
echo ======================================================

set SERVER=root@8.136.202.27
set DEST=/www/wwwroot/PaperServer
set LOCAL_PUBLISH=src\Publish

echo 正在上传所有服务...
echo.

for /d %%s in ("%LOCAL_PUBLISH%\*") do (
    echo 上传：%%~nxs
    scp -r "%%s" %SERVER%:%DEST%/
    echo.
)

echo ======================================================
echo ✅ 所有服务上传完成！
echo ======================================================
pause