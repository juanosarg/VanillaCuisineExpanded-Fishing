using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;
using RimWorld;
using System.Linq;
using UnityEngine;



namespace VCE_Fishing
{
    class JobDriver_Fish : JobDriver
    {
        public float pawnFishingSkill = 10;
        public FishSizeCategory sizeAtBeginning = FishSizeCategory.Medium;
        public ThingDef fishCaught = DefDatabase<ThingDef>.GetNamed("VCEF_RawMackerel");
        public int fishAmount = 1;
        public int fishAmountWithSkill;
        public int minFishingSkillForMinYield = 6;
        public float skillGainperTick = 0.025f;
        public float fishingSkill = 1;
        Zone_Fishing fishingZone;
        public bool caughtSomethingSpecial = false;

        private System.Random rand = new System.Random();

        public const int smallFishDurationFactor = 4500;
        public const int mediumFishDurationFactor = 9000;
        public const int largeFishDurationFactor = 13500;

        public const float minMapTempForLowFish = 0f;
        public const float maxMapTempForLowFish = 50.0f;


        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<float>(ref this.pawnFishingSkill, "pawnFishingSkill", 0f, true);
            Scribe_Values.Look<int>(ref this.fishAmount, "fishAmount", 1, true);
            Scribe_Values.Look<int>(ref this.fishAmountWithSkill, "fishAmountWithSkill", 1, true);
            Scribe_Values.Look<FishSizeCategory>(ref this.sizeAtBeginning, "sizeAtBeginning", FishSizeCategory.Medium, true);
            Scribe_Defs.Look<ThingDef>(ref this.fishCaught, "fishCaught");
            Scribe_References.Look<Zone_Fishing>(ref this.fishingZone, "fishingZone");
            Scribe_Values.Look<bool>(ref this.caughtSomethingSpecial, "caughtSomethingSpecial",false,true);



        }

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {
           
            fishingZone = this.Map.zoneManager.ZoneAt(this.job.targetA.Cell) as Zone_Fishing;
            sizeAtBeginning = fishingZone.GetFishToCatch();
            caughtSomethingSpecial = false;
            fishCaught = SelectFishToCatch(fishingZone);
            foreach (FishDef element in DefDatabase<FishDef>.AllDefs.Where(element => element.thingDef == fishCaught))
            {
                fishAmount = element.baseFishingYield;
            }

            fishAmountWithSkill = CalculateFishAmountWithSkillAndConditions(fishAmount);

            Pawn pawn = this.pawn;
            LocalTargetInfo target = this.job.targetA;
            Job job = this.job;
            bool result;
            if (pawn.Reserve(target, job, 1, -1, null, errorOnFailed))
            {
                pawn = this.pawn;
                target = this.job.targetA.Cell;
                job = this.job;
                int index = 0;
                LocalTargetInfo moreTargetCells;
                while (index < fishingZone.cells.Count)
                {
                    moreTargetCells = fishingZone.cells[index];
                    pawn.Reserve(moreTargetCells, job, 1, -1, null, errorOnFailed);
                    index++;
                }


                result = pawn.Reserve(target, job, 1, -1, null, errorOnFailed);
            }
            else
            {
                
                result = false;
            }
            return result;

           
           
        }

        protected override IEnumerable<Toil> MakeNewToils()
        {

           
          

           
            

            if (fishCaught == null)
             {
                 this.EndJobWith(JobCondition.Incompletable);
               
            }

            this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
                this.FailOnBurningImmobile(TargetIndex.A);
                yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.Touch);
                this.pawn.rotationTracker.FaceTarget(base.TargetA);
                Toil fishToil = new Toil();
                fishToil.tickAction = delegate ()
                {
                    this.pawn.skills.Learn(SkillDefOf.Animals, skillGainperTick);
                    if (fishingZone != null)
                    {
                        if (!fishingZone.isZoneBigEnough)
                        {
                            this.EndJobWith(JobCondition.Incompletable);
                           

                        }
                    }
                };

                Rot4 pawnRotation = pawn.Rotation;
                IntVec3 facingCell = pawnRotation.FacingCell;
                if (facingCell == new IntVec3(0, 0, 1))
                {
                    //Log.Message("Looking north");
                    fishToil.WithEffect(() => DefDatabase<EffecterDef>.GetNamed("VCEF_FishingEffectNorth"), () => this.TargetA.Cell + new IntVec3(0, 0, 1));


                }
                else if (facingCell == new IntVec3(1, 0, 0))
                {
                    // Log.Message("Looking east");

                    fishToil.WithEffect(() => DefDatabase<EffecterDef>.GetNamed("VCEF_FishingEffectEast"), () => this.TargetA.Cell + new IntVec3(1, 0, 0));

                }
                else if (facingCell == new IntVec3(0, 0, -1))
                {
                    // Log.Message("Looking south");
                    fishToil.WithEffect(() => DefDatabase<EffecterDef>.GetNamed("VCEF_FishingEffectSouth"), () => this.TargetA.Cell + new IntVec3(0, 0, -1));


                }
                else if (facingCell == new IntVec3(-1, 0, 0))
                {
                    //  Log.Message("Looking west");
                    fishToil.WithEffect(() => DefDatabase<EffecterDef>.GetNamed("VCEF_FishingEffectWest"), () => this.TargetA.Cell + new IntVec3(-1, 0, 0));

                }
                fishToil.defaultCompleteMode = ToilCompleteMode.Delay;

