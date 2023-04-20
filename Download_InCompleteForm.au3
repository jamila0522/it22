#Include <IE.au3>
#Include <Excel.au3>
#include <Debug.au3>
#include <Array.au3>
#include <INet.au3>
#Include <file.au3>
#include-once

#region variable declare
$SmtpServer = "10.108.0.31"              
$FromName = "SERVER REPORT"                      
$FromAddress = "" 
$ToAddress = ""   
$Subject = "REPORT ALL FORM AND INCOMPLETE FORM"                   
$Body = ""                              
$AttachFiles = ""
$CcAddress = ""       
$BccAddress = ""    
$Importance = "Normal"                 
$Username = "******"                    
$Password = "********"                  
$IPPort = 25                           
$ssl = 0  
Global $oMyRet[2]
Global $oMyError = ObjEvent("AutoIt.Error", "MyErrFunc")
$isloop = True
$isLogout = False
Global $timeLoop = 0
Global $bocheck, $bolink, $bolink1, $startDate, $endDate, $userName, $passWord, $formID, $verSion, $downloadStatus, $processStatus
Main()
Func Main()
	Do
		BlockInput(1)
		MainProcess()
		BlockInput(0)
		Sleep($timeLoop)
		
	Until $isloop = False
EndFunc
#endregion

#region Main Process
Func MainProcess()

	Select
	Case WinExists("Online Application Form BO - Windows Internet Explorer provided by Standard Chartered Bank")
		$bocheck = True
	Case Else
		$bocheck = False
	EndSelect

	$iLine = 2 ; line for setup Incomplete Form
	$iLine2 = 6 ; line for setup complete Form
	
	if $bocheck Then
;~ 	set path excel file
		$sFileName = @ScriptDir & "\Config.xls"
		$excel = _ExcelBookAttach($sFileName)
		
;~ 	read config form excell	
	$startDate = _ExcelReadCell($excel, $iLine, 1)
	$endDate = _ExcelReadCell($excel, $iLine, 2)
	$userName = _ExcelReadCell($excel, $iLine, 3)
	$passWord = _ExcelReadCell($excel, $iLine, 4)
	$FromAddress =  _ExcelReadCell($excel, $iLine, 5)
	$ToAddress  = _ExcelReadCell($excel, $iLine, 6)
	$timeLoop = _ExcelReadCell($excel, $iLine, 7)
	$bolink = _ExcelReadCell($excel, $iLine, 8)
	$bolink1 = _ExcelReadCell($excel, $iLine, 9)
	$formID = _ExcelReadCell($excel, $iLine, 10)
	
	If $isLogout Then
	 $oIE = _IEAttach($bolink1, "url")
	Else
	 $oIE = _IEAttach($bolink, "url")
	EndIf
 
	_IEPropertySet($oIE, "silent", 1)
	_IELoadWait($oIE)

	$oFormLogin = _IEFormGetObjByName ($oIE, "loginForm")
	$user =  _IEFormElementGetObjByName($oFormLogin , "userName")
	$pass =  _IEFormElementGetObjByName($oFormLogin , "password")
	$submit =  _IEFormElementGetObjByName($oFormLogin , "methodToCall")
;~ 	Do login
	_IEFormElementSetValue($user, $userName)
	_IEFormElementSetValue($pass, $passWord)
	
;~ 	Submit Form
	_IEAction ($submit, "click")
	_IELoadWait($oIE)
	
	
;~ Case	Login second time
	If _IEFormGetObjByName ($oIE, "loginForm") <> 0 Then
		$oFormLogin = _IEFormGetObjByName ($oIE, "loginForm")
		$pass =  _IEFormElementGetObjByName($oFormLogin , "password")
		$submit =  _IEFormElementGetObjByName($oFormLogin , "methodToCall")
		$opass = _IEFormElementGetValue($pass)
		If $opass = "" Then
			_IEFormElementSetValue($pass, $passWord)
			_IEAction ($submit, "click")
			_IELoadWait($oIE)
		EndIf
	EndIf
	


;~ -----------------Get report from All Form----------------

	ReportAllForm($oIE)
