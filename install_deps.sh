#!/bin/bash

function read_solution() {
    echo "Parsing solution $1"

    while IFS='' read -r line || [[ -n "$line" ]]; do
            if [[ $line =~ \"([^\"]*.csproj)\" ]]; then
                    project="${BASH_REMATCH[1]}"

                    read_project "$(echo "$project"|tr '\\' '/')"
            fi
    done < "$1"
}

function read_project() {
    echo "Parsing project $1"
    package_regex='PackageReference Include="([^"]*)" Version="([^"]*)"'

    while IFS='' read -r line || [[ -n "$line" ]]; do
            if [[ $line =~ $package_regex ]]; then
                    name="${BASH_REMATCH[1]}"
                    version="${BASH_REMATCH[2]}"

                    if [[ $version != *-* ]]; then
                            dotnet add "$1" package "$name" --version "$version"
                    fi
            fi
    done < $1
}


for service in "common" "core-service" "file-service"; do
        cd "$service"
        for project in "$service".csproj; do
                if [ ! -f ${project} ]; then
                        continue
                fi

                read_project "${project}"
        done
        cd ..
done
