using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using Verse.Sound;

namespace HyperUnity
{
  
  public class CompProperties_FieldTrap : CompProperties
  {
    public int checkInterval = 120;
    public int stunTick = 180;
    public float range = 1;
    public bool ignoreDistance = false;

    public CompProperties_FieldTrap()
    {
      compClass = typeof(CompFieldTrap);
    }
  }
  
  public class CompFieldTrap : ThingComp
  {
    private CompProperties_FieldTrap Props => (CompProperties_FieldTrap)props;

    private CompPowerTrader _powerTrader;
    
    private bool _activated;

    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
      base.PostSpawnSetup(respawningAfterLoad);
      _powerTrader = parent.TryGetComp<CompPowerTrader>();

      if (_powerTrader == null)
      {
       Log.Error("[HyperUnity] CompPowerTrader needed for CompFieldTrap!"); 
      }
    }
    
    public override void PostExposeData()
    {
      base.PostExposeData();
      Scribe_Values.Look(ref _activated, "emitActivated");
    }

    public override IEnumerable<Gizmo> CompGetGizmosExtra()
    {
      yield return new Command_Toggle()
      {
        defaultLabel = "R_HyperUnity_CompFieldTrap_Gizmo_Label".Translate(),
        defaultDesc = "R_HyperUnity_CompFieldTrap_Gizmo_Desc".Translate(),
        icon = _activated ? TexCommand.ForbidOff : TexCommand.ForbidOn,
        isActive = () => _activated,
        toggleAction = () => _activated = !_activated
      };
    }

    public override void CompTick()
    {
      base.CompTick();
      if (!parent.IsHashIntervalTick(Props.checkInterval) || !_powerTrader.PowerOn || !_activated || !parent.Spawned)
      {
        return;
      }
      pullPawns();
    }

    private void pullPawns()
    {
      var targetPos = parent.Position;
      List<Pawn> pawns;

      if (Props.ignoreDistance)
      {
        pawns = parent.Map.mapPawns.AllPawnsSpawned
          .Where(pawn => !pawn.health.Dead)
          .Where(pawn => (pawn.Faction != null && pawn.Faction.HostileTo(Faction.OfPlayer)) ||
                         (pawn.AnimalOrWildMan() && pawn.InAggroMentalState))
          .Where(pawn => !pawn.IsPrisonerInPrisonCell())
          .ToList();
      }
      else
      {
        pawns = this.FindPawnsAliveInRange(Props.range)
          .Where(pawn => (pawn.Faction != null && pawn.Faction.HostileTo(Faction.OfPlayer)) ||
                         (pawn.AnimalOrWildMan() && pawn.InAggroMentalState))
          .Where(pawn => !pawn.IsPrisonerInPrisonCell())
          .ToList();
      }
      
      if (pawns.Count == 0)
      {
        return;
      }
      SoundDefOf.Psycast_Skip_Entry.PlayOneShot(new TargetInfo(parent.Position, parent.Map));
      foreach (var pawn in pawns)
      {
        pawn.ApplyHediff(HU_HediffDefOf.R_SlowHediff);//别动！
        pawn.Position = targetPos;//过来！
        pawn.jobs.StopAll();//别走！
        pawn.stances.stunner.StunFor(Props.stunTick, parent, true, !pawn.stances.stunner.Stunned);
      }
    }
  }
  
}