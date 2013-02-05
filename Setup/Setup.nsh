# build version info
!execute "makensis GetVersion.nsi"

#define general product info
!define NAME "Moving Pictures"
!define URL www.moving-pictures.tv
!define REGKEY "SOFTWARE\${Name}"

# Required MediaPortal Version
!define MP_MAJOR 1
!define MP_MINOR 2
!define MP_POINT 3
!define MP_VER_NAME "1.2.3"

# Required MediaPortal Version for New DefaultWide
!define MP_13B_MAJOR 1
!define MP_13B_MINOR 2
!define MP_13B_POINT 200
!define MP_13B_VER_NAME "1.3 Beta"

# grab version from DLL
!system "GetVersion.exe"
!include "Version.txt"

# Moving Pictures Specific Variable
Var MEDIAPORTAL_DIR
Var PLUGIN_DIR
Var SKIN_DIR
Var DB_DIR
Var CURR_SKIN
Var CURR_SKIN_WIDTH
Var CURR_SKIN_HEIGHT
Var CURR_SKIN_ASPECT_RATIO
Var GENERIC_SKIN_TAG_VALUE
Var INSTALL_NUMBER
Var CURR_DATE
Var DEFAULT_SKIN_VERSION

# MUI Symbol Definitions
!define MUI_FINISHPAGE_NOAUTOCLOSE
!define MUI_FINISHPAGE_SHOWREADME $PLUGIN_DIR\moving-pictures-release-notes.txt

# Included files
!include Sections.nsh
!include MUI2.nsh
!include LogicLib.nsh
!include FileFunc.nsh
!include XML.nsh
!include LanguageMacros.nsh
!include StrRep.nsh

# Installer pages
!insertmacro MUI_PAGE_WELCOME
!insertmacro MUI_PAGE_COMPONENTS
!insertmacro MUI_PAGE_INSTFILES
!insertmacro MUI_PAGE_FINISH

# Installer languages
!insertmacro LANG_LOAD "English"
!insertmacro LANG_LOAD "Danish"
!insertmacro LANG_LOAD "Swedish"
!insertmacro LANG_LOAD "French"
!insertmacro LANG_LOAD "German"
!insertmacro LANG_LOAD "Spanish"
!insertmacro LANG_LOAD "Italian"

# set the build filename based on environment variables
!ifdef RELEASE
    OutFile moving-pictures-${VersionShort}-setup.exe
!else
    OutFile moving-pictures-${VersionLong}-alpha.exe
!endif

# Installer attributes
CRCCheck on
XPStyle on
ShowInstDetails hide
Name "${NAME}"
Icon "logo.ico"
VIProductVersion ${VERSION}
VIAddVersionKey ProductName "${NAME}"
VIAddVersionKey CompanyName "${NAME}"
VIAddVersionKey ProductVersion "${VERSION}"
VIAddVersionKey CompanyWebsite "${URL}"
VIAddVersionKey FileVersion "${VERSION}"
VIAddVersionKey FileDescription ""
VIAddVersionKey LegalCopyright ""

# installs main Moving Pictures DLL file.
Section "Moving Pictures Plugin" SEC0000
    
    # try to install the DLL
    retry:
 		# copy all localized translations xml in the correct location
		SetOverwrite ifnewer
		SetOutPath $SKIN_DIR\..\language\MovingPictures
		File "..\MovingPictures\Resources\language\*.xml"

        SetOutPath $PLUGIN_DIR
        SetOverwrite try
        File ..\MovingPictures\Resources\moving-pictures-release-notes.txt
        !ifdef RELEASE
        File ..\MovingPictures\bin\Release\MovingPictures.dll
		File ..\MovingPictures\bin\Release\Cornerstone.dll
		File ..\MovingPictures\bin\Release\Cornerstone.MP.dll
        SetOutPath $MEDIAPORTAL_DIR
		File ..\MovingPictures\bin\Release\NLog.dll
        !else
        File ..\MovingPictures\bin\Debug\MovingPictures.dll
		File ..\MovingPictures\bin\Debug\Cornerstone.dll
		File ..\MovingPictures\bin\Debug\Cornerstone.MP.dll
        SetOutPath $MEDIAPORTAL_DIR
		File ..\MovingPictures\bin\Debug\NLog.dll
        !endif
 		
		Delete $MEDIAPORTAL_DIR\Cornerstone.dll
		Delete $MEDIAPORTAL_DIR\Cornerstone.MP.dll
		
        # if the files failed to copy, MediaPortal is probably running
        # prompt to close MediaPortal and retry.
        IfErrors mediaportal_running everything_is_fine
        mediaportal_running:
            MessageBox MB_RETRYCANCEL|MB_ICONEXCLAMATION $(MEDIAPORTAL_RUNNING) IDRETRY retry
            Abort
    everything_is_fine:
    
	Call backupDatabase
    Call updateRegistry
