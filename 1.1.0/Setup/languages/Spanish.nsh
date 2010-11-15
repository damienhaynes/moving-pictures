!define LANG "SPANISH" ; Must be the lang name define my NSIS

!insertmacro LANG_STRING NO_MEDIAPORTAL "Moving Pictures es un plug-in para el programa HTPC Mediaportal.$\nDebe instalar Mediaportal antes de instalar este plug-in. $\n¿Desea ir a la página principal de Mediaportal?"
!insertmacro LANG_STRING MEDIAPORTAL_RUNNING "Por favor, cierre Mediaportal para continuar con la instalación."
!insertmacro LANG_STRING INVALID_SKIN1 "No se pudo determinar la relación de aspecto del "
!insertmacro LANG_STRING INVALID_SKIN2 "skin. El skin $\nGenérico no se instalará para "
!insertmacro LANG_STRING BAD_SKIN_PATH "No se localizó el directorio del skin. ¡Los archivos del skin no serán instalados!"
!insertmacro LANG_STRING BAD_DB_PATH "No se localizó el directorio de la base de datos, y por tanto, no sé hará una copia de la misma."
!insertmacro LANG_STRING OLD_VER1 "Moving Pictures require MediaPortal en su version "
!insertmacro LANG_STRING OLD_VER2 "$\no superior."

!insertmacro LANG_STRING DLL_DESCRIPTION "El plug-in Moving Pictures."
!insertmacro LANG_STRING GENERIC_SKIN_DESCRIPTION "Si lo selecciona, se instalarán los archivos del skin genérico para los skins que no soporten Moving Pictures. Se hará una copia de todos los datos"
!insertmacro LANG_STRING BLUE3WIDE_SKIN_DESCRIPTION "Si lo selecciona, se instalarán los archivos del skin Blue3wide. Esto no sobreescribirá archivos recientes."