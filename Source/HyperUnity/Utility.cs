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
        Msg.D("ConsumePower: Start power check.");
        var powerSum = powerComp.PowerNet.CurrentStoredEnergy();
        if (powerSum < requiredVal)
        {
          Msg.D("ConsumePower: Insufficient power.");
          return false;
        }
        var toConsume = requiredVal;
        foreach (var battery in powerComp.PowerNet.batteryComps)
        {
          if (battery.StoredEnergy > toConsume)
          {
            battery.SetStoredEnergyPct((battery.StoredEnergy - toConsume) / (battery.Props.storedEnergyMax));
            Msg.D("ConsumePower: Clear.");
            return true;
          }
          toConsume -= battery.StoredEnergy;
          battery.SetStoredEnergyPct(0f);
          Msg.D("ConsumePower: Cost " + battery.StoredEnergy);
          if (toConsume <= 0f)
          {
            Msg.D("ConsumePower: Clear.");
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

    public static bool DepleteSkillLevel(this SkillRecord skillRecord, float xp)
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
      if (pawn == null)
      {
        return;
      }
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

    public static void ApplyHediffWithStat(this Pawn pawn, HediffDef hediff, List<StatDef> stats = null, float severityAdjust = 1.0f)
    {
      if (pawn == null)
      {
        return;
      }
      if (!(stats?.NullOrEmpty() ?? true))
      {
        stats.ForEach(stat => severityAdjust *= pawn.GetStatValue(stat));
      }
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
      var target = pawn?.health.hediffSet.GetFirstHediffOfDef(hediff);
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
      Msg.D("Doing damage to " + bodyPart.def.label);
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

    public static void DamageRandomBodyPart(this Pawn pawn, float amount = 1f)
    {
      var target = pawn.health.hediffSet.GetNotMissingParts().RandomElement();
      if (target == null)
      {
        return;
      }
      Msg.D("Doing damage to " + target.def.label);
      pawn.TakeDamage(new DamageInfo(
        DamageDefOf.SurgicalCut,
        amount,
        999f,
        -1f,
        null,
        target,
        null,
        DamageInfo.SourceCategory.ThingOrUnknown,
        null,
        true,
        true,
        QualityCategory.Normal,
        false
      ));
    }

    public static void ThrowMote(this ThingComp comp, string s)
    {
      MoteMaker.ThrowText(comp.parent.TrueCenter(), comp.parent.Map, s);
    }

    public static void ThrowMote(this Thing thing, string s)
    {
      MoteMaker.ThrowText(thing.TrueCenter(), thing.Map, s);
    }

    public static IEnumerable<Thing> ThingGridInRange(this IntVec3 origin, Map map, float radius)
    {
      return GenRadial.RadialCellsAround(origin, radius, true)
        .Where(cell => cell.InBounds(map))
        .SelectMany(cell => map.thingGrid.ThingsListAt(cell));
    }

    public static IEnumerable<Thing> ThingGridInRoom(this IntVec3 origin, Map map)
    {
      var room = origin.GetRoom(map);
      
      if (room == null)
      {
        return new List<Thing>();
      }

      return room.Cells
        .SelectMany(cell => map.thingGrid.ThingsListAt(cell));
    }

    public static IEnumerable<Thing> ThingGridInRoom(this Thing origin)
    {
      var room = origin.GetRoom();
      
      if (room == null)
      {
        return new List<Thing>();
      }

      return room.Cells
        .SelectMany(cell => origin.Map.thingGrid.ThingsListAt(cell));
    }

    public static IEnumerable<Thing> ThingGridInRoom(this Room room)
    {
      if (room == null)
      {
        return new List<Thing>();
      }

      return room.Cells
        .SelectMany(cell => room.Map.thingGrid.ThingsListAt(cell));
    }

    public static TaggedString BoolTranslate(this bool b)
    {
      return b ? "R_HyperUnity_U_Enable".Translate() : "R_HyperUnity_U_Disable".Translate();
    }

    public static bool IsBetween(this int i, int min, int max)
    {
      return i > min && i < max;
    }

    public static bool IsBetween(this float f, float min, float max)
    {
      return f > min && f < max;
    }
  }

  public static class Msg
  {
    public static void D(string s)
    {
      if (HyperUnity_ModSettings.Debug)
      {
        Log.Message($"[HyperUnity] {s}");
      }
    }

    public static void E(string s)
    {
      Log.Error($"[HyperUnity] {s}");
    }
  }
}