#include <GUIConstantsEx.au3>
#include <WindowsConstants.au3>
#Include <IE.au3>
#include <Array.au3>
#include <Debug.au3>

Dim $oIE
Dim $iCard
Dim $iTotalCard
Dim $CardTypeValue
;_DebugSetup("_DebugReportVar", True)

Opt("SendAttachMode", 1)
GUICreate("Card Creation Tool", 300, 220)
$iCard = 1

;~ card type
GUICtrlCreateLabel("Please choose card type to issue:", 20, 20)
$CardType = GUICtrlCreateCombo("48354200 - Visa Debit Card", 20, 40, 260)
GUICtrlSetData($CardType, "45732800 - Platinum Card|97041000 - Smartlink Card|48354288 - Priority Platinum Card|45858400 - Priority Card|48354201 - Visa Debit Card")

;~ Number of card
GUICtrlCreateLabel("Number of card to be issued:", 20, 82)
$Numbercard = GUICtrlCreateInput("", 180, 80, 100)
GUICtrlSetData($Numbercard, 0)

;~ Running card
$LABLE = GUICtrlCreateLabel("Processed: 0", 20, 122, 160)

;~ BUTTON
$Reset_btn = GUICtrlCreateButton("Reset", 180, 120, 100, 17)
$Proc_btn = GUICtrlCreateButton("Process", 20, 170, 125, 30)
$Cancel_btn = GUICtrlCreateButton("Cancel", 155, 170, 125, 30)

GUISetState()      ; will display an  dialog box with 2 button

; Run the GUI until the dialog is closed
While 1
	$msg = GUIGetMsg()
	Select
		Case $msg = $GUI_EVENT_CLOSE
			ExitLoop
		Case $msg = $Cancel_btn
			GUIDelete()
		Case $msg = $Reset_btn
			$iCard = 1
			GUICtrlSetState($Numbercard, $GUI_ENABLE)
			GUICtrlSetState($CardType, $GUI_ENABLE)
			GUICtrlSetState($Proc_btn, $GUI_ENABLE)
			GUICtrlSetData($LABLE, "Processed: 0")
		Case $msg = $Proc_btn
			$iTotalCard = GUICtrlRead($Numbercard)
			$CardTypeValue = StringLeft(GUICtrlRead($CardType),8)
			if $iTotalCard > 0 Then
				GUICtrlSetState($CardType, $GUI_DISABLE)
				GUICtrlSetState($Numbercard, $GUI_DISABLE)
				GUICtrlSetState($Proc_btn, $GUI_DISABLE)
				$oIE = _IEAttach("Card Management System UI")
				_IEPropertySet($oIE, "silent", 1)
				For $iCard = $iCard to $iTotalCard
					Call("CardIssuance",$oIE, $iCard, $CardTypeValue)
				Next
				MsgBox(0,"Complete","DONE!")
			Else
				MsgBox(0,"Warning","Number of card to be issued must be larger than 0")
			EndIf
	EndSelect
WEnd

