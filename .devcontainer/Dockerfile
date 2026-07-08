FROM mcr.microsoft.com/devcontainers/dotnet:2-10.0-noble

ARG ANDROID_SDK_ROOT=/opt/android-sdk
ARG ANDROID_COMMANDLINE_TOOLS_VERSION=14742923

ENV ANDROID_HOME=${ANDROID_SDK_ROOT}
ENV ANDROID_SDK_ROOT=${ANDROID_SDK_ROOT}
ENV ANDROID_AVD_HOME=/home/vscode/.android/avd
ENV PATH="${PATH}:${ANDROID_SDK_ROOT}/cmdline-tools/latest/bin:${ANDROID_SDK_ROOT}/platform-tools:${ANDROID_SDK_ROOT}/emulator"

USER root

RUN apt-get update \
    && apt-get install -y --no-install-recommends \
        bash \
        ca-certificates \
        curl \
        git \
        jq \
        unzip \
        wget \
        zip \
        python3 \
        python3-pip \
        libc6 \
        libstdc++6 \
        zlib1g \
        libncurses6 \
        libtinfo6 \
        libicu-dev \
        libssl-dev \
        libx11-6 \
        libpulse0 \
        libglu1-mesa \
        libgl1 \
        libxi6 \
        libxrender1 \
        libxtst6 \
        libnss3 \
        libxcomposite1 \
        libxcursor1 \
        libxdamage1 \
        libxrandr2 \
        libatk-bridge2.0-0 \
        libgtk-3-0 \
    && rm -rf /var/lib/apt/lists/*

RUN mkdir -p ${ANDROID_SDK_ROOT}/cmdline-tools \
    && curl -fsSL "https://dl.google.com/android/repository/commandlinetools-linux-${ANDROID_COMMANDLINE_TOOLS_VERSION}_latest.zip" -o /tmp/android-commandline-tools.zip \
    && unzip -q /tmp/android-commandline-tools.zip -d /tmp/android-sdk \
    && mv /tmp/android-sdk/cmdline-tools ${ANDROID_SDK_ROOT}/cmdline-tools/latest \
    && rm -rf /tmp/android-commandline-tools.zip /tmp/android-sdk \
    && chown -R vscode:vscode ${ANDROID_SDK_ROOT}

USER vscode
