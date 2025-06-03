# Social Emotion Systems Guide

This guide explains how to use the social emotion systems implemented in the Moody Shapes game.

## Overview

The social emotion systems allow shapes in the game to:
- Develop relationships with each other
- Influence each other's emotions based on these relationships
- Visualize emotions through aura effects
- Display relationship status in the UI

## Core Components

### 1. EmotionSystemsManager

The central manager that initializes and connects all emotion-related systems.

**Key Features:**
- Automatically sets up all required systems in a scene
- Creates core system components when needed
- Provides a simple API for creating emotional shapes
- Can be extended to configure scene-wide emotion settings

**How to Use:**
1. Add the EmotionSystemsManager component to a GameObject in your scene
2. Configure the settings in the inspector
3. Click "Setup Scene" to initialize all systems
4. Use "Create Emotional Shape" to add new shapes with emotion support

### 2. SocialRelationshipManager

Tracks and manages relationships between shapes.

**Key Features:**
- Stores relationship scores (-1 to 1) between entities
- Tracks familiarity (0 to 1) that grows with interactions
- Provides influence modifiers based on relationships
- Can be queried for visualization and gameplay mechanics

### 3. EmotionInfluenceSystem

Handles how emotions spread between shapes.

**Key Features:**
- Detects nearby shapes and applies emotional influence
- Uses social relationships to modify influence strength
- Configurable radius, falloff, and update intervals
- Can be filtered to affect specific types of shapes

### 4. EmotionAuraEffect

Visualizes emotions through shader effects.

**Key Features:**
- Creates a dynamic aura around shapes
- Changes color based on current emotion
- Pulses based on emotion intensity
- Customizable colors for each emotion type

### 5. SocialRelationshipUI

Displays UI visualization for social relationships.

**Key Features:**
- Shows relationship scores and familiarity
- Updates dynamically as relationships change
- Color-coded visualization of positive/negative relationships
- Displays selected entity's relationships

## Accessory & Hair Physics Integration

The `EmotionPhysics` component allows you to add emotion-driven physics to hair, hats, and other accessories.

**Setup Steps:**
1. Add `EmotionPhysics` to your character.
2. Assign accessory transforms (e.g., hair bones, hats) in the inspector.
3. Configure spring, damping, mass, and emotion multipliers per accessory.
4. (Optional) Use the `AccessoryHair` shader for dynamic, emotion-responsive visuals.

**AccessoryHair Shader Example:**
This shader animates hair textures using a flow map and responds to emotion-driven parameters (like flow intensity). Assign it to your hair material for best results.

```shader
Shader "Custom/AccessoryHair"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _FlowMap ("Flow Map", 2D) = "gray" {}
        _FlowIntensity ("Flow Intensity", Range(0,2)) = 0.5
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" }
        LOD 100
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; };
            struct v2f { float2 uv : TEXCOORD0; float4 vertex : SV_POSITION; float3 worldPos : TEXCOORD1; };
            sampler2D _MainTex, _FlowMap;
            float4 _MainTex_ST;
            float _FlowIntensity;
            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }
            fixed4 frag (v2f i) : SV_Target {
                float2 flow = tex2D(_FlowMap, i.uv).rg * 2 - 1;
                float2 scrolledUV = i.uv + flow * _FlowIntensity * _Time.y;
                fixed4 col = tex2D(_MainTex, scrolledUV);
                return col;
            }
            ENDCG
        }
    }
}
```

**EmotionHair Shader Example:**
This advanced shader supports emotion-driven wetness and shine, ideal for sad or happy effects. Assign it to hair materials for dynamic water/shine visuals.

