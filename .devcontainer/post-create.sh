#!/usr/bin/env bash
set -euo pipefail

echo "==> .NET info"
dotnet --info

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

echo "==> Installing .NET workloads"

dotnet workload install maui-android wasm-tools

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

echo "==> Installed .NET workloads"
dotnet workload list

echo "==> Android SDK packages"
sdkmanager --list_installed || true

echo "==> Dev container setup complete"
