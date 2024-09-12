using System.Collections.Generic;
using RimWorld;
using Verse;

namespace HyperUnity
{
  public class CompProperties_GasEmit : CompProperties
  {
    public int gas = 1;
    public int amount = 1600;
    
    public CompProperties_GasEmit() => compClass = typeof(CompGasEmit);
  }
  public class CompGasEmit : ThingComp
  {
    private CompProperties_GasEmit Props => (CompProperties_GasEmit)props;
    private CompPowerTrader _powerTrader;

    private static GasType[] _gasTypes = {GasType.BlindSmoke, GasType.ToxGas, GasType.RotStink, GasType.DeadlifeDust};
    private bool _activated;

    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
      base.PostSpawnSetup(respawningAfterLoad);
      _powerTrader = parent.TryGetComp<CompPowerTrader>();
      
      if (_powerTrader == null)
      {
        Log.Error("[HyperUnity] CompPowerTrader needed for CompGasEmit!"); 
      }
    }

    public override void PostExposeData()
    {
      base.PostExposeData();
      Scribe_Values.Look(ref _activated, "emitActivated");
    }

    public override IEnumerable<Gizmo> CompGetGizmosExtra()
    {
      foreach (var gizmo in base.CompGetGizmosExtra())
      {
        yield return gizmo;
      }
      yield return new Command_Toggle()
      {
        defaultLabel = "R_HyperUnity_CompGasEmit_Gizmo_Label".Translate(),
        defaultDesc = "R_HyperUnity_CompGasEmit_Gizmo_Desc".Translate(),
        icon = _activated ? TexCommand.ForbidOff : TexCommand.ForbidOn,
        isActive = () => _activated,
        toggleAction = () => _activated = !_activated
      };
    }

    public override void CompTick()
    {
      base.CompTick();
      if (!parent.IsHashIntervalTick(300) || !_powerTrader.PowerOn || !_activated || !parent.Spawned)
      {
        return;
      }
      GasUtility.AddGas(parent.Position, parent.Map, _gasTypes[Props.gas], Props.amount);
    }
  }
}