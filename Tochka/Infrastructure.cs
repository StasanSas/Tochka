namespace Tochka;

public static class Infrastructure
{
    public static string ChangeString(this string str, Char c, int indexChange)
    {
        return str.Substring(0, indexChange)
                            + c
                            + str.Substring(indexChange + 1);
    }
    
}