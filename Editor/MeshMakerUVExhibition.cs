using UnityEngine;
using UnityEditor;

public class MeshMakerUVExhibition : EditorWindow
{
    private Vector2 _scrollPosition = Vector2.zero;
    private MeshMaker _meshMaker;
    private Texture _texture;
    private float _amplification;
    private float _moveAmount;
    private float _scaleAmount;
    private Rect _vertexButtonRect;
    private float _vertexButtonSize;

    public MeshMakerVertex CurrentCheckedVertex
    {
        get {
            return _currentCheckedVertex;
        }
        set {
            _currentCheckedVertex = value;
            if (_currentCheckedVertex != null)
                _vertexButtonRect = new Rect(_currentCheckedVertex.UV.x * _amplification - _vertexButtonSize / 2,
                    _currentCheckedVertex.UV.y * _amplification - _vertexButtonSize / 2,
                    _vertexButtonSize, _vertexButtonSize);
        }
    }
    private MeshMakerVertex _currentCheckedVertex;    

    public void Init(MeshMaker meshMaker)
    {
        _meshMaker = meshMaker;
        _texture = _meshMaker._meshRenderer.sharedMaterial.GetTexture("_MainTex");
        if (!_texture)
        {
            _texture = AssetDatabase.LoadAssetAtPath("Assets/MeshEditor/MeshMaker/Editor/Texture/UVBg.png", typeof(Texture)) as Texture;
            _meshMaker._meshRenderer.sharedMaterial.SetTexture("_MainTex", _texture);
        }
        _amplification = 400;
        _moveAmount = 0.02f;
        _scaleAmount = 10;
        _vertexButtonSize = 10;
        position = new Rect(0, 0, _amplification, _amplification + 20);
    }

    private void OnGUI()
    {
        if(_meshMaker)
        {
            GUILayout.BeginHorizontal("toolbarbutton");
            if (GUILayout.Button("Magnify", "toolbarbutton"))
            {
                _amplification += _scaleAmount;
                if (_amplification > 1000)
                    _amplification = 1000;
            }
            if (GUILayout.Button("Shrink", "toolbarbutton"))
            {
                _amplification -= _scaleAmount;
                if (_amplification < 100)
                    _amplification = 100;
            }
            if (GUILayout.Button("←", "toolbarbutton"))
            {
                if (_currentCheckedVertex != null)
                {
                    _currentCheckedVertex.UV = new Vector2(_currentCheckedVertex.UV.x - _moveAmount, _currentCheckedVertex.UV.y);
                    _vertexButtonRect = new Rect(_currentCheckedVertex.UV.x * _amplification - _vertexButtonSize / 2,
                        _currentCheckedVertex.UV.y * _amplification - _vertexButtonSize / 2,
                        _vertexButtonSize, _vertexButtonSize);
                    Repaint();
                }
            }
            if (GUILayout.Button("→", "toolbarbutton"))
            {
                if (_currentCheckedVertex != null)
                {
                    _currentCheckedVertex.UV = new Vector2(_currentCheckedVertex.UV.x + _moveAmount, _currentCheckedVertex.UV.y);
                    _vertexButtonRect = new Rect(_currentCheckedVertex.UV.x * _amplification - _vertexButtonSize / 2,
                        _currentCheckedVertex.UV.y * _amplification - _vertexButtonSize / 2,
                        _vertexButtonSize, _vertexButtonSize);
                    Repaint();
                }
            }
            if (GUILayout.Button("↑", "toolbarbutton"))
            {
                if (_currentCheckedVertex != null)
                {
                    _currentCheckedVertex.UV = new Vector2(_currentCheckedVertex.UV.x, _currentCheckedVertex.UV.y - _moveAmount);
                    _vertexButtonRect = new Rect(_currentCheckedVertex.UV.x * _amplification - _vertexButtonSize / 2,
                        _currentCheckedVertex.UV.y * _amplification - _vertexButtonSize / 2,
                        _vertexButtonSize, _vertexButtonSize);
                    Repaint();
                }
            }
            if (GUILayout.Button("↓", "toolbarbutton"))
            {
                if (_currentCheckedVertex != null)
                {
                    _currentCheckedVertex.UV = new Vector2(_currentCheckedVertex.UV.x, _currentCheckedVertex.UV.y + _moveAmount);
                    _vertexButtonRect = new Rect(_currentCheckedVertex.UV.x * _amplification - _vertexButtonSize / 2,
                        _currentCheckedVertex.UV.y * _amplification - _vertexButtonSize / 2,
                        _vertexButtonSize, _vertexButtonSize);
                    Repaint();
                }
            }
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Apply", "toolbarbutton"))
            {
                _meshMaker.RefreshUV();
            }
            GUILayout.EndHorizontal();

            _scrollPosition = GUILayout.BeginScrollView(_scrollPosition);

            EditorGUI.DrawPreviewTexture(new Rect(0, 0, _amplification, _amplification), _texture);
            Handles.color = Color.red;
            for (int i = 0; i < _meshMaker.Triangles.Count; i++)
            {
                Handles.DrawLine(_meshMaker.Triangles[i].Vertex1.UV * _amplification, _meshMaker.Triangles[i].Vertex2.UV * _amplification);
                Handles.DrawLine(_meshMaker.Triangles[i].Vertex2.UV * _amplification, _meshMaker.Triangles[i].Vertex3.UV * _amplification);
                Handles.DrawLine(_meshMaker.Triangles[i].Vertex3.UV * _amplification, _meshMaker.Triangles[i].Vertex1.UV * _amplification);
            }
            if (_currentCheckedVertex != null)
            {
                GUI.color = Color.cyan;
                if (GUI.RepeatButton(_vertexButtonRect, ""))
                {
                    _currentCheckedVertex.UV = new Vector2(Event.current.mousePosition.x / _amplification, Event.current.mousePosition.y / _amplification);
                    _vertexButtonRect = new Rect(_currentCheckedVertex.UV.x * _amplification - _vertexButtonSize / 2,
                        _currentCheckedVertex.UV.y * _amplification - _vertexButtonSize / 2,
                        _vertexButtonSize, _vertexButtonSize);
                    Repaint();
                }
            }

            GUILayout.EndScrollView();
        }
    }

    private void OnDestroy()
    {
        if (_meshMaker)
        {
            _meshMaker.IsCanEdit = true;
        }
    }
}
