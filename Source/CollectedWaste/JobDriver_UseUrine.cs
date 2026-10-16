using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace CollectedWaste
{
	public class JobDriver_PourUrine : JobDriver
	{
		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return pawn.Reserve(job.targetB, job, 1, -1, null, errorOnFailed);
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDestroyedOrNull(TargetIndex.B);
			yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.ClosestTouch);
			yield return Toils_Haul.StartCarryThing(TargetIndex.B);
			yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.Touch);
			yield return Toils_General.Wait(50);
			Toil pour = ToilMaker.MakeToil("PourUrine");
			pour.defaultCompleteMode = ToilCompleteMode.Instant;
			pour.initAction = delegate
			{
                if (!CollectedWasteUtility.ConsumeOne(pawn, pawn.carryTracker?.CarriedThing)) return;
				CollectedWasteUtility.Extinguish(pawn.Map, job.targetA.Cell);
				CollectedWasteUtility.LeaveUrineFilth(pawn.Map, job.targetA.Cell);
			};
			yield return pour;
		}
	}

	public class JobDriver_FertilizeWithUrine : JobDriver
	{
		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return pawn.Reserve(job.targetB, job, 1, -1, null, errorOnFailed);
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDestroyedOrNull(TargetIndex.B);
			this.FailOn(() => !CollectedWasteUtility.CanFertilize(pawn.Map, job.targetA.Cell));
			yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.ClosestTouch);
			yield return Toils_Haul.StartCarryThing(TargetIndex.B);
			yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.Touch);
			yield return Toils_General.Wait(60);
			Toil pour = ToilMaker.MakeToil("FertilizeWithUrine");
			pour.defaultCompleteMode = ToilCompleteMode.Instant;
			pour.initAction = delegate
			{
				if (!CollectedWasteUtility.CanFertilize(pawn.Map, job.targetA.Cell))
					return;
                if (!CollectedWasteUtility.ConsumeOne(pawn, pawn.carryTracker?.CarriedThing)) return;
				CollectedWasteUtility.Fertilize(pawn.Map, job.targetA.Cell);
				CollectedWasteUtility.LeaveUrineFilth(pawn.Map, job.targetA.Cell);
			};
			yield return pour;
		}
	}
}
