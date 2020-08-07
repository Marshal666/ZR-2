using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    public float lookAtSmoothSpeed = 1f;

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
    StateMachine<PlayerCameraStates> sm;

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

    #region STATE_METHODS

    //State methods...

    void FollowPlayer()
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

        distance += InputMapper.main.CameraZoom * zoomSpeed * Time.deltaTime;
        distance = Mathf.Clamp(distance, distanceMin, distanceMax);

        Vector3 targetPos = playerTransform.position + lookAtPlayerDirections[lookDirectionIndex] * (followPlayerOffset.normalized * distance);

        //uses line as path
        transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref velocitySmooth, moveSmoothTime);

        if(lookAtPlayer)
        {
            Quaternion rot = Quaternion.LookRotation(playerTransform.position - targetPos);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, rot, lookAtSmoothSpeed * Time.deltaTime);
        }

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
            transform.rotation = Quaternion.RotateTowards(transform.rotation, rot, lookAtSmoothSpeed * Time.deltaTime);
        }

    }

    #endregion

    /// <summary>
    /// Script init
    /// </summary>
    void Awake()
    {

        //init state machine
        sm = new StateMachine<PlayerCameraStates>(PlayerCameraStates.Idle);
        sm.Methods[PlayerCameraStates.FollowPlayer] = FollowPlayer;
        sm.Methods[PlayerCameraStates.LookAtWorld] = LookAtWorld;

        //find player transfrom if needed
        if(!playerTransform)
        {
            playerTransform = FindObjectOfType<Player>().transform;
        }

    }

    // Update is called once per frame
    void LateUpdate()
    {
        sm.Execute();
    }

    /*
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Vector3[] dots = new Vector3[100];
        for(int i = 0; i < dots.Length; i++)
        {
            dots[i] = VectorCurveLerp(Vector3.zero, new Vector3(1, 0, 1), offsetPathCurve, (float)i / dots.Length);
            if(i > 0)
            {
                Gizmos.DrawLine(dots[i - 1], dots[i]);
            }
        }
    }*/
}
