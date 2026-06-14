using UnityEngine;
using UnityEditor;

public class CreateParticlePrefabs
{
    public static void Execute()
    {
        CreateKillBurst();
        CreateHitBurst();
        CreateClearBurst();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[JuiceManager] All 3 particle prefabs created in Assets/Prefabs/");
    }

    private static void CreateKillBurst()
    {
        var go = new GameObject("KillBurst");
        var ps = go.AddComponent<ParticleSystem>();

        var main = ps.main;
        main.startLifetime  = new ParticleSystem.MinMaxCurve(0.15f);
        main.startSpeed     = new ParticleSystem.MinMaxCurve(2f, 4f);
        main.startSize      = new ParticleSystem.MinMaxCurve(0.05f, 0.15f);
        main.startColor     = new ParticleSystem.MinMaxGradient(Color.yellow, Color.white);
        main.maxParticles   = 30;
        main.loop           = false;
        main.playOnAwake    = false;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 15) });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius    = 0.3f;

        // Remove renderer default material for clarity (will use default particle material)
        PrefabUtility.SaveAsPrefabAsset(go, "Assets/Prefabs/KillBurst.prefab");
        Object.DestroyImmediate(go);
        Debug.Log("[JuiceManager] KillBurst.prefab created");
    }

    private static void CreateHitBurst()
    {
        var go = new GameObject("HitBurst");
        var ps = go.AddComponent<ParticleSystem>();

        var main = ps.main;
        main.startLifetime  = new ParticleSystem.MinMaxCurve(0.3f);
        main.startSpeed     = new ParticleSystem.MinMaxCurve(2f, 5f);
        main.startSize      = new ParticleSystem.MinMaxCurve(0.1f, 0.2f);
        main.startColor     = new ParticleSystem.MinMaxGradient(Color.red, new Color(1f, 0.5f, 0f));
        main.maxParticles   = 50;
        main.loop           = false;
        main.playOnAwake    = false;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 25) });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius    = 0.5f;

        PrefabUtility.SaveAsPrefabAsset(go, "Assets/Prefabs/HitBurst.prefab");
        Object.DestroyImmediate(go);
        Debug.Log("[JuiceManager] HitBurst.prefab created");
    }

    private static void CreateClearBurst()
    {
        var go = new GameObject("ClearBurst");
        var ps = go.AddComponent<ParticleSystem>();

        var main = ps.main;
        main.startLifetime  = new ParticleSystem.MinMaxCurve(0.8f);
        main.startSpeed     = new ParticleSystem.MinMaxCurve(3f, 7f);
        main.startSize      = new ParticleSystem.MinMaxCurve(0.1f, 0.3f);

        // Multi-color gradient for fireworks feel
        var grad = new Gradient();
        grad.SetKeys(
            new GradientColorKey[]
            {
                new GradientColorKey(Color.cyan,    0.0f),
                new GradientColorKey(Color.yellow,  0.33f),
                new GradientColorKey(Color.magenta, 0.66f),
                new GradientColorKey(Color.white,   1.0f),
            },
            new GradientAlphaKey[]
            {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(0f, 1f),
            }
        );
        main.startColor  = new ParticleSystem.MinMaxGradient(grad);
        main.maxParticles = 120;
        main.loop         = false;
        main.playOnAwake  = false;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        var emission = ps.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new[] { new ParticleSystem.Burst(0f, 60) });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Circle;
        shape.radius    = 1.5f;

        PrefabUtility.SaveAsPrefabAsset(go, "Assets/Prefabs/ClearBurst.prefab");
        Object.DestroyImmediate(go);
        Debug.Log("[JuiceManager] ClearBurst.prefab created");
    }
}