SectionEnd

Section "DefaultWide Skin Support" SEC0002
    ${If} ${FileExists} $SKIN_DIR\DefaultWide\*.*
        SetOverwrite ifnewer

		${If} $DEFAULT_SKIN_VERSION == "OLD"

        SetOutPath $SKIN_DIR\DefaultWide
        File "..\MovingPictures\Resources\skins\DefaultWide-1.2\*.*"

        SetOutPath $SKIN_DIR\DefaultWide\Media
        File "..\MovingPictures\Resources\skins\DefaultWide-1.2\Media\*.*"  
        
        SetOutPath $SKIN_DIR\DefaultWide\Media\Categories
        File "..\MovingPictures\Resources\skins\DefaultWide-1.2\Media\Categories\*.*"  
		
		${Else}

        SetOutPath $SKIN_DIR\DefaultWide
        File "..\MovingPictures\Resources\skins\DefaultWide\*.*"

        SetOutPath $SKIN_DIR\DefaultWide\Media
        File "..\MovingPictures\Resources\skins\DefaultWide\Media\*.*"  
        
        SetOutPath $SKIN_DIR\DefaultWide\Media\Categories
        File "..\MovingPictures\Resources\skins\DefaultWide\Media\Categories\*.*"  

		
		${EndIf}
		

    ${EndIf}
SectionEnd

Section "Default Skin Support" SEC0003
    ${If} ${FileExists} $SKIN_DIR\Default\*.*
        SetOverwrite ifnewer

        SetOutPath $SKIN_DIR\Default
        File "..\MovingPictures\Resources\skins\Default-1.2\*.*"

        SetOutPath $SKIN_DIR\Default\Media
        File "..\MovingPictures\Resources\skins\Default-1.2\Media\*.*"  
        
        SetOutPath $SKIN_DIR\Default\Media\Categories
        File "..\MovingPictures\Resources\skins\Default-1.2\Media\Categories\*.*"  

    ${EndIf}
SectionEnd

Section "Titan Skin Support (Alpha)" SEC0004
    ${If} ${FileExists} $SKIN_DIR\Titan\*.*
        SetOverwrite ifnewer

        SetOutPath $SKIN_DIR\Titan
        File "..\MovingPictures\Resources\skins\Titan\*.*"

        SetOutPath $SKIN_DIR\Titan\Media
        File "..\MovingPictures\Resources\skins\Titan\Media\*.*"  

    ${EndIf}
SectionEnd

# set description text for install components
!insertmacro MUI_FUNCTION_DESCRIPTION_BEGIN
  !insertmacro MUI_DESCRIPTION_TEXT ${SEC0000} $(DLL_DESCRIPTION)
  !insertmacro MUI_DESCRIPTION_TEXT ${SEC0001} $(GENERIC_SKIN_DESCRIPTION)
  !insertmacro MUI_DESCRIPTION_TEXT ${SEC0002} $(DEFAULTWIDE_SKIN_DESCRIPTION)
  !insertmacro MUI_DESCRIPTION_TEXT ${SEC0003} $(DEFAULT_SKIN_DESCRIPTION)  
  !insertmacro MUI_DESCRIPTION_TEXT ${SEC0004} $(TITAN_SKIN_DESCRIPTION)  
!insertmacro MUI_FUNCTION_DESCRIPTION_END

# startup tasks
Function .onInit
    !insertmacro MUI_LANGDLL_DISPLAY

    InitPluginsDir
    
    # set the main plugin as selected and read only in selection list
    IntOp $0 ${SF_SELECTED} | ${SF_RO}
    SectionSetFlags ${SEC0000} $0
    
    # grab various fields from registry
    SetShellVarContext all
    Call getMediaPortalDir
    Call getPluginDir
    Call verifyMediaPortalVer
	Call determineDefaultSkinVer
	Call getSkinDir
	Call getDatabaseDir
    Call getPreviousInstallInfo
    
