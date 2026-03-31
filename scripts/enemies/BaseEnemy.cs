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

    public EnemyHealthState Health { get; private set; } = null!;

    private MeshInstance3D? _mesh;
    private ShaderMaterial? _flashMaterial;
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

        _mesh = GetNodeOrNull<MeshInstance3D>("MeshInstance3D");
        SetupFlashMaterial();

        _gemScene = GD.Load<PackedScene>("res://scenes/pickups/GemPickup.tscn");

        var contactArea = GetNodeOrNull<Area3D>("ContactArea");
        if (contactArea != null)
            contactArea.BodyEntered += OnContactBodyEntered;

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

        Vector3 direction = player.GlobalPosition - GlobalPosition;
        direction.Y = 0;

        if (direction.LengthSquared() < 0.25f) return;

        direction = direction.Normalized();
        Velocity = direction * MoveSpeed;
        MoveAndSlide();
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
        SpawnDeathParticles();
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

        if (_flashTimer > 0f)
        {
            _flashTimer -= dt;
            float intensity = Mathf.Clamp(_flashTimer / FlashDecayDuration, 0f, 1f);
            SetFlashIntensity(intensity);
        }
        else if (Health.IsLowHealth)
        {
            _lowHealthTime += dt;
            float osc = (Mathf.Sin(_lowHealthTime * Mathf.Tau * LowHealthFrequency) + 1f) * 0.5f;
            SetFlashIntensity(osc * LowHealthMaxIntensity);
        }
        else
        {
            SetFlashIntensity(0f);
        }
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
