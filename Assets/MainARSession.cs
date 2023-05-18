using Niantic.ARDK.AR;
using Niantic.ARDK.AR.Configuration;
using Niantic.ARDK.AR.HitTest;
using Niantic.ARDK.Utilities;
using Niantic.ARDK.Utilities.Input.Legacy;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Android;

public class MainARSession : MonoBehaviour
{
    public Camera active_camera;
    public GameObject spawn_prefab;
    public CanvasUI my_canvas;
    public BoardMeshGenerator board_generator;

    IARSession _ar_session;

    private string[] _required_permissions = { Permission.Camera, Permission.FineLocation };
    private List<string> _acquired_permissions;

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log("Bryan In Start");
#if UNITY_ANDROID
        Debug.Log("Bryan Android Version 2");
        var callbacks = new PermissionCallbacks();
        callbacks.PermissionGranted += PermissionCallbacks_PermissionGranted;
        List<string> acquired_permissions = new List<string>();

        foreach (var permission in _required_permissions)
        {
            if (Permission.HasUserAuthorizedPermission(permission))
            {
                acquired_permissions.Add(permission);
            }
        }
        Debug.Log("Bryan acquired permissions are " + acquired_permissions.Count);

        if (acquired_permissions.Count == _required_permissions.Length)
        {
            StartAR();
        } else
        {
            Permission.RequestUserPermissions(_required_permissions, callbacks);
        }
#else
        Debug.Log("Bryan iOS Version");
        StartAR();
#endif
    }

    void StartAR()
    {
        Debug.Log("Bryan StartAR");
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
        Debug.Log("Bryan, in SpawnObject");
        // If the ARSession isn't currently running, its CurrentFrame property will be null
        var currentFrame = _ar_session.CurrentFrame;
        if (currentFrame == null)
            return;
        Debug.Log("Bryan, in SpawnObject passed 1");

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
        Debug.Log("Bryan, in SpawnObject passed 2");

        var closestHit = results[0];
        var position = closestHit.WorldTransform.ToPosition();

        Debug.Log("Bryan, calling Set Vector");
        board_generator.SetVertex(position);
        Debug.Log("Bryan, calling Instantiate at " + position);
        GameObject.Instantiate(spawn_prefab, position, Quaternion.identity);
    }

    void PalmProcessing()
    {
        if (_ar_session.CurrentFrame == null ||
            _ar_session.CurrentFrame.PalmDetections == null)
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

    internal void PermissionCallbacks_PermissionGranted(string permissionName)
    {
        if (_required_permissions.Contains(permissionName))
        {
            _acquired_permissions.Add(permissionName);
        }

        Debug.Log("Bryan. " + _required_permissions.Contains(permissionName) + " and now " + _acquired_permissions.Count + " vs " + _required_permissions.Length);
            StartAR();
    }
}
