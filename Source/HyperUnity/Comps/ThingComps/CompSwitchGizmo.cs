using System.Collections.Generic;
using RimWorld;
using Verse;

namespace HyperUnity
{
  public class CompSwitchGizmo : ThingComp
  {
    private bool _activated;

    public override void CompTick()
    {
      base.CompTick();
      Scribe_Values.Look(ref _activated, "compSwitchActivated");
    }

    public override IEnumerable<Gizmo> CompGetGizmosExtra()
    {
      yield return new Command_Toggle()
      {
        defaultLabel = "R_HyperUnity_CompSwitchGizmo_Gizmo_Label".Translate(),
        defaultDesc = "R_HyperUnity_CompSwitchGizmo_Gizmo_Desc".Translate(),
        icon = _activated ? TexCommand.ForbidOff : TexCommand.ForbidOn,
        isActive = () => _activated,
        toggleAction = () => _activated = !_activated
      };
    }
  }
}