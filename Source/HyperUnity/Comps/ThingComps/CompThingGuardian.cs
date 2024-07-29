using Verse;

namespace HyperUnity
{
  public class CompProperties_RepairMachine : CompProperties
  {
    public int checkInterval = 60;
    public float radius = 3.0f;
    
    public CompProperties_RepairMachine()
    {
      compClass = typeof(CompRepairMachine);
    }
  }
  
  public class CompRepairMachine : ThingComp
  {
    private CompProperties_RepairMachine Props => (CompProperties_RepairMachine)props;

    public override void CompTick()
    {
      base.CompTick();
      if (!parent.IsHashIntervalTick(Props.checkInterval) || !parent.Spawned)
      {
        return;
      }
    }

    private void DoRepair()
    {
      var grid = Utility.ThingGridInRange(parent.Position, parent.Map, Props.radius);
    }
  }
}