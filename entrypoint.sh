#!/bin/bash
rabbitmq-server -detached
sleep 5
rabbitmqctl import_definitions /etc/rabbitmq/definitions.json
exec rabbitmq-server