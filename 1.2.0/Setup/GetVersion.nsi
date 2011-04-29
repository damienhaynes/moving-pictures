!define File "..\MovingPictures\bin\Release\MovingPictures.dll"
 
OutFile "GetVersion.exe"
SilentInstall silent
 
Section
 
 ## Get file version
 GetDllVersion "${File}" $R0 $R1
  IntOp $R2 $R0 / 0x00010000
  IntOp $R3 $R0 & 0x0000FFFF
  IntOp $R4 $R1 / 0x00010000
  IntOp $R5 $R1 & 0x0000FFFF
  StrCpy $R6 "$R2.$R3.$R4"
  StrCpy $R1 "$R2.$R3.$R4.$R5"
 
 ## Write it to a !define for use in main script
 FileOpen $R0 "$EXEDIR\Version.txt" w
  FileWrite $R0 '!define VersionMajor "$R2"$\n'
  FileWrite $R0 '!define VersionMinor "$R3"$\n'
  FileWrite $R0 '!define VersionPoint "$R4"$\n'
  FileWrite $R0 '!define VersionRevision "$R5"$\n'
  FileWrite $R0 '!define VersionShort "$R6"$\n'
  FileWrite $R0 '!define VersionLong "$R1"$\n'
  FileWrite $R0 '!define Version "$R1"'
 FileClose $R0
 
SectionEnd