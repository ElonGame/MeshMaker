using UnityEngine;
using System;
using System.Collections.Generic;
using UnityEditor;

public static class MeshMakerTool 
{
    /// <summary>
    /// 仅在编辑器中编译并执行
    /// </summary>
    public static void CompileOnlyEditor(this MeshMaker meshMaker, Action action)
    {
#if UNITY_EDITOR
        action();
#endif
    }

    /// <summary>
    /// 仅在编辑器以外编译并执行
    /// </summary>
    public static void CompileNoEditor(this MeshMaker meshMaker, Action action)
    {
#if !UNITY_EDITOR
        action();
#endif
    }

    /// <summary>
    /// 将场景中的屏幕坐标转换为世界坐标
    /// </summary>
    public static Vector3 ScreenToWorldPointInScene(this MeshMaker meshMaker, Camera sceneCamera, Vector3 screenPositon, Vector3 referencePosition)
    {
        Vector3 vec = screenPositon;
        vec.y = sceneCamera.pixelHeight - vec.y;
        vec.z = sceneCamera.worldToCameraMatrix.MultiplyPoint(referencePosition).z * -1;
        vec = sceneCamera.ScreenToWorldPoint(vec);
        return vec;
    }

    /// <summary>
    /// 根据顶点索引获取顶点值
    /// </summary>
    public static MeshMakerVertex GetVertexByIndex(this List<MeshMakerVertex> vertexs, int index)
    {
        for (int i = 0; i < vertexs.Count; i++)
        {
            if (vertexs[i].VertexIndexs.Contains(index))
            {
                return vertexs[i];
            }
        }
        return null;
    }

    /// <summary>
    /// 重新排列顶点组的ID
    /// </summary>
    public static void RefreshID(this List<MeshMakerVertex> vertexs)
    {
        for (int i = 0; i < vertexs.Count; i++)
        {
            vertexs[i].ID = i;
        }
    }

    /// <summary>
    /// 为所有面执行移除操作，条件是该面包含顶点vertex
    /// </summary>
    public static void RemoveByVertex(this List<MeshMakerTriangle> triangles, MeshMakerVertex vertex)
    {
        for (int i = 0; i < triangles.Count; i++)
        {
            if (triangles[i].Vertex1 == vertex || triangles[i].Vertex2 == vertex || triangles[i].Vertex3 == vertex)
            {
                triangles.RemoveAt(i);
                i--;
            }
        }
    }

    /// <summary>
    /// 为所有面执行移除操作，条件是该面包含顶点vertex1和vertex2
    /// </summary>
    public static void RemoveByVertexs(this List<MeshMakerTriangle> triangles, MeshMakerVertex vertex1, MeshMakerVertex vertex2)
    {
        for (int i = 0; i < triangles.Count; i++)
        {
            if ((triangles[i].Vertex1 == vertex1 || triangles[i].Vertex2 == vertex1 || triangles[i].Vertex3 == vertex1) &&
                (triangles[i].Vertex1 == vertex2 || triangles[i].Vertex2 == vertex2 || triangles[i].Vertex3 == vertex2))
            {
                triangles.RemoveAt(i);
                i--;
            }
        }
    }

    /// <summary>
    /// 为所有面执行替换操作，将旧的顶点oldVertex替换为新的顶点newVertex
    /// </summary>
    public static void ReplaceVertex(this List<MeshMakerTriangle> triangles, MeshMakerVertex oldVertex, MeshMakerVertex newVertex)
    {
        for (int i = 0; i < triangles.Count; i++)
        {
            if (triangles[i].Vertex1 == oldVertex)
            {
                triangles[i].Vertex1 = newVertex;
                triangles[i].RefreshEdge();
            }
            else if (triangles[i].Vertex2 == oldVertex)
            {
                triangles[i].Vertex2 = newVertex;
                triangles[i].RefreshEdge();
            }
            else if (triangles[i].Vertex3 == oldVertex)
            {
                triangles[i].Vertex3 = newVertex;
                triangles[i].RefreshEdge();
            }
        }
    }

