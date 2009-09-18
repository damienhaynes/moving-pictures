# build version info
!execute "makensis GetVersion.nsi"

#define general product info
!define NAME "Moving Pictures"
!define URL www.moving-pictures.tv
!define REGKEY "SOFTWARE\${Name}"

# grab version from DLL
!system "GetVersion.exe"
!include "Version.txt"

# Moving Pictures Specific Variable
Var MEDIAPORTAL_DIR
Var PLUGIN_DIR
Var SKIN_DIR
Var CURR_SKIN
Var CURR_SKIN_WIDTH
Var CURR_SKIN_HEIGHT
Var CURR_SKIN_ASPECT_RATIO
Var GENERIC_SKIN_TAG_VALUE
Var INSTALL_NUMBER

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
        File ..\MovingPictures\bin\Release\MovingPictures.dll
 		
        # if the files failed to copy, MediaPortal is probably running
        # prompt to close MediaPortal and retry.
        IfErrors mediaportal_running everything_is_fine
        mediaportal_running:
            MessageBox MB_RETRYCANCEL|MB_ICONEXCLAMATION $(MEDIAPORTAL_RUNNING) IDRETRY retry
            Abort
    everything_is_fine:
    
    Call updateRegistry
SectionEnd

Section "Blue3wide Skin Support" SEC0002
    ${If} ${FileExists} $SKIN_DIR\Blue3wide\*.*
        SetOverwrite ifnewer

        SetOutPath $SKIN_DIR\Blue3wide
        File "..\MovingPictures\MainUI\Blue3wide\*.*"

        SetOutPath $SKIN_DIR\Blue3wide\Media
        File "..\MovingPictures\MainUI\Blue3wide\Media\*.*"  
    ${EndIf}
SectionEnd

# Loops through each skin folder and sends them off
# for processing and possible generic skin installation
Section "Generic Skin Support" SEC0001
    # loop through folders in the skin folder 
    FindFirst $0 $1 "$SKIN_DIR\*.*"
    directory_loop:
        # if no results, quit
        StrCmp $1 "" directory_loop_done
        
        # if this is the current or previous directory, skip
        StrCmp $1 "." next
        StrCmp $1 ".." next

        # if this is a folder check the movingpictures.xml
        IfFileExists "$SKIN_DIR\$1\*.*" 0 not_a_directory
            StrCpy $CURR_SKIN $1
			Call processCurrentSkin            
        not_a_directory:
        
        # setup iteration variables for next loop
        next:
        FindNext $0 $1
        goto directory_loop
    directory_loop_done:
SectionEnd

# set description text for install components
!insertmacro MUI_FUNCTION_DESCRIPTION_BEGIN
  !insertmacro MUI_DESCRIPTION_TEXT ${SEC0000} $(DLL_DESCRIPTION)
  !insertmacro MUI_DESCRIPTION_TEXT ${SEC0001} $(GENERIC_SKIN_DESCRIPTION)
  !insertmacro MUI_DESCRIPTION_TEXT ${SEC0002} $(BLUE3WIDE_SKIN_DESCRIPTION)
!insertmacro MUI_FUNCTION_DESCRIPTION_END

# startup tasks
Function .onInit
    InitPluginsDir
    
    # set the main plugin as selected and read only in selection list
    IntOp $0 ${SF_SELECTED} | ${SF_RO}
    SectionSetFlags ${SEC0000} $0
    
    # grab various fields from registry
    Call getMediaPortalDir
    Call getPreviousInstallInfo
    
FunctionEnd

# Checks if the current skin should be updated, and if so,
# sends it off for file copying.
Function processCurrentSkin
    Push $0 

    # if this skin already has a movingpictures.xml file, 
    # check if it's the generic skin 
    ${If} ${FileExists} $SKIN_DIR\$CURR_SKIN\movingpictures.xml
    
        # determine the filesize of the movingpixtures.xml file
        # for this skin       
        Push "$SKIN_DIR\$CURR_SKIN\movingpictures.xml"
        Call FileSize
        Pop $0

        # if the filesize matches a known size for a 0.6 generic skin, copy
        # this should be removed for 0.8
        ${If} $0 == 23571
        ${OrIf} $0 == 23544
        ${OrIf} $0 == 29568
            goto copy
        ${EndIf}
        
        # try to grab the <movingpictures_generic_skin> node if we fail at any step,
        # assume this is NOT a generic skin
        push $0
        push $1
        ${xml::LoadFile} "$SKIN_DIR\$CURR_SKIN\movingpictures.xml" $1
        IntCmp $1 -1 xmlfail
        ${xml::RootElement} $0 $1
        IntCmp $1 -1 xmlfail
        ${xml::XPathNode} "//movingpictures_generic_skin" $1
        IntCmp $1 -1 xmlfail
        ${xml::GetText} $GENERIC_SKIN_TAG_VALUE $1
        IntCmp $1 -1 xmlfail   
        goto xmlsuccess 

        # clean up after XML processing                  
        xmlfail:   
            StrCpy $GENERIC_SKIN_TAG_VALUE "false"
        xmlsuccess:
        pop $1
        pop $0
        
        # check the <movingpictures_generic_skin> value to see if this is 
        # the generic skin
        StrCmp $GENERIC_SKIN_TAG_VALUE "true" copy
        
        # if we didnt meet one of the above conditions, it's not the generic 
        # skin file, so we dont copy any files
        goto done
    ${EndIf}
    
    # copy the skin files to the folder       
    copy:
    call copyGenericSkin    

    done:
    pop $0
