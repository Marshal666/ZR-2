using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{

    public Transform playerTransform;

    public Vector3 followPlayerOffset = new Vector3(0, 2, -5);

    public bool lookAtPlayer = true;

    public float moveSmoothTime = 2f, rotationMoveSmoothTime = 0.5f;

    public float lookAtSmoothSpeed = 1f;

    public Vector3 lookAtWorldOffset = new Vector3(0, 5, -10);

    public bool lookAtWorld = true;

    Vector3 velocitySmooth;

    static readonly Vector3[] lookAtPlayerVectorDirections = {  new Vector3(0, 0, 1), 
                                                                new Vector3(-1, 0, 0), 
                                                                new Vector3(0, 0, -1), 
                                                                new Vector3(1, 0, 0) };

    static readonly Quaternion[] lookAtPlayerDirections = new Quaternion[4] {   Quaternion.LookRotation(lookAtPlayerVectorDirections[0]),
                                                                                Quaternion.LookRotation(lookAtPlayerVectorDirections[1]),
                                                                                Quaternion.LookRotation(lookAtPlayerVectorDirections[2]),
                                                                                Quaternion.LookRotation(lookAtPlayerVectorDirections[3]) };

    public int lookDirectionIndex = 0;

    public enum PlayerCameraStates
    {
        Idle,
        FollowPlayer,
        LookAtWorld
    }

    StateMachine<PlayerCameraStates> sm;

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

        Vector3 targetPos = playerTransform.position + lookAtPlayerDirections[lookDirectionIndex] * followPlayerOffset;

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

        if (InputMapper.main.CameraRotateLeft)
        {
            lookDirectionIndex = lookDirectionIndex - 1 < 0 ? lookAtPlayerVectorDirections.Length - 1 : lookDirectionIndex - 1;
        }

        if (InputMapper.main.CameraRotateRight)
        {
            lookDirectionIndex = (lookDirectionIndex + 1) % lookAtPlayerVectorDirections.Length;
        }

        Vector3 targetPos = World.main.WorldCenter + lookAtPlayerDirections[lookDirectionIndex] * lookAtWorldOffset;

        transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref velocitySmooth, moveSmoothTime);

        if (lookAtWorld)
        {
            Quaternion rot = Quaternion.LookRotation(World.main.WorldCenter - targetPos);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, rot, lookAtSmoothSpeed * Time.deltaTime);
        }

    }

    #endregion

    void Awake()
    {

        //init state machine
        sm = new StateMachine<PlayerCameraStates>(PlayerCameraStates.FollowPlayer);
        sm.Methods[PlayerCameraStates.FollowPlayer] = FollowPlayer;
        sm.Methods[PlayerCameraStates.LookAtWorld] = LookAtWorld;

        //find player transfrom if needed
        if(!playerTransform)
        {
            playerTransform = FindObjectOfType<Player>().transform;
        }

    }

    private void Start()
    {

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
