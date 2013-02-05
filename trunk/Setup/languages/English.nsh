!define LANG "ENGLISH" ; Must be the lang name define my NSIS

!insertmacro LANG_STRING NO_MEDIAPORTAL "Moving Pictures is a plug-in for the MediaPortal HTPC Application.$\nYou must install MediaPortal before installing Moving Pictures. Would$\nyou like to goto the MediaPortal homepage?"
!insertmacro LANG_STRING MEDIAPORTAL_RUNNING "Please shutdown MediaPortal to continue the installation."
!insertmacro LANG_STRING INVALID_SKIN1 "Could not determine the aspect ratio of the "
!insertmacro LANG_STRING INVALID_SKIN2 " skin. The$\nGeneric Skin will not be installed for "
!insertmacro LANG_STRING BAD_SKIN_PATH "Could not locate Skins directory. Skin files will not be installed!" 
!insertmacro LANG_STRING BAD_DB_PATH "Could not locate Database directory. Database will not be backed up."
!insertmacro LANG_STRING OLD_VER1 "Moving Pictures requires MediaPortal "
!insertmacro LANG_STRING OLD_VER2 "$\nor greater."
!insertmacro LANG_STRING NOT_12B "This version of Moving Pictures will not work with MediaPortal 1.2 Beta+."

!insertmacro LANG_STRING DLL_DESCRIPTION "The Moving Pictures plug-in."
!insertmacro LANG_STRING GENERIC_SKIN_DESCRIPTION "If selected, this will install the Generic Skin files for skins that do not currently support Moving Pictures. All existing files will be backed up." 
!insertmacro LANG_STRING DEFAULTWIDE_SKIN_DESCRIPTION "If selected, this will install skin files for the DefaultWide skin. This will not overwrite newer files." 
!insertmacro LANG_STRING DEFAULT_SKIN_DESCRIPTION "If selected, this will install skin files for the Default skin. This will not overwrite newer files.$\n$\nCurrently not available for the redesigned Default skin in MediaPortal 1.3." 
!insertmacro LANG_STRING TITAN_SKIN_DESCRIPTION "If selected, this will install ALPHA skin files for the Titan skin. These are unofficial and will not overwrite newer files." 