using CollectedWaste;
using RimWorld;
using Verse;
using Verse.AI;

namespace Umrae.UsefulGenes;

public static class FecesWeapon
{
    public static float Range => UsefulGenesMod.Settings.throwRange;
    public static float VomitChance => UsefulGenesMod.Settings.vomitChance;
    public static bool CanThrow(Pawn pawn) => pawn?.Spawned == true && !pawn.Dead && !pawn.Downed &&
        !pawn.WorkTagIsDisabled(WorkTags.Violent) && pawn.health.capacities.CapableOf(PawnCapacityDefOf.Manipulation);
    public static bool CanAim(Pawn pawn, Pawn target) => target != null && target != pawn && target.Spawned && !target.Dead && target.Map == pawn.Map;
    public static bool InRange(Pawn pawn, Pawn target) => CanAim(pawn, target) && pawn.Position.DistanceToSquared(target.Position) <= Range * Range && GenSight.LineOfSight(pawn.Position, target.Position, pawn.Map);
    public static bool ShouldVomit(bool flesh, bool dead, bool downed, bool hasFood, bool shielded, bool coolingDown, float roll) =>
        flesh && !dead && !downed && hasFood && !shielded && !coolingDown && roll < VomitChance;

    public static IntVec3 FindPosition(Pawn pawn, Pawn target)
    {
        if (!CanAim(pawn, target)) return IntVec3.Invalid;
        if (InRange(pawn, target)) return pawn.Position;
        return GenRadial.RadialCellsAround(target.Position, Range, true)
            .Where(c => c.InBounds(pawn.Map) && c.Standable(pawn.Map) && !c.IsForbidden(pawn) && GenSight.LineOfSight(c, target.Position, pawn.Map))
            .OrderBy(c => c.DistanceToSquared(pawn.Position))
            .FirstOrDefault(c => pawn.CanReach(c, PathEndMode.OnCell, Danger.Some), IntVec3.Invalid);
    }
}

public sealed class CompProperties_FecesThrow : CompProperties
{
    public CompProperties_FecesThrow() => compClass = typeof(CompFecesThrow);
}

public sealed class CompFecesThrow : ThingComp
{
    public override IEnumerable<FloatMenuOption> CompFloatMenuOptions(Pawn selPawn)
    {
        if (!FecesWeapon.CanThrow(selPawn) || !parent.Spawned || parent.IsForbidden(selPawn) ||
            !selPawn.CanReserveAndReach(parent, PathEndMode.ClosestTouch, Danger.Some)) yield break;
        yield return new FloatMenuOption("Throw feces at...", () =>
            Find.Targeter.BeginTargeting(new TargetingParameters { canTargetPawns = true, canTargetBuildings = false, canTargetLocations = false }, target =>
            {
                if (!FecesWeapon.CanAim(selPawn, target.Pawn)) return;
                var job = JobMaker.MakeJob(DefDatabase<JobDef>.GetNamed("RFS_ThrowFeces"), parent, target);
                job.count = 1;
                selPawn.jobs.TryTakeOrderedJob(job);
            }, selPawn));
    }
}

public sealed class JobDriver_ThrowFeces : JobDriver
{
    public override bool TryMakePreToilReservations(bool errorOnFailed) => pawn.Reserve(job.targetA, job, 1, 1, null, errorOnFailed);
    protected override IEnumerable<Toil> MakeNewToils()
    {
        AddFinishAction(condition =>
        {
            if (pawn.Spawned && pawn.carryTracker?.CarriedThing?.def?.defName == "Feces")
                pawn.carryTracker.TryDropCarriedThing(pawn.Position, ThingPlaceMode.Near, out _);
        });
        this.FailOn(() => !FecesWeapon.CanThrow(pawn) || !FecesWeapon.CanAim(pawn, job.targetB.Pawn));
        this.FailOnDestroyedOrNull(TargetIndex.A);
        yield return Toils_Goto.GotoThing(TargetIndex.A, PathEndMode.ClosestTouch);
        yield return Toils_Haul.StartCarryThing(TargetIndex.A);
        var aim = ToilMaker.MakeToil("FindThrowingPosition");
        aim.defaultCompleteMode = ToilCompleteMode.Instant;
        aim.initAction = () =>
        {
            var cell = FecesWeapon.FindPosition(pawn, job.targetB.Pawn);
            if (!cell.IsValid) { EndJobWith(JobCondition.Incompletable); return; }
            job.SetTarget(TargetIndex.C, cell);
        };
        yield return aim;
        yield return Toils_Goto.GotoCell(TargetIndex.C, PathEndMode.OnCell);
        var windup = Toils_General.Wait(60);
        windup.tickAction = () => pawn.rotationTracker.FaceTarget(job.targetB);
        yield return windup;
        var toss = ToilMaker.MakeToil("ThrowFeces");
        toss.defaultCompleteMode = ToilCompleteMode.Instant;
        toss.initAction = () =>
        {
            var ammo = pawn.carryTracker?.CarriedThing;
            if (ammo?.def?.defName != "Feces" || !FecesWeapon.InRange(pawn, job.targetB.Pawn))
            { EndJobWith(JobCondition.Incompletable); return; }
            var projectile = (Projectile)GenSpawn.Spawn(DefDatabase<ThingDef>.GetNamed("RFS_FecesProjectile"), pawn.Position, pawn.Map);
            projectile.Launch(pawn, job.targetB, job.targetB, ProjectileHitFlags.All);
            CollectedWasteUtility.ConsumeOne(pawn, ammo);
        };
        yield return toss;
    }
}

public sealed class Projectile_Feces : Bullet
{
    protected override void Impact(Thing hitThing, bool blockedByShield = false)
    {
        base.Impact(hitThing, blockedByShield);
        if (hitThing is not Pawn victim) return;
        var nausea = DefDatabase<HediffDef>.GetNamed("RFS_FecesNausea");
        if (!FecesWeapon.ShouldVomit(victim.RaceProps.IsFlesh, victim.Dead, victim.Downed,
            victim.needs?.food != null, blockedByShield, victim.health.hediffSet.HasHediff(nausea) || victim.CurJobDef == JobDefOf.Vomit, Rand.Value)) return;
        var condition = victim.health.AddHediff(nausea);
        var timer = condition.TryGetComp<HediffComp_Disappears>();
        if (timer != null) timer.ticksToDisappear = (int)MathF.Round(UsefulGenesMod.Settings.vomitCooldownSeconds * 60f);
        if (victim.Spawned && victim.jobs != null) victim.jobs.StartJob(JobMaker.MakeJob(JobDefOf.Vomit), JobCondition.InterruptForced);
    }
}
