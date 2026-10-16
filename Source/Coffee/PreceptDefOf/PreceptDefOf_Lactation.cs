using RimWorld;

namespace Umrae.UsefulGenes.Coffee
{
    [DefOf]
    public static class PreceptDefOf_Lactation
    {
        static PreceptDefOf_Lactation()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(PreceptDefOf_Lactation));
        }

        public static PreceptDef Lactating_Essential;
        public static PreceptDef Lactating_MandatoryHucow;
        //public static PreceptDef IdeoRole_Hucow;

    }
}