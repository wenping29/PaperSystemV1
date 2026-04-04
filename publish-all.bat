@echo off
setlocal enabledelayedexpansion
chcp 65001 >nul
color 0A
title PaperSystem 微服务 一键批量打包

echo ======================================================
echo          PaperSystem 微服务 一键批量打包
echo              Linux-x64 独立无依赖版
echo ======================================================
echo.

:: ===================== 基础配置 =====================
set "CONFIG=Release"
set "RUNTIME=linux-x64"
set "SRC_DIR=.\src\Services"
set "OUT_DIR=.\src\Publish"
:: ====================================================

:: 检查服务目录
if not exist "%SRC_DIR%" (
    echo [错误] 服务目录不存在：%SRC_DIR%
    pause
    exit /b 1
)

:: 自动创建输出根目录
if not exist "%OUT_DIR%" mkdir "%OUT_DIR%"

echo [开始] 正在批量打包所有微服务...
echo.

:: 遍历服务（已修复路径拼接错误）
for /d %%s in ("%SRC_DIR%\*") do (
    set "svcName=%%~nxs"
    set "projPath=%%s\!svcName!.csproj"
    set "outPath=%OUT_DIR%\!svcName!"

    if exist "!projPath!" (
        echo ======================================================
        echo 服务名：!svcName!
        echo 项目：!projPath!
        echo 输出：!outPath!
        echo ======================================================

        dotnet publish "!projPath!" ^
            -c %CONFIG% ^
            -r %RUNTIME% ^
            --self-contained true ^
            /p:PublishSingleFile=true ^
            /p:IncludeNativeLibrariesForSelfExtract=true ^
            /p:DebugType=None ^
            /p:DebugSymbols=false ^
            -o "!outPath!"

        echo.
        if !errorlevel! equ 0 (
            echo ✅ 打包成功：!svcName!
        ) else (
            echo ❌ 打包失败：!svcName!
        )
        echo.
    )
)

echo ======================================================
echo 🎉 所有服务打包完成
echo 输出根目录：%OUT_DIR%
echo ======================================================
echo.
pause