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
                
                hotKey = KeyBindingDefOf.Misc1,
                map = map,
                zone = zone
               
            };
        }

       
    }
}

