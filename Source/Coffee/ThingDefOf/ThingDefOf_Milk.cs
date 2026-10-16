using System.Collections.Generic;
using Verse;

namespace Umrae.UsefulGenes.Coffee
{
    public static class ThingDefOf_Milk
    {
        private static List<ThingDef> cachedMilk;

        public static List<ThingDef> MilkConfig
        {
            get
            {
                if (cachedMilk == null)
                {
                    // Hardcoded the defName here, but honestly, that's all it needs
                    MilkConfigDef config = DefDatabase<MilkConfigDef>.GetNamedSilentFail("GlobalMilkSettings");
                  
                    if (config != null)
                    {
                        cachedMilk = config.MilkThingDef;
                    }
                }
                return cachedMilk;
            }
        }

    }
}
