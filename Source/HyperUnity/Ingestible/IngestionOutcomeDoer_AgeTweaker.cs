using RimWorld;
using Verse;

namespace HyperUnity
{
  public class IngestionOutcomeDoer_AgeTweaker : IngestionOutcomeDoer
  {
    private int offsetDays = 0;
    
    protected override void DoIngestionOutcomeSpecial(Pawn pawn, Thing ingested, int ingestedCount)
    {
      if (offsetDays == 0) return;
      if (offsetDays > 0)
      {
        if (pawn.RaceProps.Humanlike || pawn.RaceProps.baseBodySize <= 1f)
        {
          pawn.ageTracker.AgeBiologicalTicks += offsetDays * 60000;
        }
        else
        {
          pawn.ageTracker.AgeBiologicalTicks += (long)((float)(offsetDays) / pawn.RaceProps.baseBodySize);
        }
      }
      else
      {
        var offsetDaysAbs = -offsetDays;
        if (pawn.RaceProps.Humanlike || pawn.RaceProps.baseBodySize <= 1f)
        {
          if (pawn.ageTracker.AgeBiologicalTicks < offsetDaysAbs)
          {
            pawn.ageTracker.AgeBiologicalTicks = 0L;
          }
          else
          {
            pawn.ageTracker.AgeBiologicalTicks -= offsetDays * 60000;
          }
        }
        else
        {
          var offsetAdjusted = (long)((float)(offsetDays) / pawn.RaceProps.baseBodySize);
          if (pawn.ageTracker.AgeBiologicalTicks < offsetAdjusted)
          {
            pawn.ageTracker.AgeBiologicalTicks = 0L;
          }
          else
          {
            pawn.ageTracker.AgeBiologicalTicks -= offsetAdjusted * 60000;
          }
        }
      }
    }
  }
}