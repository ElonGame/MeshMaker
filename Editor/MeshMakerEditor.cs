using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(MeshMaker))]
public class MeshMakerEditor : Editor 
{
    private MeshMaker _meshMaker;
    //当前鼠标经过的三角面
    private MeshMakerTriangle _currentHoverTriangle;
    //当前鼠标选中的三角面
    private MeshMakerTriangle _currentCheckedTriangle;
    //当前鼠标选中的顶点
    private MeshMakerVertex _currentCheckedVertex;
    //当前鼠标选中的边
    private MeshMakerEdge _currentCheckedEdge;
    //当前操作柄模式
    private HandleTool _currentHandleTool;
    //当前编辑模式
    private EditMode _currentEditMode;
    //当前编辑的面
    private GameObject _currentEditTriangle;
    private GameObject _currentEditTriangleVertex1;
    private GameObject _currentEditTriangleVertex2;
    private GameObject _currentEditTriangleVertex3;
    //当前编辑的边
    private GameObject _currentEditEdge;
    private GameObject _currentEditEdgeVertex1;
    private GameObject _currentEditEdgeVertex2;
    //按钮内容
    private GUIContent _vertexButtonContent;
    private GUIContent _edgeButtonContent;
    private GUIContent _faceButtonContent;
    private GUIContent _noneButtonContent;
    //高级编辑模式
    private bool _secondaryHandle;
    private SecondaryHandleMode _secondaryHandleMode;
    //场景摄像机
    private Camera _sceneCamera;
    //UV浏览界面
    private MeshMakerUVExhibition _uvPanel;

    private void OnEnable()
	{
        Init();
    }

    private void OnDisable()
    {
        if (_currentEditTriangle)
            DestroyImmediate(_currentEditTriangle);
        if (_currentEditEdge)
            DestroyImmediate(_currentEditEdge);
        Undo.undoRedoPerformed -= OnRecord;
    }
    
    private void Init()
    {
        _meshMaker = target as MeshMaker;
        if (_meshMaker == null || EditorApplication.isPlaying)
        {
            return;
        }

        TransformChange(_meshMaker);
        _currentHoverTriangle = null;
        _currentCheckedTriangle = null;
        _currentCheckedVertex = null;
        _currentCheckedEdge = null;
        _currentHandleTool = HandleTool.None;
        _currentEditMode = EditMode.Vertex;

        _currentEditTriangle = new GameObject("Triangle");
        _currentEditTriangle.transform.SetParent(_meshMaker.Target.transform);
        _currentEditTriangle.hideFlags = HideFlags.HideInHierarchy;
        _currentEditTriangleVertex1 = new GameObject("Vertex1");
        _currentEditTriangleVertex1.transform.SetParent(_currentEditTriangle.transform);
        _currentEditTriangleVertex1.hideFlags = HideFlags.HideInHierarchy;
        _currentEditTriangleVertex2 = new GameObject("Vertex2");
        _currentEditTriangleVertex2.transform.SetParent(_currentEditTriangle.transform);
        _currentEditTriangleVertex2.hideFlags = HideFlags.HideInHierarchy;
        _currentEditTriangleVertex3 = new GameObject("Vertex3");
        _currentEditTriangleVertex3.transform.SetParent(_currentEditTriangle.transform);
        _currentEditTriangleVertex3.hideFlags = HideFlags.HideInHierarchy;

        _currentEditEdge = new GameObject("Edge");
        _currentEditEdge.transform.SetParent(_meshMaker.Target.transform);
        _currentEditEdge.hideFlags = HideFlags.HideInHierarchy;
        _currentEditEdgeVertex1 = new GameObject("Vertex1");
        _currentEditEdgeVertex1.transform.SetParent(_currentEditEdge.transform);
        _currentEditEdgeVertex1.hideFlags = HideFlags.HideInHierarchy;
        _currentEditEdgeVertex2 = new GameObject("Vertex2");
        _currentEditEdgeVertex2.transform.SetParent(_currentEditEdge.transform);
        _currentEditEdgeVertex2.hideFlags = HideFlags.HideInHierarchy;
        
        if (_vertexButtonContent == null)
        {
            Texture2D t2d = AssetDatabase.LoadAssetAtPath("Assets/MeshEditor/MeshMaker/Editor/Texture/Vertex.png", typeof(Texture2D)) as Texture2D;
            _vertexButtonContent = new GUIContent("", t2d, "Vertex Edit Mode");
        }
        if (_edgeButtonContent == null)
        {
            Texture2D t2d = AssetDatabase.LoadAssetAtPath("Assets/MeshEditor/MeshMaker/Editor/Texture/Edge.png", typeof(Texture2D)) as Texture2D;
            _edgeButtonContent = new GUIContent("", t2d, "Edge Edit Mode");
        }
        if (_faceButtonContent == null)
        {
            Texture2D t2d = AssetDatabase.LoadAssetAtPath("Assets/MeshEditor/MeshMaker/Editor/Texture/Face.png", typeof(Texture2D)) as Texture2D;
            _faceButtonContent = new GUIContent("", t2d, "Face Edit Mode");
        }
        if (_noneButtonContent == null)
        {
            Texture2D t2d = AssetDatabase.LoadAssetAtPath("Assets/MeshEditor/MeshMaker/Editor/Texture/None.png", typeof(Texture2D)) as Texture2D;
            _noneButtonContent = new GUIContent("", t2d, "None Edit Mode");
        }
        _secondaryHandle = false;
        _secondaryHandleMode = SecondaryHandleMode.None;
        _sceneCamera = SceneView.lastActiveSceneView.camera;
        Undo.undoRedoPerformed += OnRecord;
    }
    
