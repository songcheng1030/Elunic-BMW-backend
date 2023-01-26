#!/bin/bash


for service in "core-service" "file-service"; do
        echo "Running $service in watch mode"
        cd "$service" && dotnet watch run --no-hot-reload &
done