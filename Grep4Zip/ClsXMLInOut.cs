using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Grep4Zip
{ 
    class XMLINOUT
    {
        public const string ROOTKEY = "Display";
        public const string ELEMENTKEY = "Initialize";
        public const string ITEM_KEY = "key";
        public const string ITEM_FOLDER = "folder";
        public const string ITEM_SUBFOLDER = "Subfolder";
        public const string ITEM_EXTENSION = "extension";

        private string mstrFPath;

        public string strKey { get; set; }
        public string strFolder { get; set; }
        public Boolean blnSubFolder { get; set; }
        public string strExtension { get; set; }

        public XMLINOUT(string FPath)
        {
            mstrFPath = FPath;
        }

        public Boolean ReadData()
        {
            Boolean blnRet = false;

            try
            {
                XDocument mXML = XDocument.Load(mstrFPath);
                XElement mElement = mXML.Element(ROOTKEY);

                var items = mElement.Elements(ELEMENTKEY);
                foreach(XElement item in items)
                {
                    strKey = item.Element(ITEM_KEY).Value;
                    strFolder = item.Element(ITEM_FOLDER).Value;
                    if (item.Element(ITEM_SUBFOLDER).Value != "")
                    {
                        blnSubFolder = Convert.ToBoolean(item.Element(ITEM_SUBFOLDER).Value);
                    }
                    strExtension = item.Element(ITEM_EXTENSION).Value;
                }

                blnRet = true;
            }
            catch
            {
                blnRet = false;
            }

            return blnRet;
        }

        public Boolean WriteXML(string key,string folder,Boolean Subfolder,string extension)
        {
            try
            {
                XDocument mXML = XDocument.Load(mstrFPath);
                XElement mElement = mXML.Element(ROOTKEY);
                XElement items = (from item in mElement.Elements(ELEMENTKEY)
                                     where item.Element(ITEM_KEY).Value == strKey
                                     select item).Single();

                items.Element(ITEM_KEY).Value = key;
                items.Element(ITEM_FOLDER).Value = folder;
                items.Element(ITEM_SUBFOLDER).Value = Convert.ToString(Subfolder);
                items.Element(ITEM_EXTENSION).Value = extension;

                mXML.Save(mstrFPath);

                return true;

            }catch
            {
                return false;
            }
        }
    }
}
