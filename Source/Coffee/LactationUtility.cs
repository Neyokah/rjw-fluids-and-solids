
using rjw;
using Verse;

namespace Umrae.UsefulGenes.Coffee
{
    public static class LactationUtility
    {

        public static bool IsLactating(Pawn p)
        {

            return
                p.health.hediffSet.HasHediff(HediffDefOf_Milk.Lactating, false);

        }

        public static bool HasMilkableBreasts(Pawn p)
        {
            if (!Umrae.UsefulGenes.SquirterPolicy.AdultHumanlike(p)) return false;

            if (Umrae.UsefulGenes.LactationPolicy.SizeFactor(p) > 0f)
            {
                return true;
            }

            return false;
        }

        public static void StartLactating(Pawn p, bool natural)
        {
            if (!HasMilkableBreasts(p)) return;
            if (IsLactating(p)) { ExtendLactationDuration(p); return; }
            Hediff lactating = HediffMaker.MakeHediff(HediffDefOf_Milk.Lactating, p, null);
            lactating.Severity = 1;// Rand.Value;
            p.health.AddHediff(lactating, null, null, null);
            //just adding to base body, not to breasts. Just in case the target has multiple breasts so to not confuse
            //p.health.AddHediff(lactating, Genital_Helper.get_breastsBPR(p));
        }

        public static void StopBeingHucow(Pawn p)
        {
            var hucow = p.health.hediffSet.GetFirstHediffOfDef(HediffDefOf_Milk.Hucow, false);
            if (hucow != null) p.health.RemoveHediff(hucow);
        }

        public static bool IsHucow(Pawn p)
        {
            return p.health.hediffSet.HasHediff(HediffDefOf_Milk.Hucow);
        }

        public static bool isMassageable(Pawn p)
        {
            if (!HasMilkableBreasts(p)) return false;
            Hediff lactationInductionHediff = p.health?.hediffSet?.GetFirstHediffOfDef(HediffDefOf_Milk.InducingLactation);
            Hediff lactationInductionCooldownHediff = p.health?.hediffSet?.GetFirstHediffOfDef(HediffDefOf_Milk.InducingLactationCooldown);
            if (lactationInductionHediff == null) return false;
            if (lactationInductionCooldownHediff != null) return false;

            return true;

        }

        public static void ExtendLactationDuration(Pawn p)
        {

            var Lact = p.health.hediffSet.GetFirstHediffOfDef(HediffDefOf_Milk.Lactating);
            if (Lact != null)
            {
                Lact.Severity = 1;
                //Lact.TryGetComp<HediffComp_Disappears>().ticksToDisappear = 1800000;
            }




        }
    }
}
