using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace iningwei.AStar
{
    public class Path
    {
        public static Action<int> OnWayPointReached;

        public List<Vector3> vetexList { get; set; }
        public Path(List<Vector3> vetexs)
        {
            this.vetexList = vetexs;
        }

    }
}