FunctionEnd

# reads the references.xml file from the skin folder to determine
# the aspect ratio of the current skin
Function determineSkinAspectRatio
    push $0
    push $1
    
    # grab the skin width
    ${xml::LoadFile} "$SKIN_DIR\$CURR_SKIN\references.xml" $1
    IntCmp $1 -1 fail
    ${xml::RootElement} $0 $1
    IntCmp $1 -1 fail
    ${xml::XPathNode} "//skin/width" $1
    IntCmp $1 -1 fail
    ${xml::GetText} $CURR_SKIN_WIDTH $1
    IntCmp $1 -1 fail
    
    # grab the skin height
    ${xml::RootElement} $0 $1
    IntCmp $1 -1 fail
    ${xml::XPathNode} "//skin/height" $1
    IntCmp $1 -1 fail
    ${xml::GetText} $CURR_SKIN_HEIGHT $1
    IntCmp $1 -1 fail
    ${xml::Unload}
    
    # set the aspect ratio variable based on width/height ratio
    IntOp $0 $CURR_SKIN_WIDTH * 100
    IntOp $0 $0 / $CURR_SKIN_HEIGHT
    IntCmp $0 140 0 0 _16x9
        StrCpy $CURR_SKIN_ASPECT_RATIO "4x3"
        goto aspect_complete
    _16x9:
        StrCpy $CURR_SKIN_ASPECT_RATIO "16x9"
    aspect_complete:
    
    # error logic to clear out aspect ratio variable
    goto done
    fail:
    StrCpy $CURR_SKIN_ASPECT_RATIO ""
    done:
    
    pop $1
    pop $0
FunctionEnd

Function backupOldSkinFiles
    ${If} ${FileExists} $SKIN_DIR\$CURR_SKIN\movingpictures.xml
        # create dir and backup existing skin files
        CreateDirectory "$SKIN_DIR$CURR_SKIN\MovingPictures_Backup\$CURR_DATE\$INSTALL_NUMBER\Media\MovingPictures"
        CopyFiles /SILENT "$SKIN_DIR$CURR_SKIN\movingpictures*.xml" "$SKIN_DIR$CURR_SKIN\MovingPictures_Backup\$CURR_DATE\$INSTALL_NUMBER\"
        CopyFiles /SILENT "$SKIN_DIR$CURR_SKIN\Media\MovingPictures\*.*" "$SKIN_DIR$CURR_SKIN\MovingPictures_Backup\$CURR_DATE\$INSTALL_NUMBER\Media\MovingPictures\"
    ${EndIf}
FunctionEnd

Function backupDatabase
    ${If} ${FileExists} $DB_DIR\movingpictures.db3
        # create dir and backup existing database
        CreateDirectory "$DB_DIR\MovingPictures_Backup\$CURR_DATE\$INSTALL_NUMBER"
        CopyFiles /SILENT "$DB_DIRmovingpictures.db3" "$DB_DIRMovingPictures_Backup\$CURR_DATE\$INSTALL_NUMBER\movingpictures.db3"
    ${EndIf}
FunctionEnd

# Grabs the Install Path of MediaPortal
Function getMediaPortalDir
    Push $0
	Push $1
    Push $2
	
	ReadRegStr $MEDIAPORTAL_DIR HKEY_LOCAL_MACHINE SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\MediaPortal InstallPath
    
    # if the mediaportal install path was missing, inform the user and prompt to
    # goto http://www.team-mediaportal.com
    IfErrors mediaportal_not_installed mediaportal_found
    mediaportal_not_installed:
        MessageBox MB_YESNO|MB_ICONSTOP $(NO_MEDIAPORTAL) IDYES true IDNO false
        true:
            ExecShell "open" "http://www.team-mediaportal.com"
        false:
            
        Abort
    mediaportal_found:
    
	Pop $2
    Pop $1
	Pop $0
FunctionEnd

