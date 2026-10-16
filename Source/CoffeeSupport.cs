using System.Xml;
using EuterpeMilkyTitfuck;
using HarmonyLib;
using RimWorld;
using Verse;

namespace Umrae.UsefulGenes;

public static class CoffeeSupport
{
    public const string Package = "c0ffee.rjw.ideologyaddons";
    public static bool NativeModule => AppDomain.CurrentDomain.GetAssemblies().Any(a => a.GetName().Name == "CRIALactation");

    public static string ModuleFolder;

    public static void EnableInstalledModule(ModContentPack content)
    {
        var coffee = LoadedModManager.RunningModsListForReading.FirstOrDefault(m => m.PackageIdPlayerFacing.Equals(Package, StringComparison.OrdinalIgnoreCase));
        if (coffee == null || NativeModule) return;
        string folder = Path.Combine(coffee.RootDir, "CRIALactation");
        if (!Directory.Exists(Path.Combine(folder, "Defs")))
        { Log.Error("[RJW Fluids and Solids] Coffee's lactation definitions were not found."); return; }
        ModuleFolder = folder;
    }

    public static bool IsMilk(ThingDef def) => def != null && (def.defName is "SEX_BreastMilk" or "EM_HumanMilk" or "MooGirl_Milk" or "VCE_HumanoidCheese" or "cheese");
    public static bool ContainsMilk(Thing food, ThingDef def) => IsMilk(def) || food?.TryGetComp<CompIngredients>()?.ingredients?.Any(IsMilk) == true;

    public static void ApplyStats(HediffComp_Lactation comp)
    {
        var speed = DefDatabase<StatDef>.GetNamedSilentFail("MilkProductionSpeed");
        var yield = DefDatabase<StatDef>.GetNamedSilentFail("MilkProductionYield");
        if (speed == null || yield == null) return;
        float s = Math.Max(0f, comp.Pawn.GetStatValue(speed));
        float y = Math.Max(0f, comp.Pawn.GetStatValue(yield));
        comp.lactationPerDay *= s * y;
        comp.maximumNutritionStored *= y;
        comp.lactationPerTick = comp.lactationPerDay / 60000f;
    }

    public static void AddEventThoughts(List<FoodUtility.ThoughtFromIngesting> thoughts, IEnumerable<Precept> precepts, string eventName)
    {
        if (precepts == null) return;
        foreach (var precept in precepts)
            foreach (var effect in precept.def.comps?.OfType<PreceptComp_SelfTookMemoryThought>() ?? Enumerable.Empty<PreceptComp_SelfTookMemoryThought>())
                if (effect.eventDef?.defName == eventName && effect.thought != null && !thoughts.Any(t => t.thought == effect.thought && t.fromPrecept == precept))
                    thoughts.Add(new FoodUtility.ThoughtFromIngesting { thought = effect.thought, fromPrecept = precept });
    }

    public static void RecordMilk(Pawn pawn)
    {
        // Find the original or adapted saved component by name, including Equal Milking installations.
        var comp = pawn?.AllComps?.FirstOrDefault(c => c.GetType().FullName is "CRIALactation.CompLactation" or "Umrae.UsefulGenes.Coffee.CompLactation");
        if (comp != null) AccessTools.Field(comp.GetType(), "lastHumanLactationIngestedTick")?.SetValue(comp, GenTicks.TicksGame);
    }
}

// Runs against Coffee's original XML, including when Equal Milking already enables the module.
public sealed class PatchOperation_CoffeeMilk : PatchOperation
{
    protected override bool ApplyWorker(XmlDocument xml)
    {
        if (!CoffeeSupport.NativeModule && xml.SelectSingleNode("Defs/Umrae.UsefulGenes.Coffee.MilkConfigDef[defName='GlobalMilkSettings']") == null &&
            xml.SelectSingleNode("Defs/CRIALactation.MilkConfigDef[defName='GlobalMilkSettings']") == null && CoffeeSupport.ModuleFolder != null)
        {
            foreach (string path in Directory.GetFiles(Path.Combine(CoffeeSupport.ModuleFolder, "Defs"), "*.xml", SearchOption.AllDirectories).OrderBy(p => p, StringComparer.Ordinal))
            {
                var source = new XmlDocument(); source.Load(path);
                foreach (XmlNode optional in source.SelectNodes("//*[@MayRequire]"))
                    if (!optional.Attributes["MayRequire"].Value.Split(',').All(id => ModsConfig.IsActive(id.Trim()))) optional.ParentNode.RemoveChild(optional);
                foreach (XmlNode def in source.DocumentElement.ChildNodes)
                    if (def is XmlElement) xml.DocumentElement.AppendChild(xml.ImportNode(def, true));
            }
            var pawn = xml.SelectSingleNode("Defs/ThingDef[@Name='BasePawn']");
            if (pawn != null)
            {
                var comps = pawn.SelectSingleNode("comps");
                if (comps == null) { comps = xml.CreateElement("comps"); pawn.AppendChild(comps); }
                var comp = xml.CreateElement("li"); comp.SetAttribute("Class", "Umrae.UsefulGenes.Coffee.CompProperties_Lactation"); comps.AppendChild(comp);
            }
        }
        var config = xml.SelectSingleNode("Defs/CRIALactation.MilkConfigDef[defName='GlobalMilkSettings']/MilkThingDef");
        if (config == null) return true;
        if (xml.SelectSingleNode("Defs/ThingDef[defName='EM_HumanMilk']") == null)
            foreach (XmlNode n in config.SelectNodes("li[text()='EM_HumanMilk']")) config.RemoveChild(n);
        if (config.SelectSingleNode("li[text()='SEX_BreastMilk']") == null)
        { var item = xml.CreateElement("li"); item.InnerText = "SEX_BreastMilk"; config.AppendChild(item); }
        foreach (XmlNode icon in xml.SelectNodes("Defs/*/iconPath[text()='UI/Issues/Lactating' or text()='UI/Memes/Hucow']"))
            icon.InnerText = "UI/Genes/UG_PermanentLactation";
        if (!CoffeeSupport.NativeModule)
        {
            foreach (XmlNode node in xml.SelectNodes("Defs//*"))
            {
                if (node.Attributes != null)
                    foreach (XmlAttribute attr in node.Attributes)
                        if (attr.Value.StartsWith("CRIALactation.")) attr.Value = attr.Value.Replace("CRIALactation.", "Umrae.UsefulGenes.Coffee.");
                if (node.ChildNodes.Count == 1 && node.FirstChild is XmlText && node.InnerText.StartsWith("CRIALactation."))
                    node.InnerText = node.InnerText.Replace("CRIALactation.", "Umrae.UsefulGenes.Coffee.");
            }
            var old = config.ParentNode;
            var replacement = xml.CreateElement("Umrae.UsefulGenes.Coffee.MilkConfigDef");
            foreach (XmlAttribute attr in old.Attributes) replacement.SetAttribute(attr.Name, attr.Value);
            while (old.FirstChild != null) replacement.AppendChild(old.FirstChild);
            old.ParentNode.ReplaceChild(replacement, old);
        }
        return true;
    }
}

[HarmonyPatch(typeof(Thing), nameof(Thing.Ingested))]
public static class Patch_CoffeeIngested
{
    public static void Postfix(Thing __instance, Pawn ingester, float __result)
    { if (__result > 0f && CoffeeSupport.ContainsMilk(__instance, __instance.def)) CoffeeSupport.RecordMilk(ingester); }
}
