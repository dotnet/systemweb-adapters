namespace System.Web.Util;

internal static class StringUtil
{
    /*
     * Determines if the first string starts with the second string, ignoring case.
     * Fast, non-culture aware.  
     */
    internal static bool StringStartsWithIgnoreCase(string s1, string s2)
    {
        if (string.IsNullOrEmpty(s1) || string.IsNullOrEmpty(s2)) return false;
        if (s2.Length > s1.Length) return false;
        return s1.StartsWith(s2, StringComparison.OrdinalIgnoreCase);
    }
}
