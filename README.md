# RJW Fluids and Solids

**Version 1.3.1 · RimWorld 1.7 · Package ID `umrae.usefulgenes`**

A combined Biotech gene and resource mod for an RJW/Dubs Bad Hygiene setup. It includes gene-built walls, configurable milk production, an adult-only event-driven resource gene, collected waste, sealed storage, manual waste throwing, ideology precepts, and targeted milk-processing integrations.

## Main features

| Feature | Behavior |
| --- | --- |
| Fecal walls gene | Gene carriers construct material-free, fragile walls. Rough walls have **20 HP** and polished walls have **50 HP**. Both support roofs, block movement/sight/wind, are airtight and nonflammable, and yield no recovered material. Normal smoothing improves appearance and durability. |
| Permanent lactation gene | Uses Lactation Expansion's existing reservoir and collection jobs. Eligible humanlike carriers start at biological age **16**; output depends on qualifying RJW anatomy. Production stops during malnutrition and charges only for actual milk stored. This is production logic, not an interaction trigger. |
| Squirter gene | Adult-only event-driven resource output using supported RJW completion signals and installed S16 stages. Requires biological age **18+** and an adult developmental stage; partnered signals require adult humanlike participants. Does not initiate activities or change their frequency. |
| Collection bucket | Collects one urine bottle and one feces per successful DBH use, up to eight of each. Empty manually or through hauling work. |
| Waste use and processing | Fire suppression, DBH fertilizer, composting, and refining recipes, with the supplied consumption beliefs. |
| Manual waste throwing | A right-click, one-shot command using one item; configurable range, vomiting chance, and cooldown. It is not an equipped weapon. |
| Sealed waste container | Three stacks on one tile. Stored waste contributes no beauty/cleanliness penalty; ordinary rot still applies without refrigeration. |
| Milk compatibility | Reuses Coffee's existing beliefs and recognizes the supported milk item in processing categories. An optional separate pipe network preserves the milk item's identity. |

Normal gene availability and extraction remain in use. Existing pawns do not automatically receive the new genes. All three genes have complexity 1 and no archite requirement; wall metabolism is 0, and each other gene's metabolism is −1 before other mods adjust it.

All three custom genes—**Fecal walls**, **Permanent lactation**, and **Squirter**—appear under **RJW Fluids and Solids** in the gene editor.

## Installation

1. Install the required DLC and dependency mods listed below.
2. Download this repository using **Code → Download ZIP**, then extract it. The compiled mod is included; building is optional.
3. Put the extracted folder in `RimWorld/Mods/`. You may rename it to `RJWFluidsAndSolids`. `About/About.xml` must sit directly inside that mod folder, rather than an extra nested folder.
4. Keep only one enabled copy of this mod. Enable it in RimWorld's Mods menu after its dependencies and the supported mods you use.
5. Restart RimWorld after installation or updating.

For Steam, **Manage → Browse local files** opens the game installation. On macOS the usual Mods location is `~/Library/Application Support/Steam/steamapps/common/RimWorld/Mods/`.

## Required dependencies

| Dependency | Package ID |
| --- | --- |
| Biotech DLC | `ludeon.rimworld.biotech` |
| Ideology DLC | `ludeon.rimworld.ideology` |
| Harmony | `brrainz.harmony` |
| RimJobWorld | `rim.job.world` |
| Lactation Expansion | `euterpe.milkytitfuck` |
| S16's Extension | `euclidean.s16.core` |
| Dubs Bad Hygiene | `dubwise.dubsbadhygiene` |
| C0ffeeRIA / Coffee's RJW Ideology Add-ons | `c0ffee.rjw.ideologyaddons` |

**Disable standalone DBH Collected Waste** (`umrae.dbh.collectedwaste`): its content is included. Remove the old UsefulGenes installation when updating; the stable package ID is intentionally retained. Load after the required dependencies and after RJW Genes and RJW Stack Compatibility when present.

## Optional integrations

