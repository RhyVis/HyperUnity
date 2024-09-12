using RimWorld;
using UnityEngine;
using Verse;

namespace HyperUnity
{
  public class HediffCompProperties_SkillDeplete : HediffCompProperties
  {
    public int checkInterval = 10000;
    public int depleteAmount = 5000;

    public HediffCompProperties_SkillDeplete() => compClass = typeof(HediffComp_SkillDeplete);
  }

  public class HediffComp_SkillDeplete : HediffComp
  {
    private HediffCompProperties_SkillDeplete Props => (HediffCompProperties_SkillDeplete)props;

    private int _ticker;

    public override void CompPostMake()
    {
      base.CompPostMake();
      _ticker = Props.checkInterval;
    }

    public override void CompPostTick(ref float severityAdjustment)
    {
      base.CompPostTick(ref severityAdjustment);
      if (_ticker > 0)
      {
        _ticker--;
        return;
      }
      doDeplete();
      _ticker = Props.checkInterval;
    }

    private void doDeplete()
    {
      var skillRecord = parent.pawn.skills?.skills.RandomElement();
      if (skillRecord == null) return;
      if (!skillRecord.DepleteSkillLevel(Props.depleteAmount))
      {
        return;
      }
      MoteMaker.ThrowText(parent.pawn.TrueCenter() + new Vector3(0.5f, 0.5f, 0.5f),
        parent.pawn.Map,
        "R_HyperUnity_HediffComp_SkillDeplete_Mote".Translate(skillRecord.def.label, Props.depleteAmount));
    }
  }
}