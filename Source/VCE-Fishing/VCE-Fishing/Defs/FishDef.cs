using RimWorld;
using System;
using Verse;
using System.Collections.Generic;


namespace VCE_Fishing
{
    public class FishDef : Def
    {
        public ThingDef thingDef;       
        public List<string> allowedBiomes;
        public FishSizeCategory fishSizeCategory;

    }
}
