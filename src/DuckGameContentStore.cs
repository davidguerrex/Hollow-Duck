using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace HollowDuck
{
    internal sealed class DuckGameContentStore
    {
        private readonly Dictionary<string, Sprite> _weaponSprites;

        private DuckGameContentStore(Sprite duckSprite, Dictionary<string, Sprite> weaponSprites)
        {
            DuckSprite = duckSprite;
            _weaponSprites = weaponSprites;
        }

        public Sprite DuckSprite { get; private set; }

        public Sprite GetWeaponSprite(string weaponId)
        {
            Sprite sprite;
            return _weaponSprites.TryGetValue(weaponId, out sprite) ? sprite : null;
        }

        public static DuckGameContentStore Load(string modDirectory, WeaponSheet weapons)
        {
            string contentDirectory = DuckGameInstallLocator.FindContentFolder();
            if (contentDirectory == null)
            {
                throw new DirectoryNotFoundException("Duck Game was not found in a Steam library.");
            }

            PlayerSheet playerSheet = PlayerSheet.Load(Path.Combine(modDirectory, "player.json"));
            PlayerSheetRow duck = Array.Find(playerSheet.rows, row => row.id == "duck-visual");
            if (duck == null || string.IsNullOrWhiteSpace(duck.assetRelativePath))
            {
                throw new InvalidDataException("The duck visual row is missing from player.json.");
            }

            Sprite duckSprite = XnbTextureReader.LoadSprite(
                Path.Combine(Path.GetDirectoryName(contentDirectory), duck.assetRelativePath.Replace('/', Path.DirectorySeparatorChar)),
                duck.frameWidth,
                duck.frameHeight,
                duck.initialFrameIndex);

            Dictionary<string, Sprite> weaponSprites = new Dictionary<string, Sprite>(StringComparer.Ordinal);
            foreach (WeaponDefinition weapon in weapons.rows)
            {
                if (weapon.textureSize == null || weapon.textureSize.Length != 2)
                {
                    throw new InvalidDataException("Weapon texture dimensions are missing for " + weapon.id + ".");
                }

                string texturePath = Path.Combine(contentDirectory, weapon.texturePath.Replace('/', Path.DirectorySeparatorChar));
                weaponSprites.Add(
                    weapon.id,
                    XnbTextureReader.LoadSprite(texturePath, weapon.textureSize[0], weapon.textureSize[1], 0));
            }

            return new DuckGameContentStore(duckSprite, weaponSprites);
        }
    }
}