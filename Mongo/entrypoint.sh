#!/bin/bash

init(){
    sleep 25

    echo -e "\n\n\nCreating collections...\n\n\n"
    mongo localhost:30001 <<EOF
    use test

    db.createCollection("people")
    db.createCollection("pets")
EOF
}

mongod --replSet my-replica-set --bind_ip_all --port 30001 & init & wait