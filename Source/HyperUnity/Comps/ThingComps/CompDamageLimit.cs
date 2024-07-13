using Verse;

namespace HyperUnity
{
  public class CompProperties_DamageLimit : CompProperties
  {
    public int maxPerDamage = 5;
    public float minPercent = 0.5f;
    
    public CompProperties_DamageLimit()
    {
      compClass = typeof(CompDamageLimit);
    }
  }
  
  public class CompDamageLimit : ThingComp
  {
    private CompProperties_DamageLimit Props => (CompProperties_DamageLimit) props;

    private int _hitpointsCap;

    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
      base.PostSpawnSetup(respawningAfterLoad);
      _hitpointsCap = parent.def.useHitPoints ? parent.MaxHitPoints : 0;
    }

    public override void PostPreApplyDamage(ref DamageInfo dinfo, out bool absorbed)
    {
      if (Props.maxPerDamage == 0)
      {
        absorbed = true;
        dinfo.SetAmount(0);
      }
      else if (Props.maxPerDamage > 0 && Props.maxPerDamage < dinfo.Amount)
      {
        absorbed = false;
        dinfo.SetAmount(Props.maxPerDamage);
      }
      else
      {
        base.PostPreApplyDamage(ref dinfo, out absorbed);
      }
    }

    public override void PostPostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
    {
      base.PostPostApplyDamage(dinfo, totalDamageDealt);
      if (_hitpointsCap == 0)
      {
        return; 
      }
      if ((float)parent.HitPoints / _hitpointsCap <= Props.minPercent)
      {
        parent.HitPoints = _hitpointsCap;
      }
    }
  }
}