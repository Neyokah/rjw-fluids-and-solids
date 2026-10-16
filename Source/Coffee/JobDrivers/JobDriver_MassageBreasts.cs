using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;

namespace Umrae.UsefulGenes.Coffee
{
    public class JobDriver_MassageBreasts : JobDriver
    {
        private readonly float WorkTotal = 600f;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
            LocalTargetInfo Target = job.GetTarget(TargetIndex.A);
            return pawn.Reserve(Target, job, 1, -1, null, errorOnFailed);
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {
            this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            this.FailOn(() => !Umrae.UsefulGenes.SquirterPolicy.AdultHumanlike(pawn) || !LactationUtility.HasMilkableBreasts(job.targetA.Pawn) || !LactationUtility.isMassageable(job.targetA.Pawn));
            yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
            Toil massage = new Toil();
            massage.FailOnDespawnedOrNull(TargetIndex.A);
            massage.FailOnAggroMentalStateAndHostile(TargetIndex.A);
            float massageProgress = 0;
            massage.initAction = delegate
            {
                Pawn p = job.GetTarget(TargetIndex.A).Thing as Pawn;
                pawn.pather.StopDead();
                PawnUtility.ForceWait(p, 15000, null, true);
            };
            massage.tickAction = delegate ()
            {
                pawn.skills.Learn(SkillDefOf.Animals, 0.13f, false);

                massageProgress += pawn.GetStatValue(StatDefOf.AnimalGatherSpeed, true);

            };
            massage.AddEndCondition(delegate
            {
                Pawn p = job.GetTarget(TargetIndex.A).Thing as Pawn;
                if (massageProgress >= WorkTotal)
                {

                    Hediff induce = p.health.hediffSet.GetFirstHediffOfDef(HediffDefOf_Milk.InducingLactation);
                    induce.Severity += (1f / LactationSettings.totalMassagesUntilLactation);
                    //0.05f ; //20 times, 3 times a day = 7 days, give or take

                    if (induce.Severity >= 1)
                    {
                        //p.health.AddHediff(cooldown, 
                        LactationUtility.StartLactating(p, true);
                        //remove the other hediff
                        p.health.RemoveHediff(induce);
                    }

                    //Add cooldown hediff. This has the pain debuff so i'm going to add it even if you're now lactating. The last one would have still hurt.
                    Hediff cooldown = HediffMaker.MakeHediff(HediffDefOf_Milk.InducingLactationCooldown, p, null);
                    cooldown.Severity = LactationSettings.massageCooldown;
                    p.health.AddHediff(cooldown, null, null, null);

                    return JobCondition.Succeeded;
                }

                return JobCondition.Ongoing;

            });

            massage.AddFinishAction(delegate
            {
                Pawn pawn = this.job.GetTarget(TargetIndex.A).Thing as Pawn;
                if (pawn != null && pawn.CurJobDef == JobDefOf.Wait_MaintainPosture)
                {
                    pawn.jobs.EndCurrentJob(JobCondition.InterruptForced, true, true);
                }
            });
            massage.defaultCompleteMode = ToilCompleteMode.Never;

            massage.WithProgressBar(TargetIndex.A, () => massageProgress / WorkTotal);
            massage.activeSkill = (() => SkillDefOf.Animals);
            yield return massage;
            yield break;

        }
    }
}
