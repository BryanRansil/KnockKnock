using Niantic.ARDK.AR;
using Niantic.ARDK.AR.Configuration;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainARSession : MonoBehaviour
{
    IARSession _ar_session;

    // Start is called before the first frame update
    void Start()
    {
        _ar_session = ARSessionFactory.Create();

        var configuration = ARWorldTrackingConfigurationFactory.Create();
        configuration.WorldAlignment = WorldAlignment.Gravity;
        configuration.IsLightEstimationEnabled = true;
        configuration.PlaneDetection = PlaneDetection.Horizontal;
        configuration.IsAutoFocusEnabled = true;
        configuration.IsDepthEnabled = false;
        configuration.IsSharedExperienceEnabled = true;
        _ar_session.Run(configuration);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
