using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;
using System.Xml;
using EuterpeMilkyTitfuck;
using HarmonyLib;
using RimWorld;
using rjw;
using Umrae.UsefulGenes;
using Verse;

internal static class Program
{
    static int checks;
    internal static T Empty<T>() => (T)RuntimeHelpers.GetUninitializedObject(typeof(T));
    static void Check(bool condition, string message) { if (!condition) throw new Exception("FAIL: " + message); checks++; }
    static void Near(float a, float b, string message, float epsilon = 0.00005f) => Check(Math.Abs(a-b) < epsilon, $"{message}: {a} versus {b}");
    static void Set(object obj, string field, object value) => AccessTools.Field(obj.GetType(), field).SetValue(obj, value);
    static void Fake(Harmony h, MethodBase target, string method) => h.Patch(target, prefix: new HarmonyMethod(typeof(Fakes), method));

    static int Main()
    {
        AssemblyLoadContext.Default.Resolving += (_, name) => {
            var dir = Environment.GetEnvironmentVariable("RIMWORLD_MANAGED_DIR")!;
            var path = Path.Combine(dir, name.Name + ".dll");
            if (File.Exists(path)) return AssemblyLoadContext.Default.LoadFromAssemblyPath(path);
            path = Path.Combine(Path.Combine(Environment.GetEnvironmentVariable("RIMWORLD_WORKSHOP")!, "2023507013/1.7/Assemblies"), name.Name + ".dll");
            return File.Exists(path) ? AssemblyLoadContext.Default.LoadFromAssemblyPath(path) : null;
        };
        try { Run(); Console.WriteLine($"PASS: {checks} checks against installed game, RJW and Lactation Expansion assemblies."); return 0; }
        catch (Exception e) { Console.WriteLine(e); return 1; }
    }

