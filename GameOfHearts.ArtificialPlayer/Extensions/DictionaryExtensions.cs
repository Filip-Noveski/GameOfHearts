namespace GameOfHearts.ArtificialPlayer.Extensions;

public static class DictionaryExtensions
{
    public static void AddRange<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, Dictionary<TKey, TValue> range)
        where TKey : notnull
    {
        foreach (var pair in range)
        {
            dictionary.Add(pair.Key, pair.Value);
        }
    }
}
