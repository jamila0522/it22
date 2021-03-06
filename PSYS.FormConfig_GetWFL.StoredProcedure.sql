USE [BIS]
GO
/****** Object:  StoredProcedure [PSYS].[FormConfig_GetWFL]    Script Date: 8/28/2019 3:44:33 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROC [PSYS].[FormConfig_GetWFL]
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
	DECLARE @FormID INT 
	
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
		
		SELECT @FormID = formid 
		FROM PSYS.FormCode 
		WHERE SourceInfo = @SourceInfo
		
		SET @FormCode  ='WFL-' + RIGHT(CAST(@FormID + 10000 AS VARCHAR(20)),4)
		
		PRINT @FormCode
	END
	ELSE IF @FormCode <> '' AND @SourceInfo = ''
	SELECT @SourceInfo = SourceInfo
	FROM PSYS.FormCode  
	WHERE cast(right(@FormCode,4) AS INT) = FormID
	PRINT 'SOURCE INFOR'
	
	DECLARE @TableName NVARCHAR(255)
	DECLARE @Schema NVARCHAR(255)
	
	PRINT @SourceInfo
	PRINT @FormCode
	IF NOT EXISTS (SELECT * FROM PSYS.FormConfig WHERE FormCode = @FormCode )
	SET @IsReCreate = 1
	
	IF @SourceInfo <> '' AND @IsReCreate = 1 AND @FormCode <> ''
	BEGIN
		SELECT @TableName = t.TABLE_NAME, @Schema = t.TABLE_SCHEMA
		FROM INFORMATION_SCHEMA.TABLES AS t
		WHERE t.TABLE_SCHEMA + '.' + t.TABLE_NAME = @SourceInfo
		
		PRINT 'DELETE'
		DELETE FROM PSYS.FormConfig WHERE FormCode = @FormCode
		PRINT 'INSERT HEADER'
		INSERT INTO PSYS.FormConfig(
			ItemName, ItemParent, ItemType, ItemStatus,
		    FormCode, UserIDCreated, DomainID, NodeLevel, IsChild, [Value], [Key]
		)
		SELECT ItemName, ItemParent, ItemType, ItemStatus,
		       @FormCode AS FormCode, UserIDCreated, DomainID, NodeLevel, IsChild, 
		       CASE 
					WHEN t.ItemName = 'ObjectID' THEN NEWID() 
					WHEN t.ItemName = 'FormCode' THEN @FormCode 
					WHEN t.ItemName = 'Source' THEN @SourceInfo 
					WHEN t.ItemName = 'Target' THEN @SourceInfo + '_Save' 
					WHEN t.ItemName = 'ColID' THEN [PSYS].[ufn_FindDataSource](@SourceInfo, 'ID')
					WHEN t.ItemName = 'Layout' THEN 'ProcessDocumentCreate'
					WHEN t.ItemName = 'Title' THEN 'Titles'
					WHEN t.ItemName = 'FixRequestID' THEN '1'
					WHEN t.ItemName = 'PublicRequest' THEN '0'
					WHEN t.ItemName = 'SourceType' THEN 'Table'
		       ELSE NULL END AS [Value], t.ItemName
		FROM PSYS.FormConfigTemplate t
		UNION ALL
		SELECT 'Process',0 ItemParent, 'FormHeader' as ItemType, 1 ItemStatus,
		       @FormCode AS FormCode,  1 UserIDCreated, 1 DomainID, 1 NodeLevel, 0 IsChild, 
		       @SourceInfo AS [Value],'Process'
	
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
	
		exec UTIL.[GenWFLItems] @ItemID,@FormCode,@IsReCreate
		
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
	WHERE formcode = @FormCode
	AND (isnull(@NodeID,0) = 0 OR @NodeID = ItemID)
	
	DECLARE @ParentID_f INT 
	
	IF @ACTION <> 'Process' update #tmp_Fillter SET StepAction = NULL, StepControl = NULL, StepOption = NULL
	
	exec PSYS.FormConfig_GetData @UserID,@ACTION 
	
END
/*
	
	[PSYS].[FormConfig_GetList]  
		@USERID = 1, 
		@ACTION = 'FormHeader', 
		@DomainID = 1, 
		@SourceInfo ='',
		@FormCode='FAC-0105',
		@IsReCreate = 0,
		@FormType='CATE'
		
	[PSYS].[FormConfig_GetList]  
		@USERID = 1, 
		@ACTION = 'Form', 
		@DomainID = 1, 
		@SourceInfo ='',
		@FormCode='FAC-1000',
		@IsReCreate = 1
		
	[PSYS].[FormConfig_GetList]  
		@USERID = 1, 
		@ACTION = 'Layout', 
		@DomainID = 1, 
		@SourceInfo ='',
		@FormCode='FAC-0105',
		@IsReCreate = 0,
		@FormType = 'CATE'
	
	PSYS.FormConfig_GetList @SSID='',@ACTION ='DataLists',@OBJECTID='',@USERID=1,@DomainID=1,@NodeID=2045,@FormCode='FAC-1000'
	
		select *  
		FROM PSYS.FormConfig f
		WHERE (ISNULL(2045,0) = 0 or ItemID = 2045)
		AND formcode = 'FAC-1000'
	
	
			
	[PSYS].[FormConfig_GetList]  
		@USERID = 1, 
		@ACTION = 'DataLists', 
		@DomainID = 1, 
		@SourceInfo ='',
		@FormCode='FAC-1000',
		@IsReCreate = 0
		
		
	SELECT * FROM PSYS.FormConfig where formcode = 'FAC-1007'
	
	PSYS.FormConfig_GetLisT @ACTION = 'Item', @USERID = 1,@DomainID =1, @FormCode = 'FAC-1007'
 
 */

GO