    public override void OnInspectorGUI()
    {
        GUI.enabled = _meshMaker.IsCanEdit;

        #region 编辑模式选择
        EditorGUILayout.BeginHorizontal();
        GUI.color = _currentEditMode == EditMode.Vertex ? Color.green : Color.white;
        if (GUILayout.Button(_vertexButtonContent, GUILayout.Height(40), GUILayout.Width(45)))
        {
            _currentEditMode = EditMode.Vertex;
            StopSecondaryHandle();
        }
        GUI.color = _currentEditMode == EditMode.Edge ? Color.green : Color.white;
        if (GUILayout.Button(_edgeButtonContent, GUILayout.Height(40), GUILayout.Width(45)))
        {
            _currentEditMode = EditMode.Edge;
            StopSecondaryHandle();
        }
        GUI.color = _currentEditMode == EditMode.Face ? Color.green : Color.white;
        if (GUILayout.Button(_faceButtonContent, GUILayout.Height(40), GUILayout.Width(45)))
        {
            _currentEditMode = EditMode.Face;
            StopSecondaryHandle();
        }
        GUI.color = _currentEditMode == EditMode.None ? Color.green : Color.white;
        if (GUILayout.Button(_noneButtonContent, GUILayout.Height(40), GUILayout.Width(45)))
        {
            _currentEditMode = EditMode.None;
            StopSecondaryHandle();
        }
        GUI.color = Color.white;
        EditorGUILayout.EndHorizontal();
        #endregion

        #region 点编辑模式
        if (_currentEditMode == EditMode.Vertex)
        {
            EditorGUILayout.BeginHorizontal("HelpBox");
            GUILayout.Label("Click the right mouse to select vertex!");
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Hanlde Size:", GUILayout.Width(80));
            _meshMaker.VertexHandleSize = GUILayout.HorizontalSlider(_meshMaker.VertexHandleSize, 0.001f, 0.01f);
            _meshMaker.VertexHandleSize = EditorGUILayout.FloatField(_meshMaker.VertexHandleSize, GUILayout.Width(50));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUI.color = Color.cyan;
            if (GUILayout.Button("目标删除", "ButtonLeft"))
            {
                DeleteVertex();
            }
            if (GUILayout.Button("目标焊接", "ButtonMid"))
            {
                WeldingVertex();
            }
            if (GUILayout.Button("目标克隆", "ButtonRight"))
            {
                CloneVertex();
            }
            GUI.color = Color.white;
            EditorGUILayout.EndHorizontal();
        }
        #endregion

        #region 边编辑模式
        if (_currentEditMode == EditMode.Edge)
        {
            EditorGUILayout.BeginHorizontal("HelpBox");
            GUILayout.Label("Click the right mouse to select edge!");
            EditorGUILayout.EndHorizontal();
        }
        #endregion

        #region 面编辑模式
        if (_currentEditMode == EditMode.Face)
        {
            EditorGUILayout.BeginHorizontal("HelpBox");
            GUILayout.Label("Click the right mouse to select face!");
            EditorGUILayout.EndHorizontal();
        }
        #endregion

        #region 其他
        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Hover Color:", GUILayout.Width(80));
        _meshMaker.HoverColor = EditorGUILayout.ColorField(_meshMaker.HoverColor);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        GUILayout.Label("Checked Color:", GUILayout.Width(80));
        _meshMaker.CheckedColor = EditorGUILayout.ColorField(_meshMaker.CheckedColor);
        EditorGUILayout.EndHorizontal(); 

        EditorGUILayout.BeginHorizontal();
        GUI.color = Color.green;
        if (GUILayout.Button("UV Exhibition", "LargeButton"))
        {
            UVExhibition();
        }
        GUI.color = Color.white;
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        GUI.color = Color.green;
        if (GUILayout.Button("Save Mesh", "LargeButton"))
        {
            SaveMesh();
        }
        GUI.color = Color.white;
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        GUI.color = Color.red;
        if (GUILayout.Button("Edit End", "LargeButton"))
        {
            EditEnd();
        }
        GUI.color = Color.white;
        EditorGUILayout.EndHorizontal();
        #endregion
    }

