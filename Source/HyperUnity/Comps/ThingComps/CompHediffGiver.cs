using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace HyperUnity
{
  public class CompProperties_HediffGiver : CompProperties
  {
    public int checkInterval = 600;
    public float radius = 2.9f;
    public float severityAdjust = 1.0f;
    public bool danger = false;
    public HediffDef hediffDef;
    public List<StatDef> stats;

    public CompProperties_HediffGiver() => compClass = typeof(CompHediffGiver);
  }
  
  public class CompHediffGiver : ThingComp
  {
    private CompProperties_HediffGiver Props => (CompProperties_HediffGiver)props;

    // 1: All, 2: Hostile, 3: Everyone except Player, 4: Prisoner
    private int _factionType;

    public override IEnumerable<Gizmo> CompGetGizmosExtra()
    {
      foreach (var gizmo in base.CompGetGizmosExtra())
      {
        yield return gizmo;
      }
      yield return new Command_Action()
      {
        defaultLabel = GizmoLabel,
        defaultDesc = "R_HyperUnity_CompHediffGiver_Gizmo_Desc".Translate(),
        icon = GizmoIcon,
        action = SwitchFactionType
      };
    }

    public override void PostExposeData()
    {
      base.PostExposeData();
      Scribe_Values.Look(ref _factionType, "factionType");
    }

    public override void CompTick()
    {
      base.CompTick();
      if (_factionType == 0 || !parent.IsHashIntervalTick(Props.checkInterval) || !parent.Spawned)
      {
        return;
      }
      GiveHediff();
    }

    private void GiveHediff()
    {
      switch (_factionType)
      {
        case 0: break;
        case 1:
        {
          this.FindPawnsAliveInRange(Props.radius)
            .ToList().ForEach(pawn => pawn.ApplyHediffWithStat(Props.hediffDef, Props.stats, Props.severityAdjust));
          break;
        }
        case 2:
        {
          this.FindPawnsAliveInRange(Props.radius)
            .Where(pawn => (pawn.Faction != null && pawn.Faction.HostileTo(Faction.OfPlayer)) ||
                           (pawn.AnimalOrWildMan() && pawn.InAggroMentalState))
            .ToList().ForEach(pawn => pawn.ApplyHediffWithStat(Props.hediffDef, Props.stats, Props.severityAdjust));
          break;
        }
        case 3:
        {
          this.FindPawnsAliveInRange(Props.radius)
            .Where(pawn => !pawn.Faction?.IsPlayer ?? true)
            .ToList().ForEach(pawn => pawn.ApplyHediffWithStat(Props.hediffDef, Props.stats, Props.severityAdjust));
          break;
        }
        case 4:
        {
          this.FindPawnsAliveInRange(Props.radius)
            .Where(pawn => pawn.IsPrisoner)
            .ToList().ForEach(pawn => pawn.ApplyHediffWithStat(Props.hediffDef, Props.stats, Props.severityAdjust));
          break;
        }
        default:
        {
          Msg.E($"Unexpected Type of {_factionType}");
          this.ThrowMote("Unexpected Type");
          _factionType = 0;
          break;
        }
      }
    }

    private void SwitchFactionType()
    {
      _factionType = (_factionType + 1) % 5;
    }

    private string GizmoLabel
    {
      get
      {
        switch (_factionType)
        {
          case 0: return "R_HyperUnity_CompHediffGiver_Gizmo_LabelA".Translate();
          case 1: return "R_HyperUnity_CompHediffGiver_Gizmo_LabelB".Translate();
          case 2: return "R_HyperUnity_CompHediffGiver_Gizmo_LabelC".Translate();
          case 3: return "R_HyperUnity_CompHediffGiver_Gizmo_LabelD".Translate();
          case 4: return "R_HyperUnity_CompHediffGiver_Gizmo_LabelE".Translate();
          default: return "ERR";
        }
      }
    }
    
    private static readonly Texture2D _prisonerIcon = ContentFinder<Texture2D>.Get("UI/Commands/ForPrisoners");

    private Texture2D GizmoIcon
    {
      get
      {
        switch (_factionType)
        {
          case 0: return TexCommand.ClearPrioritizedWork;
          case 1: return TexCommand.ForbidOff;
          case 2: return TexCommand.Attack;
          case 3: return TexCommand.FireAtWill;
          case 4: return _prisonerIcon;
          default: return TexCommand.CannotShoot;
        }
      }
    }
  }
}