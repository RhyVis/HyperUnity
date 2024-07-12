using System;
using HarmonyLib;
using RimWorld;
using Verse;

namespace HyperUnity
{
  [HarmonyPatch(typeof(WealthWatcher), "CalculateWealthItems")]
  public static class Patch_WealthWatcher
  {
    public static void Postfix(ref float __result)
    {
      try
      {
        __result += Utility.countAllAdjustWealthVal();
      }
      catch (NullReferenceException)
      {
      }
      catch (Exception e)
      {
        Log.Error("Exception in wealth fix: " + e);
      }
    }
  }
}