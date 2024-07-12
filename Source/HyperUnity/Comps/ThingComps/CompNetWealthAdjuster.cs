using System.Text;
using PipeSystem;
using Verse;

namespace HyperUnity
{
  public class CompProperties_NetWealthAdjuster : CompProperties
  {
    public float ratio = 0.5f;
    
    public CompProperties_NetWealthAdjuster()
    {
      compClass = typeof(CompNetWealthAdjuster);
    }
  }
  
  public class CompNetWealthAdjuster : ThingComp
  {
    private CompProperties_NetWealthAdjuster Props => (CompProperties_NetWealthAdjuster)props;

    private CompResourceStorage _resourceStorage;
    public float AdjustWealthVal;

    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
      base.PostSpawnSetup(respawningAfterLoad);
      _resourceStorage = parent.TryGetComp<CompResourceStorage>();

      if (_resourceStorage == null)
      {
        Log.Error("[HyperUnity] CompNetWealthAdjuster must be used with CompResourceStorage!");
      }
    }

    public override void PostExposeData()
    {
      base.PostExposeData();
      Scribe_Values.Look(ref AdjustWealthVal, "adjustVal");
    }

    public override string CompInspectStringExtra()
    {
      var sb = new StringBuilder().Append(base.CompInspectStringExtra());
      sb.AppendLineIfNotEmpty().Append($"{"R_HyperUnity_PipeStorageWealthInspection".Translate()}: {AdjustWealthVal}");
      return sb.ToString();
    }

    public override void CompTick()
    {
      base.CompTick();
      if (!parent.IsHashIntervalTick(10000) || !parent.Spawned)
      {
        return;
      }
      UpdateAdjustWealthVal();
    }

    private void UpdateAdjustWealthVal()
    {
      if (_resourceStorage.AmountStored <= 0)
      {
        return;
      }
      AdjustWealthVal = _resourceStorage.AmountStored * Props.ratio;
    }
  }
}