Func CardIssuance($oIE, $iCard, $CardTypeValue)
	
	If mod(@MIN, 5) = 0 Then
		RunWait(@COMSPEC & " /c Dir C:\")
	EndIf
	
	;add client
	_IELinkClickByText ($oIE, "Clients")
	_IELinkClickByText ($oIE, "Add Client")
	_IELoadWait($oIE)
	Sleep(100)
	
	
	;set client info
	If WinExists("Card Management System UI - Add Client","") Then
		$oForm = _IEFormGetCollection ($oIE, 1)
		$oQuery = _IEFormElementGetCollection ($oForm,2)

		_IEFormElementSetValue($oQuery, @YEAR & @MON & @MDAY & @HOUR & @MIN & @MSEC)
		$oQuery = _IEFormElementGetCollection ($oForm,3)
		_IEFormElementSetValue($oQuery, "CARD HOLDER")
		$oQuery = _IEFormElementGetCollection ($oForm,6)
		_IEFormElementSetValue($oQuery, "CARD HOLDER")

		;save client info
		;$oQuery = _IEFormElementGetCollection ($oForm,34)	old
		$oQuery = _IEFormElementGetObjByName ($oForm,"notexpired:form1:btnSave")
		
		_IEAction($oQuery,"click")
		_IELoadWait($oIE)
		Sleep(100)
		
		;add service
		_IELinkClickByText ($oIE, "Add Service")
		_IELoadWait($oIE)
		Sleep(100)
		
		If WinExists("Card Management System UI - Add Services","") Then
			;choose debit card
			;$oForm = _IEFormGetCollection ($oIE,4)
			$oForm = _IEFormGetObjByName($oIE, "notexpired:form1")
			
			;$oQuery = _IEFormElementGetCollection($oForm, 0)
			;_IEFormElementSetValue($oQuery, "true")
			;_IEAction($oQuery, "click")
			;_IELoadWait($oIE)
			
			BlockInput(1)
			
			MouseClick("left",947,432,2)
			Send("{TAB}")
			Send("{SPACE}")
			
			BlockInput(0)
			;MouseClick("left",508,368,2)

			;Click button Add service
			$oQuery = _IEFormElementGetObjByName($oForm,"notexpired:form1:button1")
			_IEAction($oQuery, "click")
			_IELoadWait($oIE)

			Sleep(100)
			
			If WinExists("Card Management System UI - Add Debit Card","") Then
				;choose card type
				;$oForm = _IEFormGetCollection ($oIE, 4)
				;$oQuery = _IEFormElementGetCollection($oForm,0)
				$oForm = _IEGetObjByName($oIE,"notexpired:addCard")
				$oQuery = _IEFormElementGetObjByName($oForm,"notexpired:addCard:cardType")
				_IELoadWait($oIE)
				Sleep(100)
				;choose visa debit
				Select
					Case $CardTypeValue = "48354200"
						_IEFormElementOptionSelect($oQuery, 2, 1, "byIndex")
					Case $CardTypeValue = "45732800"
						_IEFormElementOptionSelect($oQuery, 5, 1, "byIndex")
					Case $CardTypeValue = "97041000"
						_IEFormElementOptionSelect($oQuery, 0, 1, "byIndex")
					Case $CardTypeValue = "48354288"
						_IEFormElementOptionSelect($oQuery, 6, 1, "byIndex")
					Case $CardTypeValue = "45858400"
						_IEFormElementOptionSelect($oQuery, 4, 1, "byIndex")
					Case $CardTypeValue = "48354201"
						_IEFormElementOptionSelect($oQuery, 3, 1, "byIndex")
				EndSelect
				;$oQuery = _IEFormElementGetCollection($oForm,11)
				$oQuery = _IEFormElementGetObjByName($oForm,"notexpired:addCard:btnContinue")
				_IEAction($oQuery,"click")
				_IELoadWait($oIE)
				Sleep(100)
				;_DebugReportVar("Test","ok")
				GUICtrlSetData($LABLE, "Processed: " & $iCard)
				
				;add account to card
				
				if $CardTypeValue = "48354288" Then
					$oForm = _IEFormGetCollection ($oIE, 4)
					$oQuery = _IEFormElementGetCollection($oForm,11)
					_IEFormElementSetValue($oQuery,"False")
				EndIf
				$oForm = _IEFormGetCollection ($oIE, 4)
				;$oQuery = _IEFormElementGetCollection($oForm,14)
				;-------click Add account------------
				$oQuery = _IEFormElementGetObjByName($oForm,"notexpired:form1:btnAddAccount")
				_IEAction($oQuery,"click")
				_IELoadWait($oIE)
				Sleep(100)
				
				;input account Number
				$oForm = _IEFormGetCollection ($oIE, 4)
				;Set account number
				;$oQuery = _IEFormElementGetCollection ($oForm,1)
				$oQuery = _IEFormElementGetObjByName($oForm,"notexpired:account:txtCAACCT")
				_IEFormElementSetValue($oQuery, StringRight("00000" & $iCard,5) & @YEAR & @MON & @MDAY & @HOUR & @MIN & @MSEC)
				_IELoadWait($oIE)
				Sleep(100)
				
				;Set primary
				$oQuery = _IEFormElementGetCollection ($oForm,6)
				_IEAction($oQuery,"click")
				_IELoadWait($oIE)
				Sleep(100)
				
				;Set unspecify
				$oQuery = _IEFormElementGetCollection ($oForm,7)
				_IEAction($oQuery,"click")
				_IELoadWait($oIE)
				Sleep(100)
				
				;Save account
				;$oQuery = _IEFormElementGetCollection ($oForm,11)
				$oQuery = _IEFormElementGetObjByName($oForm,"notexpired:account:saveAcct")
				_IEAction($oQuery,"click")
				_IELoadWait($oIE)
				Sleep(100)

				;save card
				$oForm = _IEFormGetCollection ($oIE, 4)
				;$oQuery = _IEFormElementGetCollection($oForm,17)
				$oQuery = _IEFormElementGetObjByName($oForm,"notexpired:form1:btnSaveCard")
				_IEAction($oQuery,"click")
				_IELoadWait($oIE)
				Sleep(100)
			EndIf
		EndIf
	EndIf
EndFunc
