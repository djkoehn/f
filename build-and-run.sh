#!/bin/bash

# Run dotnet build and capture the exit code
echo "Building C# project..."
dotnet build
BUILD_STATUS=$?

# Check if the build was successful
if [ $BUILD_STATUS -eq 0 ]; then
  echo "Build succeeded. Launching Godot..."
  /Applications/Godot_mono.app/Contents/MacOS/Godot
else
  echo "Build failed. Exiting."
  exit $BUILD_STATUS
fi