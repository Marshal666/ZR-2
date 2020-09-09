using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IWorldRenderer
{

    void RenderPositionChanges();

    float BuildingDistance { get; set; }

}
