using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Diagnostics;

namespace iningwei.AStar
{





    [ExecuteInEditMode]
    public class AStar : MonoBehaviour
    {
        public Action<Vector3, Vector3> OnSearchPath;
        public Action<Path> OnGetPath;

        //这里必须声明为public，否则对应Editor脚本找不到
        public float gridSize = 1f;
        public int rowCount = 10;
        public int columnCount = 10;


        //List<Grid> allGrids = new List<Grid>();
        Grid[,] allGrids;

        List<Grid> openGrids = new List<Grid>();
        List<Grid> closeGrids = new List<Grid>();

        Vector3 bottomLeft;//左下角index为（0，0）的点的坐标
        Vector3 upRight;//右上角index为（rowCount-1,columnCount-1）的点的坐标

        float checkOverlapThreshold;
        void OnEnable()
        {
            _int();
            Stopwatch watch = new Stopwatch();
            watch.Start();
            _getAllGrids();
            watch.Stop();
            UnityEngine.Debug.Log("格网信息初始化用时：" + watch.ElapsedMilliseconds);

            OnSearchPath += _doSearchPath;
        }

        private void _int()
        {
            checkOverlapThreshold = _sqrt(gridSize * 0.5f);
            bottomLeft = new Vector3(-columnCount * 0.5f * gridSize, 0, -rowCount * 0.5f * gridSize);
            upRight = new Vector3(-bottomLeft.x, bottomLeft.y, -bottomLeft.z);
        }

        void OnDisable()
        {
            allGrids = null;

            openGrids.Clear();
            closeGrids.Clear();
        }

        Grid startGrid = null;
        Grid endGrid = null;
        void _doSearchPath(Vector3 startPos, Vector3 endPos)
        {
            Stopwatch watch = new Stopwatch();
            watch.Start();


            //先清空
            openGrids.Clear();
            closeGrids.Clear();
            for (int i = 0; i < rowCount; i++)
            {
                for (int j = 0; j < columnCount; j++)
                {
                    if (allGrids[i, j].GridType != GridType.Obstacle)
                    {
                        allGrids[i, j].GridType = GridType.Normal;
                        allGrids[i, j].Parent = null;
                        allGrids[i, j].FGHValue = new FGH() { f = 0f, g = 0f, h = 0f };
                    }
                }

            }



            startGrid = _getGridByPoint(startPos);
            startGrid.GridType = GridType.Start;

            endGrid = _getGridByPoint(endPos);
            if (endGrid.GridType == GridType.Obstacle)
            {
                UnityEngine.Debug.LogError("this endPos is in obstalce");
                return;
            }

            if (endGrid.GridType == GridType.Start)
            {
                UnityEngine.Debug.LogError("startGrid和endGrid重复");
                return;
            }
            endGrid.GridType = GridType.End;


            //把起点放入openGrids            
            openGrids.Add(startGrid);
            Grid currentGrid = startGrid;
            while (openGrids.Count > 0 && currentGrid.GridType != GridType.End)
            {

                //8方向遍历，寻找可达节点，并放入openGrids中
                for (int i = -1; i <= 1; i++)
                {
                    for (int j = -1; j <= 1; j++)
                    {
                        if (i != 0 || j != 0)
                        {
                            //Grid temp = allGrids.Find(a => (a.Index.rowIndex == currentGrid.Index.rowIndex + i) && (a.Index.columnIndex == currentGrid.Index.columnIndex + j));
                            Grid temp = allGrids[currentGrid.Index.rowIndex + i, currentGrid.Index.columnIndex + j];

                            if (temp != null && temp.GridType != GridType.Obstacle && !closeGrids.Contains(temp))
                            {
                                if (!openGrids.Contains(temp))
                                {
                                    if (temp.CompareTo(currentGrid) == 1)//temp的f值＜currentGrid的f值
                                    {
                                        openGrids.Insert(0, temp);
                                    }
                                    else
                                    {
                                        openGrids.Add(temp);
                                    }

                                    temp.Parent = currentGrid;
                                    temp.FGHValue = _getFGHValue(temp, endGrid);
                                }
                                else
                                {
                                    Grid oldParent = temp.Parent;
                                    temp.Parent = currentGrid;
                                    FGH newFGH = _getFGHValue(temp, endGrid);
                                    if (newFGH.g < temp.FGHValue.g)
                                    {
                                        temp.FGHValue = newFGH;
                                    }
                                    else
                                    {
                                        temp.Parent = oldParent;
                                    }
                                }
                            }
                        }
                    }
                }

                //从openGrids中删除当前点，并把它加入closeGrids。
                openGrids.Remove(currentGrid);
                closeGrids.Add(currentGrid);

                //从openGrids中寻找出相对靠谱的作为当前点
                //currentGrid = openGrids.OrderBy(a => a.FGHValue.f).FirstOrDefault();//法一

                ////法二， 比法一效率稍微高点
                //currentGrid = openGrids[0];
                //for (int i = 1; i < openGrids.Count; i++)
                //{
                //    if (openGrids[i].FGHValue.f < currentGrid.FGHValue.f)
                //    {
                //        currentGrid = openGrids[i];
                //    }
                //}

                //法三，基于每次更新openList的时候把最小的放在index 0的基础
                currentGrid = openGrids[0];


                if (currentGrid == null)
                {
                    UnityEngine.Debug.LogError("error!!!!");
                }

                if (currentGrid.GridType == GridType.End)
                {
                    watch.Stop();
                    UnityEngine.Debug.Log("找到路径, 用时：" + watch.ElapsedMilliseconds);
                    watch.Reset();
                    _getPath(currentGrid, startPos, endPos);
                }


            }



        }

