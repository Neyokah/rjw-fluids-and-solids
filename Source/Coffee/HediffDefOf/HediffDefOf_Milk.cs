using RimWorld;
using Verse;

namespace Umrae.UsefulGenes.Coffee
{
    [DefOf]
    public static class HediffDefOf_Milk
    {
        static HediffDefOf_Milk()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(HediffDefOf_Milk));
        }

        public static HediffDef Lactating;



        public static HediffDef InducingLactation;
        public static HediffDef Hucow;
        public static HediffDef InducingLactationCooldown;



    }
}
