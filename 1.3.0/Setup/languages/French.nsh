!define LANG "FRENCH" ; Must be the lang name define my NSIS

!insertmacro LANG_STRING NO_MEDIAPORTAL "Moving Pictures est un plug-in pour l'application PCHC MediaPortal.$\nVous devez installer MediaPortal avant d'installer Moving Pictures.$\n Voulez-vous aller sur la page d'accueil de MediaPortal ?"
!insertmacro LANG_STRING MEDIAPORTAL_RUNNING "MediaPortal doit être fermé pour continuer l'installation."
!insertmacro LANG_STRING INVALID_SKIN1 "Impossible de déterminer l'aspect ratio du skin "
!insertmacro LANG_STRING INVALID_SKIN2 " . Le skin $\nGenerique ne sera pas installé pour "
!insertmacro LANG_STRING BAD_SKIN_PATH "Impossible de déterminer le répertoire des Skins. Les fichiers du skin ne seront pas installés!" 
!insertmacro LANG_STRING BAD_DB_PATH "Impossible de déterminer le répertoire des bases de données. La base de données ne sera pas sauvegardée."
!insertmacro LANG_STRING OLD_VER1 "Moving Pictures exige MediaPortal version "
!insertmacro LANG_STRING OLD_VER2 "$\ni supérieur."

!insertmacro LANG_STRING DLL_DESCRIPTION "Le plug-in Moving Pictures."
!insertmacro LANG_STRING GENERIC_SKIN_DESCRIPTION "Si sélectionné, cela installera les fichiers de skin générique pour les skins qui ne supportent pas actuellement Moving Pictures. Tous les fichiers existants seront sauvegardés." 
!insertmacro LANG_STRING DEFAULTWIDE_SKIN_DESCRIPTION "Si sélectionné, cela installera les fichiers de skin pour DefaultWide. Cela ne va pas écraser les fichiers plus récents." 