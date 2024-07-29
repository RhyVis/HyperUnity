using Verse;

namespace HyperUnity
{
  public class CompProperties_TemperatureControl : CompProperties
  {
    public CompProperties_TemperatureControl()
    {
      compClass = typeof(CompTemperatureControl);
    }
  }
  public class CompTemperatureControl : ThingComp
  {
    private bool _activated;
    private float _targetTemperature = 21f;
    
    public override void CompTick()
    {
      base.CompTick();
      if (!_activated || !parent.Spawned)
      {
        return;
      }
      TweakTemperature();
    }

    public override void PostExposeData()
    {
      base.PostExposeData();
      Scribe_Values.Look(ref _activated, "activated");
    }

    private void TweakTemperature()
    {
      var room = parent.GetRoom();
      if (room == null)
      {
        this.ThrowMote("R_HyperUnity_CompThingGuardian_Mote1".Translate());
        _activated = false;
        return;
      }

      var present = room.Temperature;
      if (present < _targetTemperature || present > _targetTemperature)
      {
        room.Temperature = _targetTemperature;
      }
    }
  }
}