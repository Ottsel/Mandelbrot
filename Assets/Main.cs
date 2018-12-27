using System;
using System.Collections.Generic;
using System.IO;
using MathNet.Numerics;
using UnityEngine;
using UnityEngine.UI;

public class Main : MonoBehaviour
{
    private const float MaxN = 50f;
    
    public InputField InputField;
    public GameObject Panel;
    private RawImage _rawImage;

    private void Start()
    {
        _rawImage = GetComponent<RawImage>();
    }

    private void Update()
    {
        if (!Input.GetKeyDown(KeyCode.Return) || !Panel.activeInHierarchy) return;
        Mandelbrot();
        Panel.SetActive(false);
    }

    // ReSharper disable once MemberCanBePrivate.Global need public for button click
    public void Mandelbrot()
    {
        var resolution = Mathf.RoundToInt(_rawImage.rectTransform.rect.width);
        if (InputField.text != string.Empty)
        {
            resolution = Convert.ToInt32(InputField.text);
        }

        var tex = new Texture2D(resolution, resolution);
        _rawImage.texture = tex;
        for (var x = 0; x < tex.width; x++)
        for (var y = 0; y < tex.height; y++)
        {
            Complex z = 0;
            var cA = Twoinator(new[] {x, y}, resolution);
            var c = new Complex(cA[0], cA[1]);
            var nColor = Color.black;
            for (var n = 0; n < MaxN; n++)
            {
                z = Complex.Add(z * z, c);

                if (!(Complex.Abs(z) > 2)) continue;
                if (n > 1 && n < MaxN / 2) nColor = new Color(Color.red.r / 1 / (MaxN / 2 / n), 0, 0, 1);
                else if (n >= MaxN / 2)
                {
                    var bgGradient = 1 / (MaxN / n * 2);
                    nColor = new Color(1, bgGradient, bgGradient, 1);
                }
                break;
            }

            tex.SetPixel(x, y, nColor);
        }

        tex.Apply();
        var bytes = tex.EncodeToPNG();
        File.WriteAllBytes(Application.dataPath + "/../Mandelbrot.png", bytes);
    }

    private static float[] Twoinator(IList<int> currentPixel, int resolution)
    {
        currentPixel[0] = Mathf.RoundToInt(currentPixel[0] - resolution / 1.5f);
        currentPixel[1] = Mathf.RoundToInt(currentPixel[1] - resolution / 2f);
        var factor = 3f / resolution;
        var newPixel = new[] {currentPixel[0] * factor, currentPixel[1] * factor};
        return newPixel;
    }
}