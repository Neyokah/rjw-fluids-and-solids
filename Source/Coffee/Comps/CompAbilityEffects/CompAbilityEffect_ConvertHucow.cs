using RimWorld;
using rjw;
using rjw.Modules.Interactions;
using UnityEngine;
using Verse;

namespace Umrae.UsefulGenes.Coffee
{
    public class CompAbilityEffect_ConvertHucow : CompAbilityEffect
    {

        public override bool Valid(LocalTargetInfo target, bool throwMessages = false)
        {
            Pawn pawn = target.Pawn;
            return Umrae.UsefulGenes.SquirterPolicy.AdultHumanlike(this.parent.pawn) && Umrae.UsefulGenes.SquirterPolicy.AdultHumanlike(pawn) && AbilityUtility.ValidateMustBeHuman(pawn, throwMessages, this.parent) &&
                AbilityUtility.ValidateNoMentalState(pawn, throwMessages, this.parent) &&
                AbilityUtility.ValidateSameIdeo(this.parent.pawn, pawn, throwMessages, this.parent) &&
                LactationUtility.IsLactating(pawn) &&
                !LactationUtility.IsHucow(pawn);

        }


        public override void Apply(LocalTargetInfo target, LocalTargetInfo dest)
        {
            if (!Valid(target)) return;
            base.Apply(target, dest);
            Pawn pawn = target.Pawn;
            LactationUtility.ExtendLactationDuration(pawn);

            //let's expand the breasts too!
            var breastList = pawn.GetBreastList();
            if (!breastList.EnumerableNullOrEmpty())
                foreach (var breasts in breastList)
                {
                    if (breasts is Hediff_NaturalSexPart naturalBreasts)
                    {
                        var severity = naturalBreasts.Severity;
                        var hucowMinimumSize = LactationSettings.hucowBreastSizeMinimum; 
                        
                        severity += LactationSettings.hucowBreastSizeBonus; //this could make some big ones rediculous.
                        if (severity < hucowMinimumSize)
                        {
                            severity = hucowMinimumSize;
                        }
                        naturalBreasts.Severity = severity;

                        var comp = breasts.TryGetComp<HediffComp_SexPart>();
                        comp.SetSeverity(severity, sync: false);
                    } 
                }
        }
    }
}
