using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIBinder : MonoBehaviour
{
    public Button spinButton;
    public TMP_Text cooldownText;
    public TMP_Text goldText, gemsText;

    public void SetButtonInteractable(bool v) => spinButton.interactable = v;
    public void SetCooldown(string s) => cooldownText.text = s;
    public void SetWallet(long gold, long gems)
    {
        goldText.text = gold.ToString();
        gemsText.text = gems.ToString();
    }

    public void HighLighReward(GameObject rewardPrefab)
    {
        var obj = Instantiate(rewardPrefab, this.transform);

        obj.GetComponent<RectTransform>().SetLocalPositionAndRotation(Vector3.zero, Quaternion.Euler(0, 0, 0));

        obj.transform.DOScale(Vector3.one * 4f, 2f).OnComplete(() =>
        {
            Destroy(obj, 1f);
        });
    }
}
