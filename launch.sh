DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"
mono $DIR/PasswordPrompt.exe "CryFS Password" $DIR/script.sh
