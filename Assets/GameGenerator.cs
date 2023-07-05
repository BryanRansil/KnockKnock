

using Niantic.ARDK.AR;
using Niantic.ARDK.AR.HitTest;
using Niantic.ARDK.Utilities;
using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;
/**
* When we interact with a GameBoard, create a whole board
*/
public class GameGenerator : MonoBehaviour
{
    public GameObject terrain_prefab;
    public Camera game_camera;

    private IARFrame _current_frame;
    private Vector3 _furthest_boundary, _closest_boundary, _left_boundary, _right_boundary;
    private GameObject _terrain;
    private Collider _terrain_collider;
    private float _terrain_level;

    // Generation Variables
    private float _height_tolerance = 10;
    private bool _active;
    private float _increment_time;
    private const int _expansion_step = 40;
    private readonly float game_growth_delta = 0.1f;

    /**
     * From the screena and world positions, use the game_camera and current frame information to make a game
     * on the same horizontal plane.
     */
    public void Populate(Vector3 worldPosition, IARFrame currentFrame)
    {
        if (_active)
            return;

        _current_frame = currentFrame;
        bool expand_farther, expand_closer, expand_left, expand_right;
        (_furthest_boundary, expand_farther) = ExpandGame(worldPosition, Vector2.down);
        (_closest_boundary, expand_closer) = ExpandGame(worldPosition, Vector2.up);
        (_left_boundary, expand_left) = ExpandGame(worldPosition, Vector2.left);
        (_right_boundary, expand_right) = ExpandGame(worldPosition, Vector2.right);

        while (expand_farther || expand_closer || expand_left || expand_right)
        {
            if (expand_farther)
            {
                (_furthest_boundary, expand_farther) = ExpandGame(_furthest_boundary, Vector2.down);
            }

            if (expand_closer)
            {
                (_closest_boundary, expand_closer) = ExpandGame(_closest_boundary, Vector2.up);
            }

            if (expand_left)
            {
                (_left_boundary, expand_left) = ExpandGame(_left_boundary, Vector2.left);
            }

            if (expand_right)
            {
                (_right_boundary, expand_right) = ExpandGame(_right_boundary, Vector2.right);
            }
        }

        _terrain_level = _furthest_boundary.y;
        if (_closest_boundary.y > _terrain_level)
            _terrain_level = _closest_boundary.y;
        if (_left_boundary.y > _terrain_level)
            _terrain_level = _left_boundary.y;
        if (_right_boundary.y > _terrain_level)
            _terrain_level = _right_boundary.y;

        // Create the center of the terrain
        var center = new Vector3(_furthest_boundary.x / 2 + _closest_boundary.x / 2,
                                 _terrain_level,
                                 _left_boundary.z / 2 + _right_boundary.z / 2);

        _terrain = GameObject.Instantiate(terrain_prefab, center, Quaternion.identity);
        _terrain_collider = _terrain.GetComponent<Collider>();
        _active = true;
    }

    private (Vector3, bool) ExpandGame(Vector3 boundary, Vector2 vector2)
    {
        var new_screen_point = game_camera.WorldToScreenPoint(boundary);
        var orig_x = new_screen_point.x;
        var orig_y = new_screen_point.y;

        new_screen_point.x += vector2.x * _expansion_step;
        new_screen_point.y += vector2.y * _expansion_step;

        if (new_screen_point.y < 0 || new_screen_point.y > game_camera.pixelHeight ||
            new_screen_point.x < 0 || new_screen_point.x > game_camera.pixelWidth)
            return (boundary, false);

        var results = _current_frame.HitTest(
            game_camera.pixelWidth,
            game_camera.pixelHeight,
            new_screen_point,
            ARHitTestResultType.All);

        if (results.Count == 0)
            return (boundary, false);

        var new_world_position = results[0].WorldTransform.ToPosition();
        if (new_world_position.y > boundary.y + _height_tolerance ||
            new_world_position.y < boundary.y - _height_tolerance )
            return (boundary, false);

        return (new_world_position, true);
    }

    public void Update()
    {
        if (!_active)
            return;
        /*
        _increment_time += Time.deltaTime;

        if (_increment_time < game_growth_delta)
            return;

        _increment_time = 0;
        _terrain.transform.localScale += new Vector3(0.1f, 0.0f, 0.1f);
        if (_terrain_collider.bounds.size.x >= Math.Abs(_furthest_boundary.x - _closest_boundary.x) ||
            _terrain_collider.bounds.size.z >= Math.Abs(_left_boundary.z - _right_boundary.z))
        {
            _active = false;
        }*/
    }
}
