using System.Collections.Generic;
using RimWorld;
using UnityEngine;
using Verse;

namespace HyperUnity
{
  public class CompProperties_RemoteDoor : CompProperties
  {
    public CompProperties_RemoteDoor() => compClass = typeof(CompRemoteDoor);
  }

  public class CompRemoteDoor : ThingComp
  {
    private CompPowerTrader _powerTrader;

    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
      base.PostSpawnSetup(respawningAfterLoad);
      _powerTrader = parent.TryGetComp<CompPowerTrader>();

      if (_powerTrader == null)
      {
        Log.Error($"No CompPowerTrader set with CompRemoteDoor at {parent.Position}");
      }
    }

    public override IEnumerable<Gizmo> CompGetGizmosExtra()
    {
      foreach (var gizmo in base.CompGetGizmosExtra())
      {
        yield return gizmo;
      }
      yield return new Command_Toggle()
      {
        defaultLabel = "R_HyperUnity_CompRemoteDoor_Gizmo_Label".Translate(),
        defaultDesc = "R_HyperUnity_CompRemoteDoor_Gizmo_Desc".Translate(),
        icon = TexCommand.HoldOpen,
        hotKey = KeyBindingDefOf.Misc6,
        isActive = () => parent is Building_Door door && door.Open,
        toggleAction = delegate
        {
          if (_powerTrader != null && _powerTrader.PowerOn)
          {
            if (parent is Building_Door door)
            {
              setDoorState(!door.Open);
            }
          }
          else
          {
            MoteMaker.ThrowText(parent.TrueCenter() + new Vector3(0.5f, 0.5f, 0.5f), parent.Map, "未通电");
          }
        }
      };
    }
    
    private void setDoorState(bool b)
    {
      if (parent is Building_Door door)
      {
        door.SetPrivateField("openInt", b);
      }
    }
  }
}