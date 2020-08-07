using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Maps input
/// </summary>
[DefaultExecutionOrder(-1)]
public class InputMapper : MonoBehaviour
{

    [System.Serializable]
    public struct DimensionInputs
    {
        public KeyCode positive;
        public KeyCode negative;
    }

    public DimensionInputs[] dimensionInputs;

    public KeyCode UndoKey = KeyCode.B, RedoKey = KeyCode.N;

    public KeyCode CameraRotateLeftKey = KeyCode.LeftArrow, CameraRotateRightKey = KeyCode.RightArrow;

    public bool InvertMouseScroll = true;

    public static InputMapper main;

    public int[] moveDimension;

    public int[] MoveDimension {  get { return main.moveDimension; } }

    public bool Undo;

    public bool Redo;

    public bool CameraRotateLeft, CameraRotateRight;

    public float CameraZoom = 0f;

    private void Awake()
    {
        main = this;

        moveDimension = new int[dimensionInputs != null ? dimensionInputs.Length : 0];
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < dimensionInputs.Length; i++)
        {
            if (Input.GetKeyDown(dimensionInputs[i].positive))
            {
                moveDimension[i] = 1;
            }
            else if (Input.GetKeyDown(dimensionInputs[i].negative))
            {
                moveDimension[i] = -1;
            } else
            {
                moveDimension[i] = 0;
            }

            Undo = Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(UndoKey);

            Redo = Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Y) || Input.GetKeyDown(RedoKey);

        }

        CameraRotateLeft = Input.GetKeyDown(CameraRotateLeftKey);
        CameraRotateRight = Input.GetKeyDown(CameraRotateRightKey);

        CameraZoom = Input.mouseScrollDelta.y * (InvertMouseScroll ? -1 : 1);

    }
}
