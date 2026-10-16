# License scope and third-party notices

The MIT license in `LICENSE` applies to this repository's original code and original assets only. It does **not** grant rights to third-party material or to the underlying game and dependency mods.

**Adapted Coffee material is excluded from that MIT grant:** `Source/Coffee/` and `1.7/Languages/English/Keyed/Coffee_CRIALaction.xml` are adapted from C0ffee's installed CRIALactation module. Original authorship and applicable upstream terms remain in force. No independent license grant for that original material was found in the local compatibility package. Retain this notice and verify upstream terms before redistributing those parts separately.

Source/CollectedWaste and associated waste definitions were adapted from the owner's earlier DBH Collected Waste mod. Its identifiers are preserved for saves. Runtime pipe templates are read from the user's installed network mod; its binaries/artwork are not bundled.

The dependency authors retain their own rights. No claim of ownership is made over definitions, names, or assets loaded from the user's installed game and mods. The original detailed notices follow.

---

RJW FLUIDS AND SOLIDS — LOCAL COMPATIBILITY BUILD

Collected Waste
Source/CollectedWaste and the associated waste definitions are adapted from the
user's installed DBH Collected Waste, authored by Umrae. Original definition and
class identifiers are retained for saves. The redundant material-built wall is
replaced by a save-name migration to the gene-built wall.

Coffee's RJW Ideology Add-ons / C0ffeeRIA
Source/Coffee and Coffee_CRIALaction.xml are adapted from the source and English
text in the user's installed c0ffee.rjw.ideologyaddons/CRIALactation module.
Original work is credited to C0ffee. Changes include a separate compatibility
namespace, Lactation Expansion integration, shared-condition reuse, adult
interaction eligibility, and the current runtime tick hook. Milk belief, role,
ability and thought definitions are read from the installed Coffee mod at runtime;
Coffee assemblies and artwork are not redistributed. No independent license grant
for that original work is asserted by this local compatibility package.

RimWorld / Ludeon Studios, Harmony, RJW, S16's Extension, Lactation Expansion and
Dubs Bad Hygiene remain required external dependencies. Their DLLs are referenced
for compilation and are not included. No dependency files are edited.

Milk Pipe Network / Vanilla Expanded Framework PipeSystem
Progression support clones definitions from the installed Milk Pipe Network XML
when present. Its native PipeSystem classes, graphics, research, costs and runtime
behaviors are reused; their original DLLs and assets are not bundled. The copied
runtime templates form a separate network whose endpoints preserve SEX_BreastMilk.
No license or ownership claim is made over those original templates or assets.
