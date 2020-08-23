using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngineInternal;

/// <summary>
/// Player camera managment class
/// </summary>
public class PlayerCamera : MonoBehaviour
{

    /// <summary>
    /// Reference to players transform compnent
    /// </summary>
    public Transform playerTransform;

    /// <summary>
    /// Offset from player - used for getting direction
    /// </summary>
    public Vector3 followPlayerOffset = new Vector3(0, 2, -5);

    /// <summary>
    /// Current distance from player
    /// </summary>
    public float distance = 5f;

    /// <summary>
    /// Min and Max possible distances from player
    /// </summary>
    public float distanceMin = 3f, distanceMax = 10f;

    /// <summary>
    /// Speed of changing distance from player
    /// </summary>
    public float zoomSpeed = 8f;

    /// <summary>
    /// Should camera look at player?
    /// </summary>
    public bool lookAtPlayer = true;

    /// <summary>
    /// Move and rotation smooth times
    /// </summary>
    public float moveSmoothTime = 2f, rotationMoveSmoothTime = 0.5f;

    /// <summary>
    /// speed of lookAt
    /// </summary>
    public float lookAtSmoothTime = 0.1f;

    /// <summary>
    /// Offset from central world point when looking at it
    /// </summary>
    public Vector3 lookAtWorldOffset = new Vector3(0, 5, -10);

    /// <summary>
    /// Should camera look at world?
    /// </summary>
    public bool lookAtWorld = true;

    /// <summary>
    /// Used in SmoothDamp()
    /// </summary>
    Vector3 velocitySmooth;

    /// <summary>
    /// Used in SmoothDamp()
    /// </summary>
    Vector3 rotationSmooth;

    /// <summary>
    /// For easier getting in scripts
    /// </summary>
    public static PlayerCamera main;

    /// <summary>
    /// Vector directions of looking at player 
    /// </summary>
    static readonly Vector3[] lookAtPlayerVectorDirections = {  new Vector3(0, 0, 1),       //to forward
                                                                new Vector3(-1, 0, 0),      //to left
                                                                new Vector3(0, 0, -1),      //to back
                                                                new Vector3(1, 0, 0) };     //to right

    /// <summary>
    /// Rotations based on vector directions of looking at player
    /// </summary>
    static readonly Quaternion[] lookAtPlayerDirections = new Quaternion[4] {   Quaternion.LookRotation(lookAtPlayerVectorDirections[0]),
                                                                                Quaternion.LookRotation(lookAtPlayerVectorDirections[1]),
                                                                                Quaternion.LookRotation(lookAtPlayerVectorDirections[2]),
                                                                                Quaternion.LookRotation(lookAtPlayerVectorDirections[3]) };

    /// <summary>
    /// Which direction is player currently being looked at?
    /// </summary>
    public int lookDirectionIndex = 0;

    /// <summary>
    /// Ways of player looking at camera
    /// </summary>
    public enum PlayerCameraStates
    {
        Idle,
        FollowPlayer,
        LookAtWorld
    }

    /// <summary>
    /// State machine to process ways of looking at player
    /// </summary>
    public StateMachine<PlayerCameraStates> CameraState;

    /// <summary>
    /// Position camera is set to when component resets
    /// </summary>
    public Vector3 defaultPosition;

    /// <summary>
    /// Rotation camera is set to when component resets
    /// </summary>
    public Vector3 defaultEulerAngles;

    /// <summary>
    /// Layers of cells used for raycasting
    /// </summary>
    public LayerMask CellLayers;

    /// <summary>
    /// Returns point from start to end, X and Y are lerped while Z is evaluated from curve
    /// </summary>
    /// <param name="start">start point</param>
    /// <param name="end">end point</param>
    /// <param name="curve">curve on Z axis between points</param>
    /// <param name="t">value from 0 to 1</param>
    /// <returns>Point on calculated curve on t</returns>
    Vector3 VectorCurveLerp(Vector3 start, Vector3 end, AnimationCurve curve, float t)
    {
        Vector3 res = new Vector3();

        res.x = Mathf.Lerp(start.x, end.x, t);

        res.y = Mathf.Lerp(start.y, end.y, t);

        float size = Mathf.Abs(start.z - end.z);

        res.z = start.z + curve.Evaluate(t) * size;

        return res;
    }


    /// <summary>
    /// Cell that is under a raycast
    /// </summary>
    Cell currentCell = null;

    void CheckRaycast()
    {
        const float raycastDistance = 1000f;

        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        //Debug.DrawRay(ray.origin, ray.direction * raycastDistance, Color.red);

        if(Physics.Raycast(ray, out hit, raycastDistance, CellLayers))
        {
            //search for parent cell
            //limit levels of going up to 4
            int cutoff = 4;
            Cell cell = null;
            Transform t = hit.transform.parent;
            while(cutoff > 0 && cell == null)
            {
                if (!t)
                    break;
                Cell c = t.GetComponent<Cell>();
                if (c)
                    cell = c;
                t = t.parent;
                cutoff--;
            }

            if(cell)
            {
                if(cell != currentCell)
                {
                    if (currentCell != null)
                        currentCell.RemovePreviewChanges?.Invoke();
                }
                if(cell)
                {
                    cell.PreviewChanges?.Invoke();
                }
                currentCell = cell;
            }

        } else
        {

            if(currentCell != null)
            {
                currentCell.RemovePreviewChanges?.Invoke();
                currentCell = null;
            }

        }

    }

