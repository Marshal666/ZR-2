using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class RoadGenerator
{

    static Vector2[] quadCenterUVs = new Vector2[] { };

    public static Mesh GenerateRoadGrid(int width, int height)
    {
        Mesh mesh = new Mesh();

        List<Vector3> verts = new List<Vector3>();
        int vertsoffset = 0;

        List<Vector2> uvs = new List<Vector2>();

        List<int> tris = new List<int>();

        

        //Make road mesh

        makeQuad(Vector3.zero, Vector3.right, Vector3.up + Vector3.right, Vector3.up, ref quadCenterUVs);


        mesh.SetVertices(verts);
        mesh.SetTriangles(tris, 0);
        mesh.SetUVs(0, uvs);

        mesh.RecalculateNormals();

        return mesh;

        void makeQuad(Vector3 a, Vector3 b, Vector3 c, Vector3 d, ref Vector2[] uvs_)
        {
            verts.Add(a);   //A 6
            verts.Add(b);   //B1 5
            verts.Add(b);   //B2 4
            verts.Add(c);   //C 3
            verts.Add(d);   //D1 2
            verts.Add(d);   //D2 1
            vertsoffset += 6;

            //triangle AB1D1
            tris.Add(vertsoffset - 6);
            tris.Add(vertsoffset - 2);
            tris.Add(vertsoffset - 5);

            //uvs
            uvs.Add(uvs_[0]);
            uvs.Add(uvs_[1]);
            uvs.Add(uvs_[4]);
            
            //triangle CB2D2
            tris.Add(vertsoffset - 3);
            tris.Add(vertsoffset - 4);
            tris.Add(vertsoffset - 1);

            //uvs
            uvs.Add(uvs_[3]);
            uvs.Add(uvs_[2]);
            uvs.Add(uvs_[5]);

        }

    }

}
