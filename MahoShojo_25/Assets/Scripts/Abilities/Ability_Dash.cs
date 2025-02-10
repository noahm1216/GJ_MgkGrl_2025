using UnityEngine;

[CreateAssetMenu]
public class Ability_Dash : Ability
{
    public Rigidbody rb3DPhysics;
    public bool verticalDash;
    public int dashPower;

    private void GetReferences(Transform _playerObj)
    {
        _playerObj.TryGetComponent(out rb3DPhysics);
    }

    public override void Activate(Transform _playerObj)
    {
        //base.Activate(); // if we also want to activate things in the original function

        if (!rb3DPhysics)
            GetReferences(_playerObj);

        if (!verticalDash) // dash left or right
        {
            if (rb3DPhysics)
                rb3DPhysics.AddForce(Vector3.right * dashPower);
            else
                _playerObj.position += new Vector3(dashPower * .1f, 0, 0);
        }
        else // dash up or down
        {
            if (rb3DPhysics)
                rb3DPhysics.AddForce(Vector3.up * dashPower);
            else
                _playerObj.position += new Vector3(0, dashPower * .1f, 0);
        }

    }
}
