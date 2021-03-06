USE [BIS]
GO
/****** Object:  StoredProcedure [PSYS].[FormConfigETL_GetList]    Script Date: 8/28/2019 3:44:33 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

--	[PSYS].[FormConfigETL_GetList]  @ItemID  = '3924'
CREATE PROC [PSYS].[FormConfigETL_GetList]
	@DomainID INT =0,
	@SSID VARCHAR(50) = '',
	@ACTION VARCHAR(50) = '',
	@OBJECTID VARCHAR(50) = '',
	@USERID INT = 0,
	
	@ItemID INT = 0
AS
BEGIN
	SET NOCOUNT ON 
	SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED
	DECLARE @FormCode VARCHAR(20)
	DECLARE @Value NVARCHAR(MAX)
	
	
	SELECT @FormCode = c.FormCode, @Value = c.ETLConfig
	FROM PSYS.FormConfig c JOIN PSYS.FormConfig c1 ON c.ItemParent = c1.ItemID
	WHERE c.ItemID = @ItemID
	AND c1.ItemName = 'ETL'
	
	PRINT @FormCode
	
	
	SELECT id, [KEY], val 
	INTO #tmp_data
	FROM OPENJSON(@Value)
	WITH
	(
		id VARCHAR(255), 
		[key] VARCHAR(255), 
		val VARCHAR(255)
	) AS JS
	print @Value

	
	
	if @Value <> ''
	BEGIN 
		SELECT	DISTINCT  id, 
				(SELECT Val FROM #tmp_data t WHERE [key] = 'ItemName' AND  d.ID = t.ID ) as ItemName,
				(SELECT Val FROM #tmp_data t WHERE [key] = 'DataType' AND  d.ID = t.ID ) as DataType,
				(SELECT Val FROM #tmp_data t WHERE [key] = 'Col' AND  d.ID = t.ID ) as Col, 
				(SELECT Val FROM #tmp_data t WHERE [key] = 'Row' AND  d.ID  = t.ID ) as Row
		INTO #tmp_report1
		FROM #tmp_data d
		WHERE id IS NOT NULL
		
		SELECT * 
		FROM #tmp_report1 c
		ORDER BY cast(c.Col AS INT), cast(c.Row AS INT) 
		
	END
	ELSE 
	BEGIN
		DECLARE @parentid INT 
		SELECT @parentid = ItemID
		FROM PSYS.FormConfig 
		WHERE formcode = @FormCode
		AND ItemName = CASE WHEN @FormCode LIKE 'RPT%' THEN 'Report' ELSE 'FORM' END 
		
		SELECT	itemid id, 
				[Key] as ItemName,
				ItemType as DataType,
				c.[Index]  as Row, 
				Col as Col
		FROM PSYS.FormConfig c
		WHERE formcode = @FormCode
		AND ItemParent = @parentid
		ORDER BY cast(c.Col AS INT), cast(c.[Index] AS INT) 
	END 

END

GO
