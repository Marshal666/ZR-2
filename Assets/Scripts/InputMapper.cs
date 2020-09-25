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

    public KeyCode PauseKey = KeyCode.Escape;

    public KeyCode HoverKey1 = KeyCode.LeftShift, HoverKey2 = KeyCode.Mouse2;

    public KeyCode CellSelectKey = KeyCode.Mouse0;

    public KeyCode CellIncreaseKey = KeyCode.Mouse0, CellActionKey = KeyCode.LeftShift, CellDecreaseKey = KeyCode.Mouse1;

    public bool InvertMouseScroll = true;

    public static InputMapper main;

    public int[] moveDimension;

    public int[] MoveDimension {  get { return main.moveDimension; } }

    public bool Undo;

    public bool Redo;

    public bool CameraRotateLeft, CameraRotateRight;

    public bool Pause;

    public float CameraZoom = 0f;

    public bool EditorHover;
    public Vector3 EditorHoverValue;
    public Vector3 EditorHoverValueOld;

    public bool CellSelect, CellDeselect;

    public bool CellIncrease, CellDecrease;

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

        }

        Undo = Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Z) || Input.GetKeyDown(UndoKey);
        Redo = Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.Y) || Input.GetKeyDown(RedoKey);

        CameraRotateLeft = Input.GetKeyDown(CameraRotateLeftKey);
        CameraRotateRight = Input.GetKeyDown(CameraRotateRightKey);

        Pause = Input.GetKeyDown(PauseKey);

        CameraZoom = Input.mouseScrollDelta.y * (InvertMouseScroll ? -1 : 1);

        EditorHover = Input.GetKey(HoverKey1) & Input.GetKey(HoverKey2);

        CellSelect = Input.GetKeyDown(CellSelectKey);

        CellDeselect = Input.GetKeyDown(CellDecreaseKey);

        CellIncrease = Input.GetKeyDown(CellIncreaseKey) & Input.GetKey(CellActionKey);
        CellDecrease = Input.GetKeyDown(CellDecreaseKey) & Input.GetKey(CellActionKey);

        if(EditorHover)
        {
            //Can't hover in 1st frame of it because it causes big delta jumps
            if (EditorHoverValueOld != Vector3.zero)
            {
                EditorHoverValue = EditorHoverValueOld - Input.mousePosition;
            }
            EditorHoverValueOld = Input.mousePosition;
        } else
        {
            EditorHoverValue = Vector3.zero;
            EditorHoverValueOld = EditorHoverValue;
        }

    }
}
