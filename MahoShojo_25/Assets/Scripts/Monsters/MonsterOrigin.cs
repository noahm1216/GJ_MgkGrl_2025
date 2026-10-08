using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// <para> The core of all monster behavior. Holds their stats and data </para>
/// </summary>
public abstract class MonsterOrigin : MonoBehaviour
{
    // refs
    protected BehaviorCameraFollower _BehaviorCameraFollower;
    protected Manager_Platforms _Manager_Platforms;
    protected PlayerCore _PlayerCore;

    // public data    
    public bool IsCaptured { get; protected set; }
    [Header("General Omnipitent Variables\n______________")]
    public string nickname;
    public int healthMax;
    public int pointsRewarded;
    // local data
    protected int healthCurrent;
    protected bool runMonsterActive;
    protected Vector3 storedSpawnPos;
    protected Quaternion startRotation;
    protected Vector3 spawnPosition;
    // events to listen to
    public UnityEvent eOnSpawned, eOnHit, eOnCaptured, eOnDespawned; // we may do an injections 

    // CAPTURING
    [Header("Capturing Omnipitent Variables\n______________")]
    public int pointsForCapturing = 100;
    public Transform uiHolderOfHeartPoints;
    public float sizeToShrinkTo = 0.15f;
    public float sizePercentChangeEveryFrame = 0.999f;
    public float capturedMoveSpeed = 1;
    public bool spinWhileCaptured;
    public Vector3 spinSpeedDir = new Vector3(0, 90, 0);
    [Space]
    [Space]
    public bool flyTowardsPlayer;
    public float distanceToCollect = 0.5f;
    [Space]
    [Space]
    public float stateCaptureFlyTime = 10;

    private float audioPlayedTimeWait = 0.3f;
    private float[] audioPlayedTimeStamps = new float[6];


    public void BeginIntroduction() // called to begin the monster's behavior
    {
        // the level maneger + platform manager --> spawn the creature --> stop the running & call this
        UpdateInjectListeners(true); // add this monsters events to other scripts
        if (PlayerCore.Instance) _PlayerCore = PlayerCore.Instance;
        if (BehaviorCameraFollower.Instance) _BehaviorCameraFollower = BehaviorCameraFollower.Instance; // cam ref
        if (Manager_Platforms.Instance) _Manager_Platforms = Manager_Platforms.Instance;
        storedSpawnPos = transform.position;
        runMonsterActive = false;
        healthCurrent = healthMax;
        IsCaptured = false;
        MonsterSpawn(); //setup variables
        eOnSpawned?.Invoke();

        MonsterIntroduction();// run the monster introduction
        if (_BehaviorCameraFollower) { _BehaviorCameraFollower.LimitCamera(false); _BehaviorCameraFollower.MoveCamTargetSmooth(5f, transform.position, 1, 5); } // call the camera
    }

    protected void UpdateInjectListeners(bool _add)
    {
        // find all instances and add this monsters's events or remove based on passed variables
        // EXAMPLES DONE LIKE THIS PlayerHealth.OnDamageTaken += UpdateHealthUI; || m_MyEvent.AddListener(OnKeyPressed); &  m_MyEvent.RemoveListener(OnKeyPressed);
        if (_add)
        {

        }
        else
        {

        }
    }

    protected virtual void MonsterSpawn() // sets up monster's referencable variables
    {        
        // place as needed (ahead of player or behind player or wherever) 
        // store reference variables (get start position if needed, start timers / timestamps if needed)
    }

    protected virtual void MonsterIntroduction()
    {        
        // this behavior will be custom and inherited / overriden per monster
    }

    protected void CompleteIntroduction()
    {
        print("COMPLETE MONSTER INTRO");
        Vector3 returnPos = Vector3.zero;
        if (_PlayerCore) returnPos = _PlayerCore.transform.position;
        if (_BehaviorCameraFollower) { _BehaviorCameraFollower.LimitCamera(true); _BehaviorCameraFollower.MoveCamTargetSmooth(5f, returnPos, 0.33f, 5); } // return camera back to player, 
        // TODO perhaps the camera will handle the call to begin running again.

        runMonsterActive = true;
    }      

    public virtual void Tick() // run the monster
    {
        // run code that should check when not captured

        if (IsCaptured) return;
        // code that can run when not captured

        if (!runMonsterActive) return;
        // code that handles the monster's behavior
        MonsterActive();
    }

