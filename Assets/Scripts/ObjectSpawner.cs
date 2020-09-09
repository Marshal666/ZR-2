using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ObjectSpawner
{

    public enum SpawnerType
    {
        ConstantSize,   //never adds new objects
        Expandable      //adds new objects if neccesary
    }

    GameObject PrefabObject;

    Transform HolderParent;

    int Count = 16;

    List<GameObject> Objects;

    int currentIndex;

    SpawnerType Type;

    public ObjectSpawner(GameObject prefabObject, Transform holderParent, int count = 16, SpawnerType spawnerType = SpawnerType.Expandable)
    {

        if (!prefabObject)
            throw new NullReferenceException("Given prefab to clone is null.");
        PrefabObject = prefabObject;

        if (!holderParent)
            throw new NullReferenceException("Holder parent is null.");
        HolderParent = holderParent;

        if (count < 0)
            throw new ArgumentOutOfRangeException("Given count is less than zero.");
        Count = count;

        Objects = new List<GameObject>(Count);

        for(int i = 0; i < Count; i++)
        {
            GameObject o = UnityEngine.Object.Instantiate(PrefabObject);
            Objects.Add(o);
            o.SetActive(false);
            o.transform.SetParent(HolderParent);
        }

        currentIndex = 0;

        Type = spawnerType;

    }

    public GameObject GetObject()
    {
        GameObject ret = null;
        if(currentIndex >= Objects.Count)
        {
            switch (Type)
            {
                case SpawnerType.ConstantSize:
                    currentIndex = 0;
                    break;
                case SpawnerType.Expandable:
                    GameObject o = UnityEngine.Object.Instantiate(PrefabObject);
                    Objects.Add(o);
                    o.SetActive(false);
                    o.transform.SetParent(HolderParent);
                    ret = o;
                    break;
                default:
                    break;
            }
        } else
        {
            ret = Objects[currentIndex++];
        }
        ret.SetActive(true);
        return ret;
    }

    public void ReturnObject(GameObject o)
    {
        for(int i = 0; i < Objects.Count && i < currentIndex; i++)
        {
            if(Objects[i] == o)
            {
                if(currentIndex > 0)
                {
                    currentIndex--;
                }
                o.SetActive(false);
                GameObject t = Objects[currentIndex];
                Objects[currentIndex] = o;
                Objects[i] = t;
            }
        }
    }

    public void ReturnAll()
    {
        for(int i = 0; i < Objects.Count; i++)
        {
            Objects[i].SetActive(false);
        }
        currentIndex = 0;
    }

}
