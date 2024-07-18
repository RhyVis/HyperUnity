using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;

namespace HyperUnity
{
  public class CompProperties_HarvestBed : CompProperties
  {
    public int checkInterval = 1250;
    public float harvestFullCost = 8.0f;
    public float harvestLegCost = 2.0f;
    public float harvestEyeCost = 2.0f;
    
    public CompProperties_HarvestBed()
    {
      compClass = typeof(CompHarvestBed);
    }
  }
  
  public class CompHarvestBed : ThingComp
  {
    private CompProperties_HarvestBed Props => (CompProperties_HarvestBed)props;

    private CompRefuelable _refuelable;

    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
      base.PostSpawnSetup(respawningAfterLoad);
      _refuelable = parent.TryGetComp<CompRefuelable>();
    }

    public override IEnumerable<Gizmo> CompGetGizmosExtra()
    {
      yield return new Command_Action()
      {
        defaultLabel = "R_HyperUnity_Building_BodypartRemove_Gizmo1_Label".Translate(),
        defaultDesc = "R_HyperUnity_Building_BodypartRemove_Gizmo1_Desc".Translate(),
        icon = ThingDefOf.MedicineHerbal.uiIcon,
        action = Harvest
      };
      yield return new Command_Action()
      {
        defaultLabel = "R_HyperUnity_Building_BodypartRemove_Gizmo2_Label".Translate(),
        defaultDesc = "R_HyperUnity_Building_BodypartRemove_Gizmo2_Desc".Translate(),
        icon = ThingDefOf.MedicineHerbal.uiIcon,
        action = HarvestLegs
      };
      yield return new Command_Action()
      {
        defaultLabel = "R_HyperUnity_Building_BodypartRemove_Gizmo3_Label".Translate(),
        defaultDesc = "R_HyperUnity_Building_BodypartRemove_Gizmo3_Desc".Translate(),
        icon = ThingDefOf.MedicineHerbal.uiIcon,
        action = HarvestEyes
      };
    }
    
    private void Harvest()
    {
      if (parent is Building_Bed bed && bed.CurOccupants.FirstOrDefault() != null)
      {
        if (_refuelable.Fuel < Props.harvestFullCost)
        {
          MoteMaker.ThrowText(bed.TrueCenter() + Utility.RightUp, bed.Map,
            "R_HyperUnity_Building_BodypartRemove_Mote2".Translate());
          return;
        }

        var pawn = bed.CurOccupants.First();
        pawn.ApplyHediff(HU_HediffDefOf.R_BodyPartWorking);
        pawn.health.hediffSet.GetNotMissingParts()
          .Where(record => record.def.spawnThingOnRemoved != null)
          .ToList()
          .ForEach(record =>
          {
            var partToSpawn = ThingMaker.MakeThing(record.def.spawnThingOnRemoved);
            partToSpawn.stackCount = 1;
            GenPlace.TryPlaceThing(partToSpawn, parent.Position, parent.Map, ThingPlaceMode.Near);
            pawn.DamageBodyPart(record);
          });
        pawn.RemoveHediff(HU_HediffDefOf.R_BodyPartWorking);
        _refuelable.ConsumeFuel(Props.harvestFullCost);
      }
      else
      {
        MoteMaker.ThrowText(parent.TrueCenter() + Utility.RightUp, parent.Map,
          "R_HyperUnity_Building_BodypartRemove_Mote1".Translate());
      }
    }
    
    private void HarvestLegs()
    {
      if (parent is Building_Bed bed && bed.CurOccupants.FirstOrDefault() != null)
      {
        if (_refuelable.Fuel < Props.harvestLegCost)
        {
          MoteMaker.ThrowText(bed.TrueCenter() + Utility.RightUp, bed.Map,
            "R_HyperUnity_Building_BodypartRemove_Mote2".Translate());
          return;
        }

        var pawn = bed.CurOccupants.First();
        pawn.ApplyHediff(HU_HediffDefOf.R_BodyPartWorking);
        pawn.health.hediffSet.GetNotMissingParts()
          .Where(record => record.def.defName.Contains("Leg"))
          .ToList()
          .ForEach(record =>
          {
            if (record.def.spawnThingOnRemoved != null)
            {
              var partToSpawn = ThingMaker.MakeThing(record.def.spawnThingOnRemoved);
              partToSpawn.stackCount = 1;
              GenPlace.TryPlaceThing(partToSpawn, parent.Position, parent.Map, ThingPlaceMode.Near);
            }
            pawn.DamageBodyPart(record);
          });
        pawn.RemoveHediff(HU_HediffDefOf.R_BodyPartWorking);
        _refuelable.ConsumeFuel(Props.harvestLegCost);
      }
      else
      {
        MoteMaker.ThrowText(parent.TrueCenter() + Utility.RightUp, parent.Map,
          "R_HyperUnity_Building_BodypartRemove_Mote1".Translate());
      }
    }
    
    private void HarvestEyes()
    {
      if (parent is Building_Bed bed && bed.CurOccupants.FirstOrDefault() != null)
      {
        if (_refuelable.Fuel < Props.harvestEyeCost)
        {
          MoteMaker.ThrowText(bed.TrueCenter() + Utility.RightUp, bed.Map,
            "R_HyperUnity_Building_BodypartRemove_Mote2".Translate());
          return;
        }

        var pawn = bed.CurOccupants.First();
        pawn.ApplyHediff(HU_HediffDefOf.R_BodyPartWorking);
        pawn.health.hediffSet.GetNotMissingParts()
          .Where(record => record.def.defName.Contains("Eye"))
          .ToList()
          .ForEach(record =>
          {
            if (record.def.spawnThingOnRemoved != null)
            {
              var partToSpawn = ThingMaker.MakeThing(record.def.spawnThingOnRemoved);
              partToSpawn.stackCount = 1;
              GenPlace.TryPlaceThing(partToSpawn, parent.Position, parent.Map, ThingPlaceMode.Near);
            }
            pawn.DamageBodyPart(record);
          });
        pawn.RemoveHediff(HU_HediffDefOf.R_BodyPartWorking);
        _refuelable.ConsumeFuel(Props.harvestEyeCost);
      }
      else
      {
        MoteMaker.ThrowText(parent.TrueCenter() + Utility.RightUp, parent.Map,
          "R_HyperUnity_Building_BodypartRemove_Mote1".Translate());
      }
    }
  }
}