        private void _getPath(Grid currentGrid, Vector3 startPoint, Vector3 endPoint)
        {
            List<Vector3> pathList = new List<Vector3>();
            pathList.Add(currentGrid.Position);
            while (currentGrid.Parent != null)
            {
                currentGrid = currentGrid.Parent;
                pathList.Add(currentGrid.Position);
            }

            pathList.RemoveAt(pathList.Count - 1);//删除最后一个，改成startPoint
            pathList.Add(startPoint);

            pathList.Reverse();

            pathList.RemoveAt(pathList.Count - 1);//删除最后一个，改成endPoint
            pathList.Add(endPoint);
            //说明：以上不用寻路找到网格的起点grid和终点grid作为坐标点，而用原始点代替，是为了防止后续按照路径行走的扭曲

            UnityEngine.Debug.Log("路径点数：" + pathList.Count);
            if (OnGetPath != null)
            {
                OnGetPath(new Path(pathList));
            }
        }

        private FGH _getFGHValue(Grid temp, Grid endGrid)
        {
            float fValue = 0;
            float gValue = 0;
            float hValue = 0;

            gValue = temp.Parent.FGHValue.g + Vector3.Distance(temp.Parent.Position, temp.Position);
            hValue = _getHValue(temp, endGrid);
            fValue = gValue + hValue;
            return new FGH() { f = fValue, g = gValue, h = hValue };
        }

        private float _getHValue(Grid temp, Grid endGrid)
        {
            //使用哈夫曼距离
            float xValue = temp.Position.x - endGrid.Position.x;
            float yValue = temp.Position.y - endGrid.Position.y;
            float zValue = temp.Position.z - endGrid.Position.z;
            float hValue = Mathf.Abs(xValue) + Mathf.Abs(yValue) + Mathf.Abs(zValue);
            return hValue;
        }

        Grid _getGridByPoint(Vector3 point)
        {
            Grid grid = null;
            //////for (int i = 0; i < rowCount; i++)
            //////{
            //////    for (int j = 0; j < columnCount; j++)
            //////    {
            //////        if (allGrids[i, j].GridType != GridType.Obstacle)
            //////        {
            //////            if (_checkPointOverlap(point, allGrids[i, j]))
            //////            {
            //////                grid = allGrids[i, j];
            //////                return grid;
            //////            }
            //////        }
            //////    }
            //////}
            //////if (grid == null)
            //////{
            //////    UnityEngine.Debug.LogError("point:" + point + ", can not find grid");
            //////}

            //上述寻找目标点的方式比较耗时，改成根据point坐标获得index的方式
            Vector2 indexPair = _getIndexPairByPoint(point);
            grid = allGrids[(int)indexPair.x, (int)indexPair.y];


            return grid;
        }

        Vector2 _getIndexPairByPoint(Vector3 point)
        {
            Vector2 indexPair = Vector2.zero;
            float pairX = 0;
            float pairZ = 0;

            float xValue = point.x;
            float zValue = point.z;
            //TODO:
            //if (xValue - bottomLeft.x > 0)
            //{
            //    pairX = (int)((xValue - bottomLeft.x) / gridSize);
            //}
            //else
            //{
            //    pairX = 0;
            //}

            pairZ = (int)(Mathf.Abs(xValue - bottomLeft.x) / gridSize);//X方向上获得的是列index
            pairX = (int)(Mathf.Abs(zValue - bottomLeft.z) / gridSize);//Z方向上获得的是行index

            indexPair = new Vector2(pairX, pairZ);
            return indexPair;
        }

