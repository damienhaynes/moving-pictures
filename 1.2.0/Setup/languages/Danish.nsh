!define LANG "DANISH" ; Must be the lang name define my NSIS

!insertmacro LANG_STRING NO_MEDIAPORTAL "Moving Pictures er et plugin for MediaPortal HTPC softwaren.$\nDu skal derfor installere MediaPortal før du installerer Moving Pictures. Ønkser$\ndu at gå til MediaPortal hjemmeside?"
!insertmacro LANG_STRING MEDIAPORTAL_RUNNING "Luk venligst MediaPortal for at fortsætte installationen."
!insertmacro LANG_STRING INVALID_SKIN1 "Kunne ikke bestemme synsviklen for"
!insertmacro LANG_STRING INVALID_SKIN2 " skinnet. Fælles$\n skinnet vil derfor ikke blive installeret for "
!insertmacro LANG_STRING BAD_SKIN_PATH "Kunne ikke finde skin mappen. Skin filerne vil IKKE blive installeret!" 
!insertmacro LANG_STRING BAD_DB_PATH "Kunne ikke finde database mappen. Databasen vil ikke blive sikkerhedskopieret."
!insertmacro LANG_STRING OLD_VER1 "Moving Pictures kræver MediaPortal version "
!insertmacro LANG_STRING OLD_VER2 "$\neller større."

!insertmacro LANG_STRING DLL_DESCRIPTION "Moving Pictures plug-in'et."
!insertmacro LANG_STRING GENERIC_SKIN_DESCRIPTION "Hvis valgt, vil dette installere fælles skin filerne for skin der på nuværende tidspunkt ikke understøtter Moving Pictures. Alle eksisterende filer vil blive sikkerhedskopiret." 
!insertmacro LANG_STRING DEFAULTWIDE_SKIN_DESCRIPTION "Hvis valgt, vil dette installere skin filerne for DefaultWide skinnet. Dette vil ikke overskrive nyere filer." 