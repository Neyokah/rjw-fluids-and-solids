using RimWorld;
using Verse;

namespace Umrae.UsefulGenes.Coffee
{
    [DefOf]
    public static class JobDefOf_CRIALactation
    {
        static JobDefOf_CRIALactation()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(HediffDefOf_Milk));
        }

        public static JobDef MassageBreasts;
    }
}
