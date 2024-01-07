using GameOfHearts.ArtificialPlayer.Models;

namespace GameOfHearts.ArtificialPlayer.Utilities;

public static class DecisionFilterUtilities
{
    public static List<Decision<T>> FilterOptimal<T>(
        Dictionary<Decision<T>, int> decisions) where T :Enum
    {
        List<Decision<T>> optimal = new();
        int? maxUtility = null;

        foreach (var decision in decisions)
        {
            if (maxUtility is null)
            {
                // first iteration, set initial values
                maxUtility = decision.Value;
                optimal.Add(decision.Key);
                continue;
            }

            if (decision.Value > maxUtility)
            {
                // found better decision, clear old and add new
                optimal.Clear();
                optimal.Add(decision.Key);
                maxUtility = decision.Value;
                continue;
            }

            if (decision.Value == maxUtility)
            {
                // multiple optimal decisions, add new one to list
                optimal.Add(decision.Key);
                continue;
            }
        }

        return optimal;
    }
}
