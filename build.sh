#!/bin/bash
# Build script for Challenge Mode

echo -e "\033[36mBuilding Challenge Mode...\033[0m"

# Build the project (skip PostBuild to avoid launching game)
cd schedule-one-challange-mode
dotnet build -c MONO -p:SkipGameLaunch=true

if [ $? -ne 0 ]; then
    echo -e "\033[31mBuild failed!\033[0m"
    cd ..
    exit 1
fi

cd ..

# Copy DLL to dist
SOURCE_DLL="schedule-one-challange-mode/bin/MONO/netstandard2.1/challange_mode.dll"
DIST_DIR="dist"

mkdir -p "$DIST_DIR"
cp "$SOURCE_DLL" "$DIST_DIR/"

echo -e "\n\033[32mBuild successful!\033[0m"
echo -e "\033[32mDLL copied to: dist/challange_mode.dll\033[0m"

