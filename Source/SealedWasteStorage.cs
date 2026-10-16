using RimWorld;
using Verse;

namespace Umrae.UsefulGenes;

public sealed class Building_SealedWasteContainer : Building_Storage { }

public sealed class SealedWasteContainerExtension : DefModExtension { }

public static class SealedWasteStorage
{
    public static bool IsWaste(string defName) => defName is "Feces" or "UrineBottle";
    static bool IsContainer(Thing owner) => owner is Building_SealedWasteContainer || owner?.def?.GetModExtension<SealedWasteContainerExtension>() != null;
    public static bool IsSealed(Thing thing)
    {
        if (thing?.def == null || !IsWaste(thing.def.defName)) return false;
        // Adaptive Storage can hold contents in a ThingOwner instead of a spawned slot group.
        if (thing.holdingOwner?.Owner is Thing owner && IsContainer(owner)) return true;
        return thing.Spawned && IsContainer(thing.GetSlotGroup()?.parent as Thing);
    }
}

public sealed class StatPart_SealedWaste : StatPart
{
    public override void TransformValue(StatRequest req, ref float val)
    { if (req.HasThing && SealedWasteStorage.IsSealed(req.Thing)) val = 0f; }
    public override string ExplanationPart(StatRequest req) => req.HasThing && SealedWasteStorage.IsSealed(req.Thing)
        ? "Sealed waste container: stored waste penalty removed." : null;
}