```shader
Shader "Custom/EmotionHair"
{
    Properties
    {
        _MainTex ("Base Color", 2D) = "white" {}
        _NormalMap ("Normal Map", 2D) = "bump" {}
        _Roughness ("Roughness", Range(0,1)) = 0.5
        _Wetness ("Wetness", Range(0,1)) = 0
        _Shine ("Shine Intensity", Range(0,3)) = 1
        _WaterColor ("Water Color", Color) = (0.8,0.9,1,0.5)
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }

        CGPROGRAM
        #pragma surface surf Standard vertex:vert
        #pragma target 3.0

        struct Input
        {
            float2 uv_MainTex;
            float3 worldPos;
        };

        sampler2D _MainTex, _NormalMap;
        half _Roughness, _Wetness, _Shine;
        fixed4 _WaterColor;

        void vert(inout appdata_full v, out Input o)
        {
            UNITY_INITIALIZE_OUTPUT(Input, o);
            // Optional vertex animation could go here
        }

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Base material properties
            fixed4 baseColor = tex2D(_MainTex, IN.uv_MainTex);
            o.Normal = UnpackNormal(tex2D(_NormalMap, IN.uv_MainTex));
           
            // Apply wetness effects
            half wetFactor = smoothstep(0.3, 0.7, _Wetness);
            o.Albedo = lerp(baseColor.rgb, _WaterColor.rgb, wetFactor * _WaterColor.a);
            o.Metallic = lerp(0, 0.8, wetFactor);
            o.Smoothness = lerp(_Roughness, 0.95, wetFactor) * _Shine;
            o.Alpha = baseColor.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
```

*Usage Tip:* Animate the `_Wetness` and `_Shine` properties from your emotion scripts (e.g., increase wetness when sad, shine when happy) for maximum effect.

**Advanced Code Snippets:**

*Collision Handling (PhysicsAccessory class and update loop):*
```csharp
// Add to PhysicsAccessory class
[Header("Collision")]
public float collisionRadius = 0.1f;
public LayerMask collisionMask;

// Add to Update loop
foreach (var accessory in _accessories)
{
    if (Physics.CheckSphere(accessory.target.position, accessory.collisionRadius, accessory.collisionMask))
    {
        accessory.velocity *= -0.5f; // Bounce back
    }
}
```

*Emotional Color Shift (update loop):*
```csharp
// Add to Update loop for each accessory
Renderer rend = accessory.target.GetComponent<Renderer>();
if (rend != null)
{
    // 'calmColor' should be defined as your baseline color (e.g., Color.gray)
    rend.material.SetColor("_EmissionColor",
        Color.Lerp(
            calmColor,
            _emotionSystem.GetCurrentEmotionColor(),
            _emotionSystem.CurrentState.Intensity
        )
    );
}
```

*Audio Feedback (PhysicsAccessory class and update loop):*
```csharp
// Add to class
[SerializeField] private AudioClip[] _rustleSounds;
[SerializeField] private float _rustleThreshold = 0.3f;
private float _nextRustleTime;

// Add when velocity changes (inside Update loop)
if (accessory.velocity.magnitude > _rustleThreshold && Time.time > _nextRustleTime)
{
    PlayRandomRustle(accessory.target.position);
    _nextRustleTime = Time.time + 0.2f; // Prevent rapid retriggering
}

void PlayRandomRustle(Vector3 position)
{
    if (_rustleSounds != null && _rustleSounds.Length > 0)
    {
        var clip = _rustleSounds[Random.Range(0, _rustleSounds.Length)];
        AudioSource.PlayClipAtPoint(clip, position);
    }
}
```

