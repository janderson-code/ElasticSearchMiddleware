using System.Text.RegularExpressions;

namespace elasticsearch.Utils;

public static class DocumentFormat
{
    public static string FormatDoc(string doc)
    {
        if (doc.Length.Equals(11))
            return Regex.Replace(doc, "([0-9]{3})([0-9]{3})([0-9]{3})([0-9]{2})", "$1.*.*.*-$4");

        if (doc.Length.Equals(14))
            return Regex.Replace(doc, "([0-9]{3})([0-9]{3})([0-9]{5})([0-9]{3})", "$1.*.*.*-$4");

        return doc;
    }
}