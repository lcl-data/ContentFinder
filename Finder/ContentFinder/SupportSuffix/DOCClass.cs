using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.Diagnostics;
using DocumentFormat.OpenXml.Packaging;
using System.Xml;
using DocumentFormat.OpenXml.Wordprocessing;

namespace ContentFinder.SupportSuffix
{
    class DOCClass : BaseClass
    {
        override public List<ContentUsage> GetAllContent(string fileName, string content)
        {
            List<string> allText = new List<string>();
            List<ContentUsage> allFileList = new List<ContentUsage>();
            try
            {
                string schemaUri = @"http://schemas.openxmlformats.org/";
                string wordmlNamespace = schemaUri + @"wordprocessingml/2006/main";
                string relationshipNamespace = schemaUri + @"officeDocument/2006/relationships";
                using (WordprocessingDocument wdDoc = WordprocessingDocument.Open(fileName, true))
                {
                    // Get the main document part.
                    // http://msdn.microsoft.com/en-us/library/cc850833.aspx
                    MainDocumentPart mainDocPart = wdDoc.MainDocumentPart;

                    #region Find the document's paragraph property nodes
                    mainDocPart.Document.Body.ToList().ForEach(it => allText.Add(it.InnerText + Environment.NewLine));
                    #endregion
                    #region Find the document's header property nodes
                    mainDocPart.HeaderParts.ToList().ForEach(it => allText.Add(it.Header.InnerText + Environment.NewLine));
                    #endregion

                    #region Find the document's footer property nodes
                    mainDocPart.FooterParts.ToList().ForEach(it => allText.Add(it.Footer.InnerText + Environment.NewLine));
                    #endregion

                    #region Find the document's comment property nodes
                    WordprocessingCommentsPart comments = mainDocPart.WordprocessingCommentsPart;
                    if (comments != null)
                    {
                        comments.Comments.ToList().ForEach(it => allText.Add(it.InnerText + Environment.NewLine));
                    }
                    #endregion

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
                    allFileList.Add(new ContentUsage(allText[i].Trim(), fileName, i));
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
