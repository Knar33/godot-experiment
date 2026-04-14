using System;
using Godot;
using GodotExperiment.Combat;
using GodotExperiment.Enemies;

namespace GodotExperiment;

public partial class BaseEnemy : CharacterBody3D
{
    [Export] public int MaxHealth { get; set; } = 3;
    [Export] public float MoveSpeed { get; set; } = 5f;
    [Export] public int GemDropCount { get; set; } = 1;
    [Export] public bool IsLargeEnemy { get; set; }
    [Export] public float SpawnInDuration { get; set; } = 1.0f;

    [Export] public Color DeathParticleColor { get; set; } = new Color(1f, 0.3f, 0.3f);
    [Export] public int DeathParticleCount { get; set; } = 20;
    [Export] public float DeathParticleExplosiveness { get; set; } = 0.8f;

    private float _separationRadius = 2.5f;
    private float _separationWeight = 8.0f;
    private float _separationTangent = 0.4f;

    [Export]
    public float SeparationRadius
    {
        get => _separationRadius;
        set
        {
            _separationRadius = Mathf.Max(0.01f, value);
            RebuildSeparation();
        }
    }

    [Export]
    public float SeparationWeight
    {
        get => _separationWeight;
        set
        {
            _separationWeight = Mathf.Max(0.01f, value);
            RebuildSeparation();
        }
    }

    [Export]
    public float SeparationTangent
    {
        get => _separationTangent;
        set
        {
            _separationTangent = Mathf.Max(0f, value);
            RebuildSeparation();
        }
    }

    public EnemyHealthState Health { get; private set; } = null!;

    protected bool SeparationEnabled { get; set; } = true;
    protected float TelegraphFlashIntensity { get; set; }

    private SeparationState _separation = null!;

    private MeshInstance3D? _mesh;
    private ShaderMaterial? _flashMaterial;
    private AudioStreamPlayer3D? _ambientAudio;
    private float _flashTimer;
    private float _lowHealthTime;
    private float _spawnInTimer;
    private bool _isActive;
    private PackedScene? _gemScene;
    private RandomNumberGenerator _rng = new();

    private const float FlashDecayDuration = 2f / 60f;
    private const float LowHealthFrequency = 2f;
    private const float LowHealthMaxIntensity = 0.3f;

