using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class SpinWheelController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Transform wheel;
    [SerializeField] private UIBinder ui;
    [SerializeField] private RewardConfig rewardConfig;
    
    [Header("Spin Settings")]
    [SerializeField] private int segmentCount = 8;
    [SerializeField] private float minSpinSpeed = 720f;   
    [SerializeField] private float maxSpinSpeed = 1080f;  
    [SerializeField] private int extraRotations = 4;     
    [SerializeField] private float slowDownDuration = 3f; 

    private bool spinning;
    private float currentAngle;
    private Coroutine countdownCo;

    // Data Cache
    private long gold, gems;
    private long spinCount, cooldownMinutes;
    private System.DateTime nextAvailableUtc;

    private void Awake()
    {
        ui.spinButton.onClick.AddListener(OnSpinPressed);
    }

    private void OnDisable()
    {
        ui.spinButton.onClick.RemoveAllListeners();
    }

    public async void OnSpinPressed()
    {
        if (spinning) return;

        if (System.DateTime.UtcNow < nextAvailableUtc) return;

        spinning = true;
        ui.SetButtonInteractable(false);

        float spinSpeed = UnityEngine.Random.Range(minSpinSpeed, maxSpinSpeed);
        bool resultArrived = false;
        int resultIndex = -1;

        var apiTask = GetRandomIndexFromAPI();

        while (!apiTask.IsCompleted)
        {
            currentAngle += spinSpeed * Time.deltaTime;
            wheel.localEulerAngles = new Vector3(0, 0, -currentAngle);
            await Task.Yield();
        }

        if (apiTask.IsFaulted || apiTask.IsCanceled)
        {
            Debug.LogError("Connection error !");
            resultIndex = 0;
        }
        else
        {
            resultIndex = apiTask.Result % segmentCount;
        }

        resultArrived = true;

        float finalAngle = ComputeFinalAngle(currentAngle, resultIndex);
        await SmoothTo(finalAngle, slowDownDuration);

        var reward = rewardConfig.rewards[resultIndex];

        Debug.Log("REWARD: " + reward.currency.ToString() + " Amount: " + reward.amount.ToString());

        ui.HighLighReward(reward.display);
        
        nextAvailableUtc = System.DateTime.UtcNow.AddMinutes((double)cooldownMinutes);

        await Task.Delay(500);

        if (countdownCo != null) StopCoroutine(countdownCo);
        countdownCo = StartCoroutine(CooldownCountdown());

        spinning = false;
        ui.SetButtonInteractable(false); 
    }

    IEnumerator CooldownCountdown()
    {
        while (true)
        {
            var now = System.DateTime.UtcNow;
            if (now >= nextAvailableUtc)
            {
                ui.SetCooldown("Ready");
                ui.SetButtonInteractable(!spinning);
                yield break;
            }
            var remain = nextAvailableUtc - now;
            ui.SetCooldown($"{remain.Minutes:D2}:{remain.Seconds:D2}");
            yield return new WaitForSeconds(0.5f);
        }
    }

    float ComputeFinalAngle(float fromAngle, int resultIndex)
    {
        float segAngle = 360f / segmentCount;
        float center = resultIndex * segAngle + segAngle * 0.5f;

        float targetMod = (360f - center) % 360f;

        float currentMod = Mathf.Repeat(fromAngle, 360f);

        float delta = Mathf.Repeat(targetMod - currentMod, 360f);
        if (delta < 1f) delta += 360f; 

        float final = fromAngle + delta + extraRotations * 360f;
        return final;
    }
    async Task SmoothTo(float finalAngle, float duration)
    {
        float start = currentAngle;
        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / duration;
            float e = EaseOutCubic(t);
            currentAngle = Mathf.Lerp(start, finalAngle, e);
            wheel.localEulerAngles = new Vector3(0, 0, -currentAngle);
            await Task.Yield();
        }
        currentAngle = finalAngle;
        wheel.localEulerAngles = new Vector3(0, 0, -currentAngle);
    }

    float EaseOutCubic(float x) => 1f - Mathf.Pow(1f - x, 3f);

    async Task<int> GetRandomIndexFromAPI()
    {
        string url = "http://www.randomnumberapi.com/api/v1.0/random?min=0&max=8&count=1";
        using (UnityWebRequest req = UnityWebRequest.Get(url))
        {
            var op = req.SendWebRequest();
            while (!op.isDone) await Task.Yield();

            if (req.result != UnityWebRequest.Result.Success)
                throw new System.Exception("Random API error: " + req.error);

            string json = req.downloadHandler.text.Trim();

            if (json.Length >= 3 && json[0] == '[' && json[^1] == ']')
            {
                var inner = json.Substring(1, json.Length - 2);
                if (int.TryParse(inner, out int idx))
                    return idx;
            }
            throw new System.Exception("Random API unexpected: " + json);
        }
    }

}
