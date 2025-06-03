using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Runtime audio generator for the Moody Shapes game.
/// This component creates simple procedural audio to use for testing and development
/// when actual audio assets are not yet available.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class AudioPlaceholderGenerator : MonoBehaviour
{
    private AudioSource _audioSource;
    private Dictionary<EmotionType, AudioClip> _generatedClips = new Dictionary<EmotionType, AudioClip>();
    
    [SerializeField] private bool _generateOnStart = true;
    [SerializeField] private int _sampleRate = 44100;
    [SerializeField] private float _defaultClipLength = 3.0f;
    
    private void Awake()
    {
        _audioSource = GetComponent<AudioSource>();
    }
    
    private void Start()
    {
        if (_generateOnStart)
        {
            GenerateAllEmotionSounds();
        }
    }
    
    /// <summary>
    /// Generates placeholder audio clips for all emotion types.
    /// </summary>
    public void GenerateAllEmotionSounds()
    {
        // Generate for each emotion type
        GenerateEmotionSound(EmotionType.Happy, 440.0f, 550.0f); // A4 to C#5
        GenerateEmotionSound(EmotionType.Sad, 196.0f, 220.0f);   // G3 to A3 (lower, minor feel)
        GenerateEmotionSound(EmotionType.Angry, 311.1f, 329.6f); // D#4 to E4 (dissonant)
        GenerateEmotionSound(EmotionType.Scared, 293.7f, 370.0f); // D4 to F#4 (tense)
        GenerateEmotionSound(EmotionType.Surprised, 587.3f, 659.3f); // D5 to E5 (higher)
        GenerateEmotionSound(EmotionType.Calm, 261.6f, 329.6f);  // C4 to E4 (major third)
        GenerateEmotionSound(EmotionType.Frustrated, 349.2f, 311.1f); // F4 to D#4 (descending, tense)
        GenerateEmotionSound(EmotionType.Joyful, 523.3f, 659.3f); // C5 to E5 (higher, major)
        GenerateEmotionSound(EmotionType.Curious, 392.0f, 440.0f); // G4 to A4 (questioning feel)
    }
    
    /// <summary>
    /// Generates a simple audio clip for a specific emotion.
    /// </summary>
    public AudioClip GenerateEmotionSound(EmotionType emotion, float baseFreq, float secondFreq)
    {
        int samples = Mathf.FloorToInt(_sampleRate * _defaultClipLength);
        float[] data = new float[samples];
        
        // Different patterns for different emotions
        switch (emotion)
        {
            case EmotionType.Happy:
            case EmotionType.Joyful:
                GenerateMajorPattern(data, baseFreq, secondFreq);
                break;
                
            case EmotionType.Sad:
                GenerateMinorPattern(data, baseFreq, secondFreq);
                break;
                
            case EmotionType.Angry:
            case EmotionType.Frustrated:
                GenerateDissonantPattern(data, baseFreq, secondFreq);
                break;
                
            case EmotionType.Scared:
                GenerateTremoloPattern(data, baseFreq, secondFreq);
                break;
                
            case EmotionType.Surprised:
                GenerateUpliftPattern(data, baseFreq, secondFreq);
                break;
                
            case EmotionType.Calm:
                GenerateSmoothPattern(data, baseFreq, secondFreq);
                break;
                
            case EmotionType.Curious:
                GenerateQuestioningPattern(data, baseFreq, secondFreq);
                break;
                
            default:
                GenerateNeutralTone(data, baseFreq);
                break;
        }
        
        // Create the clip
        AudioClip clip = AudioClip.Create(
            emotion.ToString() + "_Generated", 
            samples, 
            1, // mono
            _sampleRate, 
            false // not streaming
        );
        
        clip.SetData(data, 0);
        _generatedClips[emotion] = clip;
        
        return clip;
    }
    
    /// <summary>
    /// Gets a previously generated audio clip for the specified emotion.
    /// </summary>
    public AudioClip GetEmotionClip(EmotionType emotion)
    {
        if (_generatedClips.TryGetValue(emotion, out AudioClip clip))
        {
            return clip;
        }
        
        return null;
    }
    
    /// <summary>
    /// Plays the audio clip associated with the specified emotion.
    /// </summary>
    public void PlayEmotionSound(EmotionType emotion)
    {
        AudioClip clip = GetEmotionClip(emotion);
        
        if (clip != null)
        {
            _audioSource.clip = clip;
            _audioSource.Play();
        }
    }
    
    #region Audio Generation Patterns
    
    private void GenerateNeutralTone(float[] data, float frequency)
    {
        float amplitude = 0.3f;
        for (int i = 0; i < data.Length; i++)
        {
            float t = (float)i / _sampleRate;
            data[i] = amplitude * Mathf.Sin(2 * Mathf.PI * frequency * t);
            
            // Apply fade in/out
            float fadeTime = 0.1f;
            float fadeIn = Mathf.Min(1, t / fadeTime);
            float fadeOut = Mathf.Min(1, (_defaultClipLength - t) / fadeTime);
            data[i] *= fadeIn * fadeOut;
        }
    }
    
    private void GenerateMajorPattern(float[] data, float freq1, float freq2)
    {
        float amplitude = 0.3f;
        float mod = 2.0f; // modulation speed
        
        for (int i = 0; i < data.Length; i++)
        {
            float t = (float)i / _sampleRate;
            float time = t % _defaultClipLength;
            
            // Alternate between frequencies with smooth crossfade
            float blend = 0.5f * (1 + Mathf.Sin(2 * Mathf.PI * mod * time));
            float freq = Mathf.Lerp(freq1, freq2, blend);
            
            // Add a bit of tremolo for liveliness
            float tremolo = 0.9f + 0.1f * Mathf.Sin(2 * Mathf.PI * 8 * time);
            
            data[i] = amplitude * tremolo * Mathf.Sin(2 * Mathf.PI * freq * t);
            
            // Apply envelope
            float attack = 0.1f;
            float decay = 0.2f;
            float fadeOut = 0.3f;
            
            if (time < attack)
                data[i] *= time / attack;
            else if (time > _defaultClipLength - fadeOut)
                data[i] *= (_defaultClipLength - time) / fadeOut;
        }
    }
    
    private void GenerateMinorPattern(float[] data, float freq1, float freq2)
    {
        float amplitude = 0.3f;
        
        for (int i = 0; i < data.Length; i++)
        {
            float t = (float)i / _sampleRate;
            float time = t % _defaultClipLength;
            
            // Slowly descending pitch
            float pitchBend = 1.0f - 0.05f * (time / _defaultClipLength);
            float freq = Mathf.Lerp(freq1, freq2, time / _defaultClipLength) * pitchBend;
            
            // Add vibrato for expressiveness
            float vibrato = 1.0f + 0.01f * Mathf.Sin(2 * Mathf.PI * 5 * time);
            
            data[i] = amplitude * Mathf.Sin(2 * Mathf.PI * freq * vibrato * t);
            
            // Apply envelope with longer attack
            float attack = 0.3f;
            float fadeOut = 0.5f;
            
            if (time < attack)
                data[i] *= time / attack;
            else if (time > _defaultClipLength - fadeOut)
                data[i] *= (_defaultClipLength - time) / fadeOut;
        }
    }
    
    private void GenerateDissonantPattern(float[] data, float freq1, float freq2)
    {
        float amplitude = 0.25f;
        
        for (int i = 0; i < data.Length; i++)
        {
            float t = (float)i / _sampleRate;
            float time = t % _defaultClipLength;
            
            // Mix slightly detuned frequencies for dissonance
            float detune = 1.01f;
            float signal1 = Mathf.Sin(2 * Mathf.PI * freq1 * t);
            float signal2 = Mathf.Sin(2 * Mathf.PI * freq2 * detune * t);
            
            // Add some noise pulse
            float noise = (Random.value * 2 - 1) * 0.1f * Mathf.Max(0, Mathf.Sin(2 * Mathf.PI * 2 * time));
            
            data[i] = amplitude * (signal1 * 0.6f + signal2 * 0.4f + noise);
            
            // Apply aggressive envelope
            float attack = 0.05f;
            float decay = 0.3f;
            float sustain = 0.7f;
            float release = 0.2f;
            
            if (time < attack)
                data[i] *= time / attack;
            else if (time < attack + decay)
                data[i] *= 1.0f - 0.3f * ((time - attack) / decay);
            else if (time > _defaultClipLength - release)
                data[i] *= (_defaultClipLength - time) / release;
        }
    }
    
    private void GenerateTremoloPattern(float[] data, float freq1, float freq2)
    {
        float amplitude = 0.3f;
        float tremoloRate = 12.0f;
        
        for (int i = 0; i < data.Length; i++)
        {
            float t = (float)i / _sampleRate;
            float time = t % _defaultClipLength;
            
            // Pitch drops slightly over time for fear effect
            float freqMod = 1.0f - 0.08f * (time / _defaultClipLength);
            float freq = Mathf.Lerp(freq1, freq2, 0.5f + 0.5f * Mathf.Sin(2 * Mathf.PI * 0.5f * time)) * freqMod;
            
            // Strong tremolo
            float tremolo = 0.5f + 0.5f * Mathf.Sin(2 * Mathf.PI * tremoloRate * time);
            tremolo = Mathf.Pow(tremolo, 0.7f); // Shape the tremolo
            
            data[i] = amplitude * tremolo * Mathf.Sin(2 * Mathf.PI * freq * t);
            
            // Apply envelope with quick attack
            float attack = 0.03f;
            float decay = 0.1f;
            float release = 0.4f;
            
            if (time < attack)
                data[i] *= time / attack;
            else if (time < attack + decay)
                data[i] *= 1.0f - 0.2f * ((time - attack) / decay);
            else if (time > _defaultClipLength - release)
                data[i] *= (_defaultClipLength - time) / release;
        }
    }
    
    private void GenerateUpliftPattern(float[] data, float freq1, float freq2)
    {
        float amplitude = 0.3f;
        
        for (int i = 0; i < data.Length; i++)
        {
            float t = (float)i / _sampleRate;
            float time = t % _defaultClipLength;
            
            // Quick upward frequency sweep
            float sweep = Mathf.Min(1.0f, time * 4 / _defaultClipLength);
            float freq = Mathf.Lerp(freq1, freq2, sweep);
            
            // Add brightness with harmonics
            float signal = Mathf.Sin(2 * Mathf.PI * freq * t);
            float harmonic = 0.3f * Mathf.Sin(2 * Mathf.PI * freq * 2 * t); // Octave up
            
            data[i] = amplitude * (signal + harmonic);
            
            // Apply percussive envelope
            float attack = 0.02f;
            float decay = 0.5f;
            
            if (time < attack)
                data[i] *= time / attack;
            else if (time > attack)
                data[i] *= Mathf.Pow(1.0f - Mathf.Min(1.0f, (time - attack) / decay), 0.7f);
        }
    }
    
    private void GenerateSmoothPattern(float[] data, float freq1, float freq2)
    {
        float amplitude = 0.25f;
        
        for (int i = 0; i < data.Length; i++)
        {
            float t = (float)i / _sampleRate;
            float time = t % _defaultClipLength;
            
            // Gentle frequency modulation
            float freqMod = 0.5f + 0.5f * Mathf.Sin(2 * Mathf.PI * 0.25f * time);
            float freq = Mathf.Lerp(freq1, freq2, freqMod);
            
            // Soft tone with limited harmonics
            float signal = Mathf.Sin(2 * Mathf.PI * freq * t);
            float harmonic = 0.1f * Mathf.Sin(2 * Mathf.PI * freq * 1.5f * t); // Fifth up
            
            // Soft amplitude modulation
            float am = 0.9f + 0.1f * Mathf.Sin(2 * Mathf.PI * 0.5f * time);
            
            data[i] = amplitude * am * (signal + harmonic);
            
            // Apply smooth envelope
            float attack = 0.3f;
            float release = 0.7f;
            
            if (time < attack)
                data[i] *= Mathf.Pow(time / attack, 2); // Curved attack
            else if (time > _defaultClipLength - release)
                data[i] *= Mathf.Pow((_defaultClipLength - time) / release, 2); // Curved release
        }
    }
    
    private void GenerateQuestioningPattern(float[] data, float freq1, float freq2)
    {
        float amplitude = 0.3f;
        
        for (int i = 0; i < data.Length; i++)
        {
            float t = (float)i / _sampleRate;
            float time = t % _defaultClipLength;
            
            // Upward pitch at the end like a question
            float pitchCurve;
            if (time < _defaultClipLength * 0.7f)
                pitchCurve = 0;
            else
                pitchCurve = (time - _defaultClipLength * 0.7f) / (_defaultClipLength * 0.3f);
            
            float freq = Mathf.Lerp(freq1, freq2, pitchCurve * pitchCurve);
            
            data[i] = amplitude * Mathf.Sin(2 * Mathf.PI * freq * t);
            
            // Apply envelope
            float attack = 0.1f;
            float decay = 0.2f;
            float release = 0.3f;
            
            if (time < attack)
                data[i] *= time / attack;
            else if (time < attack + decay)
                data[i] *= 1.0f - 0.1f * ((time - attack) / decay);
            else if (time > _defaultClipLength - release)
                data[i] *= (_defaultClipLength - time) / release;
        }
    }
    
    #endregion
}
