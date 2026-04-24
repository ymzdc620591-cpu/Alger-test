using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Light))]
public class VolumicLightController : MonoBehaviour
{
    public VolumeLightFeature.Settings setting = new VolumeLightFeature.Settings();
    // Start is called before the first frame update
    private void OnEnable()
    {
        VolumeLightFeature.settings = setting;
    }

    private void OnDisable()
    {
        VolumeLightFeature.settings = null;
    }
}
