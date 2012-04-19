using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;

namespace ContentFinder.SupportSuffix
{
    abstract class BaseClass
    {
        virtual public string Suffix
        {
            get
            {
                return System.Reflection.MethodBase.GetCurrentMethod().ReflectedType.Name.Replace("Class", "").ToLowerInvariant();
            }
        }

        virtual public List<ContentUsage> GetAllContent(string fileName, string content)
        {
            string[] lines;
            List<ContentUsage> allFileList = new List<ContentUsage>();
            try
            {
                lines = File.ReadAllLines(fileName);
            }
            catch (Exception ex)
            {
                Log.Failure(ex.Message + Environment.NewLine + ex.StackTrace);
                return allFileList;
            }
            for (int i = 0; i < lines.Length; i++)
            {
                if (Regex.IsMatch(lines[i], content.ToString(), RegexOptions.Compiled))
                {
                    allFileList.Add(new ContentUsage(lines[i].Trim(), fileName, i));
                }
            }
            return allFileList;
        }

        virtual public void OpenFile(string fileName)
        {
            Process.Start("notepad", fileName);
        }
    }
}