    private void OnSceneGUI()
    {
        ChangeHandleTool();

        if (_meshMaker.IsCanEdit)
        {
            CaptureHoverTarget();
            CaptureCheckedTarget();
            SecondaryHandle();
            SceneView.RepaintAll();

            if (GUI.changed)
            {
                _meshMaker.RefreshMesh();

                if (Event.current.button == 0 && Event.current.isMouse && Event.current.type == EventType.MouseDown)
                    Undo.RecordObject(_meshMaker, "Edit Mesh");
            }
        }
        else
        {
            if (_uvPanel)
                CaptureCheckedTargetInUV();
        }      
    }

    private void OnRecord()
    {
        _meshMaker.RefreshMesh();
    }

    /// <summary>
    /// 强行改变Transform,使重绘
    /// </summary>
    private void TransformChange(MeshMaker meshMaker)
    {
        meshMaker.transform.position += new Vector3(1, 0, 0);
        meshMaker.transform.position -= new Vector3(1, 0, 0);
    }
    /// <summary>
    /// 改变操作柄模式
    /// </summary>
    private void ChangeHandleTool()
    {
        if (Tools.current == Tool.View)
        {
            _currentHandleTool = HandleTool.None;
        }
        else if (Tools.current == Tool.Move)
        {
            _currentHandleTool = HandleTool.Move;
        }
        else if (Tools.current == Tool.Rotate)
        {
            _currentHandleTool = HandleTool.Rotate;
        }
        else if (Tools.current == Tool.Scale)
        {
            _currentHandleTool = HandleTool.Scale;
        }
        Tools.current = Tool.None;

        if (_secondaryHandle)
        {
            _currentHandleTool = HandleTool.None;
        }
    }

