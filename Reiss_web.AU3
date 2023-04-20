#Include <IE.au3>
#Include <Excel.au3>
#include <Array.au3>
#include <Debug.au3>

Dim $oIE
Dim $cardNumber
Dim $sFilePath, $excel

Opt("SendAttachMode", 1)

$sFilePath = @ScriptDir & "\Working.xlsm"
$excel = _ExcelBookAttach($sFilePath)
;~ _DebugSetup("_DebugReportVar", True)
if WinExists("Card Management System UI") <> 1 Then
	$oIE = _IECreate("https://10.142.16.15/ITMUI/login.jsf")
	MsgBox(0,"Warning", "Please login to WebCMS first!")
	
	Do
		Sleep(100)
		WinActivate("Card Management System UI")
	until WinExists("Card Management System UI - Welcome -")
Else
	$oIE = _IEAttach("Card Management System UI")
	_IELinkClickByText ($oIE, "Clients")
	_IELoadWait($oIE)
	$oClickResult = _IELinkClickByText($oIE,"Click here to return to the login screen.")
	if $oClickResult = -1 Then
		WinActivate("Card Management System UI")
		MsgBox(0,"Warning", "Please login to WebCMS first!")
		Do
			Sleep(100)
		until WinExists("Card Management System UI - Welcome -")		
	EndIf
EndIf

$iline = 3
WinActivate("Card Management System UI")

Do
	$cardNumber = _ExcelReadCell($excel, $iline, 3)
	if ($cardNumber <> "")  Then
		
		$vTmp = Check_Status($cardNumber, $oIE)
		Sleep(500)
;~ 		_DebugReportVar("Test", $vTmp)
		
		if $vTmp = "No" Then Set_status($cardNumber, $oIE)
		Sleep(500)
	EndIf
	$iline = $iline + 1
Until $cardNumber = ""
Change_mode_to_inquiry($oIE)
	

Func Change_mode_to_inquiry($oIE)
	Dim $oForm, $oQuery
	_IEPropertySet($oIE, "silent", 1)

	$oForm = _IEFormGetCollection ($oIE, 0)
	$oQuery = _IEGetObjById($oForm, "notexpired:editMode:modeToggle")
	
	_IEFormElementOptionSelect ($oQuery, 0, 1, "byIndex")
	_IELoadWait($oIE)
	Sleep(100)
;~ 	MsgBox(0,"Complete","Done")
EndFunc

Func Set_status(ByRef $cardNumber, $oIE)
	Dim $oForm, $oQuery
;~ 	_DebugReportVar("Test", $cardNumber)

	_IEPropertySet($oIE, "silent", 1)
	
	$oForm = _IEFormGetCollection ($oIE, 0)
;~ 	$oQuery = _IEFormElementGetCollection ($oForm,1)
	$oQuery = _IEGetObjById($oForm, "notexpired:editMode:modeToggle")
;~ 	MsgBox(true,"Test",$oQuery)

	_IEFormElementOptionSelect ($oQuery, 1, 1, "byIndex")
	_IELoadWait($oIE)
	Sleep(200)
	
	$oForm = _IEFormGetCollection ($oIE,4)
	$oQuery = _IEFormElementGetObjByName($oForm, "notexpired:form1:checkbox3")
	_IEAction($oQuery,"click")
	_IELoadWait($oIE)
	
	$oQuery = _IEFormElementGetObjByName($oForm, "notexpired:form1:btnOK")
	_IEAction($oQuery,"click")
	_IELoadWait($oIE)
    Change_mode_to_inquiry($oIE)
	_IELoadWait($oIE)
	Sleep(100)
EndFunc

Func Check_Status(ByRef $cardNumber, $oIE)
	Dim $oForm, $oQuery, $oCardDiv
	
	_IEPropertySet($oIE, "silent", 1)

	_IELinkClickByText ($oIE, "Clients")
	_IELoadWait($oIE)
	Sleep(100)
	
	$oForm =_IEGetObjById ($oIE, "notexpired:search")
	$oQuery = _IEGetObjById($oForm, "notexpired:search:searchName")
	_IEPropertySet ($oQuery, "innertext", $cardNumber)
	_IELinkClickByText ($oIE, "Card Number")
	_IELoadWait($oIE)
	Sleep(100)
	
	;SELECT Inquiry
	$oForm = _IEFormGetCollection ($oIE, 0)
	$oQuery = _IEFormElementGetCollection ($oForm,1)
	_IEFormElementOptionSelect ($oQuery, 0,1, "byIndex")
	_IELoadWait($oIE)
	Sleep(500)	
	_IELoadWait($oIE)
	
	
	_IELinkClickByText ($oIE, $cardNumber)
	_IELoadWait($oIE)
	Sleep(1000)
	
	_IELinkClickByText ($oIE, "PIN Information")
	_IELoadWait($oIE)	
	Sleep(100)
		
;~ 	MouseMove(100,100,1)
;~ 	MouseMove(500,500,1)

	$oTable = _IETableGetCollection ($oIE,10)
	$aTableData = _IETableWriteToArray ($oTable)
;~  	_ArrayDisplay($aTableData) ;rem
	Return StringStripWS($aTableData[2][18],8)
	Sleep(100)
EndFunc