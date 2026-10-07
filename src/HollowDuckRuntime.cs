using System;
using System.Collections.Generic;
using System.IO;
using GlobalEnums;
using UnityEngine;

namespace HollowDuck
{
    internal sealed class HollowDuckRuntime : MonoBehaviour
    {
        private const string ProgressFolder = "HollowDuck";
        private const string ProgressFile = "progress.json";

        private HollowDuckMod _mod;
        private WeaponSheet _weapons;
        private PlayerSheet _player;
        private PlayerSheetRow _movement;
        private DuckGameContentStore _content;
        private ProgressionData _progress;
        private readonly Dictionary<string, int> _ammo = new Dictionary<string, int>(StringComparer.Ordinal);
        private readonly HashSet<string> _pendingDrops = new HashSet<string>(StringComparer.Ordinal);
        private HeroController _hero;
        private MeshRenderer _heroRenderer;
        private bool _wasRendererEnabled;
        private SpriteRenderer _duckRenderer;
        private SpriteRenderer _weaponRenderer;
        private string _contentError;
        private string _progressPath;
        private float _nextShotTime;
        private bool _armoryOpen;

        public void Initialize(
            HollowDuckMod mod,
            WeaponSheet weapons,
            PlayerSheet player,
            DuckGameContentStore content,
            string contentError)
        {
            _mod = mod;
            _weapons = weapons;
            _player = player;
            _movement = Array.Find(player.rows, row => row.id == "duck-controller");
            _content = content;
            _contentError = contentError;
            _progressPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                ProgressFolder,
                ProgressFile);
            _progress = LoadProgress();
            RepairSelectedWeapon();
        }

        public void OnHeroUpdate()
        {
            if (_content == null)
            {
                return;
            }

            HeroController hero = HeroController.instance;
            if (hero == null)
            {
                return;
            }

            if (_hero != hero)
            {
                AttachToHero(hero);
            }

            if (_heroRenderer != null)
            {
                _heroRenderer.enabled = false;
            }

            DuckMovementAdapter.Apply(hero, _movement);
            if (_duckRenderer != null)
            {
                _duckRenderer.flipX = !hero.cState.facingRight;
            }

            UpdateWeaponVisual();
        }

        public void OnAttack(AttackDirection direction)
        {
            FireSelectedWeapon();
        }

        public void TryDropNextWeapon(Vector3 position)
        {
            if (_content == null || _pendingDrops.Count > 0)
            {
                return;
            }

            foreach (WeaponDefinition weapon in GetWeaponsInUnlockOrder())
            {
                if (_progress.unlockedWeaponIds.Contains(weapon.id))
                {
                    continue;
                }

                Sprite sprite = _content.GetWeaponSprite(weapon.id);
                if (sprite == null)
                {
                    return;
                }

                _pendingDrops.Add(weapon.id);
                WeaponPickup.Create(position + Vector3.up * 0.35f, weapon, sprite, this);
                return;
            }
        }

        public void UnlockWeapon(string weaponId)
        {
            _pendingDrops.Remove(weaponId);
            if (!_progress.unlockedWeaponIds.Contains(weaponId))
            {
                _progress.unlockedWeaponIds.Add(weaponId);
            }

            _progress.selectedWeaponId = weaponId;
            if (!_ammo.ContainsKey(weaponId))
            {
                WeaponDefinition weapon = FindWeapon(weaponId);
                _ammo[weaponId] = weapon == null ? 0 : weapon.magazine;
            }

            SaveProgress();
            UpdateWeaponVisual();
        }

