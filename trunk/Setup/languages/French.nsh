!define LANG "FRENCH" ; Must be the lang name define my NSIS

!insertmacro LANG_STRING NO_MEDIAPORTAL "Moving Pictures est un plug-in pour l'application PCHC MediaPortal.$\nVous devez installer MediaPortal avant d'installer Moving Pictures.$\n Voulez-vous aller sur la page d'accueil de MediaPortal ?"
!insertmacro LANG_STRING MEDIAPORTAL_RUNNING "MediaPortal doit �tre ferm� pour continuer l'installation."
!insertmacro LANG_STRING INVALID_SKIN1 "Impossible de d�terminer l'aspect ratio du skin "
!insertmacro LANG_STRING INVALID_SKIN2 " . Le skin $\nGenerique ne sera pas install� pour "
!insertmacro LANG_STRING BAD_SKIN_PATH "Impossible de d�terminer le r�pertoire des Skins. Les fichiers du skin ne seront pas install�s!" 
!insertmacro LANG_STRING BAD_DB_PATH "Impossible de d�terminer le r�pertoire des bases de donn�es. La base de donn�es ne sera pas sauvegard�e."
!insertmacro LANG_STRING OLD_VER1 "Moving Pictures exige MediaPortal version "
!insertmacro LANG_STRING OLD_VER2 "$\ni sup�rieur."

!insertmacro LANG_STRING DLL_DESCRIPTION "Le plug-in Moving Pictures."
!insertmacro LANG_STRING GENERIC_SKIN_DESCRIPTION "Si s�lectionn�, cela installera les fichiers de skin g�n�rique pour les skins qui ne supportent pas actuellement Moving Pictures. Tous les fichiers existants seront sauvegard�s." 
!insertmacro LANG_STRING DEFAULTWIDE_SKIN_DESCRIPTION "Si s�lectionn�, cela installera les fichiers de skin pour DefaultWide. Cela ne va pas �craser les fichiers plus r�cents." 