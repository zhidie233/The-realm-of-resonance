using UnityEngine;
using Game.Runtime.AI;
using UnityEngine.AI;

public abstract class AISoldierCore : MonoBehaviourBase
{
    public AIBrainState AgentState
    {
        get;
        set;
    } = AIBrainState.Idle;

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

    public AIGaze LookingAt 
    { 
        get; 
        set; 
    } = AIGaze.Path;

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
            return (AITeam == NetworkWrapper.LocalPlayer.GetPlayerTeam() && !isOneTeamMode);
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

    private AISoldierReferences _references;
    public AISoldierReferences References
    {
        get
        {
            if (_references == null) _references = GetComponent<AISoldierReferences>();
            return _references;
        }
    }

}