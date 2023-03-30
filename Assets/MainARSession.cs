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
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
