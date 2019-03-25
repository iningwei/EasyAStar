using UnityEngine;
using System.Collections;
using iningwei.AStar;

public class MoveControl : MonoBehaviour
{
    public Vector3 targetPosition;

    private Seeker seeker;
    private CharacterController controller;
    public Path path;
    public float moveSpeed = 1f;
    public float nextWaypointDistance = 0.01f;

    private int nextWaypointIndex = 0;
    public float firstTurnSpeed = 100f;
    public float normalTurnSpeed = 10f;

    void Start()
    {
        seeker = GetComponent<Seeker>();
        controller = GetComponent<CharacterController>();
        seeker.OnGetPath += OnGetPath;
    }

    Vector3 downPoint;
    Vector3 upPoint;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            downPoint = Input.mousePosition;
        }

        if (Input.GetMouseButtonUp(0))
        {
            upPoint = Input.mousePosition;
            if (_checkMove(downPoint, upPoint) == false)
            {
                RaycastHit raycastHit;
                if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out raycastHit, 2000, 1 << LayerMask.NameToLayer("Walkable")))
                {
                    targetPosition = raycastHit.point;
                    seeker.SearchPath(transform.position, targetPosition);
                }
            }
        }
    }


    bool _checkMove(Vector3 downPos, Vector3 upPos)
    {
        bool isMove = false;
        if (Mathf.Abs(upPos.x - downPos.x) > 1f || Mathf.Abs(upPos.y - downPos.y) > 1f)
        {
            isMove = true;
        }
        return isMove;
    }


    public Vector3 dir;
    void FixedUpdate()
    {
        if (path == null)
        {
            return;
        }
        if (nextWaypointIndex >= path.vetexList.Count)
        {
            path = null;
            Debug.Log("EndOfPathReached");
            return;
        }

        dir = path.vetexList[nextWaypointIndex] - transform.position;
        transform.Translate(dir.normalized * moveSpeed * Time.fixedDeltaTime, Space.World);

        if (dir != Vector3.zero)//加上这句，防止报警告：Look rotation viewing vector is zero//https://forum.unity3d.com/threads/simple-fix-for-look-rotation-viewing-vector-is-zero.411731/
        {
            transform.forward = Vector3.Slerp(transform.forward, dir, (nextWaypointIndex == 0 ? firstTurnSpeed : normalTurnSpeed) * Time.fixedDeltaTime);
        }

        if (Vector3.Distance(transform.position, path.vetexList[nextWaypointIndex]) < nextWaypointDistance)
        {
            Path.OnWayPointReached(nextWaypointIndex);
            nextWaypointIndex++;
        }
    }


    public void OnGetPath(Path p)
    {
        path = p;
        nextWaypointIndex = 0;
    }

    void OnDisable()
    {
        seeker.OnGetPath -= OnGetPath;
    }
}
