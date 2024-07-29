using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace HyperUnity
{
  public class CompProperties_ThingGuardian : CompProperties
  {
    public int checkInterval = 60;
    
    public CompProperties_ThingGuardian()
    {
      compClass = typeof(CompThingGuardian);
    }
  }
  
  public class CompThingGuardian : ThingComp
  {
    private CompProperties_ThingGuardian Props => (CompProperties_ThingGuardian)props;

    private bool _activated;

    public override IEnumerable<Gizmo> CompGetGizmosExtra()
    {
      foreach (var gizmo in base.CompGetGizmosExtra())
      {
        yield return gizmo;
      }
      yield return new Command_Toggle()
      {
        defaultLabel = "R_HyperUnity_CompThingGuardian_Gizmo_Label".Translate(),
        defaultDesc = "R_HyperUnity_CompThingGuardian_Gizmo_Desc".Translate(),
        icon = _activated ? TexCommand.ForbidOff : TexCommand.ForbidOn,
        isActive = () => _activated,
        toggleAction = () => _activated = !_activated
      };
    }

    public override void PostExposeData()
    {
      base.PostExposeData();
      Scribe_Values.Look(ref _activated, "activated");
    }

    public override void CompTick()
    {
      base.CompTick();
      
      if (!_activated || !parent.IsHashIntervalTick(Props.checkInterval) || !parent.Spawned)
      {
        return;
      }

      if (parent.GetRoom() == null)
      {
        _activated = false;
        parent.ThrowMote("R_HyperUnity_CompThingGuardian_Mote1".Translate());
      }
      
      DoRepair();
    }

    private void DoRepair()
    {
      var grid = parent.ThingGridInRoom()
        .Where(thing => thing.def.category != ThingCategory.Pawn);
      
      foreach (var thing in grid)
      {
        if (thing.def.useHitPoints && thing.HitPoints < thing.MaxHitPoints)
        {
          thing.HitPoints = thing.MaxHitPoints;
        }

        var rottable = thing.TryGetComp<CompRottable>();
        if (rottable != null)
        {
          rottable.RotProgress -= 2000f;
          if (rottable.RotProgress < 0f)
          {
            rottable.RotProgress = 0f;
          }
        }
      }
    }
  }
}