        public void Shutdown()
        {
            RestoreHero();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F2))
            {
                _armoryOpen = !_armoryOpen;
            }

            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                SelectWeaponByOrder(1);
            }

            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                SelectWeaponByOrder(2);
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                ReloadSelectedWeapon();
            }
        }

        private void OnGUI()
        {
            if (_content == null)
            {
                GUI.Box(new Rect(16f, 16f, 520f, 54f), "Duck Game content unavailable: " + _contentError);
            }

            if (!_armoryOpen)
            {
                if (_progress.unlockedWeaponIds.Count > 0)
                {
                    WeaponDefinition current = FindWeapon(_progress.selectedWeaponId);
                    int loaded = current == null || !_ammo.ContainsKey(current.id) ? 0 : _ammo[current.id];
                    GUI.Label(new Rect(16f, 76f, 300f, 28f), "Hollow Duck  |  " + (current == null ? "No weapon" : current.id + "  " + loaded + "/" + current.magazine));
                }

                return;
            }

            GUI.Box(new Rect(18f, 100f, 340f, 300f), "Hollow Duck Armory  |  F2 to close");
            float y = 140f;
            foreach (WeaponDefinition weapon in GetWeaponsInUnlockOrder())
            {
                bool unlocked = _progress.unlockedWeaponIds.Contains(weapon.id);
                string label = unlocked
                    ? (weapon.id == _progress.selectedWeaponId ? "> " : "") + weapon.id
                    : "Locked";
                if (GUI.Button(new Rect(34f, y, 290f, 34f), label) && unlocked)
                {
                    SelectWeapon(weapon.id);
                }

                y += 42f;
            }

            GUI.Label(new Rect(34f, y + 4f, 290f, 48f), "Weapons are found in Hallownest.  R reloads.  Tab changes weapon.");
        }

        private void AttachToHero(HeroController hero)
        {
            RestoreHero();
            _hero = hero;
            _heroRenderer = hero.GetComponent<MeshRenderer>();
            if (_heroRenderer != null)
            {
                _wasRendererEnabled = _heroRenderer.enabled;
                _heroRenderer.enabled = false;
            }

            GameObject duckObject = new GameObject("Duck Game duck visual");
            duckObject.transform.SetParent(hero.transform, false);
            duckObject.transform.localPosition = Vector3.zero;
            _duckRenderer = duckObject.AddComponent<SpriteRenderer>();
            _duckRenderer.sprite = _content.DuckSprite;
            _duckRenderer.sortingOrder = 20;

            GameObject weaponObject = new GameObject("Duck Game held weapon");
            weaponObject.transform.SetParent(duckObject.transform, false);
            weaponObject.transform.localPosition = new Vector3(0.2f, -0.05f, -0.01f);
            _weaponRenderer = weaponObject.AddComponent<SpriteRenderer>();
            _weaponRenderer.sortingOrder = 21;
            UpdateWeaponVisual();
        }

        private void RestoreHero()
        {
            if (_heroRenderer != null)
            {
                _heroRenderer.enabled = _wasRendererEnabled;
            }

            if (_duckRenderer != null)
            {
                Destroy(_duckRenderer.gameObject);
            }

            _hero = null;
            _heroRenderer = null;
            _duckRenderer = null;
            _weaponRenderer = null;
        }

        private void FireSelectedWeapon()
        {
            if (_hero == null || _content == null || Time.time < _nextShotTime)
            {
                return;
            }

            WeaponDefinition weapon = FindWeapon(_progress.selectedWeaponId);
            if (weapon == null || !_progress.unlockedWeaponIds.Contains(weapon.id))
            {
                return;
            }

            if (!_ammo.ContainsKey(weapon.id))
            {
                _ammo[weapon.id] = weapon.magazine;
            }

            if (_ammo[weapon.id] <= 0)
            {
                return;
            }

            _ammo[weapon.id]--;
            _nextShotTime = Time.time + weapon.hostCooldownSeconds;
            float facing = _hero.cState.facingRight ? 1f : -1f;
            Vector2 origin = _hero.transform.position + Vector3.right * (facing * 0.35f);
            float spread = (1f - weapon.accuracy) * 20f;
            for (int i = 0; i < weapon.pelletsPerFire; i++)
            {
                float offset = weapon.pelletsPerFire == 1
                    ? UnityEngine.Random.Range(-spread, spread)
                    : Mathf.Lerp(-spread, spread, i / (float)(weapon.pelletsPerFire - 1));
                float angle = offset * Mathf.Deg2Rad;
                Vector2 direction = new Vector2(facing * Mathf.Cos(angle), Mathf.Sin(angle));
                DuckProjectile.Create(origin, direction, weapon, _hero.transform);
            }

            UpdateWeaponVisual();
        }

        private void ReloadSelectedWeapon()
        {
            WeaponDefinition weapon = FindWeapon(_progress.selectedWeaponId);
            if (weapon != null && _progress.unlockedWeaponIds.Contains(weapon.id))
            {
                _ammo[weapon.id] = weapon.magazine;
            }
        }

        private void SelectWeaponByOrder(int order)
        {
            foreach (WeaponDefinition weapon in GetWeaponsInUnlockOrder())
            {
                if (weapon.pickupUnlockOrder == order && _progress.unlockedWeaponIds.Contains(weapon.id))
                {
                    SelectWeapon(weapon.id);
                    return;
                }
            }
        }

        private void SelectWeapon(string weaponId)
        {
            if (!_progress.unlockedWeaponIds.Contains(weaponId))
            {
                return;
            }

            _progress.selectedWeaponId = weaponId;
            SaveProgress();
            UpdateWeaponVisual();
        }

        private void UpdateWeaponVisual()
        {
            if (_weaponRenderer == null)
            {
                return;
            }

            _weaponRenderer.sprite = _content == null ? null : _content.GetWeaponSprite(_progress.selectedWeaponId);
            _weaponRenderer.enabled = _weaponRenderer.sprite != null;
            if (_hero != null)
            {
                _weaponRenderer.flipX = !_hero.cState.facingRight;
            }
        }

        private IEnumerable<WeaponDefinition> GetWeaponsInUnlockOrder()
        {
            Array.Sort(_weapons.rows, (first, second) => first.pickupUnlockOrder.CompareTo(second.pickupUnlockOrder));
            return _weapons.rows;
        }

        private WeaponDefinition FindWeapon(string weaponId)
        {
            return Array.Find(_weapons.rows, weapon => weapon.id == weaponId);
        }

        private ProgressionData LoadProgress()
        {
            try
            {
                if (File.Exists(_progressPath))
                {
                    ProgressionData saved = JsonUtility.FromJson<ProgressionData>(File.ReadAllText(_progressPath));
                    if (saved != null)
                    {
                        saved.unlockedWeaponIds = saved.unlockedWeaponIds ?? new List<string>();
                        return saved;
                    }
                }
            }
            catch (Exception exception)
            {
                _mod.Log("Could not read Hollow Duck progression: " + exception.Message);
            }

            return new ProgressionData();
        }

        private void SaveProgress()
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(_progressPath));
                File.WriteAllText(_progressPath, JsonUtility.ToJson(_progress, true));
            }
            catch (Exception exception)
            {
                _mod.Log("Could not save Hollow Duck progression: " + exception.Message);
            }
        }

        private void RepairSelectedWeapon()
        {
            if (!_progress.unlockedWeaponIds.Contains(_progress.selectedWeaponId))
            {
                _progress.selectedWeaponId = _progress.unlockedWeaponIds.Count > 0
                    ? _progress.unlockedWeaponIds[0]
                    : string.Empty;
            }
        }

        [Serializable]
        private sealed class ProgressionData
        {
            public List<string> unlockedWeaponIds = new List<string>();
            public string selectedWeaponId = string.Empty;
        }
    }
}