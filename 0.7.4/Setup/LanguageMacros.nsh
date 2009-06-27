# These macros are used to simplify the translation files
# so that translators have to deal with as little with the
# scripting language as possible.

!macro LANG_LOAD LANGLOAD
  !insertmacro MUI_LANGUAGE "${LANGLOAD}"
  !include "languages\${LANGLOAD}.nsh"
  !undef LANG
!macroend
 
!macro LANG_STRING NAME VALUE
  LangString "${NAME}" "${LANG_${LANG}}" "${VALUE}"
!macroend