using System.Linq;
using Content.Shared.SimpleStation14.Traits;
using Robust.Server.GameObjects;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Systems;

namespace Content.Server.SimpleStation14.Traits;

public sealed class HeightAdjustedSystem : EntitySystem
{
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly IEntityManager _entityManager = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<HeightAdjustedComponent, ComponentStartup>(Startup);
        SubscribeLocalEvent<HeightAdjustedComponent, ComponentShutdown>(Shutdown);
    }

    /// <summary>
    ///     Sets the scale and density of the entity.
    /// </summary>
    private void Startup(EntityUid uid, HeightAdjustedComponent component, ComponentStartup args)
    {
        if (!_entityManager.TryGetComponent<SpriteComponent>(uid, out var sprite)) return;
        EnsureComp<ScaleVisualsComponent>(uid);

        var oldScale = GetScale(uid, sprite);
        component.OriginalScale = (Vector2) oldScale;
        var newScale = (Vector2) oldScale * new Vector2(component.Width, component.Height);
        component.NewScale = newScale;

        SetScale(uid, newScale);

        // Get fixtures component of the entity
        // Get density of the entities fixtures in a loop
        // Multiply density by an average of the scale
        // Set density of the entity using SetDensity
        if (_entityManager.TryGetComponent<FixturesComponent>(uid, out var fixtures))
        {
            var density = fixtures.Fixtures.Values.First().Density;
            component.OriginalDensity = density;
            var newDensity = density * (component.Width + component.Height) / 2;
            component.NewDensity = newDensity;

            SetDensity(uid, newDensity);
        }
    }

    /// <summary>
    ///     Resets the scale and density of the entity.
    /// </summary>
    private void Shutdown(EntityUid uid, HeightAdjustedComponent component, ComponentShutdown args)
    {
        SetScale(uid, component.OriginalScale);
        SetDensity(uid, component.OriginalDensity);
    }



    /// <summary>
    ///     Gets the scale of the entity.
    /// </summary>
    /// <param name="uid">The entity to get the scale of.</param>
    /// <param name="sprite">The sprite component of the entity.</param>
    public Vector2 GetScale(EntityUid uid, SpriteComponent sprite)
    {
        if (!_appearance.TryGetData(uid, ScaleVisuals.Scale, out var oldScale)) oldScale = Vector2.One;

        return (Vector2) oldScale * sprite.Scale;
    }

    /// <summary>
    ///     Sets the scale of the entity and its eye zoom level.
    /// </summary>
    /// <param name="uid">The entity to set the scale of.</param>
    /// <param name="scale">The (x, y) scale to set things to.</param>
    public void SetScale(EntityUid uid, Vector2 scale)
    {
        _appearance.SetData(uid, ScaleVisuals.Scale, scale);

        if (TryComp<SharedEyeComponent>(uid, out var eye)) eye.Zoom = scale;
    }


    /// <summary>
    ///     Gets the density of the first fixture on an entity.
    /// </summary>
    /// <param name="uid">The entity to get the density of.</param>
    /// <returns>The density of the first fixture on the entity OR 1.</returns>
    public float GetDensity(EntityUid uid)
    {
        if (_entityManager.TryGetComponent<FixturesComponent>(uid, out var fixtures))
        {
            return fixtures.Fixtures.Values.First().Density;
        }

        return 1f;
    }

    /// <summary>
    ///     Gets the density of a fixture on an entity at an index.
    /// </summary>
    /// <param name="uid">The entity to get the density of.</param>
    /// <param name="fixtureIndex">The index of the fixture to get the density of.</param>
    /// <returns>The density of the fixture on the entity OR 1.</returns>
    public float GetDensity(EntityUid uid, int fixtureIndex)
    {
        if (_entityManager.TryGetComponent<FixturesComponent>(uid, out var fixtures))
        {
            return fixtures.Fixtures.Values.ElementAt(fixtureIndex).Density;
        }

        return 1f;
    }

    /// <summary>
    ///     Sets the density of the first fixture on an entity.
    /// </summary>
    /// <param name="uid">The entity to set the density of.</param>
    /// <param name="density">The density to set the fixture to.</param>
    public void SetDensity(EntityUid uid, float density)
    {
        if (_entityManager.TryGetComponent<FixturesComponent>(uid, out var fixtures))
        {
            _physics.SetDensity(uid, fixtures.Fixtures.Values.First(), density);
        }
    }

    /// <summary>
    ///     Sets the density of a fixture on an entity.
    /// </summary>
    /// <param name="uid">The entity to set the density of.</param>
    /// <param name="density">The density to set the fixture to.</param>
    /// <param name="fixtureIndex">The index of the fixture to set the density of.</param>
    public void SetDensity(EntityUid uid, float density, int fixtureIndex)
    {
        if (_entityManager.TryGetComponent<FixturesComponent>(uid, out var fixtures))
        {
            _physics.SetDensity(uid, fixtures.Fixtures.Values.ElementAt(fixtureIndex), density);
        }
    }
}
