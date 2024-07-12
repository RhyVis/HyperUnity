using System.Text;
using RimWorld;
using Verse;

namespace HyperUnity
{
  
  public class CompProperties_UltimateReactor: CompProperties
  {
    public int checkInterval = 3600; // One hour in game
    public int minCount = 0;
    public int refillCount = 10;
    public int eachItemCost = 10;
    public string itemDefName = "Steel";

    public CompProperties_UltimateReactor()
    {
      compClass = typeof(CompUltimateReactor);
    }
  }

  public class CompUltimateReactor : ThingComp
  {
    private CompProperties_UltimateReactor Props => (CompProperties_UltimateReactor)props;

    private CompPowerTrader _powerTrader;
    private CompForbiddable _forbiddable;

    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
      base.PostSpawnSetup(respawningAfterLoad);
      _powerTrader = parent.TryGetComp<CompPowerTrader>();
      _forbiddable = parent.TryGetComp<CompForbiddable>();

      if (_powerTrader == null || _forbiddable == null)
      {
        Log.Error("[HyperUnity] CompUltimateReactor need CompPowerTrader and CompForbiddable!");
      }
    }

    public override string CompInspectStringExtra()
    {
      var sb = new StringBuilder();
      sb.Append(base.CompInspectStringExtra());
      sb.AppendLineIfNotEmpty().Append($"{"R_HyperUnity_CompUltimateReactor_AmountKeep".Translate()}: {Props.minCount}/{Props.refillCount}");
      sb.AppendLineIfNotEmpty().Append($"{"R_HyperUnity_CompUltimateReactor_PowerUse".Translate()}: {Props.eachItemCost} Wd");
      return sb.ToString();
    }

    public override void CompTick()
    {
      base.CompTick();
      if (parent.IsHashIntervalTick(Props.checkInterval) && _powerTrader.PowerOn && !_forbiddable.Forbidden || !parent.Spawned)
      {
        this.CompSpawnThingWithPowerCost(Props.itemDefName, Props.minCount, Props.refillCount, Props.eachItemCost);
      }
    }
  }
  
}