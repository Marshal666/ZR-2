using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Player editor component, used only in raycasting
/// </summary>
public class PlayerEditor : EditorObject
{

    public Outline outline;

    private void Awake()
    {
        if(!outline)
        {
            outline = GetComponentInChildren<Outline>();
        }

        if(outline)
        {
            outline.enabled = false;
        }
    }

    public void EnableOutline()
    {
        outline.enabled = true;
    }

    public void DisableOutline()
    {
        outline.enabled = false;
    }

}
