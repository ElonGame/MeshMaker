using UnityEngine;
using System;

[Serializable]
public class MeshMakerEdge : MeshMakerBase
{
    public MeshMakerVertex Vertex1;
    public MeshMakerVertex Vertex2;

    public MeshMakerEdge(ref MeshMakerVertex vertex1, ref MeshMakerVertex vertex2)
    {
        Vertex1 = vertex1;
        Vertex2 = vertex2;
    }

    public Vector3 EdgeCenter()
    {
        Vector3 edge = new Vector3((Vertex1.Vertex.x + Vertex2.Vertex.x) / 2,
           (Vertex1.Vertex.y + Vertex2.Vertex.y) / 2,
           (Vertex1.Vertex.z + Vertex2.Vertex.z) / 2);

        return edge;
    }
}
