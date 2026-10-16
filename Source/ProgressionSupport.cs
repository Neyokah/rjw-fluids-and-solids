using System.Xml;
using Verse;

namespace Umrae.UsefulGenes;

// Uses definitions already present in the load, so optional mods stay optional.
public sealed class PatchOperation_ProgressionSupport : PatchOperation
{
    static XmlNode Def(XmlDocument xml, string type, string name) => xml.SelectSingleNode($"Defs/{type}[defName='{name}']");
    static XmlNode Child(XmlNode parent, string name)
    {
        var node = parent.SelectSingleNode(name);
        if (node != null) return node;
        node = parent.OwnerDocument.CreateElement(name); parent.AppendChild(node); return node;
    }
    static void AddUnique(XmlNode parent, string value)
    {
        if (parent == null || parent.SelectNodes("li").Cast<XmlNode>().Any(n => n.InnerText == value)) return;
        var item = parent.OwnerDocument.CreateElement("li"); item.InnerText = value; parent.AppendChild(item);
    }
    static void Research(XmlDocument xml, string thing, string project)
    {
        var target = Def(xml, "ThingDef", thing);
        if (target != null && (Def(xml, "ResearchProjectDef", project) != null || Def(xml, "VFETribals.TribalResearchProjectDef", project) != null))
            AddUnique(Child(target, "researchPrerequisites"), project);
    }
    protected override bool ApplyWorker(XmlDocument xml)
    {
        // Match Progression's own primitive sanitation and barrel research, without gating genes.
        if (Def(xml, "ResearchProjectDef", "Ferny_Barrels") != null)
            Research(xml, "RFS_SealedWasteContainer", "Ferny_Barrels");
        if (Def(xml, "ThingDef", "PitLatrine")?.SelectSingleNode("researchPrerequisites/li[text()='VFET_Construction']") != null)
            Research(xml, "CollectionBucket", "VFET_Construction");

        var milk = Def(xml, "ThingDef", "SEX_BreastMilk");
        if (milk != null)
            foreach (string category in new[] { "DankPyon_Milk", "ROBTRG_MilkTypes", "RJW_Gastronomy_Humilk" })
                if (Def(xml, "ThingCategoryDef", category) != null) AddUnique(Child(milk, "thingCategories"), category);

        // Clone the installed milk-pipe templates as a distinct breast-milk network.
        // Shared animal-milk networks would turn the output back into Milk and lose its origin.
        string[] originals = { "MP_MilkPipe", "MP_UndergroundMilkPipe", "MP_MilkTank", "MP_MilkDrain", "MP_MilkTap" };
        string[] names = { "RFS_BreastMilkPipe", "RFS_UndergroundBreastMilkPipe", "RFS_BreastMilkTank", "RFS_BreastMilkDrain", "RFS_BreastMilkTap" };
        string[] labels = { "breast milk pipe", "underground breast milk pipe", "breast milk tank", "breast milk drain", "breast milk tap" };
        var network = Def(xml, "PipeSystem.PipeNetDef", "MP_MilkNetwork");
        if (milk == null || network == null || originals.Any(n => Def(xml, "ThingDef", n) == null)) return true;
        const string netName = "RFS_BreastMilkNetwork";
        if (Def(xml, "PipeSystem.PipeNetDef", netName) == null)
        {
            var clone = network.CloneNode(true); clone.SelectSingleNode("defName").InnerText = netName;
            clone.SelectSingleNode("resource/name").InnerText = "Breast milk";
            foreach (XmlNode pipe in clone.SelectNodes("pipeDefs/li"))
                if (Array.IndexOf(originals, pipe.InnerText) is int index && index >= 0) pipe.InnerText = names[index];
            xml.DocumentElement.AppendChild(clone);
        }
        for (int i = 0; i < originals.Length; i++)
        {
            if (Def(xml, "ThingDef", names[i]) != null) continue;
            var clone = Def(xml, "ThingDef", originals[i]).CloneNode(true);
            clone.Attributes.RemoveNamedItem("Name"); clone.Attributes.RemoveNamedItem("Abstract");
            clone.SelectSingleNode("defName").InnerText = names[i];
            clone.SelectSingleNode("label").InnerText = labels[i];
            Child(clone, "description").InnerText = "Part of a separate breast-milk pipe network. Accepts and dispenses Lactation Expansion breast milk, preserving its identity for food recipes and Coffee's beliefs. Uses the installed milk-pipe system's normal research, power, capacity and spoilage rules.";
            foreach (XmlNode node in clone.SelectNodes(".//pipeNet")) if (node.InnerText == "MP_MilkNetwork") node.InnerText = netName;
            foreach (XmlNode node in clone.SelectNodes("comps/li/thing | building/fixedStorageSettings/filter/thingDefs/li | building/defaultStorageSettings/filter/thingDefs/li"))
                if (node.InnerText == "Milk") node.InnerText = "SEX_BreastMilk";
            // The nutrient-paste grinder registration assumes animal milk. Keep these networks separate.
            foreach (XmlNode node in clone.SelectNodes("comps/li[@Class='VNPE.CompProperties_RegisterToGrinder']")) node.ParentNode.RemoveChild(node);
            xml.DocumentElement.AppendChild(clone);
        }
        return true;
    }
}
