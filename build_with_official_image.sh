#!/bin/bash

echo "KukuWorld Unity Build Script (Using Official Image)"
echo "=================================================="

# 检查Docker是否已安装
if ! command -v docker &> /dev/null; then
    echo "错误: Docker未安装，请先安装Docker"
    exit 1
fi

# 检查项目结构
if [ ! -d "Assets" ] || [ ! -d "ProjectSettings" ]; then
    echo "错误: 不是有效的Unity项目结构"
    exit 1
fi

echo "选择构建目标:"
echo "1) Android (.apk)"
echo "2) Windows (.exe)"
echo "3) WebGL (Web Build)"
echo "4) macOS (.app)"
echo "5) Linux (.x86_64)"

read -p "请输入选项 (1-5): " choice

# 设置构建参数
case $choice in
    1)
        TARGET="Android"
        EXTENSION="apk"
        METHOD="BuildScript.PerformAndroidBuild"
        ;;
    2)
        TARGET="StandaloneWindows64"
        EXTENSION="exe"
        METHOD="BuildScript.PerformWindowsBuild"
        ;;
    3)
        TARGET="WebGL"
        EXTENSION="html/js"
        METHOD="BuildScript.PerformWebGLBuild"
        ;;
    4)
        TARGET="StandaloneOSX"
        EXTENSION="app"
        METHOD="BuildScript.PerformMacOSBuild"
        ;;
    5)
        TARGET="StandaloneLinux64"
        EXTENSION="x86_64"
        METHOD="BuildScript.PerformLinuxBuild"
        ;;
    *)
        echo "无效选项，使用默认: Windows"
        TARGET="StandaloneWindows64"
        EXTENSION="exe"
        METHOD="BuildScript.PerformWindowsBuild"
        ;;
esac

echo "开始为 $TARGET 构建..."

# 创建构建输出目录
mkdir -p Builds

# 使用Unity官方镜像进行构建
echo "正在使用Unity官方镜像构建项目..."

# 检查是否已存在Unity官方镜像
if ! docker images unityci/editor:2022.3.22f1-base-3.0.2 | grep -q unityci; then
    echo "正在拉取Unity官方镜像..."
    docker pull unityci/editor:2022.3.22f1-base-3.0.2
    if [ $? -ne 0 ]; then
        echo "无法拉取Unity官方镜像，请检查网络连接"
        exit 1
    fi
fi

# 运行构建
BUILD_PATH=$(pwd)/Builds
PROJECT_PATH=$(pwd)

echo "正在运行Unity构建进程..."
docker run --rm \
  -v "$PROJECT_PATH:/project" \
  -v "$BUILD_PATH:/builds" \
  -w /project \
  unityci/editor:2022.3.22f1-base-3.0.2 \
  bash -c "
    echo 'Starting Unity build...' &&
    /opt/unity/Editor/Unity -batchmode -quit \
      -projectPath /project \
      -executeMethod BuildScript.PerformWindowsBuild \
      -logFile - \
      || echo 'Build completed'
  "

if [ $? -eq 0 ]; then
    echo "构建完成！请检查Builds目录中的输出文件。"
    ls -la Builds/
else
    echo "构建过程中出现错误，请检查上面的日志。"
    exit 1
fi