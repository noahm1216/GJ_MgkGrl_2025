using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// <para> The core of all monster behavior. Holds their stats and data </para>
/// </summary>
public abstract class MonsterOrigin : MonoBehaviour
{
    // refs
    private BehaviorCameraFollower _BehaviorCameraFollower;
    private PlayerCore _PlayerCore;
    // public data
    public bool IsCaptured { get; protected set; }
    public string nickname;
    public int healthMax;
    public int pointsRewarded;
    // local data
    protected int healthCurrent;
    protected bool runMonsterActive;
    protected Vector3 storedSpawnPos;
    // events to listen to
    public UnityEvent eOnSpawned, eOnHit, eOnCapture, eOnDespawned; // we may do an injection since all 


    public void BeginIntroduction() // called to begin the monster's behavior
    {
        // the level maneger + platform manager --> spawn the creature --> stop the running & call this
        UpdateInjectListeners(true); // add this monsters events to other scripts
        if (PlayerCore.Instance) _PlayerCore = PlayerCore.Instance;
        if (BehaviorCameraFollower.Instance) _BehaviorCameraFollower = BehaviorCameraFollower.Instance; // cam ref
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
        // DONE LIKE THIS PlayerHealth.OnDamageTaken += UpdateHealthUI; || m_MyEvent.AddListener(OnKeyPressed); &  m_MyEvent.RemoveListener(OnKeyPressed);
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

    protected virtual void Update()
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
        eOnCapture?.Invoke();
    }

    protected virtual void MonsterCaptured()
    {
        // fly towards player
        // on reaching player - play more vfx/sounds
        // let the level manager know it was captured signalling event             
    }

    protected void Despawn()
    {
        UpdateInjectListeners(false); // remove this monsters events to other scripts
        eOnDespawned?.Invoke();
        //if captured then just remove
        //else we need to let the level manager know to spawn us again soon
    }     

}
