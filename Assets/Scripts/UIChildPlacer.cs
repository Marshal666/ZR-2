using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//[ExecuteInEditMode]
public class UIChildPlacer : MonoBehaviour
{

    public bool AutoRedraw = false;

    public Vector2 ChildMinPadding;
    public Vector2 ChildMaxPadding;

    public void Redraw()
    {
        int count = transform.childCount;
        int che = count;
        for(int i = 0; i < count; i++)
        {
            Transform ch = transform.GetChild(i);
            if(ch is RectTransform t)
            {
                if(ch.gameObject.activeInHierarchy)
                {
                    t.anchorMin = new Vector2(0f, (float)(che - 1) / count);
                    t.anchorMax = new Vector2(1f, (float)che / count);
                    t.offsetMin = ChildMinPadding;
                    t.offsetMax = -ChildMaxPadding;
                    che--;
                    //print("h");
                }
            } else
            {
                break;
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(AutoRedraw)
        {
            Redraw();
        }
    }
}
