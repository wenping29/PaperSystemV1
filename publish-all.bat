@echo off
chcp 65001
echo ======================================================
echo          PaperSystem 微服务 一键批量打包
echo              Linux-x64 独立无依赖版
echo ======================================================

:: 基础配置（不用改）
set "CONFIG=Release"
set "RUNTIME=linux-x64"
set "SRC_DIR=..\..\src\Services"
set "OUT_DIR=..\..\src\Publish"

:: 检查服务目录是否存在
if not exist "%SRC_DIR%" (
    echo 错误：服务目录不存在 %SRC_DIR%
    pause
    exit /b 1
)

echo 正在打包所有微服务...
echo.

:: 遍历所有子目录（每个子目录=一个微服务）
for /d %%s in ("%SRC_DIR%\*") do (
    set "SERVICE_NAME=%%~nxs"
    set "CSPROJ=%%s\%%~nxs.csproj"
    set "OUT_PATH=%OUT_DIR%\%%~nxs"

    if exist "%%s\%%~nxs.csproj" (
        echo ======================================================
        echo 打包服务：%%~nxs
        echo 项目文件：%%~nxs.csproj
        echo 输出目录：!OUT_PATH!
        echo ======================================================

        dotnet publish "%%s\%%~nxs.csproj" ^
            -c %CONFIG% ^
            -r %RUNTIME% ^
            --self-contained true ^
            /p:PublishSingleFile=true ^
            /p:IncludeNativeLibrariesForSelfExtract=true ^
            /p:DebugType=None ^
            -o "!OUT_PATH!"

        echo.
        echo ✅ 打包完成：%%~nxs
        echo.
    )
)

echo ======================================================
echo 🎉 所有微服务打包完成！
echo 输出路径：%OUT_DIR%
echo ======================================================
pause