    /// <summary>
    /// 截获鼠标当前经过的目标
    /// </summary>
    private void CaptureHoverTarget()
    {
        if (Event.current.type == EventType.MouseMove)
        {
            RaycastHit hit;
            if (Physics.Raycast(HandleUtility.GUIPointToWorldRay(Event.current.mousePosition), out hit))
            {
                if (hit.triangleIndex >= 0 && hit.triangleIndex < _meshMaker.Triangles.Count)
                {
                    _currentHoverTriangle = _meshMaker.Triangles[hit.triangleIndex];
                    if (_currentEditMode == EditMode.Face && _currentHoverTriangle == _currentCheckedTriangle)
                        ClearHoverTarget();
                }
                else
                {
                    ClearHoverTarget();
                }
            }
            else
            {
                ClearHoverTarget();
            }
        }
        else if (Event.current.type == EventType.MouseDown)
        {
            ClearHoverTarget();
        }

        ShowHoverTarget();
    }
    /// <summary>
    /// 显示鼠标当前经过的目标
    /// </summary>
    private void ShowHoverTarget()
    {
        if (_currentHoverTriangle != null)
        {
            switch (_currentEditMode)
            {
                case EditMode.Vertex:
                    Handles.color = _meshMaker.HoverColor;
                    if (_currentCheckedVertex != _currentHoverTriangle.Vertex1)
                        Handles.DotCap(0, _currentHoverTriangle.Vertex1.Vertex, Quaternion.identity, _meshMaker.VertexHandleSize);
                    if (_currentCheckedVertex != _currentHoverTriangle.Vertex2)
                        Handles.DotCap(0, _currentHoverTriangle.Vertex2.Vertex, Quaternion.identity, _meshMaker.VertexHandleSize);
                    if (_currentCheckedVertex != _currentHoverTriangle.Vertex3)
                        Handles.DotCap(0, _currentHoverTriangle.Vertex3.Vertex, Quaternion.identity, _meshMaker.VertexHandleSize);
                    break;
                case EditMode.Edge:
                    Handles.color = _meshMaker.HoverColor;
                    if (_currentCheckedEdge != _currentHoverTriangle.Edge1)
                        Handles.DrawLine(_currentHoverTriangle.Edge1.Vertex1.Vertex, _currentHoverTriangle.Edge1.Vertex2.Vertex);
                    if (_currentCheckedEdge != _currentHoverTriangle.Edge2)
                        Handles.DrawLine(_currentHoverTriangle.Edge2.Vertex1.Vertex, _currentHoverTriangle.Edge2.Vertex2.Vertex);
                    if (_currentCheckedEdge != _currentHoverTriangle.Edge3)
                        Handles.DrawLine(_currentHoverTriangle.Edge3.Vertex1.Vertex, _currentHoverTriangle.Edge3.Vertex2.Vertex);
                    break;
                case EditMode.Face:
                    Vector3[] vecs = new Vector3[] { _currentHoverTriangle.Vertex1.Vertex, _currentHoverTriangle.Vertex2.Vertex,
                        _currentHoverTriangle.Vertex3.Vertex, _currentHoverTriangle.Vertex3.Vertex };
                    Handles.DrawSolidRectangleWithOutline(vecs, _meshMaker.HoverColor, Color.black);
                    break;
            }
        }
    }
    /// <summary>
    /// 清空鼠标当前经过的目标
    /// </summary>
    private void ClearHoverTarget()
    {
        if (_currentHoverTriangle != null)
            _currentHoverTriangle = null;
    }

    /// <summary>
    /// 截获鼠标当前选中的目标
    /// </summary>
    private void CaptureCheckedTarget()
    {
        if (!_secondaryHandle && Event.current.button == 1 && Event.current.isMouse && Event.current.type == EventType.MouseDown)
        {
            RaycastHit hit;
            if (Physics.Raycast(HandleUtility.GUIPointToWorldRay(Event.current.mousePosition), out hit))
            {
                if (hit.triangleIndex >= 0 && hit.triangleIndex < _meshMaker.Triangles.Count)
                {
                    _currentCheckedTriangle = _meshMaker.Triangles[hit.triangleIndex];
                }
                else
                {
                    ClearCheckedTarget();
                }
            }
            else
            {
                ClearCheckedTarget();
            }

            if (_currentCheckedTriangle != null)
            {
                switch (_currentEditMode)
                {
                    case EditMode.Vertex:
                        _currentCheckedVertex = _currentCheckedTriangle.GetVertexByClick(hit.point);
                        break;
                    case EditMode.Edge:
                        _currentCheckedEdge = _currentCheckedTriangle.GetEdgeByClick(hit.point);
                        SetEditEdge();
                        break;
                    case EditMode.Face:
                        SetEditTriangle();
                        break;
                }
            }
        }

        Selection.activeObject = _meshMaker.gameObject;

        ShowCheckedTarget();
    }
    /// <summary>
    /// 显示鼠标当前选中的目标
    /// </summary>
    private void ShowCheckedTarget()
    {
        if (_currentCheckedTriangle != null)
        {
            switch (_currentEditMode)
            {
                case EditMode.Vertex:
                    EditVertex();
                    break;
                case EditMode.Edge:
                    EditEdge();
                    break;
                case EditMode.Face:
                    EditFace();
                    break;
            }
        }
    }
    /// <summary>
    /// 清空鼠标当前选中的目标
    /// </summary>
    private void ClearCheckedTarget()
    {
        if (_currentCheckedTriangle != null)
            _currentCheckedTriangle = null;
        if (_currentCheckedEdge != null)
            _currentCheckedEdge = null;
        if (_currentCheckedVertex != null)
            _currentCheckedVertex = null;
    }

