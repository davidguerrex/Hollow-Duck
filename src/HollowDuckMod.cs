using Modding;
using Modding.Delegates;
using System;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace HollowDuck
{
    public sealed class HollowDuckMod : Mod, ITogglableMod
    {
        private HollowDuckRuntime _runtime;

        public override string GetVersion()
        {
            return "0.1.0";
        }

        public override void Initialize()
        {
            if (_runtime != null)
            {
                return;
            }

            string modDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            WeaponSheet weapons = WeaponSheet.Load(Path.Combine(modDirectory, "weapons.json"));
            PlayerSheet player = PlayerSheet.Load(Path.Combine(modDirectory, "player.json"));
            DuckGameContentStore content = null;
            string contentError = null;
            try
            {
                content = DuckGameContentStore.Load(modDirectory, weapons);
            }
            catch (Exception exception)
            {
                contentError = exception.Message;
            }

            GameObject runtimeObject = new GameObject("Hollow Duck runtime");
            UnityEngine.Object.DontDestroyOnLoad(runtimeObject);
            _runtime = runtimeObject.AddComponent<HollowDuckRuntime>();
            _runtime.Initialize(this, weapons, player, content, contentError);
            ModHooks.HeroUpdateHook += _runtime.OnHeroUpdate;
            ModHooks.AttackHook += _runtime.OnAttack;
            ModHooks.OnReceiveDeathEventHook += OnReceiveDeathEvent;
            Log(content == null
                ? "Loaded without Duck Game content: " + contentError
                : "Loaded Duck Game content from the local Steam install.");
        }

        public void Unload()
        {
            if (_runtime == null)
            {
                return;
            }

            ModHooks.HeroUpdateHook -= _runtime.OnHeroUpdate;
            ModHooks.AttackHook -= _runtime.OnAttack;
            ModHooks.OnReceiveDeathEventHook -= OnReceiveDeathEvent;
            if (_runtime != null)
            {
                _runtime.Shutdown();
                UnityEngine.Object.Destroy(_runtime.gameObject);
                _runtime = null;
            }
        }

        private void OnReceiveDeathEvent(
            EnemyDeathEffects enemyDeathEffects,
            bool eventAlreadyReceived,
            ref float? attackDirection,
            ref bool resetDeathEvent,
            ref bool spellBurn,
            ref bool isWatery)
        {
            if (!eventAlreadyReceived && enemyDeathEffects != null && _runtime != null)
            {
                _runtime.TryDropNextWeapon(enemyDeathEffects.transform.position);
            }
        }
    }
}