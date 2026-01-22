using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using RenderHeads.Media.AVProVideo;

public class AppStateManager : MonoBehaviour
{
    public enum AppState
    {
        FileExplorer,
        VideoPlayer
    }

    [Header("References")]
    [SerializeField] private GameObject _fileExplorerUI;
    [SerializeField] private GameObject _videoPlayerUI;
    [SerializeField] private GameObject _screen;

    [SerializeField] private FileExplorer _fileExplorer;
    [SerializeField] private MediaPlayer _mediaPlayer;

    [Header("UI Buttons")]
    [SerializeField] private Button _returnButton;

    [Header("Input")]
    [SerializeField] private InputActionReference _backAction;

    private AppState _currentState = AppState.FileExplorer;

    public AppState CurrentState => _currentState;

    private void OnEnable()
    {
        if (_fileExplorer != null)
        {
            _fileExplorer.OnFileSelected += OnFileSelected;
        }

        if (_returnButton != null)
        {
            _returnButton.onClick.AddListener(GoToFileExplorer);
        }

        if (_backAction != null && _backAction.action != null)
        {
            _backAction.action.Enable();
            _backAction.action.performed += OnBackPressed;
        }
    }

    private void OnDisable()
    {
        if (_fileExplorer != null)
        {
            _fileExplorer.OnFileSelected -= OnFileSelected;
        }

        if (_returnButton != null)
        {
            _returnButton.onClick.RemoveListener(GoToFileExplorer);
        }

        if (_backAction != null && _backAction.action != null)
        {
            _backAction.action.performed -= OnBackPressed;
        }
    }

    private void Start()
    {
        SetState(AppState.FileExplorer);
    }

    public void SetState(AppState newState)
    {
        _currentState = newState;

        switch (newState)
        {
            case AppState.FileExplorer:
                ShowFileExplorer();
                break;
            case AppState.VideoPlayer:
                ShowVideoPlayer();
                break;
        }
    }

    private void ShowFileExplorer()
    {
        if (_fileExplorerUI != null)
            _fileExplorerUI.SetActive(true);

        if (_videoPlayerUI != null)
            _videoPlayerUI.SetActive(false);

        // Stop video when returning to file explorer
        if (_mediaPlayer != null && _mediaPlayer.Control != null)
        {
            _mediaPlayer.Stop();
            _mediaPlayer.gameObject.SetActive(false);
        }

        if (_screen != null)
            _screen.SetActive(false);
    }

    private void ShowVideoPlayer()
    {
        if (_mediaPlayer != null)
            _mediaPlayer.gameObject.SetActive(true);

        if (_screen != null)
            _screen.SetActive(true);

        if (_fileExplorerUI != null)
            _fileExplorerUI.SetActive(false);

        if (_videoPlayerUI != null)
            _videoPlayerUI.SetActive(true);
    }

    private void OnFileSelected(string filePath)
    {
        if (_mediaPlayer != null)
        {
            _mediaPlayer.OpenMedia(new MediaPath(filePath, MediaPathType.AbsolutePathOrURL), autoPlay: true);
        }

        SetState(AppState.VideoPlayer);
    }

    private void OnBackPressed(InputAction.CallbackContext context)
    {
        if (_currentState == AppState.VideoPlayer)
        {
            SetState(AppState.FileExplorer);
        }
    }

    // Public methods for manual state changes
    public void GoToFileExplorer()
    {
        SetState(AppState.FileExplorer);
    }

    public void GoToVideoPlayer()
    {
        SetState(AppState.VideoPlayer);
    }
}
