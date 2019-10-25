
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
        private ThingDef fishToCatch = null;

        public bool allowSow = true;

        

        public override bool IsMultiselectable
        {
            get
            {
                return true;
            }
        }

        protected override Color NextZoneColor
        {
            get
            {
                //Color current = Color.Lerp(new Color(0.27f, 0.41f, 0.37f), Color.gray, 0.5f);
                //return new Color(current.r, current.g, current.b, 0.09f);
                return ZoneColorUtility.NextGrowingZoneColor();


            }
        }



        public Zone_Fishing()
        {
             fishToCatch = ThingDef.Named("VCEF_RawMackerel");


        }

        public Zone_Fishing(ZoneManager zoneManager) : base("GrowingZone".Translate(), zoneManager)
        {
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look<ThingDef>(ref this.fishToCatch, "fishToCatch");
            Scribe_Values.Look<bool>(ref this.allowSow, "allowSow", true, false);
        }

        public override string GetInspectString()
        {
            string text = string.Empty;
            if (!base.Cells.NullOrEmpty<IntVec3>())
            {
                IntVec3 c = base.Cells.First<IntVec3>();
                if (c.UsesOutdoorTemperature(base.Map))
                {
                    string text2 = text;
                    text = string.Concat(new string[]
                    {
                        text2,
                        "OutdoorGrowingPeriod".Translate(),
                        ": ",
                        Zone_Growing.GrowingQuadrumsDescription(base.Map.Tile),
                        "\n"
                    });
                }
                if (PlantUtility.GrowthSeasonNow(c, base.Map, true))
                {
                    text += "GrowSeasonHereNow".Translate();
                }
                else
                {
                    text += "CannotGrowBadSeasonTemperature".Translate();
                }
            }
            return text;
        }

        public static string GrowingQuadrumsDescription(int tile)
        {
            List<Twelfth> list = GenTemperature.TwelfthsInAverageTemperatureRange(tile, 10f, 42f);
            if (list.NullOrEmpty<Twelfth>())
            {
                return "NoGrowingPeriod".Translate();
            }
            if (list.Count == 12)
            {
                return "GrowYearRound".Translate();
            }
            return "PeriodDays".Translate(list.Count * 5 + "/" + 60) + " (" + QuadrumUtility.QuadrumsRangeLabel(list) + ")";
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
                    defaultLabel = "CommandAllowSow".Translate(),
                    defaultDesc = "CommandAllowSowDesc".Translate(),
                    hotKey = KeyBindingDefOf.Command_ItemForbid,
                    icon = TexCommand.ForbidOff,
                    isActive = (() => this.allowSow),
                    toggleAction = delegate
                    {
                        this.allowSow = !this.allowSow;
                    }
                };
            

        }

        [DebuggerHidden]
        public override IEnumerable<Gizmo> GetZoneAddGizmos()
        {
            yield return DesignatorUtility.FindAllowedDesignator<Designator_ZoneAdd_Fishing_Expand>();
        }

        public ThingDef GetFishToCatch()
        {
           
                return this.fishToCatch;
           

        }

        public void SetFishToCatch(ThingDef fishDef)
        {
            this.fishToCatch = fishDef;
        }

        public bool CanAcceptSowNow()
        {
            return true;
        }
    }
}

