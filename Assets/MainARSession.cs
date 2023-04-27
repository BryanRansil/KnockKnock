using ARDK.Extensions;
using Niantic.ARDK.AR;
using Niantic.ARDK.AR.Awareness;
using Niantic.ARDK.AR.Configuration;
using Niantic.ARDK.AR.HitTest;
using Niantic.ARDK.Utilities;
using Niantic.ARDK.Utilities.Input.Legacy;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.FullSerializer;
using UnityEditor;
using UnityEngine;

public class MainARSession : MonoBehaviour
{
    public Camera active_camera;
    public GameObject spawn_prefab;
    public CanvasUI my_canvas;

    IARSession _ar_session;
    private ARHandTrackingManager _handTrackingManager;


    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Bryan, Start Called");
        _ar_session = ARSessionFactory.Create();

        var configuration = ARWorldTrackingConfigurationFactory.Create();
        configuration.WorldAlignment = WorldAlignment.Gravity;
        configuration.IsLightEstimationEnabled = true;
        configuration.PlaneDetection = PlaneDetection.Horizontal;
        configuration.IsAutoFocusEnabled = true;
        configuration.IsDepthEnabled = true;
        configuration.DepthTargetFrameRate = 20;
        configuration.IsPalmDetectionEnabled = true;
        configuration.IsSharedExperienceEnabled = false;
        _ar_session.Run(configuration);

        my_canvas.Print("Is Depth Supported? " + ARWorldTrackingConfigurationFactory.CheckDepthSupport() + ", is estimation supported? " + ARWorldTrackingConfigurationFactory.CheckDepthEstimationSupport());
        _handTrackingManager.HandTrackingUpdated += OnHandTrackingUpdated;

    }

    // Update is called once per frame
    void Update()
    {
        ProcessInput();
    }

    void ProcessInput()
    {
        //PalmProcessing();

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

    private void OnHandTrackingUpdated(HumanTrackingArgs args)
    {
        Debug.Log("Bryan, OnHandTrackingUpdated");
        var data = args.TrackingData;
        if (data == null || data.AlignedDetections.Count == 0)
        {
            return;
        }

        Debug.Log("Bryan, creating rect & sending. ");
        var detection = data.AlignedDetections[0];
        Color border_color = new Color((1 - detection.Confidence), detection.Confidence, 0);
        Debug.Log("Bryan, creating rect & sending. " + detection.Rect);
        my_canvas.DrawRectangle(detection.Rect, border_color);

        Debug.Log("Bryan, sent");
        for (var i = 0; i < data.AlignedDetections.Count; i++)
        {
            var item = data.AlignedDetections[i];
            Debug.Log(item.X + " " + item.Y + " -- " + item.Width + " " + item.Height);
        }
        Debug.Log("Bryan, Finished");
    }


    void PalmProcessing()
    {
        if (_ar_session.CurrentFrame.PalmDetections == null)
        {
            my_canvas.Print("Is Depth Supported? " + ARWorldTrackingConfigurationFactory.CheckDepthSupport() + ", is estimation supported? " + ARWorldTrackingConfigurationFactory.CheckDepthEstimationSupport());
            return;
        }

        var palm_detection = _ar_session.CurrentFrame.PalmDetections[0];
        
        // Color is red if we have low confidence, green if we have high
        Color border_color = new Color((1 - palm_detection.Confidence), palm_detection.Confidence, 0);
        my_canvas.DrawRectangle(palm_detection.Rect, border_color);
    }
}
