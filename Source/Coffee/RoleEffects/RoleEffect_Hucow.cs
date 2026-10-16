using RimWorld;
using Verse;

namespace Umrae.UsefulGenes.Coffee
{
    public class RoleEffect_Hucow : RoleEffect
    {
        public override string Label(Pawn pawn, Precept_Role role)
        {
            return "Greatly increased milk yield, permenant lactation, slower speed";
        }

        public RoleEffect_Hucow()
        {

        }
    }
}
