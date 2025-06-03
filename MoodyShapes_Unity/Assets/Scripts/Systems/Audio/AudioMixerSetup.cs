#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.Audio;
using UnityEditor;
using System.IO;

/// <summary>
/// Editor utility for creating the audio mixer structure for the Moody Shapes game.
/// This is an editor-only script that helps set up the initial audio configuration.
/// </summary>
public class AudioMixerSetup : MonoBehaviour
{
    [MenuItem("MoodyShapes/Audio/Create Audio Mixer")]
    public static void CreateAudioMixer()
    {
        // Create the main audio mixer
        AudioMixer mainMixer = AssetDatabase.LoadAssetAtPath<AudioMixer>("Assets/Audio/Mixers/MoodyShapesMixer.mixer");
        
        if (mainMixer == null)
        {
            // Create main mixer if it doesn't exist
            mainMixer = new AudioMixer();
            
            // Ensure the directories exist
            if (!Directory.Exists("Assets/Audio/Mixers"))
            {
                Directory.CreateDirectory("Assets/Audio/Mixers");
            }
            
            // Create the asset
            AssetDatabase.CreateAsset(mainMixer, "Assets/Audio/Mixers/MoodyShapesMixer.mixer");
            Debug.Log("Created main audio mixer at Assets/Audio/Mixers/MoodyShapesMixer.mixer");
            
            // Note: Due to how Unity creates AudioMixers, we need to manually add groups through the UI
            // This script will just create the initial asset
            EditorUtility.DisplayDialog("Audio Mixer Created", 
                "The main audio mixer has been created. Please open it and add the following groups:\n\n" +
                "- Master\n" +
                "  - Music\n" +
                "    - MusicLayers\n" +
                "    - MusicDefault\n" +
                "  - SFX\n" +
                "    - EmotionSFX\n" +
                "    - UiSFX\n" +
                "    - AmbientSFX\n", 
                "OK");
        }
        else
        {
            EditorUtility.DisplayDialog("Audio Mixer Exists", 
                "The audio mixer already exists at Assets/Audio/Mixers/MoodyShapesMixer.mixer", 
                "OK");
        }
    }
    
    [MenuItem("MoodyShapes/Audio/Create Placeholder Audio Files")]
    public static void CreatePlaceholderAudioFiles()
    {
        string[] emotionTypes = new string[] {
            "Happy", "Sad", "Angry", "Scared", "Surprised", "Calm", "Frustrated", "Joyful", "Curious"
        };
        
        // Create placeholder files for emotion SFX
        foreach (string emotion in emotionTypes)
        {
            string directory = $"Assets/Audio/SFX/Emotions/{emotion}";
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            
            CreateEmptyAudioFile($"{directory}/{emotion}_Reaction.wav");
            CreateEmptyAudioFile($"{directory}/{emotion}_Ambient.wav");
        }
        
        // Create placeholder files for music layers
        foreach (string emotion in emotionTypes)
        {
            string directory = "Assets/Audio/Music/Layers";
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            
            CreateEmptyAudioFile($"{directory}/{emotion}_Layer.wav");
        }
        
        // Create default music
        string musicDir = "Assets/Audio/Music";
        if (!Directory.Exists(musicDir))
        {
            Directory.CreateDirectory(musicDir);
        }
        CreateEmptyAudioFile($"{musicDir}/DefaultTheme.wav");
        
        AssetDatabase.Refresh();
        Debug.Log("Created placeholder audio files. Replace them with actual audio content.");
    }
    
    private static void CreateEmptyAudioFile(string path)
    {
        // Note: We can't actually create audio files here.
        // Instead, we'll create an empty text file as a placeholder
        string textPath = path + ".placeholder.txt";
        File.WriteAllText(textPath, "Replace this placeholder with an actual audio file.");
        Debug.Log($"Created placeholder for: {path}");
    }
}
#endif
