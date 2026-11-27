using UnityEngine;
using Resonance.Foundation;
using Resonance.Core;
using Resonance.Actors;
namespace Resonance.World
{

    public class ThrowablePack : MonoBehaviour
    {
        [Header("SETTINGS")]
        public int AmountOfPacks = 3;
        /// 实例化预制体时强制应用
        public float ForceImpulse = 500;
        [Range(1, 4)] public float CallDelay = 1.4f;
        public float dropInstanceDistance = 1f;

        [Header("REFERENCES")]
        /// 用于实例化的预制体
        public GameObject DropCallerPrefab;
        public AudioClip SpawnSound;
        public PlayerClass CurrentPlayerClass { get; set; } = PlayerClass.Vanguard;

        private int remainingPacks;

        void Start()
        {
            CurrentPlayerClass = PlayerClass.Vanguard.GetSavePlayerClass();
            remainingPacks = AmountOfPacks;
        }

        protected virtual void Update()
        {
            if (ProjectConfigRoot.Instance.isChatOpen) return;

            if (GameInput.ThrowPack())
            {
                DispatchThrow();
            }
        }

        public void DispatchThrow()
        {
            if (remainingPacks <= 0 || DropCallerPrefab == null) return;

            int id = 0;
            if ((CurrentPlayerClass == PlayerClass.Vanguard || CurrentPlayerClass == PlayerClass.Pathfinder))
            {
                id = 1;
            }

            ThrowCaller(id);
        }

        void ThrowCaller(int id)
        {
            remainingPacks--;
            Vector3 point = (transform.position + (PlayerReferences.PlayerCameraTransform.forward * dropInstanceDistance)) + transform.up;
            GameObject kit = Instantiate(DropCallerPrefab, point, Quaternion.identity) as GameObject;
            kit.GetComponent<LootCaster>().SetUp(new LootCaster.LootData()
            {
                PackID = id,
                Delay = CallDelay
            });
            kit.GetComponent<Rigidbody>().AddForce(RuntimeServices.LocalPlayerReferences.PlayerCameraTransform.forward * ForceImpulse);
            if (SpawnSound)
            {
                AudioSource.PlayClipAtPoint(SpawnSound, this.transform.position, 1.0f);
            }
        }

        public void AddKits(int amount)
        {
            remainingPacks += amount;
        }

        private PlayerRig _playerReferences;
        public PlayerRig PlayerReferences
        {
            get
            {
                if (_playerReferences == null) _playerReferences = transform.GetComponent<PlayerRig>();
                return _playerReferences;
            }
        }
    }
}
