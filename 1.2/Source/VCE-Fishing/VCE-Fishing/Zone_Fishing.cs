﻿
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using Verse;
using RimWorld;


namespace VCE_Fishing
{
    public class Zone_Fishing : Zone
    {
        public FishSizeCategory fishSizeToCatch = FishSizeCategory.Medium;

        public bool allowFishing = true;

        public bool isOcean = false;

        public bool isZoneBigEnough = false;

        public bool isZoneEmpty = true;

        public bool someoneFishing = false;


        private static List<Color> fishingZoneColors = new List<Color>();

        private static int nextFishingZoneColorIndex = 0;

        public List<ThingDef> fishInThisZone = new List<ThingDef>();



        public override bool IsMultiselectable
        {
            get
            {
                return false;
            }
        }

        protected override Color NextZoneColor
        {
            get
            {
               
                return NextFishingZoneColor();


            }
        }

        private static IEnumerable<Color> FishingZoneColors()
        {
            yield return Color.Lerp(new Color(0f, 0f, 1f), Color.gray, 0.5f);
            yield return Color.Lerp(new Color(0f, 0.5f, 1f), Color.gray, 0.5f);
            yield return Color.Lerp(new Color(0f, 0.25f, 1f), Color.gray, 0.5f);
            yield return Color.Lerp(new Color(0.25f, 0.25f, 0.75f), Color.gray, 0.5f);
            yield return Color.Lerp(new Color(0.25f, 0f, 0.65f), Color.gray, 0.5f);
            yield break;
        }

        public static Color NextFishingZoneColor()
        {
            fishingZoneColors.Clear();
            foreach (Color color in FishingZoneColors())
            {
                Color item = new Color(color.r, color.g, color.b, 0.09f);
                fishingZoneColors.Add(item);

            }
            Color result = fishingZoneColors[nextFishingZoneColorIndex];
            nextFishingZoneColorIndex++;
            if (nextFishingZoneColorIndex >= fishingZoneColors.Count)
            {
                nextFishingZoneColorIndex = 0;
            }
            return result;
        }

        

        public void initialSetZoneFishList()
        {
            if (this.cells.Count < Options.VCE_Fishing_Settings.VCEF_minimumZoneSize)
            {
                isZoneBigEnough = false;
            }
            else isZoneBigEnough = true;
            fishInThisZone.Clear();
            foreach (FishDef element in DefDatabase<FishDef>.AllDefs.Where(element => element.fishSizeCategory == this.fishSizeToCatch))
            {
                foreach (string biomeTemp in element.allowedBiomes)
                {

                    foreach (BiomeTempDef biometempdef in DefDatabase<BiomeTempDef>.AllDefs.Where(biometempdef => biometempdef.biomeTempLabel == biomeTemp))
                    {
                        foreach (string biome in biometempdef.biomes)
                        {


                            if (this.Map.Biome.defName == biome)
                            {
                                if (this.isOcean && element.canBeSaltwater)
                                {
                                    this.fishInThisZone.Add(element.thingDef);
                                }
                                if (!this.isOcean && element.canBeFreshwater)
                                {
                                    this.fishInThisZone.Add(element.thingDef);

                                }
                            }
                        }
                    }
                }
            }
            if (fishInThisZone.Count > 0)
            {
                isZoneEmpty = false;
            } else isZoneEmpty = true;
        }

        public Zone_Fishing()
        {
           
        }

        public Zone_Fishing(ZoneManager zoneManager) : base("VCEF_FishingGrowingZone".Translate(), zoneManager)
        {
            initialSetZoneFishList();
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look<ThingDef>(ref this.fishInThisZone, "fishInThisZone", LookMode.Undefined, new object[0]);
            Scribe_Values.Look<FishSizeCategory>(ref this.fishSizeToCatch, "fishSizeToCatch", FishSizeCategory.Medium, false);
            Scribe_Values.Look<bool>(ref this.allowFishing, "allowFishing", true, false);
            Scribe_Values.Look<bool>(ref this.isOcean, "isOcean", false, false);
            Scribe_Values.Look<bool>(ref this.isZoneBigEnough, "isZoneBigEnough", true, false);
            Scribe_Values.Look<bool>(ref this.isZoneEmpty, "isZoneEmpty", true, false);



        }


        public override string GetInspectString()
        {
            string text = string.Empty;
            if (isZoneBigEnough) {
                if (!isZoneEmpty) {
                    text += "VCEF_ZoneSetTo".Translate() + ": " + GetFishToCatch().ToString();
                    if (this.fishInThisZone != null)
                    {
                        text += "\n" + "VCEF_FishesInThisZone".Translate();
                        IList<string> fishInThisZoneString = new List<string>();
                        foreach (ThingDef fish in this.fishInThisZone)
                        {
                            fishInThisZoneString.Add(fish.label);
                        }
                        string[] array = fishInThisZoneString.ToArray();
                        string joined = string.Join(", ", array);
                        text += joined;
                    }
                    text += "\n" + "VCEF_IsZoneOceanZone".Translate();
                    if (this.isOcean)
                    {
                        text += "VCEF_Yes".Translate();
                    }
                    else text += "VCEF_No".Translate();


                }
                else
                {
                    text += "VCEF_NoFish".Translate();
                }


            } else
            {
                text += "VCEF_ZoneTooSmall".Translate(Options.VCE_Fishing_Settings.VCEF_minimumZoneSize);
            }
            

            return text;
        }

       

        [DebuggerHidden]
        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo g in base.GetGizmos())
            {
                yield return g;
            }
            
                yield return FishToCatchSettableUtility.SetFishToCatchCommand(this, this.Map);
                yield return new Command_Toggle
                {
                    defaultLabel = "VCEF_CommandAllowFishing".Translate(),
                    defaultDesc = "VCEF_CommandAllowFishingDesc".Translate(),
                    hotKey = KeyBindingDefOf.Command_ItemForbid,
                    icon = ContentFinder<Texture2D>.Get("UI/Designators/VCEF_AllowFish", true),
                    isActive = (() => this.allowFishing),
                    toggleAction = delegate
                    {
                        this.allowFishing = !this.allowFishing;
                    }
                };
            

        }

        [DebuggerHidden]
        public override IEnumerable<Gizmo> GetZoneAddGizmos()
        {
            yield return DesignatorUtility.FindAllowedDesignator<Designator_ZoneAdd_Fishing_Expand>();
        }

        public FishSizeCategory GetFishToCatch()
        {
           
                return this.fishSizeToCatch;
           

        }

        public void SetFishToCatch(FishSizeCategory fishSizeDef)
        {
            this.fishSizeToCatch = fishSizeDef;
        }

      
    }
}

