#!/bin/bash
control_c() {
    docker-compose down
    exit
}

trap control_c SIGINT

while true ; do 
   docker-compose up --rmi all | while read line ; do
   PID=$!
   echo $line 
done
