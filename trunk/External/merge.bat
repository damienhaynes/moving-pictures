@echo off
IF EXIST MovingPictures_UNMERGED.dll del MovingPictures_UNMERGED.dll
ren MovingPictures.dll MovingPictures_UNMERGED.dll
ilmerge /out:MovingPictures.dll MovingPictures_UNMERGED.dll Cornerstone.dll Cornerstone.MP.dll NLog.dll
