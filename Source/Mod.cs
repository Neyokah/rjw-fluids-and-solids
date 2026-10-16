using HarmonyLib;
using RimWorld;
using UnityEngine;
using Verse;

namespace Umrae.UsefulGenes;

public sealed class UsefulGenesMod : Mod
{
    public static UsefulGenesSettings Settings = new();

    public UsefulGenesMod(ModContentPack content) : base(content)
    {
        Settings = GetSettings<UsefulGenesSettings>();
        Settings.Sanitize();
        CoffeeSupport.EnableInstalledModule(content);
    }

    public override string SettingsCategory() => "RJW Fluids and Solids";

    public override void DoSettingsWindowContents(Rect inRect)
    {
        var list = new Listing_Standard();
        list.Begin(inRect);
        list.Label($"Milk production at average RJW size: {Settings.productionMultiplier:0.0}× normal");
        Settings.productionMultiplier = Mathf.Round(list.Slider(Settings.productionMultiplier, 0.1f, 20f) * 10f) / 10f;
        list.Label("Larger and additional breasts increase production and storage. Other lactation modifiers still apply.");
        list.Gap();
        list.Label($"Food cost: {Settings.foodCostFraction:P0} of the nutrition actually stored as milk");
        Settings.foodCostFraction = Mathf.Round(list.Slider(Settings.foodCostFraction, 0f, 2f) * 100f) / 100f;
        list.Label("50% means 1 nutrition of food produces 2 nutrition of milk. Full storage costs no food. Malnutrition pauses production.");
        list.Gap();
        list.Label($"Feces throwing range: {Settings.throwRange:0} tiles");
        Settings.throwRange = Mathf.Round(list.Slider(Settings.throwRange, 1f, 30f));
        list.Label($"Vomiting chance on a hit: {Settings.vomitChance:P0}");
        Settings.vomitChance = Mathf.Round(list.Slider(Settings.vomitChance, 0f, 1f) * 100f) / 100f;
        list.Label($"Vomiting cooldown: {Settings.vomitCooldownSeconds:0} seconds");
        Settings.vomitCooldownSeconds = Mathf.Round(list.Slider(Settings.vomitCooldownSeconds, 1f, 120f));
        list.Gap();
        if (list.ButtonText("Restore defaults"))
        {
            Settings.productionMultiplier = 4f;
            Settings.foodCostFraction = 0.5f;
            Settings.throwRange = 12f;
            Settings.vomitChance = 0.3f;
            Settings.vomitCooldownSeconds = 10f;
        }
        list.End();
        Settings.Sanitize();
    }
}

public sealed class UsefulGenesSettings : ModSettings
{
    public float productionMultiplier = 4f;
    public float foodCostFraction = 0.5f;
    public float throwRange = 12f;
    public float vomitChance = 0.3f;
    public float vomitCooldownSeconds = 10f;

    public void Sanitize()
    {
        productionMultiplier = float.IsFinite(productionMultiplier) ? Math.Clamp(productionMultiplier, 0.1f, 20f) : 4f;
        foodCostFraction = float.IsFinite(foodCostFraction) ? Math.Clamp(foodCostFraction, 0f, 2f) : 0.5f;
        throwRange = float.IsFinite(throwRange) ? Math.Clamp(throwRange, 1f, 30f) : 12f;
        vomitChance = float.IsFinite(vomitChance) ? Math.Clamp(vomitChance, 0f, 1f) : 0.3f;
        vomitCooldownSeconds = float.IsFinite(vomitCooldownSeconds) ? Math.Clamp(vomitCooldownSeconds, 1f, 120f) : 10f;
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref productionMultiplier, "productionMultiplier", 4f);
        Scribe_Values.Look(ref foodCostFraction, "foodCostFraction", 0.5f);
        Scribe_Values.Look(ref throwRange, "throwRange", 12f);
        Scribe_Values.Look(ref vomitChance, "vomitChance", 0.3f);
        Scribe_Values.Look(ref vomitCooldownSeconds, "vomitCooldownSeconds", 10f);
        Sanitize();
    }
}

[DefOf]
public static class UsefulGeneDefOf
{
    public static GeneDef UG_FecalWalls;
    public static GeneDef UG_PermanentLactation;
    public static GeneDef UG_Squirter;
    public static ThingDef UG_FecalWall;
    public static ThingDef UG_SmoothedFecalWall;

    static UsefulGeneDefOf() => DefOfHelper.EnsureInitializedInCtor(typeof(UsefulGeneDefOf));
}

[StaticConstructorOnStartup]
public static class Startup
{
    static Startup()
    {
        SquirterIntegration.Initialize();
        new Harmony("umrae.usefulgenes").PatchAll(typeof(Startup).Assembly);
        Log.Message("[RJW Fluids and Solids] Genes, collected waste and Coffee compatibility ready.");
    }
}
