using UnityEngine;
using System;

[Serializable]
public class MeshMakerTriangle : MeshMakerBase
{
    public int ID;
    public MeshMakerVertex Vertex1;
    public MeshMakerVertex Vertex2;
    public MeshMakerVertex Vertex3;
    public MeshMakerEdge Edge1;
    public MeshMakerEdge Edge2;
    public MeshMakerEdge Edge3;

    public MeshMakerTriangle(int id, ref MeshMakerVertex vertex1, ref MeshMakerVertex vertex2, ref MeshMakerVertex vertex3)
    {
        ID = id;
        Vertex1 = vertex1;
        Vertex2 = vertex2;
        Vertex3 = vertex3;
        RefreshEdge();
    }

    public Vector3 TriangleCenter()
    {
        Vector3 triangle = new Vector3((Vertex1.Vertex.x + Vertex2.Vertex.x + Vertex3.Vertex.x) / 3,
           (Vertex1.Vertex.y + Vertex2.Vertex.y + Vertex3.Vertex.y) / 3,
           (Vertex1.Vertex.z + Vertex2.Vertex.z + Vertex3.Vertex.z) / 3);

        return triangle;
    }

    public void RefreshEdge()
    {
        Edge1 = new MeshMakerEdge(ref Vertex1, ref Vertex2);
        Edge2 = new MeshMakerEdge(ref Vertex2, ref Vertex3);
        Edge3 = new MeshMakerEdge(ref Vertex3, ref Vertex1);
    }
}
