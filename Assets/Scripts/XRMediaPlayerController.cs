using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using RenderHeads.Media.AVProVideo;
using RenderHeads.Media.AVProVideo.Demos.UI;
using TMPro;

/// <summary>
/// XR/VR compatible media player controller for AVPro Video.
/// Works with XR Interaction Toolkit - no EventSystem required.
/// Automatically adds XR interactables to referenced UI elements.
/// Uses MediaPlayerUI material properties for button visuals.
/// </summary>
public class XRMediaPlayerController : MonoBehaviour
{
    [Header("Media Player")]
    [SerializeField] private MediaPlayer _mediaPlayer;
    [SerializeField] private VideoScreen _videoScreen;
    [SerializeField] private ApplyToMesh _applyToMesh;

    [Header("Options")]
    [SerializeField] private float _jumpDeltaTime = 5f;
    [SerializeField] private float _volumeDelta = 0.1f;
    [SerializeField] private bool _autoHide = false;
    [SerializeField] private float _autoHideDelay = 3f;
    [SerializeField] private float _colliderDepth = 0.01f;

    [Header("UI Components")]
    [SerializeField] private CanvasGroup _controlsGroup;
    [SerializeField] private Slider _sliderTime;
    [SerializeField] private Slider _sliderVolume;
    [SerializeField] private Text _textTimeDuration;
    [SerializeField] private Text _textMediaName;
    [SerializeField] private GameObject _liveItem;

    [Header("Buttons")]
    [SerializeField] private Button _buttonPlayPause;
    [SerializeField] private Button _buttonTimeBack;
    [SerializeField] private Button _buttonTimeForward;
    [SerializeField] private Button _buttonVolumeMute;

    [Header("Settings Dropdowns")]
    [SerializeField] private TMP_Dropdown _dropdownScreenType;
    [SerializeField] private TMP_Dropdown _dropdownStereoType;

    [Header("Timeline Hover (Optional)")]
    [SerializeField] private RectTransform _timelineTip;
    [SerializeField] private Text _timelineTipText;
    [SerializeField] private MediaPlayer _thumbnailMediaPlayer;
    [SerializeField] private RectTransform _canvasTransform;

    [Header("Timeline Segments (Optional)")]
    [SerializeField] private HorizontalSegmentsPrimitive _segmentsSeek;
    [SerializeField] private HorizontalSegmentsPrimitive _segmentsBuffered;
    [SerializeField] private HorizontalSegmentsPrimitive _segmentsProgress;

    // Material instances for button visuals
    private Material _playPauseMaterial;
    private Material _volumeMaterial;

    // Shader property IDs
    private static readonly int PropMorph = Shader.PropertyToID("_Morph");
    private static readonly int PropMute = Shader.PropertyToID("_Mute");
    private static readonly int PropVolume = Shader.PropertyToID("_Volume");

    private float _audioVolume = 1f;
    private bool _isDraggingTimeSlider = false;
    private bool _wasPlayingBeforeDrag = false;
    private float _lastInteractionTime;
    private float _controlsFade = 1f;

    // Timeline drag state
    private float _lastDragTime = 0f;

    private void Start()
    {
        if (_mediaPlayer != null)
        {
            _audioVolume = _mediaPlayer.AudioVolume;
        }

        SetupButtonMaterials();
        SetupXRInteractables();
        SetupButtonListeners();
        SetupSliderListeners();
        UpdateVolumeSlider();

        // Hide timeline tip initially
        if (_timelineTip != null)
            _timelineTip.gameObject.SetActive(false);
        if (_segmentsSeek != null)
            _segmentsSeek.gameObject.SetActive(false);

        _lastInteractionTime = Time.time;
    }

    private void SetupButtonMaterials()
    {
        // Duplicate materials so we don't modify the shared asset
        if (_buttonPlayPause != null)
        {
            Image img = _buttonPlayPause.GetComponent<Image>();
            if (img != null && img.material != null)
            {
                img.material = new Material(img.material);
                _playPauseMaterial = img.material;
            }
        }

        if (_buttonVolumeMute != null)
        {
            Image img = _buttonVolumeMute.GetComponent<Image>();
            if (img != null && img.material != null)
            {
                img.material = new Material(img.material);
                _volumeMaterial = img.material;
            }
        }
    }

