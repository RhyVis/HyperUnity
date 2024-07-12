using System.Linq;
using Verse;

namespace HyperUnity
{
  public class CompProperties_HediffGiver : CompProperties
  {
    public int checkInterval = 600;
    public float range = 1;
    public float severityAdd;
    public HediffDef hediffDef;

    public CompProperties_HediffGiver()
    {
      compClass = typeof(CompHediffGiver);
    }
  }
  
  public class CompHediffGiver : ThingComp
  {
    private CompProperties_HediffGiver Props => (CompProperties_HediffGiver)props;

    public override void CompTick()
    {
      base.CompTick();
      if (!parent.IsHashIntervalTick(Props.checkInterval) || parent.Spawned)
      {
        return;
      }
      this.FindPawnsAliveInRange(Props.range)
        .ToList().ForEach(pawn => pawn.ApplyHediff(Props.hediffDef, Props.severityAdd));
    }
  }
}