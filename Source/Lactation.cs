using EuterpeMilkyTitfuck;
using RimWorld;
using rjw;
using Verse;

namespace Umrae.UsefulGenes;

public sealed class Gene_PermanentLactation : Gene
{
    public override void PostAdd()
    {
        base.PostAdd();
        Refresh();
    }

    public override void TickInterval(int delta)
    {
        base.TickInterval(delta);
        if (pawn.IsHashIntervalTick(120, delta)) Refresh();
    }

    public void Refresh()
    {
        if (!Active || !LactationPolicy.Eligible(pawn)) return;
        var lactating = pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.Lactating);
        if (lactating == null)
        {
            lactating = HediffMaker.MakeHediff(HediffDefOf.Lactating, pawn);
            pawn.health.AddHediff(lactating);
        }
        lactating.Severity = 1f;
    }

    public override void PostRemove()
    {
        base.PostRemove();
        // Preserve a shared natural/drug lactation condition and any stored milk.
        // Without the gene it uses ordinary production, cost and timed decay.
        pawn.health?.hediffSet?.GetFirstHediffOfDef(HediffDefOf.Lactating)?
            .TryGetComp<HediffComp_Lactation>()?.RecacheStats();
    }
}

public static class LactationPolicy
{
    public static bool HasGene(Pawn pawn) => UsefulGeneDefOf.UG_PermanentLactation != null &&
        pawn?.genes?.HasActiveGene(UsefulGeneDefOf.UG_PermanentLactation) == true;

    public static bool Eligible(Pawn pawn) => pawn?.health?.hediffSet != null && !pawn.Dead &&
        pawn.RaceProps.Humanlike && pawn.needs?.food != null &&
        pawn.ageTracker != null && pawn.ageTracker.AgeBiologicalYearsFloat >= 16f &&
        !pawn.health.hediffSet.HasHediff(HediffDefOf.Malnutrition) && SizeFactor(pawn) > 0f;

    public static float SizeFactor(Pawn pawn)
    {
        if (pawn?.health?.hediffSet == null) return 0f;
        float sum = 0f;
        // Use the live health list so surgery and size changes cannot leave a stale RJW cache.
        foreach (var part in pawn.health.hediffSet.hediffs)
        {
            if (part.def is not HediffDef_SexPart def || def.genitalFamily != GenitalFamily.Breasts ||
                def.defName == "FeaturelessChest" || part.CurStageIndex <= 0 ||
                part.Part == null || pawn.health.hediffSet.PartIsMissing(part.Part)) continue;
            if (float.IsFinite(part.Severity) && part.Severity > 0f) sum += part.Severity / 0.5f;
        }
        return sum;
    }
}
