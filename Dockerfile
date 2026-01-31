# Use Unity official CI/CD image for building
# This is a template - use with official Unity CI/CD images
FROM unityci/editor:2022.3.22f1-base-3.0.2

WORKDIR /project

# Copy project files
COPY . /project

# Ensure the build script is executable
RUN chmod +x /project/tools/build_with_official_image.sh

# By default, show usage information
CMD echo "KukuWorld Unity Project - Build Container"; \
    echo "Use official Unity CI/CD images directly:"; \
    echo "docker run -it --rm -v \$(pwd):/project unityci/editor:2022.3.22f1-base-3.0.2 /opt/unity/Editor/Unity -batchmode -quit -projectPath /project -executeMethod BuildScript.PerformWindowsBuild"; \
    echo "See tools/build_with_official_image.sh for automated build options"