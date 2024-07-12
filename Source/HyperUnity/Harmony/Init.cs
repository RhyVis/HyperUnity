using HarmonyLib;
using Verse;

namespace HyperUnity
{
  [StaticConstructorOnStartup]
  internal static class Init
  {
    static Init()
    {
      var harmony = new Harmony("rhynia.works.hyperunity");
      harmony.PatchAll();
    }
  }
}