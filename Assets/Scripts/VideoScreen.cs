using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class VideoScreen : MonoBehaviour
{
    public enum ScreenType
    {
        Sphere,
        Fisheye,
        Equirect,
        Flat
    }

    [Header("Screen Settings")]
    [SerializeField] private ScreenType _screenType = ScreenType.Sphere;

    [Header("Sphere Settings")]
    [SerializeField] private int _sphereSegments = 64;
    [SerializeField] private float _sphereRadius = 100f;

    [Header("Fisheye Settings")]
    [SerializeField] private int _fisheyeSegments = 64;
    [SerializeField] private float _fisheyeRadius = 100f;
    [SerializeField] [Range(0f, 1f)] private float _fisheyeCoverage = 0.5f;

    [Header("Equirect 180 Settings")]
    [SerializeField] private int _equirectSegments = 64;
    [SerializeField] private float _equirectRadius = 100f;

    [Header("Flat Settings")]
    [SerializeField] private float _flatWidth = 4f;
    [SerializeField] private float _flatHeight = 2.25f;
    [SerializeField] private float _flatDistance = 3f;

    [Header("Other")]
    [SerializeField] private Renderer Renderer;

    private MeshFilter _meshFilter;
    private ScreenType _lastScreenType;

    private const string KeywordAlphaPacking = "ALPHA_PACKING_ENABLED";
    private bool _isPassthroughEnabled = false;

    public ScreenType CurrentScreenType => _screenType;

    private void Awake()
    {
        _meshFilter = GetComponent<MeshFilter>();
        GenerateMesh();
        _lastScreenType = _screenType;
    }

    private void OnValidate()
    {
        if (_meshFilter == null)
            _meshFilter = GetComponent<MeshFilter>();

        if (_meshFilter != null && _lastScreenType != _screenType)
        {
            GenerateMesh();
            _lastScreenType = _screenType;
        }
    }

    public void SetScreenType(ScreenType type)
    {
        if (_screenType == type) return;

        _screenType = type;
        GenerateMesh();
        ApplyRotationForScreenType();
        _lastScreenType = type;
    }

    private void ApplyRotationForScreenType()
    {
        switch (_screenType)
        {
            case ScreenType.Sphere:
                transform.localRotation = Quaternion.Euler(-90f, 180f, 0f);
                break;
            case ScreenType.Fisheye:
                transform.localRotation = Quaternion.Euler(-90f, 180f, 0f);
                break;
            case ScreenType.Equirect:
                transform.localRotation = Quaternion.Euler(0f, 90f, 0f);
                break;
            case ScreenType.Flat:
                transform.localRotation = Quaternion.identity;
                break;
        }
    }

    public void GenerateMesh()
    {
        if (_meshFilter == null)
            _meshFilter = GetComponent<MeshFilter>();

        IScreenMeshGenerator generator = CreateGenerator();
        _meshFilter.mesh = generator.Generate();
    }

    private IScreenMeshGenerator CreateGenerator()
    {
        return _screenType switch
        {
            ScreenType.Sphere => new SphereMeshGenerator(_sphereSegments, _sphereRadius),
            ScreenType.Fisheye => new FisheyeMeshGenerator(_fisheyeSegments, _fisheyeRadius, _fisheyeCoverage),
            ScreenType.Equirect => new EquirectMeshGenerator(_equirectSegments, _equirectRadius),
            ScreenType.Flat => new FlatMeshGenerator(_flatWidth, _flatHeight, _flatDistance),
            _ => new SphereMeshGenerator(_sphereSegments, _sphereRadius)
        };
    }

    public void CycleScreenType()
    {
        int current = (int)_screenType;
        int next = (current + 1) % 4;
        SetScreenType((ScreenType)next);
    }

    public void SwitchPassthrough()
    {
        _isPassthroughEnabled = !_isPassthroughEnabled;
        if (_isPassthroughEnabled)
            Renderer.material.EnableKeyword(KeywordAlphaPacking);
        else
            Renderer.material.DisableKeyword(KeywordAlphaPacking);
    }
}
