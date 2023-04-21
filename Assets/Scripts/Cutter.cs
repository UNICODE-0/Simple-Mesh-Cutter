using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class Cutter
{
    private static Plane _CutPlane;
    
    public static GameObject[] CutObject( GameObject ObjectToCut, Vector3 CutPlanePos, Vector3 PlaneNormal, 
                                          GameObjectProperties LeftPartProperties, GameObjectProperties RightPartProperties, 
                                          bool CalculateSliceUV)
    {
        MeshFilter ObjMeshFilter = ObjectToCut.GetComponent<MeshFilter>();

        if(ObjMeshFilter is null)
        {
            Debug.LogError("The object cannot be cut, there is no Mesh Filter component on it.");
            return null;
        }

        MeshRenderer ObjMeshRenderer = ObjectToCut.GetComponent<MeshRenderer>();

        if(ObjMeshRenderer is null)
        {
            Debug.LogError("The object cannot be cut, there is no Mesh Renderer component on it.");
            return null;
        }

        Mesh ObjectMesh = ObjMeshFilter.mesh;

        if(ObjMeshRenderer is null)
        {
            Debug.LogError("The object cannot be cut, there is no mesh in the mesh filter component.");
            return null;
        }

        Vector3 LocalPlaneNormal = ObjectToCut.transform.InverseTransformVector(PlaneNormal);
        _CutPlane = new Plane(LocalPlaneNormal, ObjectToCut.transform.InverseTransformPoint(CutPlanePos));

        CustomMesh LeftMesh = new CustomMesh();
        LeftMesh.vertices.AddRange(ObjectMesh.vertices);
        LeftMesh.normals.AddRange(ObjectMesh.normals);
        LeftMesh.uvs.AddRange(ObjectMesh.uv);

        CustomMesh RightMesh = new CustomMesh();
        RightMesh.vertices.AddRange(ObjectMesh.vertices);
        RightMesh.normals.AddRange(ObjectMesh.normals);
        RightMesh.uvs.AddRange(ObjectMesh.uv);

        int TrianglesOnPositiveSide = 0;
        int TrianglesOnNegativeSide = 0;

        List<Vector3> RightMeshVerticesOnSlice = new List<Vector3>();
        List<Vector3> LeftMeshVerticesOnSlice = new List<Vector3>();

        for (int i = 0; i < ObjectMesh.triangles.Length; i+=3)
        {
            int FirstIndex = ObjectMesh.triangles[i];
            int SecondIndex = ObjectMesh.triangles[i + 1];
            int ThirdIndex = ObjectMesh.triangles[i + 2];

            bool FirstIndexOnPositiveSide = _CutPlane.GetSide(ObjectMesh.vertices[FirstIndex]);
            bool SecondIndexOnPositiveSide = _CutPlane.GetSide(ObjectMesh.vertices[SecondIndex]);
            bool ThirdIndexOnPositiveSide = _CutPlane.GetSide(ObjectMesh.vertices[ThirdIndex]);

            if(FirstIndexOnPositiveSide && SecondIndexOnPositiveSide && ThirdIndexOnPositiveSide)
            {
                RightMesh.triangles.Add(FirstIndex);
                RightMesh.triangles.Add(SecondIndex);
                RightMesh.triangles.Add(ThirdIndex);

                TrianglesOnPositiveSide++;
            } else if(!FirstIndexOnPositiveSide && !SecondIndexOnPositiveSide && !ThirdIndexOnPositiveSide)
            {
                LeftMesh.triangles.Add(FirstIndex);
                LeftMesh.triangles.Add(SecondIndex);
                LeftMesh.triangles.Add(ThirdIndex);

                TrianglesOnNegativeSide++;
            } else
            {
                Vector3 Vert1 = ObjectMesh.vertices[FirstIndex];
                Vector3 Vert2 = ObjectMesh.vertices[SecondIndex];
                Vector3 Vert3 = ObjectMesh.vertices[ThirdIndex];

                Vector3 Normal1 = ObjectMesh.normals[FirstIndex];
                Vector3 Normal2 = ObjectMesh.normals[SecondIndex];
                Vector3 Normal3 = ObjectMesh.normals[ThirdIndex];

                Vector3 UV1 = ObjectMesh.uv[FirstIndex];
                Vector3 UV2 = ObjectMesh.uv[SecondIndex];
                Vector3 UV3 = ObjectMesh.uv[ThirdIndex];

                if(FirstIndexOnPositiveSide && !SecondIndexOnPositiveSide && !ThirdIndexOnPositiveSide)
                {
                    CutTriangle( FirstIndex, ThirdIndex, SecondIndex,
                                 Vert1, Vert3 ,Vert2,
                                 Normal1, Normal3, Normal2,
                                 UV1, UV3, UV2,
                                 RightMesh, _CutPlane,  ref RightMeshVerticesOnSlice,
                                 true);
                }
                else if(!FirstIndexOnPositiveSide && SecondIndexOnPositiveSide && !ThirdIndexOnPositiveSide)
                {
                    CutTriangle( SecondIndex, FirstIndex, ThirdIndex,
                                 Vert2, Vert1, Vert3,
                                 Normal2, Normal1, Normal3,
                                 UV2, UV1, UV3,
                                 RightMesh, _CutPlane,  ref RightMeshVerticesOnSlice,
                                 true);
                }
                else if(!FirstIndexOnPositiveSide && !SecondIndexOnPositiveSide && ThirdIndexOnPositiveSide)
                {
                    CutTriangle( ThirdIndex, SecondIndex, FirstIndex,
                                 Vert3, Vert2, Vert1,
                                 Normal3, Normal2, Normal1,
                                 UV3, UV2, UV1,
                                 RightMesh, _CutPlane,  ref RightMeshVerticesOnSlice,
                                 true);
                }
                else if(!FirstIndexOnPositiveSide && SecondIndexOnPositiveSide && ThirdIndexOnPositiveSide)
                {
                    CutTriangle( SecondIndex, ThirdIndex, FirstIndex,
                                 Vert2, Vert3, Vert1,
                                 Normal2, Normal3, Normal1,
                                 UV2, UV3, UV1,
                                 RightMesh, _CutPlane,  ref RightMeshVerticesOnSlice,
                                 false);
                }
                else if(FirstIndexOnPositiveSide && !SecondIndexOnPositiveSide && ThirdIndexOnPositiveSide)
                {
                    CutTriangle( FirstIndex, ThirdIndex, SecondIndex,
                                 Vert1, Vert3, Vert2,
                                 Normal1, Normal3, Normal2,
                                 UV1, UV3, UV2,
                                 RightMesh, _CutPlane,  ref RightMeshVerticesOnSlice,
                                 false, true);
                }
                else if(FirstIndexOnPositiveSide && SecondIndexOnPositiveSide && !ThirdIndexOnPositiveSide)
                {
                    CutTriangle( FirstIndex, SecondIndex, ThirdIndex,
                                 Vert1, Vert2, Vert3,
                                 Normal1, Normal2, Normal3,
                                 UV1, UV2, UV3,
                                 RightMesh, _CutPlane,  ref RightMeshVerticesOnSlice,
                                 false);
                }

                if(!FirstIndexOnPositiveSide && SecondIndexOnPositiveSide && ThirdIndexOnPositiveSide)
                {
                    CutTriangle( FirstIndex, ThirdIndex, SecondIndex,
                                 Vert1, Vert3 ,Vert2,
                                 Normal1, Normal3, Normal2,
                                 UV1, UV3, UV2,
                                 LeftMesh, _CutPlane,  ref LeftMeshVerticesOnSlice,
                                 true);
                }
                else if(FirstIndexOnPositiveSide && !SecondIndexOnPositiveSide && ThirdIndexOnPositiveSide)
                {
                    CutTriangle( SecondIndex, FirstIndex, ThirdIndex,
                                 Vert2, Vert1, Vert3,
                                 Normal2, Normal1, Normal3,
                                 UV2, UV1, UV3,
                                 LeftMesh, _CutPlane,  ref LeftMeshVerticesOnSlice,
                                 true);
                }
                else if(FirstIndexOnPositiveSide && SecondIndexOnPositiveSide && !ThirdIndexOnPositiveSide)
                {
                    CutTriangle( ThirdIndex, SecondIndex, FirstIndex,
                                 Vert3, Vert2, Vert1,
                                 Normal3, Normal2, Normal1,
                                 UV3, UV2, UV1,
                                 LeftMesh, _CutPlane,  ref LeftMeshVerticesOnSlice,
                                 true);
                }
                else if(FirstIndexOnPositiveSide && !SecondIndexOnPositiveSide && !ThirdIndexOnPositiveSide)
                {
                    CutTriangle( SecondIndex, ThirdIndex, FirstIndex,
                                 Vert2, Vert3, Vert1,
                                 Normal2, Normal3, Normal1,
                                 UV2, UV3, UV1,
                                 LeftMesh, _CutPlane, ref LeftMeshVerticesOnSlice, 
                                 false);
                }
                else if(!FirstIndexOnPositiveSide && SecondIndexOnPositiveSide && !ThirdIndexOnPositiveSide)
                {
                    CutTriangle( FirstIndex, ThirdIndex, SecondIndex,
                                 Vert1, Vert3, Vert2,
                                 Normal1, Normal3, Normal2,
                                 UV1, UV3, UV2,
                                 LeftMesh, _CutPlane, ref LeftMeshVerticesOnSlice,
                                 false, true);
                }
                else if(!FirstIndexOnPositiveSide && !SecondIndexOnPositiveSide && ThirdIndexOnPositiveSide)
                {
                    CutTriangle( FirstIndex, SecondIndex, ThirdIndex,
                                 Vert1, Vert2, Vert3,
                                 Normal1, Normal2, Normal3,
                                 UV1, UV2, UV3,
                                 LeftMesh, _CutPlane,  ref LeftMeshVerticesOnSlice,
                                 false);
                }
            }
        }

        if(CalculateSliceUV)
        {
            FillCutHoleWithUV(RightMeshVerticesOnSlice, RightMesh, _CutPlane, false);
            FillCutHoleWithUV(LeftMeshVerticesOnSlice, LeftMesh, _CutPlane, true);
        } else
        {
            FillCutHole(RightMeshVerticesOnSlice, RightMesh, _CutPlane, false);
            FillCutHole(LeftMeshVerticesOnSlice, LeftMesh, _CutPlane, true);
        }

        int TrianglesCount = ObjectMesh.triangles.Length / 3;
        if(TrianglesOnNegativeSide != TrianglesCount && TrianglesOnPositiveSide != TrianglesCount)
        {
            GameObject[] NewParts = new GameObject[2];
            NewParts[0] = CreateGameObject("LeftPart", ObjectToCut.transform, LeftMesh.GetMesh(), ObjMeshRenderer.sharedMaterial, LeftPartProperties);
            NewParts[1] = CreateGameObject("RightPart", ObjectToCut.transform, RightMesh.GetMesh(), ObjMeshRenderer.sharedMaterial, RightPartProperties);
            MonoBehaviour.Destroy(ObjectToCut);

            return NewParts;
        }

        return null;
    }
    private static void AddVertexData(Vector3 Vert, Vector3 Normal, Vector3 UV, CustomMesh Mesh)
    {
        Mesh.vertices.Add(Vert);
        Mesh.normals.Add(Normal);
        Mesh.uvs.Add(UV);
        Mesh.triangles.Add(Mesh.vertices.Count - 1);
    }
    private static void CutTriangle( int Vert1Index, int Vert2Index, int Vert3Index,
                              Vector3 Vert1, Vector3 Vert2, Vector3 Vert3, 
                              Vector3 Normal1, Vector3 Normal2, Vector3 Normal3,
                              Vector3 UV1, Vector3 UV2, Vector3 UV3,
                              CustomMesh Mesh, Plane CutPlane, ref List<Vector3> NewVertices,
                              bool IsBasedOnOneVert, bool FlipVerticesOrder = false)
    {
        Vector3 NewVert;
        Vector3 NewNormal;
        Vector3 NewUV;

        if(IsBasedOnOneVert)
        {
            LerpMeshDataByIntersection( Vert1, Vert3, out NewVert,
                                        Normal1, Normal3, out NewNormal,
                                        UV1, UV3, out NewUV,
                                        CutPlane);
            AddVertexData(NewVert, NewNormal, NewUV, Mesh);
            NewVertices.Add(NewVert);

            LerpMeshDataByIntersection( Vert1, Vert2, out NewVert,
                                        Normal1, Normal2, out NewNormal,
                                        UV1, UV2, out NewUV,
                                        CutPlane);
            AddVertexData(NewVert, NewNormal, NewUV, Mesh);
            NewVertices.Add(NewVert);

            Mesh.triangles.Add(Vert1Index);
        }
        else
        {
            Mesh.triangles.Add(Vert2Index);

            LerpMeshDataByIntersection( Vert1, Vert3, out NewVert,
                                        Normal1, Normal3, out NewNormal,
                                        UV1, UV3, out NewUV,
                                        CutPlane);
            AddVertexData(NewVert, NewNormal, NewUV, Mesh);
            int VertBetween1And3Index = Mesh.vertices.Count - 1;
            NewVertices.Add(NewVert);
        
            Mesh.triangles.Add(Vert1Index);
            
            Mesh.triangles.Add(Vert2Index);

            LerpMeshDataByIntersection( Vert2, Vert3, out NewVert,
                                        Normal2, Normal3, out NewNormal,
                                        UV2, UV3, out NewUV,
                                        CutPlane);
            AddVertexData(NewVert, NewNormal, NewUV, Mesh);
            NewVertices.Add(NewVert);

            Mesh.triangles.Add(VertBetween1And3Index);
        }

        if(FlipVerticesOrder) FlipTriVertOrder(Mesh, IsBasedOnOneVert ? 1 : 2);
    }
    private static void FillCutHole(List<Vector3> VerticesOnSlice, CustomMesh Mesh, Plane CutPlane, bool FlipVerticesOrder)
    {
        Vector3 MiddleVertex = Vector3.zero;
        for (int i = 0; i < VerticesOnSlice.Count; i++)
        {
            MiddleVertex += VerticesOnSlice[i];
        }
        MiddleVertex /= VerticesOnSlice.Count;

        Vector3 SliceNormal = FlipVerticesOrder ? CutPlane.normal : -CutPlane.normal;

        Vector2 ZeroUV = Vector2.zero;
        Vector3 NewVert1 = Vector3.zero;
        Vector3 NewVert2 = Vector3.zero;
        for (int i = 0; i < VerticesOnSlice.Count; i += 2)
        {
            NewVert1 = VerticesOnSlice[(i + 1) % VerticesOnSlice.Count];
            NewVert2 = VerticesOnSlice[i];
            
            AddTriangleToSlice( MiddleVertex, NewVert1, NewVert2,
                                ZeroUV, ZeroUV, ZeroUV,
                                SliceNormal, CutPlane.normal, Mesh, 
                                FlipVerticesOrder);
        }
    }
    private static void FillCutHoleWithUV(List<Vector3> VerticesOnSlice, CustomMesh Mesh, Plane CutPlane, bool FlipVerticesOrder)
    {
        Vector3 MiddleVertex = Vector3.zero;

        List<Vector3> LinesMiddle = new List<Vector3>();
        List<float> LinesLength = new List<float>();
        for (int i = 0; i < VerticesOnSlice.Count; i += 2)
        {
            Vector3 Vert1 = VerticesOnSlice[(i + 1) % VerticesOnSlice.Count];
            Vector3 Vert2 = VerticesOnSlice[i];

            LinesMiddle.Add((Vert1 + Vert2) / 2);
            LinesLength.Add((Vert1 - Vert2).magnitude);
        }

        Vector3 Temp = Vector3.zero;
        for (int i = 0; i < LinesMiddle.Count; i++)
        {
            Temp +=  LinesMiddle[i] * LinesLength[i];
        }
        MiddleVertex = Temp / LinesLength.Sum();

        Vector3 SliceNormal = FlipVerticesOrder ? CutPlane.normal : -CutPlane.normal;

        Vector3 tangent;
        Vector3 t1 = Vector3.Cross( CutPlane.normal, Vector3.forward );
        Vector3 t2 = Vector3.Cross( CutPlane.normal, Vector3.up );
        if( t1.magnitude > t2.magnitude )
        {
            tangent = t1.normalized;
        }
        else
        {
            tangent = t2.normalized;
        }

        Vector3 Up = Vector3.Cross(tangent, CutPlane.normal).normalized;

        for (int i = 0; i < VerticesOnSlice.Count; i += 2)
        {
            Vector3 NewVert1 = VerticesOnSlice[(i + 1) % VerticesOnSlice.Count];
            Vector3 DirToVertex = NewVert1 - MiddleVertex;
            Vector2 UV1 = new Vector2(0.5f + Vector3.Dot(DirToVertex, tangent), 0.5f + Vector3.Dot(DirToVertex, Up));

            Vector3 NewVert2 = VerticesOnSlice[i];
            DirToVertex = NewVert2 - MiddleVertex;
            Vector2 UV2 = new Vector2(0.5f + Vector3.Dot(DirToVertex, tangent), 0.5f + Vector3.Dot(DirToVertex, Up));

            AddTriangleToSlice( MiddleVertex, NewVert1, NewVert2,
                                new(0.5f, 0.5f), UV1, UV2,
                                SliceNormal, CutPlane.normal, Mesh, 
                                FlipVerticesOrder);
        }
    }
    private static void AddTriangleToSlice( Vector3 MiddleVertex, Vector3 Vert1, Vector3 Vert2,
                                     Vector2 MiddleVertexUV, Vector2 UV1, Vector2 UV2,
                                     Vector3 SliceNormal, Vector3 CutPlaneNormal, CustomMesh Mesh,
                                     bool FlipVerticesOrder)
    {
        AddVertexData(MiddleVertex, SliceNormal, MiddleVertexUV, Mesh);
        AddVertexData(Vert1, SliceNormal, UV1, Mesh);
        AddVertexData(Vert2, SliceNormal, UV2, Mesh);
        if(Vector3.Dot(Vector3.Cross(Vert1 - Vert2, MiddleVertex - Vert2), CutPlaneNormal) < 0)
        {
            if(!FlipVerticesOrder) FlipTriVertOrder(Mesh, 1);
        } else if(FlipVerticesOrder) FlipTriVertOrder(Mesh, 1);
    }
    private static void FlipTriVertOrder(CustomMesh Mesh, int TrisNumberToFlip)
    {
        int Temp;
        int IndexesCount = Mesh.triangles.Count;
        int LastTriIndex = IndexesCount - TrisNumberToFlip * 3;
        for (int i = IndexesCount; i > LastTriIndex; i-=3)
        {
            Temp = Mesh.triangles[i - 2];
            Mesh.triangles[i - 2] = Mesh.triangles[i - 1];
            Mesh.triangles[i - 1] = Temp;
        }
    }
    private static void LerpMeshDataByIntersection( Vector3 Vert1, Vector3 Vert2, out Vector3 NewVert,
                                             Vector3 Normal1, Vector3 Normal2, out Vector3 NewNormal,
                                             Vector3 UV1, Vector3 UV2, out Vector3 NewUV,
                                             Plane IntersectionPlane)
    {
        Vector3 DistanceVector = Vert2 - Vert1;
        float WholeDistance = DistanceVector.magnitude;
        float DistanceToPlane; 
        IntersectionPlane.Raycast(new Ray(Vert1, DistanceVector), out DistanceToPlane);

        NewVert = Vector3.Lerp(Vert1, Vert2, DistanceToPlane / WholeDistance);
        NewNormal = Vector3.Lerp(Normal1, Normal2, DistanceToPlane / WholeDistance);
        NewUV = Vector3.Lerp(UV1, UV2, DistanceToPlane / WholeDistance);
    }
    private static GameObject CreateGameObject(string Name, Transform ObjTransform, Mesh ObjMesh, Material ObjMaterial, GameObjectProperties Properties)
    {
        GameObject NewObject = new GameObject(Name);
        NewObject.transform.position = ObjTransform.position;
        NewObject.transform.rotation = ObjTransform.rotation;
        NewObject.transform.localScale = ObjTransform.localScale; 

        MeshFilter NewObjectMeshFilter = NewObject.AddComponent<MeshFilter>();
        MeshRenderer NewObjectMeshRenderer = NewObject.AddComponent<MeshRenderer>();
        
        switch (Properties.collider)
        {
            case ColliderType.Mesh:
                MeshCollider NewObjectMeshCollider = NewObject.AddComponent<MeshCollider>();
                NewObjectMeshCollider.sharedMesh = ObjMesh;
                NewObjectMeshCollider.convex = true;
            break;
            case ColliderType.Box:
                BoxCollider NewObjectBoxCollider = NewObject.AddComponent<BoxCollider>();
                NewObjectBoxCollider.size = ObjMesh.bounds.size;
                NewObjectBoxCollider.center = ObjMesh.bounds.center;
            break;
        }

        if(Properties.hasRigidbody)
        {
            Rigidbody NewObjectRigidbody = NewObject.AddComponent<Rigidbody>();
        }

        NewObjectMeshFilter.mesh = ObjMesh;

        if(Properties.material is null) NewObjectMeshRenderer.material = ObjMaterial;
        else NewObjectMeshRenderer.material = Properties.material;

        return NewObject;
    }
}
public class GameObjectProperties
{
    public GameObjectProperties(ColliderType Collider, bool HasRigidbody, Material Material = null)
    {
        collider = Collider;
        hasRigidbody = HasRigidbody;
        material = Material;
    }
    public Material material { get; set; }
    public ColliderType collider { get; set; }
    public bool hasRigidbody { get; set; }
}

public enum ColliderType
{
    None,
    Box,
    Mesh
}