// Assets/Scripts/Gameplay/Visuals/AdvancedEmotionPhysics.cs
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
