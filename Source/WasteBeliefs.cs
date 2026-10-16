using System.Xml;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Umrae.UsefulGenes;

public sealed class WasteBeliefExtension : DefModExtension
{
    public string resource;
    public ThoughtDef thought;
}

public static class WasteBeliefs
{
    public static void Apply(List<FoodUtility.ThoughtFromIngesting> thoughts, IEnumerable<Precept> precepts, string resource)
    {
        var precept = precepts?.FirstOrDefault(p => p.def.GetModExtension<WasteBeliefExtension>()?.resource == resource);
        var rule = precept?.def.GetModExtension<WasteBeliefExtension>();
        if (rule == null) return;
        string original = resource == "Feces" ? "AteFeces" : "DrankUrine";
        thoughts.RemoveAll(t => t.thought?.defName == original);
        if (rule.thought != null && !thoughts.Any(t => t.thought == rule.thought && t.fromPrecept == precept))
            thoughts.Add(new FoodUtility.ThoughtFromIngesting { thought = rule.thought, fromPrecept = precept });
    }
}

[HarmonyPatch(typeof(FoodUtility), nameof(FoodUtility.ThoughtsFromIngesting))]
public static class Patch_FluidFoodThoughts
{
    public static void Postfix(Pawn ingester, Thing foodSource, ThingDef foodDef, ref List<FoodUtility.ThoughtFromIngesting> __result)
    {
        var precepts = ingester?.Ideo?.PreceptsListForReading;
        if (precepts == null || __result == null) return;
        var ingredients = foodSource?.TryGetComp<CompIngredients>()?.ingredients;
        foreach (string resource in new[] { "Feces", "UrineBottle" })
            if (foodDef?.defName == resource || ingredients?.Any(i => i.defName == resource) == true)
                WasteBeliefs.Apply(__result, precepts, resource);
        if (CoffeeSupport.NativeModule) return;
        string historyEvent = CoffeeSupport.IsMilk(foodDef) ? "DrankMilkRaw" :
            CoffeeSupport.ContainsMilk(foodSource, foodDef) ? "DrankMilkMeal" : "DrankNonMilkMeal";
        CoffeeSupport.AddEventThoughts(__result, precepts, historyEvent);
    }
}

[HarmonyPatch(typeof(FoodUtility), nameof(FoodUtility.AddFoodPoisoningHediff))]
public static class Patch_FecesPoisoning
{
    public static bool Prefix(Thing ingestible) => ingestible?.def?.defName != "Feces";
}

[HarmonyPatch(typeof(BackCompatibility), nameof(BackCompatibility.BackCompatibleDefName))]
public static class Patch_LegacyFecalWall
{
    public static void Prefix(Type defType, ref string defName)
    {
        if (defType == typeof(ThingDef)) defName = Migrate(defName);
    }
    public static string Migrate(string name) => name switch
    {
        "FecalWall" => "UG_FecalWall",
        "Blueprint_FecalWall" => "Blueprint_UG_FecalWall",
        "Frame_FecalWall" => "Frame_UG_FecalWall",
        "FecalWall_Blueprint" => "UG_FecalWall_Blueprint",
        "FecalWall_Frame" => "UG_FecalWall_Frame",
        _ => name
    };
}
