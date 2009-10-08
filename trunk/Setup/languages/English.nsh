!define LANG "ENGLISH" ; Must be the lang name define my NSIS

!insertmacro LANG_STRING NO_MEDIAPORTAL "Moving Pictures is a plug-in for the MediaPortal HTPC Application.$\nYou must install MediaPortal before installing Moving Pictures. Would$\nyou like to goto the MediaPortal homepage?"
!insertmacro LANG_STRING MEDIAPORTAL_RUNNING "Please shutdown MediaPortal to continue the installation."
!insertmacro LANG_STRING INVALID_SKIN1 "Could not determine the aspect ratio of the "
!insertmacro LANG_STRING INVALID_SKIN2 " skin. The$\nGeneric Skin will not be installed for "
!insertmacro LANG_STRING BAD_SKIN_PATH "Could not locate Skins directory. Skin files will not be installed!" 
!insertmacro LANG_STRING BAD_DB_PATH "Could not locate Database directory. Database will not be backed up."

!insertmacro LANG_STRING DLL_DESCRIPTION "The Moving Pictures plug-in."
!insertmacro LANG_STRING GENERIC_SKIN_DESCRIPTION "If selected, this will be install the Generic Skin files for skins that do not currently support Moving Pictures. All existing files will be backed up." 
!insertmacro LANG_STRING BLUE3WIDE_SKIN_DESCRIPTION "If selected, this will be install skin files for the Blue3wide skin. This will not overwrite newer files." 