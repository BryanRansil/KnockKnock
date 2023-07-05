using Niantic.ARDK.AR;
using Niantic.ARDK.AR.Awareness;
using Niantic.ARDK.AR.Configuration;
using Niantic.ARDK.AR.HitTest;
using Niantic.ARDK.Utilities;
using Niantic.ARDK.Utilities.Input.Legacy;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Android;

public class MainARSession : MonoBehaviour
{
    public Camera active_camera;
    public GameObject spawn_prefab;
    public CanvasUI my_canvas;
    public GameGenerator game_generator;
    public GameObject button;

    IARSession _ar_session;

    private string[] _required_permissions = { Permission.Camera, Permission.FineLocation };
    private List<string> _acquired_permissions;

    private Vector2 _button_screen_position;
    private readonly float _click_distance = 0.04f;

    // Start is called before the first frame update
    void Start()
    {
#if UNITY_ANDROID
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

        Debug.Log("In Start, require " + (_required_permissions.Length - acquired_permissions.Count) + " permissions before running.");
        if (acquired_permissions.Count == _required_permissions.Length)
        {
            StartAR();
        } else
        {
            Permission.RequestUserPermissions(_required_permissions, callbacks);
        }
#else
        StartAR();
#endif
}

    void StartAR()
    {
        Debug.Log("StartAR");
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
        _button_screen_position = new Vector2(Screen.width / 2, Screen.height / 2);
        button.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        if (!button.active)
            return;

        var palm_detection = PalmProcessing();
        if (palm_detection.HasValue)
        {
            var actual_position = PercentToScreenCoord(palm_detection.Value.Rect.position);
            if (PalmTouchButton(palm_detection.Value))
            {
                SpawnObject(button.transform.position);
                button.SetActive(false);
            }
        }
        else
        {
            SetButton();
        }
    }

    private Vector2 PercentToScreenCoord(Vector2 position)
    {
        return new Vector2(position.x * active_camera.pixelWidth, position.y * active_camera.pixelHeight);
    }

    void SetButton()
    {
        Debug.Log("Bryan, calling SetButton");
        var currentFrame = _ar_session.CurrentFrame;
        if (currentFrame == null)
            return;

        var results =
            currentFrame.HitTest
            (
                active_camera.pixelWidth,
                active_camera.pixelHeight,
                _button_screen_position,
                ARHitTestResultType.All
            );

        if (results.Count == 0)
            return;

        var closestHit = results[0];
        var position = closestHit.WorldTransform.ToPosition();
        button.transform.position = position;
    }

    private bool PalmTouchButton(Detection palm_detection)
    {
        var computed_position = GetWorldPosition(PercentToScreenCoord(palm_detection.Rect.position) +
            new Vector2(palm_detection.Rect.width / 2, 0));

        if (!computed_position.HasValue)
            return false;

        var palm_position = computed_position.Value;

        Debug.Log("Bryan, " + palm_position + " vs " + button.transform.position + " is " + Vector3.Distance(palm_position, button.transform.position));
        return Vector3.Distance(palm_position, button.transform.position) < _click_distance;
    }

    Nullable<Vector3> GetWorldPosition(Vector2 screen_position)
    {
        var currentFrame = _ar_session.CurrentFrame;
        if (currentFrame == null)
            return null;

        // Hit test from the touch position
        var results =
            _ar_session.CurrentFrame.HitTest
            (
                active_camera.pixelWidth,
                active_camera.pixelHeight,
                screen_position,
                ARHitTestResultType.All
            );

        if (results.Count == 0)
            return null;

        var closestHit = results[0];
        return closestHit.WorldTransform.ToPosition();
    }

    void SpawnObject(Vector3 world_position)
    {
        Debug.Log("Bryan, SpawnObject");
        game_generator.Populate(world_position, _ar_session.CurrentFrame);
    }

    Nullable<Niantic.ARDK.AR.Awareness.Detection> PalmProcessing()
    {
        if (_ar_session.CurrentFrame == null ||
            _ar_session.CurrentFrame.PalmDetections == null)
        {
            my_canvas.Print("0 Palms detected");
            return null;
        }

        var palm_detection = _ar_session.CurrentFrame.PalmDetections[0];
        // Color is red if we have low confidence, green if we have high
        Color border_color = new Color((1 - palm_detection.Confidence), palm_detection.Confidence, 0);
        my_canvas.DrawRectangle(palm_detection.Rect, border_color);

        return palm_detection;
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
