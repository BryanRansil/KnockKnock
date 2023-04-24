using Niantic.ARDK.AR;
using Niantic.ARDK.AR.Configuration;
using Niantic.ARDK.AR.HitTest;
using Niantic.ARDK.Utilities;
using Niantic.ARDK.Utilities.Input.Legacy;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainARSession : MonoBehaviour
{
    public Camera active_camera;
    public GameObject spawn_prefab;
    public CanvasUI my_canvas;

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
        configuration.IsDepthEnabled = true;
        configuration.IsPalmDetectionEnabled = true;
        configuration.IsSharedExperienceEnabled = false;
        _ar_session.Run(configuration);
    }

    // Update is called once per frame
    void Update()
    {
        ProcessInput();
    }

    void ProcessInput()
    {
        PalmProcessing();

        // Check if the user's touched the screen
        if (PlatformAgnosticInput.touchCount > 0 &&
            PlatformAgnosticInput.GetTouch(0).phase == TouchPhase.Began)
        {
            SpawnObject(PlatformAgnosticInput.GetTouch(0));
        }
    }

    void SpawnObject(Touch touch)
    {
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

        GameObject.Instantiate(spawn_prefab, position, Quaternion.identity);
    }

    void PalmProcessing()
    {
        if (_ar_session.CurrentFrame.PalmDetections == null)
        {
            my_canvas.Print("0 Palms detected");
            return;
        }

        my_canvas.Print(_ar_session.CurrentFrame.PalmDetections.Count + " Palms detected");

        var palm_detection = _ar_session.CurrentFrame.PalmDetections[0];
        // Color is red if we have low confidence, green if we have high
        Color border_color = new Color((1 - palm_detection.Confidence), palm_detection.Confidence, 0);
        my_canvas.DrawRectangle(palm_detection.Rect, border_color);
    }
}
