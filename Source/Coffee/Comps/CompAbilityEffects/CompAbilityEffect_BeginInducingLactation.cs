using RimWorld;
using rjw;
using Verse;

namespace Umrae.UsefulGenes.Coffee
{
    public class CompAbilityEffect_BeginInducingLactation : CompAbilityEffect
    {

        public override bool Valid(LocalTargetInfo target, bool throwMessages = false)
        {
            Pawn pawn = target.Pawn;


            if (!Umrae.UsefulGenes.SquirterPolicy.AdultHumanlike(parent.pawn) || !LactationUtility.HasMilkableBreasts(pawn)) return false;
            if (!(pawn.IsColonistPlayerControlled || pawn.IsSlaveOfColony || pawn.IsPrisonerOfColony)) return false;
            if (!Genital_Helper.has_breasts(pawn) || Genital_Helper.has_male_breasts(pawn)) return false;
            if (LactationUtility.IsLactating(pawn)) return false;

            return true;

        }

    }
}
