using System.Text;

namespace GameOfHearts.LoggingProvider.Extensions;

public static class EnumerableExtensions
{
    public static string Stringify<T>(this IEnumerable<T> enumerable)
    {
        StringBuilder stringBuilder = new();
        for (int i = 0; i < enumerable.Count(); i++)
        {
            stringBuilder.Append(enumerable.ElementAt(i));
            if (i < enumerable.Count() - 1)
            {
                stringBuilder.Append(", ");
            }
        }
        return stringBuilder.ToString();
    }
}
