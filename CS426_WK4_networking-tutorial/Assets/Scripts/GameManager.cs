using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public int priorityValue = 4;
    
    void Start()
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public int GetPriorityValue()
    {
        
        return priorityValue;
    }

    public void SubtractPriorityValue()
    {
        priorityValue -= 1;
    }
}
