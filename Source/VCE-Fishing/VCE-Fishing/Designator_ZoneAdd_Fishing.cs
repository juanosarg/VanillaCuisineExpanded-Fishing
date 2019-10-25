using System;
using UnityEngine;
using Verse;
using RimWorld;

namespace VCE_Fishing
{
    public class Designator_ZoneAdd_Fishing : Designator_ZoneAdd
    {
        protected override string NewZoneLabel
        {
            get
            {
                return "VCEF_FishingGrowingZone".Translate();
            }
        }

        public Designator_ZoneAdd_Fishing()
        {

            this.zoneTypeToPlace = typeof(Zone_Fishing);
            this.defaultLabel = "VCEF_FishingGrowingZone".Translate();
            this.defaultDesc = "VCEF_FishingGrowingZoneDesc".Translate();
            this.icon = ContentFinder<Texture2D>.Get("UI/Designators/VCEF_ZoneCreate_Fishing", true);
            this.hotKey = KeyBindingDefOf.Misc2;

            //  this.tutorTag = "ZoneAdd_Growing";
        }

        public override AcceptanceReport CanDesignateCell(IntVec3 c)
        {
            if (!base.CanDesignateCell(c).Accepted)
            {
                return false;
            }
            if (base.Map.fertilityGrid.FertilityAt(c) < 0.1)
            {
                return false;
            }
            return true;
        }

        protected override Zone MakeNewZone()
        {
            // PlayerKnowledgeDatabase.KnowledgeDemonstrated(ConceptDefOf.GrowingFood, KnowledgeAmount.Total);
            return new Zone_Fishing(Find.CurrentMap.zoneManager);
        }
    }
}

