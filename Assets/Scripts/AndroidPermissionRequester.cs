using UnityEngine;
#if UNITY_ANDROID
using UnityEngine.Android;
#endif

/// <summary>
/// Requests necessary Android permissions on app start.
/// For Meta Quest file access.
/// </summary>
public class AndroidPermissionRequester : MonoBehaviour
{
    [SerializeField] private bool _requestOnStart = true;

    private void Start()
    {
        if (_requestOnStart)
        {
            RequestPermissions();
        }
    }

    public void RequestPermissions()
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        RequestStoragePermissions();
#endif
    }

#if UNITY_ANDROID
    private void RequestStoragePermissions()
    {
        // Check and request READ permission
        if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageRead))
        {
            Permission.RequestUserPermission(Permission.ExternalStorageRead);
        }

        // Check and request WRITE permission
        if (!Permission.HasUserAuthorizedPermission(Permission.ExternalStorageWrite))
        {
            Permission.RequestUserPermission(Permission.ExternalStorageWrite);
        }

        if (!Permission.HasUserAuthorizedPermission("android.permission.READ_MEDIA_VIDEO"))
        {
            Permission.RequestUserPermission("android.permission.READ_MEDIA_VIDEO");
        }
    }

    /// <summary>
    /// Opens Android settings for this app to grant "All Files Access" (Android 11+)
    /// </summary>
    public void OpenAllFilesAccessSettings()
    {
        try
        {
            using (var unityClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            using (var currentActivity = unityClass.GetStatic<AndroidJavaObject>("currentActivity"))
            using (var intent = new AndroidJavaObject("android.content.Intent"))
            using (var uri = new AndroidJavaClass("android.net.Uri"))
            {
                string packageName = currentActivity.Call<string>("getPackageName");
                var uriObj = uri.CallStatic<AndroidJavaObject>("parse", "package:" + packageName);

                intent.Call<AndroidJavaObject>("setAction", "android.settings.MANAGE_APP_ALL_FILES_ACCESS_PERMISSION");
                intent.Call<AndroidJavaObject>("setData", uriObj);

                currentActivity.Call("startActivity", intent);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogWarning($"Could not open All Files Access settings: {e.Message}");
        }
    }

    /// <summary>
    /// Check if app has "All Files Access" permission (Android 11+)
    /// </summary>
    public bool HasAllFilesAccess()
    {
        try
        {
            using (var environment = new AndroidJavaClass("android.os.Environment"))
            {
                return environment.CallStatic<bool>("isExternalStorageManager");
            }
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Get Android SDK version
    /// </summary>
    public int GetAndroidSDKVersion()
    {
        try
        {
            using (var version = new AndroidJavaClass("android.os.Build$VERSION"))
            {
                return version.GetStatic<int>("SDK_INT");
            }
        }
        catch
        {
            return 0;
        }
    }

    public bool HasStoragePermissions()
    {
        return Permission.HasUserAuthorizedPermission(Permission.ExternalStorageRead) &&
               Permission.HasUserAuthorizedPermission(Permission.ExternalStorageWrite);
    }
#endif
}
