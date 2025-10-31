using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Database.Runtime.Utils
{
    public static class XmlLoader
    {
        public static Dictionary<string, string> Load(string path)
        {
            var doc = XDocument.Load(path);
            /*
             * <root>
             *      <key1>value</key1>
             *      <key2>value</key2>
             *  </root>
             */
            return doc.Root.Elements()
                .ToDictionary(
                // <key>value</key>
                    k => k.Name.LocalName,
                    v => v.Value
                );
        }
    }
}