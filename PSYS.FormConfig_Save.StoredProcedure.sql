USE [BIS]
GO
/****** Object:  StoredProcedure [PSYS].[FormConfig_Save]    Script Date: 8/28/2019 3:44:33 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROC [PSYS].[FormConfig_Save]
	@DomainID INT =0,
	@SSID VARCHAR(50) = '',
	@ACTION VARCHAR(50) = '', 
	@OBJECTID VARCHAR(50) = '',
	@USERID INT,
	@ItemID INT = 0,
	@ItemParent INT = 0,
	@ItemName NVARCHAR(255) = '', 
	@ItemType NVARCHAR(255) = '', 
	@ItemStatus VARCHAR(255),
	@NodeID INT ,
	@ParentID INT,
	@FormCode VARCHAR(255),
	@Value NVARCHAR(255) = '', 
	-----textbox
	@Index NVARCHAR(255) = '',
	@Col NVARCHAR(255) = '',
	@Key NVARCHAR(255) = '',
	@Type NVARCHAR(255) = '',
	@Holder NVARCHAR(255) = '',
	@Display NVARCHAR(255) = '',
	@DisplayVN NVARCHAR(255) = '',
	@OptionConfig NVARCHAR(4000) = '',
	@DefaultValue  NVARCHAR(255) = '',
	@MaxLength  NVARCHAR(255) = '',
	@IsRequire  NVARCHAR(255) = '',
	@Note  NVARCHAR(255) = '',
	@Pattern  NVARCHAR(255) = '',
	@Disable  NVARCHAR(255) = '',
	@DataSource NVARCHAR(255) = '',
	@ColCode NVARCHAR(255) = '',
	@ColName NVARCHAR(255) = '',
	@Condition NVARCHAR(255) = '',
	@SourceCondition NVARCHAR(255) = '',
	@RowNum  NVARCHAR(255) = '',
	@IsReCreate  NVARCHAR(255) = '',
	@FormType VARCHAR(255) = '',
	@StepControl NVARCHAR(MAX) = '',
	@StepAction NVARCHAR(MAX) = '',
	@ObjectInCharge NVARCHAR(MAX) = '', 
	@StepOption NVARCHAR(MAX) = '',
	@TitleIcon NVARCHAR(255) = '', 
	@TitleConfig NVARCHAR(255) = '', 
	@Scripts NVARCHAR(MAX) = '',
	@TabKey NVARCHAR(255) = '',
	@EditOnList NVARCHAR(20) = '',
	@NotSaveDB NVARCHAR(20) = '',
	@ETLConfig NVARCHAR(MAX) = NULL
AS
BEGIN
	SET NOCOUNT ON 
	SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED
	
	DECLARE @StatusCode nVARCHAR(4000)
	DECLARE @StatusMess NVARCHAR(MAX)
	if @IsReCreate IN( 'on','true') SET @IsReCreate = '1'
	if @IsReCreate IN( 'off','false') SET @IsReCreate = '0'
	
	SET @StatusCode = 'DONE'
	SET @StatusMess = N'Cập nhật thành công'

	BEGIN TRY
	
		SET @Disable = CASE WHEN @Disable = 'on' THEN '1' WHEN @Disable = 'off' THEN '0' ELSE @Disable END 
		SET @IsRequire = CASE WHEN @IsRequire = 'on' THEN '1' WHEN @IsRequire = 'off' THEN '0' ELSE @IsRequire END 
		SET @ItemStatus = CASE WHEN @ItemStatus = 'on' THEN '1' WHEN @ItemStatus = 'off' THEN '0' ELSE @ItemStatus END 
		
	
		IF(@ACTION = 'DELETE') AND @DomainID <> 0 AND @ItemID <> 0 
		AND NOT EXISTS(SELECT * FROM PSYS.FormConfig AS r WHERE r.ItemParent = @ItemID) --- no child
		BEGIN
			PRINT 'DELETE'
			DELETE PSYS.FormConfig
			WHERE ItemID = @ItemID 
			AND FormCode = @FormCode
		END 
		ELSE ---- EDIT 		
		IF @DomainID <> 0 AND @ItemID <> 0
		BEGIN
			
		
			
			
			SET @StatusCode =  'EDIT'
			UPDATE PSYS.FormConfig
			SET ItemName = @ItemName, 
				ItemType = @ItemType, 
				ItemStatus = @ItemStatus,
				[VALUE] = @Value,				
				[Index]=@Index  ,
				Col=@Col  ,
				[Key]=@Key  ,
				
				Holder=@Holder  ,
				Display=@Display  ,
				DisplayVN=@DisplayVN  ,
				OptionConfig = @OptionConfig,
				DefaultValue=@DefaultValue  ,
				[MaxLength]=@MaxLength  ,
				IsRequire=@IsRequire  ,
				Note=@Note  ,
				Pattern=@Pattern  ,
				[Disable]=@Disable  ,
				DataSource=@DataSource  ,
				ColCode=@ColCode  ,
				ColName=@ColName  ,
				Condition=@Condition  ,
				SourceCondition=@SourceCondition,
				RowNum = @RowNum,
				StepControl = @StepControl,
				StepAction = @StepAction,
				ObjectInCharge = @ObjectInCharge,
				StepOption = @StepOption,
				Scripts = @Scripts,
				TitleIcon = @TitleIcon,
				TitleConfig = @TitleConfig,
				TabKey= @TabKey,
				EditOnList = @EditOnList,
				NotSaveDB = @NotSaveDB,
				ETLConfig = @ETLConfig
			WHERE ItemID = @ItemID
		
			
		END
		ELSE ---move node---	
		IF @ACTION= 'MoveNode' AND @NodeID <> 0 
		BEGIN
			SET @StatusCode =  'MoveNode'
			SET @ItemID = @NodeID
			SET  @ItemParent  = @ParentID
			UPDATE PSYS.FormConfig
			SET ItemParent = @ParentID		
			WHERE ItemID = @NodeID
		END 
		ELSE ---- add new ----	
		IF @ItemID = 0 AND @DomainID <> 0 
		BEGIN
			SET @StatusCode =  'ADD'
			
			---	alter table LGS.FormConfig_LOG ADD NotSaveDB VARCHAR(20)
			
			INSERT INTO PSYS.FormConfig
			(
				ItemName,
				ItemType,
				ItemStatus,
				ItemParent,
				DomainID,
				FormCode ,
				
				[VALUE] ,		
				[Index],
				Col,
				[Key],
				Holder,
				Display,
				DisplayVN,
				OptionConfig,
				DefaultValue,
				[MaxLength],
				IsRequire,
				Note,
				Pattern,
				[Disable],
				DataSource,
				ColCode,
				ColName,
				Condition,
				SourceCondition,
				RowNum,
				StepControl,
				StepAction,
				ObjectInCharge, 
				StepOption,
				TitleIcon,TitleConfig,
				Scripts,
				TabKey,
				EditOnList,
				NotSaveDB,
				ETLConfig
			)
			select 
				@ItemName,
				@ItemType,
				@ItemStatus,
				@ItemParent,
				@DomainID,
				@FormCode,
				@Value,				
				@Index  ,
				@Col  ,
				@Key  ,
				@Holder  ,
				@Display  ,
				@DisplayVN  ,
				@OptionConfig,
				@DefaultValue  ,
				@MaxLength  ,
				@IsRequire  ,
				@Note  ,
				@Pattern  ,
				@Disable  ,
				@DataSource  ,
				@ColCode  ,
				@ColName  ,
				@Condition  ,
				@SourceCondition,
				@RowNum,  
				@StepControl,
				@StepAction,
				@ObjectInCharge,
				@StepOption,
				@TitleIcon,@TitleConfig,
				@Scripts,
				@TabKey,
				@EditOnList,
				@NotSaveDB,
				@ETLConfig
			
		END	
		
		DECLARE @ParentString VARCHAR(20)
		DECLARE @ChildString nVARCHAR(4000)
		DECLARE @NodeLevel INT 
		
		exec PSYS.sp_FindParent 'PSYS.FormConfig','ItemID', 'ItemParent', @ItemID, @ParentString OUTPUT, @NodeLevel OUTPUT 
		EXEC PSYS.sp_FindChild 'PSYS.FormConfig','ItemID', 'ItemParent', @ItemID, @ChildString OUTPUT
		
		UPDATE PSYS.FormConfig 
		SET ItemCode = @ParentString , 
			NodeLevel = @NodeLevel, 
			IsChild = CASE WHEN ISNULL(@ChildString,'') = '' THEN 1 ELSE 0 END 
		WHERE ItemID = @ItemID
		
		if @FormType = 'CATE'
		BEGIN 
			SET @StatusCode =  'UPDATE NEW FROM ITEMS'
			exec UTIL.GenFormItems	 @ItemID, @FormCode ,	@IsReCreate  
			
			SET @StatusCode =  'UPDATE NEW ITEMS DATALIST'
			exec UTIL.GenFormDataList @ItemID, @FormCode ,	@IsReCreate  
			SET @StatusCode =  'UPDATE NEW ITEMS TREE LIST CONFIG'
			exec UTIL.GenTreeListConfig	 @ItemID, @FormCode ,	@IsReCreate  
		END 
		
		if @FormType = 'WFL'
		BEGIN 
			SET @StatusCode =  'UPDATE NEW FROM ITEMS'
			exec UTIL.GenFormItems	 @ItemID, @FormCode ,	@IsReCreate  
			
			SET @StatusCode =  'UPDATE NEW ITEMS DATALIST'
			exec UTIL.GenFormDataList @ItemID, @FormCode ,	@IsReCreate  
			SET @StatusCode =  'UPDATE NEW ITEMS TREE LIST CONFIG'
			exec UTIL.GenTreeListConfig	 @ItemID, @FormCode , @IsReCreate  
			
			exec WFL.DocumentStepConfig_UpdateAll  @ItemID
			
		END 
		
		if @FormType = 'RPT'
		BEGIN
			SET @StatusCode =  'UPDATE NEW ITEMS REPORT'
			PRINT @ItemID
			SET @StatusCode =  'GenReportForm'
			exec UTIL.GenReportForm	 @ItemID, @FormCode ,@IsReCreate  
			
			SET @StatusCode =  'GenReportItem'
			exec UTIL.GenReportItem	 @ItemID, @FormCode ,@IsReCreate  
			RETURN;
			
		END 
		
	END TRY
	BEGIN CATCH
		
		SET @StatusMess = ERROR_MESSAGE()	
		INSERT INTO LGS.[EXEC_LOG]([DateCreated],[ObjectName],[SSID],[RowID],[StatusCode],[StatusMess])
		SELECT GETDATE(),OBJECT_NAME(@@PROCID), @ItemID AS SSID,CAST(@DomainID as varchar(50)) as RowID, @StatusCode, @StatusMess AS StatusMess;
			
		SELECT @StatusCode as StatusCode, @StatusMess as StatusMess
		RETURN;		
	END CATCH
	
	SELECT 'DONE' as StatusCode, @StatusMess as StatusMess
END


/*
	
	exec UTIL.GenReportItem	 @ItemID=3320, @FormCode='RPT-0108' ,@IsReCreate  = 1
		
*/
GO
