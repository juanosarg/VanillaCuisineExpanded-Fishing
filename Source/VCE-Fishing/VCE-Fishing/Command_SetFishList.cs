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
       
       

        public Command_SetFishList()
        {

            defaultDesc = "VCEF_ChooseFishDesc".Translate();
            defaultLabel = "VCEF_ChooseFish".Translate();

            foreach (object obj in Find.Selector.SelectedObjects)
            {
                Zone_Fishing zone = obj as Zone_Fishing;
                if (zone != null)
                {
                    if (zone.GetFishToCatch() == FishSizeCategory.Small)
                    {
                        this.icon = ContentFinder<Texture2D>.Get("UI/Commands/VCEF_Command_ChooseSmallFish", true);

                    }
                    else if (zone.GetFishToCatch() == FishSizeCategory.Medium)
                    {
                        this.icon = ContentFinder<Texture2D>.Get("UI/Commands/VCEF_Command_ChooseMediumFish", true);

                    }
                    else if (zone.GetFishToCatch() == FishSizeCategory.Large)
                    {
                        this.icon = ContentFinder<Texture2D>.Get("UI/Commands/VCEF_Command_ChooseLargeFish", true);

                    }
                } else 
                this.icon = ContentFinder<Texture2D>.Get("UI/Commands/VCEF_Command_ChooseMediumFish", true);
            }


            

            

            




        }

        public override void ProcessInput(Event ev)
        {
            base.ProcessInput(ev);
            List<FloatMenuOption> list = new List<FloatMenuOption>();

            
           list.Add(new FloatMenuOption("VCEF_ChooseFishSmallLabel".Translate(), delegate
           {
            zone.SetFishToCatch(FishSizeCategory.Small);
           
                          
           }, MenuOptionPriority.Default, null, null, 29f, null, null));
            list.Add(new FloatMenuOption("VCEF_ChooseFishMediumLabel".Translate(), delegate
            {
                zone.SetFishToCatch(FishSizeCategory.Medium);
              

            }, MenuOptionPriority.Default, null, null, 29f, null, null));
            list.Add(new FloatMenuOption("VCEF_ChooseFishLargeLabel".Translate(), delegate
            {
                zone.SetFishToCatch(FishSizeCategory.Large);
              

            }, MenuOptionPriority.Default, null, null, 29f, null, null));

            Find.WindowStack.Add(new FloatMenu(list));
        }

       




    }


}


