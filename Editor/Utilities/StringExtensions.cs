using System.Text;

namespace NPTP.ReferenceableScriptables.Editor.Utilities
{
    public static class StringExtensions
    {
        public static string AsInspectorLabel(this string s) => s.SpaceBetweenWords().CapitalizeFirst();
        
        public static string SpaceBetweenWords(this string s)
        {
            StringBuilder sb = new();
            
            for (int i = 0; i < s.Length; i++)
            {
                char cur = s[i];
                if (char.IsWhiteSpace(cur))
                {
                    continue;
                }
                
                if (i > 0)
                {
                    char prev = s[i - 1];
                    bool newWordCondition = char.IsLower(prev) && char.IsUpper(cur);
                    bool numberStartCondition = char.IsLetter(prev) && char.IsNumber(cur);
                    if (newWordCondition || numberStartCondition)
                    {
                        sb.Append(' ');
                    }
                }
                sb.Append(cur);
            }

            return sb.ToString();
        }
        
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