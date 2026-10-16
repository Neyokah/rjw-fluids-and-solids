using RimWorld;


namespace Umrae.UsefulGenes.Coffee
{
    [DefOf]
    public static class StatDefOf_Lactation
    {
        static StatDefOf_Lactation()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(StatDefOf_Lactation));
        }

        public static StatDef MilkProductionSpeed;
        public static StatDef MilkProductionYield;

    }
}