    private void SetupXRInteractables()
    {
        // Setup buttons with XR interactables and colliders
        SetupButtonXR(_buttonPlayPause);
        SetupButtonXR(_buttonTimeBack);
        SetupButtonXR(_buttonTimeForward);
        SetupButtonXR(_buttonVolumeMute);

        // Setup sliders with XR interactables
        SetupTimelineSliderXR();
        SetupSliderXR(_sliderVolume);
    }

    private void SetupButtonXR(Button button)
    {
        if (button == null) return;

        // Add collider
        RectTransform rect = button.GetComponent<RectTransform>();
        if (rect != null && button.GetComponent<Collider>() == null)
        {
            BoxCollider col = button.gameObject.AddComponent<BoxCollider>();
            Vector2 size = rect.rect.size;
            col.size = new Vector3(size.x, size.y, _colliderDepth);
        }

        // Add XR interactable
        if (button.GetComponent<XRSimpleInteractable>() == null)
        {
            button.gameObject.AddComponent<XRSimpleInteractable>();
        }
    }

    private void SetupTimelineSliderXR()
    {
        // Timeline uses standard UI slider events via TrackedDeviceGraphicRaycaster
        // No additional XR setup needed
    }

    private void SetupSliderXR(Slider slider)
    {
        if (slider == null) return;

        // Add collider to slider
        RectTransform rect = slider.GetComponent<RectTransform>();
        if (rect != null && slider.GetComponent<Collider>() == null)
        {
            BoxCollider col = slider.gameObject.AddComponent<BoxCollider>();
            Vector2 size = rect.rect.size;
            col.size = new Vector3(size.x, size.y, _colliderDepth);
        }

        // Add XR interactable to slider
        XRSimpleInteractable interactable = slider.GetComponent<XRSimpleInteractable>();
        if (interactable == null)
        {
            interactable = slider.gameObject.AddComponent<XRSimpleInteractable>();
        }
    }

    private void SetupButtonListeners()
    {
        if (_buttonPlayPause != null)
            _buttonPlayPause.onClick.AddListener(OnPlayPauseClicked);

        if (_buttonTimeBack != null)
            _buttonTimeBack.onClick.AddListener(OnTimeBackClicked);

        if (_buttonTimeForward != null)
            _buttonTimeForward.onClick.AddListener(OnTimeForwardClicked);

        if (_buttonVolumeMute != null)
            _buttonVolumeMute.onClick.AddListener(OnVolumeMuteClicked);
    }

    private void SetupSliderListeners()
    {
        if (_sliderTime != null)
            _sliderTime.onValueChanged.AddListener(OnTimeSliderValueChanged);

        if (_sliderVolume != null)
            _sliderVolume.onValueChanged.AddListener(OnVolumeSliderValueChanged);
    }

    #region Button Callbacks

    private void OnPlayPauseClicked()
    {
        RegisterInteraction();
        TogglePlayPause();
    }

    private void OnTimeBackClicked()
    {
        RegisterInteraction();
        SeekRelative(-_jumpDeltaTime);
    }

    private void OnTimeForwardClicked()
    {
        RegisterInteraction();
        SeekRelative(_jumpDeltaTime);
    }

    private void OnVolumeMuteClicked()
    {
        RegisterInteraction();
        ToggleMute();
    }

    #endregion

    #region Timeline Seek

    private void OnTimeSliderValueChanged(float value)
    {
        if (_mediaPlayer == null || _mediaPlayer.Control == null) return;

        RegisterInteraction();
        _lastDragTime = Time.time;

        // Start dragging on first interaction
        if (!_isDraggingTimeSlider)
        {
            _isDraggingTimeSlider = true;
            _wasPlayingBeforeDrag = _mediaPlayer.Control.IsPlaying();
            if (_wasPlayingBeforeDrag)
            {
                _mediaPlayer.Pause();
            }
        }

        // Seek to position
        TimeRange timelineRange = GetTimelineRange();
        double seekTime = timelineRange.startTime + (value * timelineRange.duration);
        _mediaPlayer.Control.SeekFast(seekTime);
    }

    private void UpdateTimelineDrag()
    {
        // End drag after a short timeout (user released slider)
        if (_isDraggingTimeSlider && Time.time - _lastDragTime > 0.1f)
        {
            EndTimelineDrag();
        }
    }

