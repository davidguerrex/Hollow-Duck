using System;
using System.IO;
using UnityEngine;

namespace HollowDuck
{
#pragma warning disable CS0649
    [Serializable]
    internal sealed class WeaponSheet
    {
        public WeaponDefinition[] rows;

        public static WeaponSheet Load(string path)
        {
            WeaponSheet sheet = JsonUtility.FromJson<WeaponSheet>(File.ReadAllText(path));
            if (sheet == null || sheet.rows == null || sheet.rows.Length == 0)
            {
                throw new InvalidDataException("The weapon sheet has no rows.");
            }

            return sheet;
        }
    }

    [Serializable]
    internal sealed class WeaponDefinition
    {
        public string id;
        public string guestClass;
        public string ammoClass;
        public string texturePath;
        public int[] textureSize;
        public int magazine;
        public float bulletSpeed;
        public float rangePixels;
        public float accuracy;
        public float rangeVariationPixels;
        public float impactPower;
        public float kickForce;
        public int pelletsPerFire;
        public bool manualLoad;
        public float hostCooldownSeconds;
        public int hostDamagePerHit;
        public int hostDamagePerPellet;
        public float hostProjectileRange;
        public float hostProjectileSpeed;
        public int pickupUnlockOrder;
        public string[] sourceMembers;
    }

    [Serializable]
    internal sealed class PlayerSheet
    {
        public PlayerSheetRow[] rows;

        public static PlayerSheet Load(string path)
        {
            PlayerSheet sheet = JsonUtility.FromJson<PlayerSheet>(File.ReadAllText(path));
            if (sheet == null || sheet.rows == null)
            {
                throw new InvalidDataException("The player sheet has no rows.");
            }

            return sheet;
        }
    }

    [Serializable]
    internal sealed class PlayerSheetRow
    {
        public string id;
        public string assetRelativePath;
        public int frameWidth;
        public int frameHeight;
        public int initialFrameIndex;
        public float guestRunMax;
        public float guestAccelerationPerTick;
        public float guestJumpSpeed;
        public float pixelsPerHostUnit;
        public float hostAdaptationTickRateHz;
        public float hostRunSpeed;
        public float hostJumpSpeed;
        public float hostAcceleration;
    }
#pragma warning restore CS0649
}