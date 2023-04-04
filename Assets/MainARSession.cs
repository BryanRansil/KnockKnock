using Niantic.ARDK.AR;
using Niantic.ARDK.AR.Anchors;
using Niantic.ARDK.AR.ARSessionEventArgs;
using Niantic.ARDK.AR.Configuration;
using Niantic.ARDK.AR.HitTest;
using Niantic.ARDK.Utilities;
using Niantic.ARDK.Utilities.Input.Legacy;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainARSession : MonoBehaviour
{
    public GameObject plane_prefab;
    public GameObject spawn_prefab;
    public float spawn_vertical_offset;
    public Camera active_camera;
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

    private void Update()
    {
        SpawnAtHitPoint();
    }

    // Taken from Niantic's Hit Test sample
    void SpawnAtHitPoint()
    {
        // Check if the user's touched the screen
        var touch = PlatformAgnosticInput.GetTouch(0);
        if (touch.phase != TouchPhase.Began)
            return;

        // If the ARSession isn't currently running, its CurrentFrame property will be null
        var currentFrame = _ar_session.CurrentFrame;
        if (currentFrame == null)
            return;

        // Hit test from the touch position
        var results =
            _ar_session.CurrentFrame.HitTest
            (
                active_camera.pixelWidth,
                active_camera.pixelHeight,
                touch.position,
                ARHitTestResultType.All
            );

        if (results.Count == 0)
            return;

        var closestHit = results[0];
        var position = closestHit.WorldTransform.ToPosition();

        // The position y-value offset needed to spawn your prefab at the
        // correct height (not intersecting with the plane) will depend on
        // where the center of your prefab is.
        position.y += spawn_vertical_offset;

        GameObject.Instantiate(spawn_prefab, position, Quaternion.identity);
    }

}