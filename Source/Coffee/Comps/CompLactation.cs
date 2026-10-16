using Verse;

namespace Umrae.UsefulGenes.Coffee
{
    public class CompLactation : ThingComp
    {
        public int lastHumanLactationIngestedTick = 0;
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look<int>(ref this.lastHumanLactationIngestedTick, "lastHumanLactationIngestedTick", 0);

        }

        public override string CompInspectStringExtra()
        {
            //if ((parent as Pawn).health?.hediffSet?.GetFirstHediffOfDef(HediffDefOf_Milk.InducingLactation) != null
            //    && (parent as Pawn).health.hediffSet.GetFirstHediffOfDef(HediffDefOf_Milk.InducingLactation).TryGetComp<HediffComp_LactationInduction>().canMassage())

            if ((parent as Pawn).health?.hediffSet?.GetFirstHediffOfDef(HediffDefOf_Milk.InducingLactation) != null
                && (parent as Pawn).health.hediffSet.GetFirstHediffOfDef(HediffDefOf_Milk.InducingLactationCooldown) == null)
            {
                return "Ready to stimulate breasts for lactation.";
            }

            else
            {
                return "";
            }
        }
    }


    /*
    public class CompLactation : ThingComp
    {
        private readonly int OneDayInTicks = 60000;
        private int TicksSinceLastMassage = -60000;

        private float InductionCompletionPercent = 0f;

        public bool isActive = false;
        public bool CanMassage = true;

        public int lastHumanLactationIngestedTick = 0;

        public override void CompTick()
        {
            base.CompTick();
            Pawn p = this.parent as Pawn;

            if (!p.IsHashIntervalTick(100))
            {
                return;
            }
            if (LactationUtility.IsLactating(p) || !isActive)
            {
                return;
            }
            if(TicksSinceLastMassage + OneDayInTicks / Props.TimesMassagedADay < GenTicks.TicksGame)
            {
                CanMassage = true;
            }

            if(InductionCompletionPercent >= 1)
            {
                string main = p.Name.ToStringShort + "'s breasts have been stimulated enough to induce lactation! They can now begin producing milk for their colony's consumption.";

                var letter = LetterMaker.MakeLetter(p.Name.ToStringShort + " induced lactation", main, LetterDefOf.PositiveEvent);
                Find.LetterStack.ReceiveLetter(letter);

                LactationUtility.StartLactating(p, true);
                InductionCompletionPercent = 0.90f + Random.Range(0f, 5f); //start at 90-95% in case they ever lose lactating again
            }
        }

        public void MassageBreasts()
        {
            InductionCompletionPercent += (float)1 / (Props.DaysToLactating * (Props.TimesMassagedADay + Rand.Value));
            TicksSinceLastMassage = GenTicks.TicksGame;
            CanMassage = false;
        }

        public CompProperties_InduceLactation Props
        {
            get
            {
                return (CompProperties_InduceLactation)props;
            }
        }

        public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn pawn)
        {
            if (pawn != this.parent as Pawn && !((this.parent as Pawn).IsSlaveOfColony || (this.parent as Pawn).IsPrisonerOfColony)) yield break;
            if (LactationUtility.IsLactating(pawn)) yield break;
            if(LactationUtility.HasMilkableBreasts(this.parent as Pawn))
            {
                if(isActive)
                {
                    //stop trying to induce lactation
                    yield return new FloatMenuOption("Stop inducing lactation", () =>
                    {
                        isActive = false;
                    });
                } 
                else
                {
                    //induce lactation
                    yield return new FloatMenuOption("Start inducing lactation", () =>
                    {
                        isActive = true;
                    });
                }
            }
            else
            {
                yield return new FloatMenuOption("Start inducing lactation (no milkable breasts)", null);
            }

            yield break;
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look<float>(ref this.InductionCompletionPercent, "InductionCompletionPercent", 0f);
            Scribe_Values.Look<int>(ref this.TicksSinceLastMassage, "TicksSinceLastMassage", -60000);
            Scribe_Values.Look<int>(ref this.lastHumanLactationIngestedTick, "lastHumanLactationIngestedTick", 0);

            Scribe_Values.Look<bool>(ref this.isActive, "IsActive", false);
            Scribe_Values.Look<bool>(ref this.CanMassage, "CanMassage", false);

        }

        public override string CompInspectStringExtra()
        {
            if (!isActive) return null;

            string result = "Induce lactation completion: " + InductionCompletionPercent.ToStringPercent();

            if(CanMassage)
            {
                result += "\nReady to massage.";
            }

            return result;
        }
    }

    */
}