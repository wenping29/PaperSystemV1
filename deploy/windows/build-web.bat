@echo off
setlocal enabledelayedexpansion
chcp 65001 >nul
color 0A
title 前端项目一键打包

echo ======================================================
echo               前端项目 一键打包构建
echo                 npm run build
echo ======================================================
echo.

:: 跳转到前端项目目录（自动兼容路径）
cd /d "%~dp0..\..\apps\admin"

echo 正在进入项目目录：
echo %cd%
echo.

:: 检查目录是否存在
if not exist "%cd%" (
    echo [错误] 项目目录不存在！
    pause
    exit /b 1
)

:: 检查 package.json
if not exist "package.json" (
    echo [错误] 当前目录不是前端项目！
    pause
    exit /b 1
)

echo ==================== 开始构建 =================