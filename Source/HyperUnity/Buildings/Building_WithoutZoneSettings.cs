using System.Collections.Generic;
using RimWorld;
using Verse;

namespace HyperUnity
{
  public class Building_WithoutZoneSettings : Building_Storage
  {
    public override IEnumerable<Gizmo> GetGizmos()
    {
      string copyStr = "CommandCopyZoneSettingsLabel".Translate();
      string pasteStr = "CommandPasteZoneSettingsLabel".Translate();

      foreach (var gizmo in base.GetGizmos())
      {
        if (gizmo is Command_Action act && (act.defaultLabel == copyStr || act.defaultLabel == pasteStr)) continue;
        yield return gizmo;
      }
    }
  }
}