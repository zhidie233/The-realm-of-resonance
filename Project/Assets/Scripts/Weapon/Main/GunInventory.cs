using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GunInventory : MonoBehaviourBase
{
    [Header("武器列表")]
    public List<GunController> AllGuns = new List<GunController>();
    [HideInInspector] public List<GunController> PlayerEquip = new List<GunController>();

    [Header("玩家职业")]
    public PlayerClassLoadout m_AssaultClass;
    public PlayerClassLoadout m_EngineerClass;
    public PlayerClassLoadout m_ReconClass;
    public PlayerClassLoadout m_SupportClass;

    [Header("设置")]
    // 初始武器 ID
    public int currentWeaponIndex = 0;
    // 切换武器所需时间
    public float SwichTime = 1;
    public float PickUpTime = 2.5f;
    public ChangeWeaponStyle changeWeaponStyle = ChangeWeaponStyle.HideAndDraw;

    [Header("引用")]
    public Animator HeadAnimator;
    public AudioClip SwitchFireAudioClip;

    // 当前装备的武器
    public GunController CurrentGun { get; set; }

    // 玩家是否可以切换武器
    public bool CanSwich { get; set; } = true;

    public bool isGameStarted { get; set; }

    // 从 GameData 缓存的选项
    public AutoChangeOnPickup EquipPickUpMode { get; set; } = AutoChangeOnPickup.Always;

    private int PreviousGun = 0;
    private bool isFastFire = false;
    public bool ObservedComponentsFoldoutOpen = false;
    private AudioSource ASource, fireAudioSource;
#if GR
    public bool isGunRace { get; set; }
    private GunRaceMode GunRace;
#endif

    protected override void Awake()
    {
        base.Awake();
        ASource = GetComponent<AudioSource>();
        isGameStarted = MatchTimeManagerBase.HaveTimeStarted();
#if GR
        if (playerReferences.photonView.IsMine)
        {
            GunRace = FindObjectOfType<GunRaceMode>();
            if (GunRace != null) { GunRace.SetGunManager(this); }
            else { Debug.Log("Gun Race is not integrated in this map, just go to GFWK -> Addons -> Gun Race -> Integrate, with the map scene open)."); }
        }
#endif
        //when player instance select player class select in RoomMenu
        SetupLoadout();
    }

    private void Start()
    {
        //Disable all weapons in children and take the first
        foreach (GunController g in PlayerEquip) { g?.Setup(true); }
        foreach (GunController guns in AllGuns) { guns.gameObject?.SetActive(false); }
        WeaponLoadoutUIBase.Instance?.SetLoadout(PlayerEquip);
        EquipPickUpMode = GameConfig.Instance.switchToPickupWeapon;
#if GR
        if (isGunRace)
        {
            PlayerEquip[0] = GunRace.GetGunInfo(AllGuns);
            currentWeaponIndex = 0;
        }
#endif
        var firstWeapon = PlayerEquip[currentWeaponIndex];
        TakeWeapon(firstWeapon);

        if (firstWeapon != null)
            GameEvents.ChangeWeaponEvent(firstWeapon.GunID);

        if (GameController.Instance.GameMatchState == MatchState.Waiting && !GameController.Instance.FirstSpawnDone)
        {
            BlockAllWeapons();
        }
#if LMS
        if (GetGameMode == GameMode.BR)
        {
            UnEquipEverything();
        }
#endif
    }

    // Setup the player equipped weapons from the selected player class
    void SetupLoadout()
    {
#if CLASS_CUSTOMIZER
        ClassManager.Instance.SetUpClasses(this);
#else
        //when player instance select player class select in RoomMenu
        PlayerClassLoadout pcl = null;
        var currentClass = PlayerClass.Assault.GetSavePlayerClass();
        switch (currentClass)
        {
            case PlayerClass.Assault:
                pcl = m_AssaultClass;
                break;
            case PlayerClass.Recon:
                pcl = m_ReconClass;
                break;
            case PlayerClass.Engineer:
                pcl = m_EngineerClass;
                break;
            case PlayerClass.Support:
                pcl = m_SupportClass;
                break;
        }

        if (pcl == null)
        {
            Debug.LogError($"Player Class Loadout has not been assigned for the class {currentClass.ToString()}");
            return;
        }

        // Assign the player class loadout weapons
        AddWeaponToSlot(0, GetGunOnListById(pcl.Primary), true);
        AddWeaponToSlot(1, GetGunOnListById(pcl.Secondary), true);
        AddWeaponToSlot(2, GetGunOnListById(pcl.Letal), true);
        AddWeaponToSlot(3, GetGunOnListById(pcl.Perks), true);

        // if you want to add more weapons you can do it here
        // e.g AddWeaponToSlot(4, GetGunOnListById(pcl.Custom), true);

#endif
        for (int i = 0; i < PlayerEquip.Count; i++)
        {
            if (PlayerEquip[i] == null) continue;
            PlayerEquip[i].Initialized();
        }
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        GameEvents.onPickUpGun += this.PickUpGun;
        GameEvents.onMatchStart += OnMatchStart;
        GameEvents.onGameSettingsChange += OnGameSettingsChanged;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        GameEvents.onPickUpGun -= this.PickUpGun;
        GameEvents.onMatchStart -= OnMatchStart;
        GameEvents.onGameSettingsChange -= OnGameSettingsChanged;
    }

    public override void OnUpdate()
    {
        if (!RoomMenu.Instance.isCursorLocked)
            return;

        InputControl();
        CurrentGun = PlayerEquip[currentWeaponIndex];
    }

    void InputControl()
    {
        if (!CanSwich || GameConfig.Instance.isChating)
            return;
#if GR
        if (isGunRace) return;
#endif

        if (GameInput.WeaponSlot(1))
        {
            ChangeCurrentWeaponTo(0);
        }
        else if (GameInput.WeaponSlot(2))
        {
            ChangeCurrentWeaponTo(1);
        }
        else if (GameInput.WeaponSlot(3))
        {
            ChangeCurrentWeaponTo(2);
        }
        else if (GameInput.WeaponSlot(4))
        {
            ChangeCurrentWeaponTo(3);
        }

        //fast fire knife
        if (GameInput.QuickMelee() && PlayerEquip[3] != null && PlayerEquip[3].m_AllowQuickFire && currentWeaponIndex != 3 && !isFastFire)
        {
            DoFastKnifeShot();
        }

        //fast throw grenade
        if (GameInput.QuickNade() && PlayerEquip[2] != null && PlayerEquip[2].AllowQuickFire() && currentWeaponIndex != 2 && !isFastFire)
        {
            DoSingleGrenadeThrow();
        }

        if (PlayerEquip.Count <= 0 || PlayerEquip == null)
            return;

        //change gun with Scroll mouse
        if (InputHandler.isButtonDown("NextWeapon"))
        {
            SwitchNext();
        }
        if (InputHandler.isButtonDown("PreviousWeapon"))
        {
            SwitchPrevious();
        }
    }

    public int SwitchNext()
    {
        if (PlayerEquip.Count <= 0 || PlayerEquip == null)
            return 0;
#if GR
        if (isGunRace) return 0;
#endif

        int next = (this.currentWeaponIndex + 1) % this.PlayerEquip.Count;
        if (PlayerEquip[next] == null) return 0;

        ChangeCurrentWeaponTo(next);
        return currentWeaponIndex;
    }

    public int SwitchPrevious()
    {
        if (PlayerEquip.Count <= 0 || PlayerEquip == null)
            return 0;
#if GR
        if (isGunRace) return 0;
#endif

        int next = 0;
        if (this.currentWeaponIndex != 0)
        {
            next = (this.currentWeaponIndex - 1) % this.PlayerEquip.Count;
        }
        else
        {
            next = PlayerEquip.Count - 1;
        }
        if (PlayerEquip[next] == null) return 0;
        ChangeCurrentWeaponTo(next);
        return currentWeaponIndex;
    }

    public void DoFastKnifeShot()
    {
        var equippedKnife = PlayerEquip[3];
        if (equippedKnife == null || equippedKnife.Info.Type != GunType.Knife) return;

        PreviousGun = currentWeaponIndex;
        isFastFire = true;
        currentWeaponIndex = 3; // 3 = knife position in list
        PlayerEquip[PreviousGun].gameObject.SetActive(false);
        equippedKnife.gameObject.SetActive(true);
        equippedKnife.QuickMelee(OnReturnWeapon);
        CanSwich = false;
    }

    public void DoSingleGrenadeThrow()
    {
        var equippedGrenade = PlayerEquip[2];// 2 = GRENADE position in list
        if (equippedGrenade == null || equippedGrenade.Info.Type != GunType.Grenade) return;

        PreviousGun = currentWeaponIndex;
        isFastFire = true;
        currentWeaponIndex = 2;
        PlayerEquip[PreviousGun].gameObject.SetActive(false);
        equippedGrenade.gameObject.SetActive(true);
        StartCoroutine(equippedGrenade.FastGrenadeFire(OnReturnWeapon));
        CanSwich = false;
    }

    public void OnReturnWeapon()
    {
        PlayerEquip[currentWeaponIndex].gameObject.SetActive(false);
        currentWeaponIndex = PreviousGun;
        PlayerEquip[currentWeaponIndex].gameObject.SetActive(true);
        CurrentGun = PlayerEquip[currentWeaponIndex];
        CanSwich = true;
        isFastFire = false;
        GameEvents.ChangeWeaponEvent(PlayerEquip[currentWeaponIndex].GunID);
    }

    void TakeWeapon(GunController gun, bool forced = false)
    {
        if (!CanSwich && !forced) return;
        if (gun == null) return;
        gun.gameObject.SetActive(true);
        CanSwich = true;
    }

    // Call to set none weapon in the local player and can't select any weapon
    public void BlockAllWeapons()
    {
        foreach (GunController g in PlayerEquip) { g.gameObject.SetActive(false); }
        CrosshairBase.Instance.Show(false);
        CrosshairBase.Instance.Block = true;
        CanSwich = false;
        playerReferences.playerNetwork.SetWeaponBlocked(1);
    }

    // Make local player can switch weapons again
    public void ReleaseWeapons(bool takeFirst)
    {
        CanSwich = true;
        CrosshairBase.Instance.Block = false;
        CrosshairBase.Instance.Show(true);

        if (!HasAWeaponActive())
        {
            if (takeFirst) TakeWeapon(PlayerEquip[0]);
            else TakeWeapon(PlayerEquip[currentWeaponIndex]);
        }

        playerReferences.playerNetwork.SetWeaponBlocked(0);
    }

    // Unequipped all the player weapons leaving him with anything to attack.
    public void UnEquipEverything()
    {
        foreach (GunController g in PlayerEquip) { g.gameObject.SetActive(false); }
        for (int i = 0; i < PlayerEquip.Count; i++)
        {
            PlayerEquip[i] = null;
        }
        CrosshairBase.Instance.Show(false);
        CrosshairBase.Instance.Block = true;
        GameEvents.ChangeWeaponEvent(-1);
        UIReferences.Instance.PlayerUI.WeaponStatsUI.SetActive(false);
    }

    public void SetInfinityAmmoToAllEquippeds(bool infinity)
    {
        AllGuns.ForEach((x) =>
        {
            if (x != null) x.SetInifinityAmmo(infinity);
        });
    }

    // Change the current weapon (if any) to the given one
    public void ChangeTo(int AllWeaponsIndex)
    {
        StartCoroutine(ChangeGun(currentWeaponIndex, AllGuns[AllWeaponsIndex].gameObject, currentWeaponIndex));
        PlayerEquip[currentWeaponIndex] = AllGuns[AllWeaponsIndex];
    }

    // Change to a weapon without delay/animation
    public void ChangeToInstant(int AllWeaponsIndex)
    {
        PlayerEquip[currentWeaponIndex].gameObject.SetActive(false);
        AllGuns[AllWeaponsIndex].gameObject.SetActive(true);
        PlayerEquip[currentWeaponIndex] = AllGuns[AllWeaponsIndex];
        GameEvents.ChangeWeaponEvent(PlayerEquip[currentWeaponIndex].GunID);
    }

    // Change the current equipped weapon to the one the one in the provided weapon slot
    // index of the weapon in the <see cref="PlayerEquip"/> list
    public void ChangeCurrentWeaponTo(int nextSlotID)
    {
        if (currentWeaponIndex == nextSlotID || PlayerEquip[nextSlotID] == null) return;
        if (!CanSwich || GameConfig.Instance.isChating)
            return;

        var next = PlayerEquip[nextSlotID];
        if (next == null || !next.CanBeTaken()) return;//if the weapons can't be taken when is empty (of ammo)

        PreviousGun = currentWeaponIndex;
        StartCoroutine(ChangeGun(currentWeaponIndex, next.gameObject, nextSlotID));
        currentWeaponIndex = nextSlotID;
    }

    // Coroutine to Change of Gun
    // <returns></returns>
    public IEnumerator ChangeGun(int IDfrom, GameObject t_next, int newID)
    {
        CanSwich = false;
        WeaponLoadoutUIBase.Instance?.SetEquippedSlot(newID);
        float hideTime = PlayerEquip[IDfrom].DisableWeapon();
        //instantly disable the current weapon and active the next weapon
        if (changeWeaponStyle == ChangeWeaponStyle.CounterStrike)
        {
            PlayerEquip[IDfrom].gameObject.SetActive(false);
        }
        else if (changeWeaponStyle == ChangeWeaponStyle.HideAndDraw)
        {
            HeadAnimator?.Play("SwichtGun", 0, 0);
            //wait a fixed delay before active the next weapon
            yield return new WaitForSeconds(SwichTime);
        }
        else if (changeWeaponStyle == ChangeWeaponStyle.HideCompletelyAndThenDraw)
        {
            HeadAnimator?.Play("SwichtGun", 0, 0);
            //wait until the current weapon hide animation complete before active the next weapon
            yield return new WaitForSeconds(hideTime);
        }
        foreach (GunController guns in AllGuns)
        {
            if (guns.gameObject.activeSelf == true)
            {
                guns.gameObject.SetActive(false);
            }
        }
        TakeWeapon(PlayerEquip[newID], true);
        GameEvents.ChangeWeaponEvent(PlayerEquip[newID].GunID);
    }

    // This function is called when the local player pick up a weapon in the map
    public void PickUpGun(GunPickUpData e)
    {
        if (GunPickUpManagerBase.Instance == null)
        {
            Debug.LogWarning("Need a 'Pick Up Manager' in the scene!");
            return;
        }
        //find the pick up weapon in the FP weapon list of this player prefab
        GunController pickedLocalGun = AllGuns.Find(x => x.GunID == e.ID);
        if (pickedLocalGun == null)
        {
            Debug.LogWarning($"The weapon {e.ID} is not integrated in this player prefab.");
            return;
        }
        //If not already equipped
        if (!PlayerEquip.Exists(x => x != null && x.GunID == e.ID))
        {
            GunData gunInfo = GameConfig.Instance.GetWeapon(e.ID);
            //first we need to make sure that there's not an empty slot in the loadout
            for (int i = 0; i < PlayerEquip.Count; i++)
            {
                //if there's an empty slot
                if (PlayerEquip[i] == null)
                {
                    //check if the this weapon can by equipped in this slot
                    if (GameConfig.Instance.weaponSlotRuler.CanBeOnSlot(gunInfo.Type, i))
                    {
                        SetUpPickUpWeapon(e, pickedLocalGun, i);
                        if (EquipPickUpMode.IsEnumFlagPresent(AutoChangeOnPickup.OnlyOnEmptySlots | AutoChangeOnPickup.Always))
                        {
                            StartCoroutine(SwitchToPickUpGun(null, pickedLocalGun, null, i));
                            currentWeaponIndex = i;
                        }
                        return;
                    }
                }
            }

            //if get there means that there's not empty slots so we have to replace the current weapon
            //so first let's check if the pick up weapon can replace the weapon in the current slot.

            bool isEmpty = PlayerEquip[currentWeaponIndex] == null;
            int actualID = isEmpty ? -1 : PlayerEquip[currentWeaponIndex].GunID;

            //Get the required data from the weapon that we are going to replace
            int[] info = new int[2];
            int clips = isEmpty ? 3 : (int)PlayerEquip[currentWeaponIndex].RemainingClips;
            info[0] = clips;
            info[1] = isEmpty ? 30 : PlayerEquip[currentWeaponIndex].bulletsLeft;

            GunController oldGun;
            //if the pick up weapon can't replace the current slot
            if (!GameConfig.Instance.weaponSlotRuler.CanBeOnSlot(gunInfo.Type, currentWeaponIndex))
            {
                //let's find out the a compatible slot for it and replace the weapon there
                for (int i = 0; i < 4; i++)
                {
                    //when find the slot for this pick up weapon
                    if (GameConfig.Instance.weaponSlotRuler.CanBeOnSlot(gunInfo.Type, i))
                    {
                        oldGun = PlayerEquip[i];
                        //change the weapon on that slot with the pick up one
                        SetUpPickUpWeapon(e, pickedLocalGun, i);
                        if (EquipPickUpMode == (AutoChangeOnPickup.OnlyOnReplacements | AutoChangeOnPickup.Always))
                        {
                            StartCoroutine(SwitchToPickUpGun(oldGun, pickedLocalGun, info, i));
                            currentWeaponIndex = i;
                        }
                        else
                        {
                            GunPickUpManagerBase.Instance.ThrowGun(new GunPickUpManagerBase.ThrowData()
                            {
                                GunID = oldGun.GunID,
                                Origin = GetThrowPosition(),
                                Direction = GetThrowDirection(),
                                Data = info,
                                AutoDestroy = false
                            });
                        }
                        return;
                    }
                }
            }

            oldGun = PlayerEquip[currentWeaponIndex];
            //Replace the current equipped weapon
            SetUpPickUpWeapon(e, pickedLocalGun, currentWeaponIndex);
            StartCoroutine(SwitchToPickUpGun(oldGun, pickedLocalGun, info, currentWeaponIndex));
        }
        else//if the weapon is already equipped
        {
            // pick up it as ammo only
            foreach (GunController g in PlayerEquip)
            {
                if (g != null && g.GunID == e.ID)
                {
                    g.OnPickUpAmmo(e.Bullets * e.Clips, 1, e.ID);
                    break;
                }
            }
        }
    }

    void SetUpPickUpWeapon(GunPickUpData data, GunController gun, int slotID)
    {
        PlayerEquip[slotID] = gun;
        gun.Setup(true);

        if (gun.Info.Type == GunType.Grenade)
        {
            if (gun.bulletsLeft <= 0) gun.bulletsLeft = 1;
            else
                gun.RemainingClips++;
        }
        else
        {
            gun.bulletsLeft = data.Bullets;
            gun.RemainingClips = data.Clips;
        }
        WeaponLoadoutUIBase.Instance?.ReplaceSlot(slotID, gun);
    }

    public IEnumerator SwitchToPickUpGun(GunController currentGun, GunController nextGun, int[] info, int newSlotID)
    {
        HeadAnimator?.Play("TakeGun", 0, 0);
        currentGun?.DisableWeapon();

        if (currentWeaponIndex != newSlotID) WeaponLoadoutUIBase.Instance?.SetEquippedSlot(newSlotID);

        yield return new WaitForSeconds(PickUpTime);

        AllGuns.ForEach(x => { if (x != null) { x.gameObject.SetActive(false); } });
        TakeWeapon(nextGun, true);

        if (currentGun != null)
        {
            GunPickUpManagerBase.Instance.ThrowGun(new GunPickUpManagerBase.ThrowData()
            {
                GunID = currentGun.GunID,
                Origin = GetThrowPosition(),
                Direction = GetThrowDirection(),
                Data = info,
                AutoDestroy = false
            });
        }

        GameEvents.ChangeWeaponEvent(nextGun.GunID);
        UIReferences.Instance.PlayerUI.WeaponStatsUI.SetActive(true);
        CanSwich = true;
    }

    // Throw the current gun
    public void ThrowCurrent(bool AutoDestroy, Vector3 throwPosition)
    {
        if (PlayerEquip[currentWeaponIndex] == null)
            return;

        int actualID = PlayerEquip[currentWeaponIndex].GunID;
        int[] info = new int[2];
        int clips = (GameConfig.Instance.AmmoType == AmmunitionType.Bullets) ?
            (int)PlayerEquip[currentWeaponIndex].RemainingClips / PlayerEquip[currentWeaponIndex].bulletsPerClip : (int)PlayerEquip[currentWeaponIndex].RemainingClips;

        info[0] = clips;
        info[1] = PlayerEquip[currentWeaponIndex].bulletsLeft;

        GunPickUpManagerBase.Instance.ThrowGun(new GunPickUpManagerBase.ThrowData()
        {
            GunID = actualID,
            Origin = throwPosition,
            Direction = transform.forward,
            Data = info,
            AutoDestroy = AutoDestroy
        });
    }

    public GunController GetGunOnListById(int id)
    {
        GunController gun = null;
        if (AllGuns.Exists(x => x != null && x.GunID == id))
        {
            gun = AllGuns.Find(x => x.GunID == id);
        }
        else
        {
            Debug.LogError($"The FPWeapon: {id} has not been integrated in this player prefab '{GameController.Instance.LastInstancedPlayerPrefabName}'");
        }
        return gun;
    }

    // Called when the game settings changed in runtime
    public void OnGameSettingsChanged()
    {
        if (GFWKCore.Settings == null) return;

        float fov = (float)GFWKCore.Settings.GetSettingOf("武器FOV");
        foreach (var item in PlayerEquip)
        {
            if (item == null) continue;
            item.SetDefaultWeaponCameraFOV(fov);
        }
    }

    // Add a weapon to the player quipped weapons list
    // <param name="slot"></param>
    // <param name="weapon"></param>
    // <param name="addSlotIfNotExist"></param>
    private void AddWeaponToSlot(int slot, GunController weapon, bool addSlotIfNotExist = false)
    {
        if (PlayerEquip == null) PlayerEquip = new List<GunController>();

        if (PlayerEquip.Count - 1 < slot)
        {
            if (!addSlotIfNotExist)
            {
                Debug.LogWarning($"Weapon slot {slot} has not been preassigned.");
                return;
            }
            else
            {
                while (PlayerEquip.Count - 1 < slot)
                {
                    PlayerEquip.Add(null);
                }
            }
        }

        PlayerEquip[slot] = weapon;
    }

    // <returns></returns>
    private Vector3 GetThrowPosition()
    {
#if GFWKTPV
        if (CameraViewSettings.IsThirdPerson())
        {
            return transform.root.TransformPoint(new Vector3(0, 1, 1) * 0.55f);
        }
#endif
        return playerReferences.PlayerCameraTransform.TransformPoint(Vector3.forward * 0.55f);
    }

    // <returns></returns>
    private Vector3 GetThrowDirection()
    {
#if GFWKTPV
        if (CameraViewSettings.IsThirdPerson())
        {
            return transform.root.forward;
        }
#endif
        return playerReferences.PlayerCameraTransform.forward;
    }
    public void HeadAnimation(int state, float speed)
    {
        if (HeadAnimator == null)
            return;
#if GFWKTPV
        if (CameraViewSettings.IsThirdPerson()) return;
#endif
        switch (state)
        {
            case 0:
                HeadAnimator.SetInteger("Reload", 0);
                break;
            case 1:
                HeadAnimator.SetInteger("Reload", 1);
                break;
            case 2:
                HeadAnimator.SetInteger("Reload", 2);
                break;
            case 3:
                HeadAnimator.Play("Insert", 0, 0);
                break;
        }
    }

    // <param name="id"></param>
    public void PlaySound(int id)
    {
        if (ASource == null) return;
        if (id == 0)
        {
            if (SwitchFireAudioClip == null) return;
            ASource.clip = SwitchFireAudioClip;
            ASource.Play();
        }
    }

    // <returns></returns>
    public AudioSource GetFireAudioSource()
    {
        if (fireAudioSource == null)
        {
            fireAudioSource = gameObject.AddComponent<AudioSource>();
            fireAudioSource.loop = false;
            fireAudioSource.playOnAwake = false;
        }

        return fireAudioSource;
    }

    // <returns></returns>
    public GunController GetCurrentWeapon()
    {
        if (CurrentGun == null)
        {
            return PlayerEquip[currentWeaponIndex];
        }
        else
        {
            return CurrentGun;
        }
    }

    // The GunID of the current equipped weapon -1 means that the player doesn't have a weapon equipped yet
    public int GetCurrentGunID
    {
        get
        {
            if (GetCurrentWeapon() == null) { return -1; }
            return GetCurrentWeapon().GunID;
        }
    }

    void OnMatchStart() { isGameStarted = true; }

    // <returns></returns>
    public bool HasAWeaponActive()
    {
        foreach (var item in PlayerEquip)
        {
            if (item == null) continue;
            if (item.gameObject.activeSelf) return true;
        }
        return false;
    }

    private PlayerReferencesHandler _playerReferences;
    public PlayerReferencesHandler playerReferences
    {
        get
        {
            if (_playerReferences == null) _playerReferences = GetComponentInParent<PlayerReferencesHandler>();
            return _playerReferences;
        }
    }

    [System.Serializable]
    public enum ChangeWeaponStyle
    {
        HideAndDraw,
        CounterStrike,
        HideCompletelyAndThenDraw,
    }

    [System.Serializable, System.Flags]
    public enum AutoChangeOnPickup
    {
        Always = 0,
        OnlyOnEmptySlots,
        OnlyOnReplacements,
        Never,
    }
}