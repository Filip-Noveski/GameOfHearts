using System.Text;

namespace GameOfHearts.LoggingProvider.Extensions;

public static class ReadOnlySpanExtensions
{
    public static string Stringify<T>(this ReadOnlySpan<T> array)
    {
        StringBuilder stringBuilder = new();
        for (int i = 0; i < array.Length; i++)
        {
            stringBuilder.Append(array[i]);
            if (i < array.Length - 1)
            {
                stringBuilder.Append(", ");
            }
        }
        return stringBuilder.ToString();
    }
}
