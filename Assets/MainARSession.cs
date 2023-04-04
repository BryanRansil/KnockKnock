using Niantic.ARDK.AR;
using Niantic.ARDK.AR.Anchors;
using Niantic.ARDK.AR.ARSessionEventArgs;
using Niantic.ARDK.AR.Configuration;
using Niantic.ARDK.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainARSession : MonoBehaviour
{
    public GameObject plane_prefab;
    private IARSession _ar_session;
    private readonly Dictionary<Guid, GameObject> planeLookup = new Dictionary<Guid, GameObject>();

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

        _ar_session.AnchorsAdded += OnAnchorsAdded;
        _ar_session.AnchorsUpdated += OnAnchorsUpdated;
    }

    private void OnAnchorsAdded(AnchorsArgs args)
    {
        foreach (IARPlaneAnchor anchor in args.Anchors)
        {
            // If the anchor isn't a plane, don't instantiate a GameObject
            if (anchor.AnchorType != AnchorType.Plane)
                continue;
            // Remember this anchor and its GameObject so we can update its position
            // if we receive an update.
            planeLookup.Add(anchor.Identifier, Instantiate(plane_prefab));
            var gameObject = planeLookup[anchor.Identifier];

            // Display the plane GameObject in the same position, orientation, and scale as the detected plane
            gameObject.transform.position = anchor.Transform.ToPosition();
            gameObject.transform.rotation = anchor.Transform.ToRotation();
            gameObject.transform.localScale = anchor.Extent;
        }
    }

    private void OnAnchorsUpdated(AnchorsArgs args)
    {
        foreach (IARPlaneAnchor anchor in args.Anchors)
        {
            GameObject gameObject;
            if (planeLookup.TryGetValue(anchor.Identifier, out gameObject))
            {
                gameObject.transform.position = anchor.Transform.ToPosition();
                gameObject.transform.rotation = anchor.Transform.ToRotation();
                gameObject.transform.localScale = anchor.Extent;
            }
        }
    }
}