Function getPluginDir
    Push $0
	Push $1
    Push $2

	#grab the plugins folder
	${xml::LoadFile} "$DOCUMENTS\Team MediaPortal\MediaPortalDirs.xml" $1
	IntCmp $1 0 specialdir
	SetShellVarContext current
	${xml::LoadFile} "$DOCUMENTS\Team MediaPortal\MediaPortalDirs.xml" $1
	IntCmp $1 0 specialdir
	${xml::LoadFile} "$MEDIAPORTAL_DIR\MediaPortalDirs.xml" $1
	specialdir:
	SetShellVarContext all
    IntCmp $1 -1 fail
    ${xml::RootElement} $0 $1
    IntCmp $1 -1 fail
    ${xml::XPathNode} "//Config/Dir[@id='Plugins']/Path" $1
    IntCmp $1 -1 fail
    ${xml::GetText} $2 $1
    IntCmp $1 -1 fail
    
    #check_for_new_path_on_vista_or_win7
        ExpandEnvStrings $PLUGIN_DIR "$2\Windows"
        IfFileExists $PLUGIN_DIR\*.* done check_for_relative_path
	check_for_relative_path:
		StrCpy $PLUGIN_DIR "$MEDIAPORTAL_DIR\$PLUGIN_DIR"
		IfFileExists $PLUGIN_DIR\*.* done check_for_new_xp_path
    check_for_new_xp_path:
        ${StrReplace} $PLUGIN_DIR "%ProgramData%" $APPDATA $2
        ExpandEnvStrings $PLUGIN_DIR $PLUGIN_DIR
        IfFileExists $PLUGIN_DIR\*.* done fail
	fail:
		StrCpy $PLUGIN_DIR "$MEDIAPORTAL_DIR\plugins\Windows"
	done:
	
	Pop $2
    Pop $1
	Pop $0
FunctionEnd

Function getSkinDir
    Push $0
	Push $1
    Push $2

	#grab the skin folder
	${xml::LoadFile} "$DOCUMENTS\Team MediaPortal\MediaPortalDirs.xml" $1
	IntCmp $1 0 specialdir
	SetShellVarContext current
	${xml::LoadFile} "$DOCUMENTS\Team MediaPortal\MediaPortalDirs.xml" $1
	IntCmp $1 0 specialdir
	${xml::LoadFile} "$MEDIAPORTAL_DIR\MediaPortalDirs.xml" $1
	specialdir:
	SetShellVarContext all
    IntCmp $1 -1 fail
    ${xml::RootElement} $0 $1
    IntCmp $1 -1 fail
    ${xml::XPathNode} "//Config/Dir[@id='Skin']/Path" $1
    IntCmp $1 -1 fail
    ${xml::GetText} $2 $1
    IntCmp $1 -1 fail
    
    #check_for_new_path_on_vista_or_win7
        ExpandEnvStrings $SKIN_DIR $2
        IfFileExists $SKIN_DIR\*.* done check_for_relative_path
	check_for_relative_path:
		StrCpy $SKIN_DIR "$MEDIAPORTAL_DIR\$SKIN_DIR"
		IfFileExists $SKIN_DIR\*.* done check_for_new_xp_path
    check_for_new_xp_path:
        ${StrReplace} $SKIN_DIR "%ProgramData%" $APPDATA $2
        ExpandEnvStrings $SKIN_DIR $SKIN_DIR
        IfFileExists $SKIN_DIR\*.* done fail
	fail:
		MessageBox MB_OK|MB_ICONEXCLAMATION $(BAD_SKIN_PATH)	
	done:
	
	Pop $2
    Pop $1
	Pop $0
FunctionEnd

Function getDatabaseDir
    Push $0
	Push $1
    Push $2

	#grab the database folder
	${xml::LoadFile} "$DOCUMENTS\Team MediaPortal\MediaPortalDirs.xml" $1
	IntCmp $1 0 specialdir
	SetShellVarContext current
	${xml::LoadFile} "$DOCUMENTS\Team MediaPortal\MediaPortalDirs.xml" $1
	IntCmp $1 0 specialdir
	${xml::LoadFile} "$MEDIAPORTAL_DIR\MediaPortalDirs.xml" $1
	specialdir:
	SetShellVarContext all
    IntCmp $1 -1 fail
    ${xml::RootElement} $0 $1
    IntCmp $1 -1 fail
    ${xml::XPathNode} "//Config/Dir[@id='Database']/Path" $1
    IntCmp $1 -1 fail
    ${xml::GetText} $2 $1
    IntCmp $1 -1 fail
    
    #check_for_new_path_on_vista_or_win7
        ExpandEnvStrings $DB_DIR $2
        IfFileExists $DB_DIR\*.* done check_for_relative_path
	check_for_relative_path:
		StrCpy $DB_DIR "$MEDIAPORTAL_DIR\$DB_DIR"
		IfFileExists $DB_DIR\*.* done check_for_new_xp_path
    check_for_new_xp_path:
        ${StrReplace} $DB_DIR "%ProgramData%" $APPDATA $2
        ExpandEnvStrings $DB_DIR $DB_DIR
        IfFileExists $DB_DIR\*.* done fail
	fail:
		MessageBox MB_OK|MB_ICONEXCLAMATION $(BAD_DB_PATH)	
	done:
	
	Pop $2
    Pop $1
	Pop $0
