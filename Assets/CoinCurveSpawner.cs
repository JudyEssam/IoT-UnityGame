using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;

public class CoinCurveSpawner : MonoBehaviour
{
    public enum CurveType
    {
        None,
        Sine,
        Cosine,
        Wave,
        Zigzag,
        Spiral
    }

    [Header("Spline Settings")]
    public SplineContainer spline;
    public GameObject coinPrefab;
    public int coinCount = 50;

    [Header("Curve Settings")]
    public CurveType curveType = CurveType.Sine;
    public float lateralOffset = 1.5f;
    [Tooltip("Number of complete waves/cycles along the spline")]
    public float frequency = 2f;
    [Tooltip("Additional amplitude multiplier")]
    public float amplitude = 1f;
    [Tooltip("Phase shift for the curve (0-1)")]
    [Range(0f, 1f)]
    public float phaseShift = 0f;

    void Start()
    {
        if (!spline || !coinPrefab) return;

        for (int i = 0; i < coinCount; i++)
        {
            float t = (coinCount == 1) ? 0f : i / (float)(coinCount - 1);

            spline.Evaluate(
                t,
                out float3 posF,
                out float3 tangentF,
                out float3 upF
            );

            // Compute right direction (float3 math only)
            float3 rightF = math.normalize(math.cross(upF, tangentF));

            // Calculate curve based on type
            float curve = CalculateCurve(t);
            posF += rightF * curve * lateralOffset;

            // Convert ONCE to Vector3
            Vector3 position = (Vector3)posF + Vector3.up;
            Quaternion baseRotation = Quaternion.LookRotation((Vector3)tangentF, (Vector3)upF);
            Quaternion correction = Quaternion.Euler(0, 90, 90); // adjust axis to match your model
            Quaternion rotation = baseRotation * correction;

            Instantiate(coinPrefab, position, rotation, transform);
        }
    }

    float CalculateCurve(float t)
    {
        float adjustedT = (t + phaseShift) * math.PI * frequency;

        switch (curveType)
        {
            case CurveType.None:
                return 0f;

            case CurveType.Sine:
                return math.sin(adjustedT) * amplitude;

            case CurveType.Cosine:
                return math.cos(adjustedT) * amplitude;

            case CurveType.Wave:
                // Combination of sine waves for a more complex pattern
                return (math.sin(adjustedT) + math.sin(adjustedT * 2f) * 0.5f) * amplitude;

            case CurveType.Zigzag:
                // Triangle wave
                return (2f * math.abs(2f * ((adjustedT / (2f * math.PI)) - math.floor((adjustedT / (2f * math.PI)) + 0.5f))) - 1f) * amplitude;

            case CurveType.Spiral:
                // Gradually increasing amplitude
                return math.sin(adjustedT) * t * amplitude;

            default:
                return 0f;
        }
    }
}