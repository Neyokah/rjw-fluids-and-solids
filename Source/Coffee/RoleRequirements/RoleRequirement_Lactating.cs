using RimWorld;
using Verse;

namespace Umrae.UsefulGenes.Coffee
{
    public class RoleRequirement_Lactating : RoleRequirement
    {

        public override string GetLabel(Precept_Role role)
        {
            return "must be lactating";
        }

        public override bool Met(Pawn p, Precept_Role role)
        {
            if (LactationUtility.HasMilkableBreasts(p) && LactationUtility.IsLactating(p))
            {
                return true;
            }
            return false;
        }
    }
}
