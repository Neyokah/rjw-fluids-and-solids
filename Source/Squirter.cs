using System.Reflection;
using System.Runtime.CompilerServices;
using HarmonyLib;
using RimWorld;
using rjw;
using Verse;

namespace Umrae.UsefulGenes;

public sealed class Gene_Squirter : Gene
{
    public int pendingItems;
    private int lastJobId = -1;
    private int lastOrdinal = -1;
    private int lastRjwTick = -1;
    private int lastFallbackTick = -1;
    private bool fallbackCredit;
    private readonly ConditionalWeakTable<SexProps, Receipt> receipts = new();

    private sealed class Receipt
    {
        public int jobId = -2;
        public int ordinal = -1;
    }

    public void OnRjwEvent(SexProps props)
    {
        if (!SquirterPolicy.Eligible(pawn) || props == null || props.pawn != pawn ||
            (props.partner != null && !SquirterPolicy.AdultHumanlike(props.partner))) return;
        int jobId = pawn.CurJob?.loadID ?? -1;
        var receipt = receipts.GetValue(props, static _ => new Receipt());
        if (receipt.jobId == jobId && receipt.ordinal >= props.orgasms) return;
        receipt.jobId = jobId;
        receipt.ordinal = props.orgasms;
        // Persist the active job receipt so a repeated callback after load cannot pay twice.
        if (jobId >= 0 && lastJobId == jobId && lastOrdinal >= props.orgasms) return;
        lastJobId = jobId;
        lastOrdinal = props.orgasms;
        int now = GenTicks.TicksGame;
        lastRjwTick = now;
        if (lastFallbackTick == now && fallbackCredit) fallbackCredit = false;
        else Queue(1);
    }

    public void OnFallbackEvent()
    {
        if (!SquirterPolicy.Eligible(pawn)) return;
        int now = GenTicks.TicksGame;
        if (lastFallbackTick == now || lastRjwTick == now) return;
        lastFallbackTick = now;
        fallbackCredit = true;
        Queue(1);
    }

    public void Queue(int normalAmount)
    {
        if (!SquirterPolicy.Eligible(pawn) || normalAmount <= 0) return;
        pendingItems = (int)Math.Min(int.MaxValue, pendingItems + (long)normalAmount * 3);
        Flush();
    }

    public override void TickInterval(int delta)
    {
        base.TickInterval(delta);
        if (pendingItems > 0 && pawn.IsHashIntervalTick(120, delta)) Flush();
    }

    public void Flush()
    {
        if (pendingItems <= 0 || !SquirterPolicy.Eligible(pawn) || !pawn.Spawned) return;
        ThingDef output = SquirterIntegration.Output;
        if (output == null) return;
        for (int i = 0; i < 16 && pendingItems > 0; i++)
        {
            int requested = Math.Min(pendingItems, Math.Max(1, output.stackLimit));
            var thing = ThingMaker.MakeThing(output);
            thing.stackCount = requested;
            int placed = 0;
            GenPlace.TryPlaceThing(thing, pawn.Position, pawn.Map, ThingPlaceMode.Near, out _,
                (_, count) =>
                {
                    int actual = Math.Clamp(count, 0, requested - placed);
                    placed += actual;
                    pendingItems -= actual;
                });
            // Any unplaced remainder stays in the saved queue; only confirmed placement is debited.
            if (placed < requested) break;
        }
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref pendingItems, "pendingLoveJuice", 0);
        Scribe_Values.Look(ref lastJobId, "lastSquirterJob", -1);
        Scribe_Values.Look(ref lastOrdinal, "lastSquirterOrdinal", -1);
        Scribe_Values.Look(ref lastRjwTick, "lastSquirterRjwTick", -1);
        Scribe_Values.Look(ref lastFallbackTick, "lastSquirterFallbackTick", -1);
        Scribe_Values.Look(ref fallbackCredit, "squirterFallbackCredit", false);
        pendingItems = Math.Max(0, pendingItems);
    }
}

public static class SquirterPolicy
{
    public static bool AdultHumanlike(Pawn pawn)
    {
        if (pawn?.ageTracker == null || !pawn.RaceProps.Humanlike || pawn.ageTracker.AgeBiologicalYearsFloat < 18f) return false;
        var stage = pawn.DevelopmentalStage;
        return (stage & DevelopmentalStage.Adult) != 0 &&
            (stage & (DevelopmentalStage.Newborn | DevelopmentalStage.Baby | DevelopmentalStage.Child)) == 0;
    }

