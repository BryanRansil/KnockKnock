using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CanvasUI : MonoBehaviour
{
    public LineRenderer rectangle_line_prefab;

    public TextMeshProUGUI text;
    public Canvas canvas;

    public void Print(string msg)
    {
        text.text = msg;
    }

    // Draw a single rectangle on the canvas.
    public void DrawRectangle(Rect rect, Color border_color)
    {
        if (canvas == null)
            return;

        // Pre-calculate the bottom right to enhance readability
        Vector2 bottom_right_corner = new Vector2(rect.x + rect.width,
                                                  rect.y + rect.height);

        // TODO: save previous LineRenderers to reuse objects
        LineRenderer palm_line = Instantiate(rectangle_line_prefab);
        palm_line.startColor = border_color;
        palm_line.endColor = border_color;
        palm_line.positionCount = 4;
        palm_line.SetPosition(0, new Vector2(rect.x, rect.y));
        palm_line.SetPosition(1, new Vector2(bottom_right_corner.x, rect.y));
        palm_line.SetPosition(2, new Vector2(bottom_right_corner.x, bottom_right_corner.y));
        palm_line.SetPosition(3, new Vector2(rect.x, bottom_right_corner.y));
    }
}
