#!/bin/bash
docker-compose up --rmi all
control_c() {
    docker-compose down
    exit
}

trap control_c SIGINT

while true ; do
done
