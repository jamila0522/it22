USE [BIS]
GO
/****** Object:  StoredProcedure [PSYS].[FormConfig_GetRpt]    Script Date: 8/28/2019 3:44:33 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROC [PSYS].[FormConfig_GetRpt]
	@DomainID INT =0,
	@SSID VARCHAR(50) = '',
	@ACTION VARCHAR(50) = '',
	@OBJECTID VARCHAR(50) = '',
	@UserID INT, 
	@NodeID INT = 0,
	@IsReCreate BIT = 0, 
	@SourceInfo NVARCHAR(255) = '',
	@FormCode VARCHAR(50) = '',
	@FormType VARCHAR(20) = ''
AS
BEGIN

	SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED
	DECLARE @SourceType VARCHAR(50)
	DECLARE @FormID INT 
	SET @SourceInfo = REPLACE(REPLACE(@SourceInfo,'[',''), ']','')
	IF @FormCode = '' AND @SourceInfo <> ''
	BEGIN
		PRINT 'GET FORMCODE'
		--------
		SELECT @FormID = formid 
		FROM PSYS.FormCode 
		WHERE SourceInfo = @SourceInfo
		--------
		---- DELETE  PSYS.FormCode
		IF @FormID IS NULL
		INSERT INTO PSYS.FormCode(SourceInfo)
		SELECT @SourceInfo 
	
		SELECT @FormID = formid , @SourceType =  FormCodeType
		FROM PSYS.FormCode 
		WHERE SourceInfo = @SourceInfo
		
		SET @FormCode  ='RPT-' + RIGHT(CAST(@FormID + 10000 AS VARCHAR(20)),4)
		
		PRINT @FormCode
	END
	ELSE IF @FormCode <> '' AND @SourceInfo = ''
	SELECT @SourceInfo = REPLACE(REPLACE(SourceInfo,'[',''), ']',''), @SourceType =  FormCodeType
	FROM PSYS.FormCode  
	WHERE cast(right(@FormCode,4) AS INT) = FormID
	
	IF @SourceType = 'TableDimType'
	SET @SourceInfo = @SourceInfo + '_GetList'
	PRINT 'SOURCE INFOR'
	PRINT @SourceInfo
	
	IF NOT EXISTS (SELECT * FROM PSYS.FormConfig WHERE FormCode = @FormCode )
	SET @IsReCreate = 1
	
	IF @SourceInfo <> '' AND @IsReCreate = 1 AND @FormCode <> ''
	BEGIN
		PRINT 'DELETE'
		DELETE FROM PSYS.FormConfig WHERE FormCode = @FormCode
		
		PRINT 'INSERT HEADER'
		INSERT INTO PSYS.FormConfig(
			ItemName, ItemParent, ItemType, ItemStatus,
		    FormCode, UserIDCreated, DomainID, NodeLevel, IsChild, [Value],[Key]
		)
		SELECT ItemName, ItemParent, ItemType, ItemStatus,
		       @FormCode AS FormCode, UserIDCreated, DomainID, NodeLevel, IsChild, 
		       CASE 
					WHEN t.ItemName = 'ObjectID' THEN cast(NEWID() AS VARCHAR(50)) 
					WHEN t.ItemName = 'FormCode' THEN @FormCode 
					WHEN t.ItemName = 'Source' THEN @SourceInfo 
					WHEN t.ItemName = 'Target' THEN NULL 
					WHEN t.ItemName = 'ColID' THEN NULL
					WHEN t.ItemName = 'Layout' THEN 'ReportTopDetail'
					WHEN t.ItemName = 'Title' THEN N'Báo cáo' + @FormCode
					WHEN t.ItemName = 'Report' THEN @SourceInfo
					WHEN t.ItemName = 'FixRequestID' THEN '1'
					WHEN t.ItemName = 'PublicRequest' THEN '0'
					WHEN t.ItemName = 'SourceType' THEN 'Table'
		       ELSE NULL END AS [Value], t.ItemName
		FROM PSYS.FormConfigTemplate t
		
		UPDATE PSYS.FormConfig
		SET ItemParent = (
			SELECT ItemID 
			FROM PSYS.FormConfig 
			WHERE FormCode = @FormCode AND ItemName = 'FormHeader'
		) 
		WHERE FormCode = @FormCode 
		AND ItemParent = 1
		
		UPDATE PSYS.FormConfig
		SET ItemParent = (
			SELECT ItemID 
			FROM PSYS.FormConfig 
			WHERE FormCode = @FormCode 
			AND ItemName = 'Layout'
		) 
		WHERE FormCode = @FormCode 
		AND ItemParent = 60
		
		UPDATE PSYS.FormConfig
		SET ItemParent = (
			SELECT ItemID 
			FROM PSYS.FormConfig 
			WHERE FormCode = @FormCode 
			AND ItemName = 'Process'
		) 
		WHERE FormCode = @FormCode 
		AND ItemParent = 1001
		
		UPDATE PSYS.FormConfig
		SET ItemParent = (
			SELECT ItemID 
			FROM PSYS.FormConfig 
			WHERE FormCode = @FormCode 
			AND ItemName = 'TabList'
		) 
		WHERE FormCode = @FormCode 
		AND ItemParent = 401
		UPDATE PSYS.FormConfig
		SET ItemParent = (
			SELECT ItemID 
			FROM PSYS.FormConfig 
			WHERE FormCode = @FormCode 
			AND ItemName = 'ETL'
		) 
		WHERE FormCode = @FormCode 
		AND ItemParent = 2001
		
		PRINT 'FROMS ITEMS'
		DECLARE @ItemID INT 
		SELECT @ItemID = ItemID FROM PSYS.FormConfig 
		WHERE FormCode = @FormCode AND ItemName = 'Form'
		
		
		PRINT 'FROM ITEM'	
		EXEC [UTIL].[GenReportForm] @ItemID, @FormCode,@IsReCreate
		
		PRINT 'REPORT ITEM'
		SELECT @ItemID = ItemID 
		FROM PSYS.FormConfig 
		WHERE FormCode = @FormCode 
		AND ItemName = 'Report'
		EXEC [UTIL].[GenReportItem] @ItemID, @FormCode,@IsReCreate
				
	END
	
	SELECT 
		f.ItemID AS ID, 
		f.ItemCode AS Code, 
		f.ItemName AS Name, 
		f.ItemStatus AS [Status],
		f.ItemType AS [Type],
	    f.ItemParent AS  ParentID,   
	    CAST(1 AS BIT) IsOpen,
	    CAST(0 AS BIT) IsCheck,
	    cast(@USERID as varchar(255)) AS ObjectCall, f.*
	INTO #tmp_Fillter
	FROM PSYS.FormConfig f
	WHERE (ISNULL(@NodeID,0) = 0 or ItemID = @NodeID)
	AND formcode = @FormCode
	DECLARE @ParentID_f INT 
	
	update #tmp_Fillter SET StepAction = NULL, StepControl = NULL, StepOption = NULL
	
	exec PSYS.FormConfig_GetData @UserID,@ACTION 
	
END
/*
	
			
	[PSYS].[FormConfig_GetRPT]  
		@USERID = 1, 
		@ACTION = 'Report', 
		@DomainID = 1, 
		@SourceInfo ='',
		@FormCode='RPT-0100',
		@IsReCreate = 1,
		@FormType = 'RPT'
	
	
	
 
 */

GO
