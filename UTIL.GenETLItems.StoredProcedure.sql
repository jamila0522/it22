USE [BIS]
GO
/****** Object:  StoredProcedure [UTIL].[GenETLItems]    Script Date: 8/28/2019 3:44:33 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE  PROC [UTIL].[GenETLItems]
	@ItemID INT = 0,
	@FormCode VARCHAR(255),
	@IsReCreate BIT = 0
AS
BEGIN
	SET NOCOUNT ON 
	SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED
	IF( 
		(NOT EXISTS ( --- itemid
				SELECT * 
				FROM PSYS.ETLConfig c JOIN  PSYS.ETLConfig c1 ON c.ItemID = c1.ItemParent
				WHERE c.ItemID = @ItemID AND C.ItemName ='Form' 
			) OR @IsReCreate = '1' 
		)AND EXISTS ( --- is datalist
			SELECT * 
			FROM PSYS.ETLConfig c 
			WHERE ItemID = @ItemID 
			AND  C.ItemName ='Form' 
		)
	) 
	BEGIN
		DECLARE @COUNTCOL INT  
		DECLARE @SourceInfo VARCHAR(255)
		SELECT @SourceInfo = Value
		FROM PSYS.ETLConfig WHERE FormCode = @FormCode
		AND ItemName = 'Source'
		
		
		SET @COUNTCOL = (SELECT count(S.ORDINAL_POSITION) FROM INFORMATION_SCHEMA.COLUMNS S WHERE @SourceInfo LIKE S.TABLE_SCHEMA + '.' + S.TABLE_NAME)
		
		PRINT 'INSERT DETAILS'
	
		DELETE PSYS.ETLConfig WHERE ItemParent = @ItemID
		
		INSERT INTO PSYS.ETLConfig(
			ItemName, ItemParent, ItemType, ItemStatus,
		    FormCode, UserIDCreated, DomainID, NodeLevel, IsChild, [Value],[Index],
		    Col, [Key], Holder, Display,DisplayVN, DefaultValue, MaxLength,
		    IsRequire, Note, Pattern, [Disable], DataSource, ColCode, ColName,
		    Condition, SourceCondition)
		SELECT  
				S.COLUMN_NAME AS ItemName, 
				@ItemID AS ItemParent, 
				CASE
					WHEN s.DATA_TYPE IN ( 'datetime' ) THEN 'DateTime'
					WHEN s.DATA_TYPE IN ( 'date' ) THEN 'DateTime'
					WHEN s.DATA_TYPE IN ( 'time' ) THEN 'DateTime'
					WHEN s.DATA_TYPE IN ( 'BIT') THEN 'OptionBox2'
					WHEN s.COLUMN_NAME LIKE '%DataLists' THEN 'DataLists'
					WHEN s.COLUMN_NAME LIKE '%CardViews' THEN 'CardViews'
					WHEN s.COLUMN_NAME LIKE '%OptionList' THEN 'OptionList'
					WHEN s.COLUMN_NAME LIKE '%CheckList' THEN 'CheckList'
					WHEN s.COLUMN_NAME LIKE '%Type' THEN 'SelectListAjax'
					WHEN s.COLUMN_NAME LIKE '%List' THEN 'SelectMutiListAjax'
					WHEN s.DATA_TYPE LIKE '%char' AND S.CHARACTER_MAXIMUM_LENGTH = -1 THEN 'HtmlEditor'
					WHEN s.DATA_TYPE LIKE '%char' AND S.CHARACTER_MAXIMUM_LENGTH > 255 THEN 'TextArea'
					WHEN s.DATA_TYPE LIKE '%char' THEN 'TextBox'
					WHEN s.DATA_TYPE IN ('int','TINYINT', 'bigint','float', 'decimal' ) THEN 'TextBox'
				ELSE 'TextBox' END AS ItemType, 
				CASE WHEN s.COLUMN_NAME IN('ID') THEN '0' ELSE '1' END AS ItemStatus,
				@FormCode as FormCode, 
				0 UserIDCreated, 
				0 as DomainID, 
				2 NodeLevel, 
				1 IsChild, 
				NULL [Value],
				ROW_NUMBER() OVER (ORDER BY S.ORDINAL_POSITION ) AS  [Index],
				CASE 
					WHEN @COUNTCOL < 10 THEN 0 
					WHEN S.ORDINAL_POSITION <= @COUNTCOL / 2 THEN '0' ELSE '1' END as Col, 
				S.COLUMN_NAME AS [Key], 
				CASE
					WHEN s.COLUMN_NAME LIKE '%Mobile%' THEN '9999-999-999'
					WHEN s.COLUMN_NAME LIKE '%PRate' THEN N'0 %' 
					WHEN s.COLUMN_NAME = 'Email' THEN N'diachi@email.com' 
					WHEN s.DATA_TYPE IN ( 'int', 'bigint', 'TINYINT' ) THEN '0'
					WHEN s.DATA_TYPE IN ( 'decimal' ) THEN '0.00'
					WHEN s.DATA_TYPE IN ( 'float' ) THEN '0.0000'
					WHEN s.DATA_TYPE IN ( 'date') THEN N'Ngay/Thang/Nam'
					WHEN s.DATA_TYPE IN ( 'time') THEN N'Gio:Phut:Giay'
				ELSE S.COLUMN_NAME END as Holder, 
				CASE WHEN s.DATA_TYPE IN ('BIT') THEN S.COLUMN_NAME + ',Enable,Disable' else  S.COLUMN_NAME end as Display, 
				CASE WHEN s.DATA_TYPE IN ('BIT') THEN S.COLUMN_NAME + ',Enable,Disable' else  S.COLUMN_NAME end as DisplayVN, 
				NULL DefaultValue, 
				CASE WHEN  s.DATA_TYPE LIKE '%char' AND S.CHARACTER_MAXIMUM_LENGTH > 0 THEN  CAST(S.CHARACTER_MAXIMUM_LENGTH AS VARCHAR(20)) ELSE NULL END AS MaxLength,
				CASE WHEN S.IS_NULLABLE = 'NO' THEN '1' ELSE '0' END AS IsRequire, 
				'' Note, 
				
				CASE 
					WHEN s.COLUMN_NAME LIKE '%PassWord%' THEN 'PassWord'
					WHEN s.COLUMN_NAME LIKE '%Mobile%' THEN 'MobileNo'
					WHEN s.COLUMN_NAME LIKE '%email%' THEN 'Email'
					WHEN s.COLUMN_NAME LIKE '%PRate' THEN 'Percent'
					WHEN s.DATA_TYPE IN ( 'datetime' ) THEN 'DateTimePick'
					WHEN s.DATA_TYPE IN ( 'date' ) THEN 'DatePick'
					WHEN s.DATA_TYPE IN ( 'time' ) THEN 'TimePick'
					WHEN s.DATA_TYPE IN ( 'int', 'TINYINT','bigint' ) THEN 'NumberOnly'
					WHEN s.DATA_TYPE IN ( 'decimal' ) THEN 'Number2'
					WHEN s.DATA_TYPE IN ( 'float' ) THEN 'Number4'
				ELSE '' END AS Pattern, 
				CASE WHEN s.COLUMN_NAME = PSYS.ufn_FindDataSource( S.TABLE_NAME,'ID') THEN '1' ELSE '0' END AS [Disable], 
				PSYS.ufn_FindDataSource(s.COLUMN_NAME,'TABLE') AS DataSource, 
				PSYS.ufn_FindDataSource(s.COLUMN_NAME,'CODE') AS ColCode, 
				PSYS.ufn_FindDataSource(s.COLUMN_NAME,'NAME') AS ColName,
				'' AS Condition, 
				'' AS SourceCondition 
		-- SELECT *
		FROM INFORMATION_SCHEMA.COLUMNS S 
		WHERE @SourceInfo LIKE S.TABLE_SCHEMA + '.' + S.TABLE_NAME
		AND S.COLUMN_NAME NOT IN (
			'UserID', 'ID', 'UserIDCreated', 'DateTimeCreated', 'UserIDUpdated', 'DateTimeUpdated', 
			'DomainID', 'NodeLevel', 'IsChild', 'Ordinal'
		)
		ORDER BY S.ORDINAL_POSITION
		
	END 


END


/*
		
*/
GO
