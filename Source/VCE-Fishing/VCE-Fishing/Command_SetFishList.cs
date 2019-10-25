using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse.AI;
using Verse;


namespace VCE_Fishing
{
    [StaticConstructorOnStartup]
    public class Command_SetFishList : Command
    {

        public Map map;
        public Zone_Fishing zone;
        public Thing drone;
        public ThingDef thingSelected = null;



        public Command_SetFishList()
        {
            defaultDesc = "VCEF_ChooseFishDesc".Translate();
            defaultLabel = "VCEF_ChooseFish".Translate();
            icon = ContentFinder<Texture2D>.Get("UI/Commands/VCEF_Command_ChooseFish", true);
           
        
            
            

        }

        public override void ProcessInput(Event ev)
        {
            base.ProcessInput(ev);
            List<FloatMenuOption> list = new List<FloatMenuOption>();

            foreach (FishDef element in DefDatabase<FishDef>.AllDefs)
            {
                foreach (string allowed in element.allowedBiomes)
                {
                    if (allowed == map.Biome.defName) {
                        list.Add(new FloatMenuOption(element.thingDef.LabelCap, delegate
                        {
                            zone.SetFishToCatch(element.thingDef);
                            thingSelected = element.thingDef;



                        }, MenuOptionPriority.Default, null, null, 29f, null, null));
                        
                    }
                    
                }

                    
                    
                

            }



            if (list.Count > 0)
            {

            }
            else
            {
                list.Add(new FloatMenuOption("VCEF_NoFish".Translate(), delegate
                {

                }, MenuOptionPriority.Default, null, null, 29f, null, null));
            }
            Find.WindowStack.Add(new FloatMenu(list));
        }

       




    }


}


