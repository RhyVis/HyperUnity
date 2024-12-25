using System.Linq;
using RimWorld;
using Verse;

namespace HyperUnity
{
  public class CompProperties_BedHediffGiver : CompProperties
  {
    public int checkInterval = 300;
    public HediffDef hediff;
    public float severityAdd = 1.0f;

    public CompProperties_BedHediffGiver()
    {
      compClass = typeof(CompBedHediffGiver);
    }
  }

  public class CompBedHediffGiver : ThingComp
  {
    private CompFacility _facility;
    private CompProperties_BedHediffGiver Props => (CompProperties_BedHediffGiver)props;

    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
      base.PostSpawnSetup(respawningAfterLoad);
      _facility = parent.TryGetComp<CompFacility>();
    }

    public override void CompTick()
    {
      base.CompTick();
      if (!parent.IsHashIntervalTick(Props.checkInterval) || parent.Fogged() || !parent.Spawned) return;
      GiveHediff();
    }

    private void GiveHediff()
    {
      _facility.LinkedBuildings.OfType<Building_Bed>()
        .SelectMany(bed => bed.CurOccupants)
        .Where(pawn => !pawn?.health.Dead ?? false)
        .ToList()
        .ForEach(pawn => pawn.ApplyHediff(Props.hediff, Props.severityAdd));
    }
  }
}