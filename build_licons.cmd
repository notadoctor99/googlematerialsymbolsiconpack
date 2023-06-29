@echo off

set fileName=GoogleMaterialSymbols.licons

if exist %fileName% del %fileName%

cd ico

..\zip\7za.exe a -r -tzip %fileName% *.*

move %fileName% ..

cd ..
