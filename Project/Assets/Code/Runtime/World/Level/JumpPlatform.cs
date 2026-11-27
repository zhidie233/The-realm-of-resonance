using UnityEngine;
using Resonance.Core;
using Resonance.Actors;
namespace Resonance.World
{

    public class JumpPlatform : MonoBehaviour
    {
        [Range(0, 25)] public float JumpForce;
        [SerializeField] private AudioClip JumpSound;

        private void OnTriggerEnter(Collider other)
        {
            if (other.isLocalPlayerCollider())
            {
                var fpc = other.GetComponent<PlayerLocomotionController>();
                fpc.PlatformJump(JumpForce);
                if (JumpSound != null) { AudioSource.PlayClipAtPoint(JumpSound, transform.position); }
            }
        }
    }
}
