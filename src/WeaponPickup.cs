using UnityEngine;

namespace HollowDuck
{
    internal sealed class WeaponPickup : MonoBehaviour
    {
        private HollowDuckRuntime _runtime;
        private string _weaponId;

        public static WeaponPickup Create(
            Vector3 position,
            WeaponDefinition weapon,
            Sprite sprite,
            HollowDuckRuntime runtime)
        {
            GameObject pickupObject = new GameObject("Duck Game weapon pickup");
            pickupObject.transform.position = position;
            WeaponPickup pickup = pickupObject.AddComponent<WeaponPickup>();
            pickup._runtime = runtime;
            pickup._weaponId = weapon.id;

            SpriteRenderer renderer = pickupObject.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = 30;

            CircleCollider2D circle = pickupObject.AddComponent<CircleCollider2D>();
            circle.isTrigger = true;
            circle.radius = 0.45f;

            Rigidbody2D body = pickupObject.AddComponent<Rigidbody2D>();
            body.gravityScale = 1f;
            body.freezeRotation = true;
            body.linearVelocity = new Vector2(0f, 2f);
            return pickup;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.GetComponentInParent<HeroController>() == null)
            {
                return;
            }

            _runtime.UnlockWeapon(_weaponId);
            Destroy(gameObject);
        }
    }
}