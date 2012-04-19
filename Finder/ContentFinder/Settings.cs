using System;
using System.Configuration;
using System.IO;
using System.Xml;

namespace ContentFinder
{
    public class Settings
    {
        public string SearchFolder { get; set; }
        public string FileFilter { get; set; }
        public bool ShowWarning { get; set; }

        private static Settings mInstance = new Settings();
        public static Settings Instance
        {
            get
            {
                return mInstance;
            }
        }

        public static bool FileExists
        {
            get
            {
                return File.Exists(AppConfig);
            }
        }

        private static string _appconfig = "App.config";

        public static string AppConfig
        {
            get
            {
                if (!Path.IsPathRooted(_appconfig))
                {
                    _appconfig = Path.Combine(System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase, _appconfig);
                }

                return _appconfig;
            }
            set
            {
                _appconfig = value;
            }

        }

        public void ReadFromFile()
        {
            if (!FileExists)
            {
                throw new Exception();
            }

            SearchFolder = GetSettingFromAppConfig("XMLFileDir");
            FileFilter = GetSettingFromAppConfig("FileFilter");
            ShowWarning = Convert.ToBoolean(GetSettingFromAppConfig("ShowWarning"));
        }

        public void SaveToFile()
        {
            SetSettingToAppConfig("XMLFileDir", SearchFolder);
            SetSettingToAppConfig("FileFilter", FileFilter);
            SetSettingToAppConfig("ShowWarning", ShowWarning.ToString());
        }

        public bool Validate()
        {
            foreach (var item in SearchFolder.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries))
            {
                if (!Directory.Exists(item))
                    return false;
            }
            return true;
        }
        private string GetSettingFromAppConfig(string key)
        {
            string returnValue = "";
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException(key);
            }
            else
            {
                key = key.Trim();
            }

            if (!FileExists)
            {
                throw new DirectoryNotFoundException();
            }
            File.SetAttributes(AppConfig, FileAttributes.Normal);
            XmlDocument xmldoc = new XmlDocument();
            xmldoc.Load(AppConfig);
            XmlNodeList xmllst = xmldoc.SelectNodes("/configuration/appSettings/add");
            if (xmldoc.SelectSingleNode("/configuration/appSettings") == null)
            {
                throw new Exception("Can't find the node /configuration/appSettings");
            }
            else
            {
                bool existed = false;
                foreach (XmlNode n1 in xmllst)
                {
                    if (n1.Attributes["key"].Value.ToUpper() == key.ToUpper())
                    {
                        returnValue = n1.Attributes["value"].Value;
                        existed = true;
                        break;
                    }
                }
                if (!existed)
                {
                    throw new Exception(string.Format("Can't find the key {0}", key));
                }
            }
            return returnValue;
        }

        private void SetSettingToAppConfig(string key, string value)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentNullException(key);
            }
            else
            {
                key = key.Trim();
            }
            if (string.IsNullOrEmpty(value))
                value = "";
            else
                value = value.Trim();

            if (!FileExists)
            {
                throw new DirectoryNotFoundException();
            }
            File.SetAttributes(AppConfig, FileAttributes.Normal);
            XmlDocument xmldoc = new XmlDocument();
            xmldoc.Load(AppConfig);
            XmlNodeList xmllst = xmldoc.SelectNodes("/configuration/appSettings/add");
            if (xmldoc.SelectSingleNode("/configuration/appSettings") == null)
            {
                XmlNode n2 = xmldoc.CreateNode("element", "appSettings", "");
                n2.InnerXml = "<add key=\"" + key + "\" value=\"" + value + "\"/>";
                xmldoc.SelectSingleNode("/configuration").AppendChild(n2);
                xmldoc.Save(AppConfig);
            }
            else if (xmllst.Count == 0)
            {
                XmlNode n2 = xmldoc.CreateNode("element", "add", "");
                XmlAttribute xa = xmldoc.CreateAttribute("key");
                xa.Value = key;
                n2.Attributes.Append(xa);
                xa = xmldoc.CreateAttribute("value");
                xa.Value = value;
                n2.Attributes.Append(xa);
                xmldoc.SelectSingleNode("/configuration/appSettings").AppendChild(n2);
                xmldoc.Save(AppConfig);
            }
            else
            {
                bool existed = false;
                foreach (XmlNode n1 in xmllst)
                {
                    if (n1.Attributes["key"].Value.ToUpper() == key.ToUpper())
                    {
                        n1.Attributes["value"].Value = value;
                        xmldoc.Save(AppConfig);
                        existed = true;
                        break;
                    }
                }
                if (!existed)
                {
                    XmlNode xmlnd = xmldoc.SelectSingleNode("/configuration/appSettings");
                    XmlNode n2 = xmldoc.CreateNode("element", "add", "");
                    XmlAttribute xa = xmldoc.CreateAttribute("key");
                    xa.Value = key;
                    n2.Attributes.Append(xa);
                    xa = xmldoc.CreateAttribute("value");
                    xa.Value = value;
                    n2.Attributes.Append(xa);
                    xmlnd.AppendChild(n2);
                    xmldoc.Save(AppConfig);
                }
            }
            ConfigurationManager.RefreshSection("appSettings");
        }


    }
}
