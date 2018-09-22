using UnityEngine;

public class LightFX : MonoBehaviour
{

    public Light lighting;
    public float IntensityTarget = 0;
    public float Speed = 30;
    void Start()
    {
        if (lighting == null)
            lighting = this.GetComponent<Light>();
    }

    void Update()
    {
        if (lighting != null)
        {
            lighting.intensity = Mathf.Lerp(lighting.intensity, IntensityTarget, Speed * Time.deltaTime);

        }
    }
}
