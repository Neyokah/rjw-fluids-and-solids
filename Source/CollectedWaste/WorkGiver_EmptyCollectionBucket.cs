using RimWorld;
using Verse;
using Verse.AI;

namespace CollectedWaste
{
	public class WorkGiver_EmptyCollectionBucket : WorkGiver_Scanner
	{
		public override ThingRequest PotentialWorkThingRequest => ThingRequest.ForDef(DefDatabase<ThingDef>.GetNamed("CollectionBucket"));
		public override PathEndMode PathEndMode => PathEndMode.Touch;

		public override bool HasJobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			Building_CollectionBucket bucket = t as Building_CollectionBucket;
			if (bucket == null || !bucket.HasOutput || bucket.IsBurning() || bucket.IsForbidden(pawn))
				return false;
			return pawn.CanReserve(bucket, 1, -1, null, forced);
		}

		public override Job JobOnThing(Pawn pawn, Thing t, bool forced = false)
		{
			return JobMaker.MakeJob(CollectedWasteUtility.EmptyJob, t);
		}
	}
}
