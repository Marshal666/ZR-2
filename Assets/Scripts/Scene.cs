using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scene : MonoBehaviour
{

    Transform rootTransform;

    public GameObject root;

    public static Scene main;

    public static GameObject Root { get { return main.root; } }

    public static Transform RootTransform { get { return main.rootTransform; } }

    public GameObject player;

    Player playerC;

    public static GameObject PlayerObject { get { return main.player; } }

    public static Player Player { get { return main.playerC; } }

    EventSystem es;

    public static EventSystem EventSystem { get { return main.es; } }

    private void Awake()
    {
        main = this;

        if (!root)
        {
            root = GameObject.Find("Root");
            if(!root)
            {
                root = new GameObject("Root");
            }
        }

        rootTransform = root.transform;

        if(!player)
        {
            player = GameObject.Find("Player");
            if(!player)
            {
#if UNITY_EDITOR
                Debug.LogError("Player does not exist");
#endif
            }
        }

        playerC = player.GetComponent<Player>();

        es = new EventSystem();

    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(InputMapper.main.Undo)
        {
            es.Undo();
        }

        if(InputMapper.main.Redo)
        {
            es.Redo();
        }
    }

    public static void ClearRoot()
    {
        if(RootTransform)
        {
            for(int i = RootTransform.childCount - 1; i >= 0; i--)
            {
                Destroy(RootTransform.GetChild(i).gameObject);
            }
        }
    }

}
