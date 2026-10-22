using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FirearmInventory : MonoBehaviourBase
{
    [Header("武器列表")]
    public List<FirearmController> AllGuns = new List<FirearmController>();
    [HideInInspector] public List<FirearmController> PlayerEquip = new List<FirearmController>();
    [Header("玩家职业")]
    public PlayerClassPreset m_VanguardClass;
    public PlayerClassPreset m_ArtificerClass;
    public PlayerClassPreset m_PathfinderClass;
    public PlayerClassPreset m_GuardianClass;
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
    public FirearmController CurrentGun { get; set; }
    // 玩家是否可以切换武器
    public bool CanSwich { get; set; } = true;

    public bool isGameStarted { get; set; }
    // 从 GameData 缓存的选项
    public SwapOnPickup EquipPickUpMode { get; set; } = SwapOnPickup.Always;

    private int PreviousGun = 0;
    private bool isFastFire = false;
    public bool ObservedComponentsFoldoutOpen = false;
    private AudioSource ASource, fireAudioSource;

    protected override void Awake()
    {
        base.Awake();
        ASource = GetComponent<AudioSource>();
        isGameStarted = MatchTimeManagerBase.HaveTimeStarted();
        //玩家实例创建后应用 LobbyMenu 中选定的玩家职业
        SetupEquipment();
    }

    private void Start()
    {
        //禁用所有子武器并取第一个
        foreach (FirearmController g in PlayerEquip) { g?.Setup(true); }
        foreach (FirearmController guns in AllGuns) { guns.gameObject?.SetActive(false); }
        EquipmentScreenBase.Instance?.SetEquipment(PlayerEquip);
        EquipPickUpMode = GameConfig.Instance.swapOnPickupMode;
        var firstWeapon = PlayerEquip[currentWeaponIndex];
        TakeWeapon(firstWeapon);

        if (firstWeapon != null)
            GameEvents.ChangeWeaponEvent(firstWeapon.WeaponID);

        if (GameController.Instance.GameMatchState == MatchState.Waiting && !GameController.Instance.FirstSpawnDone)
        {
            BlockWeaponTable();
        }
    }
    // 按所选玩家职业设置玩家已装备的武器
    void SetupEquipment()
    {
        //玩家实例创建后应用 LobbyMenu 中选定的玩家职业
        PlayerClassPreset pcl = null;
        var currentClass = PlayerClass.Vanguard.GetSavePlayerClass();
        switch (currentClass)
        {
            case PlayerClass.Vanguard:
                pcl = m_VanguardClass;
                break;
            case PlayerClass.Pathfinder:
                pcl = m_PathfinderClass;
                break;
            case PlayerClass.Artificer:
                pcl = m_ArtificerClass;
                break;
            case PlayerClass.Guardian:
                pcl = m_GuardianClass;
                break;
        }

        if (pcl == null)
        {
            Debug.LogError($"职业 {currentClass} 尚未指定职业预设。");
            return;
        }
        // 分配玩家职业的配装武器
        AddWeaponToSlot(0, GetGunOnListById(pcl.Primary), true);
        AddWeaponToSlot(1, GetGunOnListById(pcl.Secondary), true);
        AddWeaponToSlot(2, GetGunOnListById(pcl.Lethal), true);
        AddWeaponToSlot(3, GetGunOnListById(pcl.Perks), true);
        // 想加更多武器可以在这里加

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
        if (!LobbyMenu.Instance.isCursorLocked)
            return;

        InputControl();
        CurrentGun = PlayerEquip[currentWeaponIndex];
    }

    void InputControl()
    {
        if (!CanSwich || GameConfig.Instance.isChatOpen)
            return;

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
        //快速挥刀
        if (GameInput.QuickMelee() && PlayerEquip[3] != null && PlayerEquip[3].m_AllowQuickFire && currentWeaponIndex != 3 && !isFastFire)
        {
            DoFastKnifeShot();
        }
        //快速扔手雷
        if (GameInput.QuickNade() && PlayerEquip[2] != null && PlayerEquip[2].AllowQuickFire() && currentWeaponIndex != 2 && !isFastFire)
        {
            DoSingleGrenadeThrow();
        }

        if (PlayerEquip.Count <= 0 || PlayerEquip == null)
            return;
        //滚轮切换枪械
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

        int next = (this.currentWeaponIndex + 1) % this.PlayerEquip.Count;
        if (PlayerEquip[next] == null) return 0;

        ChangeCurrentWeaponTo(next);
        return currentWeaponIndex;
    }

    public int SwitchPrevious()
    {
        if (PlayerEquip.Count <= 0 || PlayerEquip == null)
            return 0;

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
        if (equippedKnife == null || equippedKnife.Info.Type != FirearmType.Knife) return;

        PreviousGun = currentWeaponIndex;
        isFastFire = true;
        currentWeaponIndex = 3;
        PlayerEquip[PreviousGun].gameObject.SetActive(false);
        equippedKnife.gameObject.SetActive(true);
        equippedKnife.QuickMelee(OnReturnWeapon);
        CanSwich = false;
    }

    public void DoSingleGrenadeThrow()
    {
        var equippedGrenade = PlayerEquip[2];
        if (equippedGrenade == null || equippedGrenade.Info.Type != FirearmType.Grenade) return;

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
        GameEvents.ChangeWeaponEvent(PlayerEquip[currentWeaponIndex].WeaponID);
    }

    void TakeWeapon(FirearmController gun, bool forced = false)
    {
        if (!CanSwich && !forced) return;
        if (gun == null) return;
        gun.gameObject.SetActive(true);
        CanSwich = true;
    }
    // 调用后本地玩家没有武器，且不能选择任何武器
    public void BlockWeaponTable()
    {
        foreach (FirearmController g in PlayerEquip) { g.gameObject.SetActive(false); }
        ReticleBase.Instance.Show(false);
        ReticleBase.Instance.Block = true;
        CanSwich = false;
        playerReferences.playerNetwork.SetWeaponBlocked(1);
    }
    // 让本地玩家可以重新切换武器
    public void ReleaseWeapons(bool takeFirst)
    {
        CanSwich = true;
        ReticleBase.Instance.Block = false;
        ReticleBase.Instance.Show(true);

        if (!HasAWeaponActive())
        {
            if (takeFirst) TakeWeapon(PlayerEquip[0]);
            else TakeWeapon(PlayerEquip[currentWeaponIndex]);
        }

        playerReferences.playerNetwork.SetWeaponBlocked(0);
    }
    // 卸下玩家所有武器，使其无法攻击。
    public void UnEquipEverything()
    {
        foreach (FirearmController g in PlayerEquip) { g.gameObject.SetActive(false); }
        for (int i = 0; i < PlayerEquip.Count; i++)
        {
            PlayerEquip[i] = null;
        }
        ReticleBase.Instance.Show(false);
        ReticleBase.Instance.Block = true;
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
    // 把当前武器（若有）换成指定武器
    public void ChangeTo(int WeaponTableIndex)
    {
        StartCoroutine(ChangeGun(currentWeaponIndex, AllGuns[WeaponTableIndex].gameObject, currentWeaponIndex));
        PlayerEquip[currentWeaponIndex] = AllGuns[WeaponTableIndex];
    }
    // 无延迟/无动画地切换武器
    public void ChangeToInstant(int WeaponTableIndex)
    {
        PlayerEquip[currentWeaponIndex].gameObject.SetActive(false);
        AllGuns[WeaponTableIndex].gameObject.SetActive(true);
        PlayerEquip[currentWeaponIndex] = AllGuns[WeaponTableIndex];
        GameEvents.ChangeWeaponEvent(PlayerEquip[currentWeaponIndex].WeaponID);
    }
    // 把当前已装备武器换成指定武器槽中的那把
    // 武器在 <see cref="PlayerEquip"/> 列表中的索引
    public void ChangeCurrentWeaponTo(int nextSlotID)
    {
        if (currentWeaponIndex == nextSlotID || PlayerEquip[nextSlotID] == null) return;
        if (!CanSwich || GameConfig.Instance.isChatOpen)
            return;

        var next = PlayerEquip[nextSlotID];
        if (next == null || !next.CanBeTaken()) return;

        PreviousGun = currentWeaponIndex;
        StartCoroutine(ChangeGun(currentWeaponIndex, next.gameObject, nextSlotID));
        currentWeaponIndex = nextSlotID;
    }
    // 切换枪械的协程
    public IEnumerator ChangeGun(int IDfrom, GameObject t_next, int newID)
    {
        CanSwich = false;
        EquipmentScreenBase.Instance?.SetEquippedSlot(newID);
        float hideTime = PlayerEquip[IDfrom].DisableWeapon();
        //立即禁用当前武器并启用下一把
        if (changeWeaponStyle == ChangeWeaponStyle.CounterStrike)
        {
            PlayerEquip[IDfrom].gameObject.SetActive(false);
        }
        else if (changeWeaponStyle == ChangeWeaponStyle.HideAndDraw)
        {
            HeadAnimator?.Play("SwichtGun", 0, 0);
            //等待固定延迟后再启用下一把
            yield return new WaitForSeconds(SwichTime);
        }
        else if (changeWeaponStyle == ChangeWeaponStyle.HideCompletelyAndThenDraw)
        {
            HeadAnimator?.Play("SwichtGun", 0, 0);
            //等当前武器的隐藏动画播完再启用下一把
            yield return new WaitForSeconds(hideTime);
        }
        foreach (FirearmController guns in AllGuns)
        {
            if (guns.gameObject.activeSelf == true)
            {
                guns.gameObject.SetActive(false);
            }
        }
        TakeWeapon(PlayerEquip[newID], true);
        GameEvents.ChangeWeaponEvent(PlayerEquip[newID].WeaponID);
    }
    // 本地玩家在地图上拾取武器时调用
    public void PickUpGun(FirearmPickUpData e)
    {
        if (FirearmPickUpManagerBase.Instance == null)
        {
            Debug.LogWarning("场景中缺少 PickUpManager。");
            return;
        }
        //在该玩家预制体的第一人称武器列表中查找拾取的武器
        FirearmController pickedLocalGun = AllGuns.Find(x => x.WeaponID == e.ID);
        if (pickedLocalGun == null)
        {
            Debug.LogWarning($"武器 {e.ID} 未集成到当前玩家预制体。");
            return;
        }
        //若尚未装备
        if (!PlayerEquip.Exists(x => x != null && x.WeaponID == e.ID))
        {
            FirearmData gunInfo = GameConfig.Instance.GetWeapon(e.ID);
            //先确认配装里没有空槽位
            for (int i = 0; i < PlayerEquip.Count; i++)
            {
                //若有空槽位
                if (PlayerEquip[i] == null)
                {
                    //检查这把武器能否装备在该槽位
                    if (GameConfig.Instance.weaponSlotGuide.CanBeOnSlot(gunInfo.Type, i))
                    {
                        SetUpPickUpWeapon(e, pickedLocalGun, i);
                        if (EquipPickUpMode.IsEnumFlagPresent(SwapOnPickup.OnlyOnEmptySlots | SwapOnPickup.Always))
                        {
                            StartCoroutine(SwitchToPickUpGun(null, pickedLocalGun, null, i));
                            currentWeaponIndex = i;
                        }
                        return;
                    }
                }
            }
            //走到这里说明没有空槽位，只能替换当前武器
            //先检查拾取的武器能否替换当前槽位的武器。
            bool isEmpty = PlayerEquip[currentWeaponIndex] == null;
            int actualID = isEmpty ? -1 : PlayerEquip[currentWeaponIndex].WeaponID;
            //获取要被替换的武器所需的数据
            int[] info = new int[2];
            int clips = isEmpty ? 3 : (int)PlayerEquip[currentWeaponIndex].RemainingClips;

            info[0] = clips;
            info[1] = isEmpty ? 30 : PlayerEquip[currentWeaponIndex].bulletsLeft;
            FirearmController oldGun;
            //若拾取的武器不能替换当前槽位
            if (!GameConfig.Instance.weaponSlotGuide.CanBeOnSlot(gunInfo.Type, currentWeaponIndex))
            {
                //就找一个兼容的槽位并在那里替换武器
                for (int i = 0; i < 4; i++)
                {
                    //找到这把拾取武器的槽位时
                    if (GameConfig.Instance.weaponSlotGuide.CanBeOnSlot(gunInfo.Type, i))
                    {
                        oldGun = PlayerEquip[i];
                        //用拾取的武器替换该槽位上的武器
                        SetUpPickUpWeapon(e, pickedLocalGun, i);
                        if (EquipPickUpMode == (SwapOnPickup.OnlyOnReplacements | SwapOnPickup.Always))
                        {
                            StartCoroutine(SwitchToPickUpGun(oldGun, pickedLocalGun, info, i));
                            currentWeaponIndex = i;
                        }
                        else
                        {
                            FirearmPickUpManagerBase.Instance.ThrowGun(new FirearmPickUpManagerBase.ThrowData()
                            {
                                WeaponID = oldGun.WeaponID,
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
            //替换当前已装备的武器
            SetUpPickUpWeapon(e, pickedLocalGun, currentWeaponIndex);
            StartCoroutine(SwitchToPickUpGun(oldGun, pickedLocalGun, info, currentWeaponIndex));
        }
        else
        {
            // 只当作弹药拾取
            foreach (FirearmController g in PlayerEquip)
            {
                if (g != null && g.WeaponID == e.ID)
                {
                    g.OnPickUpAmmo(e.Bullets * e.Clips, 1, e.ID);
                    break;
                }
            }
        }
    }
    void SetUpPickUpWeapon(FirearmPickUpData data, FirearmController gun, int slotID)
    {
        PlayerEquip[slotID] = gun;
        gun.Setup(true);
        if (gun.Info.Type == FirearmType.Grenade)
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
        EquipmentScreenBase.Instance?.ReplaceSlot(slotID, gun);
    }
    public IEnumerator SwitchToPickUpGun(FirearmController currentGun, FirearmController nextGun, int[] info, int newSlotID)
    {
        HeadAnimator?.Play("TakeGun", 0, 0);
        currentGun?.DisableWeapon();
        if (currentWeaponIndex != newSlotID) EquipmentScreenBase.Instance?.SetEquippedSlot(newSlotID);
        yield return new WaitForSeconds(PickUpTime);
        AllGuns.ForEach(x => { if (x != null) { x.gameObject.SetActive(false); } });
        TakeWeapon(nextGun, true);
        if (currentGun != null)
        {
            FirearmPickUpManagerBase.Instance.ThrowGun(new FirearmPickUpManagerBase.ThrowData()
            {
                WeaponID = currentGun.WeaponID,
                Origin = GetThrowPosition(),
                Direction = GetThrowDirection(),
                Data = info,
                AutoDestroy = false
            });
        }
        GameEvents.ChangeWeaponEvent(nextGun.WeaponID);
        UIReferences.Instance.PlayerUI.WeaponStatsUI.SetActive(true);
        CanSwich = true;
    }
    // 扔掉当前枪械
    public void ThrowCurrent(bool AutoDestroy, Vector3 throwPosition)
    {
        if (PlayerEquip[currentWeaponIndex] == null)
            return;
        int actualID = PlayerEquip[currentWeaponIndex].WeaponID;
        int[] info = new int[2];
        int clips = (GameConfig.Instance.AmmoType == AmmunitionType.Bullets) ?
            (int)PlayerEquip[currentWeaponIndex].RemainingClips / PlayerEquip[currentWeaponIndex].bulletsPerClip : (int)PlayerEquip[currentWeaponIndex].RemainingClips;
        info[0] = clips;
        info[1] = PlayerEquip[currentWeaponIndex].bulletsLeft;
        FirearmPickUpManagerBase.Instance.ThrowGun(new FirearmPickUpManagerBase.ThrowData()
        {
            WeaponID = actualID,
            Origin = throwPosition,
            Direction = transform.forward,
            Data = info,
            AutoDestroy = AutoDestroy
        });
    }
    public FirearmController GetGunOnListById(int id)
    {
        FirearmController gun = null;
        if (AllGuns.Exists(x => x != null && x.WeaponID == id))
        {
            gun = AllGuns.Find(x => x.WeaponID == id);
        }
        else
        {
            Debug.LogError($"第一人称武器 {id} 未集成到玩家预制体“{GameController.Instance.LastInstancedPlayerPrefabName}”。");
        }
        return gun;
    }
    // 运行时游戏设置变化时调用
    public void OnGameSettingsChanged()
    {
        if (GameCore.Settings == null) return;
        float fov = (float)GameCore.Settings.GetSettingOf("武器FOV");
        foreach (var item in PlayerEquip)
        {
            if (item == null) continue;
            item.SetDefaultWeaponCameraFOV(fov);
        }
    }
    // 把武器加入玩家已装备武器列表
    private void AddWeaponToSlot(int slot, FirearmController weapon, bool addSlotIfNotExist = false)
    {
        if (PlayerEquip == null) PlayerEquip = new List<FirearmController>();
        if (PlayerEquip.Count - 1 < slot)
        {
            if (!addSlotIfNotExist)
            {
                Debug.LogWarning($"武器槽位 {slot} 尚未预分配。");
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
    private Vector3 GetThrowPosition()
    {
        return playerReferences.PlayerCameraTransform.TransformPoint(Vector3.forward * 0.55f);
    }
    private Vector3 GetThrowDirection()
    {
        return playerReferences.PlayerCameraTransform.forward;
    }
    public void HeadAnimation(int state, float speed)
    {
        if (HeadAnimator == null)
            return;
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
    public FirearmController GetCurrentWeapon()
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
    // 当前已装备武器的 WeaponID，-1 表示玩家还没有装备武器
    public int GetCurrentGunID
    {
        get
        {
            if (GetCurrentWeapon() == null) { return -1; }
            return GetCurrentWeapon().WeaponID;
        }
    }
    void OnMatchStart() { isGameStarted = true; }
    public bool HasAWeaponActive()
    {
        foreach (var item in PlayerEquip)
        {
            if (item == null) continue;
            if (item.gameObject.activeSelf) return true;
        }
        return false;
    }
    private PlayerRig _playerReferences;
    public PlayerRig playerReferences
    {
        get
        {
            if (_playerReferences == null) _playerReferences = GetComponentInParent<PlayerRig>();
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
    public enum SwapOnPickup
    {
        Always = 0,
        OnlyOnEmptySlots,
        OnlyOnReplacements,
        Never,
    }
}
