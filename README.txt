RJW FLUIDS AND SOLIDS
Version 1.3.1 | Built for your installed RimWorld 1.7 Mac compatibility runtime

INSTALL
1. Close RimWorld. Copy the RJWFluidsAndSolids folder into RimWorld/Mods.
2. Remove the older UsefulGenes folder if you installed a previous release.
3. Remove/disable standalone DBH Collected Waste. Its content is included here;
   enabling both causes duplicate definitions.
4. Enable "RJW Fluids and Solids" after all dependencies and compatibility patches.
   Required: Harmony, Biotech, Ideology, RimJobWorld, S16's Extension,
   Lactation Expansion, Dubs Bad Hygiene and C0ffeeRIA (Coffee's RJW Ideology Add-ons).
   Load after RJW Genes and RJW Stack Compatibility if present.
5. Restart RimWorld. Existing pawns do not automatically gain any genes.

Your Mods folder:
<RimWorld installation>/Mods

GET THE GENES
All three genes appear in the xenotype editor and random genepacks. They can be extracted
normally. Add them to existing pawns using normal gene implantation or a developer
gene editor. Anty is not required.

UPDATING / COLLECTED WASTE MIGRATION
The visible name and folder changed; package ID umrae.usefulgenes, assembly identity
and all UG_ gene/wall identifiers remain stable. Keep one installed copy.
Original CollectionBucket, UrineBottle, Feces, jobs and recipes keep their identifiers
and class names. Saved bucket contents use the original urine/feces fields.
The standalone material-built FecalWall is removed. Saved copies/blueprints/frames
resolve to the existing gene-built UG_FecalWall. Unfinished construction then requires
an active wall-gene builder. Finished walls continue to support roofs.

GENE CATEGORY
Fecal walls, Permanent lactation and Squirter all appear under the
RJW Fluids and Solids category in the gene editor.

FECAL WALLS
Designate "fecal wall" under Architect > Structure using the normal wall drawing
tools. Only pawns with the active Fecal walls gene can construct the blueprints and
frames, including forced orders. Construction work must be enabled as usual.

Walls cost no materials and take 650 work. Rough walls have 20 hit points;
smoothed walls have 50. They block sight, movement and wind,
support roofs, are airtight and do not burn. Rough walls use RimWorld's natural
rock atlas with a brown tint; smooth walls use its polished rock atlas.

After construction, use Architect > Orders > Smooth Surface and drag over the rough
walls. Normal Construction workers can polish them using the game's normal smoothing
job and work requirement. Smoothing improves beauty from -2 to +1 and cleanliness
from -0.01 to 0, and raises maximum hit points from 20 to 50. The polished version is created by smoothing,
rather than appearing as a second free build command.

Existing smoothed walls gain the new maximum after restarting and can be repaired
normally; updating does not instantly heal damaged walls.

Neither version yields materials when demolished or destroyed. Finished walls stay
if the builder loses the gene. Roof support follows ordinary wall rules. Existing
fecal walls keep their definition ID and become the new rough, smoothable version.

PERMANENT LACTATION
Works from biological age 16 on humanlike pawns of any sex or faction with the
active gene and RJW breasts above the nipples-only stage. The gene does not grow
or resize breasts. Existing implants classified by RJW as breasts are included.
Featureless chests, missing parts and nipples-only parts contribute no output.

Production and capacity use the sum of qualifying RJW breast severities / 0.5.
At the default settings, a standard human with severity 0.5 produces 2 nutrition
(about 40 breast-milk items) per fed game day. Severity 1.0 produces twice that.
Additional breast pairs add their size. Body size, other milk genes, normal hunger
effects and Lactation Expansion's speed/capacity settings also apply once.

The default food cost is HALF the nutrition actually added to milk storage.
2 nutrition of milk costs 1 nutrition of food, in addition to the pawn's normal
food needs and the gene's ordinary metabolic effect. Full storage costs nothing.
Production cannot spend food the pawn does not have. Any malnutrition pauses it;
recovery restarts it automatically. Severe malnutrition can remove the underlying
normal lactation condition, after which the gene recreates it on recovery.

Uses Lactation Expansion's existing breast-milk item, reservoir, milking toggles,
self-milking, patient/prisoner collection jobs and breastfeeding. Enable the existing
milking toggle to collect routinely. The new gene bypasses the dependency's male
lactation restriction for its own eligible carriers only.

Look under Options > Mod settings > RJW Fluids and Solids to adjust production (0.1x–20x)
and food cost (0%–200%). Defaults are 4x and 50%. Capacity scales with production,
with a minimum of one collectible milk item for very small carriers.

Removing or suppressing the gene restores normal output, food cost and lactation
decay. It preserves the shared lactation condition and stored milk, so ordinary
post-pregnancy/drug lactation is not deleted. If Wet nurse or another mod sustains
that condition, its ordinary behavior continues. There is only one milk reservoir;
adding Wet nurse does not create another reservoir or another production loop.

SQUIRTER
Active from biological age 18 and only during the adult life stage. Works for
humanlike carriers of any sex. Partnered RJW events require adult humanlike
participants. The gene does not start activities or change their frequency.

Produces the existing S16GirlCum item (displayed as "love juice") near the carrier.
Each completed RJW orgasm produces three items: three times a baseline one-item
yield. This includes partnered activity, masturbation and add-ons using RJW's
shared SatisfyPersonal event. The multiplier is fixed at 3; it does not change RJW's
underlying fluid amounts, other fluid items, or the milk gene's settings.

For the six installed S16 vibrator definitions and the nipple-wrist-cuff cycle,
the gene watches entry into the actual completion stage. A one-item toy produces
three; a two-item toy produces six. For active carriers, this replaces those toys'
old periodic love-juice drops, avoiding both timed and completion drops together.
Noncarriers retain the original toy behavior. Toy stages and normal counts are
read from the installed definitions without relying on translated stage labels.

Repeated updates at the same toy stage do not produce extra items. A new cycle
can produce again. RJW event ordinals prevent duplicate or older callbacks from
paying again, including after save/load. The gene also supports RJW's standalone
Hediff_Orgasm signal and avoids counting it alongside the shared event.

If the pawn is off-map or placement is blocked, output stays in a saved queue and
is retried when the eligible pawn is spawned on a map. Placement respects the
item's normal stack limit and debits only items actually placed. Suppression or
removal stops new gene output. There is no separate food charge for this gene
beyond its ordinary metabolic cost.

An add-on must emit one of the supported completion signals, or use the supported
S16 toy stages, to trigger output. An animation or silent refill of a need alone
is not treated as a new completion. Other third-party toy systems may require
an additional event adapter.

GENE STATS
Fecal walls: complexity 1, metabolism 0, archites 0.
Permanent lactation: complexity 1, metabolism -1, archites 0.
Squirter: complexity 1, metabolism -1, archites 0.

COLLECTED WASTE
Build the collection bucket from the DBH hygiene category. It collects one urine
bottle and one feces per successful use, up to eight of each. Empty it manually or
through the existing hauling workgiver. Normal toilets and DBH plumbing remain
unchanged. Interrupted emptying does not release contents; partial item placement
subtracts only output actually placed.

Urine can be drunk, poured on fires, or used as DBH fertilizer on diggable ground.
Each use consumes one bottle. Feces can be consumed, composted (8 -> 4 biosolids),
or refined (20 -> 10 chemfuel). Consuming feces cannot add food poisoning through
normal ingestion. Other foods and existing illnesses retain their normal behavior.
Waste resources still rot, deteriorate and impose their original storage penalties.

THROWING FECES
Select a pawn, right-click a feces stack, and choose "Throw feces at..."; then choose
a pawn target. The carrier retrieves one unit and moves within 12 tiles and clear
line of sight. Throwing takes a one-second windup and consumes exactly one unit.
By default, the projectile deals 1 blunt damage. A hit on a living, upright flesh pawn with a
food need has a 30% vomiting chance. Shields prevent the vomiting effect. Mechs,
buildings and misses cannot vomit. A ten-second nausea cooldown and the active
vomiting job prevent repeatedly interrupting the same target. Failed throws drop
unused carried ammunition. This is a manual one-shot order, not an equipped gun.
Options > Mod settings > RJW Fluids and Solids also controls throwing range
(1-30 tiles), vomiting chance (0-100%) and nausea cooldown (1-120 seconds).
Defaults are 12 tiles, 30% and 10 seconds. Active vomiting is never restarted.

SEALED WASTE CONTAINERS
Build a sealed waste container under Architect > Hygiene. It costs 25 wood, metal
or stone material, takes 650 work, and holds three stacks on one tile. Its fixed
filter accepts only UrineBottle and Feces, with Preferred storage priority by default.
Use the standard storage tab and hauling jobs to manage it.

Waste stored there contributes no beauty or cleanliness penalty. Removing it or
destroying the container restores its ordinary penalties. The container itself has
zero base beauty/cleanliness, though normal material modifiers may apply. It uses
ordinary shelf storage, so there is no separate hidden inventory or save format.
Like a shelf it prevents deterioration; contents still rot without refrigeration.
It does not clean nearby filth or satisfy a pawn's hygiene needs.

IDEOLOGY AND COFFEE SUPPORT
Urine consumption and feces consumption each have three precepts: disapproved,
acceptable or approved. Disapproved gives -6/-12 mood respectively; approved gives
+4/+6; acceptable removes the original waste-specific taste penalty. Other food
thoughts still apply. No precept leaves the original behavior. There is no forced
daily waste consumption requirement.

Milk uses Coffee's existing Lactating issue and precepts, with no separate milk
belief issue. Without Equal Milking, compatibility reads the installed Coffee
lactation module's original XML and adapts its source classes for our milk system.
With Coffee's native lactation module already enabled, it updates the existing
milk configuration instead of loading a second set of definitions.

SEX_BreastMilk, including milk used as a cooking ingredient, satisfies Coffee's
milk-consumption beliefs and updates its saved last-consumption time. Previewing
food cannot update that time. Original Coffee raw-milk/meal thoughts are retained.
Coffee milk speed/yield modifiers apply to Lactation Expansion production once;
yield also scales capacity. The Hucow 1.5x speed and 2.5x yield therefore multiply
production by 3.75 and capacity by 2.5, before our gene's size/production settings.
The same milk reservoir and collection jobs are used, with no extra output loop.
Coffee induction/conversion interactions in the adapted module require adults.

COMPATIBILITY AND SOURCE
This assembly targets the installed .NET 10 / RimWorld 1.7 compatibility compatibility
runtime. It is not a RimWorld 1.6 Mono build. Dependency DLLs are not bundled.
Coffee's installed files remain unchanged and are required at runtime. Adapted
compatibility source is credited in THIRD_PARTY_NOTICES.txt.

Source/ contains full C# source and the build project. Rebuild with:
dotnet build Source/UsefulGenes.csproj -c Release
GameRoot and Workshop can be overridden as MSBuild properties for another location.
Source/generate_icons.py regenerates the original sprites with Pillow.
Tests/ contains the regression fixtures. Run python3 Tests/run_tests.py on this Mac
with the installed dependencies and .NET SDK.
See VALIDATION.txt for automated coverage and the remaining live-colony checks.

PROGRESSION MODPACK SUPPORT (1.3.0)
Optional integrations activate only when their actual definitions exist:
- Progression: Core / VFE Tribals: collection buckets follow the same
  VFET_Construction requirement as Progression's pit latrine.
- Progression: Storage: sealed containers require Ferny_Barrels, matching barrels.
  The free gene-built walls keep their no-research behavior.
- Medieval Overhaul / Progression: Kitchen: breast milk joins DankPyon_Milk, so milk
  category filters used by cheese presses and processing recipes can accept it.
- Milk Based Drugs: ROBTRG_MilkTypes membership is ensured once, preserving its
  existing support without removing other milk categories.
- RJW Unleashed Gastronomy: breast milk joins its human-milk food category.
- Milk Pipe Network: adds breast-milk pipes, underground pipes, tanks, a drain and
  a tap under the installed milk-network menu. This is a SEPARATE network that
  accepts and dispenses SEX_BreastMilk. Animal-milk pipes/taps remain unchanged.
  Existing pipe-system costs, research, power, capacity and spoilage rules apply.
  Collect milk normally and haul it to the breast-milk drain; the gene does not
  automatically transfer a pawn's internal milk into a pipe.
- Adaptive Storage Framework: seal protection recognizes both spawned slot groups
  and container-held items; a definition marker survives building-class changes.
  The supplied sealed container still uses normal storage and hauling.

No optional mod DLLs are required by the main assembly. Dependency files remain
unchanged. Research, nutrition, milk output multipliers and gene extraction systems
are not globally overwritten. Standard gene definitions continue to work with
Progression: Genetics and ordinary gene selection/extraction interfaces.

See COMPATIBILITY.txt for audit scope and limits. This does not claim blanket
compatibility with every mod in the Progression collection.
