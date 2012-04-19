using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Text.RegularExpressions;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

namespace ContentFinder.SupportSuffix
{
    class XLSClass : BaseClass
    {
        override public List<ContentUsage> GetAllContent(string fileName, string content)
        {
            List<string> allText = new List<string>();
            List<ContentUsage> allFileList = new List<ContentUsage>();
            try
            {
                // Open the document for editing.
                using (SpreadsheetDocument spreadsheetDocument = SpreadsheetDocument.Open(fileName, false))
                {
                    WorkbookPart workbookPart = spreadsheetDocument.WorkbookPart;
                    IEnumerable<WorksheetPart> allWorkbookSheets = workbookPart.WorksheetParts;
                    foreach (WorksheetPart worksheet in allWorkbookSheets)
                    {
                        IEnumerable<Row> rows = worksheet.Worksheet.Descendants<Row>();
                        foreach (Row row in rows)
                        {
                            string result = string.Empty;
                            foreach (Cell cell in row)
                            {
                                //get cell value
                                String value = cell.CellValue.InnerText;

                                //Look up real value from shared string table
                                if ((cell.DataType != null) && (cell.DataType == CellValues.SharedString))
                                    result += workbookPart.SharedStringTablePart.SharedStringTable
                                    .ChildElements[Int32.Parse(value)]
                                    .InnerText;

                            }
                            allText.Add(result + Environment.NewLine);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Failure(ex.Message + Environment.NewLine + ex.StackTrace);
                return allFileList;
            }

            for (int i = 0; i < allText.Count; i++)
            {
                if (Regex.IsMatch(allText[i], content.ToString(), RegexOptions.Compiled | RegexOptions.IgnoreCase))
                {
                    allFileList.Add(new ContentUsage(allText[i].Trim(), fileName, i + 1));
                }
            }
            return allFileList;
        }

        override public void OpenFile(string fileName)
        {
            Process.Start(fileName);
        }
    }
}
