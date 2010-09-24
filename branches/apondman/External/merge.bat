@echo off

md tmp
ilmerge /out:tmp\MovingPictures.dll MovingPictures.dll CookComputing.XmlRpcV2.dll MovingPicturesSocialAPI.dll
IF EXIST MovingPictures_UNMERGED.dll del MovingPictures_UNMERGED.dll
ren MovingPictures.dll MovingPictures_UNMERGED.dll
IF EXIST MovingPictures_UNMERGED.pdb del MovingPictures_UNMERGED.pdb
ren MovingPictures.pdb MovingPictures_UNMERGED.pdb

move tmp\*.* .
rd tmp

