#!/usr/bin/env bash
set -euo pipefail

EXPECTED_DOTNET_SDK_VERSION="11.0.100-preview.5.26302.115"

echo "==> .NET info"
dotnet --info

INSTALLED_DOTNET_SDK_VERSION="$(dotnet --version)"

if [ "${INSTALLED_DOTNET_SDK_VERSION}" != "${EXPECTED_DOTNET_SDK_VERSION}" ]; then
  echo "ERROR: Expected .NET SDK ${EXPECTED_DOTNET_SDK_VERSION}, but found ${INSTALLED_DOTNET_SDK_VERSION}."
  exit 1
fi

echo "==> Installing Android SDK packages"

yes | sdkmanager --licenses >/dev/null || true

sdkmanager --install \
  "cmdline-tools;latest" \
  "platform-tools" \
  "emulator" \
  "platforms;android-36" \
  "build-tools;36.0.0" \
  "platforms;android-35" \
  "build-tools;35.0.0"

echo "==> Installing .NET 11 preview workloads"

dotnet workload install \
  maui-android \
  wasm-tools

echo "==> Restoring .NET tools"

if [ -f ".config/dotnet-tools.json" ]; then
  dotnet tool restore
else
  echo "No .config/dotnet-tools.json found; skipping dotnet tool restore."
fi

echo "==> Restoring solution/project"

SOLUTION_FILE="$(find . -maxdepth 3 -name '*.sln' | head -n 1 || true)"

if [ -n "${SOLUTION_FILE}" ]; then
  echo "Restoring ${SOLUTION_FILE}"
  dotnet restore "${SOLUTION_FILE}"
else
  echo "No .sln file found within max depth 3; restoring current directory."
  dotnet restore
fi

echo "==> Installed .NET SDK"
dotnet --version

echo "==> Installed .NET workloads"
dotnet workload list

echo "==> Android SDK packages"
sdkmanager --list_installed || true

echo "==> Dev container setup complete"
