using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using RimWorld;
using UnityEngine;
using Verse;

namespace HyperUnity
{
  public static class Utility
  {
    // Values
    public static Vector3 RightUp = new Vector3(0.5f, 0.5f, 0.5f);
    
    public static void SetPrivateField(this object instance, string fieldName, object value)
    {
      var flag = BindingFlags.Instance | BindingFlags.NonPublic;
      var type = instance.GetType();
      var field = type.GetField(fieldName, flag);
      field?.SetValue(instance, value);
    }

    public static IEnumerable<Pawn> FindPawnsInRange(this ThingComp compInCenter, float range)
    {
      return compInCenter.parent.Map.mapPawns.AllPawnsSpawned
        .Where(pawn => pawn.Position.DistanceToSquared(compInCenter.parent.Position) < range * range);
    }

    public static IEnumerable<Pawn> FindPawnsAliveInRange(this ThingComp compInCenter, float range)
    {
      return compInCenter.parent.Map.mapPawns.AllPawnsSpawned
        .Where(pawn => pawn.Position.DistanceToSquared(compInCenter.parent.Position) < range * range)
        .Where(pawn => !pawn.health.Dead);
    }
    
    public static bool ConsumePower(this ThingComp comp, float requiredVal)
    {
      var powerComp = comp.parent.TryGetComp<CompPowerTrader>();
      if (powerComp != null && powerComp.PowerOn)
      {
        Log.Message("[HyperUnity] Start power check.");
        var powerSum = powerComp.PowerNet.CurrentStoredEnergy();
        if (powerSum < requiredVal)
        {
          return false;
        }
        var toConsume = requiredVal;
        foreach (var battery in powerComp.PowerNet.batteryComps)
        {
          if (battery.StoredEnergy > toConsume)
          {
            battery.SetStoredEnergyPct((battery.StoredEnergy - toConsume) / (battery.Props.storedEnergyMax));
            return true;
          }
          toConsume -= battery.StoredEnergy;
          battery.SetStoredEnergyPct(0f);
          if (toConsume <= 0f)
          {
            return true;
          }
        }
      }
      return false;
    }

    public static void CompSpawnThingWithPowerCost(this ThingComp comp, string defName, int minCount, int refillCount, int eachItemCost)
    {
      if (comp.parent is IHaulDestination)
      {
        var slotGroup = comp.parent.GetSlotGroup();
        if (slotGroup != null)
        {
          var presentCount = slotGroup.HeldThings
            .Where(thing => thing.def.defName == defName)
            .Sum(thing => thing.stackCount);
          if (presentCount < minCount)
          {
            var targetDef = ThingDef.Named(defName);
            if (targetDef == null)
            {
              Log.Error($"[HyperUnity] There's no thing named {defName} at {comp.parent.Position} setting.");
              return;
            }
            var addUpStack = ThingMaker.MakeThing(targetDef);
            var addUpVal = refillCount <= targetDef.stackLimit
              ? refillCount - presentCount
              : targetDef.stackLimit - presentCount;
            if (comp.ConsumePower(addUpVal * eachItemCost))
            {
              addUpStack.stackCount = addUpVal;
              GenPlace.TryPlaceThing(addUpStack, comp.parent.Position, comp.parent.Map, ThingPlaceMode.Near);
            }
          }
        }
      }
    }

    public static float countAllAdjustWealthVal()
    {
      return Find.CurrentMap.listerBuildings
        .AllBuildingsColonistOfDef(ThingDef.Named("R_WealthTank"))
        .Where(thing => thing.TryGetComp<CompNetWealthAdjuster>() != null)
        .Sum(thing => thing.GetComp<CompNetWealthAdjuster>().AdjustWealthVal);
    }
  
    public static bool depleteSkillLevel(this SkillRecord skillRecord, float xp)
    {
      if (skillRecord.levelInt <= 0 || skillRecord.TotallyDisabled || xp <= 0) return false;

      while (xp > skillRecord.xpSinceLastLevel)
      {
        --skillRecord.levelInt;
        skillRecord.xpSinceLastLevel += skillRecord.XpRequiredForLevelUp;
        if (skillRecord.levelInt <= 0)
        {
          skillRecord.levelInt = 0;
          skillRecord.xpSinceLastLevel = 0f;
          return true;
        }
      }
      
      if (skillRecord.xpSinceLastLevel >= xp)
      {
        skillRecord.xpSinceLastLevel -= xp;
      }
      else
      {
        skillRecord.xpSinceLastLevel = 0;
      }

      return true;
    }

    public static void ApplyHediff(this Pawn pawn, HediffDef hediff, float severityAdjust = 1.0f)
    {
      var target = pawn.health.hediffSet.GetFirstHediffOfDef(hediff);
      if (target == null)
      {
        target = HediffMaker.MakeHediff(hediff, pawn);
        target.Severity = severityAdjust;
        pawn.health.AddHediff(target);
      }
      else
      {
        target.Severity += severityAdjust;
      }
    }

    public static void RemoveHediff(this Pawn pawn, HediffDef hediff)
    {
      var target = pawn.health.hediffSet.GetFirstHediffOfDef(hediff);
      if (target == null)
      {
        return;
      }
      pawn.health.RemoveHediff(target);
    }

    public static bool HasHediff(this Pawn pawn, HediffDef hediff)
    {
      return pawn?.health.hediffSet.GetFirstHediffOfDef(hediff) != null;
    }

    public static void DamageBodyPart(this Pawn pawn, BodyPartRecord bodyPart)
    {
      pawn.TakeDamage(new DamageInfo(
        DamageDefOf.SurgicalCut,
        9999f,
        999f,
        -1f,
        null,
        bodyPart,
        null,
        DamageInfo.SourceCategory.ThingOrUnknown,
        null,
        true,
        true,
        QualityCategory.Normal,
        false
      ));
    }
  }
}