    public override void _Ready()
    {
        AddToGroup("enemy");
        Health = new EnemyHealthState(MaxHealth);
        Health.Damaged += OnDamaged;
        Health.Died += OnDied;

        RebuildSeparation();

        _mesh = GetNodeOrNull<MeshInstance3D>("MeshInstance3D");
        SetupFlashMaterial();

        _gemScene = GD.Load<PackedScene>("res://scenes/pickups/GemPickup.tscn");

        var contactArea = GetNodeOrNull<Area3D>("ContactArea");
        if (contactArea != null)
            contactArea.BodyEntered += OnContactBodyEntered;

        SetupAmbientAudio();

        if (IsLargeEnemy)
        {
            _spawnInTimer = SpawnInDuration;
            _isActive = false;
        }
        else
        {
            _isActive = true;
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        float dt = (float)delta;

        if (!_isActive)
        {
            _spawnInTimer -= dt;
            if (_spawnInTimer <= 0f)
                _isActive = true;
            return;
        }

        if (Health.IsDead) return;

        MoveTowardPlayer(dt);
        UpdateFlash(dt);
    }

    public void TakeDamage(int amount = 1)
    {
        Health.TakeDamage(amount);
    }

    protected virtual void MoveTowardPlayer(float dt)
    {
        var player = GetTree().GetFirstNodeInGroup("player") as Node3D;
        if (player == null) return;

        Vector3 toPlayer = player.GlobalPosition - GlobalPosition;
        toPlayer.Y = 0;

        if (toPlayer.LengthSquared() < 0.25f) return;

        Vector3 direction = toPlayer.Normalized();
        direction = ApplySeparationSteering(direction);

        Velocity = direction * MoveSpeed;
        MoveAndSlide();
    }

    protected Vector3 ApplySeparationSteering(Vector3 desiredDirection)
    {
        if (!SeparationEnabled) return desiredDirection;

        var sepForce = ComputeSeparationFromGroup();
        Vector3 separation3D = new(sepForce.X, 0, sepForce.Y);

        Vector3 blended = desiredDirection + separation3D;
        return blended.LengthSquared() > 0.0001f ? blended.Normalized() : desiredDirection;
    }

    private System.Numerics.Vector2 ComputeSeparationFromGroup()
    {
        var enemies = GetTree().GetNodesInGroup("enemy");
        Span<System.Numerics.Vector2> neighbors = stackalloc System.Numerics.Vector2[enemies.Count - 1];
        int count = 0;

        var myPos2D = new System.Numerics.Vector2(GlobalPosition.X, GlobalPosition.Z);

        foreach (var node in enemies)
        {
            if (node == this) continue;
            if (node is not Node3D other) continue;

            var otherPos2D = new System.Numerics.Vector2(other.GlobalPosition.X, other.GlobalPosition.Z);
            float dist = System.Numerics.Vector2.Distance(myPos2D, otherPos2D);
            if (dist > SeparationRadius) continue;

            neighbors[count++] = otherPos2D;
        }

        return _separation.ComputeSeparationForce(myPos2D, neighbors[..count]);
    }

    private void RebuildSeparation()
    {
        _separation = new SeparationState(SeparationRadius, SeparationWeight, SeparationTangent);
    }

    private void OnContactBodyEntered(Node3D body)
    {
        if (!_isActive || Health.IsDead) return;
        if (!body.IsInGroup("player")) return;

        if (body is Player player)
            player.TakeDamage(DamageSource.Contact);
    }

    private void OnDamaged()
    {
        _flashTimer = FlashDecayDuration;
        SetFlashIntensity(1.0f);
    }

    private void OnDied()
    {
        _ambientAudio?.Stop();
        SpawnDeathParticles();
        PlayDeathSound();
        DropGems();
        GameManager.Instance?.RecordEnemyKill();
        QueueFree();
    }

    private void SetupFlashMaterial()
    {
        if (_mesh == null) return;

        var shader = GD.Load<Shader>("res://assets/shaders/enemy_flash.gdshader");
        if (shader == null) return;

        Color baseColor = new(0.8f, 0.2f, 0.2f);
        var existingMaterial = _mesh.GetActiveMaterial(0);
        if (existingMaterial is StandardMaterial3D stdMat)
            baseColor = stdMat.AlbedoColor;

        _flashMaterial = new ShaderMaterial { Shader = shader };
        _flashMaterial.SetShaderParameter("albedo_color", baseColor);
        _flashMaterial.SetShaderParameter("flash_intensity", 0.0f);
        _mesh.MaterialOverride = _flashMaterial;
    }

    private void UpdateFlash(float dt)
    {
        if (_flashMaterial == null) return;

        float intensity = 0f;

        if (_flashTimer > 0f)
        {
            _flashTimer -= dt;
            intensity = Mathf.Clamp(_flashTimer / FlashDecayDuration, 0f, 1f);
        }
        else if (Health.IsLowHealth)
        {
            _lowHealthTime += dt;
            float osc = (Mathf.Sin(_lowHealthTime * Mathf.Tau * LowHealthFrequency) + 1f) * 0.5f;
            intensity = osc * LowHealthMaxIntensity;
        }

        intensity = Mathf.Max(intensity, TelegraphFlashIntensity);
        SetFlashIntensity(intensity);
    }

    private void SetFlashIntensity(float intensity)
    {
        _flashMaterial?.SetShaderParameter("flash_intensity", intensity);
    }

    private void SpawnDeathParticles()
    {
        var particles = new GpuParticles3D();
        var material = new ParticleProcessMaterial
        {
            Direction = new Vector3(0, 1, 0),
            Spread = 180f,
            InitialVelocityMin = 3f,
            InitialVelocityMax = 6f,
            Gravity = new Vector3(0, -9.8f, 0),
            Color = DeathParticleColor
        };

        particles.ProcessMaterial = material;
        particles.Amount = DeathParticleCount;
        particles.Explosiveness = DeathParticleExplosiveness;
        particles.OneShot = true;
        particles.Lifetime = 0.4f;
        particles.Emitting = true;

        var temp = new Node3D();
        GetTree().CurrentScene.AddChild(temp);
        temp.GlobalPosition = GlobalPosition + new Vector3(0, 0.5f, 0);
        temp.AddChild(particles);

        var timer = GetTree().CreateTimer(particles.Lifetime + 0.1);
        timer.Timeout += () => temp.QueueFree();
    }

    private void SetupAmbientAudio()
    {
        _ambientAudio = GetNodeOrNull<AudioStreamPlayer3D>("AmbientAudio");
        if (_ambientAudio?.Stream == null) return;

        _ambientAudio.Finished += OnAmbientFinished;
        _ambientAudio.Play();
    }

    private void OnAmbientFinished()
    {
        if (_ambientAudio != null && !Health.IsDead)
            _ambientAudio.Play();
    }

    private void PlayDeathSound()
    {
        var deathAudio = GetNodeOrNull<AudioStreamPlayer3D>("DeathAudio");
        if (deathAudio?.Stream == null) return;

        deathAudio.GetParent().RemoveChild(deathAudio);

        var temp = new Node3D();
        GetTree().CurrentScene.AddChild(temp);
        temp.GlobalPosition = GlobalPosition;
        temp.AddChild(deathAudio);
        deathAudio.Play();
        deathAudio.Finished += () => temp.QueueFree();
    }

    private void DropGems()
    {
        if (_gemScene == null || GemDropCount <= 0) return;

        for (int i = 0; i < GemDropCount; i++)
        {
            var gem = _gemScene.Instantiate<Node3D>();
            GetTree().CurrentScene.AddChild(gem);
            gem.GlobalPosition = GlobalPosition + new Vector3(0, 0.3f, 0);

            float angle = _rng.RandfRange(0f, Mathf.Tau);
            float dist = _rng.RandfRange(0.5f, 1.5f);
            Vector3 scatter = new(Mathf.Cos(angle) * dist, 0f, Mathf.Sin(angle) * dist);

            if (gem is GemPickup pickup)
                pickup.SetScatterVelocity(scatter);
        }
    }
}