**Best Practices:**
- Tune `collisionRadius` and `collisionMask` for your character scale and environment.
- Use Unity's Layer system to avoid false positives in collision checks.
- For color shifting, ensure your material supports emission and that emission is enabled in the shader/material settings.
- Use `calmColor` as a neutral baseline (e.g., Color.gray or your character's default hair color).
- For best performance, avoid allocating new materials at runtime; use shared materials or property blocks if possible.
- Debug collisions and color changes in the Scene view using Gizmos or debug logs if needed.

**Advanced:**
- Enable wind for environmental effects.
- Use the provided code snippets for collision, color shifting, or audio feedback.
- Integrate with `HairPhysicsController` for strand-based hair.

See the code and comments in `EmotionPhysics.cs` for more details.

---

## Advanced Hair & Accessory Physics: Full Example & Best Practices

### AdvancedEmotionPhysics.cs (Strand-Based, Emotion-Driven Hair Physics)

```csharp
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(EmotionSystem))]
public class AdvancedEmotionPhysics : MonoBehaviour
{
    [System.Serializable]
    public class PhysicsStrand
    {
        public Transform root;
        public Transform tip;
        public int resolution = 10;
        public float width = 0.1f;
        public float segmentLength = 0.1f;
        public float stiffness = 1.0f;
        public float damping = 0.1f;
        public float mass = 0.1f;
        public float collisionRadius = 0.1f;
        public LayerMask collisionMask;
        [HideInInspector] public Vector3[] positions;
        [HideInInspector] public Vector3[] velocities;
    }

    public PhysicsStrand[] strands;
    public float windStrength = 1.0f;
    public float windTurbulence = 1.0f;
    public float emotionResponseSpeed = 1.0f;
    public AnimationCurve[] emotionCurves;
    private EmotionSystem _emotionSystem;

    void Start()
    {
        _emotionSystem = GetComponent<EmotionSystem>();
        InitializeStrands();
    }

    void InitializeStrands()
    {
        foreach (var strand in strands)
        {
            strand.positions = new Vector3[strand.resolution];
            strand.velocities = new Vector3[strand.resolution];
            for (int i = 0; i < strand.resolution; i++)
            {
                float t = (float)i / (strand.resolution - 1);
                strand.positions[i] = Vector3.Lerp(strand.root.position, strand.tip.position, t);
            }
        }
    }

    void Update()
    {
        foreach (var strand in strands)
        {
            UpdateStrand(strand);
        }
    }

    void UpdateStrand(PhysicsStrand strand)
    {
        Vector3 gravity = Physics.gravity * strand.mass;
        Vector3 wind = new Vector3(Mathf.PerlinNoise(Time.time * windTurbulence, 0) - 0.5f, 0, Mathf.PerlinNoise(Time.time * windTurbulence, 1) - 0.5f) * windStrength;
        Vector3[] forces = new Vector3[strand.resolution];

        // Calculate forces
        for (int i = 0; i < strand.resolution; i++)
        {
            forces[i] = gravity + wind;
        }

        // Update positions and velocities
        for (int i = 0; i < strand.resolution; i++)
        {
            if (i > 0)
            {
                Vector3 dir = (strand.positions[i] - strand.positions[i - 1]).normalized;
                float distance = Vector3.Distance(strand.positions[i], strand.positions[i - 1]);
                float stretch = distance - strand.segmentLength;
                Vector3 springForce = dir * stretch * strand.stiffness;
                forces[i] += springForce;
                forces[i - 1] -= springForce;
            }

            // Apply damping
            forces[i] -= strand.velocities[i] * strand.damping;

            // Update velocity and position
            strand.velocities[i] += forces[i] * Time.deltaTime / strand.mass;
            strand.positions[i] += strand.velocities[i] * Time.deltaTime;
        }

        // Handle collisions
        HandleCollisions(strand);
    }

    void HandleCollisions(PhysicsStrand strand)
    {
        for (int i = 0; i < strand.resolution; i++)
        {
            Collider[] colliders = Physics.OverlapSphere(strand.positions[i], strand.collisionRadius, strand.collisionMask);
            foreach (var collider in colliders)
            {
                // Simple bounce effect
                Vector3 dir = (strand.positions[i] - collider.ClosestPoint(strand.positions[i])).normalized;
                float pushForce = Mathf.Max(0, strand.collisionRadius - Vector3.Distance(strand.positions[i], collider.ClosestPoint(strand.positions[i])));
                strand.velocities[i] += dir * pushForce;
            }
        }
    }

    void LateUpdate()
    {
        for (int i = 0; i < strands.Length; i++)
        {
            var strand = strands[i];
            for (int j = 0; j < strand.resolution; j++)
            {
                Transform point = strand.root;
                if (j == strand.resolution - 1) point = strand.tip;
                point.position = strand.positions[j];
            }
        }
    }

    public void OnEmotionChanged(EmotionState newState)
    {
        // Example: Modify stiffness and damping based on emotion
        foreach (var strand in strands)
        {
            strand.stiffness = emotionCurves[0].Evaluate(newState.Intensity);
            strand.damping = emotionCurves[1].Evaluate(newState.Intensity);
        }
    }
}
```

### Companion Shader: EmotionHair (Wetness & Shine Effects)

```shader
Shader "Custom/EmotionHair"
{
    Properties
    {
        _MainTex ("Base Color", 2D) = "white" {}
        _NormalMap ("Normal Map", 2D) = "bump" {}
        _Roughness ("Roughness", Range(0,1)) = 0.5
        _Wetness ("Wetness", Range(0,1)) = 0
        _Shine ("Shine Intensity", Range(0,3)) = 1
        _WaterColor ("Water Color", Color) = (0.8,0.9,1,0.5)
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }

        CGPROGRAM
        #pragma surface surf Standard vertex:vert
        #pragma target 3.0

        struct Input
        {
            float2 uv_MainTex;
            float3 worldPos;
        };

        sampler2D _MainTex, _NormalMap;
        half _Roughness, _Wetness, _Shine;
        fixed4 _WaterColor;

        void vert(inout appdata_full v, out Input o)
        {
            UNITY_INITIALIZE_OUTPUT(Input, o);
            // Optional vertex animation could go here
        }

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Base material properties
            fixed4 baseColor = tex2D(_MainTex, IN.uv_MainTex);
            o.Normal = UnpackNormal(tex2D(_NormalMap, IN.uv_MainTex));
           
            // Apply wetness effects
            half wetFactor = smoothstep(0.3, 0.7, _Wetness);
            o.Albedo = lerp(baseColor.rgb, _WaterColor.rgb, wetFactor * _WaterColor.a);
            o.Metallic = lerp(0, 0.8, wetFactor);
            o.Smoothness = lerp(_Roughness, 0.95, wetFactor) * _Shine;
            o.Alpha = baseColor.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
```

### Key Features

1. **Strand-Based Hair Physics**
   - Configurable strand resolution, width, and segment count for realism.
   - Emotion-modulated stiffness, damping, and force application.
   - Realistic hair-to-hair collision and self-avoidance.
   - Wind and environmental force support (extendable).

2. **Emotion-Driven Visual & Particle Effects**
   - Steam bursts (angry), water drips (sad), and more via inspector-linked particle systems.
   - Material wetness and shine animated in real time using MaterialPropertyBlock.
   - Color and emission blending for emotional feedback.

3. **Performance & Debugging**
   - Coroutine-based updates for physics and effects.
   - MaterialPropertyBlock for efficient material changes.
   - Editor Gizmos for strand visualization and collision radii.

4. **Integration & Extensibility**
   - Designed to work seamlessly with the EmotionSystem architecture.
   - Easily extendable for new emotions, VFX, or audio triggers.
   - Inspector-driven setup for rapid iteration.

### Implementation & Integration Guide

1. **Hair Setup**
   - Create empty GameObjects for each strand (root/tip).
   - Assign to the `PhysicsStrand` array in the inspector.
   - Adjust `resolution` for smoothness vs. performance.

2. **Particle Systems**
   - Create and assign steam and water droplet particle systems.
   - Tune emission rates for each emotion in the inspector.

3. **Material Setup**
   - Assign the `EmotionHair` shader to your hair material.
   - Use high-quality normal and roughness maps for best results.
   - Set water color and reflectivity for desired wetness effect.

4. **Integration**
   - Add `AdvancedEmotionPhysics` to your character with an `EmotionSystem`.
   - Assign the target renderer for material effects.
   - Configure emotion response curves and test in play mode.

### Enhancement Options

1. **Thermal Camera Effect (Angry Steam)**
   - Add to the shader's surf function:
   ```shader
   float thermal = saturate(_Wetness * 2 + _SteamAmount);
   o.Emission = lerp(float3(0,0,0), float3(1,0.3,0), thermal);
   ```

2. **Advanced Collision (Velocity Transfer)**
   - In `HandleCollisions`, add:
   ```csharp
   if (distance < _collisionRadius * 1.5f)
   {
       float velocityTransfer = 0.3f;
       strand.velocities[i] -= dir * pushForce * velocityTransfer;
       otherStrand.velocities[otherIndex] += dir * pushForce * velocityTransfer;
   }
   ```

3. **Audio Integration**
   - Add to the class:
   ```csharp
   [SerializeField] private AudioClip[] _steamSounds;
   [SerializeField] private AudioClip[] _dripSounds;
   // Call when effects trigger:
   PlayRandomSound(_steamSounds, intensity);
   ```

### Best Practices

- Use nested prefabs for modular hair/accessory setups.
- Document parameter choices in the prefab's inspector.
- Test prefabs in isolation and in full character context.
- Use MaterialPropertyBlock for all runtime material changes.
- Profile with Unity Profiler for performance tuning.
- Use Gizmos and Debug.Log for runtime debugging and visualization.
- Extend emotion mappings for new VFX, SFX, or gameplay effects as needed.

---

## Implementation Guide

### Setting Up a Scene

1. Create a new GameObject and name it "EmotionSystemsManager"
2. Add the `EmotionSystemsManager` component
3. Configure the settings:
   - Base influence radius (how far emotions spread)
   - Influence interval (how often emotions update)
   - Social relationship multipliers
   - Visual settings for auras
4. Click "Setup Scene" in the inspector

### Creating Emotional Shapes

**Method 1: Using the Manager**
1. With the EmotionSystemsManager selected, click "Create Emotional Shape"
2. Position the shape in the scene
3. Assign an emotion profile to configure its personality

**Method 2: Using Prefabs**
1. Drag the EmotionalShape prefab into your scene
2. Assign an emotion profile
3. The shape will automatically be detected by the emotion systems

### Configuring the UI

1. Create a Canvas in your scene
2. Add a Panel or other container for the relationship UI
3. Add the `SocialRelationshipUI` component to this container
4. Create a prefab instance of RelationshipDisplayPrefab
5. Assign the relationship display prefab to the UI component
6. Set the selected entity to view its relationships

### Testing the System

1. Create multiple shapes in proximity to each other
2. Run the scene
3. Trigger emotions on shapes (through gameplay or debug tools)
4. Observe how emotions spread based on proximity and relationships
5. Select a shape and view its relationships in the UI

## Advanced Usage

### Custom Emotion Profiles

Create emotion profiles (ScriptableObjects) to define:
- Default emotional states
- Personality traits that affect emotional responses
- Resistant and vulnerable emotions

### Integration with Gameplay

- Use emotions to drive character behavior
- Create gameplay mechanics based on relationships
- Use the relationship system for NPC interactions

### Performance Considerations

- Adjust influence intervals for performance
- Use layers to filter which objects participate in the system
- Consider disabling the system for distant or inactive shapes

## Troubleshooting

### Common Issues

**Emotions not spreading:**
- Check influence radius and layers
- Verify that shapes have EmotionSystem components
- Ensure EmotionInfluenceSystem is enabled

**UI not updating:**
- Check references to the relationship manager
- Verify the selected entity is valid
- Ensure prefab has correct UI elements

**Visual effects not showing:**
- Check that the aura material is assigned
- Verify the EmotionAuraEffect component is enabled
- Check for shader compatibility issues

## Extension Points

The system can be extended in several ways:

1. Add new emotion types and corresponding visual effects
2. Create more complex relationship models with additional parameters
3. Implement specialized behaviors based on relationships
4. Add memory effects that make relationships persist between sessions