    /// <summary>
    /// 判断vertex1和vertex2是否相连，也就是判断是否存在于同一面中
    /// </summary>
    public static bool IsConnected(this List<MeshMakerTriangle> triangles, MeshMakerVertex vertex1, MeshMakerVertex vertex2)
    {
        for (int i = 0; i < triangles.Count; i++)
        {
            if ((triangles[i].Vertex1 == vertex1 || triangles[i].Vertex2 == vertex1 || triangles[i].Vertex3 == vertex1) &&
                (triangles[i].Vertex1 == vertex2 || triangles[i].Vertex2 == vertex2 || triangles[i].Vertex3 == vertex2))
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 为所有面执行分割操作，条件是该面包含顶点vertex1和vertex2，并以新的顶点newVertex作为分割之后的顶点
    /// </summary>
    public static void SegmentationTriangle(this List<MeshMakerTriangle> triangles, MeshMakerVertex vertex1, MeshMakerVertex vertex2, MeshMakerVertex newVertex)
    {
        List<MeshMakerTriangle> newTriangles = new List<MeshMakerTriangle>();
        for (int i = 0; i < triangles.Count; i++)
        {
            if ((triangles[i].Vertex1 == vertex1 || triangles[i].Vertex1 == vertex2) && (triangles[i].Vertex2 == vertex1 || triangles[i].Vertex2 == vertex2))
            {
                MeshMakerTriangle mmt1 = new MeshMakerTriangle(0, ref triangles[i].Vertex1, ref newVertex, ref triangles[i].Vertex3);
                MeshMakerTriangle mmt2 = new MeshMakerTriangle(0, ref newVertex, ref triangles[i].Vertex2, ref triangles[i].Vertex3);
                triangles.RemoveAt(i);
                newTriangles.Add(mmt1);
                newTriangles.Add(mmt2);
                i--;
            }
            else if ((triangles[i].Vertex2 == vertex1 || triangles[i].Vertex2 == vertex2) && (triangles[i].Vertex3 == vertex1 || triangles[i].Vertex3 == vertex2))
            {
                MeshMakerTriangle mmt1 = new MeshMakerTriangle(0, ref triangles[i].Vertex1, ref triangles[i].Vertex2, ref newVertex);
                MeshMakerTriangle mmt2 = new MeshMakerTriangle(0, ref triangles[i].Vertex1, ref newVertex, ref triangles[i].Vertex3);
                triangles.RemoveAt(i);
                newTriangles.Add(mmt1);
                newTriangles.Add(mmt2);
                i--;
            }
            else if ((triangles[i].Vertex1 == vertex1 || triangles[i].Vertex1 == vertex2) && (triangles[i].Vertex3 == vertex1 || triangles[i].Vertex3 == vertex2))
            {
                MeshMakerTriangle mmt1 = new MeshMakerTriangle(0, ref triangles[i].Vertex1, ref triangles[i].Vertex2, ref newVertex);
                MeshMakerTriangle mmt2 = new MeshMakerTriangle(0, ref newVertex, ref triangles[i].Vertex2, ref triangles[i].Vertex3);
                triangles.RemoveAt(i);
                newTriangles.Add(mmt1);
                newTriangles.Add(mmt2);
                i--;
            }
        }
        triangles.AddRange(newTriangles);
    }

    /// <summary>
    /// 重新排列三角面组的ID
    /// </summary>
    public static void RefreshID(this List<MeshMakerTriangle> triangles)
    {
        for (int i = 0; i < triangles.Count; i++)
        {
            triangles[i].ID = i;
        }
    }

    /// <summary>
    /// 鼠标点击，获取距离鼠标最近的顶点
    /// </summary>
    public static MeshMakerVertex GetVertexByClick(this MeshMakerTriangle triangle, Vector3 clickPoint)
    {
        float distance1 = Vector3.Distance(triangle.Vertex1.Vertex, clickPoint);
        float distance2 = Vector3.Distance(triangle.Vertex2.Vertex, clickPoint);
        float distance3 = Vector3.Distance(triangle.Vertex3.Vertex, clickPoint);

        if (distance1 < distance2 && distance1 < distance3)
            return triangle.Vertex1;
        if (distance2 < distance1 && distance2 < distance3)
            return triangle.Vertex2;
        if (distance3 < distance1 && distance3 < distance2)
            return triangle.Vertex3;
        return triangle.Vertex1;
    }

    /// <summary>
    /// 鼠标点击，获取距离鼠标最近的边
    /// </summary>
    public static MeshMakerEdge GetEdgeByClick(this MeshMakerTriangle triangle, Vector3 clickPoint)
    {
        float distance1 = HandleUtility.DistancePointLine(clickPoint, triangle.Edge1.Vertex1.Vertex, triangle.Edge1.Vertex2.Vertex);
        float distance2 = HandleUtility.DistancePointLine(clickPoint, triangle.Edge2.Vertex1.Vertex, triangle.Edge2.Vertex2.Vertex);
        float distance3 = HandleUtility.DistancePointLine(clickPoint, triangle.Edge3.Vertex1.Vertex, triangle.Edge3.Vertex2.Vertex);

        if (distance1 < distance2 && distance1 < distance3)
            return triangle.Edge1;
        if (distance2 < distance1 && distance2 < distance3)
            return triangle.Edge2;
        if (distance3 < distance1 && distance3 < distance2)
            return triangle.Edge3;
        return triangle.Edge1;
    }
}