    /// <summary>
    /// 顶点编辑模式
    /// </summary>
    private void EditVertex()
    {
        if (_currentCheckedVertex != null)
        {
            Handles.color = _meshMaker.CheckedColor;
            Handles.DotCap(0, _currentCheckedVertex.Vertex, Quaternion.identity, _meshMaker.VertexHandleSize);

            switch (_currentHandleTool)
            {
                case HandleTool.Move:
                    Vector3 oldVec = _currentCheckedVertex.Vertex;
                    Vector3 newVec = Handles.PositionHandle(oldVec, Quaternion.identity);
                    if (oldVec != newVec)
                    {
                        _currentCheckedVertex.Vertex = newVec;
                    }
                    break;
                case HandleTool.Rotate:
                    Handles.RotationHandle(Quaternion.identity, _currentCheckedVertex.Vertex);
                    break;
                case HandleTool.Scale:
                    Handles.ScaleHandle(Vector3.one, _currentCheckedVertex.Vertex, Quaternion.identity, 1);
                    break;
            }
        }
    }
    /// <summary>
    /// 边编辑模式
    /// </summary>
    private void EditEdge()
    {
        if (_currentCheckedEdge != null)
        {
            Handles.color = _meshMaker.CheckedColor;
            Handles.DrawLine(_currentCheckedEdge.Vertex1.Vertex, _currentCheckedEdge.Vertex2.Vertex);

            switch (_currentHandleTool)
            {
                case HandleTool.Move:
                    Vector3 oldVec = _currentEditEdge.transform.position;
                    Vector3 newVec = Handles.PositionHandle(oldVec, Quaternion.identity);
                    if (oldVec != newVec)
                    {
                        _currentEditEdge.transform.position = newVec;
                        ApplyEditEdge();
                    }
                    break;
                case HandleTool.Rotate:
                    Quaternion oldRot = _currentEditEdge.transform.rotation;
                    Quaternion newRot = Handles.RotationHandle(oldRot, _currentEditEdge.transform.position);
                    if (oldRot != newRot)
                    {
                        _currentEditEdge.transform.rotation = newRot;
                        ApplyEditEdge();
                    }
                    break;
                case HandleTool.Scale:
                    Vector3 oldSca = _currentEditEdge.transform.localScale;
                    Vector3 newSca = Handles.ScaleHandle(oldSca, _currentEditEdge.transform.position, Quaternion.identity, 1);
                    if (oldSca != newSca)
                    {
                        _currentEditEdge.transform.localScale = newSca;
                        ApplyEditEdge();
                    }
                    break;
            }
        }
    }
    /// <summary>
    /// 面编辑模式
    /// </summary>
    private void EditFace()
    {
        if (_currentCheckedTriangle != null)
        {
            Vector3[] vecs = new Vector3[] { _currentCheckedTriangle.Vertex1.Vertex, _currentCheckedTriangle.Vertex2.Vertex,
                        _currentCheckedTriangle.Vertex3.Vertex, _currentCheckedTriangle.Vertex3.Vertex };
            Handles.DrawSolidRectangleWithOutline(vecs, _meshMaker.CheckedColor, Color.black);

            switch (_currentHandleTool)
            {
                case HandleTool.Move:
                    Vector3 oldVec = _currentEditTriangle.transform.position;
                    Vector3 newVec = Handles.PositionHandle(oldVec, Quaternion.identity);
                    if (oldVec != newVec)
                    {
                        _currentEditTriangle.transform.position = newVec;
                        ApplyEditTriangle();
                    }
                    break;
                case HandleTool.Rotate:
                    Quaternion oldRot = _currentEditTriangle.transform.rotation;
                    Quaternion newRot = Handles.RotationHandle(oldRot, _currentEditTriangle.transform.position);
                    if (oldRot != newRot)
                    {
                        _currentEditTriangle.transform.rotation = newRot;
                        ApplyEditTriangle();
                    }
                    break;
                case HandleTool.Scale:
                    Vector3 oldSca = _currentEditTriangle.transform.localScale;
                    Vector3 newSca = Handles.ScaleHandle(oldSca, _currentEditTriangle.transform.position, Quaternion.identity, 1);
                    if (oldSca != newSca)
                    {
                        _currentEditTriangle.transform.localScale = newSca;
                        ApplyEditTriangle();
                    }
                    break;
            }
        }
    }

