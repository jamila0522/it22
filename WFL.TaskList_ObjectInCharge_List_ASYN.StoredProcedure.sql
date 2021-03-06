USE [BIS]
GO
/****** Object:  StoredProcedure [WFL].[TaskList_ObjectInCharge_List_ASYN]    Script Date: 8/28/2019 3:44:33 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROC [WFL].[TaskList_ObjectInCharge_List_ASYN]
AS
BEGIN
	SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED
	
	
	CREATE TABLE #TaskList_ObjectInCharge_List(ItemType VARCHAR(20), ItemCode VARCHAR(20), ItemName NVARCHAR(255))
	
	INSERT INTO #TaskList_ObjectInCharge_List(ItemType, ItemCode, ItemName)
	SELECT 'PERSON',  CAST(u.UserID AS VARCHAR(20)) AS ItemCode, u.UserName AS ItemName
	FROM HRM.Employees AS e JOIN PSYS.Users AS u ON u.EmployeeId = e.EmployeeId
	
		
	INSERT INTO #TaskList_ObjectInCharge_List(ItemType, ItemCode, ItemName)
	SELECT distinct 'RSM', RegionCode  AS ItemCode, RegionName AS ItemName
	FROM PSYS.Locations AS l
	WHERE len(RegionCode) > 2
	 
	
	INSERT INTO #TaskList_ObjectInCharge_List(ItemType, ItemCode, ItemName)
	SELECT distinct 'ASM', AreaCode  AS ItemCode, AreaName AS ItemName
	FROM PSYS.Locations 
	WHERE len(AreaCode) > 2 AND AreaName IS NOT NULL
	
	INSERT INTO #TaskList_ObjectInCharge_List(ItemType, ItemCode, ItemName)
	SELECT distinct 'SM', LocationCode  AS ItemCode, LocationName AS ItemName
	FROM PSYS.Locations  
	WHERE len(LocationCode) > 2
	 

	--INSERT INTO #TaskList_ObjectInCharge_List(ItemType, ItemCode, ItemName)
	--SELECT 'GROUP' , OrganizationHierachyCode AS ItemCode,OrganizationHierachyName AS ItemName 
	--FROM  INSIDE.FRTInsideV2.dbo.F03_OrganizationHierachies 
	--WHERE HierachyLevel = 2
		
	--INSERT INTO #TaskList_ObjectInCharge_List(ItemType, ItemCode, ItemName)
	--SELECT 'TITLE' , jt.JobTitleCode AS ItemCode,jt.JobTitleName AS ItemName
	--FROM  INSIDE.FRTInsideV2.[dbo].[F03_JobTitles] jt
	--WHERE jt.Status = 'A' AND jt.IsShop = 1
	
	TRUNCATE TABLE WFL.TaskList_ObjectInCharge_List
	INSERT INTO  WFL.TaskList_ObjectInCharge_List
	SELECT * FROM #TaskList_ObjectInCharge_List
	
	
end	
GO
