#!/bin/bash
DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
/usr/bin/mono $DIR/PasswordPrompt/PasswordPrompt.exe "CryFS Password" $DIR/PasswordPromt/script.sh
