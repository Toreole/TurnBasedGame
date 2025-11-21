using UnityEngine;

public class ScreenMeltTransition : MonoBehaviour
{
    [SerializeField]
    private Material _mat;
    [SerializeField]
    private FullScreenPassRendererFeature _effect;

    [SerializeField]
    private string _startTimePropName;

    [SerializeField]
    private RenderTexture _renderTexture;
    [SerializeField]
    private Camera _camera;

    private float _lastStart = -100f;

    // Update is called once per frame
    void Update()
    {
        //if (Input.GetKeyDown(KeyCode.Return))
        //{
        //    Trigger();
        //}
        if (Time.time - _lastStart > 5)
            _effect.SetActive(false);
    }

    public void Trigger()
    {
        _renderTexture.width = Screen.width;
        _renderTexture.height = Screen.height;
        _camera.targetTexture = _renderTexture;
        _camera.Render();
        _mat.SetFloat(_startTimePropName, Time.time);
        _lastStart = Time.time;
        _effect.SetActive(true);
    }
}
