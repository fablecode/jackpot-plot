#!/bin/bash

# Default to "development" configuration if none is provided
CONFIG=${1:-development}

echo "Starting build process with configuration: $CONFIG"

# Process SCSS to CSS
echo "Processing SCSS to temporary CSS..."

npx sass ./src/styles.scss ./src/assets/css/styles.css

if [ $? -ne 0 ]; then
  echo "SASS compilation failed!"
  exit 1
fi

# Process Tailwind CSS
echo "Processing Tailwind CSS..."

npx tailwindcss -i ./src/metronic/css/styles.css -o ./src/assets/css/tailwindcss-styles.css

if [ $? -ne 0 ]; then
  echo "Tailwind CSS processing failed!"
  exit 1
fi

# Processing Javascript
echo "Processing Javascript..."

webpack --mode="$CONFIG" --stats-error-details --no-watch

if [ $? -ne 0 ]; then
  echo "Javascript processing failed!"
  exit 1
fi

# Build Angular app with the specified configuration
echo "Building Angular app with configuration: $CONFIG"
ng build --configuration=$CONFIG
if [ $? -ne 0 ]; then
  echo "Angular build failed!"
  exit 1
fi

echo "Build process completed successfully!"
