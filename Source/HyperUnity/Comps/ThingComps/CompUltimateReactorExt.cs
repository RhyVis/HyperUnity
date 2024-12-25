using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.Sound;

namespace HyperUnity
{
  public class CompProperties_CompUltimateReactorExt : CompProperties
  {
    public int checkInterval = 3600; // One hour in game
    public int eachItemCost = 10;
    public int minCount = 0;
    public int refillCount = 10;

    public CompProperties_CompUltimateReactorExt()
    {
      compClass = typeof(CompUltimateReactorExt);
    }
  }

  public class CompUltimateReactorExt : ThingComp
  {
    private List<ThingDef> _allowedThingDefs;
    private CompForbiddable _forbiddable;

    private CompPowerTrader _powerTrader;

    private ThingDef _targetThingDef;
    private CompProperties_CompUltimateReactorExt Props => (CompProperties_CompUltimateReactorExt)props;

    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
      base.PostSpawnSetup(respawningAfterLoad);
      _powerTrader = parent.TryGetComp<CompPowerTrader>();
      _forbiddable = parent.TryGetComp<CompForbiddable>();

      _allowedThingDefs = parent.def.building.fixedStorageSettings.filter.AllowedThingDefs.ToList();

      if (!respawningAfterLoad) _targetThingDef = _allowedThingDefs[0];

      if (_powerTrader == null || _forbiddable == null)
        Log.Error("[HyperUnity] CompUltimateReactorExt need CompPowerTrader and CompForbiddable!");
    }

    public override void PostExposeData()
    {
      base.PostExposeData();
      Scribe_Defs.Look(ref _targetThingDef, "targetThingDef");
    }

    public override IEnumerable<Gizmo> CompGetGizmosExtra()
    {
      foreach (var gizmo in base.CompGetGizmosExtra()) yield return gizmo;
      yield return new Command_Action
      {
        defaultLabel = "R_HyperUnity_CompUltimateReactor_Gizmo_Label".Translate(_targetThingDef.label),
        defaultDesc = "R_HyperUnity_CompUltimateReactor_Gizmo_Desc".Translate(_targetThingDef.label),
        action = ToggleTargetThingDef,
        icon = _targetThingDef.uiIcon
      };
    }

    public override string CompInspectStringExtra()
    {
      var sb = new StringBuilder();
      sb.Append(base.CompInspectStringExtra());
      sb.AppendLineIfNotEmpty()
        .Append($"{"R_HyperUnity_CompUltimateReactor_TargetName".Translate()}: {_targetThingDef.label}");
      sb.AppendLineIfNotEmpty()
        .Append($"{"R_HyperUnity_CompUltimateReactor_AmountKeep".Translate()}: {Props.minCount}/{Props.refillCount}");
      sb.AppendLineIfNotEmpty()
        .Append($"{"R_HyperUnity_CompUltimateReactor_PowerUse".Translate()}: {Props.eachItemCost} Wd");
      return sb.ToString();
    }

    public override void CompTick()
    {
      base.CompTick();
      if ((parent.IsHashIntervalTick(Props.checkInterval) && _powerTrader.PowerOn && !_forbiddable.Forbidden) ||
          !parent.Spawned)
        this.CompSpawnThingWithPowerCost(_targetThingDef.defName, Props.minCount, Props.refillCount,
          Props.eachItemCost);
    }

    private void ToggleTargetThingDef()
    {
      SoundDefOf.FlickSwitch.PlayOneShot(new TargetInfo(parent.Position, parent.Map));
      var currentIndex = _allowedThingDefs.IndexOf(_targetThingDef);
      currentIndex = (currentIndex + 1) % _allowedThingDefs.Count;
      _targetThingDef = _allowedThingDefs.ElementAt(currentIndex);
    }
  }
}