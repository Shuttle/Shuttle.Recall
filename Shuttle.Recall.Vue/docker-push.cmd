@echo off
setlocal

:: Prompt for version input
set /p version="Enter the version tag (e.g., 1.0.0): "

:: Check if version is empty
if "%version%"=="" (
    echo ERROR: No version tag provided.
    exit /b 1
)

echo Pushing Docker image shuttle/recall-vue:%version%...
docker push shuttle/recall-vue:%version%

:: Check if the docker push command was successful
if %ERRORLEVEL% neq 0 (
    echo ERROR: Failed to push Docker image shuttle/recall-vue:%version%.
    exit /b 1
)

echo Pushing Docker image shuttle/recall-vue:latest...
docker tag shuttle/recall-vue:%version% shuttle/recall-vue:latest
docker push shuttle/recall-vue:latest

:: Check if the docker push command was successful
if %ERRORLEVEL% neq 0 (
    echo ERROR: Failed to push Docker image shuttle/recall-vue:latest.
    exit /b 1
)

echo All Docker images pushed successfully!
