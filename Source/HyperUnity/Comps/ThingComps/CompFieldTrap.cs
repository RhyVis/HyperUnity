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
    private bool _teleport;

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
      Scribe_Values.Look(ref _teleport, "emitTeleport");
    }

    public override IEnumerable<Gizmo> CompGetGizmosExtra()
    {
      foreach (var gizmo in base.CompGetGizmosExtra())
      {
        yield return gizmo;
      }
      yield return new Command_Toggle()
      {
        defaultLabel = "R_HyperUnity_CompFieldTrap_Gizmo_Label1".Translate(),
        defaultDesc = "R_HyperUnity_CompFieldTrap_Gizmo_Desc1".Translate(),
        icon = _activated ? TexCommand.ForbidOff : TexCommand.ForbidOn,
        isActive = () => _activated,
        toggleAction = delegate
        {
          _activated = !_activated;
        }
      };
      yield return new Command_Toggle()
      {
        defaultLabel = "R_HyperUnity_CompFieldTrap_Gizmo_Label2".Translate(),
        defaultDesc = "R_HyperUnity_CompFieldTrap_Gizmo_Desc2".Translate(),
        icon = TexCommand.FireAtWill,
        isActive = () => _teleport,
        toggleAction = delegate
        {
          _teleport = !_teleport;
        }
      };
    }

    public override void CompTick()
    {
      base.CompTick();
      if (!parent.IsHashIntervalTick(Props.checkInterval) || !_powerTrader.PowerOn || !_activated || !parent.Spawned)
      {
        return;
      }
      DoStun();
    }

    private void DoStun()
    {
      var targetPos = parent.Position;
      List<Pawn> pawns;

      if (Props.ignoreDistance)
      {
        pawns = parent.Map.mapPawns.AllPawnsSpawned
          .Where(pawn => !pawn.health.Dead)
          .Where(pawn => (pawn.Faction != null && pawn.Faction.HostileTo(Faction.OfPlayer)) ||
                         (pawn.AnimalOrWildMan() && pawn.InAggroMentalState))
          .Where(pawn => !pawn.IsPrisoner)
          .ToList();
      }
      else
      {
        pawns = this.FindPawnsAliveInRange(Props.range)
          .Where(pawn => (pawn.Faction != null && pawn.Faction.HostileTo(Faction.OfPlayer)) ||
                         (pawn.AnimalOrWildMan() && pawn.InAggroMentalState))
          .Where(pawn => !pawn.IsPrisoner)
          .ToList();
      }
      
      if (pawns.Count == 0)
      {
        return;
      }

      SoundDefOf.Psycast_Skip_Entry.PlayOneShot(new TargetInfo(parent.Position, parent.Map));
      
      if (_teleport)
      {
        foreach (var pawn in pawns)
        {
          pawn.ApplyHediff(HU_HediffDefOf.R_SlowHediff);
          pawn.Position = targetPos;
          pawn.jobs.StopAll();
          pawn.stances.stunner.StunFor(Props.stunTick, parent, true, !pawn.stances.stunner.Stunned);
        }
      }
      else
      {
        foreach (var pawn in pawns)
        {
          pawn.ApplyHediff(HU_HediffDefOf.R_SlowHediff);
          pawn.jobs.StopAll();
          pawn.stances.stunner.StunFor(Props.stunTick, parent, true, !pawn.stances.stunner.Stunned);
        }
      }
    }
  }
  
}