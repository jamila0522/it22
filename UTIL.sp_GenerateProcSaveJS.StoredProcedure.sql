USE [BIS]
GO
/****** Object:  StoredProcedure [UTIL].[sp_GenerateProcSaveJS]    Script Date: 8/28/2019 3:44:33 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROC [UTIL].[sp_GenerateProcSaveJS]
	
	@Schema NVARCHAR(255) = '',
	@TableName NVARCHAR(255) = '',
	@IsReCreate BIT = 0
AS
BEGIN
	DECLARE @SQL NVARCHAR(MAX)
	DECLARE @SourceInfo VARCHAR(255)
	set @SourceInfo = @Schema + '.' + @TableName
	SET @SourceInfo =  REPLACE(REPLACE(@SourceInfo, '[', ''), ']','')
	
	
	IF NOT EXISTS(
		SELECT * FROM SYS.procedures p WHERE OBJECT_SCHEMA_NAME([schema_id]) +'.' + name like  + '_SaveJS'
	) 
	BEGIN	
		DECLARE @SELECTJS NVARCHAR(4000)
		DECLARE @INSERTSTRING1 NVARCHAR(4000)
		DECLARE @INSERTSTRING2 NVARCHAR(4000)
		SET @SELECTJS = ''
	
		SET @INSERTSTRING1 = ''
		SET @INSERTSTRING2 = ''
		
		IF @IsReCreate = 1
		begin
			SET @SQL = 'DROP PROC ' + @SourceInfo +'_SaveJS'
			EXEC (@SQL)
		end
		
		SELECT 
			@SELECTJS = @SELECTJS   + CASE WHEN C.COLUMN_NAME NOT IN (@TableName + 'ID', 'ID') THEN '
			(SELECT Val FROM #tmp_data t WHERE [key] = '''+ C.COLUMN_NAME + ''' AND  d.ID = t.ID ) as '  + C.COLUMN_NAME + ',' 
			ELSE '' END, -- SELECT STRING
			
			@INSERTSTRING1 = @INSERTSTRING1 + CASE WHEN C.COLUMN_NAME NOT IN (@TableName + 'ID') THEN '
					'  +C.COLUMN_NAME + ',' ELSE '' END, -- INSERT STRING
			@INSERTSTRING2 = @INSERTSTRING2 + 
					CASE 
						WHEN C.DATA_TYPE IN ('int','double', 'float', 'decimal') THEN  '
						DBO.[ufn_get_number](' + C.COLUMN_NAME + '),' 
						
						WHEN C.DATA_TYPE IN ('date','datetime', 'time') THEN  '
						DBO.[ufn_get_dateVN](' + C.COLUMN_NAME + '),' 
						
						WHEN C.DATA_TYPE IN ('BIT') THEN  '
						DBO.[ufn_ConvertString2Bit](' + C.COLUMN_NAME + '),' 
						WHEN C.COLUMN_NAME NOT IN (@TableName + 'ID') THEN '
						' + C.COLUMN_NAME + ',' ELSE '' END -- INSERT STRING
			
		FROM INFORMATION_SCHEMA.COLUMNS C
		WHERE TABLE_SCHEMA + '.' + TABLE_NAME = @SourceInfo
		AND C.COLUMN_NAME NOT IN (
			'UserID', 'ID', 'UserIDCreated', 'DateTimeCreated', 'UserIDUpdated', 'DateTimeUpdated', 
			'DomainID', 'NodeLevel', 'IsChild', 'Ordinal', 'ObjectID', 'SSID', 'DomainID', 'Action', 'UserID', 'NodeID', 'ParentID'
		)
		--
		SET @SELECTJS = LEFT(@SELECTJS, LEN(@SELECTJS) - 1)
		SET @INSERTSTRING1 = LEFT(@INSERTSTRING1, LEN(@INSERTSTRING1) - 1)
		SET @INSERTSTRING2 = LEFT(@INSERTSTRING2, LEN(@INSERTSTRING2) - 1)
			
		set @SQL = ''
		
		SET @SQL = @SQL + '
CREATE PROC ' + @SourceInfo + '_SaveJS
	@DomainID INT =0,
	@SSID VARCHAR(50) = '''',
	@ACTION VARCHAR(50) = '''', 
	@OBJECTID VARCHAR(50) = '''',
	@USERID INT,
	@NodeID INT =0,
	@ParentID INT = 0,
	@ID INT = 0,
	@FormCode varchar(20) = '''',
	@jsString nvarchar(max)
AS 
BEGIN
	SET NOCOUNT ON 
	SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED
	
	DECLARE @StatusCode VARCHAR(20)
	DECLARE @StatusMess NVARCHAR(MAX)
	--
	SET @StatusCode = ''DONE''
	SET @StatusMess = N''Cập nhật thành công''

	BEGIN TRY
		SELECT id, [key], val 
		INTO #tmp_data
		FROM OPENJSON(@jsString)
		WITH
		(
			id VARCHAR(255), 
			[key] VARCHAR(255), 
			val VARCHAR(255)
		) AS JS
		print @jsString
	
		
		SELECT	DISTINCT  id, 
				' + @SELECTJS + '
		INTO #tmp_report
		FROM #tmp_data d
		WHERE id IS NOT NULL


		DELETE ' + @Schema + '.' + @TableName + ' WHERE '+ @TableName +'ID = @ID
		
	    INSERT INTO '+  @Schema + '.' + @TableName + '(' + @INSERTSTRING1 + ', UserIDCreated, DateTimeCreated, DomainID)
	    SELECT ' + @INSERTSTRING2 + ' , @UserID, getdate() , @DomainID
	    FROM #tmp_report
	
	
	END TRY
	BEGIN CATCH
		SET @StatusCode = ''ERR''
		SET @StatusMess = ERROR_MESSAGE()+ '' '' + @jsString  
		INSERT INTO LGS.[EXEC_LOG]([DateCreated],[ObjectName],[SSID],[RowID],[StatusCode],[StatusMess])
		SELECT GETDATE(),OBJECT_NAME(@@PROCID), @ID AS SSID,CAST(@DomainID as varchar(50)) as RowID, @StatusCode + ISNULL(@Action,''''), @StatusMess AS StatusMess;
			
		SELECT @StatusCode as StatusCode, @StatusMess as StatusMess
		RETURN;		
	END CATCH
	
	SELECT @StatusCode as StatusCode, @StatusMess as StatusMess
END
		'
		BEGIN TRY
			PRINT @SQL
			EXEC (@SQL)
		END TRY
		BEGIN CATCH
			INSERT INTO LGS.[EXEC_LOG]([DateCreated],[ObjectName],[SSID],[RowID],[StatusCode],[StatusMess])
			SELECT GETDATE(),OBJECT_NAME(@@PROCID), '' AS SSID,CAST('' as varchar(50)) as RowID, 'ERR' as StatusCode,  ERROR_MESSAGE() as StatusMess
			SELECT 'ERR' as StatusCode,  ERROR_MESSAGE() as StatusMess
			RETURN;		
		END CATCH
		SELECT 'DONE' AS StatusCode
	
		
	END
	
	
	 
END 
/*
	UTIL.sp_GenerateProcSaveJS
		@Schema  = 'WFL',
		@TableName = 'BudgetAllocations'
 
*/
GO
