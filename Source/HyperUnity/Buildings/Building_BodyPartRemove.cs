using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace HyperUnity
{
  public class Building_BodyPartRemove : Building_Bed
  {
    private CompRefuelable _refuelable;
    
    private bool _auto;

    public override void SpawnSetup(Map map, bool respawningAfterLoad)
    {
      base.SpawnSetup(map, respawningAfterLoad);
      _refuelable = this.TryGetComp<CompRefuelable>();
    }

    public override void ExposeData()
    {
      base.ExposeData();
      Scribe_Values.Look(ref _auto, "autoRemove");
    }

    public override IEnumerable<Gizmo> GetGizmos()
    {
      foreach (var gizmo in base.GetGizmos())
      {
        yield return gizmo;
      }

      yield return new Command_Toggle()
      {
        defaultLabel = "R_HyperUnity_Building_BodypartRemove_Gizmo1_Label".Translate(),
        defaultDesc = "R_HyperUnity_Building_BodypartRemove_Gizmo1_Desc".Translate(),
        icon = ThingDefOf.MedicineHerbal.uiIcon,
        isActive = () => _auto,
        toggleAction = () => _auto = !_auto
      };

      yield return new Command_Action()
      {
        defaultLabel = "R_HyperUnity_Building_BodypartRemove_Gizmo2_Label".Translate(),
        defaultDesc = "R_HyperUnity_Building_BodypartRemove_Gizmo2_Desc".Translate(),
        icon = ThingDefOf.MedicineHerbal.uiIcon,
        action = RemoveBodypart
      };
    }

    public override void Tick()
    {
      base.Tick();
      if (!_auto || !this.IsHashIntervalTick(1250) || _refuelable == null || Spawned)
      {
        return;
      }
      RemoveBodypart();
    }

    private void RemoveBodypart()
    {
      if (CurOccupants.First() == null)
      {
        MoteMaker.ThrowText(this.TrueCenter() + new Vector3(0.5f, 0.5f, 0.5f), this.Map,
          "R_HyperUnity_Building_BodypartRemove_Mote1".Translate());
        return;
      }

      if (_refuelable.Fuel < 8f)
      {
        MoteMaker.ThrowText(this.TrueCenter() + new Vector3(0.5f, 0.5f, 0.5f), this.Map,
          "R_HyperUnity_Building_BodypartRemove_Mote2".Translate());
        return;
      }

      foreach (var pawn in CurOccupants)
      {
        var existParts = pawn.health.hediffSet.GetNotMissingParts()
          .Where(record => record.def.spawnThingOnRemoved != null).ToList();
        if (existParts.Count == 0)
        {
          return;
        }
        pawn.ApplyHediff(HU_HediffDefOf.R_BodyPartWorking);
        foreach (var bodyPart in existParts)
        {
          var part = ThingMaker.MakeThing(bodyPart.def.spawnThingOnRemoved);
          part.stackCount = 1;
          GenPlace.TryPlaceThing(part, pawn.Position, pawn.Map, ThingPlaceMode.Near);
          pawn.DamageBodyPart(bodyPart);
          MoteMaker.ThrowText(this.TrueCenter() + new Vector3(0.5f, 0.5f, 0.5f), this.Map,
            "R_HyperUnity_Building_BodypartRemove_Mote3".Translate(bodyPart.def.label));
        }
        pawn.RemoveHediff(HU_HediffDefOf.R_BodyPartWorking);
      }
      _refuelable.ConsumeFuel(8f);
    }
  }
}