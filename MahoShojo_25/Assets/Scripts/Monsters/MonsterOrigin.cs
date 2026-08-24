using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// <para> The core of all monster behavior. Holds their stats and data </para>
/// </summary>
public abstract class MonsterOrigin : MonoBehaviour
{
    public string nickname;
    public int healthMax;
    public int pointsRewarded;


    public void BeginMonsterIntroduction() // called to begin the monster's behavior
    {
        // the level maneger + platform manager --> spawn the creature --> stop the running --> call the camera
        // place as needed (ahead of player or behind player or wherever) 
        // store reference variables (get start position if needed, start timers / timestamps if needed)
        // we may need to 
        // etc...
        // run the monster introduction
    }

    protected virtual void MonsterIntroduction()
    {
        // this behavior will be custom and inherited / overriden per monster
    }

    protected void CompleteMonsterIntroduction()
    {
        // this will tell the camera our introduction is done and return it back, 
        // perhaps the camera will handle the call to begin running again.
    }

}
