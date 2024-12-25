using System.Collections.Generic;
using System.Linq;
using RimWorld;
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
    private bool _activated;
    private bool _allyOnly;
    private bool _clearGas;
    private CompProperties_ThingGuardian Props => (CompProperties_ThingGuardian)props;

    public override IEnumerable<Gizmo> CompGetGizmosExtra()
    {
      foreach (var gizmo in base.CompGetGizmosExtra()) yield return gizmo;
      yield return new Command_Toggle
      {
        defaultLabel = "R_HyperUnity_CompThingGuardian_Gizmo_Label1".Translate(),
        defaultDesc = "R_HyperUnity_CompThingGuardian_Gizmo_Desc1".Translate(),
        icon = _activated ? TexCommand.ForbidOff : TexCommand.ForbidOn,
        isActive = () => _activated,
        toggleAction = () => _activated = !_activated
      };
      yield return new Command_Toggle
      {
        defaultLabel = "R_HyperUnity_CompThingGuardian_Gizmo_Label2".Translate(),
        defaultDesc = "R_HyperUnity_CompThingGuardian_Gizmo_Desc2".Translate(),
        icon = TexCommand.PauseCaravan,
        isActive = () => _allyOnly,
        toggleAction = () => _allyOnly = !_allyOnly
      };
      yield return new Command_Toggle
      {
        defaultLabel = "R_HyperUnity_CompThingGuardian_Gizmo_Label3".Translate(),
        defaultDesc = "R_HyperUnity_CompThingGuardian_Gizmo_Desc3".Translate(),
        icon = TexCommand.ToggleVent,
        isActive = () => _clearGas,
        toggleAction = () => _clearGas = !_clearGas
      };
    }

    public override void PostExposeData()
    {
      base.PostExposeData();
      Scribe_Values.Look(ref _activated, "activated");
      Scribe_Values.Look(ref _allyOnly, "allyOnly");
      Scribe_Values.Look(ref _clearGas, "clearGas");
    }

    public override void CompTick()
    {
      base.CompTick();

      if (!_activated || !parent.IsHashIntervalTick(Props.checkInterval) || !parent.Spawned) return;

      if (parent.GetRoom() == null)
      {
        _activated = false;
        parent.ThrowMote("R_HyperUnity_CompThingGuardian_Mote1".Translate());
      }

      DoRepair();
      if (_clearGas) ClearGas();
    }

    private void DoRepair()
    {
      var grid = parent.ThingGridInRoom()
        .Where(thing => thing.def.category != ThingCategory.Pawn)
        .Where(thing => !_allyOnly || (thing.Faction?.IsPlayer ?? false));

      foreach (var thing in grid)
      {
        if (thing.def.useHitPoints && thing.HitPoints < thing.MaxHitPoints) thing.HitPoints = thing.MaxHitPoints;

        var rottable = thing.TryGetComp<CompRottable>();
        if (rottable != null)
        {
          rottable.RotProgress -= 2000f;
          if (rottable.RotProgress < 0f) rottable.RotProgress = 0f;
        }
      }
    }

    private void ClearGas()
    {
      var density = parent.MapHeld.gasGrid.AccessPrivateField<uint[]>("gasDensity");
      var cells = parent.GetRoom().Cells;
      foreach (var cell in cells)
      {
        var i = CellIndicesUtility.CellToIndex(cell, parent.Map.Size.x);
        density[i] = 0U;
      }

      parent.Map.mapDrawer.WholeMapChanged((ulong)MapMeshFlagDefOf.Gas);
    }
  }
}