!define LANG "ENGLISH" ; Must be the lang name define my NSIS

!insertmacro LANG_STRING NO_MEDIAPORTAL "Moving Pictures is a plug-in for the MediaPortal HTPC Application.$\nYou must install MediaPortal before installing Moving Pictures. Would$\nyou like to goto the MediaPortal homepage?"
!insertmacro LANG_STRING MEDIAPORTAL_RUNNING "Please shutdown MediaPortal to continue the installation."
!insertmacro LANG_STRING INVALID_SKIN "does not appear to be a valid MediaPortal$\nskin. The Generic Skin will not be installed."

!insertmacro LANG_STRING DLL_DESCRIPTION "The Moving Pictures plug-in."
!insertmacro LANG_STRING GENERIC_SKIN_DESCRIPTION "If selected, this will be install the Generic Skin files for skins that do not currently support Moving Pictures. All existing files will be backed up." 