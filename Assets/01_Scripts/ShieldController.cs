using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using System.Runtime.CompilerServices;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(PolygonCollider2D))]
[RequireComponent(typeof(PlayerInput))]
public class ShieldController : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float rotationSpeed = 1.0f;
    [SerializeField] private float angleLerp = 1.0f;

    [Header("Shield")]
    [SerializeField] private float radius = 2f;
    [SerializeField] private float thickness = 0.5f;
    [SerializeField] private float angle = 90f; // degrees
    [SerializeField] private int segments = 30;

    MeshFilter _meshFilter;
    PolygonCollider2D _polyCollider;
    PlayerInput _playerInput;

    private bool _rotate = false;
    private Vector2 _startDir;
    private float _targetAngle;

    void Awake()
    {
        _meshFilter = GetComponent<MeshFilter>();
        _polyCollider = GetComponent<PolygonCollider2D>();
        _playerInput = GetComponent<PlayerInput>();
        GenerateMeshAndCollider();

        _startDir = Quaternion.Euler(0, 0, angle / 2) * Vector2.right;
    }

    void Update()
    {
        Rotate();
    }

    void OnValidate()
    {
        if (_meshFilter == null)
            _meshFilter = GetComponent<MeshFilter>();
        if (_polyCollider == null)
            _polyCollider = GetComponent<PolygonCollider2D>();

        GenerateMeshAndCollider();
    }

    private void Rotate()
    {
        if (_rotate)
        {
            Vector2 pointerPos = -Vector2.one;
            switch (_playerInput.currentControlScheme)
            {
                case "Keyboard&Mouse":
                    pointerPos = Mouse.current.position.ReadValue();
                    break;

                case "Touch":
                    pointerPos = Touchscreen.current.primaryTouch.position.ReadValue();
                    break;
            }

            Vector2 targetDir = Camera.main.ScreenToWorldPoint(pointerPos) - transform.position;
            _targetAngle = Vector2.SignedAngle(_startDir, targetDir);
        }

        float lerpedAngle = Mathf.LerpAngle(transform.rotation.eulerAngles.z, _targetAngle, angleLerp * Time.deltaTime);
        transform.rotation = Quaternion.Euler(new Vector3(0, 0, lerpedAngle));
    }

    void GenerateMeshAndCollider()
    {
        Mesh mesh = new Mesh();
        mesh.name = "ArcMesh";

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        Vector2[] colliderPoints = GenerateArcPoints(radius, thickness, angle, segments);

        // Create outer and inner arc vertices
        float innerRadius = radius - thickness;
        int vertexCount = segments + 1;

        for (int i = 0; i <= segments; i++)
        {
            float t = i / (float)segments;
            float currentAngle = Mathf.Deg2Rad * Mathf.Lerp(0, angle, t);
            float cos = Mathf.Cos(currentAngle);
            float sin = Mathf.Sin(currentAngle);

            // Outer vertex
            vertices.Add(new Vector3(cos * radius, sin * radius, 0));

            // Inner vertex
            vertices.Add(new Vector3(cos * innerRadius, sin * innerRadius, 0));
        }

        // Create triangles
        for (int i = 0; i < segments; i++)
        {
            int outer0 = i * 2;
            int inner0 = i * 2 + 1;
            int outer1 = (i + 1) * 2;
            int inner1 = (i + 1) * 2 + 1;

            // Triangle 1
            triangles.Add(outer0);
            triangles.Add(outer1);
            triangles.Add(inner1);

            // Triangle 2
            triangles.Add(outer0);
            triangles.Add(inner1);
            triangles.Add(inner0);
        }

        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);

        // Optional (for lighting or shaders)
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        _meshFilter.sharedMesh = mesh;

        // Update collider
        _polyCollider.pathCount = 1;
        _polyCollider.SetPath(0, colliderPoints);
    }

    Vector2[] GenerateArcPoints(float radius, float thickness, float angle, int segments)
    {
        float innerRadius = radius - thickness;
        int halfSegments = Mathf.Max(2, segments);

        List<Vector2> points = new List<Vector2>();

        // Outer arc
        for (int i = 0; i <= halfSegments; i++)
        {
            float t = i / (float)halfSegments;
            float currentAngle = Mathf.Deg2Rad * Mathf.Lerp(0, angle, t);
            float x = Mathf.Cos(currentAngle) * radius;
            float y = Mathf.Sin(currentAngle) * radius;
            points.Add(new Vector2(x, y));
        }

        // Inner arc (reverse)
        for (int i = halfSegments; i >= 0; i--)
        {
            float t = i / (float)halfSegments;
            float currentAngle = Mathf.Deg2Rad * Mathf.Lerp(0, angle, t);
            float x = Mathf.Cos(currentAngle) * innerRadius;
            float y = Mathf.Sin(currentAngle) * innerRadius;
            points.Add(new Vector2(x, y));
        }

        return points.ToArray();
    }

    public void SetRotate(bool rotate)
    {
        _rotate = rotate;
    }
}
