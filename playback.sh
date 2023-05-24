#!/bin/sh

# check number of arguments
if [ $# -ne 1 ]; then
    echo Enter path to the record file
    exit 1
fi

record_path=$1

cd back
dotnet run $record_path