FunctionEnd

# Copies generic skin files to the current skin folder based on
# the CURR_SKIN_ASPECT_RATIO variable
Function copyGenericSkin
    ;MessageBox MB_OK "Installing Generic Skin for $CURR_SKIN..."

    Call backupOldSkinFiles
    Call determineSkinAspectRatio
    
    # jump to the appropriate copy section based on aspect ratio
    SetOverwrite ifnewer
    StrCmp $CURR_SKIN_ASPECT_RATIO "4x3" _4x3
    StrCmp $CURR_SKIN_ASPECT_RATIO "16x9" _16x9
    goto unknown
    
    # copy over 4x3 skin files for this skin
    _4x3:
        SetOutPath $SKIN_DIR\$CURR_SKIN
        File "..\MovingPictures\MainUI\Generic Skin\4x3\*.*"
        SetOutPath $SKIN_DIR\$CURR_SKIN\Media\MovingPictures
        File "..\MovingPictures\MainUI\Generic Skin\16x9\Media\MovingPictures\*.*"
        goto done
        
    # copy over 16x9 skin files for this skin        
    _16x9:    
        SetOutPath $SKIN_DIR\$CURR_SKIN
        File "..\MovingPictures\MainUI\Generic Skin\16x9\*.*"
        SetOutPath $SKIN_DIR\$CURR_SKIN\Media\MovingPictures
        File "..\MovingPictures\MainUI\Generic Skin\16x9\Media\MovingPictures\*.*"
        goto done
    
    #unkown aspect ratio. implies the script has no references.xml
    unknown:
        MessageBox MB_OK|MB_ICONEXCLAMATION "$(INVALID_SKIN1)$CURR_SKIN$(INVALID_SKIN2)$CURR_SKIN."
    done:

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
        CreateDirectory "$SKIN_DIR\$CURR_SKIN\MovingPictures_Backup\$INSTALL_NUMBER\Media\MovingPictures"
        CopyFiles /SILENT "$SKIN_DIR\$CURR_SKIN\movingpictures*.xml" "$SKIN_DIR\$CURR_SKIN\MovingPictures_Backup\$INSTALL_NUMBER\"
        CopyFiles /SILENT "$SKIN_DIR\$CURR_SKIN\Media\MovingPictures\*.*" "$SKIN_DIR\$CURR_SKIN\MovingPictures_Backup\$INSTALL_NUMBER\Media\MovingPictures\"
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
    
    # store our output folders
    StrCpy $PLUGIN_DIR "$MEDIAPORTAL_DIR\plugins\Windows"
	
	#grab the skin folder
	${xml::LoadFile} "$MEDIAPORTAL_DIR\MediaPortalDirs.xml" $1
    IntCmp $1 -1 fail
    ${xml::RootElement} $0 $1
    IntCmp $1 -1 fail
    ${xml::XPathNode} "//Config/Dir[@id='Skin']/Path" $1
    IntCmp $1 -1 fail
    ${xml::GetText} $2 $1
    IntCmp $1 -1 fail
	
    # verify the skin folder was found properly
    
    #check_for_new_path_on_vista_or_win7
        ExpandEnvStrings $SKIN_DIR $2
        IfFileExists $SKIN_DIR\*.* done check_for_relative_path
	check_for_relative_path:
		StrCpy $SKIN_DIR "$MEDIAPORTAL_DIR\$SKIN_DIR"
		IfFileExists $SKIN_DIR\*.* done check_for_new_xp_path
    check_for_new_xp_path:
        ${StrReplace} $SKIN_DIR "%ProgramData%" "%ALLUSERSPROFILE%\Application Data" $2
        ExpandEnvStrings $SKIN_DIR $SKIN_DIR
        IfFileExists $SKIN_DIR\*.* done fail
	fail:
		MessageBox MB_OK|MB_ICONEXCLAMATION $(BAD_SKIN_PATH)	
	done:
		
	Pop $2
    Pop $1
	Pop $0
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