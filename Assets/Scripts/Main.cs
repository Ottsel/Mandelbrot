using System;
using System.Collections.Generic;
using System.IO;
using MathNet.Numerics;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Main : MonoBehaviour
{
    public InputField XOffset, YOffset, ZoomLevel;
    public Text CurrentCoords, MouseCoords, IterationsText;
    public Slider Iterations;
    public Button SaveButton;

    public Canvas Canvas;
    public EventSystem EventSystem;

    private RawImage _rawImage;
    private Texture2D _tex;
    private GraphicRaycaster _raycaster;
    private PointerEventData _pointerEventData;

    private float[] _offset;
    private float _zoom = 1, _maxN;
    private int _resolution, _save;
    private bool _calculating;

    private byte[] _bytes;

    private void Start()
    {
        _rawImage = GetComponent<RawImage>();
        _resolution = Screen.currentResolution.height > Screen.currentResolution.width
            ? Screen.currentResolution.width
            : Screen.currentResolution.height;
        _tex = new Texture2D(_resolution, _resolution);
        _offset = new[] {0f, 0f};

        _raycaster = Canvas.GetComponent<GraphicRaycaster>();
        Mandelbrot();
        XOffset.text = string.Empty;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return) && !_calculating) Mandelbrot();
        var target = Input.mousePosition;
        var coords = GetComplexCoords(Mathf.RoundToInt(target.x), Mathf.RoundToInt(target.y),
            new[] {Screen.width, Screen.height});
        MouseCoords.text = "Cursor: (" + string.Format("{0:0.0#######}", coords.Real) + ", " +
                           string.Format("{0:0.0#######}", coords.Imaginary) + ")";
        if (!Input.GetMouseButtonDown(0)) return;
        _pointerEventData = new PointerEventData(EventSystem) {position = Input.mousePosition};
        var results = new List<RaycastResult>();
        _raycaster.Raycast(_pointerEventData, results);
        var goodHit = true;
        foreach (var result in results)
        {
            if (!result.gameObject.CompareTag("Canvas"))
            {
                goodHit = false;
            }
        }

        if (!goodHit) return;
        XOffset.text = string.Format("{0:0.0#######}", coords.Real);
        YOffset.text = string.Format("{0:0.0#######}", coords.Imaginary);
    }

    // ReSharper disable once MemberCanBePrivate.Global need public for button click
    public void Mandelbrot()
    {
        _calculating = true;
        GetInput();
        SaveButton.gameObject.SetActive(true);

        _rawImage.texture = _tex;

        for (var x = 0; x < _tex.width; x++)
        for (var y = 0; y < _tex.height; y++)
        {
            var nColor = Color.black;

            Complex z = 0;
            var c = GetComplexCoords(x, y, new[] {_resolution, _resolution});

            for (var n = 0; n < _maxN; n++)
            {
                z = Complex.Add(z * z, c);

                if (!(Complex.Abs(z) > 2)) continue;
                if (n > 1 && n < _maxN / 2) nColor = new Color(1 / (_maxN / 2 / n), 0, 0, 1);
                else if (n >= _maxN / 2)
                {
                    var bgGradient = 1 / (_maxN / (n - _maxN / 2));
                    nColor = new Color(1, bgGradient, 0, 1);
                }

                break;
            }

            _tex.SetPixel(x, y, nColor);
        }

        _tex.Apply();
        _calculating = false;
    }

    private Complex GetComplexCoords(int x, int y, IList<int> size)
    {
        x = Mathf.RoundToInt(x - size[0] / 2f);
        y = Mathf.RoundToInt(y - size[1] / 2f);

        var multiplier = 4f / _zoom / size[1];

        return new Complex(x * multiplier + _offset[0], y * multiplier + _offset[1]);
    }

    private void GetInput()
    {
        _maxN = Iterations.value;
        _offset[0] = XOffset.text != string.Empty ? (float) Convert.ToDouble(XOffset.text) : 0;
        _offset[1] = YOffset.text != string.Empty ? (float) Convert.ToDouble(YOffset.text) : 0;
        if (ZoomLevel.text != string.Empty && Math.Abs(Convert.ToDouble(ZoomLevel.text)) > .0000000000001f)
            _zoom = (float) Convert.ToDouble(ZoomLevel.text) * _zoom;
        else
            _zoom = 1;
        CurrentCoords.text = "Zoom: " + string.Format("{0:e1}", _zoom) + "\nCoordinates: (" +
                             string.Format("{0:0.0#######}", _offset[0]) +
                             ", " + string.Format("{0:0.0#######}", _offset[1]) + ")\nIterations: " + _maxN;
    }

    // ReSharper disable once UnusedMember.Global // Save button OnClick
    public void Save()
    {
        _bytes = _tex.EncodeToPNG();
        if (_save == 0)
        {
            File.WriteAllBytes(Application.dataPath + "/../Mandelbrot.png", _bytes);
        }
        else
        {
            File.WriteAllBytes(Application.dataPath + "/../Mandelbrot(" + _save + ").png", _bytes);
        }

        _save++;
    }

    // ReSharper disable once UnusedMember.Global // Iteration Slider OnValueChanged
    public void UpdateIterations()
    {
        IterationsText.text = "Iterations: " + Iterations.value;
    }
}