using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using Aspose.Cells;
using System.IO;
using Scb.Framework;

namespace GenerateLayout
{
  public partial class Form1 : Form
  {
    public Form1()
    {
      InitializeComponent();
    }

    private void Form1_Load(object sender, EventArgs e)
    {
      int curRow = 0;
      try
      {
        DataTable dt = GetDataFromXLS(@"C:\Users\1545105\Desktop\SAVE\LITS\Auto_Loan\Form Field 18Jan.xlsx", "Form1&2A_SalesCoor_maker", true);

        if (File.Exists(Application.StartupPath + "\\Output.txt"))
          File.Delete(Application.StartupPath + "\\Output.txt");

        if (dt != null && dt.Rows.Count > 1)
        {
          StringBuilder strBuilder;
          foreach (DataRow row in dt.Rows)
          {
            curRow++;
            if (curRow == 34)
            {             }
            if (row[(int)ColumnIndex.Name] != null && row[(int)ColumnIndex.Name].ToString() != "Field"
              && row[(int)ColumnIndex.Mandatory] != null && row[(int)ColumnIndex.Mandatory].ToString() != "Mandatory"
              && row[(int)ColumnIndex.Editable] != null && row[(int)ColumnIndex.Editable].ToString() != "Editable"
              && row[(int)ColumnIndex.FormatNumber] != null && row[(int)ColumnIndex.FormatNumber].ToString() != "Format"
              && row[(int)ColumnIndex.Type] != null && row[(int)ColumnIndex.Type].ToString() != "Type"
              && !string.IsNullOrEmpty(row[(int)ColumnIndex.Type].ToString())
              && !string.IsNullOrEmpty(row[(int)ColumnIndex.Mandatory].ToString())
              && !string.IsNullOrEmpty(row[(int)ColumnIndex.Editable].ToString())
              && !string.IsNullOrEmpty(row[(int)ColumnIndex.FormatNumber].ToString()))
            {
              strBuilder = new StringBuilder();

              strBuilder.Append(GenFieldName(row[(int)ColumnIndex.Name].ToString()));

              string filePath = GetTemplateItemByType(row[(int)ColumnIndex.Type].ToString());
              if (!string.IsNullOrEmpty(filePath))
              {
                string temp = File.ReadAllText(filePath);
                strBuilder.Append("<td>" + temp.Replace("{Name}", RemoveSpecialChars(row[(int)ColumnIndex.ControlName].ToString())).Replace("{Mandatory}",
                    MandatoryProperty(row[(int)ColumnIndex.Mandatory].ToString())).Replace("{Editable}",
                    EditableProperty(row[(int)ColumnIndex.Editable].ToString())).Replace("{FormatNumber}",
                    FormatNumberProperty(row[(int)ColumnIndex.FormatNumber].ToString())) + "</td>");
                strBuilder.Append(Environment.NewLine);

                File.AppendAllText(Application.StartupPath + "\\Output.txt", strBuilder.ToString());
              }
            }
          }
        }
      }
      catch (Exception ex)
      {
        string err = String.Format("Error at row {0}::{1}", curRow, ex.ToString());
        Logger.Error(err);
        throw ex;
      }
    }

    private DataTable GetDataFromXLS(string path, string sheet, bool isHeader)
    {
      try
      {
        //Datatable Object
        DataTable dt = new DataTable();

        //Instantiating an Workbook object
        Workbook workbook = new Workbook();

        //Creating a file stream containing the Excel file to be opened
        FileStream fstream = new FileStream(path, FileMode.Open);

        //Opening the Excel file through the file stream
        workbook.Open(fstream);

        //Accessing the first worksheet in the Excel file
        Worksheet worksheet = workbook.Worksheets[sheet];

        //Exporting the contents of x rows and x columns starting from 1st cell to DataTable
        dt = worksheet.Cells.ExportDataTable(0, 0, worksheet.Cells.MaxDataRow + 1, worksheet.Cells.MaxDataColumn + 1, isHeader);

        //Closing the file stream to free all resources
        fstream.Close();

        //return result
        return dt;
      }
      catch (Exception ex)
      {
        Logger.Error("GetDataFromXLS::", ex);
        throw ex;
      }
    }

    private string RemoveSpecialChars(string str)
    {
      str = UppercaseFirstEach(str);

      // Create  a string array and add the special characters you want to remove
      string[] chars = new string[] { ",", ".", "/", "!", "@", "#", "$", "%", "^", "&", "*", "'", "\"", ";", "_", "(", ")", ":", "|", "[", "]", " ", "-", "?" };
      //Iterate the number of times based on the String array length.

      for (int i = 0; i < chars.Length; i++)
        if (str.Contains(chars[i]))
          str = str.Replace(chars[i], "");

      return str;
    }

    private string MandatoryProperty(string s)
    {
      if (s.ToLower() == "yes")
        return "BackColor=\"#EEF7DE\"";

      return string.Empty;
    }

    private string EditableProperty(string s)
    {
      if (s.ToLower() == "no")
        return "ReadOnly=\"True\"";

      return "";
    }

    private string FormatNumberProperty(string s)
    {
      if (s.ToLower() == "number")
        return "<MaskSettings Mask=\"&lt;0..999999999999999999g&gt;.&lt;00..99&gt;\"></MaskSettings>";

      return "";
    }

    private string UppercaseFirstEach(string s)
    {
      char[] a = s.ToCharArray();

      for (int i = 0; i < a.Length; i++)
      {
        a[i] = i == 0 || a[i - 1] == ' ' ? char.ToUpper(a[i]) : a[i];
      }

      return new string(a);
    }

    private string GenFieldName(string fieldName)
    {
      return String.Format("<td>{0}</td>", UppercaseFirstEach(fieldName));
    }

    private string GetTemplateItemByType(string type)
    {
      string result;
      string filePath = Application.StartupPath + "\\" + "Templates" + "\\";

      switch (type.ToLower().Replace(" ", ""))
      {
        case "textbox":
          result = filePath + "Textbox.htm";
          break;
        case "dropbox":
          result = filePath + "Dropbox.htm";
          break;
        case "label":
          result = filePath + "Label.htm";
          break;
        case "calbox":
          result = filePath + "DateTime.htm";
          break;
        default:
          result = string.Empty;
          break;
      }

      return result;
    }
  }

  public enum ColumnIndex
  {
    Name = 1,
    Type = 5,
    Mandatory = 6,
    FormatNumber = 7,
    Editable = 8,
    ControlName = 3
  }
}