using UnityEngine;
using System.Collections.Generic;
using System;

[Serializable]
public class MeshMakerVertex : MeshMakerBase
{
    public int ID;
    public Vector3 Vertex;
    public Vector2 UV;
    public Vector3 Normal;
    public List<int> VertexIndexs;

    public MeshMakerVertex(int id, Vector3 vertex, Vector2 uv, Vector3 normal, List<int> vertexIndexs)
    {
        ID = id;
        Vertex = vertex;
        UV = uv;
        Normal = normal;
        VertexIndexs = vertexIndexs;
    }
}
