FROM ubuntu:20.04

# 设置环境变量
ENV DEBIAN_FRONTEND=noninteractive
ENV UNITY_LICENSE_FILE=/root/.local/share/unity3d/Unity/Unity_vf.ulf

# 安装必要依赖
RUN apt-get update && apt-get install -y \
    curl \
    wget \
    unzip \
    xvfb \
    libunwind8 \
    lib32z1 \
    lib32ncurses5 \
    lib32bz2-1.0 \
    lib32stdc++6 \
    libasound2 \
    libc6-i386 \
    libgtk-3-0 \
    libnss3 \
    libxtst6 \
    libxss1 \
    libgconf-2-4 \
    libasound2t64 \
    libpulse0 \
    pulseaudio \
    software-properties-common \
    python3 \
    python3-pip \
    && rm -rf /var/lib/apt/lists/*

# 创建用户（Unity需要非root用户运行）
RUN groupadd -r unity -g 1000 && useradd -u 1000 -r -g unity -m -d /home/unity -s /sbin/nologin unity

# 安装Unity Editor (需要下载Unity Hub或Unity编辑器)
WORKDIR /tmp
RUN wget https://public-cdn.cloud.unity3d.com/hub/prod/UnityHub.AppImage -O unity-hub.AppImage && \
    chmod +x unity-hub.AppImage

# 提示用户需要安装Unity许可证
RUN mkdir -p /root/.local/share/unity3d/Unity

# 创建项目目录
WORKDIR /project

# 复制项目文件
COPY . /project

# 更改权限
RUN chown -R unity:unity /project

# 创建构建脚本
RUN echo '#!/bin/bash\n\
echo "========================================="\n\
echo "KukuWorld Unity Build Environment"\n\
echo "========================================="\n\
echo "Warning: This container does not include Unity Editor by default due to licensing."\n\
echo "You need to provide Unity license and binaries."\n\
echo ""\n\
echo "To build Unity project, you can:"\n\
echo "1. Mount Unity installation directory"\n\
echo "2. Provide Unity license file"\n\
echo "3. Use Unity official CI/CD images from Docker Hub"\n\
echo ""\n\
echo "Example usage with official Unity image:"\n\
echo "docker run -it --rm -v \$(pwd):/project unityci/editor:2022.3.22f1-android-il2cpp-1 /build_script.sh"\n\
echo "========================================="' > /entrypoint.sh && chmod +x /entrypoint.sh

ENTRYPOINT ["/entrypoint.sh"]