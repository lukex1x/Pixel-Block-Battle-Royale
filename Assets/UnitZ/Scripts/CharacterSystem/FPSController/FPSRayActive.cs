//----------------------------------------------
//      UnitZ Battleground : Online PVP starter kit
//    Copyright © Hardworker studio 2018 
// by Rachan Neamprasert www.hardworkerstudio.com
//----------------------------------------------

using UnityEngine;
using System.Linq;

public class FPSRayActive : MonoBehaviour
{
    public bool Sorting;
    public string[] IgnoreTag = { "Player" };
    public string[] DestroyerTag = { "Finish" };

    public void ShootRayOnce(Vector3 origin, Vector3 direction, int id, byte team)
    {
        // Normal Damage ray cast.
        RaycastHit hit;
        if (Physics.Raycast(origin, direction, out hit, 100.0f))
        {
            // if hit
            if (hit.collider.gameObject != this.gameObject)
            {
                // create damage package.
                DamagePackage dm;
                dm.Damage = 50;
                dm.Normal = hit.normal;
                dm.Direction = direction;
                dm.Position = hit.point;
                dm.ID = id;
                dm.Team = team;
                dm.DamageType = 0;
                // send Damage Package through OnHit function
                hit.collider.SendMessage("OnHit", dm, SendMessageOptions.DontRequireReceiver);

            }
        }
    }

    public void CheckingRay(Vector3 origin, Vector3 direction)
    {
        float raySize = 3;
        // Ray cast to any object to getting an object info
        RaycastHit[] casterhits = Physics.RaycastAll(origin, direction, raySize);
        for (int i = 0; i < casterhits.Length; i++)
        {
            if (casterhits[i].collider)
            {
                RaycastHit hit = casterhits[i];
                // get an object info via GetInfo function
                hit.collider.SendMessage("GetInfo", SendMessageOptions.DontRequireReceiver);
            }
        }
    }

    public void ActiveRay(Vector3 origin, Vector3 direction)
    {
        // Ray cast to Interactive and Pickup 
        float raySize = 3;
        RaycastHit[] casterhits = Physics.RaycastAll(origin, direction, raySize);
        for (int i = 0; i < casterhits.Length; i++)
        {
            if (casterhits[i].collider)
            {
                // interactive through Pickup funtion
                casterhits[i].collider.SendMessage("Pickup", this.gameObject, SendMessageOptions.DontRequireReceiver);
            }
        }
    }

    public void ActiveLocalRay(Vector3 origin, Vector3 direction)
    {
        // Ray cast to Interactive and Pickup 
        float raySize = 3;
        RaycastHit[] casterhits = Physics.RaycastAll(origin, direction, raySize);
        for (int i = 0; i < casterhits.Length; i++)
        {
            if (casterhits[i].collider)
            {
                // interactive through Pickup funtion
                casterhits[i].collider.SendMessage("PickupLocal", this.gameObject, SendMessageOptions.DontRequireReceiver);
            }
        }
    }

    public bool ShootSingleRay(Vector3 origin, Vector3 direction, byte num, byte spread, byte seed, byte damage, float size, int id, byte team)
    {
        bool res = false;
        System.Random random = new System.Random(seed);
        if (seed <= 0)
            spread = 0;
        for (int b = 0; b < num; b++)
        {
            Vector3 dir = direction + new Vector3(random.Next(-spread, spread) * 0.001f, random.Next(-spread, spread) * 0.001f, random.Next(-spread, spread) * 0.001f);
            RaycastHit hit;
            // Cast all objects.
            if(Physics.Raycast(origin, dir,out hit, size))
            {
                if (hit.collider)
                {
                    DamagePackage dm;
                    dm.Damage = damage;
                    dm.Normal = hit.normal;
                    dm.Direction = dir;
                    dm.Position = hit.point;
                    dm.ID = id;
                    dm.Team = team;
                    dm.DamageType = 0;
                    // send Damage Package through OnHit function
                    hit.collider.SendMessage("OnHit", dm, SendMessageOptions.DontRequireReceiver);
                    res = true;
                }
            }
        }
        return res;
    }

    public bool ShootSingleRayTest(Vector3 origin, Vector3 direction, byte num, byte spread, byte seed, byte damage, float size, int id, byte team)
    {
        bool res = false;
        System.Random random = new System.Random(seed);
        if (seed <= 0)
            spread = 0;
        for (int b = 0; b < num; b++)
        {
            Vector3 dir = direction + new Vector3(random.Next(-spread, spread) * 0.001f, random.Next(-spread, spread) * 0.001f, random.Next(-spread, spread) * 0.001f);
            RaycastHit hit;
            // Cast all objects.
            if (Physics.Raycast(origin, dir, out hit, size))
            {
                if (hit.collider)
                {
                    DamagePackage dm;
                    dm.Damage = damage;
                    dm.Normal = hit.normal;
                    dm.Direction = dir;
                    dm.Position = hit.point;
                    dm.ID = id;
                    dm.Team = team;
                    dm.DamageType = 0;
                    // send Damage Package through OnHit function
                    hit.collider.SendMessage("OnHitTest", dm, SendMessageOptions.DontRequireReceiver);
                    res = true;
                }
            }
        }
        return res;
    }

