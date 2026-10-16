using System;
using UnityEngine;
using Verse;

namespace Umrae.UsefulGenes.Coffee
{
    public class LactationSettings : ModSettings
    {

        public static float massageCooldown = 0.3f;
        public static int totalMassagesUntilLactation = 20;  // severity += 1 / this amount

        public static float hucowBreastSizeBonus = 0f; //size increase when changed to hucow
        public static float hucowBreastSizeMinimum = 0.5f; //smallest size breasts can end up.


        private static Vector2 scrollPosition;
        private static float height_modifier = 300f;

        public static void DoWindowContents(Rect inRect)
        {

            //30f for top page description and bottom close button
            Rect outRect = new Rect(0f, 30f, inRect.width, inRect.height - 30f);

            //-16 for slider, height_modifier - additional height for hidden options toggles
            Rect viewRect = new Rect(0f, 0f, inRect.width - 16f, inRect.height + height_modifier);

            Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect); // scroll

            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.maxOneColumn = true;
            listingStandard.ColumnWidth = viewRect.width - 10f; // / 2.05f;
            listingStandard.Begin(viewRect);
            listingStandard.Gap(5f);

            listingStandard.Label("CRIALactation.massageCooldown".Translate() + ": " + Math.Round(massageCooldown, 3), -1f, new TipSignal ("CRIALactation.massageCooldownDesc".Translate()));
            massageCooldown = (float)listingStandard.Slider(massageCooldown, 0.01f, 1f);
            listingStandard.Gap(5f);

            listingStandard.Label("CRIALactation.totalMassagesUntilLactation".Translate() + ": " + totalMassagesUntilLactation, -1f, new TipSignal("CRIALactation.totalMassagesUntilLactationDesc".Translate()));
            totalMassagesUntilLactation = (int)listingStandard.Slider(totalMassagesUntilLactation, 1, 100);
            listingStandard.Gap(10f);

            listingStandard.Label("CRIALactation.hucowBreastSizeBonus".Translate() + ": " + Math.Round(hucowBreastSizeBonus, 3), -1f, new TipSignal("CRIALactation.hucowBreastSizeBonusDesc".Translate()));
            hucowBreastSizeBonus = (float)listingStandard.Slider(hucowBreastSizeBonus, 0f, 1f);
            listingStandard.Gap(5f);

            listingStandard.Label("CRIALactation.hucowBreastSizeMinimum".Translate() + ": " + Math.Round(hucowBreastSizeMinimum, 3), -1f, new TipSignal ("CRIALactation.hucowBreastSizeMinimumDesc".Translate()));
            hucowBreastSizeMinimum = (float)listingStandard.Slider(hucowBreastSizeMinimum, 0.1f, 5f);
            listingStandard.Gap(5f);

            listingStandard.End();
            Widgets.EndScrollView();

        }

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.Look(ref massageCooldown, "massageCooldown", massageCooldown, true);
            Scribe_Values.Look(ref totalMassagesUntilLactation, "totalMassagesUntilLactation", totalMassagesUntilLactation, true);
            Scribe_Values.Look(ref hucowBreastSizeBonus, "hucowBreastSizeBonus", hucowBreastSizeBonus, true);
            Scribe_Values.Look(ref hucowBreastSizeMinimum, "hucowBreastSizeMinimum", hucowBreastSizeMinimum, true);

        }
    }
}


