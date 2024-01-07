namespace GameOfHearts.ArtificialPlayer.Extensions;

public static class ListExtensions
{
    public static int Count<T>(this List<T> list, Predicate<T> predicate)
    {
        int count = 0;

        foreach (T item in list)
        {
            if (predicate(item))
                count++;
        }

        return count;
    }
}
