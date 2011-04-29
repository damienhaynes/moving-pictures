!define LANG "SWEDISH" ; Must be the lang name define my NSIS

!insertmacro LANG_STRING NO_MEDIAPORTAL "Moving Pictures är ett plugin för MediaPortal.$\nDu måste installera MediaPortal för att kunna använda Moving Pictures. $\nVill du öppna MediaPortals webbsida?"
!insertmacro LANG_STRING MEDIAPORTAL_RUNNING "Stäng av MediaPortal för att kunna påbörja installationen."
!insertmacro LANG_STRING INVALID_SKIN1 "Kan inte avgöra bildproportionerna för "
!insertmacro LANG_STRING INVALID_SKIN2 "-skinnet. $\nStandardskinnet kommer inte att installeras för "
!insertmacro LANG_STRING BAD_SKIN_PATH "Hittar inte Skins-mappen. Skin-filerna kommer inte installeras!"
!insertmacro LANG_STRING BAD_DB_PATH "Hittar inte databasmappen. Databasen kommer inte säkerhetskopieras."

!insertmacro LANG_STRING DLL_DESCRIPTION "Moving Pictures-pluginet."
!insertmacro LANG_STRING GENERIC_SKIN_DESCRIPTION "Installera standard-skinfilerna för skin som inte har stöd för Moving Pictures. Alla befintliga filer kommer att säkerhetskopieras."
!insertmacro LANG_STRING DEFAULTWIDE_SKIN_DESCRIPTION "Installera skinfiler för DefaultWide-skinnet. Nyare filer skrivs ej över."