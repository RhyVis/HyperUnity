using System.Linq;
using RimWorld;
using Verse;

namespace HyperUnity
{
  public class CompProperties_BedHediffGiver : CompProperties
  {
    public int checkInterval = 300;
    public float severityAdd = 1.0f;
    public HediffDef hefiff;

    public CompProperties_BedHediffGiver()
    {
      compClass = typeof(CompBedHediffGiver);
    }
  }

  public class CompBedHediffGiver : ThingComp
  {
    private CompProperties_BedHediffGiver Props => (CompProperties_BedHediffGiver)props;

    public override void CompTick()
    {
      base.CompTick();
      if (!parent.IsHashIntervalTick(Props.checkInterval) || !parent.Spawned)
      {
        return;
      }

      var map = parent.Map;
      parent.CellsAdjacent8WayAndInside()
        .SelectMany(cell => cell.GetThingList(map).OfType<Building_Bed>().SelectMany(bed => bed.CurOccupants))
        .Where(pawn => !pawn.health.Dead)
        .Distinct()
        .ToList()
        .ForEach(pawn => pawn.ApplyHediff(Props.hefiff, Props.severityAdd));
    }
  }
}