    static void Run()
    {
        var h = new Harmony("umrae.usefulgenes.tests.environment");
        Fake(h, AccessTools.PropertyGetter(typeof(Prefs), nameof(Prefs.LogVerbose)), nameof(Fakes.False));
        Fake(h, AccessTools.PropertyGetter(typeof(Need_Food), nameof(Need_Food.MaxLevel)), nameof(Fakes.MaxFood));
        Fake(h, AccessTools.PropertyGetter(typeof(Find), nameof(Find.TickManager)), nameof(Fakes.Clock));
        Fake(h, AccessTools.PropertyGetter(typeof(TickManager), nameof(TickManager.TicksGame)), nameof(Fakes.Ticks));
        Fake(h, AccessTools.PropertyGetter(typeof(GenTicks), nameof(GenTicks.TicksGame)), nameof(Fakes.Ticks));
        Fake(h, AccessTools.Method(typeof(StatExtension), nameof(StatExtension.GetStatValue)), nameof(Fakes.Stats));
        Fake(h, AccessTools.Method(typeof(HediffMaker), nameof(HediffMaker.MakeHediff)), nameof(Fakes.MakeHediff));
        Fake(h, AccessTools.Method(typeof(Pawn_HealthTracker), nameof(Pawn_HealthTracker.AddHediff), new[] { typeof(Hediff), typeof(BodyPartRecord), typeof(DamageInfo?), typeof(DamageWorker.DamageResult) }), nameof(Fakes.AddHediff));
        Fake(h, AccessTools.Method(typeof(Pawn_HealthTracker), nameof(Pawn_HealthTracker.Notify_HediffChanged)), nameof(Fakes.Noop));
        Fake(h, AccessTools.Method(typeof(Gene), nameof(Gene.TickInterval)), nameof(Fakes.Noop));
        Fake(h, AccessTools.Method(typeof(ThingMaker), nameof(ThingMaker.MakeThing)), nameof(Fakes.MakeThing));
        Fake(h, AccessTools.PropertyGetter(typeof(Thing), nameof(Thing.Spawned)), nameof(Fakes.IsSpawned));
        Fake(h, AccessTools.PropertyGetter(typeof(Thing), nameof(Thing.Map)), nameof(Fakes.MapFor));
        Fake(h, AccessTools.Method(typeof(GenPlace), nameof(GenPlace.TryPlaceThing), new[] { typeof(Thing), typeof(IntVec3), typeof(Map), typeof(ThingPlaceMode), typeof(Thing).MakeByRefType(), typeof(Action<Thing,int>), typeof(Predicate<IntVec3>), typeof(Rot4?), typeof(int) }), nameof(Fakes.Place));
        new Harmony("umrae.usefulgenes.tests.production").PatchAll(typeof(MilkMath).Assembly);
        Console.WriteLine("All production Harmony patches installed on the actual dependency methods.");

        UsefulGeneDefOf.UG_PermanentLactation = new GeneDef { defName = "UG_PermanentLactation", minAgeActive = 16 };
        UsefulGeneDefOf.UG_FecalWalls = new GeneDef { defName = "UG_FecalWalls" };
        UsefulGeneDefOf.UG_Squirter = new GeneDef { defName = "UG_Squirter", minAgeActive = 18 };
        DefDatabase<GeneDef>.Add(UsefulGeneDefOf.UG_Squirter);
        UsefulGeneDefOf.UG_FecalWall = new ThingDef { defName = "UG_FecalWall" };
        UsefulGeneDefOf.UG_SmoothedFecalWall = new ThingDef { defName = "UG_SmoothedFecalWall" };
        HediffDefOf.Lactating = new HediffDef { defName = "Lactating", hediffClass = typeof(HediffWithComps) };
        HediffDefOf.Malnutrition = new HediffDef { defName = "Malnutrition" };
        DefDatabase<GeneDef>.Add(new GeneDef { defName = "SEX_AlwaysLactating" });
        DefDatabase<HediffDef>.Add(new HediffDef { defName = "SEX_BroodmotherHigh" });
        LactationExpansion_StatDefOf.SEX_LactationPerDay = new StatDef { defName = "SEX_LactationPerDay" };
        LactationExpansion_StatDefOf.SEX_LactationCapacity = new StatDef { defName = "SEX_LactationCapacity" };
        LactationExpansion_ThingDefOf.SEX_BreastMilk = new ThingDef { defName = "SEX_BreastMilk", statBases = new() { new StatModifier { stat = StatDefOf.Nutrition, value = 0.05f } } };
        LactationExpansion.settings = new LactationSettings { lactationSpeedMultiplier = 1, lactationCapacityMultiplier = 1 };
        UsefulGenesMod.Settings = new UsefulGenesSettings();
        Fakes.Manager = Empty<TickManager>();
        Fake(h, AccessTools.Method(typeof(ModsConfig), nameof(ModsConfig.IsActive), new[] { typeof(string) }), nameof(Fakes.False));
        TestAccounting();
        TestDefinitions();
        TestSquirter();

        var p = PawnWithGene();
        var part = Breast(p, 0.5f);
        var gene = (Gene_PermanentLactation)p.genes.GenesListForReading.Single();
        Check(LactationPolicy.Eligible(p), "average breast carrier eligible");
        foreach (var sex in new[] { Gender.Male, Gender.Female, Gender.None })
        { p.gender = sex; Check(LactationPolicy.Eligible(p), "eligibility independent of sex " + sex); }
        Set(p.ageTracker, "ageBiologicalTicksInt", 15L * 3600000);
        Check(!gene.Active && !LactationPolicy.Eligible(p), "age 15 inactive");
        Set(p.ageTracker, "ageBiologicalTicksInt", 16L * 3600000);
        Check(gene.Active && LactationPolicy.Eligible(p), "age 16 active");
        Set(p.ageTracker, "ageBiologicalTicksInt", 25L * 3600000);

        gene.Refresh(); gene.Refresh();
        Check(p.health.hediffSet.hediffs.Count(x => x.def == HediffDefOf.Lactating) == 1, "repeated activation shares a single lactation condition");
        var comp = p.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.Lactating).TryGetComp<HediffComp_Lactation>();
        Check(comp != null, "existing Lactation Expansion comp used");
        comp.RecacheStats(); Near(comp.lactationPerDay, 2, "average output nutrition/day"); Near(comp.maximumNutritionStored, 2, "average storage nutrition");
        for (int i = 0; i < 10; i++) comp.RecacheStats();
        Near(comp.lactationPerDay, 2, "repeated recache does not compound scaling");
        float foodCost = 0;
        for (int tick = 0; tick < 60000; tick += 120)
        {
            Fakes.Now = tick; p.needs.food.CurLevel = 1f;
            float adjustment = 0; comp.CompPostTickInterval(ref adjustment, 120);
            foodCost += 1f - p.needs.food.CurLevel;
        }
        Near(comp.currentNutritionStored, 2f, "one fed day stores two nutrition", 0.0001f);
        Near(foodCost, 1f, "one fed day costs one nutrition", 0.0001f);
        comp.currentNutritionStored = comp.maximumNutritionStored;
        p.needs.food.CurLevel = 1f; float severity = 0;
        comp.CompPostTickInterval(ref severity, 120); Near(p.needs.food.CurLevel, 1, "full storage costs no food");
        comp.currentNutritionStored = 1.999f;
        comp.CompPostTickInterval(ref severity, 120); Near(p.needs.food.CurLevel, 0.9995f, "near-full charges only actual addition");
        var milk = LactationUtilities.GenerateMilk(p);
        Check(milk.stackCount == 40 && milk.def == LactationExpansion_ThingDefOf.SEX_BreastMilk, "real dependency collection produces existing 40 milk items");
        Near(comp.currentNutritionStored, 0, "real collection empties same reservoir");
        Set(part, "severityInt", 1f); comp.RecacheStats(); Near(comp.lactationPerDay, 4, "double breast size doubles output");
        Set(part, "severityInt", 0.02f); comp.RecacheStats(); Check(comp.lactationPerDay > 0, "tiny breasts produce milk");
        Set(part, "severityInt", 0.01f); comp.RecacheStats(); Near(LactationPolicy.SizeFactor(p), 0, "nipples excluded");
        p.needs.food.CurLevel = 1; comp.currentNutritionStored = 0;
        comp.CompPostTickInterval(ref severity, 120); Near(comp.currentNutritionStored, 0, "nipples produce no milk");
        Near(p.needs.food.CurLevel, 1, "nipples incur no milk food cost");
        Set(part, "severityInt", 0.5f); var extra = Breast(p, 0.5f);
        Near(LactationPolicy.SizeFactor(p), 2, "additional breasts included");
        p.health.hediffSet.hediffs.Remove(extra); p.health.hediffSet.hediffs.Remove(part);
        Near(LactationPolicy.SizeFactor(p), 0, "surgery removal reflected immediately");
        p.health.hediffSet.hediffs.Add(part);
        var missing = new Hediff_MissingPart { pawn = p, Part = part.Part };
        p.health.hediffSet.hediffs.Add(missing); Near(LactationPolicy.SizeFactor(p), 0, "missing chest excludes attached breast");
        p.health.hediffSet.hediffs.Remove(missing);
        var malnutrition = new Hediff { def = HediffDefOf.Malnutrition, pawn = p };
        p.health.hediffSet.hediffs.Add(malnutrition);
        comp.CompPostTickInterval(ref severity, 120); Near(comp.currentNutritionStored, 0, "any malnutrition pauses production");
        p.health.hediffSet.hediffs.Remove(malnutrition); gene.Refresh();
        comp.CompPostTickInterval(ref severity, 120); Check(comp.currentNutritionStored > 0, "recovery resumes production");
        comp.currentNutritionStored = 0;
        LactationExpansion.settings.lactationSpeedMultiplier = 2;
        comp.RecacheStats(); Near(comp.lactationPerDay, 4, "external speed modifier applied once");
        Near(comp.lactationPerTick * 60000, 4, "reported output equals actual tick rate");
        LactationExpansion.settings.lactationSpeedMultiplier = 1;
        Fakes.StatFactor = 2; comp.RecacheStats(); Near(comp.lactationPerDay, 4, "external milk stat modifier applied once");
        Fakes.StatFactor = 1;
        UsefulGenesMod.Settings.productionMultiplier = 0.1f;
        Set(part, "severityInt", 0.02f); comp.RecacheStats();
        Check(comp.maximumNutritionStored >= 0.05f, "smallest production setting can still store a collectable milk item");
        UsefulGenesMod.Settings.productionMultiplier = 4;
        Set(part, "severityInt", 0.5f); comp.RecacheStats();
        new Gene_MilkyTits { pawn = p, def = DefDatabase<GeneDef>.GetNamed("SEX_AlwaysLactating") }.PostAdd();
        Check(p.health.hediffSet.hediffs.Count(x => x.def == HediffDefOf.Lactating) == 1, "actual wet nurse gene reuses the same reservoir");
        var decay = new HediffComp_LactationDecay { parent = (HediffWithComps)comp.parent, props = new HediffCompProperties_LactationDecay(), severityPerDay = -0.1f };
        Near(decay.SeverityChangePerDay(), 0, "eligible gene prevents timed decay");
        gene.overriddenByGene = new Gene();
        Near(decay.SeverityChangePerDay(), -0.1f, "suppression restores normal timed decay");
        comp.currentNutritionStored = 0; p.needs.food.CurLevel = 1;
        comp.CompPostTickInterval(ref severity, 120);
        Near(comp.currentNutritionStored, 0.001f, "suppression immediately restores native production, with no duplicate loop");
        Near(p.needs.food.CurLevel, 0.999f, "suppression immediately restores native food cost");
        Near(comp.lactationPerDay, 0.5f, "suppression restores normal production stats");
        gene.overriddenByGene = null; gene.Refresh(); comp.RecacheStats(); Near(comp.lactationPerDay, 2, "reactivation restores scaling");
        TestWalls(p);
        TestScribe(comp, gene);
        p.genes.GenesListForReading.Remove(gene); Set(p.genes, "endogenes", new List<Gene>()); Set(p.genes, "cachedGenes", null);
        gene.PostRemove(); Near(comp.lactationPerDay, 0.5f, "gene removal restores ordinary stats");
        Near(decay.SeverityChangePerDay(), -0.1f, "gene removal restores ordinary decay");
        Check(p.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.Lactating) != null, "removal preserves shared natural lactation and stored milk");
        TestWasteAndCoffee();
        TestStorageAndSettings();
        TestProgression();
        Console.WriteLine("Production, collection, eligibility, suppression/removal, wall gates and real Scribe round trips passed.");
    }

    static Pawn PawnWithGene()
    {
        var p = Empty<Pawn>(); p.thingIDNumber = 123;
        p.def = new ThingDef { defName = "FixtureHuman", race = new RaceProperties { intelligence = Intelligence.Humanlike } };
        p.ageTracker = new Pawn_AgeTracker(p); Set(p.ageTracker, "ageBiologicalTicksInt", 25L * 3600000);
        p.health = new Pawn_HealthTracker(p);
        p.needs = new Pawn_NeedsTracker(); p.needs.food = Empty<Need_Food>(); Set(p.needs.food, "pawn", p);
        p.needs.food.CurLevel = 1;
        p.genes = new Pawn_GeneTracker(p);
        var g = new Gene_PermanentLactation { pawn = p, def = UsefulGeneDefOf.UG_PermanentLactation };
        Set(p.genes, "endogenes", new List<Gene> { g });
        return p;
    }

    static Hediff Breast(Pawn p, float size)
    {
        var def = new HediffDef_SexPart { defName = "FixtureBreasts", genitalFamily = GenitalFamily.Breasts,
            stages = new() { new HediffStage { minSeverity = 0.01f }, new HediffStage { minSeverity = 0.02f }, new HediffStage { minSeverity = 0.2f }, new HediffStage { minSeverity = 0.4f } } };
        var part = new Hediff { def = def, pawn = p, Part = new BodyPartRecord { def = new BodyPartDef { defName = "Chest" } } };
        Set(part, "severityInt", size); p.health.hediffSet.hediffs.Add(part); return part;
    }

    static void TestAccounting()
    {
        var rng = new Random(147);
        for (int i = 0; i < 10000; i++)
        {
            float stored = rng.NextSingle()*10, capacity = rng.NextSingle()*10, request = rng.NextSingle(), food = rng.NextSingle(), cost = rng.NextSingle()*2;
            float n = MilkMath.Produced(stored, capacity, request, food, cost);
            Check(n >= 0 && n <= request + 0.00001f && n <= Math.Max(0, capacity-stored) + 0.00001f && n*cost <= food + 0.00001f, "production bounds and exact cost conservation");
        }
        Near(MilkMath.Produced(3, 2, 1, 1, 0.5f), 0, "shrinking capacity never destroys milk or charges food");
        Near(MilkMath.Produced(0, 2, 1, 0.1f, 0.5f), 0.2f, "cannot spend unavailable food");
        Near(MilkMath.Produced(0, 2, 1, 0, 0), 1, "configurable free conversion");
        Near(MilkMath.Produced(0, 2, float.NaN, 1, 0.5f), 0, "invalid request cannot corrupt reservoir");
        var settings = new UsefulGenesSettings { productionMultiplier = float.NaN, foodCostFraction = float.PositiveInfinity };
        settings.Sanitize(); Near(settings.productionMultiplier, 4, "invalid output setting repaired"); Near(settings.foodCostFraction, 0.5f, "invalid food setting repaired");
    }

    static void TestWalls(Pawn p)
    {
        var frame = new Frame { def = new ThingDef { defName = "FixtureFrame", entityDefToBuild = UsefulGeneDefOf.UG_FecalWall } };
        var blueprint = new Blueprint_Build { def = new ThingDef { defName = "FixtureBlueprint", entityDefToBuild = UsefulGeneDefOf.UG_FecalWall } };
        var smoothFrame = new Frame { def = new ThingDef { defName = "FixtureSmoothFrame", entityDefToBuild = UsefulGeneDefOf.UG_SmoothedFecalWall } };
        Check(!WallPolicy.Allowed(smoothFrame, p), "smooth-wall construction cannot bypass gene restrictions");
        Check(!WallPolicy.Allowed(frame, p) && !WallPolicy.Allowed(blueprint, p), "noncarriers cannot construct frames or blueprints");
        Check(!GenConstruct.CanConstruct(frame, p, true, true), "actual forced construction cannot bypass gene gate");
        Check(!Patch_CompleteConstruction.Prefix(frame, p), "completion boundary rejects noncarrier");
        var g = new Gene { pawn = p, def = UsefulGeneDefOf.UG_FecalWalls };
        var genes = (List<Gene>)AccessTools.Field(typeof(Pawn_GeneTracker), "endogenes").GetValue(p.genes);
        genes.Add(g); Set(p.genes, "cachedGenes", null);
        Check(WallPolicy.Allowed(smoothFrame, p), "gene gate recognizes smoothed wall variants");
        Check(WallPolicy.Allowed(frame, p) && WallPolicy.Allowed(blueprint, p), "active wall carrier may build");
        g.overriddenByGene = new Gene(); Check(!WallPolicy.Allowed(frame, p), "suppression blocks ongoing construction");
        genes.Remove(g); Set(p.genes, "cachedGenes", null);
        Check(WallPolicy.Allowed(new Thing { def = new ThingDef { defName = "OrdinaryWall" } }, p), "ordinary construction unchanged");
    }

    static void TestScribe(HediffComp_Lactation comp, Gene gene)
    {
        string file = Path.Combine(AppContext.BaseDirectory, "milk-save.xml");
        comp.currentNutritionStored = 1.2345f;
        Scribe.saver.InitSaving(file, "UsefulGenesFixture");
        comp.CompExposeData(); UsefulGenesMod.Settings.ExposeData(); Scribe.saver.FinalizeSaving();
        var restored = new HediffComp_Lactation();
        var settings = new UsefulGenesSettings();
        Scribe.loader.InitLoading(file); restored.CompExposeData(); settings.ExposeData(); Scribe.loader.FinalizeLoading();
        Near(restored.currentNutritionStored, 1.2345f, "dependency's native milk Scribe format survives load");
        Near(settings.productionMultiplier, 4, "output settings survive load"); Near(settings.foodCostFraction, 0.5f, "food cost settings survive load");
        Check(gene.GetType().GetMethod(nameof(Gene.ExposeData)).DeclaringType == typeof(Gene), "gene retains native save format with no extra persistence layer");
    }

    static void TestDefinitions()
    {
        var mod = Empty<ModContentPack>();
        mod.assemblies = new ModAssemblyHandler(mod);
        mod.assemblies.loadedAssemblies.Add(typeof(MilkMath).Assembly);
        mod.assemblies.loadedAssemblies.Add(typeof(PipeSystem.CompProperties_ConvertThingToResource).Assembly);
        mod.assemblies.loadedAssemblies.Add(typeof(s16_extension.HediffComp_ResetSeverity).Assembly);
        mod.assemblies.loadedAssemblies.Add(typeof(s16_extension.HediffComp_SexNeed).Assembly);
        LoadedModManager.RunningModsListForReading.Add(mod);
        GenTypes.ClearCache();
        string folder = FindModFolder();
        foreach (var path in Directory.GetFiles(folder, "*.xml", SearchOption.AllDirectories))
        {
            var doc = new XmlDocument(); doc.Load(path);
            foreach (XmlElement node in doc.DocumentElement.ChildNodes.OfType<XmlElement>())
            {
                Type type = GenTypes.GetTypeInAnyAssembly(node.Name, "Verse") ?? GenTypes.GetTypeInAnyAssembly(node.Name, "RimWorld");
                Check(type != null, "XML definition type exists: " + node.Name);
                ValidateFields(node, type);
            }
        }
        Console.WriteLine("Every custom XML field exists in the installed game types.");
        var buildingsDoc = new XmlDocument(); buildingsDoc.Load(Path.Combine(folder, "Buildings.xml"));
        var wall = DirectXmlToObject.ObjectFromXml<ThingDef>(buildingsDoc.DocumentElement.FirstChild, false);
        Check(wall.holdsRoof && wall.blockLight && wall.blockWind && wall.passability == Traversability.Impassable && wall.fillPercent == 1, "actual XML parser preserves roof, sight and movement blocking");
        Check(wall.building.isWall && wall.building.supportsWallAttachments, "actual wall and attachment flags");
        Check(wall.costStuffCount == 0 && wall.costList.NullOrEmpty() && wall.resourcesFractionWhenDeconstructed == 0 && !wall.leaveResourcesWhenKilled, "actual XML parser yields zero cost and zero recovery");
        Check(wall.graphicData.color.r > wall.graphicData.color.b, "wall has brown tint");
        Check(wall.researchPrerequisites.NullOrEmpty(), "wall has no research gate");
        var smoothNode = buildingsDoc.SelectSingleNode("Defs/ThingDef[defName='UG_SmoothedFecalWall']");
        var smooth = DirectXmlToObject.ObjectFromXml<ThingDef>(smoothNode, false);
        Check(buildingsDoc.SelectSingleNode("Defs/ThingDef[defName='UG_FecalWall']/building/smoothedThing").InnerText == smooth.defName, "rough wall references the smooth variant");
        Check(smoothNode.SelectSingleNode("building/unsmoothedThing").InnerText == wall.defName, "smooth variant references the rough wall");
        wall.building.smoothedThing = smooth; smooth.building.unsmoothedThing = wall;
        Check(wall.IsSmoothable && !smooth.IsSmoothable && smooth.IsSmoothed, "native smoothing properties recognize both states without repeat smoothing");
        Check(wall.statBases[0].value == 20 && smooth.statBases[0].value == 50, "smoothing raises maximum HP from 20 to 50");
        Check(wall.statBases[2].value == 0 && smooth.statBases[2].value == 0, "both wall variants are nonflammable");
        Check(wall.graphicData.texPath == "Things/Building/Linked/Rock_Atlas" && smooth.graphicData.texPath == "Things/Building/Linked/RockSmooth_Atlas", "rough and smooth surfaces use the game's actual stone atlases");
        Check(smooth.designationCategory == null && smoothNode.SelectSingleNode("designationCategory") == null, "smooth walls require smoothing rather than a second build command");
        Check(smooth.holdsRoof && smooth.blockLight && smooth.blockWind && smooth.building.isAirtight && wall.building.isAirtight, "both stone surfaces retain roof, sight, wind and airtight behavior");
        Check(smooth.costList.NullOrEmpty() && !smooth.leaveResourcesWhenKilled && smooth.resourcesFractionWhenDeconstructed == 0, "polishing creates no recovered resources");
        var categoryDoc = new XmlDocument(); categoryDoc.Load(Path.Combine(folder, "GeneCategories.xml"));
        var category = DirectXmlToObject.ObjectFromXml<GeneCategoryDef>(categoryDoc.DocumentElement.FirstChild, false);
        Check(category.defName == "UG_FluidsAndSolids" && category.label == "RJW Fluids and Solids", "shared gene category parses with the intended label");
        DefDatabase<GeneCategoryDef>.Add(category);
        var genesDoc = new XmlDocument(); genesDoc.Load(Path.Combine(folder, "Genes.xml"));
        Check(genesDoc.DocumentElement.ChildNodes.OfType<XmlElement>().Count() == 3, "all three custom genes are covered");
        foreach (XmlElement node in genesDoc.DocumentElement.ChildNodes.OfType<XmlElement>())
        {
            var gene = DirectXmlToObject.ObjectFromXml<GeneDef>(node, false);
            Check(node["displayCategory"]?.InnerText == category.defName, "every custom gene uses the common category");
            Check(gene.biostatCpx == 1 && gene.biostatArc == 0 && gene.selectionWeight > 0, "gene has normal genepack availability and specified complexity");
            if (gene.defName == "UG_PermanentLactation") Check(gene.geneClass == typeof(Gene_PermanentLactation) && gene.minAgeActive == 16 && gene.biostatMet == -1, "actual lactation gene type and activation age");
            if (gene.defName == "UG_Squirter") Check(gene.geneClass == typeof(Gene_Squirter) && gene.minAgeActive == 18, "Squirter has correct type and adult activation threshold");
        }
    }

    static void TestSquirter()
    {
        var output = new ThingDef { defName = "S16GirlCum", stackLimit = 25 };
        DefDatabase<ThingDef>.Add(output);
        var names = new HashSet<string> { "S16VibratorIHed", "S16VibratorIIHed", "S16VibratorIIIHed", "S16VibratorIIIIHed", "S16VibratorHed", "S16VibratorDoubleHed", "S16_NippleWristCuffs_MainHed" };
        string dir = Path.Combine(Environment.GetEnvironmentVariable("RIMWORLD_ROOT")!, "Mods/s16-extentions-1.5/Common/Defs/HediffDefs/Apparel");
        foreach (string path in Directory.GetFiles(dir, "*.xml"))
        {
            var doc = new XmlDocument(); doc.Load(path);
            foreach (XmlElement node in doc.DocumentElement.ChildNodes.OfType<XmlElement>())
            {
                if (!names.Contains(node["defName"]?.InnerText)) continue;
                var def = DirectXmlToObject.ObjectFromXml<HediffDef>(node, false);
                var props = def.comps.Single(c => c.GetType().FullName == "s16_extension.HediffCompProperties_ItemSpawner");
                // Fixture supplies the dependency cross-reference, as the full mod loader would.
                props.GetType().GetField("thingToSpawn").SetValue(props, output);
                DefDatabase<HediffDef>.Add(def);
            }
        }
        SquirterIntegration.Initialize(); Check(SquirterIntegration.Toys.Count == 7, "all seven installed S16 resource-producing toy/apparel definitions recognized");
        var p = PawnWithGene(); p.thingIDNumber = 811;
        p.def.race.lifeStageAges = new() { new LifeStageAge { minAge = 0, def = new LifeStageDef { developmentalStage = DevelopmentalStage.Adult } } };
        Set(p.ageTracker, "cachedLifeStageIndex", 0);
        p.jobs = new Verse.AI.Pawn_JobTracker(p);
        p.jobs.curJob = new Verse.AI.Job { loadID = 9001, def = new JobDef { defName = "FixtureActivity" } };
        var g = new Gene_Squirter { pawn = p, def = UsefulGeneDefOf.UG_Squirter };
        Set(p.genes, "endogenes", new List<Gene> { g }); Set(p.genes, "cachedGenes", null);
        Check(SquirterPolicy.Eligible(p), "adult carrier eligible");
        Fakes.Now = 100;
        var propsEvent = new SexProps(p, null) { orgasms = 1 };
        SexUtility.SatisfyPersonal(propsEvent); Check(g.pendingItems == 3, "shared RJW callback queues three existing love-juice items");
        SexUtility.SatisfyPersonal(propsEvent); Check(g.pendingItems == 3, "repeated callback cannot duplicate same ordinal");
        propsEvent.orgasms = 2; SexUtility.SatisfyPersonal(propsEvent);
        Check(g.pendingItems == 6, "second real ordinal in same job and tick pays separately");
        SexUtility.SatisfyPersonal(new SexProps(p, null) { orgasms = 2 });
        Check(g.pendingItems == 6, "reconstructed event with same job and ordinal is deduplicated");
        SexUtility.SatisfyPersonal(new SexProps(p, null) { orgasms = 1 });
        Check(g.pendingItems == 6, "late callback for an older ordinal cannot pay again");
        g.OnFallbackEvent(); Check(g.pendingItems == 6, "secondary signal for same event does not double output");
        Fakes.Now++; g.OnFallbackEvent(); g.OnFallbackEvent();
        Check(g.pendingItems == 9, "fallback signal pays once per event tick");
        propsEvent.orgasms = 3; SexUtility.SatisfyPersonal(propsEvent);
        Check(g.pendingItems == 9, "fallback followed by primary callback pays once");
        propsEvent.orgasms = 4; SexUtility.SatisfyPersonal(propsEvent);
        Check(g.pendingItems == 12, "additional distinct primary event after fallback still pays");
        int balance = g.pendingItems;
        g.TickInterval(120); Check(g.pendingItems == balance, "ordinary ticks without an event create no extra resources");
        var adultPartner = PawnWithGene();
        adultPartner.def.race.lifeStageAges = p.def.race.lifeStageAges;
        Set(adultPartner.ageTracker, "cachedLifeStageIndex", 0);
        SexUtility.SatisfyPersonal(new SexProps(p, adultPartner) { orgasms = 5 });
        Check(g.pendingItems == balance+3, "adult partnered activity produces the same three-item yield");
        balance = g.pendingItems;
        Set(p.ageTracker, "ageBiologicalTicksInt", 17L*3600000);
        SexUtility.SatisfyPersonal(new SexProps(p, null) { orgasms = 10 }); Check(g.pendingItems == balance, "under-18 source produces no items");
        Set(p.ageTracker, "ageBiologicalTicksInt", 18L*3600000);
        Check(SquirterPolicy.Eligible(p), "adult life stage at age 18 eligible");
        p.def.race.lifeStageAges[0].def.developmentalStage = DevelopmentalStage.Child;
        Check(!SquirterPolicy.Eligible(p), "nonadult life stage excluded even when old enough");
        p.def.race.lifeStageAges[0].def.developmentalStage = DevelopmentalStage.Adult | DevelopmentalStage.Child;
        Check(!SquirterPolicy.Eligible(p), "mixed child/adult life stages excluded");
        p.def.race.lifeStageAges[0].def.developmentalStage = DevelopmentalStage.Adult;
        var partner = PawnWithGene(); Set(partner.ageTracker, "ageBiologicalTicksInt", 17L*3600000);
        SexUtility.SatisfyPersonal(new SexProps(p, partner) { orgasms = 10 }); Check(g.pendingItems == balance, "nonadult participant excluded");
        g.overriddenByGene = new Gene();
        SexUtility.SatisfyPersonal(new SexProps(p, null) { orgasms = 11 }); Check(g.pendingItems == balance, "suppressed gene produces no items");
        g.overriddenByGene = null;

        foreach (var entry in SquirterIntegration.Toys)
        {
            var hed = new HediffWithComps { def = entry.Key, pawn = p };
            Set(hed, "severityInt", 0.01f);
            balance = g.pendingItems;
            hed.Severity = entry.Value.threshold - 0.001f;
            Check(g.pendingItems == balance, "toy progress below completion produces nothing");
            hed.Severity = entry.Value.threshold;
            Check(g.pendingItems == balance + 3*entry.Value.normalAmount, "toy crossing emits exactly triple its installed baseline: " + entry.Key.defName);
            hed.Severity = entry.Value.threshold; hed.Severity += 0.005f;
            Check(g.pendingItems == balance + 3*entry.Value.normalAmount, "remaining at or above completion cannot repeat output");
            hed.Severity = 0.01f; hed.Severity = 1f;
            Check(g.pendingItems == balance + 6*entry.Value.normalAmount, "new toy cycle and skipped severity interval emit once");

            var props = entry.Key.comps.Single(c => c.GetType().FullName == "s16_extension.HediffCompProperties_ItemSpawner");
            var comp = (HediffComp)Activator.CreateInstance(props.compClass);
            comp.parent = hed; comp.props = props;
            Set(comp, "SpawningTicker", 100000);
            Patch_SquirterToyTimer.TargetMethod().Invoke(comp, null);
            Check((int)AccessTools.Field(comp.GetType(), "SpawningTicker").GetValue(comp) == 100000, "carrier's periodic timer does not run alongside completion output");
            g.overriddenByGene = new Gene();
            Patch_SquirterToyTimer.TargetMethod().Invoke(comp, null);
            Check((int)AccessTools.Field(comp.GetType(), "SpawningTicker").GetValue(comp) == 0, "noncarrier toy behavior remains native");
            g.overriddenByGene = null;
        }

        int before = g.pendingItems;
        Fakes.Spawned.Add(p); Fakes.PlaceLimit = 0;
        g.Flush(); Check(g.pendingItems == before, "blocked placement retains queued output");
        Fakes.PlaceLimit = 2; g.Flush(); Check(g.pendingItems == before-2, "partial placement subtracts only actual items");
        Fakes.PlaceLimit = int.MaxValue; g.Flush(); Check(g.pendingItems == 0, "stack-split output flushes without loss");
        Check(Fakes.PlacedItems == before, "confirmed item placements conserve entire queue");
        Fakes.Spawned.Clear();
        Fakes.Now++;
        propsEvent.orgasms = 20; SexUtility.SatisfyPersonal(propsEvent);
        string file = Path.Combine(AppContext.BaseDirectory, "squirter-save.xml");
        Gene_Squirter saved = g;
        Scribe.saver.InitSaving(file, "SquirterFixture"); Scribe_Deep.Look(ref saved, "gene"); Scribe.saver.FinalizeSaving();
        Gene_Squirter loaded = null;
        Scribe.loader.InitLoading(file);
        ((LoadedObjectDirectory)AccessTools.Field(typeof(CrossRefHandler), "loadedObjectDirectory").GetValue(Scribe.loader.crossRefs)).RegisterLoaded(p);
        Scribe_Deep.Look(ref loaded, "gene"); Scribe.loader.FinalizeLoading();
        Check(loaded != null && loaded.pendingItems == 3 && loaded.pawn == p && loaded.def == UsefulGeneDefOf.UG_Squirter, "real Scribe round trip preserves queued items and gene references");
        Set(p.genes, "endogenes", new List<Gene> { loaded }); Set(p.genes, "cachedGenes", null);
        SexUtility.SatisfyPersonal(new SexProps(p, null) { orgasms = 20 });
        Check(loaded.pendingItems == 3, "saved event receipt prevents replay after load");
        SexUtility.SatisfyPersonal(new SexProps(p, null) { orgasms = 21 });
        Check(loaded.pendingItems == 6, "new completion after load still produces output");
        Set(p.genes, "endogenes", new List<Gene>()); Set(p.genes, "cachedGenes", null);
        SexUtility.SatisfyPersonal(new SexProps(p, null) { orgasms = 22 }); Check(loaded.pendingItems == 6, "removal stops new production");
        Console.WriteLine("Squirter: shared events, all installed S16 toy thresholds, adult eligibility, deduplication, stack placement and real Scribe passed.");
    }

    static void TestWasteAndCoffee()
    {
        var folder = FindModFolder();
        var doc = new XmlDocument(); doc.Load(Path.Combine(folder, "ThingDefs/Items_Waste.xml"));
        var weapon = new XmlDocument(); weapon.Load(Path.Combine(folder, "Weapon.xml"));
        var projectileDef = DirectXmlToObject.ObjectFromXml<ThingDef>(weapon.SelectSingleNode("Defs/ThingDef"), false);
        Check(projectileDef.thingClass == typeof(Projectile_Feces), "actual weapon XML selects impact class");
        Check((int)AccessTools.Field(typeof(ProjectileProperties), "damageAmountBase").GetValue(projectileDef.projectile) == 1 && (float)AccessTools.Field(typeof(ProjectileProperties), "armorPenetrationBase").GetValue(projectileDef.projectile) == 0, "actual projectile XML deals minimal damage with zero added penetration");
        var nauseaDef = DirectXmlToObject.ObjectFromXml<HediffDef>(weapon.SelectSingleNode("Defs/HediffDef"), false);
        var disappears = nauseaDef.comps.OfType<HediffCompProperties_Disappears>().Single();
        Check(disappears.disappearsAfterTicks.min == 600 && disappears.disappearsAfterTicks.max == 600, "nausea cooldown is exactly 600 ticks");
        var fecesNode = doc.SelectSingleNode("Defs/ThingDef[defName='Feces']");
        Check(fecesNode.SelectSingleNode("statBases/FoodPoisonChanceFixedHuman").InnerText == "0", "feces has zero configured poisoning chance");
        Check(fecesNode.SelectSingleNode("comps/li[@Class='Umrae.UsefulGenes.CompProperties_FecesThrow']") != null, "feces resource exposes throw command");
        var feces = new ThingDef { defName = "Feces", stackLimit = 75, thingClass = typeof(Thing) };
        var urine = new ThingDef { defName = "UrineBottle", stackLimit = 25, thingClass = typeof(Thing) };
        DefDatabase<ThingDef>.Add(feces); DefDatabase<ThingDef>.Add(urine);
        Check(!Patch_FecesPoisoning.Prefix(new Thing { def = feces }), "all ingestion poisoning paths skip feces");
        Check(Patch_FecesPoisoning.Prefix(new Thing { def = new ThingDef { defName = "Meal" } }), "ordinary food poisoning remains enabled");
        Check(Patch_FecesPoisoning.Prefix(null), "unrelated null-source poisoning remains enabled");
        foreach (int limit in new[] { 0, 2, int.MaxValue })
        {
            Fakes.PlaceLimit = limit; Fakes.PlacedItems = 0;
            int received = CollectedWaste.CollectedWasteUtility.Drop(Fakes.FixtureMap, IntVec3.Zero, feces, 160);
            int expected = limit == int.MaxValue ? 160 : limit;
            Check(received == expected && Fakes.PlacedItems == expected, "bucket output debits only confirmed partial/full placements");
        }
        Fakes.PlaceLimit = int.MaxValue;
        var bucket = new CollectedWaste.Building_CollectionBucket();
        for (int i = 0; i < 8; i++) Check(bucket.TryUseFlush(), "bucket accepts uses until capacity");
        Check(bucket.IsFull && !bucket.TryUseFlush() && bucket.urine == 8 && bucket.feces == 8, "full bucket cannot overflow");
        var ammo = new Thing { def = feces, stackCount = 3 };
        Check(CollectedWaste.CollectedWasteUtility.ConsumeOne(null, ammo) && ammo.stackCount == 2, "throw consumes one item from a stack");
        Check(!CollectedWaste.CollectedWasteUtility.ConsumeOne(null, null), "missing ammunition cannot be consumed");
        for (int mask = 0; mask < 64; mask++)
        {
            bool flesh = (mask & 1) != 0, dead = (mask & 2) != 0, downed = (mask & 4) != 0,
                 food = (mask & 8) != 0, shield = (mask & 16) != 0, cooldown = (mask & 32) != 0;
            bool eligible = flesh && !dead && !downed && food && !shield && !cooldown;
            Check(FecesWeapon.ShouldVomit(flesh, dead, downed, food, shield, cooldown, 0.1f) == eligible, "vomit eligibility covers flesh, death, downed, needs, shield and cooldown");
            Check(!FecesWeapon.ShouldVomit(flesh, dead, downed, food, shield, cooldown, 0.3f), "roll at or above 30% does not cause vomiting");
        }
        Check(BackCompatibility.BackCompatibleDefName(typeof(ThingDef), "FecalWall") == "UG_FecalWall", "native save lookup migrates removed standalone wall");
        Check(BackCompatibility.BackCompatibleDefName(typeof(ThingDef), "OrdinaryWall") == "OrdinaryWall", "ordinary save definitions are unchanged");
        doc.Load(Path.Combine(folder, "WasteBeliefs.xml"));
        foreach (string resource in new[] { "Feces", "UrineBottle" })
        foreach (string attitude in new[] { "Disapproved", "Acceptable", "Approved" })
        {
            string name = "RFS_" + (resource == "Feces" ? "Feces" : "Urine") + attitude;
            var preceptDef = DirectXmlToObject.ObjectFromXml<PreceptDef>(doc.SelectSingleNode("Defs/PreceptDef[defName='" + name + "']"), false);
            var ext = preceptDef.GetModExtension<WasteBeliefExtension>();
            var thought = attitude == "Acceptable" ? null : new ThoughtDef { defName = name + "Thought" };
            ext.thought = thought;
            var precept = new Precept { def = preceptDef };
            var other = new ThoughtDef { defName = "OtherThought" };
            var list = new List<FoodUtility.ThoughtFromIngesting> { new() { thought = new ThoughtDef { defName = resource == "Feces" ? "AteFeces" : "DrankUrine" } }, new() { thought = other } };
            WasteBeliefs.Apply(list, new[] { precept }, resource); WasteBeliefs.Apply(list, new[] { precept }, resource);
            Check(list.Count == (thought == null ? 1 : 2) && list.Any(t => t.thought == other), "belief replaces only the base taste penalty and does not duplicate thoughts");
            if (thought != null) Check(list.Single(t => t.thought == thought).fromPrecept == precept, "belief thought retains its ideology source");
        }
        var before = new List<FoodUtility.ThoughtFromIngesting> { new() { thought = new ThoughtDef { defName = "AteFeces" } } };
        WasteBeliefs.Apply(before, Array.Empty<Precept>(), "Feces"); Check(before.Count == 1, "no waste precept preserves original taste thought");

        CoffeeSupport.ModuleFolder = Path.Combine(Environment.GetEnvironmentVariable("RIMWORLD_ROOT")!, "Mods/coffees-rjw-ideology-addons/CRIALactation");
        var coffeeXml = new XmlDocument(); coffeeXml.LoadXml("<Defs><ThingDef Name='BasePawn'><comps/></ThingDef><ThingDef><defName>SEX_BreastMilk</defName></ThingDef></Defs>");
        var patch = new PatchOperation_CoffeeMilk();
        AccessTools.Method(typeof(PatchOperation_CoffeeMilk), "ApplyWorker").Invoke(patch, new object[] { coffeeXml });
        var config = coffeeXml.SelectSingleNode("Defs/Umrae.UsefulGenes.Coffee.MilkConfigDef[defName='GlobalMilkSettings']/MilkThingDef");
        Check(config != null && config.SelectNodes("li[text()='SEX_BreastMilk']").Count == 1, "Coffee original config recognizes gene-produced milk");
        Check(config.SelectSingleNode("li[text()='EM_HumanMilk']") == null && config.SelectSingleNode("li[text()='MooGirl_Milk']") == null, "missing optional milk dependencies leave no unresolved references");
        int defs = coffeeXml.DocumentElement.ChildNodes.Count;
        AccessTools.Method(typeof(PatchOperation_CoffeeMilk), "ApplyWorker").Invoke(patch, new object[] { coffeeXml });
        Check(coffeeXml.DocumentElement.ChildNodes.Count == defs, "reapplying Coffee compatibility cannot duplicate definitions");
        Check(coffeeXml.SelectNodes("Defs/IssueDef[defName='Lactating']").Count == 1, "reuses Coffee's existing milk issue");
        Check(coffeeXml.SelectNodes("Defs/ThingDef[@Name='BasePawn']/comps/li").Count == 1, "one saved milk-consumption component per pawn");
        foreach (XmlElement node in coffeeXml.DocumentElement.ChildNodes.OfType<XmlElement>())
        {
            var type = GenTypes.GetTypeInAnyAssembly(node.Name, "Verse") ?? GenTypes.GetTypeInAnyAssembly(node.Name, "RimWorld");
            Check(type != null, "adapted Coffee def type resolves " + node.Name); ValidateFields(node, type);
        }
        foreach (XmlNode n in coffeeXml.SelectNodes("//*[@Class]"))
            Check(GenTypes.GetTypeInAnyAssembly(n.Attributes["Class"].Value, "Verse") != null || GenTypes.GetTypeInAnyAssembly(n.Attributes["Class"].Value, "RimWorld") != null, "Coffee custom component type resolves: " + n.Attributes["Class"].Value);
        foreach (XmlNode n in coffeeXml.SelectNodes("//workerClass|//driverClass|//giverClass"))
            Check(GenTypes.GetTypeInAnyAssembly(n.InnerText.Trim(), "Verse") != null || GenTypes.GetTypeInAnyAssembly(n.InnerText.Trim(), "RimWorld") != null, "Coffee worker type resolves: " + n.InnerText.Trim());
        var eventDef = new HistoryEventDef { defName = "DrankMilkRaw" };
        var milkThought = new ThoughtDef { defName = "DrankMilkRaw_Essential" };
        var milkPrecept = new Precept { def = new PreceptDef { comps = new() { new PreceptComp_SelfTookMemoryThought { eventDef = eventDef, thought = milkThought } } } };
        var thoughts = new List<FoodUtility.ThoughtFromIngesting>();
        CoffeeSupport.AddEventThoughts(thoughts, new[] { milkPrecept }, "DrankMilkRaw"); CoffeeSupport.AddEventThoughts(thoughts, new[] { milkPrecept }, "DrankMilkRaw");
        Check(thoughts.Count == 1 && thoughts[0].thought == milkThought, "Coffee milk thought occurs once");
        var milk = LactationExpansion_ThingDefOf.SEX_BreastMilk;
        Check(CoffeeSupport.IsMilk(milk) && !CoffeeSupport.IsMilk(feces), "only milk counts as milk");
        var meal = new ThingWithComps { def = new ThingDef { defName = "Meal" } };
        var ingredients = new CompIngredients { parent = meal, ingredients = new() { milk } };
        Set(meal, "comps", new List<ThingComp> { ingredients });
        Check(CoffeeSupport.ContainsMilk(meal, meal.def), "milk in a cooked meal is recognized");
        var p = PawnWithGene(); Breast(p, .5f); Set(p.ageTracker, "cachedLifeStageIndex", 0); p.def.race.lifeStageAges = new() { new LifeStageAge { def = new LifeStageDef { developmentalStage = DevelopmentalStage.Adult } } };
        var tracker = new Umrae.UsefulGenes.Coffee.CompLactation { parent = p }; Set(p, "comps", new List<ThingComp> { tracker });
        Fakes.Now = 123456;
        Patch_CoffeeIngested.Postfix(meal, p, 0); Check(tracker.lastHumanLactationIngestedTick == 0, "food scoring or failed ingestion cannot satisfy milk belief");
        Patch_CoffeeIngested.Postfix(meal, p, .1f); Check(tracker.lastHumanLactationIngestedTick == 123456, "actually eating milk ingredients satisfies Coffee's recent milk belief");
        string file = Path.Combine(AppContext.BaseDirectory, "coffee-save.xml");
        Scribe.saver.InitSaving(file, "CoffeeFixture"); tracker.PostExposeData(); Scribe.saver.FinalizeSaving();
        var restored = new Umrae.UsefulGenes.Coffee.CompLactation(); Scribe.loader.InitLoading(file); restored.PostExposeData(); Scribe.loader.FinalizeLoading();
        Check(restored.lastHumanLactationIngestedTick == 123456, "Coffee milk timestamp survives actual Scribe save/load");
        var speed = new StatDef { defName = "MilkProductionSpeed" }; var yield = new StatDef { defName = "MilkProductionYield" }; DefDatabase<StatDef>.Add(speed); DefDatabase<StatDef>.Add(yield);
        var lact = (HediffWithComps)HediffMaker.MakeHediff(HediffDefOf.Lactating, p); var comp = lact.TryGetComp<HediffComp_Lactation>();
        comp.RecacheStats(); Near(comp.lactationPerDay, 7.5f, "Coffee 1.5 speed and 2.5 yield multiply gene output once"); Near(comp.maximumNutritionStored, 5f, "Coffee yield scales storage");
        comp.RecacheStats(); Near(comp.lactationPerDay, 7.5f, "Coffee stats never compound on recache");
        float adjustment = 0; comp.currentNutritionStored = 0; p.needs.food.CurLevel = 1;
        comp.CompPostTickInterval(ref adjustment, 120);
        Near(comp.currentNutritionStored, .015f, "Coffee speed and yield produce the exact expected milk gain");
        Near(p.needs.food.CurLevel, .9925f, "Coffee-buffed output pays half its actual milk nutrition");
        Umrae.UsefulGenes.Coffee.HediffDefOf_Milk.Lactating = HediffDefOf.Lactating;
        p.health.hediffSet.hediffs.Add(lact);
        Umrae.UsefulGenes.Coffee.LactationUtility.StartLactating(p, true); Umrae.UsefulGenes.Coffee.LactationUtility.StartLactating(p, true);
        Check(p.health.hediffSet.hediffs.Count(h => h.def == HediffDefOf.Lactating) == 1, "Coffee reuses the existing lactation condition");
        var roleRequirement = new Umrae.UsefulGenes.Coffee.RoleRequirement_Lactating();
        Check(roleRequirement.Met(p, null), "adult lactating carriers qualify for the Coffee role");
        Set(p.ageTracker, "ageBiologicalTicksInt", 16L * 3600000);
        Check(!roleRequirement.Met(p, null), "Coffee role requires adults even when the production gene is active at 16");
        Set(p.ageTracker, "ageBiologicalTicksInt", 25L * 3600000);
        bucket.def = new ThingDef { defName = "CollectionBucket", category = ThingCategory.Building, thingClass = typeof(CollectedWaste.Building_CollectionBucket), building = new BuildingProperties() };
        DefDatabase<ThingDef>.Add(bucket.def); bucket.thingIDNumber = 987;
        string bucketFile = Path.Combine(AppContext.BaseDirectory, "bucket-save.xml");
        Scribe.saver.InitSaving(bucketFile, "BucketFixture"); bucket.ExposeData(); Scribe.saver.FinalizeSaving();
        var bucketRestored = new CollectedWaste.Building_CollectionBucket { def = bucket.def };
        Scribe.loader.InitLoading(bucketFile); bucketRestored.ExposeData(); Scribe.loader.FinalizeLoading();
        Check(bucketRestored.urine == 8 && bucketRestored.feces == 8 && bucketRestored.IsFull, "native DBH bucket save/load preserves both output counts");
        Console.WriteLine("Collected waste transfers, beliefs, poisoning, hit policy, save migration and Coffee original definitions/consumption/stats passed.");
    }

    static void TestStorageAndSettings()
    {
        var h = new Harmony("umrae.usefulgenes.tests.storage");
        Fake(h, AccessTools.Method(typeof(StoreUtility), "GetSlotGroup", new[] { typeof(Thing) }), nameof(Fakes.GetSlotGroup));
        var group = Empty<SlotGroup>(); Set(group, "parent", new Building_SealedWasteContainer()); Fakes.StorageGroup = group;
        var part = new StatPart_SealedWaste();
        foreach (string name in new[] { "Feces", "UrineBottle", "OrdinaryMeal" })
        {
            var item = new Thing { def = new ThingDef { defName = name } };
            Fakes.Spawned.Add(item); float value = -25;
            part.TransformValue(StatRequest.For(item), ref value);
            Near(value, name == "OrdinaryMeal" ? -25 : 0, "sealed storage removes only waste resource penalties");
            Fakes.Spawned.Remove(item); value = -25;
            part.TransformValue(StatRequest.For(item), ref value); Near(value, -25, "removing waste from sealed storage restores its penalty");
        }
        Set(group, "parent", new Building_Storage());
        var loose = new Thing { def = new ThingDef { defName = "Feces" } }; Fakes.Spawned.Add(loose);
        float loosePenalty = -25; part.TransformValue(StatRequest.For(loose), ref loosePenalty); Near(loosePenalty, -25, "ordinary shelves do not gain sealed cleanliness protection");
        Fakes.Spawned.Remove(loose); Fakes.StorageGroup = null;
        var document = new XmlDocument(); document.Load(Path.Combine(FindModFolder(), "SealedWasteContainer.xml"));
        var container = DirectXmlToObject.ObjectFromXml<ThingDef>(document.DocumentElement.FirstChild, false);
        Check(container.thingClass == typeof(Building_SealedWasteContainer) && container.building.maxItemsInCell == 3, "native sealed container XML supplies triple-stack storage class");
        var allowed = document.SelectNodes("Defs/ThingDef/building/fixedStorageSettings/filter/thingDefs/li").Cast<XmlNode>().Select(n => n.InnerText).ToHashSet();
        Check(allowed.SetEquals(new[] { "Feces", "UrineBottle" }), "sealed container accepts only collected waste");
        var patchDoc = new XmlDocument(); patchDoc.Load(Path.Combine(FindModFolder(), "../Patches/SealedWaste.xml"));
        var operation = DirectXmlToObject.ObjectFromXml<PatchOperation>(patchDoc.SelectSingleNode("Patch/Operation"), false);
        var stats = new XmlDocument(); stats.LoadXml("<Defs><StatDef><defName>Beauty</defName><parts/></StatDef><StatDef><defName>Cleanliness</defName></StatDef></Defs>");
        Check((bool)AccessTools.Method(operation.GetType(), "ApplyWorker").Invoke(operation, new object[] { stats }), "storage XML patch applies through the native patch engine");
        Check(stats.SelectNodes("Defs/StatDef/parts/li[@Class='Umrae.UsefulGenes.StatPart_SealedWaste']").Count == 2, "both beauty and cleanliness receive protection even when only one parts list existed");
        var previous = UsefulGenesMod.Settings;
        var config = new UsefulGenesSettings { throwRange = 17, vomitChance = .75f, vomitCooldownSeconds = 24 };
        UsefulGenesMod.Settings = config;
        Near(FecesWeapon.Range, 17, "throwing uses configured range");
        Check(FecesWeapon.ShouldVomit(true, false, false, true, false, false, .7f), "configured vomiting probability changes hit outcomes");
        config.vomitChance = 0; Check(!FecesWeapon.ShouldVomit(true, false, false, true, false, false, 0), "zero chance disables vomiting");
        config.vomitChance = 1; Check(FecesWeapon.ShouldVomit(true, false, false, true, false, false, .9999f), "100 percent chance guarantees an eligible hit");
        config.vomitChance = .75f;
        string file = Path.Combine(AppContext.BaseDirectory, "weapon-settings.xml");
        Scribe.saver.InitSaving(file, "SettingsFixture"); config.ExposeData(); Scribe.saver.FinalizeSaving();
        var loaded = new UsefulGenesSettings(); Scribe.loader.InitLoading(file); loaded.ExposeData(); Scribe.loader.FinalizeLoading();
        Near(loaded.throwRange, 17, "range survives actual Scribe round trip"); Near(loaded.vomitChance, .75f, "vomiting chance persists"); Near(loaded.vomitCooldownSeconds, 24, "cooldown persists");
        config.throwRange = float.NaN; config.vomitChance = float.PositiveInfinity; config.vomitCooldownSeconds = -10; config.Sanitize();
        Near(config.throwRange, 12, "invalid range repaired"); Near(config.vomitChance, .3f, "invalid chance repaired"); Near(config.vomitCooldownSeconds, 1, "cooldown stays within safe UI range");
        UsefulGenesMod.Settings = previous;
        Console.WriteLine("Sealed waste penalties, native storage definitions/patches, adjustable weapon policy and saved settings passed.");
    }

    static void TestProgression()
    {
        var xml = new XmlDocument(); xml.LoadXml("<Defs/>");
        void ImportFile(string file)
        {
            var doc = new XmlDocument(); doc.Load(file);
            foreach (XmlNode node in doc.DocumentElement.ChildNodes) if (node is XmlElement) xml.DocumentElement.AppendChild(xml.ImportNode(node, true));
        }
        string workshop = Environment.GetEnvironmentVariable("RIMWORLD_WORKSHOP")! + Path.DirectorySeparatorChar;
        ImportFile(workshop + "2988129340/1.6/Defs/MilkNet.xml");
        ImportFile(workshop + "3489491945/1.6/Defs/ThingDefs_Items/Items_Resource_AnimalProduct.xml");
        ImportFile(Path.Combine(FindModFolder(), "SealedWasteContainer.xml"));
        ImportFile(Path.Combine(FindModFolder(), "ThingDefs/Buildings_CollectionBucket.xml"));
        var extra = new XmlDocument(); extra.LoadXml("<Defs><ResearchProjectDef><defName>Ferny_Barrels</defName></ResearchProjectDef><VFETribals.TribalResearchProjectDef><defName>VFET_Construction</defName></VFETribals.TribalResearchProjectDef><ThingDef><defName>PitLatrine</defName><researchPrerequisites><li>VFET_Construction</li></researchPrerequisites></ThingDef><ThingCategoryDef><defName>DankPyon_Milk</defName></ThingCategoryDef><ThingCategoryDef><defName>ROBTRG_MilkTypes</defName></ThingCategoryDef><ThingCategoryDef><defName>RJW_Gastronomy_Humilk</defName></ThingCategoryDef></Defs>");
        foreach (XmlNode node in extra.DocumentElement.ChildNodes) xml.DocumentElement.AppendChild(xml.ImportNode(node, true));
        var operation = new PatchOperation_ProgressionSupport();
        AccessTools.Method(operation.GetType(), "ApplyWorker").Invoke(operation, new object[] { xml });
        string snapshot = xml.OuterXml;
        AccessTools.Method(operation.GetType(), "ApplyWorker").Invoke(operation, new object[] { xml });
        Check(xml.OuterXml == snapshot, "Progression support is idempotent across categories, research and all network definitions");
        Check(xml.SelectSingleNode("Defs/ThingDef[defName='RFS_SealedWasteContainer']/researchPrerequisites/li").InnerText == "Ferny_Barrels", "sealed storage follows Progression barrel research");
        Check(xml.SelectSingleNode("Defs/ThingDef[defName='CollectionBucket']/researchPrerequisites/li").InnerText == "VFET_Construction", "collection bucket follows Progression primitive sanitation");
        foreach (string category in new[] { "DankPyon_Milk", "ROBTRG_MilkTypes", "RJW_Gastronomy_Humilk" })
            Check(xml.SelectNodes("Defs/ThingDef[defName='SEX_BreastMilk']/thingCategories/li[text()='" + category + "']").Count == 1, "milk is recognized once by " + category);
        var net = xml.SelectSingleNode("Defs/PipeSystem.PipeNetDef[defName='RFS_BreastMilkNetwork']");
        Check(net != null && net.SelectSingleNode("pipeDefs/li").InnerText == "RFS_BreastMilkPipe", "human milk has its own pipe network");
        ValidateFields((XmlElement)net, typeof(PipeSystem.PipeNetDef));
        var parsedNetwork = DirectXmlToObject.ObjectFromXml<PipeSystem.PipeNetDef>(net, false);
        Check(parsedNetwork.defName == "RFS_BreastMilkNetwork", "optional network parses using the installed pipe framework");
        foreach (string name in new[] { "RFS_BreastMilkPipe", "RFS_UndergroundBreastMilkPipe", "RFS_BreastMilkTank", "RFS_BreastMilkDrain", "RFS_BreastMilkTap" })
        {
            var building = xml.SelectSingleNode("Defs/ThingDef[defName='" + name + "']");
            Check(building != null && building.SelectNodes("comps/li/pipeNet[text()='RFS_BreastMilkNetwork']").Count == 1, "all optional pipe structures connect to only the breast-milk network");
            Check(building.SelectSingleNode("researchPrerequisites/li").InnerText == "Electricity", "existing network research is preserved");
            var parsed = DirectXmlToObject.ObjectFromXml<ThingDef>(building, false);
            Check(parsed != null && parsed.comps.Count > 0, "cloned network building parses against installed PipeSystem classes");
            foreach (XmlElement comp in building.SelectNodes("comps/li"))
            {
                var type = GenTypes.GetTypeInAnyAssembly(comp.GetAttribute("Class"), "Verse") ?? GenTypes.GetTypeInAnyAssembly(comp.GetAttribute("Class"), "RimWorld");
                Check(type != null, "optional network comp class resolves"); ValidateFields(comp, type);
            }
        }
        foreach (string endpoint in new[] { "RFS_BreastMilkDrain", "RFS_BreastMilkTap" })
            Check(xml.SelectSingleNode("Defs/ThingDef[defName='" + endpoint + "']/comps/li/thing").InnerText == "SEX_BreastMilk", "both network endpoints preserve the produced item identity");
        Check(xml.SelectSingleNode("Defs/ThingDef[defName='MP_MilkTap']/comps/li/thing").InnerText == "Milk", "animal-milk network remains unchanged");
        Check(xml.SelectSingleNode("Defs/ThingDef[defName='RFS_BreastMilkTap']/comps/li[@Class='VNPE.CompProperties_RegisterToGrinder']") == null, "breast milk cannot silently mix into animal-milk grinder network");
        var absent = new XmlDocument(); absent.LoadXml("<Defs><ThingDef><defName>SEX_BreastMilk</defName></ThingDef></Defs>");
        var original = absent.OuterXml;
        AccessTools.Method(operation.GetType(), "ApplyWorker").Invoke(operation, new object[] { absent });
        Check(absent.OuterXml == original, "optional mods absent creates no dangling references or extra structures");
        var h = new Harmony("umrae.usefulgenes.tests.adaptive-storage");
        var storage = new Building_Storage { def = new ThingDef { defName = "ConvertedSealedStorage", modExtensions = new() { new SealedWasteContainerExtension() } } };
        var slot = Empty<SlotGroup>(); Set(slot, "parent", storage); Fakes.StorageGroup = slot;
        var item = new Thing { def = new ThingDef { defName = "Feces" } }; Fakes.Spawned.Add(item);
        Check(SealedWasteStorage.IsSealed(item), "definition marker preserves sealing when a storage framework replaces the building class");
        Fakes.Spawned.Remove(item); Fakes.StorageGroup = null;
        var held = new FixtureStorageHolder { def = storage.def };
        // Populate held membership without the absent headless world notification manager.
        Set(item, "holdingOwner", held.contents);
        Check(!item.Spawned && SealedWasteStorage.IsSealed(item), "held contents remain sealed with Adaptive Storage-style ThingOwner storage");
        Console.WriteLine("Progression research, milk categories, separate pipe network, optional-mod absence and framework-held storage passed.");
    }

    static string FindModFolder()
    {
        foreach (var start in new[] { Directory.GetCurrentDirectory(), AppContext.BaseDirectory })
        for (var dir = new DirectoryInfo(start); dir != null; dir = dir.Parent)
        foreach (var suffix in new[] { "1.7/Defs", "outputs/RJWFluidsAndSolids/1.7/Defs" })
        {
            var candidate = Path.Combine(dir.FullName, suffix);
            if (Directory.Exists(candidate)) return candidate;
        }
        throw new DirectoryNotFoundException("Cannot locate UsefulGenes definitions.");
    }

    static void ValidateFields(XmlElement node, Type type)
    {
        foreach (XmlElement child in node.ChildNodes.OfType<XmlElement>())
        {
            var field = AccessTools.Field(type, child.Name);
            Check(field != null, $"XML field {type.Name}.{child.Name} exists");
            if (child.Name is "building" or "graphicData" or "blueprintGraphicData" or "projectile") ValidateFields(child, field.FieldType);
        }
    }
}

