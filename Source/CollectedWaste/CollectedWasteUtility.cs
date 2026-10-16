using DubsBadHygiene;
using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;

namespace CollectedWaste
{
	public static class CollectedWasteUtility
	{
		public static ThingDef UrineDef => DefDatabase<ThingDef>.GetNamed("UrineBottle");
		public static ThingDef FecesDef => DefDatabase<ThingDef>.GetNamed("Feces");
		public static JobDef EmptyJob => DefDatabase<JobDef>.GetNamed("EmptyCollectionBucket");
		public static JobDef PourJob => DefDatabase<JobDef>.GetNamed("PourUrine");
		public static JobDef FertilizeJob => DefDatabase<JobDef>.GetNamed("FertilizeWithUrine");

		public static int Drop(Map map, IntVec3 cell, ThingDef def, int count)
		{
			if (map == null || def == null || count <= 0 || def.stackLimit < 1)
				return 0;
			int left = count;
			int placed = 0;
			while (left > 0)
			{
				int stack = Mathf.Min(left, def.stackLimit);
				Thing thing = ThingMaker.MakeThing(def);
				thing.stackCount = stack;
                int received = 0;
                GenPlace.TryPlaceThing(thing, cell, map, ThingPlaceMode.Near, out _,
                    (_, amount) => received += amount);
                received = Math.Min(stack, Math.Max(0, received));
                placed += received;
                left -= received;
                if (!thing.Destroyed && !thing.Spawned) thing.Destroy(DestroyMode.Vanish);
                if (received < stack) break;
			}
			return placed;
		}

		public static bool HasFire(Map map, IntVec3 cell)
		{
			if (map == null || !cell.InBounds(map))
				return false;
			foreach (IntVec3 spot in GenRadial.RadialCellsAround(cell, 1.5f, true))
			{
				if (!spot.InBounds(map))
					continue;
				List<Thing> things = spot.GetThingList(map);
				for (int i = 0; i < things.Count; i++)
				{
					if (things[i].def == ThingDefOf.Fire || things[i].IsBurning())
						return true;
				}
			}
			return false;
		}

		public static void Extinguish(Map map, IntVec3 cell)
		{
			foreach (IntVec3 spot in GenRadial.RadialCellsAround(cell, 1.5f, true))
			{
				if (!spot.InBounds(map))
					continue;
				List<Thing> things = spot.GetThingList(map);
				for (int i = things.Count - 1; i >= 0; i--)
				{
					if (things[i].def == ThingDefOf.Fire)
						things[i].Destroy(DestroyMode.Vanish);
				}
			}
		}

		public static bool CanFertilize(Map map, IntVec3 cell)
		{
			if (map == null || !cell.InBounds(map))
				return false;
			TerrainDef terrain = cell.GetTerrain(map);
			TerrainAffordanceDef diggable = DefDatabase<TerrainAffordanceDef>.GetNamedSilentFail("Diggable");
			return diggable != null && terrain?.affordances != null && terrain.affordances.Contains(diggable);
		}

		public static void Fertilize(Map map, IntVec3 cell)
		{
			map.GetComponent<MapComponent_Hygiene>()?.AddFertilizer(cell);
		}

		public static void LeaveUrineFilth(Map map, IntVec3 cell)
		{
			ThingDef filth = DefDatabase<ThingDef>.GetNamedSilentFail("FilthUrine");
			if (filth != null)
				FilthMaker.TryMakeFilth(cell, map, filth);
		}

		public static bool ConsumeOne(Pawn pawn, Thing carried)
		{
			if (carried == null || carried.Destroyed)
				return false;
			if (carried.stackCount > 1)
			{
				carried.stackCount--;
				return true;
			}
			if (pawn?.carryTracker?.CarriedThing == carried)
				pawn.carryTracker.DestroyCarriedThing();
			else
				carried.Destroy(DestroyMode.Vanish);
			return true;
		}
	}
}
