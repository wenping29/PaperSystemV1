@echo off
chcp 65001
echo ======================================================
echo           Android APK 一键打包脚本 (Release)
echo ======================================================

:: 安卓项目路径
set ANDROID_PROJECT=..\..\apps\Mobile

:: 输出APK目录
set OUTPUT_DIR=..\..\apps\apk_output

:: 检查项目是否存在
if not exist "%ANDROID_PROJECT%" (
    echo 错误：安卓项目不存在！路径：%ANDROID_PROJECT%
    pause
    exit /b 1
)

echo 进入安卓项目目录...
cd "%ANDROID_PROJECT%"

echo.
echo ======================================================
echo 1. 清理项目
echo ======================================================
call gradlew clean

echo.
echo ======================================================
echo 2. 打包 Release APK
echo ======================================================
call gradlew assembleRelease

echo.
echo ======================================================
echo 3. 复制 APK 到输出目录
echo ======================================================
if not exist "%OUTPUT_DIR%" mkdir "%OUTPUT_DIR%"

copy /y app\build\outputs\apk\release\*.apk "%OUTPUT_DIR%\"

echo.
echo ======================================================
echo 🎉 APK 打包完成！
echo 输出目录：%OUTPUT_DIR%
echo ======================================================

cd ..\..\deploy\windows
pause