
using UnityEngine;
using System.Collections;
using System.Collections.Generic;



public class MoveAlongWayPoints : MonoBehaviour
{

    /*
    public static MoveAlongWayPoints instance;
    public GameObject moveObj = null;
    GameObject jingWei;
    Animator jingWeiAnimator;

    public GameObject wayPointsParent = null;
    Transform[] wayPoints;

    public bool directionOriented = false;
    public float directionChangeSpeed = 1f;//通过节点改变方向时，改变Z方向的速度



    public float speed = 1f;//只影响SpeedStyle为Constant的情况


    public bool spawnAtStartPos = false;//为true的话,待移动的物体会自动初始化到StartPos,否则会慢慢移动到起始点

    [Range(0.01f, 1f)]
    public float reachPosDelta = 0.05f;//物体和目标点的距离小于该值即可认为到达了目标点



    public bool allowIdle = false;

    [HideInInspector]
    public float[] idleTimesArray;

    bool isIdling = false;

    int aimedIndex = 0;//下一个目标点的索引
    int reachedIndex = -1;//当前已经到达的目标点索引,-1用于初始化情况下，表示somewhere
    bool isReachedStartPoint = false;
    int wayPointsCount = 0;

    bool isForth = true;//对应BackAndForth模式下运动的方向，true对应0，1，2，3，4...,false对应...,4,3,2,1,0

    bool isMove = false;
    public void DoMove()
    {

        isMove = true;
        SetSpawnPos(spawnAtStartPos);

    }

    public void ResetIdleTime()
    {

    }
    void Start()
    {
        instance = this;
        if (moveObj == null)
        {
            moveObj = gameObject;
        }

        if (wayPointsParent == null)
        {
            Debug.Log("you have not assign a wayPointsParent");
            return;
        }
        else
        {
            if (wayPointsParent.transform.childCount < 2)
            {
                Debug.Log("the wayPointsParent is invalid!");
                return;
            }
        }

        wayPoints = GetWayPoints(wayPointsParent);
        wayPointsCount = wayPoints.Length;

        if (directionChangeSpeed <= 0f)
        {
            Debug.LogError("directionChangeSpeed value is wrong!");
            return;
        }
        this.GetComponent<WayPointEvent>().StartWayPointReached += MoveAlongWayPoints_StartWayPointReached;
        this.GetComponent<WayPointEvent>().EndWayPointReached += MoveAlongWayPoints_EndWayPointReached;
        this.GetComponent<WayPointEvent>().WayPointReached += MoveAlongWayPoints_WayPointReached;
    }

    void MoveAlongWayPoints_WayPointReached(GameObject obj, Vector3 pos)
    {
        Debug.Log("WayPoint Reached!!!" + obj.name + ",pos x:" + pos.x);


    }

    bool isAtEndWayPoint = false;
    void MoveAlongWayPoints_EndWayPointReached(GameObject obj, Vector3 pos)
    {
        Debug.Log("EndWayPoint Reached!!!" + obj.name + ",pos x:" + pos.x);

    }

    void MoveAlongWayPoints_StartWayPointReached(GameObject obj, Vector3 pos)
    {
        Debug.Log("StartWayPoint Reached!!!" + obj.name + ",pos x:" + pos.x);


    }


    /// <summary>
    /// 按照StartWayPoint,WayPoint_1,...,EndWayPoint的顺序来存储
    /// </summary>
    /// <returns></returns>
    public Transform[] GetWayPoints(GameObject parentObj)
    {

    }


    private void SetSpawnPos(bool spawnAtStartPos)
    {
        if (!spawnAtStartPos)
        {
            aimedIndex = 0;
        }
        else
        {
            moveObj.transform.position = wayPoints[0].transform.position;
            this.GetComponent<WayPointEvent>().StartWayPointReach(wayPoints[0].gameObject, moveObj.transform.position);
             
            aimedIndex = 1;
        }
    }

    // Update is called once per frame
    void Update()
    {
        //if (Input.GetKeyDown(KeyCode.G))
        //{
        //    DoMove();
        //}
        if (!isMove)
        {
            return;
        }


        if (aimedIndex <= wayPointsCount - 1 && CheckWhetherReachPointPos(moveObj.transform.position, wayPoints[aimedIndex].transform.position, reachPosDelta))
        {
            if (reachedIndex == 0)
            {
                this.GetComponent<WayPointEvent>().StartWayPointReach(wayPoints[aimedIndex].gameObject, moveObj.transform.position);
            }
            if (reachedIndex == wayPointsCount - 1)
            {
                this.GetComponent<WayPointEvent>().EndWayPointReach(wayPoints[aimedIndex].gameObject, moveObj.transform.position);
                isMove = false;
            }
            this.GetComponent<WayPointEvent>().WayPointReach(wayPoints[aimedIndex].gameObject, moveObj.transform.position);



            if (reachedIndex == wayPointsCount - 1)
            {
                return;
            }
            else
            {
                aimedIndex++;
            }





        }

        Vector3 moveDirection = wayPoints[aimedIndex].position - moveObj.transform.position;
        MoveToPoint(moveDirection, aimedIndex, speed);

        if (directionOriented)
        {
            //moveObj.transform.rotation = Quaternion.Euler(moveDirection);
            //moveObj.transform.rotation = Quaternion.Slerp(moveObj.transform.rotation, Quaternion.Euler(moveDirection), Time.deltaTime * 100);
            //moveObj.transform.rotation = Quaternion.Euler(0, 0, Quaternion.Euler(moveDirection).z);


            //moveObj.transform.forward = new Vector3(moveDirection.x, 0, moveDirection.z);//瞬间改变

            Vector3 value = Vector3.Slerp(moveObj.transform.forward, moveDirection, directionChangeSpeed * Time.deltaTime);
            moveObj.transform.forward = new Vector3(value.x, 0, value.z);//不改变警卫父物体的z方向的指向，因此取moveDirection在x->z平面上的投影坐标

        }


    }




    void MoveToPoint(Vector3 moveDirection, int index, float speed)
    {

        moveObj.transform.Translate(moveDirection.normalized * speed * Time.deltaTime, Space.World);

    }


    bool CheckWhetherReachPointPos(Vector3 myPos, Vector3 targetPos, float delta)
    {
        if (Vector3.Distance(myPos, targetPos) < delta)
        {
            reachedIndex = aimedIndex;
            return true;
        }
        return false;
    }


    #region curved line 辅助
    //helper array for curved paths, includes control points for waypoint array
    Vector3[] points;

    //taken and modified from
    //http://code.google.com/p/hotween/source/browse/trunk/Holoville/HOTween/Core/Path.cs
    //draws the full path
    void DrawCurvedLine(int curvedSmoothIndex)
    {

        Transform[] waypoints = GetWayPoints(moveObj);

        if (waypoints.Length < 2) return;

        points = new Vector3[waypoints.Length + 2];

        for (int i = 0; i < waypoints.Length; i++)
        {
            points[i + 1] = waypoints[i].position;
        }

        points[0] = points[1];
        points[points.Length - 1] = points[points.Length - 2];

        Vector3[] drawPs;
        Vector3 currPt;

        // Store draw points.
        int subdivisions = points.Length * curvedSmoothIndex;
        drawPs = new Vector3[subdivisions + 1];
        for (int i = 0; i <= subdivisions; ++i)
        {
            float pm = i / (float)subdivisions;
            currPt = GetPoint(pm);
            drawPs[i] = currPt;
        }
    }


    //taken from
    //http://code.google.com/p/hotween/source/browse/trunk/Holoville/HOTween/Core/Path.cs
    // Catmull-Rom spline
    // Gets the point on the curve at the given percentage (0 to 1).
    // t: The percentage (0 to 1) at which to get the point.
    private Vector3 GetPoint(float t)
    {
        int numSections = points.Length - 3;
        int tSec = (int)System.Math.Floor(t * numSections);
        int currPt = numSections - 1;
        if (currPt > tSec)
        {
            currPt = tSec;
        }
        float u = t * numSections - currPt;

        Vector3 a = points[currPt];
        Vector3 b = points[currPt + 1];
        Vector3 c = points[currPt + 2];
        Vector3 d = points[currPt + 3];

        return .5f * (
                       (-a + 3f * b - 3f * c + d) * (u * u * u)
                       + (2f * a - 5f * b + 4f * c - d) * (u * u)
                       + (-a + c) * u
                       + 2f * b
                   );
    }
    #endregion

    */
}
