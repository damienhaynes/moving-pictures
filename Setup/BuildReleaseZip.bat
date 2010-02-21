@echo off

:: create temporary folder for building the zip file
if EXIST tmp rd tmp /s /q
md tmp

:: copy all needed files
xcopy ..\MovingPictures\MainUI\Blue3wide tmp\Blue3wide\ /E
xcopy ..\MovingPictures\MainUI\Blue3 tmp\Blue3\ /E
copy ..\MovingPictures\Resources\moving-pictures-release-notes.txt tmp
copy ..\MovingPictures\bin\Release\movingpictures.dll tmp
copy ..\MovingPictures\bin\Release\Cornerstone.dll tmp
copy ..\MovingPictures\bin\Release\Cornerstone.MP.dll tmp
xcopy ..\MovingPictures\Resources\language tmp\language\MovingPictures\ /E

:: xcopy "..\MovingPictures\MainUI\Generic Skin" "tmp\Generic Skin\" /E
:: xcopy "tmp\Generic Skin\16x9\Media" "tmp\Generic Skin\4x3\Media\" /E
:: rd "tmp\Generic Skin\PSD" /s /q

cd tmp
"c:\Program Files\7-Zip\7z.exe" a -r -tzip moving-pictures.zip *.* 
move *.zip ..
cd..
rd tmp /s /q