using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class SplineGrowthController : MonoBehaviour
{
    public static SplineGrowthController Instance { get; private set; }

    [Header("Growth Settings")]
    [Tooltip("단위 거리(1 unit)당 소모되는 MP")]
    [SerializeField] private float mpCostPerUnit = 10f;

    [Tooltip("Spline 포인트 사이의 최소 거리. 낮을수록 촘촘한 곡선")]
    [SerializeField] private float minPointDistance = 0.3f;

    [Tooltip("Ground 레이어 번호 (EdgeCollider2D에 설정)")]
    [SerializeField] private int groundLayerIndex = 6;

    [Tooltip("Spline의 두께 (Open Spline에서 시각적 굵기를 결정)")]
    [SerializeField] private float splineHeight = 0.5f;

    [Header("Shrink Settings")]
    [Tooltip("성장 완료 후 줄어들기 시작할 때까지 대기 시간(초)")]
    [SerializeField] private float shrinkDelay = 3f;

    [Tooltip("끝점에서 시작점까지 완전히 줄어드는 데 걸리는 시간(초)")]
    [SerializeField] private float shrinkDuration = 2f;

    [Header("SpriteShape")]
    [Tooltip("SpriteShapeController가 설정된 Prefab (Profile, Material, Open Ended 등 미리 설정)")]
    [SerializeField] private SpriteShapeController shapePrefab;

    [Header("References")]
    [Tooltip("PlayerBase 참조 (MP 소모용)")]
    [SerializeField] private PlayerBase player;

    public bool IsGrowing { get; private set; }

    private Camera mainCamera;
    private GameObject currentGrowthObject;
    private SpriteShapeController currentShape;
    private List<Vector2> splinePoints = new List<Vector2>();

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        mainCamera = Camera.main;
    }

    public void StartGrowth(Vector2 origin)
    {
        if (player == null) return;

        if (player.CurrentMp <= 0f) return;

        IsGrowing = true;
        splinePoints.Clear();

        currentShape = Instantiate(shapePrefab);
        currentGrowthObject = currentShape.gameObject;
        currentGrowthObject.name = "Growth_Vine";
        currentGrowthObject.transform.position = Vector3.zero;

        Spline spline = currentShape.spline;
        spline.Clear();
        spline.InsertPointAt(0, (Vector3)origin);
        spline.SetTangentMode(0, ShapeTangentMode.Continuous);
        spline.SetHeight(0, splineHeight);

        splinePoints.Add(origin);
    }

    private void Update()
    {
        if (!IsGrowing) return;

        if (Input.GetMouseButtonUp(0))
        {
            FinishGrowth();
            return;
        }

        Vector2 mouseWorld = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector2 lastPoint = splinePoints[splinePoints.Count - 1];
        float distance = Vector2.Distance(mouseWorld, lastPoint);

        if (distance >= minPointDistance)
        {
            float mpCost = distance * mpCostPerUnit;

            if (player.CurrentMp < mpCost)
            {
                FinishGrowth();
                return;
            }

            player.CurrentMp -= mpCost;
            AddSplinePoint(mouseWorld);
        }
    }

    private void AddSplinePoint(Vector2 point)
    {
        splinePoints.Add(point);

        Spline spline = currentShape.spline;
        int index = spline.GetPointCount();
        spline.InsertPointAt(index, (Vector3)point);
        spline.SetTangentMode(index, ShapeTangentMode.Continuous);
        spline.SetHeight(index, splineHeight);

        AutoCalculateTangent(spline, index);

        if (index >= 2)
            AutoCalculateTangent(spline, index - 1);
    }

    private void AutoCalculateTangent(Spline spline, int index)
    {
        int count = spline.GetPointCount();
        if (count < 2) return;

        Vector3 prev = index > 0
            ? spline.GetPosition(index - 1)
            : spline.GetPosition(index);

        Vector3 next = index < count - 1
            ? spline.GetPosition(index + 1)
            : spline.GetPosition(index);

        Vector3 tangent = (next - prev) * 0.25f;

        spline.SetLeftTangent(index, -tangent);
        spline.SetRightTangent(index, tangent);
    }

    private IEnumerator ShrinkAndDestroy(SpriteShapeController shape, Vector2[] points)
    {
        yield return new WaitForSeconds(shrinkDelay);

        EdgeCollider2D edge = shape.GetComponent<EdgeCollider2D>();
        int totalPoints = points.Length;
        float elapsed = 0f;

        while (elapsed < shrinkDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / shrinkDuration;

            // 끝에서부터 제거할 포인트 수 계산
            int remainCount = Mathf.Max(2, totalPoints - Mathf.FloorToInt(t * (totalPoints - 1)));

            Spline spline = shape.spline;
            // 현재 포인트가 남아야 할 수보다 많으면 끝에서 제거
            while (spline.GetPointCount() > remainCount)
            {
                spline.RemovePointAt(spline.GetPointCount() - 1);
            }

            // EdgeCollider도 동기화
            if (edge != null)
            {
                Vector2[] shrunkPoints = new Vector2[remainCount];
                System.Array.Copy(points, shrunkPoints, remainCount);
                edge.points = shrunkPoints;
            }

            yield return null;
        }

        Destroy(shape.gameObject);
    }

    private void FinishGrowth()
    {
        IsGrowing = false;

        if (currentGrowthObject == null || splinePoints.Count < 2) return;

        EdgeCollider2D edge = currentGrowthObject.AddComponent<EdgeCollider2D>();
        edge.points = splinePoints.ToArray();
        currentGrowthObject.layer = groundLayerIndex;

        StartCoroutine(ShrinkAndDestroy(currentShape, splinePoints.ToArray()));

        currentGrowthObject = null;
        currentShape = null;
    }
}
