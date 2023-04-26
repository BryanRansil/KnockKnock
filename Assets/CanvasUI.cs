using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CanvasUI : MonoBehaviour
{
    public Canvas canvas;
    public Image palm_boundary_prefab;
    public Image detection_point_image_prefab;
    public TextMeshProUGUI text;

    private Image palm_boundary = null;
    private Image knuckle_point_image = null;
    private Image palm_point_image = null;

    public void Print(string msg)
    {
        if (SceneManager.GetActiveScene().name.Equals("Gameboard"))
            return;

        text.text = msg;
    }

    // Draw a single rectangle on the canvas.
    public void DrawRectangle(Rect rect, Color border_color)
    {
        if (SceneManager.GetActiveScene().name.Equals("Gameboard"))
            return;

        if (canvas == null)
            return;

        var position = new Vector3(rect.position.x * Screen.width, Flip(rect.position.y) * Screen.height, 0);
        if (palm_boundary == null) {
            palm_boundary = Instantiate(palm_boundary_prefab, position, Quaternion.identity, canvas.transform);
        } else {
            palm_boundary.transform.position = position;
        }
        palm_boundary.color = border_color;
        palm_boundary.rectTransform.sizeDelta = new Vector2(rect.size.x * Screen.width, rect.size.y * Screen.height);

        var knuckle_detection_point = new Vector2(rect.center.x * Screen.width, position.y);
        if (knuckle_point_image == null)
        {
            knuckle_point_image = Instantiate(detection_point_image_prefab, knuckle_detection_point, Quaternion.identity, canvas.transform);
        } else {
            knuckle_point_image.transform.position = knuckle_detection_point;
        }

        var palm_detection_point = new Vector2(rect.center.x * Screen.width, Flip(rect.center.y) * Screen.height);
        if (palm_point_image == null)
        {
            palm_point_image = Instantiate(detection_point_image_prefab, palm_detection_point, Quaternion.identity, canvas.transform);
        } else {
            palm_point_image.transform.position = palm_detection_point;
        }
    }

    public void DrawPoint(Vector2 point)
    {
        Gizmos.DrawSphere(point, 4);
    }
    
    float Flip(float i)
    {
        float diff = 0.5f - i;
        return 0.5f + diff;
    }

    public void SwitchScenes()
    {
        if (SceneManager.GetActiveScene().name.Equals("Gameboard"))
        {
            SceneManager.LoadScene("SampleScene");
        }
        else
        {
            SceneManager.LoadScene("Gameboard");
        }
    }
}
