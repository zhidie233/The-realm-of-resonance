using System;

namespace Resonance.Domain.Combat
{
    public enum AmmoStorageMode
    {
        Clips = 0,
        Bullets = 1
    }

    public readonly struct AmmoPickupResult
    {
        public bool Consumed { get; }
        public int AddedAmount { get; }
        public bool IsGrenade { get; }
        public bool ShouldNotify { get; }

        public AmmoPickupResult(bool consumed, int addedAmount, bool isGrenade, bool shouldNotify)
        {
            Consumed = consumed;
            AddedAmount = addedAmount;
            IsGrenade = isGrenade;
            ShouldNotify = shouldNotify;
        }
    }

    /// <summary>
    /// Pure C# ammunition inventory model governing magazine, reserve, reload, and pickup rules.
    /// </summary>
    public sealed class AmmoInventoryModel
    {
        public AmmoStorageMode StorageMode { get; private set; }
        public int CurrentMagazine { get; set; }
        public int ReserveCount { get; set; }
        public bool HasInfiniteAmmo { get; private set; }

        public void Configure(AmmoStorageMode mode, int magazineCapacity, int initialClips, bool isInitialSetup)
        {
            StorageMode = mode;
            CurrentMagazine = magazineCapacity;
            if (mode == AmmoStorageMode.Clips)
            {
                ReserveCount = initialClips;
            }
            else if (!isInitialSetup)
            {
                ReserveCount = magazineCapacity * initialClips;
            }
        }

        public void SetInfiniteAmmo(bool infinite, int magazineCapacity)
        {
            HasInfiniteAmmo = infinite;
            CurrentMagazine = magazineCapacity;
            ReserveCount = StorageMode == AmmoStorageMode.Bullets ? magazineCapacity * ReserveCount : ReserveCount;
        }

        public bool TryConsumeRound(bool consumeAmmo = true)
        {
            if (consumeAmmo)
            {
                CurrentMagazine--;
            }
            return true;
        }

        public void FillMagazine(int magazineCapacity)
        {
            if (StorageMode == AmmoStorageMode.Clips)
            {
                CurrentMagazine = magazineCapacity;
                return;
            }

            int need = magazineCapacity - CurrentMagazine;
            int add = ReserveCount >= need ? need : ReserveCount;
            CurrentMagazine += add;
            if (!HasInfiniteAmmo)
            {
                ReserveCount -= add;
            }
        }

        public void ConsumeClipOnReloadStart()
        {
            if (StorageMode == AmmoStorageMode.Clips && !HasInfiniteAmmo)
            {
                ReserveCount--;
            }
        }

        public void AddSingleRound(int count)
        {
            if (StorageMode == AmmoStorageMode.Bullets)
            {
                ReserveCount -= count;
            }
            CurrentMagazine += count;
        }

        public int CalculateSplitReloadRounds(int magazineCapacity)
        {
            int missing = magazineCapacity - CurrentMagazine;
            return ReserveCount >= missing ? missing : ReserveCount;
        }

        public AmmoPickupResult TryPickUpAmmo(
            int bullets,
            int projectiles,
            int targetWeaponId,
            int weaponId,
            bool isKnife,
            bool isGrenade,
            int magazineCapacity,
            int maxClips)
        {
            if ((targetWeaponId != -1 && targetWeaponId != weaponId) || isKnife)
            {
                return default;
            }

            if (StorageMode == AmmoStorageMode.Clips)
            {
                if (ReserveCount >= maxClips || magazineCapacity <= 0)
                {
                    return default;
                }

                int clipsToAdd = bullets > 0 ? bullets / magazineCapacity : 0;
                ReserveCount = Math.Min(ReserveCount + clipsToAdd, maxClips);
                return new AmmoPickupResult(true, clipsToAdd, false, false);
            }

            if (isGrenade)
            {
                ReserveCount += projectiles;
                return new AmmoPickupResult(true, projectiles, true, true);
            }

            int maxBullets = magazineCapacity * maxClips;
            if (ReserveCount >= maxBullets)
            {
                return default;
            }

            int oldCount = ReserveCount;
            int nextCount = ReserveCount + bullets;
            if (nextCount < 0) nextCount = 0;
            if (nextCount > maxBullets) nextCount = maxBullets;
            ReserveCount = nextCount;
            return new AmmoPickupResult(true, ReserveCount - oldCount, false, true);
        }

        public bool CanReload(int magazineCapacity, bool isRunning, bool isReloading, bool isKnife, bool fireRatePassed)
        {
            bool can = CurrentMagazine < magazineCapacity && ReserveCount > 0 && !isRunning && !isReloading;
            if (isKnife && fireRatePassed)
            {
                can = false;
            }
            return can;
        }

        public bool CanBeTaken(bool allowWhenEmpty)
        {
            return allowWhenEmpty || CurrentMagazine > 0 || ReserveCount > 0;
        }

        public bool IsOutOfAmmo() => CurrentMagazine <= 0 && ReserveCount <= 0;

        public int GetCompactClips(int magazineCapacity)
        {
            return magazineCapacity <= 0 ? 0 : ReserveCount / magazineCapacity;
        }
    }
}
