@echo off

:: create temporary folder for building the zip file
if EXIST tmp rd tmp /s /q
md tmp
md tmp\MediaPortal
md tmp\MediaPortal\plugins
md tmp\MediaPortal\plugins\Windows


:: copy all needed files
xcopy ..\MovingPictures\Resources\skins\DefaultWide tmp\skin\DefaultWide\ /E
xcopy ..\MovingPictures\Resources\skins\Default tmp\skin\Default\ /E
copy ..\MovingPictures\Resources\moving-pictures-release-notes.txt tmp
copy ..\MovingPictures\bin\Release\movingpictures.dll tmp\MediaPortal\plugins\Windows
copy ..\MovingPictures\bin\Release\Cornerstone.dll tmp\MediaPortal\plugins\Windows
copy ..\MovingPictures\bin\Release\Cornerstone.MP.dll tmp\MediaPortal\plugins\Windows
copy ..\MovingPictures\bin\Release\NLog.dll tmp\MediaPortal
xcopy ..\MovingPictures\Resources\language tmp\language\MovingPictures\ /E

cd tmp
"c:\Program Files\7-Zip\7z.exe" a -r -tzip moving-pictures.zip *.* 
move *.zip ..
cd..
rd tmp /s /q

pause