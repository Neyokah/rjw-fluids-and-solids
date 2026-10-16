using DubsBadHygiene;
using RimWorld;
using System.Text;
using Verse;

namespace CollectedWaste
{
	public class Building_CollectionBucket : bibblefuckwit
	{
		public const int Capacity = 8;

		public int urine;
		public int feces;

		public override FixtureQuality Quality => FixtureQuality.shabby;
		public override bool NeedsSewage => false;
		public override float waterUsed => 0f;

		public bool HasOutput => urine > 0 || feces > 0;
		public bool IsFull => urine >= Capacity || feces >= Capacity;

		public override AcceptanceReport Working(float WaterUsed = 0f)
		{
			if (IsFull)
				return "CollectionBucketFull".Translate();
			return true;
		}

		public override bool TryUseFlush()
		{
			if (IsFull)
				return false;
			urine++;
			feces++;
			return true;
		}

		public void Empty(Pawn pawn)
		{
			int urinePlaced = CollectedWasteUtility.Drop(Map, pawn.Position, CollectedWasteUtility.UrineDef, urine);
			int fecesPlaced = CollectedWasteUtility.Drop(Map, pawn.Position, CollectedWasteUtility.FecesDef, feces);
			urine -= urinePlaced;
			feces -= fecesPlaced;
			if (urine < 0)
				urine = 0;
			if (feces < 0)
				feces = 0;
		}

		public override void ExposeData()
		{
			base.ExposeData();
			Scribe_Values.Look(ref urine, "urine", 0);
			Scribe_Values.Look(ref feces, "feces", 0);
		}

		public override string GetInspectString()
		{
			StringBuilder text = new StringBuilder();
			string parent = base.GetInspectString();
			if (!parent.NullOrEmpty())
			{
				text.AppendLine(parent);
			}
			text.AppendLine("CollectionBucketContents".Translate(urine, feces, Capacity));
			return text.ToString().TrimEndNewlines();
		}
	}
}
