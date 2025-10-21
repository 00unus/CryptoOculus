namespace CryptoOculus
{
    public static class StringExtensions
    {
        public static string? NullIfWhiteSpace(this string? str)
        {
            return String.IsNullOrWhiteSpace(str) ? null : str;
        }
    }
}
