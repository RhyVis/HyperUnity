using HarmonyLib;
using LWM.DeepStorage;
using Verse;

namespace HyperUnity
{
  [HarmonyPatch(typeof(Settings), "ArchitectMenu_ChangeLocation")]
  public static class Patch_LVMDontTouchMyThing
  {
    public static void Postfix()
    {
      Log.Warning("I got LWM");
    }
  }
}