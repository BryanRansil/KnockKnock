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
    public Image palm_boundary = null;
    public TextMeshProUGUI text;

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
