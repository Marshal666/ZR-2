using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditorCell : EditorObject
{

    /// <summary>
    /// Index of cell in array
    /// </summary>
    public int index = 0;

    /// <summary>
    /// Box collider of this cell
    /// </summary>
    new public BoxCollider collider;

    private void Awake()
    {
        collider = gameObject.AddComponent<BoxCollider>();
        gameObject.layer = GameEditor.main.CellEditorLayerIndex;
        FitToContent();

        //print(collider.bounds);

    }

    /// <summary>
    /// Fits box collider to include all cells children
    /// </summary>
    public void FitToContent()
    {
        if (!collider)
            collider = gameObject.AddComponent<BoxCollider>();

        //reset collider
        collider.size = Vector3.one;
        collider.center = Vector3.zero;

        Collider[] coll = gameObject.GetComponentsInChildren<Collider>(false);

        //print(string.Join(", ", (IEnumerable)coll));

        Bounds b = new Bounds(transform.position, Vector3.zero);

        for(int i = 0; i < coll.Length; i++)
        {
            Bounds cb = coll[i].bounds;
            cb.center = coll[i].transform.position;
            //print(b + " + " + cb + " n: " + coll[i].name);
            b.Encapsulate(cb);
            //print("res: " + b);
        }

        collider.center = b.center - transform.position;
        collider.size = b.size;

    }

}
