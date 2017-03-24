#!/bin/bash
echo $1 | CRYFS_FRONTEND=noninteractive cryfs $HOME/Dropbox/CryFS $HOME/CryFS &> /dev/null
