USE [BIS]
GO
/****** Object:  StoredProcedure [WFL].[DocumentContract_SaveJS]    Script Date: 8/28/2019 3:44:33 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROC [WFL].[DocumentContract_SaveJS]
	@DomainID INT,
	@SSID VARCHAR(50) = '',
	@ACTION VARCHAR(50) = '', 
	@OBJECTID VARCHAR(50) = '',
	@USERID INT,
	@FormCode VARCHAR(20), 
	@jsString nvarchar(max)
	
AS 
BEGIN
	SET NOCOUNT ON 
	SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED
	
	DECLARE @StatusCode VARCHAR(20)
	DECLARE @StatusMess NVARCHAR(MAX)
	--
	SET @StatusCode = 'DONE'
	SET @StatusMess = N'Done'

	BEGIN TRY
		SELECT id, [key], val 
		INTO #tmp_data
		FROM OPENJSON(@jsString)
		WITH
		(
			id nVARCHAR(255), 
			[key] nVARCHAR(255), 
			val nVARCHAR(255)
		) AS JS
		print @jsString
		
		SELECT	DISTINCT  id, 
				(SELECT TOP(1) Val FROM #tmp_data t WHERE [key] = 'DocumentID' AND  d.ID = t.ID ) as DocumentID,
				(SELECT TOP(1) Val FROM #tmp_data t WHERE [key] = 'ACCACode' AND  d.ID = t.ID ) as ACCACode,
				(SELECT TOP(1) Val FROM #tmp_data t WHERE [key] = 'CustomerName' AND  d.ID = t.ID ) as CustomerName,
				(SELECT TOP(1) Val FROM #tmp_data t WHERE [key] = 'ContractNo' AND  d.ID = t.ID ) as ContractNo,
				(SELECT TOP(1) Val FROM #tmp_data t WHERE [key] = 'ProfileStatus' AND  d.ID = t.ID ) as ProfileStatus,
				(SELECT TOP(1) Val FROM #tmp_data t WHERE [key] = 'DocumentNote' AND  d.ID = t.ID ) as DocumentNote,
				(SELECT TOP(1) Val FROM #tmp_data t WHERE [key] = 'Action' AND  d.ID = t.ID ) as [Action]
		INTO #tmp_report
		FROM #tmp_data d
		WHERE id IS NOT NULL
		
		declare @id INT ,@DocumentID BIGINT, @ACCACode NVARCHAR(255),	@CustomerName NVARCHAR(255), @ContractNo NVARCHAR(255),	@ProfileStatus NVARCHAR(255),	@DocumentNote NVARCHAR(255)
		DECLARE cur CURSOR 
		FOR 
		SELECT id, ACCACode,	CustomerName,	ContractNo,	ProfileStatus,	DocumentNote,[Action]
		
		FROM #tmp_report
		
		OPEN cur
		FETCH NEXT FROM cur INTO @id ,@DocumentID, @ACCACode,@CustomerName,@ContractNo,@ProfileStatus,@DocumentNote,@Action
		
		WHILE(@@FETCH_STATUS = 0)
		BEGIN
			
			EXEC [WFL].[DocumentContract_Save]
					@DomainID = @DomainID, 
					@SSID = @SSID ,
					@ACTION = @ACTION,
					@OBJECTID = @OBJECTID, 
					@USERID = @USERID ,
					@FormCode = @FormCode, 
					@DocumentID = @DocumentID, 
					@ACCACode = @ACCACode,
					@CustomerName = @CustomerName,
					@ContractNo = @ContractNo,
					@ProfileStatus = @ProfileStatus,
					@DocumentNote = @DocumentNote
			
			---------------
			FETCH NEXT FROM cur INTO @id ,@DocumentID, @ACCACode,@CustomerName,@ContractNo,@ProfileStatus,@DocumentNote,@Action
		END
		CLOSE cur
		DEALLOCATE cur
	
	END TRY
	BEGIN CATCH
		SET @StatusCode = 'ERR'
		SET @StatusMess = ERROR_MESSAGE()+ @jsString
		INSERT INTO LGS.[EXEC_LOG]([DateCreated],[ObjectName],[SSID],[RowID],[StatusCode],[StatusMess])
		SELECT GETDATE(),OBJECT_NAME(@@PROCID), '' AS SSID,CAST(@DomainID as varchar(50)) as RowID, @StatusCode + ISNULL(@Action,''), @StatusMess AS StatusMess;
			
		SELECT @StatusCode as StatusCode, @StatusMess as StatusMess
		RETURN;		
	END CATCH
	
	SELECT @StatusCode as StatusCode, @StatusMess as StatusMess, '' StatusType
END
		
GO
