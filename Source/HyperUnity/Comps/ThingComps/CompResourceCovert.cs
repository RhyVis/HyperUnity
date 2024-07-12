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
    
    private List<ThingDef> _allowedThingDefs;

    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
      base.PostSpawnSetup(respawningAfterLoad);
      _allowedThingDefs = parent.def.building.fixedStorageSettings.filter.AllowedThingDefs.ToList();
    }

    public override void CompTick()
    {
      base.CompTick();
      if (!parent.IsHashIntervalTick(Props.checkInterval) || !parent.Spawned)
      {
        return;
      }
      doCovert();
    }

    private void doCovert()
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
          "R_HyperUnity_CompResourceCovert_Mote".Translate(value));
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