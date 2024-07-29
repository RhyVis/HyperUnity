using UnityEngine;
using Verse;

namespace HyperUnity
{
  public class HyperUnity_Mod : Mod
  {
    public HyperUnity_Mod(ModContentPack content) : base(content)
    {
      Log.Message("[HyperUnity] Hello RimWorld!");
    }

    public override void DoSettingsWindowContents(Rect inRect)
    {
      var listing = new Listing_Standard();
      listing.Begin(inRect);
      listing.CheckboxLabeled("R_HyperUnity_SettingCat_DebugMsg_Label".Translate(), ref HyperUnity_ModSettings.Debug);
      listing.End();
    }
    
    public override string SettingsCategory() {
      return "R_HyperUnity_SettingCat_Label".Translate();
    }
  }
    
  public class HyperUnity_ModSettings : ModSettings
  {
    public static bool Debug;

    public override void ExposeData()
    {
      Scribe_Values.Look(ref Debug, "debugBool");
    }
  }
}