using UnityEngine;
using System.Collections.Generic;

public class WeightedRandomPicker<T>
{
    private readonly List<T> items;
    private readonly List<float> cumulativeWeights;
    private readonly float totalWeight;
    private readonly bool useUniformDistribution;
    private System.Random random;

    public WeightedRandomPicker(Dictionary<T, float> itemsWithWeights)
    {
        items = new List<T>();
        cumulativeWeights = new List<float>();
        float cumulative = 0;

        foreach (var item in itemsWithWeights)
        {
            items.Add(item.Key);
            cumulative += item.Value;
            cumulativeWeights.Add(cumulative);
        }

        totalWeight = cumulative;
        random = new System.Random();

        // If all weights are zero, we use uniform distribution
        useUniformDistribution = totalWeight == 0;

        if (items.Count == 0)
        {
            throw new System.ArgumentException("Cannot initialize WeightedRandomPicker with an empty list.");
        }
    }


    public T GetRandomItem(System.Random randomSource)
    {
        if (useUniformDistribution)
        {
            int index = randomSource.Next(items.Count);
            return items[index];
        }

        double randomValue = randomSource.NextDouble() * totalWeight;

        for (int i = 0; i < cumulativeWeights.Count; i++)
        {
            if (randomValue < cumulativeWeights[i])
            {
                return items[i];
            }
        }

        throw new System.InvalidOperationException("Failed to select a random item based on weights.");
    }

    public T GetRandomItem()
    {
        return GetRandomItem(this.random);
    }
}