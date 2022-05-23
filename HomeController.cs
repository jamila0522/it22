using BussinessObjects;
using Helpers.Functions;
using OfficeOpenXml;
using OfficeOpenXml.Style;
//using GemBox.Spreadsheet;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web.Mvc;
using Newtonsoft.Json;
using System.Web;
using System.Runtime.Caching;

namespace Recruitment.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Message = "Your application description page.";

            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Message = "Your contact page.";

            return View();
        }
        [HttpPost]
        [Route("Request/ExportData")]
        public JsonResult ExportData(string FormCode = "WFL-1288", string SourceID = "", string data = "")
        {
            try
            {
                CategoriesItemModel model = GetFormModelCache(FormCode);
                //List<string> para = MapSubmitParamsReport(model.Form, null, null, "ExportData");

                //string Source = "";
                //string title = "";
                //FItem columnsItem = new FItem();
                //if (SourceID == "")
                //{
                //    Source = model.Source;
                //    title = model.Title ?? "";
                //}
                //else if (model.ETL != null && model.ETL.Count > 0 && model.ETL.Find(m => m.Key == SourceID) != null)
                //{
                //    columnsItem = model.ETL.Find(m => m.Key == SourceID);
                //    Source = columnsItem.DataSource;
                //    title = columnsItem.Display ?? "";
                //}

                //model.ReportData = DataFunction.GetDataReportFromService(ServiceUrl_Report, "ExportData", para, model.FormCode, Source, SEmployee.DomainID.ToString(), SEmployee.UserID.ToString());
                //ReDisplayTable(model.ReportData, model.Report);
                //var pathInFull= Server.MapPath("/Files/Template/Profiles.xlsx");
                //List<FItem> test = ReadFromExcel<List<FItem>>(pathInFull);
                //WriteToExcel(pathInFull);
                try
                {
                    //bool status = ReaderTools.SerializationObjetTolJson(data, "CV_" + DateTime.Now.ToString("yyMMddhhmmss") + "_" + Guid.NewGuid().ToString().Substring(0, 4) + ".json", "Recruitment");

                    //bool status = ReaderTools.SerializationObjetTolJson(JArray.Parse(data), "CV_" + DateTime.Now.ToString("yyMMddhhmmss") + "_" + Guid.NewGuid().ToString().Substring(0, 4) + ".json", "Recruitment");

                    List<string> para = MapSubmitParamsReport(model.Form);
                    var jsData = para.LastOrDefault().ToString();
                    
                    string fileCV = "CV_" + DateTime.Now.ToString("yyMMddhhmmss") + "_" + Guid.NewGuid().ToString().Substring(0, 4) + ".json";
                    bool status = ReaderTools.SerializationObjetTolJson(JArray.Parse(jsData), fileCV, "Recruitment");
                    var pathInFullFileCV = "/Files/Recruitment/" + fileCV;
                    if (status)
                    {
                        para.Add("@FileUrl");
                        para.Add(pathInFullFileCV);
                        DataTable response = DataFunction.SaveCategoriesItemFromService("http://apidata.bps.vn", "", para, FormCode, "[STG].[CandidateData_Save]", "1", "1");
                        //var saveData =DataFunction.ExecuteNoneQuery("http://apibase.bps.vn", "Request/ExportData", "", para, "[STG].[CandidateData_Save]");
                        if (response != null)
                        {
                            Helpers.Functions.DataFunction.WriteLog("Save_FileCV", "Save data Successful!");
                        }
                        Helpers.Functions.DataFunction.WriteLog("Save_FileCV", pathInFullFileCV);
                    }

                }
                catch (Exception ex)
                {
                    Helpers.Functions.DataFunction.WriteLog("CandidateData_Save ERROR", ex.Message);
                }

                return Json(ExportToExcel(new FItem(), FormCode, data, ""), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                DataFunction.WriteLog("ExportData", ex.Message, FormCode, SourceID);
                return Json(ex.Message, JsonRequestBehavior.AllowGet);
            }
        }

        public string ExportToExcel(FItem Model, string FormCode, string dataSource, string title = "")
        {
            try
            {
                string filepath = GenerateReportName(FormCode, title);
                //string filepath = "/Files/Template/Profiles.xlsx";
                var pathInFull = Server.MapPath("/Files/Template/Profiles.xlsx");

                FileInfo filePath = new FileInfo(pathInFull);
                using (var excelPack = new ExcelPackage(filePath))
                {
                    //ExcelWorksheet ws = CopyExcelWorkbook(excelPack.Workbook, "Sheet1", "Sheet2");
                    //ExcelWorksheet ws = excelPack.Workbook.Worksheets.Add("WriteTest");

                    //excelPack.Save();
                    OfficeOpenXml.ExcelWorksheet ws = excelPack.Workbook.Worksheets.FirstOrDefault();
                    FileInfo fi = new FileInfo(Server.MapPath(filepath));
                    if (!fi.Directory.Exists)
                    {
                        fi.Directory.Create();
                    }
                    BindingCells(ws, dataSource);
                    excelPack.SaveAs(fi);
                    return filepath;
                }
            }
            catch (Exception ex)
            {
                DataFunction.WriteLog("ExportToExcel", ex.Message, FormCode);
                return ex.Message;
            }
        }
        public string ExportToExcel1(FItem Model, string FormCode, DataTable dataSource, string title = "")
        {
            try
            {
                string filepath = GenerateReportName(FormCode, title);
                //string filepath = "/Files/Template/Profiles.xlsx";
                var pathInFull = Server.MapPath("/Files/Template/Profiles.xlsx");

                using (ExcelPackage excelPackage = new ExcelPackage())
                {
                    //create a new Worksheet
                    OfficeOpenXml.ExcelWorksheet worksheet = CopySheet(pathInFull);
                    OfficeOpenXml.ExcelWorksheet worksheet2 = excelPackage.Workbook.Worksheets.Add("Sheet 1");

                    FileInfo fi = new FileInfo(Server.MapPath(filepath));
                    if (!fi.Directory.Exists)
                    {
                        fi.Directory.Create();
                    }
                    //BindingCells(worksheet2, new DataTable);
                    //BindingFormatForExcel(Model, worksheet, dataSource, title);
                    excelPackage.SaveAs(fi);
                    return filepath;
                }
            }
            catch (Exception ex)
            {
                DataFunction.WriteLog("ExportToExcel", ex.Message, FormCode);
                return ex.Message;
            }
        }
        private void BindingCells(OfficeOpenXml.ExcelWorksheet worksheet, string data)
        {
            try
            {
                DataTable dataSource = new DataTable();

                //foreach (JObject jObject in JArray.Parse(data))
                //{
                //    var item = ((Newtonsoft.Json.Linq.JContainer)jObject.First).First;
                //}

                for (int rowIndex = 0; rowIndex < 4; rowIndex++)
                {
                    for (int columnIndex = 0; columnIndex < 150; columnIndex++)
                    {
                        try
                        {
                            if (worksheet.Cells[rowIndex + 1, columnIndex + 1].Value != null)
                            {
                                string cellVal = worksheet.Cells[rowIndex + 1, columnIndex + 1].Value.ToString();
                                if (cellVal.IndexOf("#") >= 0 && cellVal.Length > 0)
                                {
                                    worksheet.Cells[rowIndex + 1, columnIndex + 1].Value = "TRAN VAN A";
                                    //worksheet.Cells[rowIndex + 1, columnIndex + 1].Value = worksheet.Cells[rowIndex + 1, columnIndex + 1].Value;
                                }
                            }
                        }
                        catch { }


                    }
                }
            }
            catch (Exception ex)
            {
            }
            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
        }
        private void BindingFormatForExcel(FItem Model, OfficeOpenXml.ExcelWorksheet worksheet, DataTable dataSource, string title)
        {
            List<string> DECIMAL2CHARACTIER = new List<string>();
            // Set default width cho tất cả column
            worksheet.DefaultColWidth = 15;
            // Tự động xuống hàng khi text quá dài
            //worksheet.Cells.Style.WrapText = true;
            //
            worksheet.Cells[1, 2].Style.Font.Size = 16;
            worksheet.Cells[1, 2].Style.Font.Bold = true;
            worksheet.Cells[1, 2].Value = title ?? "";
            worksheet.Cells[1, 2].Style.WrapText = false;

            worksheet.Cells[2, 2].Value = "Date Created:";
            worksheet.Cells[2, 2].Style.Font.Italic = true;
            worksheet.Cells[2, 3].Value = DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss");
            worksheet.Cells[2, 5].Style.Font.Italic = true;
            worksheet.Cells[2, 5].Style.WrapText = false;
            //List<BussinessObjects.IDKeyValModel> ETLConfig = Model.ETLConfig != "" ? Newtonsoft.Json.JsonConvert.DeserializeObject<List<BussinessObjects.IDKeyValModel>>(Model.ETLConfig) : new List<BussinessObjects.IDKeyValModel>();

            // create row header
            if (dataSource != null)
            {
                try
                {
                    int startRow = 4;
                    for (int c = 0; c < dataSource.Columns.Count; c++)
                    {
                        worksheet.Cells[startRow, c + 1].Value = dataSource.Columns[c].ColumnName;
                        Color colFromHex = System.Drawing.ColorTranslator.FromHtml("#B7DEE8");
                        worksheet.Cells[startRow, c + 1].Style.Fill.PatternType = ExcelFillStyle.Solid;
                        worksheet.Cells[startRow, c + 1].Style.Fill.BackgroundColor.SetColor(colFromHex);
                        worksheet.Cells[startRow, c + 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        worksheet.Cells[startRow, c + 1].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                    }
                    startRow++;
                    List<BussinessObjects.IDKeyValModel> ETLConfig = Model.ETLConfig != "" ? Newtonsoft.Json.JsonConvert.DeserializeObject<List<BussinessObjects.IDKeyValModel>>(Model.ETLConfig) : new List<BussinessObjects.IDKeyValModel>();
                    for (int r = 0; r < dataSource.Rows.Count; r++)
                    {
                        int iRow = r + startRow;
                        for (int c = 0; c < dataSource.Columns.Count; c++)
                        {
                            IDKeyValModel colID = ETLConfig.Find(m => m.key == "ItemName" && m.val.Split('.')[0] == dataSource.Columns[c].ColumnName);
                            string id = colID?.id ?? "-1";
                            string Format = id != "-1" ? ETLConfig?.Find(m => m.id == id && m.key == "FormatType")?.val ?? "string" : "string";
                            var val = dataSource.Rows[r][dataSource.Columns[c]];
                            if (Format == "DateTime")
                            {
                                worksheet.Cells[iRow, c + 1].Style.Numberformat.Format = DateTimeFormatInfo.CurrentInfo.ShortDatePattern;
                                if (val != DBNull.Value)
                                {
                                    worksheet.Cells[iRow, c + 1].Value = Convert.ToDateTime(val);
                                }
                            }
                            else
                            {
                                worksheet.Cells[iRow, c + 1].Value = val;
                            }
                        }
                    }

                }
                catch { }
                worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

            }
        }

        protected string GenerateReportName(string rkey = "", string title = "")
        {
            string rName = "/Files/Exports/";
            try
            {
                if (!string.IsNullOrEmpty(rkey))
                {
                    rName += "CV_" + title + DateTime.Now.ToString("yyMMddhhmmss") + "_" + rkey + "_" + Guid.NewGuid().ToString().Substring(0, 4) + ".xlsx";
                }
                else
                {
                    rName += "CV_" + title + "_" + DateTime.Now.ToString("yyMMddhhmmss") + "_" + Guid.NewGuid().ToString().Substring(0, 4) + ".xlsx";
                }
            }
            catch (Exception ex)
            {
                Helpers.Functions.DataFunction.WriteLog("GenerateReportName", ex.Message, rkey);
                rName += "CV_" + title + "_" + DateTime.Now.ToString("yyMMddhhmmss") + "_" + Guid.NewGuid().ToString().Substring(0, 4) + ".xlsx";
            }
            return rName;
        }

        private static T ReadFromExcel<T>(string path, bool hasHeader = true)
        {
            using (var excelPack = new ExcelPackage())
            {
                //Load excel stream
                using (var stream = System.IO.File.OpenRead(path))
                {
                    excelPack.Load(stream);
                }

                //Lets Deal with first worksheet.(You may iterate here if dealing with multiple sheets)
                OfficeOpenXml.ExcelWorksheet ws = excelPack.Workbook.Worksheets.FirstOrDefault();

                //Get all details as DataTable -because Datatable make life easy :)
                DataTable excelasTable = new DataTable();
                foreach (var firstRowCell in ws.Cells[1, 1, 1, ws.Dimension.End.Column])
                {
                    //Get colummn details
                    if (!string.IsNullOrEmpty(firstRowCell.Text))
                    {
                        string firstColumn = string.Format("Column {0}", firstRowCell.Start.Column);
                        excelasTable.Columns.Add(hasHeader ? firstRowCell.Text : firstColumn);
                    }
                }
                var startRow = hasHeader ? 2 : 1;
                //Get row details
                for (int rowNum = startRow; rowNum <= ws.Dimension.End.Row; rowNum++)
                {
                    var wsRow = ws.Cells[rowNum, 1, rowNum, excelasTable.Columns.Count];
                    DataRow row = excelasTable.Rows.Add();
                    foreach (var cell in wsRow)
                    {
                        row[cell.Start.Column - 1] = cell.Text;
                    }
                }
                //Get everything as generics and let end user decides on casting to required type
                var generatedType = JsonConvert.DeserializeObject<T>(JsonConvert.SerializeObject(excelasTable));
                return (T)Convert.ChangeType(generatedType, typeof(T));
            }
        }

        private static void WriteToExcel(string path)
        {
            //Let use below test data for writing it to excel
            //List<FItem> data = new List<FItem>()
            //{
            //new FItem() ,
            //};
            // List<UserDetails> persons = new List<UserDetails>()
            // {
            //     new UserDetails() {ID="9999", Name="ABCD", City ="City1", Country="USA"},
            //     new UserDetails() {ID="8888", Name="PQRS", City ="City2", Country="INDIA"},
            //     new UserDetails() {ID="7777", Name="XYZZ", City ="City3", Country="CHINA"},
            //     new UserDetails() {ID="6666", Name="LMNO", City ="City4", Country="UK"},
            //};

            // let's convert our object data to Datatable for a simplified logic.
            // Datatable is the easiest way to deal with complex datatypes for easy reading and formatting. 
            //DataTable table = (DataTable)JsonConvert.DeserializeObject(JsonConvert.SerializeObject(data), (typeof(DataTable)));
            FileInfo filePath = new FileInfo(path);
            using (var excelPack = new ExcelPackage(filePath))
            {
                ExcelWorksheet ws = CopyExcelWorkbook(excelPack.Workbook, "Sheet1", "Sheet2");
                //ExcelWorksheet ws = excelPack.Workbook.Worksheets.Add("WriteTest");
                //ws.Cells.LoadFromDataTable(table, true, OfficeOpenXml.Table.TableStyles.Light8);
                excelPack.Save();
            }
        }
        private static ExcelWorksheet CopySheet(string path)
        {
            FileInfo filePath = new FileInfo(path);
            using (var excelPack = new ExcelPackage(filePath))
            {
                OfficeOpenXml.ExcelWorksheet ws = excelPack.Workbook.Worksheets.FirstOrDefault();
                return ws;
            }
        }
        private static ExcelWorksheet CopyExcelWorkbook(ExcelWorkbook workbook, string existingWorksheetName, string newWorksheetName)
        {

            ExcelWorksheet worksheet = workbook.Worksheets.Copy(existingWorksheetName, newWorksheetName);
            return worksheet;
        }
        public List<string> MapSubmitParamsReport(List<FItem> fitems, string sessionId = "", string id = "", string Action = null)
        {
            try
            {
                List<string> parms = new List<string>();

                #region 'Set Default Param'
                //parms.Add("@SSID");
                //parms.Add(sessionId);
                parms.Add("@DomainID");
                parms.Add("");
                //parms.Add("@USERID");
                //parms.Add(SEmployee.UserID.ToString());
                //parms.Add("@FormCode");
                //parms.Add(Request["FormCode"].ToString());

                #endregion
                if (fitems != null && fitems.Count() > 0)
                {
                    string _action = Request["Action"]?.ToString();
                    parms.Add("@Action");
                    parms.Add("ADD");
                    MapParamFromRequest(fitems, parms, Request);
                }
                return parms;
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }
        private void MapParamFromRequest(List<FItem> fitems, List<string> pList, HttpRequestBase request)
        {
            try
            {

                int idJsData = 0;
                List<IDKeyValModel> jsData = new List<IDKeyValModel>();
                foreach (FItem item in fitems)
                {
                    if (item.NotSaveDB == "1" || item.NotSaveDB == "on" || item.NotSaveDB == "true"
                        || item.Type == "Line" || item.Type == "ViewLinkList"
                        || item.Key.ToLower() == "jsdata"
                        || pList.Exists(m => m == item.Key)
                    )
                    {
                        continue;
                    }
                    bool isJSData = (item.JsData == "1" || item.JsData == "True" || item.JsData == "On" ? true : false);

                    if (item.Type == null)
                    {
                        item.Type = "TextBox";
                        item.Pattern = "";
                    }
                    string val = "";
                    bool isAddParam = true;

                    if (Request.Unvalidated[item.Key] != null)
                    {
                        switch (item.Type.ToLower())
                        {
                            case "checkbox":
                                val = Request[item.Key].ToString();
                                if (val.ToLower() == "on" || val.ToLower() == "true" || val.ToLower() == "1")
                                {
                                    val = "1";
                                }
                                else
                                {
                                    val = "0";
                                }

                                break;
                            case "switch":
                                val = Request[item.Key].ToString();
                                if (val.ToLower() == "on" || val.ToLower() == "true" || val.ToLower() == "1")
                                {
                                    val = "1";
                                }
                                else
                                {
                                    val = "0";
                                }

                                break;
                            case "textbox":
                                switch (item.Pattern.ToLower())
                                {
                                    case "numberonly":
                                        val = (Request[item.Key].ToString().Replace(",", ""));
                                        break;
                                    case "number2":
                                        val = (Request[item.Key].ToString().Replace(",", ""));
                                        break;
                                    case "number4":
                                        val = (Request[item.Key].ToString().Replace(",", ""));
                                        break;
                                    case "percent":
                                        val = Request[item.Key].ToString().Replace(",", "").Replace("%", "");
                                        break;
                                    case "percent2":
                                        val = Request[item.Key].ToString().Replace(",", "").Replace("%", "");
                                        break;
                                    case "percent4":
                                        val = Request[item.Key].ToString().Replace(",", "").Replace("%", "");
                                        break;
                                    case "mobileno":
                                        val = Request[item.Key].ToString().Replace("-", "");
                                        break;
                                    case "jsfunction":
                                        val = Request[item.Key].ToString().Replace(",", "").Replace("%", "");
                                        break;
                                    default:
                                        if (item.Key.IndexOf("Display") >= 0)
                                        {
                                            val = (Request.Unvalidated[item.Key].ToString());
                                        }
                                        else
                                        {
                                            val = (Request[item.Key].ToString());
                                        }
                                        break;
                                }
                                break;
                            case "textonly":
                                switch (item.Pattern.ToLower())
                                {
                                    case "numberonly":
                                        val = (Request[item.Key].ToString().Replace(",", ""));
                                        break;
                                    case "number2":
                                        val = (Request[item.Key].ToString().Replace(",", ""));
                                        break;
                                    case "number4":
                                        val = (Request[item.Key].ToString().Replace(",", ""));
                                        break;
                                    case "percent":
                                        val = Request[item.Key].ToString().Replace(",", "").Replace("%", "");
                                        break;
                                    case "percent2":
                                        val = Request[item.Key].ToString().Replace(",", "").Replace("%", "");
                                        break;
                                    case "percent4":
                                        val = Request[item.Key].ToString().Replace(",", "").Replace("%", "");
                                        break;
                                    case "mobileno":
                                        val = Request[item.Key].ToString().Replace("-", "");
                                        break;
                                    case "jsfunction":
                                        val = Request[item.Key].ToString().Replace(",", "").Replace("%", "");
                                        break;
                                    default:
                                        if (item.Key.IndexOf("Display") >= 0)
                                        {
                                            val = (Request.Unvalidated[item.Key].ToString());
                                        }
                                        else
                                        {
                                            val = (Request[item.Key].ToString());
                                        }
                                        break;
                                }
                                break;
                            case "datetime":
                                switch (item.Pattern.ToLower())
                                {
                                    case "datepick":
                                        val = StaticFunc.ConvertToDateFromStringYYYYMMDD(Request[item.Key]).ToString("yyyy-MM-dd");

                                        break;
                                    case "datetimepick":
                                        val = StaticFunc.ConvertToDateFromStringYYYYMMDDhhmmss(Request[item.Key]).ToString("yyyy-MM-dd HH:mm:ss");

                                        break;
                                    default:
                                        val = (Request[item.Key].ToString());
                                        break;
                                }
                                break;
                            case "datefromto":
                                switch (item.Pattern.ToLower())
                                {
                                    case "datefrom":
                                        val = StaticFunc.ConvertToDateFromStringYYYYMMDD(Request[item.Key]).ToString("yyyy-MM-dd");

                                        break;
                                    case "dateto":
                                        val = StaticFunc.ConvertToDateFromStringYYYYMMDD(Request[item.Key]).ToString("yyyy-MM-dd");

                                        break;
                                    case "datetimefrom":
                                        val = StaticFunc.ConvertToDateFromStringYYYYMMDDhhmmss(Request[item.Key]).ToString("yyyy-MM-dd HH:mm:ss");

                                        break;
                                    case "datetimeto":
                                        val = StaticFunc.ConvertToDateFromStringYYYYMMDDhhmmss(Request[item.Key]).ToString("yyyy-MM-dd HH:mm:ss");

                                        break;
                                    case "timefrompick":
                                        val = Request[item.Key].ToString();

                                        break;
                                    case "timetopick":
                                        val = Request[item.Key].ToString();

                                        break;
                                    case "noenddate":
                                        val = Request[item.Key].ToString();
                                        if (val.ToLower() == "on" || val.ToLower() == "true" || val.ToLower() == "1")
                                        {
                                            val = "1";
                                        }
                                        else
                                        {
                                            val = "0";
                                        }

                                        break;
                                    default:
                                        val = StaticFunc.ConvertToDateFromStringYYYYMMDD(Request[item.Key]).ToString("yyyy-MM-dd");

                                        break;
                                }
                                break;
                            case "htmleditor":
                                string filePath = "";


                                System.IO.File.WriteAllText(Server.MapPath(filePath), Request.Unvalidated[item.Key].ToString());
                                if (item.Pattern == "HtmlDB")
                                {
                                    string filejs = "{\"filePath\":\"" + filePath + "\",";
                                    filejs += "\"fileContent\":" + JsonConvert.ToString(Request.Unvalidated[item.Key].ToString()) + "}";
                                    val = (filejs);
                                }
                                else val = (filePath);
                                break;
                            case "datareportedit":
                                string datastring = Request.Unvalidated[item.Key].ToString();
                                datastring.Replace("\'", "\'\'");
                                val = (datastring);
                                break;
                            case "itemsdetail":
                                string itemsdetail = Request.Unvalidated[item.Key].ToString();
                                itemsdetail.Replace("\'", "\'\'");
                                val = (itemsdetail);
                                break;
                            case "line":
                                isAddParam = false;
                                break;
                            default:
                                val = (Request[item.Key].ToString());
                                break;
                        }

                    }

                    if (isAddParam)
                    {
                        if (isJSData == true)
                        {
                            jsData.Add(new IDKeyValModel()
                            {
                                id = (idJsData++).ToString(),
                                key = item.Key,
                                val = val,
                                type = item.Type,
                                format = item.Pattern

                            });
                        }
                        else
                        {
                            pList.Add("@" + item.Key);
                            pList.Add(val);
                        }
                    }
                }
                if (jsData.Count > 0)
                {
                    pList.Add("@JsData");
                    pList.Add(JsonConvert.SerializeObject(jsData));
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }


        public CategoriesItemModel GetFormModelCache(string FormCode)
        {
            bool saveToCache = false;
            ObjectCache cache = MemoryCache.Default;
            CategoriesItemModel model = cache.Get(FormCode) as CategoriesItemModel;
            if (model == null && (FormCode.IndexOf("FAC") >= 0 || FormCode.IndexOf("WFL") >= 0))
            {
                model = ReaderTools.DeserializeJsonFileToObject<CategoriesItemModel>(Server.MapPath("/Files/Categories/" + FormCode + ".jconfig"));
                saveToCache = true;
            }
            else
            if (model == null && FormCode.IndexOf("RPT") >= 0)
            {
                model = ReaderTools.DeserializeJsonFileToObject<CategoriesItemModel>(Server.MapPath("/Files/SReports/" + FormCode + ".jconfig"));
                saveToCache = true;
            }
            else if (model == null)
            {
                model = ReaderTools.DeserializeJsonFileToObject<CategoriesItemModel>(Server.MapPath("/Files/Categories/" + FormCode + ".jconfig"));
                saveToCache = true;
            }
            if (saveToCache == true && !string.IsNullOrWhiteSpace(FormCode) && model != null)
            {
                CacheItemPolicy policy = new CacheItemPolicy
                {
                    //policy.SlidingExpiration = new TimeSpan(0,5,0);
                    AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(5)
                };
                cache.Set(FormCode, model, policy);
            }
            return model.CloneObject();

        }

        [HttpPost]
        public JsonResult MapDataCV()
        {
            var data = Request;
            var value1 = Request["cv"];
            var value2 = Request["formjs"];
            try
            {
                if (!string.IsNullOrEmpty(value1))
                {
                    var filecv = Server.MapPath("/Files/Recruitment/" + value1 + ".json");
                    string json = System.IO.File.ReadAllText(filecv);

                    List<FItem> Form = JsonConvert.DeserializeObject<List<FItem>>(value2);
                    if (json != null)
                    {
                        var datamap = ConvertDatajsonToFormItems(Form, json);
                        return Json(JsonConvert.SerializeObject(datamap), JsonRequestBehavior.AllowGet);
                    }

                }
                return Json(value2, JsonRequestBehavior.AllowGet);
            }

            catch (Exception ex)
            {
                DataFunction.WriteLog("ExportData", ex.Message);
                return Json("ERROR", JsonRequestBehavior.AllowGet);
            }
        }


        public List<FItem> ConvertDatajsonToFormItems(List<FItem> Form, string dataMap)
        {
            List<FItemValue> itemsValue = new List<FItemValue>();
            if (dataMap != null)
            {
                List<IDKeyValModel> jsDataList = new List<IDKeyValModel>();
                if (dataMap.Length>0)
                {
                    string jsData = dataMap.ToString();
                    if (jsData.Length > 0)
                        jsDataList = JsonConvert.DeserializeObject<List<IDKeyValModel>>(jsData);
                }
                else jsDataList = new List<IDKeyValModel>();

                if (Form != null)
                {
                    foreach (FItem item in Form)
                    {
                        string value = "";
                        string valueDisplay = "";
                        if (item.JsData == "True" && jsDataList.Count > 0)
                        {
                            IDKeyValModel jsDataItem = jsDataList.FirstOrDefault(m => m.key == item.Key);
                            IDKeyValModel jsDataItemDisplay = jsDataList.FirstOrDefault(m => m.key == item.Key + "Display");

                            if (jsDataItem != null)
                            {
                                value = jsDataItem.val;
                                valueDisplay = jsDataItemDisplay?.val;

                                item.Value = value;
                            }
                        }

                        //// add to list itemsValue
                        FItemValue newItem = new FItemValue
                        {
                            Key = item.Key,
                            Value = value,
                            Type = item.Type,
                            Pattern = item.Pattern,
                            OptionConfig = item.OptionConfig,
                            JsData = item.JsData,
                            ValueDisplay = valueDisplay

                        };
                        itemsValue.Add(newItem);
                    }
                }
            }
            return Form;
        }
    }
}