using RimWorld;
using Verse;

namespace Umrae.UsefulGenes.Coffee
{
    public class ThoughtWorker_Precept_Lactating_Essential_Social : ThoughtWorker_Precept_Social
    {
        protected override ThoughtState ShouldHaveThought(Pawn p, Pawn otherPawn)
        {

            if (p == null || otherPawn == null) return false;

            if (!LactationUtility.HasMilkableBreasts(otherPawn)) return false;

            if (LactationUtility.IsHucow(otherPawn))
            {
                return ThoughtState.ActiveAtStage(0);
            }
            else if (LactationUtility.IsLactating(otherPawn))
            {
                return ThoughtState.ActiveAtStage(1);

            }

            if (otherPawn.ageTracker?.Adult == null || !otherPawn.ageTracker.Adult)
            {
                return false;
            }

            if (ExpectationsUtility.CurrentExpectationFor(p).order <= ExpectationDefOf.VeryLow.order)
            {
                return ThoughtState.ActiveAtStage(2);
            }
            else if (ExpectationsUtility.CurrentExpectationFor(p).order <= ExpectationDefOf.Moderate.order)
            {
                return ThoughtState.ActiveAtStage(3);
            }
            else
            {
                return ThoughtState.ActiveAtStage(4);
            }

        }

    }
}
