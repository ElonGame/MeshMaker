using UnityEngine;
using System.Collections.Generic;
using System;

[Serializable]
public class MeshMakerVertex : MeshMakerBase
{
    private int _id;
    public int ID
    {
        get
        {
            return _id;
        }
        set
        {
            _id = value;
        }
    }

    private Vector3 _vertex;
    public Vector3 Vertex
    {
        get {
            return _vertex;
        }
        set {
            _vertex = value;
        }
    }

    private Vector2 _uv;
    public Vector2 UV
    {
        get
        {
            return _uv;
        }
        set
        {
            _uv = value;
        }
    }

    private Vector3 _normal;
    public Vector3 Normal
    {
        get
        {
            return _normal;
        }
        set
        {
            _normal = value;
        }
    }

    private List<int> _vertexIndexs;
    public List<int> VertexIndexs
    {
        get {
            return _vertexIndexs;
        }
        set {
            _vertexIndexs = value;
        }
    }

    public MeshMakerVertex(int id, Vector3 vertex, Vector2 uv, Vector3 normal, List<int> vertexIndexs)
    {
        _id = id;
        Vertex = vertex;
        UV = uv;
        Normal = normal;
        VertexIndexs = vertexIndexs;
    }
}
