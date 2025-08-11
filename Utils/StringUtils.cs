namespace FC2Editor.Utils
{
    internal static class StringUtils
    {
        public static string EscapeUIString(string s)
        {
            return s.Replace("&", "&&");
        }
    }
}