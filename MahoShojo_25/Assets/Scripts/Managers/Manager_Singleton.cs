using UnityEngine;

/// <summary>
/// <para> a singleton class that all singletons will derrive from</para>
/// </summary>
public class Manager_Singleton : MonoBehaviour
{
    public static Manager_Singleton Instance { get; private set; }

    private void Awake()
    {
        // If there is an instance, and it's not me, delete myself.
        if (Instance != null && Instance != this)
            Destroy(this);
        else
            Instance = this;
    }
}
