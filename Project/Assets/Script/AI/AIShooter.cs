using UnityEngine;
using Game.Runtime.AI;
using UnityEngine.AI;

public abstract class AIShooter : MonoBehaviourBase
{
    public AIAgentState AgentState
    {
        get;
        set;
    } = AIAgentState.Idle;

    public Transform Target
    {
        get;
        set;
    }

    public virtual Transform AimTarget
    {
        get;
        protected set;
    }

    public abstract float CachedTargetDistance
    {
        get;
        set;
    }

    public AILookAt LookingAt 
    { 
        get; 
        set; 
    } = AILookAt.Path;

    public bool isDeath
    {
        get;
        set;
    } = false;

    public Team AITeam
    {
        get;
        set;
    } = Team.None;

    public bool FocusOnSingleTarget 
    { 
        get;
        set; 
    }

    public bool isTeamMate
    {
        get
        {
            return (AITeam == PhotonNetworkWrapper.LocalPlayer.GetPlayerTeam() && !isOneTeamMode);
        }
    }

    public virtual NavMeshAgent Agent
    {
        get;
        set;
    }

    private string _AIName = string.Empty;
    public string AIName
    {
        get
        {
            return _AIName;
        }
        set
        {
            _AIName = value;
            gameObject.name = value;
        }
    }

    public abstract void Init();

    public virtual void OnDeath() { }

    public virtual void CheckTargets() { }

    public abstract void Respawn();

    public abstract void UpdateTargetList();

    public abstract void OnGetHit(Vector3 fromPosition);

    public virtual Vector3 TargetPosition
    {
        get => Target.position;
    }

    private Vector3 m_lookAtDirection = Vector3.forward;
    public virtual Vector3 LookAtDirection
    {
        get => m_lookAtDirection;
        set => m_lookAtDirection = value;
    }

    private Vector3 m_lookAtPosition = Vector3.forward;
    public virtual Vector3 LookAtPosition
    {
        get => m_lookAtPosition;
        set => m_lookAtPosition = value;
    }

    private AIShooterReferences _references;
    public AIShooterReferences References
    {
        get
        {
            if (_references == null) _references = GetComponent<AIShooterReferences>();
            return _references;
        }
    }

}