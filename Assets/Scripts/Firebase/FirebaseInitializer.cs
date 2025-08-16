using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase.Auth;
using Firebase.Extensions;
using Firebase.Firestore;
using UnityEngine;

public class FirebaseInitializer : MonoBehaviour
{
    public static FirebaseInitializer Instance { get; private set; }
    public FirebaseAuth auth;
    public FirebaseUser User;

    [SerializeField] private GameObject LoadingScreen;

    public FirebaseFirestore db;

    public UserData userData;

    public Action<Reward> OnRewardUpdate;

    public UIBinder ui;

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadingScreen.SetActive(true);

        OnRewardUpdate += OnRewardWin;
    }

    private async void Start()
    {
        await InitializeFirebase();
    }

    async Task InitializeFirebase()
    {
        auth = Firebase.Auth.FirebaseAuth.DefaultInstance;
        auth.StateChanged += AuthStateChanged;

        await OnAnonymousLogIn();

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

                _=InitializeFirestore();

            }
        }
    }

    private async Task OnAnonymousLogIn()
    {
        try
        {
            var result = await auth.SignInAnonymouslyAsync();
            Debug.Log($"User signed in successfully: {result.User.DisplayName} ({result.User.UserId})");
        }
        catch (Exception ex)
        {
            Debug.LogError("SignInAnonymouslyAsync error: " + ex);
        }
    }

    private async Task InitializeFirestore()
    {
        db = FirebaseFirestore.DefaultInstance;

        try
        {
            await  GetUserData();
        }
        catch (Exception)
        {

            await SetUserData();
        }

    }

    private async Task SetUserData()
    {

        if (auth.CurrentUser == null)
        {
            Debug.LogError("SetUserData called but CurrentUser is null!");
            return;
        }

        Debug.Log("Creating user doc for UserId " + auth.CurrentUser.UserId);

        DocumentReference docRef = db.Collection("users").Document(auth.CurrentUser.UserId);

        if (userData == null)
        {
            userData = new UserData();
        }

        try
        {
            await docRef.SetAsync(userData);
            Debug.Log($"Added data to the users document ({auth.CurrentUser.UserId}).");

            LoadingScreen.SetActive(false);
            ui.SetWallet(userData.golds, userData.gems);
        }
        catch (Exception ex)
        {
            Debug.LogError("SetUserData failed: " + ex);
        }

    }

    public async Task GetUserData()
    {

        if (auth.CurrentUser == null)
        {
            Debug.LogError("GetUserData called but CurrentUser is null!");
            return;
        }

        DocumentReference docRef = db.Collection("users").Document(auth.CurrentUser.UserId);

        DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();
        if (snapshot.Exists)
        {
            Debug.Log($"Document data for {snapshot.Id}:");
            userData = snapshot.ConvertTo<UserData>();
            Debug.Log("amount = " + userData.gems);

            LoadingScreen.SetActive(false);

            ui.SetWallet(userData.golds, userData.gems);
        }
        else
        {
            Debug.Log($"Document {snapshot.Id} does not exist. Creating new.");
            await SetUserData();
        }
    }

    private void OnRewardWin(Reward reward)
    {
        userData.gems += reward.currency == RewardType.Gem ? reward.amount : 0;
        userData.golds += reward.currency == RewardType.Gold ? reward.amount : 0;

        _ =SetUserData();
    }

    void OnDestroy()
    {
        OnRewardUpdate -= OnRewardWin;

        auth.StateChanged -= AuthStateChanged;
        auth = null;
    }
}
