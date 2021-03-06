USE [BIS]
GO
/****** Object:  StoredProcedure [PSYS].[sp_GetTreeList]    Script Date: 8/28/2019 3:44:33 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROC [PSYS].[sp_GetTreeList]
	@DomainID INT =0,
	@SSID VARCHAR(50) = '',
	@Action VARCHAR(50) = 'TABLE',
	@ObjectID VARCHAR(50) = '',
	@UserID VARCHAR(255) = '',
	@FuncCode VARCHAR(255) = '',
	@TableName VARCHAR(255),
	@ColID VARCHAR(255)= 'ID',
	@ColCode VARCHAR(255)= 'Code',
	@ColName VARCHAR(255) = 'Name',
	@ColParentID VARCHAR(255) = 'ParentID',
	@ParentID VARCHAR(255) = '0',
	@Condition NVARCHAR(255) = '',
	@option VARCHAR(50)= ''
AS
BEGIN
	SET NOCOUNT ON
	SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED
	---header
	DECLARE @DEBUGMODE BIT
	DECLARE @StatusCode VARCHAR(20)
	DECLARE @StatusMess NVARCHAR(MAX)
	SET @DEBUGMODE = DBO.UFN_GET_SYS_CONFIG('DEBUG_MODE')
	SET @StatusCode = 'DONE'
	SET @StatusMess = N'Cập nhật thành công'
	SET @TableName = REPLACE(@TableName,'[','')
	SET @TableName = REPLACE(@TableName,']','')
	
	
	DECLARE @SQL NVARCHAR(MAX)
	
	BEGIN TRY
		
		IF @DEBUGMODE = 1 and NOT EXISTS(SELECT * FROM PSYS.SelectListTable WHERE tableName = @TableName)
			INSERT INTO PSYS.SelectListTable(tablename,SelectCount) select @TableName, 1
		ELSE IF @DEBUGMODE = 1
			UPDATE  PSYS.SelectListTable SET SelectCount = SelectCount + 1 WHERE TableName = @TableName
		
		
		BEGIN
			DECLARE @tb NVARCHAR(255)
			SELECT @tb = TABLE_NAME
			FROM INFORMATION_SCHEMA.TABLES AS t 
			WHERE t.TABLE_SCHEMA + '.' + t.TABLE_NAME = @TableName
				
				
			IF @tb <> ''
			BEGIN
				IF @ColParentID = '' SET @ColParentID = @tb + 'Parent'
				
				SET @SQL = '
					SELECT 
						[' + @ColID + '] as ID, 
						[' + @ColCode + '] as Code, 
						[' + @ColName + '] as Name, 
						ISNULL(['  + @ColParentID + '],0) as ParentID, 
						0 AS IsCheck,
						cast(1 AS BIT) as IsOpen,
						cast(null as varchar(255)) AS ObjectCall,
						NodeLevel,
						Cast(IsChild as int) as IsChild
					
					FROM '+ @TableName + ' WITH(NOLOCK)
					WHERE ( DomainID = ' + CAST(@DomainID AS VARCHAR(20)) + '  OR DomainID IS NULL)
					' + CASE WHEN  ISNULL(@ColCode,'') NOT IN ('','NULL', 'undefined') AND ISNULL(@ParentID,'') NOT IN ('','NULL', 'undefined','0')  THEN ' AND ' + @ColCode + ' LIKE ''' + @ParentID  + '%''' ELSE '' END + '
					' + CASE WHEN  ISNULL(@Condition,'') NOT IN ('','NULL', 'undefined') THEN ' AND ' + @Condition ELSE '' END + '
					ORDER BY 1'
			END
			ELSE BEGIN
			
			SET @SQL = 'EXEC  ' + @TableName +'	
								@DomainID=''' + CAST(@DomainID AS VARCHAR(20)) + ''',
								@SSID=''' + CAST(@SSID AS VARCHAR(50)) + ''',
								@ACTION=''' + CAST(@Action AS VARCHAR(20)) + ''',
								@OBJECTID =''' + CAST('' AS VARCHAR(20)) + ''',
								@USERID= ' + CAST(@UserID AS VARCHAR(20)) +
								CASE WHEN  isnull(@Condition,'') = '' OR @Condition ='null' THEN '' ELSE ',' + @Condition END 
			END					
			INSERT INTO LGS.[EXEC_LOG]([DateCreated],[ObjectName],[SSID],[RowID],[StatusCode],[StatusMess])
			SELECT GETDATE(),OBJECT_NAME(@@PROCID), @SSID AS SSID,CAST('' as varchar(50)) as RowID, 'CHECK', @SQL AS StatusMess;
	
			PRINT @SQL
			EXEC SP_EXECUTESQL @SQL
		END 
		
	END TRY
		BEGIN CATCH
			SET @StatusCode = 'ERR'
			SET @StatusMess = ERROR_MESSAGE() + ' ' +  @SQL
			INSERT INTO LGS.[EXEC_LOG]([DateCreated],[ObjectName],[SSID],[RowID],[StatusCode],[StatusMess])
			SELECT GETDATE(),OBJECT_NAME(@@PROCID), @SSID AS SSID,CAST('' as varchar(50)) as RowID, @StatusCode, @StatusMess AS StatusMess;
			SELECT @StatusCode as Code, @StatusMess as Name
			RETURN;
			
	END CATCH
	
END	---	[PSYS].[sp_GetTreeList] @TableName ='PSYS.Functions', @UserID = 1

GO
