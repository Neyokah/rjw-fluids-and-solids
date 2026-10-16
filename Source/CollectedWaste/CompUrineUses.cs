using RimWorld;
using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace CollectedWaste
{
	public class CompProperties_UrineUses : CompProperties
	{
		public CompProperties_UrineUses()
		{
			compClass = typeof(CompUrineUses);
		}
	}

	public class CompUrineUses : ThingComp
	{
		public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
		{
			if (selPawn == null || parent.Destroyed || parent.IsForbidden(selPawn))
				yield break;
			if (!selPawn.CanReserveAndReach(parent, PathEndMode.ClosestTouch, Danger.Some))
				yield break;

			yield return new FloatMenuOption("PourUrineOnFire".Translate(), delegate
			{
				TargetingParameters parameters = new TargetingParameters
				{
					canTargetLocations = true
				};
				Find.Targeter.BeginTargeting(parameters, delegate(LocalTargetInfo target)
				{
					if (!CollectedWasteUtility.HasFire(selPawn.Map, target.Cell))
					{
						Messages.Message("PourUrineNoFire".Translate(), MessageTypeDefOf.RejectInput);
						return;
					}
					Job job = JobMaker.MakeJob(CollectedWasteUtility.PourJob, target.Cell, parent);
					job.count = 1;
					selPawn.jobs.TryTakeOrderedJob(job);
				}, selPawn);
			});

			yield return new FloatMenuOption("FertilizeWithUrine".Translate(), delegate
			{
				TargetingParameters parameters = new TargetingParameters
				{
					canTargetLocations = true
				};
				Find.Targeter.BeginTargeting(parameters, delegate(LocalTargetInfo target)
				{
					if (!CollectedWasteUtility.CanFertilize(selPawn.Map, target.Cell))
					{
						Messages.Message("FertilizeUrineBadSoil".Translate(), MessageTypeDefOf.RejectInput);
						return;
					}
					Job job = JobMaker.MakeJob(CollectedWasteUtility.FertilizeJob, target.Cell, parent);
					job.count = 1;
					selPawn.jobs.TryTakeOrderedJob(job);
				}, selPawn);
			});
		}
	}
}
