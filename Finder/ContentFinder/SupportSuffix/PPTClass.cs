using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Text.RegularExpressions;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Presentation;
using DocumentFormat.OpenXml;

namespace ContentFinder.SupportSuffix
{
    class PPTClass : BaseClass
    {
        override public List<ContentUsage> GetAllContent(string fileName, string content)
        {
            List<string> allText = new List<string>();
            List<ContentUsage> allFileList = new List<ContentUsage>();
            try
            {
                int numberOfSlides = CountSlides(fileName);
                string slideText;
                for (int i = 0; i < numberOfSlides; i++)
                {
                    GetSlideIdAndText(out slideText, fileName, i);
                    allText.Add(slideText);
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

        private int CountSlides(string presentationFile)
        {
            int slidesCount = 0;
            // Open the presentation as read-only.
            using (PresentationDocument presentationDocument = PresentationDocument.Open(presentationFile, false))
            {
                // Get the presentation part of document.
                PresentationPart presentationPart = presentationDocument.PresentationPart;
                // Get the slide count from the SlideParts.
                if (presentationPart != null)
                {
                    slidesCount = presentationPart.SlideParts.Count();
                }
                // Return the slide count to the previous method.
            }
            return slidesCount;
        }

        public void GetSlideIdAndText(out string sldText, string docName, int index)
        {
            using (PresentationDocument ppt = PresentationDocument.Open(docName, false))
            {
                // Get the relationship ID of the first slide.
                PresentationPart part = ppt.PresentationPart;
                OpenXmlElementList slideIds = part.Presentation.SlideIdList.ChildElements;

                string relId = (slideIds[index] as SlideId).RelationshipId;

                // Get the slide part from the relationship ID.
                SlidePart slide = (SlidePart)part.GetPartById(relId);

                // Build a StringBuilder object.
                StringBuilder paragraphText = new StringBuilder();

                // Get the inner text of the slide:
                IEnumerable<DocumentFormat.OpenXml.Drawing.Text> texts = slide.Slide.Descendants<DocumentFormat.OpenXml.Drawing.Text>();
                foreach (DocumentFormat.OpenXml.Drawing.Text text in texts)
                {
                    paragraphText.Append(text.Text);
                }
                sldText = paragraphText.ToString();
            }
        }
    }
}
