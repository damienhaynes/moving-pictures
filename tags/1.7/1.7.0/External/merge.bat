@echo off

md tmp
rem ilmerge /out:tmp\MovingPictures.dll MovingPictures.dll CookComputing.XmlRpcV2.dll FollwitApi.dll
ilmerge /out:tmp\MovingPictures.dll MovingPictures.dll CookComputing.XmlRpcV2.dll FollwitApi.dll /target:dll /targetplatform:v4,C:\Windows\Microsoft.NET\Framework\v4.0.30319 /wildcards
IF EXIST MovingPictures_UNMERGED.dll del MovingPictures_UNMERGED.dll
ren MovingPictures.dll MovingPictures_UNMERGED.dll
IF EXIST MovingPictures_UNMERGED.pdb del MovingPictures_UNMERGED.pdb
ren MovingPictures.pdb MovingPictures_UNMERGED.pdb

rem ilmerge /out:tmp\Cornerstone.dll Cornerstone.dll Lucene.Net.dll
ilmerge /out:tmp\Cornerstone.dll Cornerstone.dll Lucene.Net.dll /target:dll /targetplatform:v4,C:\Windows\Microsoft.NET\Framework\v4.0.30319 /wildcards
IF EXIST Cornerstone_UNMERGED.dll del Cornerstone_UNMERGED.dll
ren Cornerstone.dll Cornerstone_UNMERGED.dll
IF EXIST Cornerstone_UNMERGED.pdb del Cornerstone_UNMERGED.pdb
ren Cornerstone.pdb Cornerstone_UNMERGED.pdb


move tmp\*.* .
rd tmp

