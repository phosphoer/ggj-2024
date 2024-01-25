using UnityEngine;
using System.Collections.Generic;

public class SimplePostFX : MonoBehaviour
{
  public Shader Shader = null;
  public SimplePostFXDefinition InitialSettings = null;

  [Range(0, 8)]
  public int BloomIterations = 4;

  private List<PostFXLayer> _layers = new List<PostFXLayer>();
  private List<RenderTexture> _blurSteps = new List<RenderTexture>();
  private Material _postFxMaterial;
  private int _lastWidth;
  private int _lastHeight;
  private int _lastBloomIterationCount;

  [System.Serializable]
  public class PostFXLayer
  {
    public PostFXLayer(SimplePostFXDefinition settings, float weight)
    {
      Settings = settings;
      Weight = weight;
    }

    public SimplePostFXDefinition Settings = null;
    public float Weight = 1;
  }

  private static readonly int kMatSourceTex = Shader.PropertyToID("_SourceTex");
  private static readonly int kMatBloomParams = Shader.PropertyToID("_BloomParams");
  private static readonly int kMatColorParams = Shader.PropertyToID("_ColorParams");
  private static readonly int kMatChannelMixerRed = Shader.PropertyToID("_ChannelMixerRed");
  private static readonly int kMatChannelMixerGreen = Shader.PropertyToID("_ChannelMixerGreen");
  private static readonly int kMatChannelMixerBlue = Shader.PropertyToID("_ChannelMixerBlue");

  private const int kPassDownPrefilter = 0;
  private const int kPassDown = 1;
  private const int kPassUp = 2;
  private const int kPassFinal = 3;

  public void AddLayer(PostFXLayer layer)
  {
    _layers.Add(layer);
    _layers.Sort((a, b) =>
    {
      return a.Settings.Priority - b.Settings.Priority;
    });
  }

  public void RemoveLayer(PostFXLayer layer)
  {
    _layers.Remove(layer);
  }

  private void Start()
  {
    if (InitialSettings != null)
    {
      AddInitialLayer();
    }
    else
    {
      enabled = false;
    }
  }

  private void OnDestroy()
  {
    FreeRenderTextures();
    Destroy(_postFxMaterial);
  }

  [ContextMenu("Add Initial Layer")]
  private void AddInitialLayer()
  {
    AddLayer(new PostFXLayer(InitialSettings, 1));
  }

  private void FreeRenderTextures()
  {
    for (int i = 0; i < _blurSteps.Count; ++i)
    {
      if (_blurSteps[i] != null)
      {
        _blurSteps[i].Release();
        Destroy(_blurSteps[i]);
      }

      _blurSteps.Clear();
    }
  }

  private void UpdateRenderTextures(RenderTexture source)
  {
    if (_lastWidth != source.width || _lastHeight != source.height || _lastBloomIterationCount != BloomIterations)
    {
      FreeRenderTextures();

      _lastWidth = source.width;
      _lastHeight = source.height;
      _lastBloomIterationCount = BloomIterations;
      int blurWidth = _lastWidth;
      int blurHeight = _lastHeight;
      const int divisor = 2;
      for (int i = 0; i < BloomIterations && blurWidth >= divisor && blurHeight >= divisor; ++i)
      {
        blurWidth /= divisor;
        blurHeight /= divisor;
        RenderTexture blurStep = new RenderTexture(blurWidth, blurHeight, 0, source.format);
        blurStep.Create();
        _blurSteps.Add(blurStep);
      }
    }
  }

  private void OnRenderImage(RenderTexture source, RenderTexture destination)
  {
    // Create material
    if (_postFxMaterial == null && Shader != null)
    {
      _postFxMaterial = new Material(Shader);
      _postFxMaterial.hideFlags = HideFlags.HideAndDontSave;
    }

    if (_postFxMaterial == null || _layers.Count == 0)
    {
      Graphics.Blit(source, destination);
      return;
    }

    SimplePostFXDefinition.BloomSettings bloom = SimplePostFXDefinition.BloomDefault;
    SimplePostFXDefinition.SaturationContrastSettings saturationContrast = SimplePostFXDefinition.SaturationContrastDefault;
    SimplePostFXDefinition.WhiteBalanceSettings whiteBalance = SimplePostFXDefinition.WhiteBalanceDefault;
    SimplePostFXDefinition.ChannelMixerSettings channelMixers = SimplePostFXDefinition.ChannelMixerDefault;
    for (int i = 0; i < _layers.Count; ++i)
    {
      PostFXLayer layer = _layers[i];
      if (layer.Settings.Bloom.Enabled)
      {
        bloom = SimplePostFXDefinition.Lerp(bloom, layer.Settings.Bloom, layer.Weight);
      }

      if (layer.Settings.SaturationContrast.Enabled)
      {
        saturationContrast = SimplePostFXDefinition.Lerp(saturationContrast, layer.Settings.SaturationContrast, layer.Weight);
      }

      if (layer.Settings.WhiteBalance.Enabled)
      {
        whiteBalance = SimplePostFXDefinition.Lerp(whiteBalance, layer.Settings.WhiteBalance, layer.Weight);
      }

      if (layer.Settings.ChannelMixers.Enabled)
      {
        channelMixers = SimplePostFXDefinition.Lerp(channelMixers, layer.Settings.ChannelMixers, layer.Weight);
      }
    }

    // Update material params
    Vector4 bloomParams = new Vector4(bloom.BloomFilterSize, bloom.BloomThreshold, bloom.BloomIntensity, bloom.BloomThresholdSoft);
    Vector4 colorParams = new Vector4(saturationContrast.ColorSaturation, saturationContrast.ColorContrast, whiteBalance.ColorTemperature, whiteBalance.ColorTint);
    _postFxMaterial.SetVector(kMatBloomParams, bloomParams);
    _postFxMaterial.SetVector(kMatColorParams, colorParams);
    _postFxMaterial.SetVector(kMatChannelMixerRed, channelMixers.ChannelMixerRed);
    _postFxMaterial.SetVector(kMatChannelMixerGreen, channelMixers.ChannelMixerGreen);
    _postFxMaterial.SetVector(kMatChannelMixerBlue, channelMixers.ChannelMixerBlue);

    RenderTexture currentSource = source;
    RenderTexture currentDestination = destination;

    // Do bloom
    if (bloom.Enabled)
    {
      // Initialize render textures if necessary
      UpdateRenderTextures(source);

      // Downsample
      int iteration = 0;
      for (; iteration < _blurSteps.Count && iteration < BloomIterations; ++iteration)
      {
        currentDestination = _blurSteps[iteration];
        Graphics.Blit(currentSource, currentDestination, _postFxMaterial, iteration == 0 ? kPassDownPrefilter : kPassDown);
        currentSource = currentDestination;
      }

      // Upsample 
      for (iteration -= 2; iteration >= 0; iteration--)
      {
        currentDestination = _blurSteps[iteration];
        Graphics.Blit(currentSource, currentDestination, _postFxMaterial, kPassUp);
        currentSource = currentDestination;
      }
    }

    // Final blit
    _postFxMaterial.SetTexture(kMatSourceTex, source);
    Graphics.Blit(currentSource, destination, _postFxMaterial, kPassFinal);
  }
}