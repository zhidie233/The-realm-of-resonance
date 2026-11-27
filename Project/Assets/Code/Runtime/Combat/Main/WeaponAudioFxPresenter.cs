using System;
using System.Collections;
using UnityEngine;
using Resonance.Foundation;
using Resonance.Core;
using Resonance.Actors;
using Resonance.World;
using Random = UnityEngine.Random;

namespace Resonance.Combat
{
    /// <summary>
    /// Handles weapon audio playback, muzzle/shell VFX, recoil kick, camera shake, and renderers.
    /// </summary>
    public sealed class WeaponAudioFxPresenter
    {
        private AudioSource _source;
        private AudioSource _fireSource;
        private AudioClip _defaultFireSound;

        public void Initialize(AudioSource source, AudioSource fireSource)
        {
            _source = source;
            _fireSource = fireSource;
            if (_fireSource != null && _source != null)
            {
                _fireSource.volume = _source.volume;
            }
        }

        public void SyncFireVolume()
        {
            if (_fireSource != null && _source != null)
            {
                _fireSource.volume = _source.volume;
            }
        }

        public AudioClip ResolveFireClip(AudioClip currentClip, AudioClip nextClip)
        {
            if (_defaultFireSound == null) _defaultFireSound = currentClip;
            return nextClip != null ? nextClip : _defaultFireSound;
        }

        public void PlayFireAudio(AudioClip fireClip)
        {
            if (_fireSource == null) return;
            _fireSource.clip = fireClip;
            _fireSource.spread = Random.Range(1.0f, 1.5f);
            _fireSource.pitch = Random.Range(1.0f, 1.075f);
            _fireSource.Play();
        }

        public void PlayClip(AudioClip clip, float pitch = 1f)
        {
            if (_source == null) return;
            _source.clip = clip;
            _source.pitch = pitch;
            _source.Play();
        }

        public void PlayEmptyFireSound(FirearmType weaponType, AudioClip dryFireClip)
        {
            if (weaponType != FirearmType.Knife && dryFireClip != null)
            {
                PlayClip(dryFireClip);
            }
        }

        public void PlayReloadAudio(int part, bool soundReloadByAnim, AudioClip r1, AudioClip r2, AudioClip r3)
        {
            if (soundReloadByAnim) return;
            if (part == 0) PlayClip(r1);
            else if (part == 1) PlayClip(r2);
            else if (part == 2) PlayClip(r3);
        }

        public IEnumerator DelayFireSoundRoutine(AudioClip fireClip, float delay, AudioSource delaySource, AudioClip reloadSound3)
        {
            PlayFireAudio(fireClip);
            yield return new WaitForSeconds(delay);
            if (delaySource != null)
            {
                delaySource.clip = reloadSound3;
                delaySource.Play();
            }
            else
            {
                PlayClip(reloadSound3);
            }
        }

        public IEnumerator ReloadSoundRoutine(
            float reloadTime,
            FirearmType weaponType,
            int missingRounds,
            AudioClip r1,
            AudioClip r2,
            AudioClip r3,
            FirearmInventory gunManager)
        {
            float segment = reloadTime / 3f;
            if (r1 != null)
            {
                PlayClip(r1);
                gunManager?.HeadAnimation(1, segment);
            }
            if (r2 != null)
            {
                if (weaponType == FirearmType.Shotgun)
                {
                    for (int i = 0; i < missingRounds; i++)
                    {
                        yield return new WaitForSeconds(segment / missingRounds + 0.025f);
                        PlayClip(r2);
                    }
                }
                else
                {
                    yield return new WaitForSeconds(segment);
                    PlayClip(r2);
                }
            }
            if (r3 != null)
            {
                yield return new WaitForSeconds(segment);
                PlayClip(r3);
                gunManager?.HeadAnimation(2, segment);
            }
            yield return new WaitForSeconds(0.65f);
            gunManager?.HeadAnimation(0, segment);
        }

        public void PlayMuzzleFx(bool isAiming, WeaponEffects weaponFx, ParticleSystem muzzleFlash)
        {
            if (isAiming) return;
            if (weaponFx != null) weaponFx.PlayFireFX();
            else if (muzzleFlash != null) muzzleFlash.Play();
        }

        public void EjectShell(ParticleSystem shell)
        {
            if (shell != null) shell.Play();
        }

        public void ApplyKick(PlayerRig playerRefs, float recoilAmount, float recoilSpeed)
        {
            playerRefs.recoil.SetRecoil(new KickbackHandler.KickbackData
            {
                Amount = recoilAmount,
                Speed = recoilSpeed
            });
        }

        public void PublishShake(ShakePreset preset, bool isAiming)
        {
            float influence = isAiming ? 0.5f : 1f;
            EventBus.Publish(new CameraShakeEvent(preset, "fpweapon", influence));
        }

        public void SetWeaponRenderersActive(Transform root, ref Renderer[] renderers, bool active, Action<bool> callback)
        {
            if (renderers == null)
            {
                renderers = root.GetComponentsInChildren<Renderer>();
            }
            foreach (var item in renderers)
            {
                if (item == null) continue;
                item.gameObject.SetActive(active);
            }
            callback?.Invoke(active);
        }
    }
}
