﻿using RimWorld;
using System.Collections.Generic;
using System.Diagnostics;
using Verse;
using Verse.AI;

namespace VCE_Fishing
{
    class WorkGiver_Fish : WorkGiver_Scanner
    {

        public override PathEndMode PathEndMode
        {
            get
            {
                return PathEndMode.Touch;
            }

        }


        [DebuggerHidden]
        public override IEnumerable<IntVec3> PotentialWorkCellsGlobal(Pawn pawn)
        {
            Danger maxDanger = pawn.NormalMaxDanger();
            
           
            List<Zone> zonesList = pawn.Map.zoneManager.AllZones;
            for (int j = 0; j < zonesList.Count; j++)
            {
                Zone_Fishing fishingZone = zonesList[j] as Zone_Fishing;
                if (fishingZone != null)
                {
                    if (fishingZone.cells.Count == 0)
                    {
                        Log.ErrorOnce("Fishing zone has 0 cells (this should never happen): " + fishingZone, -563487, false);
                    }
                    if (!fishingZone.someoneFishing) {
                        if (fishingZone.allowFishing)
                        {
                            if (fishingZone.isZoneBigEnough)
                            {
                                if (!fishingZone.isZoneEmpty)
                                {
                                    if (!fishingZone.ContainsStaticFire)
                                    {
                                        if (pawn.CanReserveAndReach(fishingZone.Cells[0], PathEndMode.OnCell, maxDanger))
                                        {
                                            for (int k = 0; k < fishingZone.cells.Count; k++)
                                            {
                                                yield return fishingZone.cells[k];
                                            }

                                        }
                                    }


                                }



                            }

                        }

                    }
                    
                    
                }
            }
           
        }

        public override Job JobOnCell(Pawn pawn, IntVec3 c, bool forced = false)
        {
           
            LocalTargetInfo target = c;
           
            if (!pawn.CanReserve(target, 1, -1, null, false))
            {
                return null;
            }
            if (ModsConfig.IdeologyActive && DefDatabase<HistoryEventDef>.GetNamedSilentFail("VME_Fishing")!=null && !new HistoryEvent(DefDatabase<HistoryEventDef>.GetNamedSilentFail("VME_Fishing"), pawn.Named(HistoryEventArgsNames.Doer)).Notify_PawnAboutToDo_Job())
            {
                return null;
            }
            Job job = new Job(DefDatabase<JobDef>.GetNamed("VCEF_FishJob"),c);
            return job;
        }

    }
}
