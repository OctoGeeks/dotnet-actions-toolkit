namespace DotnetActionsToolkit
{
    internal static class Utils
    {
        internal static string ToCommandValue<T>(this T input)
        {
            if (input == null)
            {
                return string.Empty;
            }

            // TODO: probably better to jsonify it if its anything other than a string
            return input.ToString();
        }
    }
}
