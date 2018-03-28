using System.IO;

namespace OntologyXmlParser
{
    public static class GlobalVars
    {
        public static string url = "https://api.arca.acacia.red";
        //public static string url = "http://127.0.0.1:5904";

        public static string watcherFolder = Directory.GetParent(Directory.GetCurrentDirectory()).ToString();
    }
}