public static class Fakes
{
    public static readonly HashSet<Thing> Spawned = new();
    public static readonly Map FixtureMap = Program.Empty<Map>();
    public static SlotGroup StorageGroup;
    public static bool GetSlotGroup(ref SlotGroup __result) { __result = StorageGroup; return false; }
    public static int PlaceLimit = int.MaxValue;
    public static int PlacedItems;
    public static int Now;
    public static TickManager Manager;
    public static float StatFactor = 1;
    public static bool False(ref bool __result) { __result = false; return false; }
    public static bool MaxFood(ref float __result) { __result = 1; return false; }
    public static bool Clock(ref TickManager __result) { __result = Manager; return false; }
    public static bool Ticks(ref int __result) { __result = Now; return false; }
    public static bool Noop() => false;
    public static bool IsSpawned(Thing __instance, ref bool __result) { __result = Spawned.Contains(__instance); return false; }
    public static bool MapFor(Thing __instance, ref Map __result) { __result = Spawned.Contains(__instance) ? FixtureMap : null; return false; }
    public static bool Place(Thing thing, out Thing lastResultingThing, Action<Thing,int> placedAction, ref bool __result)
    {
        int n = Math.Min(thing.stackCount, PlaceLimit);
        int first = n/2;
        placedAction?.Invoke(thing, first); placedAction?.Invoke(thing, n-first);
        PlacedItems += n; lastResultingThing = thing; __result = n == thing.stackCount;
        return false;
    }
    public static bool Stats(StatDef stat, ref float __result)
    {
        if (stat?.defName == "MilkProductionSpeed") { __result = 1.5f; return false; }
        if (stat?.defName == "MilkProductionYield") { __result = 2.5f; return false; }
        if (stat?.defName is not ("SEX_LactationPerDay" or "SEX_LactationCapacity")) return true;
        __result = 0.5f * StatFactor; return false;
    }
    public static bool MakeHediff(HediffDef def, Pawn pawn, ref Hediff __result)
    {
        if (def != HediffDefOf.Lactating) return true;
        var hed = new HediffWithComps { def = def, pawn = pawn };
        var comp = new HediffComp_Lactation { parent = hed, props = new HediffCompProperties_Lactation() };
        AccessTools.Field(typeof(HediffWithComps), "comps").SetValue(hed, new List<HediffComp> { comp });
        comp.RecacheStats(); __result = hed; return false;
    }
    public static bool AddHediff(Pawn_HealthTracker __instance, Hediff hediff)
    { __instance.hediffSet.hediffs.Add(hediff); return false; }
    public static bool MakeThing(ThingDef def, ref Thing __result)
    {
        if (def != LactationExpansion_ThingDefOf.SEX_BreastMilk && def != SquirterIntegration.Output && def?.defName is not ("Feces" or "UrineBottle")) return true;
        __result = def.defName is "Feces" or "UrineBottle" ? new FixtureWaste { def = def } : new Thing { def = def }; return false;
    }
}

// Headless placement has no world reservation manager. Unplaced fixture items need no cleanup there.
public sealed class FixtureWaste : Thing
{ public override void Destroy(DestroyMode mode = DestroyMode.Vanish) { } }

public sealed class FixtureStorageHolder : ThingWithComps, IThingHolder
{
    public ThingOwner contents;
    public FixtureStorageHolder() => contents = new ThingOwner<Thing>(this);
    public ThingOwner GetDirectlyHeldThings() => contents;
    public void GetChildHolders(List<IThingHolder> outChildren) { }
}
