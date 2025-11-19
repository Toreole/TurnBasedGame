using UnityEngine;

// i would really like a IScreenTransition interface, but then you would not be able to
// serialize a reference to it with unitys default inspector and all that.
// using OdinInspector could help with that, but i dont really care enough rn.
public class BlackScreenTransitioner : MonoBehaviour
{
    [SerializeField]
    private FullScreenPassRendererFeature _transitionEffect;
    [SerializeField]
    private Material _fadeMaterial;

    [SerializeField]
    private Texture2D[] _fadeTextures;
    [SerializeField]
    private string _texturePropertyName;
    [SerializeField]
    private string _durationPropertyName;
    [SerializeField]
    private string _timePropertyName;

    public void Trigger(float duration)
    {
        var tex = _fadeTextures[Random.Range(0, _fadeTextures.Length)];
        _fadeMaterial.SetTexture(_texturePropertyName, tex);
        _fadeMaterial.SetFloat(_durationPropertyName, duration);
        _fadeMaterial.SetFloat(_timePropertyName, Time.time);
        _transitionEffect.SetActive(true);
    }

    public void End()
    {
        _transitionEffect.SetActive(false);
    }

}