                switch (sizeAtBeginning)
                {
                    case FishSizeCategory.Small:
                        fishToil.defaultDuration = (int)(-(smallFishDurationFactor/20) * fishingSkill + smallFishDurationFactor * 1.5);
                        break;
                    case FishSizeCategory.Medium:
                        fishToil.defaultDuration = (int)(-(mediumFishDurationFactor / 20) * fishingSkill + mediumFishDurationFactor * 1.5);
                        break;
                    case FishSizeCategory.Large:
                        fishToil.defaultDuration = (int)(-(largeFishDurationFactor / 20) * fishingSkill + largeFishDurationFactor * 1.5);
                        break;
                    default:
                        fishToil.defaultDuration = mediumFishDurationFactor;
                        break;
                }
               
                //Log.Message(fishToil.defaultDuration.ToString());       
                fishToil.FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
                yield return fishToil.WithProgressBarToilDelay(TargetIndex.A, true);
                yield return new Toil
                {
                    initAction = delegate
                    {

                        Thing newFish = ThingMaker.MakeThing(fishCaught);
                        newFish.stackCount = fishAmountWithSkill;
                        GenSpawn.Spawn(newFish, this.TargetA.Cell - GenAdj.CardinalDirections[0], this.Map);
                        if (caughtSomethingSpecial)
                        {
                            Messages.Message("VCEF_CaughtSpecial".Translate(this.pawn.LabelCap, newFish.LabelCap), this.pawn, MessageTypeDefOf.NeutralEvent);
                            caughtSomethingSpecial = false;
                        }
                        StoragePriority currentPriority = StoreUtility.CurrentStoragePriorityOf(newFish);
                        IntVec3 c;
                        if (StoreUtility.TryFindBestBetterStoreCellFor(newFish, this.pawn, this.Map, currentPriority, this.pawn.Faction, out c, true))
                        {
                            this.job.SetTarget(TargetIndex.C, c);
                            this.job.SetTarget(TargetIndex.B, newFish);
                            this.job.count = newFish.stackCount;
                            
                            


                        }
                        else
                        {
                            this.EndJobWith(JobCondition.Incompletable);
                          
                        }

                    },
                    defaultCompleteMode = ToilCompleteMode.Instant
                };
                yield return Toils_Reserve.Reserve(TargetIndex.B, 1, -1, null);
                yield return Toils_Reserve.Reserve(TargetIndex.C, 1, -1, null);
                yield return Toils_Goto.GotoThing(TargetIndex.B, PathEndMode.ClosestTouch);
                yield return Toils_Haul.StartCarryThing(TargetIndex.B, false, false, false);
                Toil carryToCell = Toils_Haul.CarryHauledThingToCell(TargetIndex.C);
                yield return carryToCell;
                yield return Toils_Haul.PlaceHauledThingInCell(TargetIndex.C, carryToCell, true);



                yield break;
         
           
        }

        public int CalculateFishAmountWithSkillAndConditions(int amount)
        {
            int fishAmountFinal = 0;
            fishingSkill = this.pawn.skills.AverageOfRelevantSkillsFor(DefDatabase<WorkTypeDef>.GetNamed("VCEF_Fishing"));

            if (fishingSkill >= minFishingSkillForMinYield)
            {
                fishAmountFinal = amount + (int)((fishingSkill - minFishingSkillForMinYield) / 2);
            }
            else
            {
                fishAmountFinal = (int)(amount - (minFishingSkillForMinYield - fishingSkill));
            }
            float currentTempInMap = this.Map.mapTemperature.OutdoorTemp;

            if (currentTempInMap < minMapTempForLowFish)
            {
              

                fishAmountFinal = (int)(fishAmountFinal * Mathf.InverseLerp(minMapTempForLowFish - 20f, minMapTempForLowFish, currentTempInMap));
               
            }
            else if (currentTempInMap > maxMapTempForLowFish)
            {
               

                fishAmountFinal = (int)(fishAmountFinal * Mathf.InverseLerp(maxMapTempForLowFish+15, maxMapTempForLowFish, currentTempInMap));
              
            }

            if (fishAmountFinal < 1) { fishAmountFinal = 1; }

            return fishAmountFinal;

        }


        public ThingDef SelectFishToCatch(Zone_Fishing fishingZone)
        {
            if (fishingZone.fishInThisZone.Count > 0) {
                if (rand.NextDouble() > 0.99) {
                    List<FishDef> tempSpecialFish = new List<FishDef>();
                    tempSpecialFish.Clear();
                    foreach (FishDef element in DefDatabase<FishDef>.AllDefs.Where(element => element.fishSizeCategory == FishSizeCategory.Special))
                    {
                        tempSpecialFish.Add(element);
                    }
                    caughtSomethingSpecial = true;
                    return tempSpecialFish.RandomElementByWeight(((FishDef s) => s.commonality)).thingDef;

                } else {
                    ThingDef fishCaught = fishingZone.fishInThisZone.RandomElement();
                    return fishCaught;

                }
                
            } else return null;


        }

    }
}
