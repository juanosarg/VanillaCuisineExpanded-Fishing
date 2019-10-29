using System;
using System.Collections.Generic;
using Verse;
using Verse.AI;
using RimWorld;

namespace VCE_Fishing
{
    class JobDriver_Fish : JobDriver
    {
        public float pawnFishingSkill;

        public override bool TryMakePreToilReservations(bool errorOnFailed)
        {

            Pawn pawn = this.pawn;
            LocalTargetInfo target = this.job.targetA;
            Job job = this.job;
            bool result;
            if (pawn.Reserve(target, job, 1, -1, null, errorOnFailed))
            {
                pawn = this.pawn;
                target = this.job.targetA.Cell;
                job = this.job;
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

            this.FailOnDespawnedNullOrForbidden(TargetIndex.A);
            this.FailOnBurningImmobile(TargetIndex.A);
            /*yield return Toils_General.DoAtomic(delegate
            {
                this.job.count = 1;
            });*/

            pawnFishingSkill = this.pawn.skills.AverageOfRelevantSkillsFor(DefDatabase<WorkTypeDef>.GetNamed("VCEF_Fishing"));
            yield return Toils_Goto.GotoCell(TargetIndex.A, PathEndMode.Touch);
            this.pawn.rotationTracker.FaceTarget(base.TargetA);
            Toil fishToil = new Toil();
            fishToil.tickAction = delegate ()
            {
               
            };

            Rot4 pawnRotation = pawn.Rotation;
            IntVec3 facingCell = pawnRotation.FacingCell;
            if (facingCell == new IntVec3(0, 0, 1))
            {
                //Log.Message("Looking north");
                fishToil.WithEffect(() => DefDatabase<EffecterDef>.GetNamed("VCEF_FishingEffectNorth"), () => this.TargetA.Cell+ new IntVec3(0, 0, 1));


            }
            else if (facingCell == new IntVec3(1, 0, 0))
            {
               // Log.Message("Looking east");
               
                fishToil.WithEffect(() => DefDatabase<EffecterDef>.GetNamed("VCEF_FishingEffectEast"), () => this.TargetA.Cell+ new IntVec3(1, 0, 0));

            }
            else if (facingCell == new IntVec3(0, 0, -1))
            {
               // Log.Message("Looking south");
                fishToil.WithEffect(() => DefDatabase<EffecterDef>.GetNamed("VCEF_FishingEffectSouth"), () => this.TargetA.Cell+ new IntVec3(0, 0, -1));


            }
            else if (facingCell == new IntVec3(-1, 0, 0))
            {
              //  Log.Message("Looking west");
                fishToil.WithEffect(() => DefDatabase<EffecterDef>.GetNamed("VCEF_FishingEffectWest"), () => this.TargetA.Cell+ new IntVec3(-1, 0, 0));

            }
            fishToil.defaultCompleteMode = ToilCompleteMode.Delay;

            Zone_Fishing fishingZone = this.Map.zoneManager.ZoneAt(this.TargetA.Cell) as Zone_Fishing;

            switch (fishingZone.GetFishToCatch())
            {
                case FishSizeCategory.Small:
                    fishToil.defaultDuration = 1000;
                    break;
                case FishSizeCategory.Medium:
                    fishToil.defaultDuration = 2000;
                    break;
                case FishSizeCategory.Large:
                    fishToil.defaultDuration = 3000;
                    break;
                default:
                    fishToil.defaultDuration = 2000;
                    break;
            }

                        
            fishToil.FailOnCannotTouch(TargetIndex.A, PathEndMode.Touch);
            yield return fishToil.WithProgressBarToilDelay(TargetIndex.A, true);
            yield return new Toil
            {
                initAction = delegate
                {
                    Thing newFish = ThingMaker.MakeThing(DefDatabase<ThingDef>.GetNamed("VCEF_RawMackerel", true));
                    GenSpawn.Spawn(newFish, this.TargetA.Cell - GenAdj.CardinalDirections[0], this.Map);
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

    }
}
