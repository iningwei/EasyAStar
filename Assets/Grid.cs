using UnityEngine;
using System.Collections;
using System;

namespace iningwei.AStar
{
    public enum GridType
    {
        /// <summary>
        /// 正常可通行
        /// </summary>
        Normal = 0,
        /// <summary>
        /// 障碍物
        /// </summary>
        Obstacle,

        /// <summary>
        /// 起点
        /// </summary>
        Start,

        /// <summary>
        /// 终点
        /// </summary>
        End,
    }


    public struct GridIndex
    {
        public int rowIndex;
        public int columnIndex;
    }


    public struct FGH
    {
        public float f;
        public float g;
        public float h;
    }


    public class Grid : IComparable
    {

        public GridType GridType { get; set; }

        /// <summary>
        /// 网格X、Y、Z坐标
        /// </summary>
        public Vector3 Position { get; set; }

        public GridIndex Index { get; set; }

        /// <summary>
        /// 网格FGH值
        /// </summary>
        public FGH FGHValue { get; set; }

        /// <summary>
        /// 父格子
        /// </summary>
        public Grid Parent { get; set; }


        public Grid(float x, float y, float z)
        {
            this.Position = new Vector3(x, y, z);

        }

        public int CompareTo(object obj)
        {
            Grid grid = obj as Grid;
            if (this.FGHValue.f < grid.FGHValue.f)
            {
                //升序
                return -1;
            }
            if (this.FGHValue.f > grid.FGHValue.f)
            {
                //降序
                return 1;
            }

            return 0;
        }
    }

}