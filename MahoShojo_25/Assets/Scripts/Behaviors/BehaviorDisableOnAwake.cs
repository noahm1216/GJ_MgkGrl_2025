using UnityEngine;

public class BehaviorDisableOnAwake : MonoBehaviour
{
    public bool waitUntilStart;
    public bool waitUntilUpdate;

    private void Awake()
    {
        if (waitUntilStart || waitUntilUpdate)
            return;

        gameObject.SetActive(false);
    }
    // Start is called before the first frame update
    void Start()
    {
        if (waitUntilUpdate)
            gameObject.SetActive(false);   
    }

    private void Update()
    {
        gameObject.SetActive(false);
    }

}