    public static Gene_Squirter Gene(Pawn pawn) => pawn?.genes?.GetGene(UsefulGeneDefOf.UG_Squirter) as Gene_Squirter;

    public static bool Eligible(Pawn pawn) => UsefulGeneDefOf.UG_Squirter != null && pawn?.health != null &&
        !pawn.Dead && AdultHumanlike(pawn) && Gene(pawn)?.Active == true;
}

public static class SquirterIntegration
{
    public readonly record struct ToySpec(float threshold, int normalAmount);
    public static readonly Dictionary<HediffDef, ToySpec> Toys = new();
    public static ThingDef Output { get; private set; }
    private static readonly string[] toyDefs = {
        "S16VibratorIHed", "S16VibratorIIHed", "S16VibratorIIIHed", "S16VibratorIIIIHed",
        "S16VibratorHed", "S16VibratorDoubleHed", "S16_NippleWristCuffs_MainHed"
    };

    public static void Initialize()
    {
        Output = DefDatabase<ThingDef>.GetNamedSilentFail("S16GirlCum");
        Toys.Clear();
        foreach (string name in toyDefs)
        {
            var def = DefDatabase<HediffDef>.GetNamedSilentFail(name);
            // The installed S16 definitions use stage 5 for completion, followed by recovery.
            // Read the actual threshold and normal item count; do not depend on translated labels.
            var spawner = def?.comps?.FirstOrDefault(c =>
                c.GetType().FullName == "s16_extension.HediffCompProperties_ItemSpawner");
            if (def?.stages?.Count <= 5 || spawner == null) continue;
            var type = spawner.GetType();
            if (type.GetField("thingToSpawn")?.GetValue(spawner) != Output || Output == null) continue;
            if (type.GetField("spawnCount")?.GetValue(spawner) is int count && count > 0)
                Toys[def] = new ToySpec(def.stages[5].minSeverity, count);
        }
    }

    public static void NotifyToy(Hediff hediff, float before)
    {
        if (hediff?.def == null || !Toys.TryGetValue(hediff.def, out var spec) ||
            !(before < spec.threshold && hediff.Severity >= spec.threshold)) return;
        if (SquirterPolicy.Eligible(hediff.pawn)) SquirterPolicy.Gene(hediff.pawn).Queue(spec.normalAmount);
    }
}

[HarmonyPatch(typeof(SexUtility), nameof(SexUtility.SatisfyPersonal))]
public static class Patch_SquirterRjw
{
    public static void Postfix(SexProps props)
    {
        if (SquirterPolicy.Eligible(props?.pawn)) SquirterPolicy.Gene(props.pawn).OnRjwEvent(props);
    }
}

[HarmonyPatch(typeof(Hediff_Orgasm), nameof(Hediff_Orgasm.PostAdd))]
public static class Patch_SquirterFallback
{
    public static void Postfix(Hediff_Orgasm __instance)
    {
        var pawn = __instance.pawn;
        if (SquirterPolicy.Eligible(pawn) &&
            pawn.jobs?.curDriver is not JobDriver_Sex &&
            (pawn.CurJob?.targetA.Thing is not Pawn partner || SquirterPolicy.AdultHumanlike(partner)))
            SquirterPolicy.Gene(pawn).OnFallbackEvent();
    }
}

[HarmonyPatch(typeof(Hediff), nameof(Hediff.Severity), MethodType.Setter)]
public static class Patch_SquirterToyStage
{
    public static void Prefix(Hediff __instance, out float __state) => __state =
        __instance.def != null && SquirterIntegration.Toys.ContainsKey(__instance.def) ? __instance.Severity : float.NaN;
    public static void Postfix(Hediff __instance, float __state)
    {
        if (float.IsFinite(__state)) SquirterIntegration.NotifyToy(__instance, __state);
    }
}

[HarmonyPatch]
public static class Patch_SquirterToyTimer
{
    public static MethodBase TargetMethod() => AccessTools.Method(
        typeof(s16_extension.HediffComp_ResetSeverity).Assembly.GetType("s16_extension.HediffComp_ItemSpawner", true), "SpawnItem");

    public static bool Prefix(HediffComp __instance) => __instance.parent?.def == null ||
        !SquirterIntegration.Toys.ContainsKey(__instance.parent.def) || !SquirterPolicy.Eligible(__instance.Pawn);
}
