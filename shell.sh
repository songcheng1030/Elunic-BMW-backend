#!/bin/bash

docker-compose -f docker-compose.shell.yml build && docker-compose -f docker-compose.shell.yml run --rm -u 1000 --service-ports --name aiqx-services app bash || true && echo Stopping environment... && docker-compose -f docker-compose.shell.yml down