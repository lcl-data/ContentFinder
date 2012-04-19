using System;
using System.IO;

namespace ContentFinder
{
    static class Log
    {
        public static void Message(string cueString, bool fromBegin = false)
        {
            string filename = Path.Combine(Directory.GetCurrentDirectory() + @"\Results.log");

            FileInfo info = new FileInfo(filename);
            if (fromBegin)
            {
                File.Delete(filename);
            }

            if (!info.Exists)
            {
                FileStream fileStream = null;
                try
                {
                    fileStream = info.Create();
                    using (StreamWriter writer = new StreamWriter(fileStream))
                    {
                        fileStream = null;
                        writer.BaseStream.Seek(0, SeekOrigin.End);
                        writer.WriteLine("{0}", cueString);                       
                    }
                }
                finally
                {
                    if (fileStream != null)
                    {
                        fileStream.Dispose();
                    }
                }
              
            }
            else
            {
                FileStream fileStream = null;
               try
               {
                   fileStream = info.OpenWrite();

                   using (StreamWriter writer = new StreamWriter(fileStream))
                   {
                       fileStream = null;
                       writer.BaseStream.Seek(0, SeekOrigin.End);
                       writer.WriteLine("{0}", cueString);
                   }
               }
               finally
               {
                   if (fileStream != null)
                   {
                       fileStream.Dispose();
                   }
               }
            }
        }

        public static void Failure(string cueString)
        {
            Message(String.Format("Failure : {0}", cueString));
            if (Settings.Instance.ShowWarning)
            {
                ErrorDialog.Show(cueString);
            }
        }

        public static void Clear()
        {
            Message("", true);
        }
    }
}
