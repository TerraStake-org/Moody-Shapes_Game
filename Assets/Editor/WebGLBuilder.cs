// filepath: Assets/Editor/WebGLBuilder.cs
// This editor script provides a menu command and a static method to build the project for WebGL.
using UnityEditor;
using UnityEditor.Build.Reporting;

public static class WebGLBuilder
{
    // You can run this from the command line using:
    // Unity.exe -batchmode -projectPath "<project path>" -executeMethod WebGLBuilder.Build -quit
    [MenuItem("Build/WebGL Build Demo")]
    public static void Build()
    {
        string[] scenes = new string[] 
        {
            "Assets/Scenes/Demo/SocialEmotionsDemo.unity"
        };

        var buildOptions = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = "Build/WebGL",
            target = BuildTarget.WebGL,
            options = BuildOptions.Development | BuildOptions.AutoRunPlayer
        };

        BuildReport report = BuildPipeline.BuildPlayer(buildOptions);
        BuildSummary summary = report.summary;

        if (summary.result == BuildResult.Succeeded)
        {
            UnityEngine.Debug.Log($"WebGL build succeeded: {summary.totalSize / 1024} KB");
        }
        else
        {
            UnityEngine.Debug.LogError($"WebGL build failed: {summary.result}");
        }
    }
}
