using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Class for generating road grids
/// </summary>
public static class RoadGenerator
{

    //UV map values for road parts

    /// <summary>
    /// Center part UVs
    /// </summary>
    static Vector2[] quadCenterUVs = new Vector2[] {    new Vector2(0.506584f, 0.006584f), new Vector2(0.993416f, 0.006584f),
                                                        new Vector2(0.993416f, 0.493416f), new Vector2(0.506584f, 0.493416f) };

    /// <summary>
    /// Road UVs
    /// </summary>
    static Vector2[] quadRoadUVs = new Vector2[] {      new Vector2(0.50625f, 0.507027f), new Vector2(0.99375f, 0.507027f),
                                                        new Vector2(0.99375f, 0.99375f), new Vector2(0.50625f, 0.99375f) };

    /// <summary>
    /// Crossing UVs
    /// </summary>
    static Vector2[] quadCrossingUVs = new Vector2[] {  new Vector2(0.00625f, 0.50625f), new Vector2(0.49375f, 0.50625f),
                                                        new Vector2(0.49375f, 0.99375f), new Vector2(0.00625f, 0.99375f) };

    /// <summary>
    /// Sideroad UVs
    /// </summary>
    static Vector2[] quadSideroadUVs = new Vector2[] {  new Vector2(0.005775f, 0.005775f), new Vector2(0.494225f, 0.005775f),
                                                        new Vector2(0.494225f, 0.494225f), new Vector2(0.005775f, 0.494225f) };



    /// <summary>
    /// Scaled basic vectors using distance parameter
    /// </summary>
    static Vector3 left, right, forward, back;

    /// <summary>
    /// Rotations for road directions
    /// </summary>
    static Quaternion   leftRotation = Quaternion.LookRotation(Vector3.left),
                        rightRotation = Quaternion.LookRotation(Vector3.right),
                        forwardRotation = Quaternion.LookRotation(Vector3.forward),
                        backRotation = Quaternion.LookRotation(Vector3.back);

    /// <summary>
    /// Generates a new road grid object(s)
    /// </summary>
    /// <param name="width">Grid width</param>
    /// <param name="height">Grid height</param>
    /// <param name="distance">Road length</param>
    /// <param name="depth">Road height</param>
    /// <param name="roadCount">Number of roads between crossings</param>
    /// <returns>Reference to a new game object holding road grid object(s)</returns>
    public static GameObject MakeRoad(int width, int height, float distance, float depth, int roadCount)
    {

        //create base road object
        GameObject road = new GameObject("Road");
        Transform roadTransform = road.transform;
        //every dynamic object should be in Root
        roadTransform.SetParent(Scene.RootTransform);

        //no point im making 0 grids
        if (width <= 0 || height <= 0)
            return road;

        //for tracking the process of made grids
        bool[,] grids = new bool[width, height];
        bool done = false;

        //calculate stuff for generation of road
        left = Vector3.left * distance;
        right = Vector3.right * distance;
        forward = Vector3.forward * distance;
        back = Vector3.back * distance;

        //id of road part object(s)
        int p = 0;

        //current position in grid
        (int x, int y) pos = (0, 0);

        //generate grids
        while(!done)
        {

            //road may be split in separate objects if its vertex count is larger than ushort.MaxValue (65535)
            GameObject g = new GameObject("RoadPart" + p);
            g.transform.SetParent(roadTransform);

            //if GenerateRoadGrids returns false, then mesh has nearly the maximum possible amount of verts
            done |= GenerateRoadGrids(ref grids, ref pos, out Mesh mesh, distance, depth, roadCount);

            //assign mesh and material to road grid object
            g.AddComponent<MeshFilter>().mesh = mesh;

            g.AddComponent<MeshRenderer>().material = GameData.RoadMaterial;

            //position object? - not needed since mesh has required offset

            p++;

        }

        return road;

    }

    /// <summary>
    /// Converts pos to actual world position
    /// </summary>
    /// <param name="pos">Current position in world cords</param>
    /// <param name="distance">Distance param</param>
    /// <param name="roadCount">Road count param</param>
    /// <returns></returns>
    static Vector3 gridCordsToWorldCords((int x, int y) pos, float distance, int roadCount)
    {
        float offset = (3f * distance + 2f * roadCount * distance) * 2f;
        Vector3 res = new Vector3(pos.x * offset, 0f, pos.y * offset);
        return res;
    }

