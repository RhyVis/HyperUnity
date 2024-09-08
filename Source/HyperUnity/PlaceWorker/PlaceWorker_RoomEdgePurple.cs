﻿using System.Linq;
using UnityEngine;
using Verse;

namespace HyperUnity
{
  public class PlaceWorker_RoomEdgePurple : PlaceWorker
  {
    public override void DrawGhost(ThingDef def, IntVec3 center, Rot4 rot, Color ghostCol, Thing thing = null)
    {
      var map = Find.CurrentMap;  
      var room = center.GetRoom(map);
      if (room != null)
      {
        GenDraw.DrawFieldEdges(room.Cells.ToList(), new Color(178,102,255));
      }
    }
  }
}