    /// <summary>
    /// 设置当前编辑的面
    /// </summary>
    private void SetEditTriangle()
    {
        _currentEditTriangle.transform.position = _currentCheckedTriangle.TriangleCenter();
        _currentEditTriangle.transform.rotation = Quaternion.identity;
        _currentEditTriangle.transform.localScale = Vector3.one;
        _currentEditTriangleVertex1.transform.position = _currentCheckedTriangle.Vertex1.Vertex;
        _currentEditTriangleVertex2.transform.position = _currentCheckedTriangle.Vertex2.Vertex;
        _currentEditTriangleVertex3.transform.position = _currentCheckedTriangle.Vertex3.Vertex;
    }
    /// <summary>
    /// 应用当前编辑的面
    /// </summary>
    private void ApplyEditTriangle()
    {
        _currentCheckedTriangle.Vertex1.Vertex = _currentEditTriangleVertex1.transform.position;
        _currentCheckedTriangle.Vertex2.Vertex = _currentEditTriangleVertex2.transform.position;
        _currentCheckedTriangle.Vertex3.Vertex = _currentEditTriangleVertex3.transform.position;
    }
    /// <summary>
    /// 设置当前编辑的边
    /// </summary>
    private void SetEditEdge()
    {
        _currentEditEdge.transform.position = _currentCheckedEdge.EdgeCenter();
        _currentEditEdge.transform.rotation = Quaternion.identity;
        _currentEditEdge.transform.localScale = Vector3.one;
        _currentEditEdgeVertex1.transform.position = _currentCheckedEdge.Vertex1.Vertex;
        _currentEditEdgeVertex2.transform.position = _currentCheckedEdge.Vertex2.Vertex;
    }
    /// <summary>
    /// 应用当前编辑的边
    /// </summary>
    private void ApplyEditEdge()
    {
        _currentCheckedEdge.Vertex1.Vertex = _currentEditEdgeVertex1.transform.position;
        _currentCheckedEdge.Vertex2.Vertex = _currentEditEdgeVertex2.transform.position;
    }

    /// <summary>
    /// 删除顶点
    /// </summary>
    private void DeleteVertex()
    {
        if (_currentCheckedVertex == null)
        {
            Debug.LogWarning("请先选中一个顶点！");
            return;
        }

        _secondaryHandle = true;
        _secondaryHandleMode = SecondaryHandleMode.Delete;
    }
    /// <summary>
    /// 焊接顶点
    /// </summary>
    private void WeldingVertex()
    {
        if (_currentCheckedVertex == null)
        {
            Debug.LogWarning("请先选中一个顶点！");
            return;
        }

        _secondaryHandle = true;
        _secondaryHandleMode = SecondaryHandleMode.Welding;
    }
    /// <summary>
    /// 克隆顶点（于选中的两个相连顶点的中心位置克隆）
    /// </summary>
    private void CloneVertex()
    {
        if (_currentCheckedVertex == null)
        {
            Debug.LogWarning("请先选中一个顶点！");
            return;
        }

        _secondaryHandle = true;
        _secondaryHandleMode = SecondaryHandleMode.Clone;
    }

