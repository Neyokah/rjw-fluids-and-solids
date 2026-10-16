using Verse;

namespace Umrae.UsefulGenes.Coffee
{
    class SpecialThingFilterWorker_NoMilk : SpecialThingFilterWorker_MilkBase
    {
        public override bool Matches(Thing t)
        {
            return !(IsHumanMilk(t) || IsFoodWithMilk(t));
        }
    }
}
