using System.Runtime.CompilerServices;
using EuterpeMilkyTitfuck;
using HarmonyLib;
using RimWorld;
using Verse;
using Verse.AI;

namespace Umrae.UsefulGenes;

public static class WallPolicy
{
    public static bool IsWallDef(BuildableDef def) => def != null &&
        (def == UsefulGeneDefOf.UG_FecalWall || def == UsefulGeneDefOf.UG_SmoothedFecalWall);

    public static bool IsFecalWall(Thing target) => target != null &&
        (IsWallDef(target.def) || IsWallDef(target.def.entityDefToBuild));

    public static bool Allowed(Thing target, Pawn pawn) => !IsFecalWall(target) ||
        (UsefulGeneDefOf.UG_FecalWalls != null && pawn?.genes?.HasActiveGene(UsefulGeneDefOf.UG_FecalWalls) == true);
}

[HarmonyPatch(typeof(GenConstruct), nameof(GenConstruct.CanConstruct),
    new[] { typeof(Thing), typeof(Pawn), typeof(bool), typeof(bool), typeof(JobDef) })]
public static class Patch_CanConstruct
{
    public static bool Prefix(Thing t, Pawn p, ref bool __result)
    {
        if (WallPolicy.Allowed(t, p)) return true;
        JobFailReason.Is("Requires the active Fecal walls gene.");
        __result = false;
        return false;
    }
}

[HarmonyPatch(typeof(Frame), nameof(Frame.CompleteConstruction))]
public static class Patch_CompleteConstruction
{
    public static bool Prefix(Frame __instance, Pawn worker) => WallPolicy.Allowed(__instance, worker);
}

[HarmonyPatch(typeof(HediffComp_Lactation), nameof(HediffComp_Lactation.RecacheStats))]
public static class Patch_LactationStats
{
    // A transient flag only restores cached stats when a gene is removed/suppressed.
    // Milk and settings continue to use the dependencies' native save formats.
    private static readonly ConditionalWeakTable<HediffComp_Lactation, object> scaled = new();

    public static void Postfix(HediffComp_Lactation __instance)
    {
        CoffeeSupport.ApplyStats(__instance);
        if (!LactationPolicy.HasGene(__instance.Pawn))
        {
            scaled.Remove(__instance);
            return;
        }
        scaled.GetValue(__instance, static _ => new object());
        var settings = UsefulGenesMod.Settings;
        float size = LactationPolicy.SizeFactor(__instance.Pawn);
        float factor = settings.productionMultiplier * size;
        if (factor > 0f)
        {
            float itemNutrition = StatUtility.GetStatValueFromList(
                LactationExpansion_ThingDefOf.SEX_BreastMilk.statBases, StatDefOf.Nutrition, 0.05f);
            // Even a very small carrier must be able to store one collectable milk item.
            __instance.maximumNutritionStored = Math.Max(Math.Max(0.05f, itemNutrition),
                __instance.maximumNutritionStored * factor);
            __instance.lactationPerDay *= factor;
        }
        else __instance.lactationPerDay = 0f;
        // The dependency's original per-tick value omits its speed setting.
        __instance.lactationPerTick = __instance.lactationPerDay / 60000f;
    }

    public static void RestoreIfNeeded(HediffComp_Lactation comp)
    {
        if (scaled.Remove(comp)) comp.RecacheStats();
    }
}

[HarmonyPatch(typeof(HediffComp_Lactation), nameof(HediffComp_Lactation.CompPostTickInterval))]
public static class Patch_LactationProduction
{
    public static bool Prefix(HediffComp_Lactation __instance, int delta)
    {
        Pawn pawn = __instance.Pawn;
        if (!LactationPolicy.HasGene(pawn))
        {
            Patch_LactationStats.RestoreIfNeeded(__instance);
            return true;
        }
        // Recache starts from the dependency's unmodified stats every time: never compound multipliers.
        __instance.RecacheStats();
        if (delta <= 0 || !LactationPolicy.Eligible(pawn)) return false;
        float requested = __instance.lactationPerTick * PawnUtility.BodyResourceGrowthSpeed(pawn) * delta;
        var food = pawn.needs.food;
        float cost = UsefulGenesMod.Settings.foodCostFraction;
        float added = MilkMath.Produced(__instance.currentNutritionStored, __instance.maximumNutritionStored,
            requested, food.CurLevel, cost);
        __instance.currentNutritionStored += added;
        if (added > 0f && cost > 0f) food.CurLevel -= added * cost;
        return false;
    }
}

[HarmonyPatch(typeof(HediffComp_LactationDecay), nameof(HediffComp_LactationDecay.SeverityChangePerDay))]
public static class Patch_LactationDecay
{
    public static void Postfix(HediffComp_LactationDecay __instance, ref float __result)
    {
        if (LactationPolicy.HasGene(__instance.Pawn) && LactationPolicy.Eligible(__instance.Pawn)) __result = 0f;
    }
}
