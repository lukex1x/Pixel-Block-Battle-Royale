using UnityEngine;

public class CarDamage : MonoBehaviour
{

    public AudioClip ClashSound;
    public byte ClashDamage;
    public Car Root;
    public float VelocityThreshold = 3;

    void Start()
    {

    }

    void OnTriggerEnter(Collider collider)
    {
        Rigidbody rig = Root.GetComponent<Rigidbody>();
        if (!rig)
            return;

        if (Root.Audiosource && ClashSound && rig.velocity.magnitude > VelocityThreshold)
        {
            Root.Audiosource.PlayOneShot(ClashSound);
        }

        DamagePackage dm = new DamagePackage();
        dm.Damage = (byte)(ClashDamage * rig.velocity.magnitude);
        dm.Normal = rig.velocity.normalized;
        dm.Direction = rig.velocity * 2;
        dm.Position = this.transform.position;
        if (Root.Seats[0].passenger != null)
        {
            dm.ID = (int)Root.Seats[0].passenger.netId.Value;
        }
        else
        {
            dm.ID = -1;
        }
        dm.Team = 3;
        dm.DamageType = 0;
        collider.gameObject.transform.root.SendMessage("DirectDamage", dm, SendMessageOptions.DontRequireReceiver);
    }
}