    private void EndTimelineDrag()
    {
        if (!_isDraggingTimeSlider) return;

        _isDraggingTimeSlider = false;

        if (_mediaPlayer != null && _mediaPlayer.Control != null && _wasPlayingBeforeDrag)
        {
            _mediaPlayer.Play();
            _wasPlayingBeforeDrag = false;
        }
    }

    #endregion

    #region Volume Slider

    private void OnVolumeSliderValueChanged(float value)
    {
        RegisterInteraction();
        _audioVolume = value;
        ApplyAudioVolume();
    }

    #endregion

    #region Media Control Methods

    public void TogglePlayPause()
    {
        if (_mediaPlayer == null || _mediaPlayer.Control == null) return;

        if (_mediaPlayer.Control.IsPlaying())
            _mediaPlayer.Pause();
        else
            _mediaPlayer.Play();
    }

    public void SeekRelative(float deltaTime)
    {
        if (_mediaPlayer == null || _mediaPlayer.Control == null) return;

        TimeRange timelineRange = GetTimelineRange();
        double time = _mediaPlayer.Control.GetCurrentTime() + deltaTime;
        time = System.Math.Max(time, timelineRange.startTime);
        time = System.Math.Min(time, timelineRange.startTime + timelineRange.duration);
        _mediaPlayer.Control.Seek(time);
    }

    public void ToggleMute()
    {
        if (_mediaPlayer == null) return;
        _mediaPlayer.AudioMuted = !_mediaPlayer.AudioMuted;
    }

    private void ApplyAudioVolume()
    {
        if (_mediaPlayer != null)
            _mediaPlayer.AudioVolume = _audioVolume;
    }

    private void UpdateVolumeSlider()
    {
        if (_sliderVolume != null)
            _sliderVolume.SetValueWithoutNotify(_audioVolume);
    }

    private TimeRange GetTimelineRange()
    {
        if (_mediaPlayer != null && _mediaPlayer.Info != null)
            return Helper.GetTimelineRange(_mediaPlayer.Info.GetDuration(), _mediaPlayer.Control.GetSeekableTimes());
        return new TimeRange();
    }

    #endregion

    #region Material Visual Updates

    private void UpdatePlayPauseMaterial()
    {
        if (_playPauseMaterial == null || _mediaPlayer == null || _mediaPlayer.Control == null) return;

        // Animate _Morph: 0 = play icon, 1 = pause icon
        float currentMorph = _playPauseMaterial.GetFloat(PropMorph);
        float targetMorph = _mediaPlayer.Control.IsPlaying() ? 0f : 1f;
        float newMorph = Mathf.MoveTowards(currentMorph, targetMorph, Time.deltaTime * 6f);
        _playPauseMaterial.SetFloat(PropMorph, newMorph);
    }

    private void UpdateVolumeMaterial()
    {
        if (_volumeMaterial == null || _mediaPlayer == null) return;

        // Animate _Mute: 0 = unmuted, 1 = muted
        float currentMute = _volumeMaterial.GetFloat(PropMute);
        float targetMute = _mediaPlayer.AudioMuted ? 1f : 0f;
        float newMute = Mathf.MoveTowards(currentMute, targetMute, Time.deltaTime * 6f);
        _volumeMaterial.SetFloat(PropMute, newMute);

        // Set volume level
        _volumeMaterial.SetFloat(PropVolume, _audioVolume);
    }

    #endregion

    #region Auto Hide

    private void RegisterInteraction()
    {
        _lastInteractionTime = Time.time;
    }

    private void UpdateControlsVisibility()
    {
        if (_controlsGroup == null || !_autoHide) return;

        float timeSinceInteraction = Time.time - _lastInteractionTime;

        if (timeSinceInteraction < _autoHideDelay)
        {
            _controlsFade = Mathf.Min(1f, _controlsFade + Time.deltaTime * 4f);
            if (!_controlsGroup.gameObject.activeSelf)
                _controlsGroup.gameObject.SetActive(true);
        }
        else
        {
            _controlsFade = Mathf.Max(0f, _controlsFade - Time.deltaTime * 2f);
            if (_controlsFade <= 0f && _controlsGroup.gameObject.activeSelf)
                _controlsGroup.gameObject.SetActive(false);
        }

        _controlsGroup.alpha = _controlsFade;
    }

    public void ShowControls()
    {
        RegisterInteraction();
    }

    #endregion

    #region Segments Update

