using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Obstacles script is inteded to take interactions from outside objects or player and put out a result
/// </summary>
public class BehaviorObstacles : MonoBehaviour
{
    public enum signalType { None, Dash, Bump, Jump }
    public enum obstacleType { None, MagicRing, Senpai, LoveBox, Endgame, StickyGoop, }
    [Space]
    [Header("Obstacle Tag \n __________")]
    [Space]
    public obstacleType thisObsType;

    public bool assignTagAtStart;
    public int scoreAddon;
    public bool tryMoveWithPlatforms = true;

    private bool waitingToApplyPhysics;
    private Transform interactor;
    private PlayerCore refPlayerCore;

    [Space]
    [Header("Physics Values\n __________")]
    [Space]
    //a variable for bouncing
    [Tooltip("When something sends a 'Touch' signal to the magic ring or box, it will add force forward to the object by this amount")]
    public int physicsPower = 15;

    [Space]
    [Header("Feedback \n __________")]
    [Space]
    //a variable for bouncing
    [Tooltip("When Maho dashes into this obstacle it will play the VFX gameobject below")]
    public UnityEvent onInteractionEvents, onBumpEvents, onDashEvent;


    void Start()
    {
        if (assignTagAtStart) transform.tag = "Obstacle";
    }// end of Start()

    private void Update() // TODO: Make a list of objects that the platformManager.cs loops through and handles moving... || or || can do something with parenting to the maps but that's not ideal
    {
        if (tryMoveWithPlatforms && Manager_Platforms.Instance)
            transform.position += new Vector3(Manager_Platforms.Instance.CurrentSpeed(), 0, 0);
    }

    private void FixedUpdate()
    {
        if (waitingToApplyPhysics)
        {
            switch (thisObsType)
            {
                case obstacleType.MagicRing:
                    if (refPlayerCore) refPlayerCore.rb3D.AddForce((Vector3.right * physicsPower) - refPlayerCore.rb3D.velocity, ForceMode.VelocityChange);
                    break;
                case obstacleType.LoveBox:
                    Vector3 directionHit = interactor.position - transform.position;
                    if (refPlayerCore) refPlayerCore.rb3D.AddForce((directionHit * physicsPower) - refPlayerCore.rb3D.velocity, ForceMode.VelocityChange);
                    break;
                default:
                    break;
            }
            waitingToApplyPhysics = false;
        }
    }

    public void Interacted(Transform _interactor, PlayerCore _refPlayerCore, signalType _sentSignal)
    {
        onInteractionEvents?.Invoke();

        interactor = _interactor;
        refPlayerCore = _refPlayerCore;

        switch (_sentSignal)
        {
            case signalType.Dash:
                Dashed(_interactor, _refPlayerCore);
                break;
            case signalType.Bump:
                Touched(_interactor, _refPlayerCore);
                break;
            default:
                print($"{_sentSignal} not accounted for yet");
                break;
        }       

        if (Manager_GameState.Instance)
            Manager_GameState.Instance.ObstaclePointChange(scoreAddon);

    }// end of Interacted()

    //for when the player dashes up against an obstacle
    //if it has an interaction with an obstacle it will be here
    private void Dashed(Transform _interactor, PlayerCore _refPlayerCore)
    {
        //print($"{thisObsType} | {_interactor.name}: was smashed");
        onDashEvent?.Invoke();        

        switch (thisObsType)
        {
            case obstacleType.MagicRing:
                print("Dashed() - MagicRing");
                //if (_refPlayerCore) _refPlayerCore.rb3D.AddForce(Vector3.right * physicsPower);
                waitingToApplyPhysics = true;
                break;
            case obstacleType.Senpai:
                print("Dashed() - Senpai");
                break;
            case obstacleType.LoveBox:
                print("Dashed() - LoveBox"); //bounce the _interactor       
                //Vector3 directionHit = _interactor.position - transform.position;
                //if (_refPlayerCore) _refPlayerCore.rb3D.AddForce(directionHit * physicsPower);
                waitingToApplyPhysics = true;
                break;
            case obstacleType.Endgame:
                print("Dashed() - Endgame");
                break;
            default:
                print($"{thisObsType}| {_interactor.name} not accounted for yet");
                break;
        }

    }//end of Dashed()

    //for when the player simply bumps up against or touches an obstacle
    //if it has an interaction with an obstacle it will be here
    private void Touched(Transform _interactor, PlayerCore _refPlayerCore)
    {
        //print($"{thisObsType} - was touched by - {_interactor.name}");
        onBumpEvents?.Invoke();

        switch (thisObsType)
        {
            case obstacleType.MagicRing:
                print("Touched() - MagicRing");
                //if (_refPlayerCore) _refPlayerCore.rb3D.AddForce(Vector3.right * physicsPower);
                waitingToApplyPhysics = true;
                break;
            case obstacleType.Senpai:
                print("Touched() - Senpai");
                break;
            case obstacleType.LoveBox:
                print("Touched() - LoveBox"); //bounce the _interactor  
                //Vector3 directionHit = _interactor.position - transform.position;
                //if (_refPlayerCore) _refPlayerCore.rb3D.AddForce(directionHit * physicsPower * 2);
                waitingToApplyPhysics = true;
                break;
            case obstacleType.Endgame:
                print("Touched() - Endgame");
                break;
            default:
                print($"{thisObsType} not accounted for yet");
                break;
        }
    }//end of Touched()



}//end of Obstacle Class