- **Progression Core / Storage:** uses matching sanitation and barrel research when those definitions exist.
- **Progression Kitchen / Medieval Overhaul:** adds the supported milk item to the milk category used by compatible processors.
- **Milk Based Drugs** and **RJW Unleashed Gastronomy:** adds matching milk-category membership without duplicating entries.
- **Milk Pipe Network / PipeSystem:** creates separate milk pipes, underground pipes, tanks, a drain, and a tap from installed templates. Haul collected milk to the drain; this does not transfer directly from a pawn's internal reservoir. Ordinary network rules still apply.
- **Adaptive Storage Framework:** sealed-waste protection uses definition markers and holder membership to survive supported storage-class changes.
- **RJW Genes / Wet nurse:** reuses the shared Lactation Expansion condition and reservoir instead of adding a second production loop.
- **Equal Milking:** recognizes an already-loaded Coffee module, but that branch has not been verified in a full live-game session.

See [COMPATIBILITY.md](COMPATIBILITY.md) for limits. Load-order hints alone do not certify every feature in a mod collection.

## Settings and use

Open **Options → Mod settings → RJW Fluids and Solids**.

| Setting | Range | Default |
| --- | --- | --- |
| Milk-production multiplier | 0.1–20× | 4× |
| Food cost as a fraction of nutrition actually stored | 0–200% | 50% |
| Throw range | 1–30 tiles | 12 |
| Vomiting chance | 0–100% | 30% |
| Cooldown | 1–120 seconds | 10 |

At the default production setting, an eligible average-size human produces approximately 2 nutrition of milk per fed game day and pays 1 nutrition in additional food, subject to storage, anatomy, and other modifiers. Full storage costs no food. Removing or suppressing the production gene restores ordinary lactation behavior and preserves stored milk.

Build gene walls under **Architect → Structure**; polish them through **Orders → Smooth Surface**. Buckets and sealed containers appear in the hygiene category. For a manual throw, select a pawn, right-click a waste stack, and choose **Throw feces at...**. See [the detailed gameplay guide](README.txt) for accounting, recipes, and exact behavior.

## Updating and saves

Keep one installed copy and retain a backup save. The package ID `umrae.usefulgenes`, assembly identity, `UG_` genes, and original collected-waste identifiers are retained. Old standalone material-wall references migrate to the gene wall; unfinished construction then requires an active wall-gene builder. Completed walls remain roof supports.

The mod includes items, jobs, genes, structures, and saved queues. Removing it from an established colony is not guaranteed safe. Do not delete dependencies or swap between duplicate installations as a migration method.

Existing smoothed walls gain the new 50 HP maximum after restarting; damaged walls can be repaired normally. No existing gene or wall identifiers changed.

## Verification status

The 1.3.1 integration run recorded **11,081 automated checks** involving real installed APIs, XML parsers, Harmony targets, and Scribe serialization, with controlled headless fixtures. Native colony pathfinding, animations, visual menus, hauling, smoothing, complete pipe transport, and a full mod-stack save/reload still need live-game checks. See `VALIDATION.txt`.

## Source, testing, and support

- [Build instructions](BUILDING.md)
- [Compatibility details](COMPATIBILITY.md)
- [Changelog](CHANGELOG.md)
- [Contribution guidelines](CONTRIBUTING.md)
- [Report a bug](https://github.com/Neyokah/rjw-fluids-and-solids/issues/new?template=bug_report.yml)

These builds target **RimWorld 1.7's .NET 10 runtime**. They are not RimWorld 1.6 binaries. Older dependency assemblies used by the installed 1.7 compatibility runtime do not establish that the mod itself supports 1.6. The original automated test records are included where available; full graphical gameplay with a large mod collection is not claimed to have been verified.

## Credits and license

Original code in this project is available under the [MIT license](LICENSE). Third-party dependencies retain their own authorship and terms and are not bundled. See [third-party notices](THIRD_PARTY_NOTICES.md) for any exceptions or adapted material.