    /// <summary>
    /// Makes road grid mesh for one object
    /// </summary>
    /// <param name="grids">Grid array for checking progress</param>
    /// <param name="pos">Position in grid</param>
    /// <param name="mesh">Returns generated mesh</param>
    /// <param name="distance">Distance param</param>
    /// <param name="depth">Depth param</param>
    /// <param name="roadCount">Road count param</param>
    /// <returns>true if whole grid was made, false if only part of it was made</returns>
    static bool GenerateRoadGrids(ref bool[,] grids, ref (int x, int y) pos, out Mesh mesh, float distance, float depth, int roadCount)
    {

        //create mesh and required lists
        mesh = new Mesh();

        List<Vector3> verts = new List<Vector3>();

        //track of how many vertices mesh has
        int vertsoffset = 0;

        List<Vector2> uvs = new List<Vector2>();

        List<int> tris = new List<int>();

        bool success = true;

        //Make road mesh

        int i = pos.x, j = pos.y;

        for(; i < grids.GetLength(0); i++)
        {
            pos.x = i;
            for (; j < grids.GetLength(1); j++)
            {
                pos.y = j;
                success &= makeSingleGrid(pos);
                grids[i, j] = success;

                //if making single grid fails, quit making more
                if (!success)
                    goto endOfLoop;
                
            }
            j = 0;
        }

        endOfLoop:

        //set up mesh
        mesh.SetVertices(verts);
        mesh.SetTriangles(tris, 0);
        mesh.SetUVs(0, uvs);

        mesh.RecalculateNormals();

        return success;

        //makes single grid, if verts offset + newGrid have more than allowed verts, return false
        bool makeSingleGrid((int x, int y) p)
        {

            if ((vertsoffset + (4 + 12 * 4 + 12 * 4 * roadCount)) > (ushort.MaxValue - 1))
                return false;

            Vector3 start = gridCordsToWorldCords(p, distance, roadCount);

            //center quad - 4 verts
            makeQuad(start + left + back, start + right + back, start + right + forward, start + left + forward, ref quadCenterUVs);

            //crossing quads - 4 * 12 verts
            makeRoadBlock(start + left * 2, leftRotation, ref quadCrossingUVs, ref quadSideroadUVs);
            makeRoadBlock(start + right * 2, rightRotation, ref quadCrossingUVs, ref quadSideroadUVs);
            makeRoadBlock(start + forward * 2, forwardRotation, ref quadCrossingUVs, ref quadSideroadUVs);
            makeRoadBlock(start + back * 2, backRotation, ref quadCrossingUVs, ref quadSideroadUVs);

            //road blocks - 4 * 12 * roadCount verts
            for (int r = 0; r < roadCount; r++)
            {
                int offset = (r + 2) * 2;
                makeRoadBlock(start + left * offset, leftRotation, ref quadRoadUVs, ref quadSideroadUVs);
                makeRoadBlock(start + right * offset, rightRotation, ref quadRoadUVs, ref quadSideroadUVs);
                makeRoadBlock(start + forward * offset, forwardRotation, ref quadRoadUVs, ref quadSideroadUVs);
                makeRoadBlock(start + back * offset, backRotation, ref quadRoadUVs, ref quadSideroadUVs);
            }

            return true;
        }

        //adds 12 verts
        void makeRoadBlock(Vector3 position, Quaternion rotation, ref Vector2[] roadCords, ref Vector2[] sideroadCords)
        {

            Vector3 a = position + rotation * (left + back),
                    b = position + rotation * (right + back),
                    c = position + rotation * (right + forward),
                    d = position + rotation * (left + forward);

            Vector3 ad = position + rotation * (left + back + Vector3.down * depth),
                    bd = position + rotation * (right + back + Vector3.down * depth),
                    cd = position + rotation * (right + forward + Vector3.down * depth),
                    dd = position + rotation * (left + forward + Vector3.down * depth);

            //road quad
            makeQuad(a, b, c, d, ref roadCords);

            //right sideroad
            makeQuad(bd, cd, c, b, ref sideroadCords);

            //left sideroad
            makeQuad(a, d, dd, ad, ref sideroadCords);

        }

        //adds 4 verts
        void makeQuad(Vector3 a, Vector3 b, Vector3 c, Vector3 d, ref Vector2[] uvCords)
        {

            if(uvCords.Length < 4)
            {
                throw new System.Exception("Given UV map for quad doesn't have 4 or more elements");
            }

            //done by makeSingleGrid
            /*if((vertsoffset + 4) >= (ushort.MaxValue - 1))
            {
                return false;
            }*/

            verts.Add(a);   //A 4
            verts.Add(b);   //B 3
            verts.Add(c);   //C 2
            verts.Add(d);   //D 1
            vertsoffset += 4;

            //triangle ABD
            tris.Add(vertsoffset - 4);
            tris.Add(vertsoffset - 1);
            tris.Add(vertsoffset - 3);
            
            //triangle CBD
            tris.Add(vertsoffset - 2);
            tris.Add(vertsoffset - 3);
            tris.Add(vertsoffset - 1);

            //uvs
            uvs.Add(uvCords[0]);
            uvs.Add(uvCords[1]);
            uvs.Add(uvCords[2]);
            uvs.Add(uvCords[3]);

        }

    }

}
