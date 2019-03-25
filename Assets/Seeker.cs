using UnityEngine;
using System.Collections;
using iningwei.AStar;
using System;
using System.Collections.Generic;

public class Seeker : MonoBehaviour
{
    public Action<Path> OnGetPath;
    public AStar aStar;
    public bool isShowSeekerPath = true;

    public int smoothDegree = 2;
    Path seekerPath;

    void Start()
    {
        aStar.OnGetPath += _onGetPath;
        Path.OnWayPointReached += OnWayPointReached;
    }

    private void OnWayPointReached(int reachedIndex)
    {
        Debug.Log("到达点：" + reachedIndex);

    }

    public void SearchPath(Vector3 startPos, Vector3 endPos)
    {
        if (aStar.OnSearchPath != null)
        {
            aStar.OnSearchPath(startPos, endPos);
        }
    }

    private void _onGetPath(Path path)
    {
        if (smoothDegree > 0)
        {
            seekerPath = path;
            Debug.LogError("原始点数：" + path.vetexList.Count);

            seekerPath.vetexList = _getCurvedPoints(smoothDegree, seekerPath.vetexList);
            Debug.LogError("差值平滑后点数：" + path.vetexList.Count);
        }
        else
        {
            seekerPath = path;
        }
        if (OnGetPath != null)
        {
            OnGetPath(seekerPath);
        }

    }

    void OnDisable()
    {
        aStar.OnGetPath -= _onGetPath;
    }

    #region curved line 辅助

    //helper array for curved paths, includes control points for waypoint array
    Vector3[] points;


    //taken and modified from
    //http://code.google.com/p/hotween/source/browse/trunk/Holoville/HOTween/Core/Path.cs
    //draws the full path
    List<Vector3> _getCurvedPoints(int smoothDegree, List<Vector3> originWayPoints)
    {
        if (originWayPoints.Count < 2) return originWayPoints;

        points = new Vector3[originWayPoints.Count + 2];

        for (int i = 0; i < originWayPoints.Count; i++)
        {
            points[i + 1] = originWayPoints[i];
        }

        points[0] = points[1];
        points[points.Length - 1] = points[points.Length - 2];

        Vector3 currPt;

        // Store curvedPoints
        int subdivisions = points.Length * smoothDegree;
        List<Vector3> curvedPoints = new List<Vector3>();
        for (int i = 0; i <= subdivisions; ++i)
        {
            float pm = i / (float)subdivisions;
            currPt = GetPoint(pm);
            curvedPoints.Add(currPt);
        }

        return curvedPoints;
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

    #region gizmos
    public void OnDrawGizmos()
    {
        if (!isShowSeekerPath)
        {
            return;
        }
        if (seekerPath != null)
        {
            Gizmos.color = Color.green;
            for (int i = 0; i < seekerPath.vetexList.Count - 1; i++)
            {
                Gizmos.DrawLine(seekerPath.vetexList[i], seekerPath.vetexList[i + 1]);
            }
        }

    }
    #endregion

}
