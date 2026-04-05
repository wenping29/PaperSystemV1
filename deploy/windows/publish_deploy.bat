@echo off
setlocal enabledelayedexpansion
chcp 65001 >nul
color 0A
title PaperSystem 微服务 一键打包部署 Linux

echo ======================================================
echo          PaperSystem 微服务 一键批量打包
echo              Linux-x64 独立无依赖版
echo ======================================================
echo.

:: ===================== 基础配置 =====================
set "CONFIG=Release"
set "RUNTIME=linux-x64"
set "SRC_DIR=..\..\src\Services"
set "OUT_DIR=..\..\src\Publish"

:: ===================== 固定服务器配置 =====================
set "SERVER=root@8.136.202.27"
set /p  "LINUX_PWD=请输入密码       :"
set /p  "LINUX_DEPLOY_PATH=/请输入服务器部署路径(如:/app/paper):"
:: ==========================================================

echo.
echo 已固定服务器：%SERVER%
echo 部署路径：%LINUX_DEPLOY_PATH%
echo.

:: 检查服务目录
if not exist "%SRC_DIR%" (
    echo [错误] 服务目录不存在：%SRC_DIR%
    pause
    exit /b 1
)
if not exist "%OUT_DIR%" mkdir "%OUT_DIR%"

:: 检查工具
if not exist "pscp.exe" (
    echo 请把 pscp.exe 放在脚本目录
    pause
    exit /b 1
)
if not exist "plink.exe" (
    echo 请把 plink.exe 放在脚本目录
    pause
    exit /b 1
)

echo [1/2] 开始打包微服务...
echo.

:: 遍历打包
for /d %%s in ("%SRC_DIR%\*") do (
    set "svcName=%%~nxs"
    set "projPath=%%s\!svcName!.csproj"
    set "outPath=%OUT_DIR%\!svcName!"

    if exist "!projPath!" (
        echo ======================================================
        echo 打包服务：!svcName!
        echo ======================================================
        dotnet publish "!projPath!" -c %CONFIG% -r %RUNTIME% --self-contained true /p:PublishSingleFile=true /p:IncludeNativeLibrariesForSelfExtract=true /p:DebugType=None /p:DebugSymbols=false -o "!outPath!"
        if !errorlevel! equ 0 (echo ✅ 打包成功：!svcName!) else (echo ❌ 打包失败：!svcName!)
        echo.
    )
)

echo.
echo [2/2] 开始上传部署到 Linux 服务器...
echo.

:: 创建目录
plink -batch -pw %LINUX_PWD% %SERVER% "mkdir -p %LINUX_DEPLOY_PATH%" 2>nul
echo 完成创建目录：%LINUX_DEPLOY_PATH%
:: 上传并授权
for /d %%s in ("%OUT_DIR%\*") do (
    echo ======================================================
    echo 上传服务：%%~nxs
    set "svcName=%%~nxs"
    set "localPath=%%s\"
    set "remotePath=%LINUX_DEPLOY_PATH%/!svcName!"

    echo 上传：!svcName!
    echo 开始pscp

    pscp -batch -pw %LINUX_PWD% -r "!localPath!" %SERVER%:"!remotePath!"
    echo 开始授权:plink：!svcName!
    plink -batch -pw %LINUX_PWD% %SERVER% "chmod +x %LINUX_DEPLOY_PATH%/!svcName!/!svcName!" 2>nul
    echo ✅ 部署完成：!svcName!
    echo.
)

echo ======================================================
echo 🎉 全部完成！
echo 服务器：%SERVER%
echo 路径：%LINUX_DEPLOY_PATH%
echo ======================================================
pause