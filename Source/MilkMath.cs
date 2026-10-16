namespace Umrae.UsefulGenes;

// Pure accounting shared by the live production loop and the regression tests.
public static class MilkMath
{
    public static float Produced(float stored, float capacity, float requested, float availableFood, float foodCost)
    {
        if (!float.IsFinite(stored) || !float.IsFinite(capacity) || !float.IsFinite(requested) ||
            !float.IsFinite(availableFood) || !float.IsFinite(foodCost) || requested <= 0f || foodCost < 0f)
            return 0f;
        float added = Math.Min(Math.Max(0f, capacity - stored), requested);
        if (foodCost > 0f) added = Math.Min(added, Math.Max(0f, availableFood) / foodCost);
        return Math.Max(0f, added);
    }
}
