using System.Linq;
using RimWorld;
using UnityEngine;
using Verse;

namespace HyperUnity
{
  public class CompProperties_PawnSuppressor : CompProperties
  {
    public int checkInterval = 180;
    public float powerCost = 200;
    public float range = 5;
    
    public CompProperties_PawnSuppressor()
    {
      compClass = typeof(CompPawnSuppressor);
    }
  }
  
  public class CompPawnSuppressor : ThingComp
  {
    private CompProperties_PawnSuppressor Props => (CompProperties_PawnSuppressor)props;
    
    private CompPowerTrader _powerTrader;
    private CompForbiddable _forbiddable;

    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
      base.PostSpawnSetup(respawningAfterLoad);
      _powerTrader = parent.TryGetComp<CompPowerTrader>();
      _forbiddable = parent.TryGetComp<CompForbiddable>();
      
      if (_powerTrader == null || _forbiddable == null)
      {
        Log.Error("[HyperUnity] CompPowerTrader and CompForbiddable needed for CompFieldTrap!"); 
      }
    }

    public override void CompTick()
    {
      base.CompTick();
      if (!parent.IsHashIntervalTick(Props.checkInterval) || !_powerTrader.PowerOn || _forbiddable.Forbidden || !parent.Spawned)
      {
        return;
      }
      
      var pawns = this.FindPawnsInRange(Props.range)
        .Where(pawn => pawn.InAggroMentalState);
      foreach (var pawn in pawns)
      {
        if (!this.ConsumePower(Props.powerCost))
        {
          continue;
        }
        pawn.ApplyHediff(HU_HediffDefOf.R_SuppressedHediff);
        MoteMaker.ThrowText(pawn.TrueCenter() + new Vector3(0.5f, 0.5f, 0.5f), parent.Map, "R_HyperUnity_CompPawnSuppressor_Mote".Translate());
      }
    }
  }
}