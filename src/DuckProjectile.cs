using System;
using GlobalEnums;
using UnityEngine;

namespace HollowDuck
{
    internal sealed class DuckProjectile : MonoBehaviour
    {
        private static Sprite _tracerSprite;
        private Transform _owner;
        private Vector2 _direction;
        private float _speed;
        private float _range;
        private float _travelled;
        private int _damage;

        public static DuckProjectile Create(
            Vector2 position,
            Vector2 direction,
            WeaponDefinition weapon,
            Transform owner)
        {
            GameObject projectileObject = new GameObject("Hollow Duck projectile");
            projectileObject.transform.position = position;
            DuckProjectile projectile = projectileObject.AddComponent<DuckProjectile>();
            projectile.Initialize(direction, weapon, owner);
            return projectile;
        }

        private void Initialize(Vector2 direction, WeaponDefinition weapon, Transform owner)
        {
            _owner = owner;
            _direction = direction.normalized;
            _speed = weapon.hostProjectileSpeed;
            _range = weapon.hostProjectileRange + UnityEngine.Random.Range(
                -weapon.rangeVariationPixels / 32f,
                weapon.rangeVariationPixels / 32f);
            _damage = weapon.hostDamagePerPellet > 0
                ? weapon.hostDamagePerPellet
                : weapon.hostDamagePerHit;

            SpriteRenderer renderer = gameObject.AddComponent<SpriteRenderer>();
            renderer.sprite = GetTracerSprite();
            renderer.color = new Color(1f, 0.82f, 0.32f, 1f);
            renderer.sortingOrder = 40;
            transform.localScale = new Vector3(0.13f, 0.06f, 1f);
        }

        private void Update()
        {
            float distance = _speed * Time.deltaTime;
            RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, _direction, distance);
            Array.Sort(hits, (first, second) => first.distance.CompareTo(second.distance));

            foreach (RaycastHit2D hit in hits)
            {
                if (hit.collider == null || IsOwnedCollider(hit.collider.transform))
                {
                    continue;
                }

                HealthManager health = hit.collider.GetComponentInParent<HealthManager>();
                if (health != null)
                {
                    health.Hit(new HitInstance
                    {
                        AttackType = AttackTypes.Nail,
                        DamageDealt = _damage
                    });
                    Destroy(gameObject);
                    return;
                }

                if (!hit.collider.isTrigger)
                {
                    Destroy(gameObject);
                    return;
                }
            }

            Vector2 step = _direction * distance;
            transform.position += new Vector3(step.x, step.y, 0f);
            _travelled += distance;
            if (_travelled >= _range)
            {
                Destroy(gameObject);
            }
        }

        private bool IsOwnedCollider(Transform candidate)
        {
            return _owner != null && (candidate == _owner || candidate.IsChildOf(_owner));
        }

        private static Sprite GetTracerSprite()
        {
            if (_tracerSprite != null)
            {
                return _tracerSprite;
            }

            Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            texture.SetPixels(new[] { Color.white, Color.white, Color.white, Color.white });
            texture.filterMode = FilterMode.Point;
            texture.Apply(false, true);
            _tracerSprite = Sprite.Create(texture, new Rect(0f, 0f, 2f, 2f), new Vector2(0.5f, 0.5f), 16f);
            return _tracerSprite;
        }
    }
}