        bool _checkPointOverlap(Vector3 point, Grid grid)
        {
            bool isOverlap = false;
            if (Vector3.Distance(point, grid.Position) <= checkOverlapThreshold)
            {
                isOverlap = true;
            }

            return isOverlap;
        }


        private void _getAllGrids()
        {
            allGrids = new Grid[rowCount, columnCount];

            for (int i = 0; i < rowCount; i++)
            {
                for (int j = 0; j < columnCount; j++)
                {
                    Grid grid = new Grid((-columnCount * 0.5f + j + 0.5f) * gridSize, 0, (-rowCount * 0.5f + i + 0.5f) * gridSize);//暂定Z值为0
                    grid.Index = new GridIndex() { rowIndex = i, columnIndex = j };

                    allGrids[i, j] = grid;
                    if (Physics.OverlapSphere(grid.Position, gridSize, 1 << LayerMask.NameToLayer("Obstacle")).Length > 0)//设置障碍格网
                    {
                        grid.GridType = GridType.Obstacle;
                    }
                }
            }
        }

        float _sqrt(float size)
        {
            return Mathf.Sqrt(2 * size * size);
        }
        #region gizmos

        void OnDrawGizmos()
        {
            _drawOutBorder();
            _drawWalkableGridBorder();
            _drawObstacleGrids();
        }

        void _drawOutBorder()
        {
            Vector3 bottomLeft = new Vector3(-columnCount * 0.5f * gridSize, 0, -rowCount * 0.5f * gridSize);
            Vector3 bottomRight = new Vector3(-bottomLeft.x, bottomLeft.y, bottomLeft.z);
            Vector3 upLeft = new Vector3(bottomLeft.x, bottomLeft.y, -bottomLeft.z);
            Vector3 upRight = new Vector3(-bottomLeft.x, bottomLeft.y, -bottomLeft.z);
            _drawBorder(Color.green, bottomLeft, bottomRight, upLeft, upRight);
        }

        private void _drawWalkableGridBorder()
        {
            if (allGrids.Length > 0)
            {
                Gizmos.color = Color.gray;

                for (int i = 0; i < rowCount; i++)
                {
                    for (int j = 0; j < columnCount; j++)
                    {
                        if (allGrids[i, j].GridType == GridType.Normal)
                        {
                            Vector3 bottomLeft = new Vector3(allGrids[i, j].Position.x - 0.5f * gridSize, 0, allGrids[i, j].Position.z - 0.5f * gridSize);
                            Vector3 bottomRight = new Vector3(allGrids[i, j].Position.x + 0.5f * gridSize, 0, allGrids[i, j].Position.z - 0.5f * gridSize);
                            Vector3 upLeft = new Vector3(allGrids[i, j].Position.x - 0.5f * gridSize, 0, allGrids[i, j].Position.z + 0.5f * gridSize);
                            Vector3 upRight = new Vector3(allGrids[i, j].Position.x + 0.5f * gridSize, 0, allGrids[i, j].Position.z + 0.5f * gridSize);
                            _drawBorder(Color.gray, bottomLeft, bottomRight, upLeft, upRight, true);
                        }
                    }


                }
            }
        }

        private void _drawObstacleGrids()
        {
            if (allGrids.Length > 0)
            {
                Gizmos.color = Color.red;
                for (int i = 0; i < rowCount; i++)
                {
                    for (int j = 0; j < columnCount; j++)
                    {
                        if (allGrids[i, j].GridType == GridType.Obstacle)
                        {
                            Gizmos.DrawCube(allGrids[i, j].Position, Vector3.one * 0.2f);
                        }
                    }
                }
            }
            else
            {
                UnityEngine.Debug.LogError("allGrids is null");
            }
        }

        void _drawBorder(Color color, Vector3 bottomLeft, Vector3 bottomRight, Vector3 upLeft, Vector3 upRight, bool isDrawDiagonal = false)
        {
            Gizmos.color = color;

            //bottom line
            Gizmos.DrawLine(bottomLeft, bottomRight);
            //left line
            Gizmos.DrawLine(bottomLeft, upLeft);
            //right line
            Gizmos.DrawLine(bottomRight, upRight);
            //up line
            Gizmos.DrawLine(upLeft, upRight);

            if (isDrawDiagonal)
            {
                Gizmos.DrawLine(bottomLeft, upRight);
                Gizmos.DrawLine(bottomRight, upLeft);
            }

        }
        #endregion
    }
}