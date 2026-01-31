#!/bin/bash

echo "KukuWorld Unity Build Script"
echo "=============================="

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
echo "4) 所有平台"

read -p "请输入选项 (1-4): " choice

case $choice in
    1)
        TARGET="android"
        ;;
    2)
        TARGET="windows"
        ;;
    3)
        TARGET="webgl"
        ;;
    4)
        TARGET="all"
        ;;
    *)
        echo "无效选项，使用默认: windows"
        TARGET="windows"
        ;;
esac

echo "开始构建..."

# 创建构建输出目录
mkdir -p Builds

# 构建Docker镜像
echo "正在构建Docker镜像..."
docker build -t kukuworld-builder .

if [ $? -ne 0 ]; then
    echo "Docker镜像构建失败"
    exit 1
fi

# 运行构建容器
echo "正在运行Unity构建..."
docker run --rm \
  -v "$(pwd)":/project \
  -v "$(pwd)/Builds":/project/Builds \
  kukuworld-builder

echo "构建完成。请检查Builds目录中的输出文件。"