    private void UpdateSegments()
    {
        if (_mediaPlayer == null || _mediaPlayer.Control == null || _mediaPlayer.Info == null) return;

        TimeRange timelineRange = GetTimelineRange();

        // Update buffered segments
        if (_segmentsBuffered != null)
        {
            TimeRanges times = _mediaPlayer.Control.GetBufferedTimes();
            float[] ranges = null;
            if (times.Count > 0 && timelineRange.duration > 0.0)
            {
                ranges = new float[times.Count * 2];
                for (int i = 0; i < times.Count; i++)
                {
                    ranges[i * 2 + 0] = Mathf.Max(0f, (float)((times[i].StartTime - timelineRange.startTime) / timelineRange.duration));
                    ranges[i * 2 + 1] = Mathf.Min(1f, (float)((times[i].EndTime - timelineRange.startTime) / timelineRange.duration));
                }
            }
            _segmentsBuffered.Segments = ranges;
        }

        // Update progress segment
        if (_segmentsProgress != null)
        {
            TimeRanges times = _mediaPlayer.Control.GetBufferedTimes();
            float[] ranges = null;
            if (times.Count > 0 && timelineRange.duration > 0.0)
            {
                ranges = new float[2];
                double x1 = (times.MinTime - timelineRange.startTime) / timelineRange.duration;
                double x2 = (_mediaPlayer.Control.GetCurrentTime() - timelineRange.startTime) / timelineRange.duration;
                ranges[0] = Mathf.Max(0f, (float)x1);
                ranges[1] = Mathf.Min(1f, (float)x2);
            }
            _segmentsProgress.Segments = ranges;
        }
    }

    #endregion

    private void Update()
    {
        if (_mediaPlayer == null) return;

        UpdateControlsVisibility();
        UpdatePlayPauseMaterial();
        UpdateVolumeMaterial();
        UpdateTimelineDrag();
        UpdateSegments();

        if (_mediaPlayer.Info != null && _mediaPlayer.Control != null)
        {
            TimeRange timelineRange = GetTimelineRange();

            // Update time slider (only when not dragging)
            if (_sliderTime != null && !_isDraggingTimeSlider)
            {
                double t = 0.0;
                if (timelineRange.duration > 0.0)
                    t = (_mediaPlayer.Control.GetCurrentTime() - timelineRange.startTime) / timelineRange.duration;
                _sliderTime.SetValueWithoutNotify(Mathf.Clamp01((float)t));
            }

            // Update time text
            if (_textTimeDuration != null)
            {
                string currentTime = Helper.GetTimeString(_mediaPlayer.Control.GetCurrentTime() - timelineRange.startTime, false);
                string duration = Helper.GetTimeString(timelineRange.duration, false);
                _textTimeDuration.text = $"{currentTime} / {duration}";
            }

            // Update media name
            if (_textMediaName != null && _mediaPlayer.Info.GetVideoWidth() > 0)
            {
                float fps = _mediaPlayer.Info.GetVideoFrameRate();
                if (fps > 0f && !float.IsNaN(fps))
                    _textMediaName.text = $"{_mediaPlayer.Info.GetVideoWidth()} x {_mediaPlayer.Info.GetVideoHeight()} @ {fps:F2}";
                else
                    _textMediaName.text = $"{_mediaPlayer.Info.GetVideoWidth()} x {_mediaPlayer.Info.GetVideoHeight()}";
            }

            // Update LIVE indicator
            if (_liveItem != null)
                _liveItem.SetActive(double.IsInfinity(_mediaPlayer.Info.GetDuration()));
        }
    }

    public void ScreenTypeChanged()
    {
        if (_videoScreen == null || _dropdownScreenType == null) return;

        RegisterInteraction();
        VideoScreen.ScreenType screenType = (VideoScreen.ScreenType)_dropdownScreenType.value;
        _videoScreen.SetScreenType(screenType);
    }

    public void StereoTypeChanged()
    {
        if (_videoScreen == null || _dropdownStereoType == null) return;

        RegisterInteraction();

        StereoPacking packing = (StereoPacking)_dropdownStereoType.value;

        MeshRenderer renderer = _videoScreen.GetComponent<MeshRenderer>();
        if (renderer != null && renderer.material != null)
        {
            VideoRender.SetupStereoMaterial(renderer.material, packing);
        }
    }
}
