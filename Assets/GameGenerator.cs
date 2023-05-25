

using Niantic.ARDK.AR;
using Niantic.ARDK.AR.HitTest;
using Niantic.ARDK.Utilities;
using System;
using UnityEngine;
using UnityEngine.UIElements;
/**
* When we interact with a GameBoard, create a whole board
*/
public class GameGenerator : MonoBehaviour
{
    public GameObject boundary_prefab;
    public Camera game_camera;

    private IARFrame _current_frame;
    private GameObject _furthest_boundary, _closest_boundary, _left_boundary, _right_boundary;
    private float _z;
    private float _z_tolerance = 10;
    private const int _expansion_step = 50;

    /**
     * From the screena and world positions, use the game_camera and current frame information to make a game
     * on the same horizontal plane.
     */
    public void Populate(Vector2 screenPosition, Vector3 worldPosition, IARFrame currentFrame)
    {
        _current_frame = currentFrame;
        _furthest_boundary = GameObject.Instantiate(boundary_prefab, worldPosition, Quaternion.identity);
        _closest_boundary = GameObject.Instantiate(boundary_prefab, worldPosition, Quaternion.identity);
        _left_boundary = GameObject.Instantiate(boundary_prefab, worldPosition, Quaternion.identity);
        _right_boundary = GameObject.Instantiate(boundary_prefab, worldPosition, Quaternion.identity);

        bool expand_farther = ExpandGame(_furthest_boundary, Vector2.down);
        bool expand_closer = ExpandGame(_closest_boundary, Vector2.up);
        bool expand_left = ExpandGame(_left_boundary, Vector2.left);
        bool expand_right = ExpandGame(_right_boundary, Vector2.right);
        while (expand_farther || expand_closer || expand_left || expand_right)
        {
            if (expand_farther)
                expand_farther = ExpandGame(_furthest_boundary, Vector2.down);

            if (expand_closer)
                expand_closer = ExpandGame(_closest_boundary, Vector2.up);

            if (expand_left)
                expand_left = ExpandGame(_left_boundary, Vector2.left);

            if (expand_right)
                expand_right = ExpandGame(_right_boundary, Vector2.right);

        }

        _z = _furthest_boundary.transform.position.z;
        if (_closest_boundary.transform.position.z > _z)
            _z = _closest_boundary.transform.position.z;
        if (_left_boundary.transform.position.z > _z)
            _z = _left_boundary.transform.position.z;
        if (_right_boundary.transform.position.z > _z)
            _z = _right_boundary.transform.position.z;
    }

    private bool ExpandGame(GameObject boundary, Vector2 vector2)
    {
        var new_screen_point = game_camera.WorldToScreenPoint(
                boundary.transform.position);

        new_screen_point.x += vector2.x * _expansion_step;
        new_screen_point.y += vector2.y * _expansion_step;

        if (new_screen_point.y < 0 || new_screen_point.y > game_camera.pixelHeight ||
                       new_screen_point.x < 0 || new_screen_point.x > game_camera.pixelWidth)
            return false;

        var results = _current_frame.HitTest(
            game_camera.pixelWidth,
            game_camera.pixelHeight,
            new_screen_point,
            ARHitTestResultType.All);

        if (results.Count == 0)
            return false;


        var new_position = results[0].WorldTransform.ToPosition();
        if (new_position.z > boundary.transform.position.z + _z_tolerance ||
            new_position.z < boundary.transform.position.z - _z_tolerance )
            return false;

        boundary.transform.position = new_position;
        return true;
    }
}
