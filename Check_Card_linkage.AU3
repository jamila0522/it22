#Include <IE.au3>
#Include <Excel.au3>
#include <Array.au3>
#include <Debug.au3>

	Dim $accountNo 
	Dim $cardNumber
	Dim $id
	Dim $name
	Dim $address
	Dim $Phone
	Dim $sFilePath, $excel
	Dim $iline
	Dim $CIFKey
	Dim $vAccountNumber
	Dim $vPhoneNumber
	Dim $vCardNumber
	Dim $cardStatus1
	Dim $cardStatus2
	
	Opt("SendAttachMode", 1)
	
	$sFilePath = @ScriptDir & "\data2check.xls"
	$excel = _ExcelBookOpen($sFilePath)
	
	NormarlizeData($sFilePath, $excel)
;~     _DebugSetup("_DebugReportVar", True)
	$iline = 2
	WinActivate("Card Management System UI")
	Do
		
		$cardNumber = _ExcelReadCell($excel, $iline, 1)
		if ($cardNumber <> "") Then
			_Checkcard($cardNumber, $iline, $excel)
		EndIf
		
		$iline = $iline + 1
	Until $cardNumber = ""
	_ExcelBookSave($excel)
	_ExcelBookClose($excel)
	
Func NormarlizeData($Excelfile, $excel)
	Dim $i, $j, $k, $c, $tempNo
	$i = 2
	Do
		For $j = 1 to 2
			$tempNo = ""
			For $k = 1 to StringLen(_ExcelReadCell($excel, $i, $j))		
				$c = StringMid(_ExcelReadCell($excel, $i, $j), $k, 1)
				if Asc($c) >= 48 and Asc($c) <= 57 Then			
					$tempNo = $tempNo & $c			
				EndIf
			Next
			_ExcelWriteCell($excel, "'"& $tempNo, $i, $j)
		Next
		$vAddress = StringLeft(_ExcelReadCell($excel, $i, 6),50)
		_ExcelWriteCell($excel, $vAddress, $i, 6)
		$i = $i + 1	
	Until _ExcelReadCell($excel, $i, 1) = ""
	

	_ExcelBookSave($excel, 0)
EndFunc

Func _Checkcard(ByRef $cardNumber, $iline, $excel)
	Dim $oIE
	$oIE = _IEAttach("Card Management System UI"); - Main Client Search")
	_IEPropertySet($oIE, "silent", 1)
;#cs ------ view card --------
	_IELinkClickByText ($oIE, "Clients")
	_IELoadWait($oIE)

	
	$oForm = _IEFormGetCollection ($oIE, 1)
	$oForm =_IEGetObjById ($oIE, "notexpired:search")
	$oQuery = _IEGetObjById($oForm, "notexpired:search:searchName")
	_IEPropertySet ($oQuery, "innertext", $cardNumber)
	_IELinkClickByText ($oIE, "Card Number")
	

	_IELoadWait($oIE)
	Sleep(100)

	$oTables = _IETableGetCollection ($oIE)
	
	for $oTable in $oTables
		if StringInStr($oTable.ID,"notexpired:jtpl_id438:tableExClientAccounts") > 0 Then  ;tableExClientAccounts
			$aTableData = _IETableWriteToArray ($oTable)
;~ 			_ArrayDisplay($aTableData)
			IF UBound($aTableData, 2) > 1 Then
				$vAccountNumber = "'" & StringRight($aTableData[1][1],11)
				
			Else
				$vAccountNumber = "NO ACCOUNT"
			EndIf
		EndIf	
			
		if StringInStr($oTable.ID,"notexpired:jtpl_id448:tableExCardList") > 0 Then  ;   tableExCardList
			$aTableData = _IETableWriteToArray ($oTable)
			IF UBound($aTableData, 2) > 1 Then
;~ 			_ArrayDisplay($aTableData)
			$vCardNumber = "'" & $aTableData[2][1]
			Else
				$vCardNumber ="No CardNumber"
			EndIf
		EndIf
		 	
		 	
		if StringInStr($oTable.ID,"notexpired:jsf_id116:tableEx2") > 0 Then ;  tableEx2
			$aTableData = _IETableWriteToArray ($oTable)
			IF UBound($aTableData, 2) > 1 Then
;~ 				_ArrayDisplay($aTableData)
				$vPhoneNumber = "'" & $aTableData[1][1]
			Else
				$vPhoneNumber = "NO PhoneNumber"
			EndIf
		EndIf
		
;~ 		_DebugReportVar("$vAccountNumber", $vAccountNumber)
;~ 		_DebugReportVar("$vCardNumber", $vCardNumber)
;~ 		_DebugReportVar("$vPhoneNumber", $vPhoneNumber)
	Next

;	_ArrayDisplay($aTableData)

	
	
	$oTable = _IETableGetCollection ($oIE,3)
	$aTableData = _IETableWriteToArray ($oTable)
;~ 	_ArrayDisplay($aTableData)
	$vCustomerName = StringStripWS($aTableData[2][0],3)
	$vCustomerID = "'" & StringStripWS($aTableData[2][1],3)

	$CIFKey =  "'" & StringStripWS($aTableData[2][2],3)
	;_DebugReportVar("CIFKey", $CIFKey)
	
;~ 	Update 2019.09.19
	_IELinkClickByText ($oIE, $cardNumber)
	$oForm = _IEGetObjById($oIE, "notexpired:cards")
	
    $cardStatus1 = _IEGetObjById($oForm,"notexpired:cards:jsf_id135")
 if @error Then
	$cardStatus1 = _IEGetObjById($oForm,"notexpired:cards:jsf_id133") 
Else
	$cardStatus1 = _IEGetObjById($oForm,"notexpired:cards:jsf_id135")
EndIf

;~ MsgBox( 0, "Status1", $cardStatus1.innertext)

   $cardStatus2 = _IEGetObjById($oForm,"notexpired:cards:text8")

	
	_ExcelWriteCell ($excel, $vCardNumber,$iline, 8)
	_ExcelWriteCell ($excel, $vAccountNumber,$iline, 9)
	_ExcelWriteCell ($excel, $vCustomerName,$iline, 10)
	_ExcelWriteCell ($excel, $vCustomerID,$iline, 11)
	_ExcelWriteCell ($excel, $vPhoneNumber,$iline, 12)
	_ExcelWriteCell ($excel, $CIFKey,$iline, 14)
	_ExcelWriteCell ($excel, $cardStatus1.innertext,$iline, 15)
	_ExcelWriteCell ($excel, $cardStatus2.innertext,$iline, 16)
	
	;_ExcelWriteFormula($excel, $iline, 7, "=IF(AND(A" & $iline & "=H" & $iline & ",B" & $iline & "=I" & $iline & ",C" & $iline & "=J" & $iline & ",D" & $iline & "=K" & $iline & ",E" & $iline & "=L" & $iline & ",F" & $iline & "=M" & $iline & "),0,1)")
	

EndFunc