    /// <summary>
    /// 高级编辑模式
    /// </summary>
    private void SecondaryHandle()
    {
        if (_secondaryHandle)
        {
            #region 顶点模式
            if (_currentEditMode == EditMode.Vertex && _currentCheckedVertex != null)
            {
                switch (_secondaryHandleMode)
                {
                    case SecondaryHandleMode.Delete:
                        _meshMaker.Vertexs.Remove(_currentCheckedVertex);
                        _meshMaker.Vertexs.RefreshID();
                        _meshMaker.Triangles.RemoveByVertex(_currentCheckedVertex);
                        _meshMaker.Triangles.RefreshID();
                        _meshMaker.GenerateMesh();
                        _currentCheckedVertex = null;
                        StopSecondaryHandle();
                        break;
                    case SecondaryHandleMode.Welding:
                        Vector3 wv = _meshMaker.ScreenToWorldPointInScene(_sceneCamera, Event.current.mousePosition, _currentCheckedVertex.Vertex);
                        Handles.DrawDottedLine(_currentCheckedVertex.Vertex, wv, 0.2f);
                        Handles.Label(wv, "   请选择焊接目标");
                        if (Event.current.button == 1 && Event.current.isMouse && Event.current.type == EventType.MouseDown)
                        {
                            RaycastHit hit;
                            if (Physics.Raycast(HandleUtility.GUIPointToWorldRay(Event.current.mousePosition), out hit))
                            {
                                if (hit.triangleIndex >= 0 && hit.triangleIndex < _meshMaker.Triangles.Count)
                                {
                                    MeshMakerVertex mmv = _meshMaker.Triangles[hit.triangleIndex].GetVertexByClick(hit.point);

                                    if (mmv != _currentCheckedVertex)
                                    {
                                        _meshMaker.Vertexs.Remove(_currentCheckedVertex);
                                        _meshMaker.Vertexs.RefreshID();
                                        _meshMaker.Triangles.RemoveByVertexs(_currentCheckedVertex, mmv);
                                        _meshMaker.Triangles.ReplaceVertex(_currentCheckedVertex, mmv);
                                        _meshMaker.Triangles.RefreshID();
                                        _meshMaker.GenerateMesh();
                                        _currentCheckedVertex = null;
                                    }
                                }
                            }
                            StopSecondaryHandle();
                        }
                        break;
                    case SecondaryHandleMode.Clone:
                        Vector3 cv = _meshMaker.ScreenToWorldPointInScene(_sceneCamera, Event.current.mousePosition, _currentCheckedVertex.Vertex);
                        Handles.DrawDottedLine(_currentCheckedVertex.Vertex, cv, 0.2f);
                        Handles.Label(cv, "   请选择参照目标");
                        if (Event.current.button == 1 && Event.current.isMouse && Event.current.type == EventType.MouseDown)
                        {
                            RaycastHit hit;
                            if (Physics.Raycast(HandleUtility.GUIPointToWorldRay(Event.current.mousePosition), out hit))
                            {
                                if (hit.triangleIndex >= 0 && hit.triangleIndex < _meshMaker.Triangles.Count)
                                {
                                    MeshMakerVertex mmv = _meshMaker.Triangles[hit.triangleIndex].GetVertexByClick(hit.point);

                                    if (mmv != _currentCheckedVertex && _meshMaker.Triangles.IsConnected(_currentCheckedVertex, mmv))
                                    {
                                        Vector3 vertex = Vector3.Lerp(_currentCheckedVertex.Vertex, mmv.Vertex, 0.5f);
                                        Vector2 uv = Vector2.Lerp(_currentCheckedVertex.UV, mmv.UV, 0.5f);
                                        MeshMakerVertex newMmv = new MeshMakerVertex(0, vertex, uv, _currentCheckedVertex.Normal, new List<int>());

                                        _meshMaker.Vertexs.Add(newMmv);
                                        _meshMaker.Vertexs.RefreshID();
                                        _meshMaker.Triangles.SegmentationTriangle(mmv, _currentCheckedVertex, newMmv);
                                        _meshMaker.Triangles.RefreshID();
                                        _meshMaker.GenerateMesh();
                                        _currentCheckedVertex = null;
                                    }
                                }
                            }
                            StopSecondaryHandle();
                        }
                        break;
                }
            }
            #endregion
        }
    }
    /// <summary>
    /// 结束高级编辑模式
    /// </summary>
    private void StopSecondaryHandle()
    {
        _secondaryHandle = false;
        _secondaryHandleMode = SecondaryHandleMode.None;
        ClearCheckedTarget();
    }

