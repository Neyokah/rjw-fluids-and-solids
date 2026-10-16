using RimWorld;
using Verse;

namespace Umrae.UsefulGenes.Coffee
{
    public abstract class SpecialThingFilterWorker_MilkBase : SpecialThingFilterWorker
    {

        protected bool IsHumanMilk(ThingDef t) => ThingDefOf_Milk.MilkConfig.Contains(t);

        protected bool IsHumanMilk(Thing t) => IsHumanMilk(t.def);

        public override bool CanEverMatch(ThingDef def)
        {
            return def.IsIngestible && def.IsProcessedFood;
        }

        protected bool IsFoodWithMilk(Thing food)
        {
            CompIngredients compIngredients = food.TryGetComp<CompIngredients>();

            if (compIngredients == null)
                return false;

            foreach (ThingDef ingredient in compIngredients.ingredients)
            {
                if (IsHumanMilk(ingredient))
                    return true;
            }

            return false;
        }


    }
}
