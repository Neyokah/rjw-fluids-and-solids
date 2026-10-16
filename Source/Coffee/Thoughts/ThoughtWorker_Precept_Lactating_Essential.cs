using RimWorld;
using Verse;

namespace Umrae.UsefulGenes.Coffee
{
    public class ThoughtWorker_Precept_Lactating_Essential : ThoughtWorker_Precept
    {

        protected override ThoughtState ShouldHaveThought(Pawn p)
        {

            if (!p.IsColonistPlayerControlled) return false;

            if (ThoughtUtility.ThoughtNullified(p, this.def))
            {
                return false;
            }

            if (!LactationUtility.HasMilkableBreasts(p))
            {
                return false;
            }
            if (LactationUtility.IsHucow(p))
            {
                return ThoughtState.ActiveAtStage(0);
            }
            if (LactationUtility.IsLactating(p))
            {
                return ThoughtState.ActiveAtStage(1);
            }

            if (!p.ageTracker.Adult)
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
