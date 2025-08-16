using Firebase.Auth;
using UnityEngine;

public class FirebaseInitializer : MonoBehaviour
{
    public static FirebaseInitializer Instance { get; private set; }
    public FirebaseAuth auth { get; private set; }
    public FirebaseUser User { get; private set; }

    [SerializeField] private GameObject LoadingScreen;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadingScreen.SetActive(true);
    }

    private void Start()
    {
        InitializeFirebase();
    }

    void InitializeFirebase()
    {
        auth = Firebase.Auth.FirebaseAuth.DefaultInstance;
        auth.StateChanged += AuthStateChanged;
        AuthStateChanged(this, null);
    }

    void AuthStateChanged(object sender, System.EventArgs eventArgs)
    {
        if (auth.CurrentUser != User)
        {
            bool signedIn = User != auth.CurrentUser && auth.CurrentUser != null
                && auth.CurrentUser.IsValid();
            if (!signedIn && User != null)
            {
                Debug.Log("Signed out " + User.UserId);
            }
            User = auth.CurrentUser;
            if (signedIn)
            {
                Debug.Log("Signed in " + User.UserId);
                OnAnonymousLogIn();
                LoadingScreen.SetActive(false);
            }
        }
    }

    private void OnAnonymousLogIn()
    {
        auth.SignInAnonymouslyAsync().ContinueWith(task => {
            if (task.IsCanceled)
            {
                Debug.LogError("SignInAnonymouslyAsync was canceled.");
                return;
            }
            if (task.IsFaulted)
            {
                Debug.LogError("SignInAnonymouslyAsync encountered an error: " + task.Exception);
                return;
            }

            Firebase.Auth.AuthResult result = task.Result;
            Debug.LogFormat("User signed in successfully: {0} ({1})",
                result.User.DisplayName, result.User.UserId);
        });
    }

    void OnDestroy()
    {
        auth.StateChanged -= AuthStateChanged;
        auth = null;
    }
}