    /// <summary>
    /// 保存Mesh数据
    /// </summary>
    private void SaveMesh()
    {
        string path = EditorUtility.SaveFilePanel("Save Mesh", Application.dataPath, "New Mesh", "asset");
        if (path.Length != 0)
        {
            string subPath = path.Substring(0, path.IndexOf("Asset"));
            path = path.Replace(subPath, "");
            AssetDatabase.CreateAsset(_meshMaker._meshFilter.sharedMesh, path);
            AssetDatabase.SaveAssets();
        }
        StopSecondaryHandle();
    }
    /// <summary>
    /// 编辑完成
    /// </summary>
    private void EditEnd()
    {
        _meshMaker.transform.GetComponent<MeshRenderer>().enabled = true;
        if (_meshMaker.transform.GetComponent<Collider>())
            _meshMaker.transform.GetComponent<Collider>().enabled = true;

        DestroyImmediate(_meshMaker);
    }
    /// <summary>
    /// 展UV
    /// </summary>
    private void UVExhibition()
    {
        _uvPanel = EditorWindow.GetWindow<MeshMakerUVExhibition>();
        _uvPanel.Show();
        _uvPanel.Init(_meshMaker);
        _meshMaker.IsCanEdit = false;
        _currentEditMode = EditMode.None;
        _currentHandleTool = HandleTool.None;
        StopSecondaryHandle();
    }

    /// <summary>
    /// 截获鼠标当前选中目标在UV模式
    /// </summary>
    private void CaptureCheckedTargetInUV()
    {
        if (Event.current.button == 0 && Event.current.isMouse && Event.current.type == EventType.MouseDown)
        {
            RaycastHit hit;
            if (Physics.Raycast(HandleUtility.GUIPointToWorldRay(Event.current.mousePosition), out hit))
            {
                if (hit.triangleIndex >= 0 && hit.triangleIndex < _meshMaker.Triangles.Count)
                {
                    _uvPanel.CurrentCheckedVertex = _meshMaker.Triangles[hit.triangleIndex].GetVertexByClick(hit.point);
                }
                else
                {
                    _uvPanel.CurrentCheckedVertex = null;
                }
            }
            else
            {
                _uvPanel.CurrentCheckedVertex = null;
            }
            _uvPanel.Repaint();
        }

        Selection.activeObject = _meshMaker.gameObject;

        ShowCheckedTargetInUV();
    }
    /// <summary>
    /// 显示鼠标当前选中的目标在UV模式
    /// </summary>
    private void ShowCheckedTargetInUV()
    {
        if (_uvPanel.CurrentCheckedVertex != null)
        {
            Handles.color = _meshMaker.CheckedColor;
            Handles.DotCap(0, _uvPanel.CurrentCheckedVertex.Vertex, Quaternion.identity, _meshMaker.VertexHandleSize);
        }
    }
}

public enum HandleTool
{
    None = -1,
    Move = 0,
    Rotate = 1,
    Scale = 2,
}
public enum EditMode
{
    None = -1,
    Vertex = 0,
    Edge = 1,
    Face = 2,
}
public enum SecondaryHandleMode
{
    None = -1,
    Delete = 0,
    Welding = 1,
    Clone = 2,
}
