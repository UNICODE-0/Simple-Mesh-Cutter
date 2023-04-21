using System.Collections.Generic;
using UnityEngine;

public class CustomMesh
{
    List<Vector3> _Vertices = new List<Vector3>();
    List<Vector3> _Normals = new List<Vector3>();
    List<Vector2> _UVs = new List<Vector2>();
    List<int> _Triangles = new List<int>();

    public List<Vector3> vertices
    {
        get { return _Vertices; }
        set { _Vertices = value; }
    }
    public List<Vector3> normals
    {
        get { return _Normals; }
        set { _Normals = value; }
    }
    public List<Vector2> uvs
    {
        get { return _UVs; }
        set { _UVs = value; }
    }
    public List<int> triangles
    {
        get { return _Triangles; }
        set { _Triangles = value; }
    }

    public void OptimizeMesh()
    {
        List<Vector3> NewVertices = new List<Vector3>();
        List<Vector3> NewNormals = new List<Vector3>();
        List<Vector2> NewUVs = new List<Vector2>();
        List<int> NewTriangles = new List<int>();;
        for (int i = 0; i < _Triangles.Count; i++)
        {
            NewVertices.Add(_Vertices[_Triangles[i]]);
            NewNormals.Add(_Normals[_Triangles[i]]);
            NewUVs.Add(_UVs[_Triangles[i]]);
            NewTriangles.Add(NewVertices.Count - 1);
        }

        _Vertices = NewVertices;
        _Normals = NewNormals;
        _UVs = NewUVs;
        _Triangles = NewTriangles;
    }
    
    public Mesh GetMesh()
    {
        Mesh NewMesh = new Mesh();
        NewMesh.name = "New Mesh";

        NewMesh.vertices = _Vertices.ToArray();
        NewMesh.normals = _Normals.ToArray();
        NewMesh.triangles = _Triangles.ToArray();
        NewMesh.SetUVs(0, uvs);

        NewMesh.Optimize();
        NewMesh.RecalculateBounds();

        return NewMesh;
    }
}