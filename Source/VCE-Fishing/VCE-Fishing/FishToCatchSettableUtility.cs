using RimWorld;
using Verse;

using UnityEngine;


namespace VCE_Fishing
{
    public static class FishToCatchSettableUtility
    {
        public static Command_SetFishList SetFishToCatchCommand(Zone_Fishing zone, Map map)
        {
            return new Command_SetFishList()
            {
                defaultDesc = "VCEF_ChooseFishDesc".Translate(),
                defaultLabel = "VCEF_ChooseFish".Translate(),
                icon = ContentFinder<Texture2D>.Get("UI/Commands/VCEF_Command_ChooseFish", true),
                hotKey = KeyBindingDefOf.Misc1,
                map = map,
                zone = zone
               
            };
        }

       
    }
}