    #region STATE_METHODS

    //State methods...

    //FIX: probably needs a rework
    void FollowPlayer()
    {

        if (InputMapper.main.CameraRotateLeft)
        {
            lookDirectionIndex = lookDirectionIndex - 1 < 0 ? lookAtPlayerVectorDirections.Length - 1 : lookDirectionIndex - 1;
        }

        if (InputMapper.main.CameraRotateRight)
        {
            lookDirectionIndex = (lookDirectionIndex + 1) % lookAtPlayerVectorDirections.Length;
        }

        distance += InputMapper.main.CameraZoom * zoomSpeed * Time.deltaTime;
        distance = Mathf.Clamp(distance, distanceMin, distanceMax);

        Vector3 targetPos = playerTransform.position + lookAtPlayerDirections[lookDirectionIndex] * (followPlayerOffset.normalized * distance);

        Vector3 newPos = Vector3.SmoothDamp(transform.position, targetPos, ref velocitySmooth, moveSmoothTime);

        /*
        float h = Mathf.Abs(targetPos.y - playerTransform.position.y);

        
        //limit XZ distance to sphereically curve the path
        float sphDistXZ = Mathf.Sqrt(distance * distance - h * h);
        print(sphDistXZ);

        Vector3 plXZ = new Vector3(playerTransform.position.x, 0, playerTransform.position.z);
        Vector3 npXZ = new Vector3(newPos.x, 0, newPos.z);

        float dXZ = Vector3.Distance(plXZ, npXZ);

        if(dXZ != sphDistXZ)
        {
            npXZ = npXZ.normalized * sphDistXZ;
        }

        npXZ.y = newPos.y;
        newPos = npXZ;*/

        //uses line as path
        transform.position = newPos;

        if(lookAtPlayer)
        {
            Quaternion rot = Quaternion.LookRotation(playerTransform.position - transform.position);
            Vector3 angles = rot.eulerAngles;
            //transform.rotation = Quaternion.RotateTowards(transform.rotation, rot, lookAtSmoothSpeed * Time.deltaTime);
            transform.eulerAngles = smoothDampAngles(transform.eulerAngles, angles, ref rotationSmooth, lookAtSmoothTime);
            //transform.rotation = rot;

            Vector3 smoothDampAngles(Vector3 s, Vector3 e, ref Vector3 v, float t)
            {
                return new Vector3( Mathf.SmoothDampAngle(s.x, e.x, ref v.x, t),
                                    Mathf.SmoothDampAngle(s.y, e.y, ref v.y, t),
                                    Mathf.SmoothDampAngle(s.z, e.z, ref v.z, t));
            }

        }

        CheckRaycast();

    }

    void LookAtWorld()
    {

        //TODO: make path from position to targetPos curvy/sphereical instead of a line

        if (InputMapper.main.CameraRotateLeft)
        {
            lookDirectionIndex = lookDirectionIndex - 1 < 0 ? lookAtPlayerVectorDirections.Length - 1 : lookDirectionIndex - 1;
        }

        if (InputMapper.main.CameraRotateRight)
        {
            lookDirectionIndex = (lookDirectionIndex + 1) % lookAtPlayerVectorDirections.Length;
        }

        Vector3 targetPos = World.main.WorldCenter + lookAtPlayerDirections[lookDirectionIndex] * lookAtWorldOffset;

        //same problem
        transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref velocitySmooth, moveSmoothTime);

        if (lookAtWorld)
        {
            Quaternion rot = Quaternion.LookRotation(World.main.WorldCenter - targetPos);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, rot, lookAtSmoothTime * Time.deltaTime);
        }

        CheckRaycast();

    }

    #endregion

    /// <summary>
    /// Script init
    /// </summary>
    void Awake()
    {

        main = this;

        //init state machine
        CameraState = new StateMachine<PlayerCameraStates>(PlayerCameraStates.Idle);
        CameraState.Methods[PlayerCameraStates.FollowPlayer] = FollowPlayer;
        CameraState.Methods[PlayerCameraStates.LookAtWorld] = LookAtWorld;

        //find player transfrom if needed
        if(!playerTransform)
        {
            playerTransform = FindObjectOfType<Player>().transform;
        }

    }

    // Update is called once per frame
    void LateUpdate()
    {
        CameraState.Execute();

        if(pts.Count < ptsc)
        {
            pts.Add(transform.position);
        } else
        {
            pts[ptsi] = transform.position;
        }
        ptsi = (ptsi + 1) % ptsc;
    }

    public void ResetToDefault()
    {
        if (CameraState.State != PlayerCameraStates.Idle)
            CameraState.SwitchState(PlayerCameraStates.Idle);
        transform.position = defaultPosition;
        transform.eulerAngles = defaultEulerAngles;
    }


    List<Vector3> pts = new List<Vector3>();
    int ptsc = 500, ptsi = 0;

    
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        for(int i = 1; i < pts.Count; i++)
        {
            if (i == ptsi)
                continue;
            Gizmos.DrawLine(pts[i - 1], pts[i]);
        }
    }
}
