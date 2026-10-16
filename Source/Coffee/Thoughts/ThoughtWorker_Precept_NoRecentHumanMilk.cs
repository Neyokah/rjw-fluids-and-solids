using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace Umrae.UsefulGenes.Coffee
{
    public class ThoughtWorker_Precept_NoRecentHumanMilk : ThoughtWorker_Precept, IPreceptCompDescriptionArgs
    {
        public IEnumerable<NamedArgument> GetDescriptionArgs()
        {
            yield return MinDaysSinceLastHumanMeatForThought.Named("HUMANMILKREQUIREDINTERVAL");
        }

        protected override ThoughtState ShouldHaveThought(Pawn p)
        {
            int num = Mathf.Max(0, (p.TryGetComp<CompLactation>()?.lastHumanLactationIngestedTick ?? 0));
            return Find.TickManager.TicksGame - num > 480000 && !LactationUtility.IsHucow(p);
        }

        public const int MinDaysSinceLastHumanMeatForThought = 8;
    }
}
