using UnityEngine;
using TMPro;

public class HitMessageUI : MonoBehaviour
{
    public static HitMessageUI Instance;

    [SerializeField] private TMP_Text messageText;
    [SerializeField] private float showSeconds = 2f;

    private float hideAt;

    private void Awake()
    {
        Instance = this;
        if (messageText != null) messageText.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (messageText != null && messageText.gameObject.activeSelf && Time.time >= hideAt)
            messageText.gameObject.SetActive(false);
    }

    public void ShowLocal(string msg)
    {
        if (messageText == null) return;
        messageText.text = msg;
        messageText.gameObject.SetActive(true);
        hideAt = Time.time + showSeconds;
    }
}
