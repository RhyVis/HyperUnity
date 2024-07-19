using System.Collections.Generic;
using RimWorld;
using Verse;

namespace HyperUnity
{
  public class CompProperties_ExplosiveTrigger : CompProperties
  {
    public CompProperties_ExplosiveTrigger()
    {
      compClass = typeof(CompExplosiveTrigger);
    }
  }
  
  public class CompExplosiveTrigger : ThingComp
  {
    private CompExplosive _explosive;

    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
      base.PostSpawnSetup(respawningAfterLoad);
      _explosive = parent.TryGetComp<CompExplosive>();
    }

    public override IEnumerable<Gizmo> CompGetGizmosExtra()
    {
      yield return new Command_Action()
      {
        defaultLabel = "R_HyperUnity_CompExplosiveTrigger_Gizmo_Label".Translate(),
        defaultDesc = "R_HyperUnity_CompExplosiveTrigger_Gizmo_Desc".Translate(),
        icon = ThingDefOf.Shell_AntigrainWarhead.uiIcon,
        action = DoTrigger
      };
    }

    private void DoTrigger()
    {
      _explosive.StartWick();
    }
  }
}