# Compatibility

## Supported target

RimWorld **1.7**, targeting its .NET 10 runtime. The source/binaries were prepared against a local macOS compatibility-runtime installation. Other operating systems, future game builds, and every possible mod combination are not separately certified. Obtain the required dependencies independently; they are not bundled.

## Integration scope

Required dependencies are declared in About/About.xml and summarized in README.md. Optional Progression, milk processing, pipe, and storage integrations are activated by the actual installed definitions. Milk Pipe Network/PipeSystem must supply all expected templates for the separate network to appear. Processor mods that discard ingredient provenance may prevent Coffee from recognizing the origin of milk in finished products. Equal Milking's native-module branch has no full live-game verification. Standalone DBH Collected Waste is incompatible because the content is merged here. No support claim extends to every feature of the entire Progression collection.

## Load-order hints from the shipped metadata

The following are placement hints, **not a list of required dependencies or a blanket compatibility guarantee**. Required dependencies are separately listed in README.md and About/About.xml.

- `ludeon.rimworld.biotech`
- `brrainz.harmony`
- `rim.job.world`
- `euterpe.milkytitfuck`
- `vegapnk.rjw.genes`
- `umrae.rjwstackcompatibility`
- `euclidean.s16.core`
- `ludeon.rimworld.ideology`
- `dubwise.dubsbadhygiene`
- `c0ffee.rjw.ideologyaddons`
- `ferny.progressioncore`
- `ferny.progressionstorage`
- `ferny.progressionkitchen`
- `ferny.progressiongenetics`
- `ferny.progressionproduction`
- `ferny.progressionagriculture`
- `ferny.progressiontemperature`
- `adaptive.storage.framework`
- `vemp.pipenetwork`
- `dankpyon.medieval.overhaul`
- `robtrg.milkdrugs`
- `rjw.unleashed.gastronomy`

## Reporting an integration problem

Report the exact game and dependency versions, whether the problem reproduces with only required dependencies, and a minimal reproduction. Preserve unrelated load-order rules from upstream mods. See the included verification records for what has actually been tested.
