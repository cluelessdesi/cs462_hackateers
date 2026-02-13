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
        messageText.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (messageText.gameObject.activeSelf && Time.time >= hideAt)
            messageText.gameObject.SetActive(false);
    }

    public void ShowLocal(string msg)
    {
        messageText.text = msg;
        messageText.gameObject.SetActive(true);
        hideAt = Time.time + showSeconds;
    }

    public void ShowLocalQ(string msg)
    {
        messageText.text = msg;
        messageText.gameObject.SetActive(true);
        hideAt = 1000000;
    }

    public void HideLocal()
    {
        messageText.gameObject.SetActive(false);
    }
}
