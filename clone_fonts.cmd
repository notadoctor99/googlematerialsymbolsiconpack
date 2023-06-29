@echo off

set dirname=git

if exist %dirname% (
    echo Directory already exists
    goto :eof
)

git clone https://github.com/google/material-design-icons --no-checkout --single-branch --depth 1 %dirname%

cd %dirname%

git sparse-checkout set --cone variablefont

git checkout master
