using UnityEngine;

namespace HollowDuck
{
    internal static class DuckMovementAdapter
    {
        public static void Apply(HeroController hero, PlayerSheetRow movement)
        {
            GameManager gameManager = GameManager.instance;
            if (gameManager == null)
            {
                return;
            }

            InputHandler inputHandler = gameManager.GetComponent<InputHandler>();
            Rigidbody2D body = hero.GetComponent<Rigidbody2D>();
            if (inputHandler == null || body == null)
            {
                return;
            }

            bool left = inputHandler.inputActions.left.IsPressed;
            bool right = inputHandler.inputActions.right.IsPressed;
            float direction = left == right ? 0f : left ? -1f : 1f;

            if (direction != 0f)
            {
                float targetVelocity = direction * movement.hostRunSpeed;
                float velocity = Mathf.MoveTowards(
                    body.linearVelocity.x,
                    targetVelocity,
                    movement.hostAcceleration * Time.deltaTime);
                float translatedSpeed = velocity / direction;
                hero.RUN_SPEED = translatedSpeed;
                hero.RUN_SPEED_CH = translatedSpeed;
                hero.RUN_SPEED_CH_COMBO = translatedSpeed;
                hero.WALK_SPEED = translatedSpeed;
                hero.UNDERWATER_SPEED = translatedSpeed;
            }

            hero.JUMP_SPEED = movement.hostJumpSpeed;
            hero.JUMP_SPEED_UNDERWATER = movement.hostJumpSpeed;
        }
    }
}