    public bool ShootRay(Vector3 origin, Vector3 direction, byte num, byte spread, byte seed, byte damage, float size, byte hitmax, int id, byte team)
    {
        // Multi piercing Damage Ray. e.g you can shoot through in many layer
        bool res = false;
        System.Random random = new System.Random(seed);
        if (seed <= 0)
            spread = 0;
        for (int b = 0; b < num; b++)
        {
            Vector3 dir = direction + new Vector3(random.Next(-spread, spread) * 0.001f, random.Next(-spread, spread) * 0.001f, random.Next(-spread, spread) * 0.001f);
            int hitcount = 0;
            // Cast all objects.
            RaycastHit[] hits = Physics.RaycastAll(origin, dir, size).OrderBy(h => h.distance).ToArray();
            for (int i = 0; i < hits.Length; i++)
            {
                if (hits[i].collider)
                {
                    if (tagCheck(hits[i].collider.gameObject) &&
                    hits[i].collider.gameObject != this.gameObject &&
                    ((hits[i].collider.transform.root &&
                        hits[i].collider.transform.root != this.gameObject.transform.root &&
                        hits[i].collider.transform.root.gameObject != this.gameObject) ||
                        hits[i].collider.transform.root == null))
                    {
                        RaycastHit hit = hits[i];
                        // Create Damage package
                        DamagePackage dm;
                        dm.Damage = damage;
                        dm.Normal = hit.normal;
                        dm.Direction = dir;
                        dm.Position = hit.point;
                        dm.ID = id;
                        dm.Team = team;
                        dm.DamageType = 0;
                        // send Damage Package through OnHit function
                        hit.collider.SendMessage("OnHit", dm, SendMessageOptions.DontRequireReceiver);
                        res = true;

                        // counting hit until max
                        hitcount++;
                        if (hitcount >= hitmax || tagDestroyerCheck(hit.collider.gameObject))
                        {
                            break;
                        }
                    }
                }
            }
        }
        return res;
    }

    public bool ShootRayTest(Vector3 origin, Vector3 direction, byte num, byte spread, byte seed, byte damage, float size, byte hitmax, int id, byte team)
    {
        bool res = false;
        System.Random random = new System.Random(seed);
        if (seed <= 0)
            spread = 0;
        for (int b = 0; b < num; b++)
        {
            Vector3 dir = direction + new Vector3(random.Next(-spread, spread) * 0.001f, random.Next(-spread, spread) * 0.001f, random.Next(-spread, spread) * 0.001f);
            int hitcount = 0;
            // Cast all objects.
            RaycastHit[] hits = Physics.RaycastAll(origin, dir, size).OrderBy(h => h.distance).ToArray();
            for (int i = 0; i < hits.Length; i++)
            {
                if (hits[i].collider)
                {
                    if (tagCheck(hits[i].collider.gameObject) &&
                    hits[i].collider.gameObject != this.gameObject &&
                    ((hits[i].collider.transform.root &&
                        hits[i].collider.transform.root != this.gameObject.transform.root &&
                        hits[i].collider.transform.root.gameObject != this.gameObject) ||
                        hits[i].collider.transform.root == null))
                    {
                        RaycastHit hit = hits[i];
                        // Create Damage package
                        DamagePackage dm;
                        dm.Damage = damage;
                        dm.Normal = hit.normal;
                        dm.Direction = dir;
                        dm.Position = hit.point;
                        dm.ID = id;
                        dm.Team = team;
                        dm.DamageType = 0;
                        // send Damage Package through OnHit function
                        hit.collider.SendMessage("OnHitTest", dm, SendMessageOptions.DontRequireReceiver);
                        res = true;

                        // counting hit until max
                        hitcount++;
                        if (hitcount >= hitmax || tagDestroyerCheck(hit.collider.gameObject))
                        {
                            break;
                        }
                    }
                }
            }
        }
        return res;
    }

    public bool Overlap(Vector3 origin, Vector3 forward, byte damage, float size, float dot, int id, byte team)
    {
        // overlap damage is not a ray it's just create a damage area, e.g. using for Melee damage
        bool res = false;
        var colliders = Physics.OverlapSphere(origin, size);

        foreach (var hit in colliders)
        {
            if (hit && hit.gameObject != this.gameObject && hit.gameObject.transform.root != this.gameObject.transform)
            {
                Debug.Log(hit.gameObject.transform.root.name);
                var dir = (hit.transform.position - origin).normalized;
                var direction = Vector3.Dot(dir, forward);

                if (direction >= dot)
                {
                    DamagePackage dm;
                    dm.Damage = damage;
                    dm.Normal = dir;
                    dm.Direction = forward;
                    dm.Position = hit.gameObject.transform.position;
                    dm.ID = id;
                    dm.Team = team;
                    dm.DamageType = 0;
                    hit.GetComponent<Collider>().SendMessage("OnHit", dm, SendMessageOptions.DontRequireReceiver);

                    res = true;
                }
            }
        }
        return res;
    }

    public bool OverlapTest(Vector3 origin, Vector3 forward, byte damage, float size, float dot, int id, byte team)
    {
        // overlap test is just for test.
        bool res = false;
        var colliders = Physics.OverlapSphere(origin, size);

        foreach (var hit in colliders)
        {
            if (hit && hit.gameObject != this.gameObject && hit.gameObject.transform.root != this.gameObject)
            {
                var dir = (hit.transform.position - origin).normalized;
                var direction = Vector3.Dot(dir, forward);

                if (direction >= dot)
                {
                    res = true;
                }
            }
        }

        return res;
    }

    private bool tagDestroyerCheck(GameObject obj)
    {
        for (int i = 0; i < DestroyerTag.Length; i++)
        {
            if (obj.CompareTag(DestroyerTag[i]))
            {
                return true;
            }
        }
        return false;
    }

    private bool tagCheck(GameObject obj)
    {
        for (int i = 0; i < IgnoreTag.Length; i++)
        {
            if (obj.CompareTag(IgnoreTag[i]))
            {
                return false;
            }
        }
        return true;
    }
}