;~ -----------------Get report Incomplete Application------------------

	ReportIncompleteForm($oIE)

;~ Unzip File

  If FileExists(@ScriptDir & "\AllForm_report.zip") = 1  Then
	  _Zip_UnzipAll(@ScriptDir & "\AllForm_report.zip", @ScriptDir &"\AllForm_report", 20)
  EndIf
;~   Get File Unzip
	Local $aFileList = _FileListToArray(@ScriptDir &"\AllForm_report")
	Local $iFileList = _FileListToArray(@ScriptDir &"\InCompleteForm")

	GetExcelDataProcess(@ScriptDir &"\AllForm_report\" & $aFileList[1], @ScriptDir &"\InCompleteForm\" & $iFileList[1])
;~ 	 _DebugSetup("_DebugReportVar", True)
;~ 	_DebugReportVar("STD",$aFileList[1])


	If FileExists(@ScriptDir & "\InCompleteForm\InCompleteForm_report.xls") = 1  Then
	 $AttachFiles =	@ScriptDir & "\InCompleteForm\InCompleteForm_report.xls;" 
 EndIf
 
	If FileExists(@ScriptDir & "\AllForm_report.zip") = 1  Then
	  $AttachFiles = $AttachFiles & @ScriptDir & "\AllForm_report.zip;"
  EndIf
  
  If FileExists(@ScriptDir & "\Result\ReportCompleteForm.xls") = 1  Then
	  $AttachFiles = $AttachFiles & @ScriptDir & "\Result\ReportCompleteForm.xls"
  EndIf
	
;~  Check file Attach
	If($AttachFiles = "") Then
		$Body = "Don't Have Data For Report"
	EndIf
; Send mail Function
	$rc = _INetSmtpMailCom($SmtpServer, $FromName, $FromAddress, $ToAddress, $Subject, $Body, $AttachFiles, $CcAddress, $BccAddress, $Importance, $Username, $Password, $IPPort, $ssl)

	If $rc = 0 Then
;;Delete File Report
		FileDelete(@ScriptDir & "\InCompleteForm\InCompleteForm_report.xls")
		FileDelete(@ScriptDir & "\AllForm_report.zip")
		FileDelete(@ScriptDir & "\Result\ReportCompleteForm.xls")
		If $aFileList[1] <> "" Then
			FileDelete(@ScriptDir & "\AllForm_report\" & $aFileList[1])
		EndIf
	EndIf
	
	If @error Then
		MsgBox(0, "Error sending message", "Error code:" & @error & "  Description:" & $rc)
	EndIf
; End Send mail Function

;LogOut System
	$oLinks = _IELinkGetCollection($oIE)
	For	 $oLink In  $oLinks
		$sLinkText = _IEPropertyGet($oLink, "innerText")
		If StringInStr($sLinkText, "Logout") Then
			_IEAction($oLink, "click")
			$isLogout = True
        ExitLoop
		EndIf
	Next	
	_IELoadWait($oIE)

	EndIf
EndFunc
#endregion
#region Get report

;~ -----------------Get report from All Form----------------
Func ReportAllForm($oIE)
		$oLinks = _IELinkGetCollection($oIE)
	For	 $oLink In  $oLinks
		$sLinkText = _IEPropertyGet($oLink, "innerText")
		If StringInStr($sLinkText, "Export All Forms") Then
			_IEAction($oLink, "click")
        ExitLoop
		EndIf
	Next	
	_IELoadWait($oIE)
	
;~ Select Form ID	
	$oForm = _IEFormGetObjByName($oIE, "exportAllFormsForm")
	$oSelect =  _IEFormElementGetObjByName($oForm , "formId")
	_IEFormElementOptionSelect ($oSelect, $formID, 1, "byText")
	_IELoadWait($oIE)
;~ 	input date time
    $oForm = _IEFormGetObjByName($oIE, "exportAllFormsForm")
	$oFrom =  _IEFormElementGetObjByName($oForm , "submissionDate")
	$oTo =  _IEFormElementGetObjByName($oForm , "submissionToDate")
	_IEFormElementSetValue($oFrom, $startDate)
	_IEFormElementSetValue($oTo, $endDate)	
	
	_IEFormSubmit ($oForm)
	Sleep(1000)

;save file download	
	if WinExists("File Download") Then
		ControlSend ("File Download", "" , "[CLASS:Button; INSTANCE:2]", "!s")
		Sleep (2000)
		If WinExists("Save As") Then
			ControlSend ("", "", "[CLASS:Edit; INSTANCE:1]", @ScriptDir &"\AllForm_report.zip" & "{Enter}")
		EndIf
	EndIf
	Sleep(3000)
EndFunc

;~ -----------------Get report Incomplete Application---------------
Func ReportIncompleteForm($oIE)
		$oLinks = _IELinkGetCollection($oIE)
	For	 $oLink In  $oLinks
		$sLinkText = _IEPropertyGet($oLink, "innerText")
		If StringInStr($sLinkText, "Incomplete Application") Then
			_IEAction($oLink, "click")
        ExitLoop
		EndIf
	Next	
	_IELoadWait($oIE)
;input form
;Select option
    $oForm = _IEFormGetObjByName($oIE, "formIncomplete")
    $oSelect =  _IEFormElementGetObjByName($oForm , "formId")
	_IEFormElementOptionSelect ($oSelect, $formID, 1, "byText")
	_IELoadWait($oIE)
	;input date time
    $oForm = _IEFormGetObjByName($oIE, "formIncomplete")
	$oFrom =  _IEFormElementGetObjByName($oForm , "fromdate")
	$oTo =  _IEFormElementGetObjByName($oForm , "todate")
	_IEFormElementSetValue($oFrom, $startDate)
	_IEFormElementSetValue($oTo, $endDate)
	
	_IEFormSubmit ($oForm)
	Sleep(1000)
	
;save file download	
	if WinExists("File Download") Then
		ControlSend ("File Download", "" , "[CLASS:Button; INSTANCE:2]", "!s")
		Sleep (2000)
		If WinExists("Save As") Then
			ControlSend ("", "", "[CLASS:Edit; INSTANCE:1]", @ScriptDir &"\InCompleteForm\InCompleteForm_report.xls" & "{Enter}")
		EndIf
	EndIf
	Sleep(3000)
EndFunc
#endregion

#region Send Mail
Func _INetSmtpMailCom($s_SmtpServer, $s_FromName, $s_FromAddress, $s_ToAddress, $s_Subject = "", $as_Body = "", $s_AttachFiles = "", $s_CcAddress = "", $s_BccAddress = "", $s_Importance="Normal", $s_Username = "", $s_Password = "", $IPPort = 25, $ssl = 0)
    Local $objEmail = ObjCreate("CDO.Message")
    $objEmail.From = '"' & $s_FromName & '" <' & $s_FromAddress & '>'
    $objEmail.To = $s_ToAddress
    Local $i_Error = 0
    Local $i_Error_desciption = ""
    If $s_CcAddress <> "" Then $objEmail.Cc = $s_CcAddress
    If $s_BccAddress <> "" Then $objEmail.Bcc = $s_BccAddress
    $objEmail.Subject = $s_Subject
    If StringInStr($as_Body, "<") And StringInStr($as_Body, ">") Then
        $objEmail.HTMLBody = $as_Body
    Else
        $objEmail.Textbody = $as_Body & @CRLF
    EndIf
    If $s_AttachFiles <> "" Then
        Local $S_Files2Attach = StringSplit($s_AttachFiles, ";")
        For $x = 1 To $S_Files2Attach[0]
            $S_Files2Attach[$x] = _PathFull($S_Files2Attach[$x])
;~          ConsoleWrite('@@ Debug : $S_Files2Attach[$x] = ' & $S_Files2Attach[$x] & @LF & '>Error code: ' & @error & @LF) ;### Debug Console
            If FileExists($S_Files2Attach[$x]) Then
                ConsoleWrite('+> File attachment added: ' & $S_Files2Attach[$x] & @LF)
                $objEmail.AddAttachment($S_Files2Attach[$x])
            Else
                ConsoleWrite('!> File not found to attach: ' & $S_Files2Attach[$x] & @LF)
                SetError(1)
                Return 0
            EndIf
        Next
    EndIf
    $objEmail.Configuration.Fields.Item ("http://schemas.microsoft.com/cdo/configuration/sendusing") = 2
    $objEmail.Configuration.Fields.Item ("http://schemas.microsoft.com/cdo/configuration/smtpserver") = $s_SmtpServer
    If Number($IPPort) = 0 then $IPPort = 25
    $objEmail.Configuration.Fields.Item ("http://schemas.microsoft.com/cdo/configuration/smtpserverport") = $IPPort
    ;Authenticated SMTP
    If $s_Username <> "" Then
        $objEmail.Configuration.Fields.Item ("http://schemas.microsoft.com/cdo/configuration/smtpauthenticate") = 1
        $objEmail.Configuration.Fields.Item ("http://schemas.microsoft.com/cdo/configuration/sendusername") = $s_Username
        $objEmail.Configuration.Fields.Item ("http://schemas.microsoft.com/cdo/configuration/sendpassword") = $s_Password
    EndIf
    If $ssl Then
        $objEmail.Configuration.Fields.Item ("http://schemas.microsoft.com/cdo/configuration/smtpusessl") = True
    EndIf
    ;Update settings
    $objEmail.Configuration.Fields.Update
    ; Set Email Importance
    Switch $s_Importance
        Case "High"
            $objEmail.Fields.Item ("urn:schemas:mailheader:Importance") = "High"
        Case "Normal"
            $objEmail.Fields.Item ("urn:schemas:mailheader:Importance") = "Normal"
        Case "Low"
            $objEmail.Fields.Item ("urn:schemas:mailheader:Importance") = "Low"
    EndSwitch
    $objEmail.Fields.Update
    ; Sent the Message
    $objEmail.Send
    If @error Then
        SetError(2)
        Return $oMyRet[1]
    EndIf
    $objEmail=""
EndFunc   ;==>_INetSmtpMailCom

Func MyErrFunc()
    $HexNumber = Hex($oMyError.number, 8)
    $oMyRet[0] = $HexNumber
    $oMyRet[1] = StringStripWS($oMyError.description, 3)
    ConsoleWrite("### COM Error !  Number: " & $HexNumber & "   ScriptLine: " & $oMyError.scriptline & "   Description:" & $oMyRet[1] & @LF)
    SetError(1); something to check for when this function returns
    Return
EndFunc   ;==>MyErrFunc
;##################################

#endregion

#region UNZIP File

Func _Zip_UnzipAll($sZipFile, $sDestPath, $iFlag = 20)
	If Not _Zip_DllChk() Then Return SetError(@error, 0, 0)
	If Not _IsFullPath($sZipFile) Or Not _IsFullPath($sDestPath) Then Return SetError(3, 0, 0)
	; get temp dir created by Windows
	Local $sTempDir = _Zip_TempDirName($sZipFile)
	Local $oNS = _Zip_GetNameSpace($sZipFile)
	If Not IsObj($oNS) Then Return SetError(4, 0, 0)
	$sDestPath = _Zip_PathStripSlash($sDestPath)
	If Not FileExists($sDestPath) Then
		DirCreate($sDestPath)
		If @error Then Return SetError(5, 0, 0)
	EndIf
	Local $oNS2 = _Zip_GetNameSpace($sDestPath)
	If Not IsObj($oNS2) Then Return SetError(6, 0, 0)
	$oNS2.CopyHere($oNS.Items(), $iFlag)
	; remove temp dir created by WIndows
	DirRemove($sTempDir, 1)
	If FileExists($sDestPath & "\" & $oNS.Items().Item($oNS.Items().Count - 1).Name) Then
		; success... most likely
		; checks for existence of last item from source in destination
		Return 1
	Else
		; failure
		Return SetError(7, 0, 0)
	EndIf
EndFunc   ;==>_Zip_UnzipAll
;-------------------------------------
Func _Zip_DllChk()
	If Not FileExists(@SystemDir & "\zipfldr.dll") Then Return SetError(1, 0, 0)
	If Not RegRead("HKEY_CLASSES_ROOT\CLSID\{E88DCCE0-B7B3-11d1-A9F0-00AA0060FA31}", "") Then Return SetError(2, 0, 0)
	Return 1
EndFunc   ;==>_Zip_DllChk


Func _IsFullPath($sPath)
	If StringInStr($sPath, ":\") Then
		Return True
	Else
		Return False
	EndIf
EndFunc   ;==>_IsFullPath

Func _Zip_TempDirName($sZipFile)
	Local $i = 0, $sTemp, $sName = _Zip_PathNameOnly($sZipFile)
	Do
		$i += 1
		$sTemp = @TempDir & "\Temporary Directory " & $i & " for " & $sName
	Until Not FileExists($sTemp) ; this folder will be created during extraction
	Return $sTemp
EndFunc   ;==>_Zip_TempDirName

Func _Zip_PathNameOnly($sPath)
	Return StringRegExpReplace($sPath, ".*\\", "")
EndFunc   ;==>_Zip_PathNameOnly

Func _Zip_GetNameSpace($sZipFile, $sPath = "")
	If Not _Zip_DllChk() Then Return SetError(@error, 0, 0)
	If Not _IsFullPath($sZipFile) Then Return SetError(3, 0, 0)
	Local $oApp = ObjCreate("Shell.Application")
	Local $oNS = $oApp.NameSpace($sZipFile)
	If Not IsObj($oNS) Then Return SetError(4, 0, 0)
	If $sPath <> "" Then
		; subfolder
		Local $aPath = StringSplit($sPath, "\")
		Local $oItem
		For $i = 1 To $aPath[0]
			$oItem = $oNS.ParseName($aPath[$i])
			If Not IsObj($oItem) Then Return SetError(5, 0, 0)
			$oNS = $oItem.GetFolder
			If Not IsObj($oNS) Then Return SetError(6, 0, 0)
		Next
	EndIf
	Return $oNS
EndFunc   ;==>_Zip_GetNameSpace

Func _Zip_PathStripSlash($sString)
	Return StringRegExpReplace($sString, "(^\\+|\\+$)", "")
EndFunc   ;==>_Zip_PathStripSlash

#endregion

#region Maping and get data from FileChangeDir
 Func GetExcelDataProcess($allFormPath, $incompleteFormPath)
	 
;~  _DebugSetup("_DebugReportVar", True)
;~ 	_DebugReportVar("IN",$incompleteFormPath)
	
;~ 	_DebugSetup ()
;~ 	_DebugReport ($allFormPath)


	Dim $excelAllForm = _ExcelBookAttach($allFormPath)
	Dim $excelInComForm = _ExcelBookAttach($incompleteFormPath)
	Dim $iLine =3, $aLine = 2
	Dim $afullname = 22, $aMobile = 23, $aEmail = 24, $aCity = 41, $aNational = 33, $aStreet = 38, $aWard = 39, $aDistrict = 40
	Dim $ifullname = 12, $iMobile = 14, $iEmail = 13
	Dim $vafullname ,$vaMobile,$vaEmail,$vaCity,$vaNational,$vaStreet, $vaWard, $vaDistrict
	Dim $excel
	Dim $isExist = False
	$viMobile = _ExcelReadCell($excelInComForm, $iLine, $iMobile) 
	$vaMobile = _ExcelReadCell($excelAllForm, $aLine, $aMobile) 
						
	While $viMobile <> ""

			$isExist = False
			$aLine = 2
			$vafullname =""
			$vaEmail =""
			$vaCity =""
			$vaNational =""
			$vaStreet =""
			$vaWard =""
			$vaDistrict =""
			
			if($viMobile <> "") Then
				
				While $vaMobile <> ""	
						
;~ 					Case exist in all form
				 if $viMobile == $vaMobile Then

						$vafullname  = _ExcelReadCell($excelAllForm, $aLine, $afullname)
						$vaMobile = _ExcelReadCell($excelAllForm, $aLine, $aMobile)
						$vaEmail  = _ExcelReadCell($excelAllForm, $aLine, $aEmail)
						$vaCity = _ExcelReadCell($excelAllForm, $aLine, $aCity)
						$vaNational = _ExcelReadCell($excelAllForm, $aLine, $aNational)
						$vaStreet = _ExcelReadCell($excelAllForm, $aLine, $aStreet)
						$vaWard = _ExcelReadCell($excelAllForm, $aLine, $aWard)
						$vaDistrict = _ExcelReadCell($excelAllForm, $aLine, $aDistrict)
						$isExist = True
						ExitLoop
				  Else
					$aLine = $aLine + 1
					$vaMobile = _ExcelReadCell($excelAllForm, $aLine, $aMobile)
				  EndIf
				WEnd
;~ 				Case not exist in all form
				if $isExist == False Then
					$vafullname  = _ExcelReadCell($excelInComForm, $iLine, $ifullname)
					$vaMobile = _ExcelReadCell($excelInComForm, $iLine, $iMobile)
					$vaEmail  = _ExcelReadCell($excelInComForm, $iLine, $iEmail)
						
				EndIf
;~ 				Wrire data to Excell
				Dim $resFileList = _FileListToArray(@ScriptDir &"\Result")
				Dim $FilePath = @ScriptDir & "\Result" & "\ReportCompleteForm.xls"
				Dim $oExcel, $oBook
				
				if $resFileList <> "" Then
;~ 					$excel = _ExcelBookOpen($FilePath)
					_ExcelWriteCell ($excel, $vafullname, $iline -1, 1)
					_ExcelWriteCell ($excel, $vaMobile, $iline -1, 2)
					_ExcelWriteCell ($excel, $vaEmail, $iline -1, 3)
					_ExcelWriteCell ($excel, $vaCity, $iline -1, 4)
					_ExcelWriteCell ($excel, $vaNational, $iline -1, 5)
					_ExcelWriteCell ($excel, $vaStreet, $iline -1, 6)
					_ExcelWriteCell ($excel, $vaWard, $iline -1, 7)
					_ExcelWriteCell ($excel, $vaDistrict, $iline -1, 8)
					_ExcelBookSave($excel)
				Else
					$oExcel = _ExcelBookNew()
					_ExcelBookSaveAs($oExcel, $FilePath)
					_ExcelBookClose ($oExcel)
;~ 					Write Colunm name
					$excel = _ExcelBookOpen($FilePath)	
					_ExcelWriteCell ($excel, "fullname", 1, 1)
					_ExcelWriteCell ($excel, "Mobile", 1, 2)
					_ExcelWriteCell ($excel, "Email", 1, 3)
					_ExcelWriteCell ($excel, "City", 1, 4)
					_ExcelWriteCell ($excel, "National", 1, 5)
					_ExcelWriteCell ($excel, "Street", 1, 6)
					_ExcelWriteCell ($excel, "Ward", 1, 7)
					_ExcelWriteCell ($excel, "District", 1, 8)
;~ 					Write data first row
					_ExcelWriteCell ($excel, $vafullname, $iline -1, 1)
					_ExcelWriteCell ($excel, $vaMobile, $iline -1, 2)
					_ExcelWriteCell ($excel, $vaEmail, $iline -1, 3)
					_ExcelWriteCell ($excel, $vaCity, $iline -1, 4)
					_ExcelWriteCell ($excel, $vaNational, $iline -1, 5)
					_ExcelWriteCell ($excel, $vaStreet, $iline -1, 6)
					_ExcelWriteCell ($excel, $vaWard, $iline -1, 7)
					_ExcelWriteCell ($excel, $vaDistrict, $iline -1, 8)
					_ExcelBookSave($excel)
				EndIf
			EndIf
				
			$iLine = $iLine + 1
			$viMobile = _ExcelReadCell($excelInComForm, $iLine, $iMobile)
	WEnd
		_ExcelBookClose ($excel)	
EndFunc
#endregion