    protected virtual void MonsterActive() // RUNTIME for each unique monster
    {
        if (IsCaptured) return;
        print("RUNNING MONSTER");
        // each monster will run this uniquely.
    }

    public void TakeDamage(int _damage) // change health negative
    {
        print("DAMAGE MONSTER");
        if (IsCaptured) return;
        healthCurrent -= _damage;
        MonsterHit();
        if (isDefeated()) Capture();
    }

    protected void TakeHealing(int _health) // change health positive
    {
        if (IsCaptured) return;
        healthCurrent += _health;
    }

    protected virtual void MonsterHit()
    {
        if (IsCaptured) return;
        // this behavior will be custom and inherited / overriden per monster
    }

    protected bool isDefeated() // check if defeated (defeated = captured)
    {
        return healthCurrent <= 0;
    }   

    protected void Capture()
    {
        if (IsCaptured) return;
        print("CAPTURED MONSTER");
        IsCaptured = true;
        runMonsterActive = false;
        MonsterCaptured();
        eOnCaptured?.Invoke();
    }

    protected virtual void MonsterCaptured()
    {
        // fly towards player
        // on reaching player - play more vfx/sounds
        // let the level manager know it was captured signalling event             
    }

    //HELPERS
    protected void Despawn()
    {
        UpdateInjectListeners(false); // remove this monsters events to other scripts
        eOnDespawned?.Invoke();
        //if captured then just remove
        //else we need to let the level manager know to spawn us again soon
    }

    protected Vector3 MoveWithBackground()
    {
        if (_Manager_Platforms)
            return new Vector3(Manager_Platforms.Instance.CurrentSpeed(), 0, 0);
        else
        { Debug.LogWarning("Missing ManagerPlatforms.cs"); return Vector3.zero; }
    }

    public void SendStartPosition(Vector3 _pos)
    { // needed to ensure any code referencing start position of creature considers where the player "experiences" the creature spawned - (which usually should not be vector3.zero because of how the endless runner is coded)
        spawnPosition = _pos;
    }

    protected Vector3 ReturnRaycastPosition(float _xOffset, LayerMask _rayCastable)
    {
        // move it
        Vector3 position = Vector3.zero + new Vector3(_xOffset, 20, 0); // add offset
        // raycast from the position down onto a platform and place it there
        RaycastHit hit;
        if (Physics.Raycast(position, Vector3.down, out hit, Mathf.Infinity, _rayCastable))
        {
            position = hit.point + new Vector3(0, 0, 0);
            //print($"New Hunting Point: {position}");
        }
        else
        {
            Debug.Log("WARNING: MonsterBehvaior Raycast wasnt able to find surface to land on");
            position = Vector3.zero;
        }

        return position;
    }

    protected void CheckOnHitAudio() // this is done to avoid audio glitches when we fire a LOT of attacks at our enemy
    {
        for (int i = 0; i < audioPlayedTimeStamps.Length; i++)
        {
            if (Time.time > audioPlayedTimeStamps[i] + audioPlayedTimeWait)
            {
                audioPlayedTimeStamps[i] = Time.time;
                //onHitAudio?.Invoke();
                break;
            }
        }
    }

    protected bool CaptureEvents() // turned this into a bool so we can ensure its done before turning the object off
    {
        transform.rotation = startRotation;

        if (Manager_GameState.Instance)
        { Manager_GameState.Instance.CaptureChange(1, pointsForCapturing); }

        if (Manager_Platforms.Instance)
        { print("BehaviorMonster: CaptureEvents() - ChangeMonsterVariable HERE"); }// Manager_Platforms.Instance.ChangeMonsterVariables(true, false, true); }

        return true;
    }

    protected void UpdateUserInterface()
    {
        if (!uiHolderOfHeartPoints || uiHolderOfHeartPoints.childCount == 0)
            return;

        float percentHP = ((float)healthCurrent / (float)healthMax); // get the decimal      
        percentHP = (Mathf.RoundToInt(percentHP * 10)); // we have 10 hp bars to show or hide     

        for (int i = 0; i < uiHolderOfHeartPoints.childCount; i++)
        {
            if (uiHolderOfHeartPoints.GetChild(i))
                if (i < percentHP)
                    uiHolderOfHeartPoints.GetChild(i).gameObject.SetActive(true);
                else
                    uiHolderOfHeartPoints.GetChild(i).gameObject.SetActive(false);
        }
    }

}
