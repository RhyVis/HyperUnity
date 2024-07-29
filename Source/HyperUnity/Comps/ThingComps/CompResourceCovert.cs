using System.Collections.Generic;
using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace HyperUnity
{
  public class CompProperties_ResourceCovert : CompProperties
  {
    public int checkInterval = 1250;
    public float ratio = -1.0f;
    
    public CompProperties_ResourceCovert()
    {
      compClass = typeof(CompResourceCovert);
    }
  }
  
  public class CompResourceCovert : ThingComp
  {
    private CompProperties_ResourceCovert Props => (CompProperties_ResourceCovert)props;

    public override IEnumerable<Gizmo> CompGetGizmosExtra()
    {
      foreach (var gizmo in base.CompGetGizmosExtra())
      {
        yield return gizmo;
      }
      yield return new Command_Action()
      {
        defaultLabel = "R_HyperUnity_CompResourceCovert_Gizmo_Label".Translate(),
        defaultDesc = "R_HyperUnity_CompResourceCovert_Gizmo_Desc".Translate(),
        icon = TexCommand.ForbidOff,
        action = DoCovert
      };
    }

    public override void CompTick()
    {
      base.CompTick();
      if (!parent.IsHashIntervalTick(Props.checkInterval) || !parent.Spawned)
      {
        return;
      }
      DoCovert();
    }

    private void DoCovert()
    {
      if (parent is IHaulDestination)
      {
        var things = parent.GetSlotGroup().HeldThings.Where(thing => thing.def != ThingDefOf.Silver).ToList();
        if (things.Count == 0)
        {
          return;
        }
      
        var value = 0f;
        foreach (var thing in things)
        {
          value += thing.MarketValue * thing.stackCount;
          thing.Destroy();
        }
        if (Props.ratio >= 0f)
        {
          value *= Props.ratio;
        }
        MoteMaker.ThrowText(parent.TrueCenter() + new Vector3(0.5f, 0.5f, 0.5f), parent.Map,
          "R_HyperUnity_CompResourceCovert_Mote".Translate((int)value));
        var stackSilverLimit = ThingDefOf.Silver.stackLimit;
        while (value > 0)
        {
          var stackSilver = ThingMaker.MakeThing(ThingDefOf.Silver);
          if (value > stackSilverLimit)
          {
            stackSilver.stackCount = stackSilverLimit;
            GenPlace.TryPlaceThing(stackSilver, parent.Position, parent.Map, ThingPlaceMode.Near);
            value -= stackSilverLimit;
          }
          else
          {
            stackSilver.stackCount = (int)value;
            GenPlace.TryPlaceThing(stackSilver, parent.Position, parent.Map, ThingPlaceMode.Near);
            value = 0;
          }
        }
      }
    }
  }
}