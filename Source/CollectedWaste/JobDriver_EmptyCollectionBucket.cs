using System.Collections.Generic;
using Verse;
using Verse.AI;

namespace CollectedWaste
{
	public class JobDriver_EmptyCollectionBucket : JobDriver
	{
		public override bool TryMakePreToilReservations(bool errorOnFailed)
		{
			return pawn.Reserve(job.targetA, job, 1, -1, null, errorOnFailed);
		}

		protected override IEnumerable<Toil> MakeNewToils()
		{
			this.FailOnDespawnedOrNull(TargetIndex.A);
			this.FailOn(() => !(job.targetA.Thing is Building_CollectionBucket bucket) || !bucket.HasOutput);
			yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.Touch);
			Toil empty = Toils_General.Wait(90);
			empty.WithProgressBarToilDelay(TargetIndex.A);
			yield return empty;
            Toil finish = ToilMaker.MakeToil("EmptyBucket");
            finish.defaultCompleteMode = ToilCompleteMode.Instant;
            finish.initAction = delegate
			{
				Building_CollectionBucket bucket = job.targetA.Thing as Building_CollectionBucket;
				bucket?.Empty(pawn);
			};
			yield return finish;
		}
	}
}
