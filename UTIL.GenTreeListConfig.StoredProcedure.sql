USE [BIS]
GO
/****** Object:  StoredProcedure [UTIL].[GenTreeListConfig]    Script Date: 8/28/2019 3:44:33 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE  PROC [UTIL].[GenTreeListConfig]
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
					FROM PSYS.FormConfig c JOIN  PSYS.FormConfig c1 ON c.ItemID = c1.ItemParent
					WHERE c.ItemID = @ItemID AND C.ItemName ='Layout' AND C.Value = 'TreeListEdit'
				) OR @IsReCreate = '1' 
			)AND EXISTS ( --- is datalist
				SELECT * 
				FROM PSYS.FormConfig c 
				WHERE ItemID = @ItemID 
				AND C.ItemName ='Layout' AND C.Value = 'TreeListEdit'
			)
		) 
		BEGIN
			PRINT 'TRY TO GET LIST ITEMS REPORTS'
			DECLARE @DataSource VARCHAR(255)
			set @DataSource = ''
			
			SELECT @DataSource = [VALUE]
			FROM PSYS.FormConfig c 
			WHERE FormCode = @FormCode AND C.ItemName ='Source'
			
			DELETE FROM  PSYS.FormConfig WHERE ItemParent = @ItemID AND ItemParent > 0 
			
			PRINT 'INSERT REPORTS ITEMS LIST'
			INSERT INTO PSYS.FormConfig(
				ItemName, ItemParent, ItemType, ItemStatus,
				FormCode, UserIDCreated, DomainID, NodeLevel, IsChild, [Value],[Index],
				Col, [Key])
			SELECT  
				'eventGetDataUrl' ItemName,	@ItemID AS ItemParent, 'TreeListEdit' ItemType, '1'  AS ItemStatus,
				@FormCode as FormCode, 0 UserIDCreated, 0 as DomainID, 3 NodeLevel,1 IsChild, 
				'/Categories/ControlsBase/SelectTreeListAjax?DataSource='+ @DataSource + 
				'&ColID=' +  [PSYS].[ufn_FindDataSource](@DataSource, 'ID') + 
				'&ColCode=' +  [PSYS].[ufn_FindDataSource](@DataSource, 'Code') + 
				'&ColName=' +  [PSYS].[ufn_FindDataSource](@DataSource, 'Name') + 
				'&ColParentID=' +  [PSYS].[ufn_FindDataSource](@DataSource, 'ParentID') 
				AS [Value],
				1 AS [Index],0 as Col, 'eventGetDataUrl' [Key]
			UNION ALL
			SELECT  
				'eventOnClickUrl' ItemName,	@ItemID AS ItemParent, 'TreeListEdit' ItemType, '1'  AS ItemStatus,
				@FormCode as FormCode, 0 UserIDCreated, 0 as DomainID, 3 NodeLevel,1 IsChild, 
				'OpenLink' AS [Value],
				2 AS [Index], 0 as Col, 'eventOnClickUrl' [Key]
			UNION ALL
			SELECT  
				'eventOnClickFunction' ItemName,	@ItemID AS ItemParent, 'TreeListEdit' ItemType, '1'  AS ItemStatus,
				@FormCode as FormCode, 0 UserIDCreated, 0 as DomainID, 3 NodeLevel,1 IsChild, 
				'onTreeViewClick' [Value],3 AS [Index],0 as Col, 'eventOnClickFunction' [Key]
			UNION ALL
			SELECT  
				'eventDnDUrl' ItemName,	@ItemID AS ItemParent, 'TreeListEdit' ItemType, '1'  AS ItemStatus,
				@FormCode as FormCode, 0 UserIDCreated, 0 as DomainID, 3 NodeLevel,1 IsChild, 
				'/Categories/CateAddUpdate/CateSave' AS [Value],
				4 AS [Index],0 as Col, 'eventDnDUrl' [Key]
			UNION ALL
			SELECT  
				'ActionType' ItemName,	@ItemID AS ItemParent, 'TreeListEdit' ItemType, '1'  AS ItemStatus,
				@FormCode as FormCode, 0 UserIDCreated, 0 as DomainID, 3 NodeLevel,1 IsChild, 
				'' [Value],5 AS [Index],0 as Col, 'ActionType' [Key]
			UNION ALL
			SELECT  
				'TreeType' ItemName,	@ItemID AS ItemParent, 'TreeListEdit' ItemType, '1'  AS ItemStatus,
				@FormCode as FormCode, 0 UserIDCreated, 0 as DomainID, 3 NodeLevel,1 IsChild, 
				'dnd' [Value],6 AS [Index],0 as Col, 'TreeType' [Key]
	END 


END


/*
		
*/
GO
