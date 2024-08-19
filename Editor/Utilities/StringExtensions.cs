namespace NPTP.ReferenceableScriptables.Editor.Utilities
{
    public static class StringExtensions
    {
        public static string CapitalizeFirst(this string s)
        {
            if (string.IsNullOrEmpty(s))
                return string.Empty;
            
            char firstChar = char.ToUpper(s[0]);
            if (s.Length == 1) return firstChar.ToString();
            return  firstChar + s[1..];
        }
    }
}