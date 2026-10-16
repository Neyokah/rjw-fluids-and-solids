using RimWorld;

namespace Umrae.UsefulGenes.Coffee
{
    [DefOf]
    public static class HistoryEventDefOf_Milk
    {
        static HistoryEventDefOf_Milk()
        {
            DefOfHelper.EnsureInitializedInCtor(typeof(HistoryEventDefOf_Milk));
        }

        public static HistoryEventDef DrankMilkRaw;

        public static HistoryEventDef DrankMilkMeal;

        public static HistoryEventDef DrankNonMilkMeal;
    }
}