FunctionEnd

Function verifyMediaPortalVer
    GetDllVersion "$MEDIAPORTAL_DIR\MediaPortal.exe" $R0 $R1
    IntOp $R2 $R0 / 0x00010000
    IntOp $R3 $R0 & 0x0000FFFF
    IntOp $R4 $R1 / 0x00010000
    IntOp $R5 $R1 & 0x0000FFFF
    
    # verify we meet the minimum version requirement
    IntCmp $R2 ${MP_MAJOR} check_minor_ver fail success 
    check_minor_ver:
        IntCmp $R3 ${MP_MINOR} check_point_ver fail success    
    check_point_ver:
	    IntCmp $R4 ${MP_POINT} success fail success 
    fail:
        MessageBox MB_OK|MB_ICONEXCLAMATION "$(OLD_VER1)${MP_VER_NAME}$(OLD_VER2)"
        Abort
    success:
	
FunctionEnd


Function determineDefaultSkinVer
    GetDllVersion "$MEDIAPORTAL_DIR\MediaPortal.exe" $R0 $R1
    IntOp $R2 $R0 / 0x00010000
    IntOp $R3 $R0 & 0x0000FFFF
    IntOp $R4 $R1 / 0x00010000
    IntOp $R5 $R1 & 0x0000FFFF
    
    # verify we meet the minimum version requirement
    IntCmp $R2 ${MP_13B_MAJOR} check_minor_ver fail success 
    check_minor_ver:
        IntCmp $R3 ${MP_13B_MINOR} check_point_ver fail success    
    check_point_ver:
	    IntCmp $R4 ${MP_13B_POINT} success fail success 
    fail:
        strcpy $DEFAULT_SKIN_VERSION "OLD"
        Goto done
    success:
	    strcpy $DEFAULT_SKIN_VERSION "NEW"
	done:
FunctionEnd


# grabs the install number counter from the registry and increments
# mostly used for ensuring unique backup directories
Function getPreviousInstallInfo
    ReadRegStr $INSTALL_NUMBER HKEY_LOCAL_MACHINE "${REGKEY}" InstallCount
    IfErrors 0 ok
        strcpy $INSTALL_NUMBER "0"
    ok:
    
    # increment our backup counter and save it to registry
    # this is needed to always have a unique backup dir
    IntOp $INSTALL_NUMBER $INSTALL_NUMBER + 1       
    WriteRegStr HKEY_LOCAL_MACHINE "${REGKEY}" InstallCount $INSTALL_NUMBER     
	
	## grab the current date. alao used for file backup purposes
	Push $0
	Push $1
	Push $2
	Push $3
	Push $4
	Push $5
	Push $6
	${GetTime} "" "L" $0 $1 $2 $3 $4 $5 $6
	StrCpy $CURR_DATE "$1-$0-$2"
	Pop $6
	Pop $5
	Pop $4
	Pop $3
	Pop $2
	Pop $1
	
FunctionEnd

# stores the version number (and other relevant data) of moving pictures 
Function updateRegistry
    WriteRegStr HKEY_LOCAL_MACHINE "${REGKEY}" Version ${VersionLong}
    WriteRegStr HKEY_LOCAL_MACHINE "${REGKEY}" VersionMajor ${VersionMajor}
    WriteRegStr HKEY_LOCAL_MACHINE "${REGKEY}" VersionMinor ${VersionMinor}
    WriteRegStr HKEY_LOCAL_MACHINE "${REGKEY}" VersionPoint ${VersionPoint}
    WriteRegStr HKEY_LOCAL_MACHINE "${REGKEY}" VersionRevision ${VersionRevision}
FunctionEnd

# returns filesize of a file. input and output is first
# variable on the stack.
Function FileSize
  Exch $0
  Push $1
  FileOpen $1 $0 "r"
  FileSeek $1 0 END $0
  FileClose $1
  Pop $1
  Exch $0
FunctionEnd