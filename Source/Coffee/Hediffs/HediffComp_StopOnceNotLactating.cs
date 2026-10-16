using Verse;

namespace Umrae.UsefulGenes.Coffee
{
    public class HediffComp_StopOnceNotLactating : HediffComp
    {

        public override void CompPostTickInterval(ref float severityAdjustment, int delta)
        {
            base.CompPostTickInterval(ref severityAdjustment, delta);

            if (!LactationUtility.IsLactating(Pawn))
            {
                LactationUtility.StopBeingHucow(Pawn);

            }
        }

    }
}
