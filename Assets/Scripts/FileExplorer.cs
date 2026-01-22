using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FileExplorer : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Button _backButton;
    [SerializeField] private Button _testStreamButton;
    [SerializeField] private TextMeshProUGUI _pathText;
    [SerializeField] private RectTransform _scrollContent;
    [SerializeField] private GameObject _fileEntryPrefab;

    [Header("Settings")]
    [SerializeField] private string _rootPath = "";
    [SerializeField] private Color _folderColor = new Color(1f, 0.9f, 0.5f);
    [SerializeField] private Color _fileColor = Color.white;

    private const string TestStreamUrl = "https://stream.mux.com/4XYzhPXzqArkFI8d1vDsScBLD69Gh1b2.m3u8";

    private string _currentPath;

    public event Action<string> OnFileSelected;

    private void Start()
    {
        // Setup back button
        if (_backButton != null)
        {
            _backButton.onClick.AddListener(NavigateUp);
        }

        // Setup test stream button
        if (_testStreamButton != null)
        {
            _testStreamButton.onClick.AddListener(PlayTestStream);
        }

        // Set initial path
        if (string.IsNullOrEmpty(_rootPath))
        {
            _rootPath = Application.persistentDataPath;
        }

        NavigateTo(_rootPath);
    }

    private void PlayTestStream()
    {
        OnFileSelected?.Invoke(TestStreamUrl);
    }

    public void NavigateTo(string path)
    {
        if (!Directory.Exists(path)) return;

        _currentPath = path;

        // Update path text
        if (_pathText != null)
        {
            _pathText.text = path;
        }

        // Clear current entries
        ClearScrollContent();

        // Populate with new entries
        PopulateEntries(path);
    }

    public void NavigateUp()
    {
        if (string.IsNullOrEmpty(_currentPath)) return;

        string parent = Path.GetDirectoryName(_currentPath);
        if (!string.IsNullOrEmpty(parent) && Directory.Exists(parent))
        {
            NavigateTo(parent);
        }
    }

    private void ClearScrollContent()
    {
        if (_scrollContent == null) return;

        for (int i = _scrollContent.childCount - 1; i >= 0; i--)
        {
            Destroy(_scrollContent.GetChild(i).gameObject);
        }
    }

    private void PopulateEntries(string path)
    {
        if (_scrollContent == null || _fileEntryPrefab == null) return;

        try
        {
            // Add directories first
            string[] directories = Directory.GetDirectories(path);
            Array.Sort(directories);

            foreach (string dir in directories)
            {
                string name = Path.GetFileName(dir);
                if (name.StartsWith(".")) continue;

                CreateEntry($"[{name}]", dir, true);
            }

            // Add files
            string[] files = Directory.GetFiles(path);
            Array.Sort(files);
            Debug.Log($"666 dirs: {string.Join(",", directories)} files: {string.Join(",", files)}");
            foreach (string file in files)
            {
                string name = Path.GetFileName(file);
                if (name.StartsWith(".")) continue;

                CreateEntry($"   {name}", file, false);
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"666 Cannot read directory: {path} - {e.Message}");
        }
    }

    private void CreateEntry(string displayName, string fullPath, bool isFolder)
    {
        GameObject entry = Instantiate(_fileEntryPrefab, _scrollContent);

        // Set text
        TextMeshProUGUI text = entry.GetComponentInChildren<TextMeshProUGUI>();
        if (text != null)
        {
            text.text = displayName;
            text.color = isFolder ? _folderColor : _fileColor;
        }

        // Setup button click
        Button button = entry.GetComponent<Button>();
        if (button != null)
        {
            string capturedPath = fullPath;
            bool capturedIsFolder = isFolder;

            button.onClick.AddListener(() => OnEntryClicked(capturedPath, capturedIsFolder));
        }
    }

    private void OnEntryClicked(string path, bool isFolder)
    {
        if (isFolder)
        {
            NavigateTo(path);
        }
        else
        {
            OnFileSelected?.Invoke(path);
        }
    }

    public void SetRootPath(string path)
    {
        _rootPath = path;
        NavigateTo(path);
    }

    public void Refresh()
    {
        if (!string.IsNullOrEmpty(_currentPath))
        {
            NavigateTo(_currentPath);
        }
    }

    public string CurrentPath => _currentPath;
}
