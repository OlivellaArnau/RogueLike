using UnityEngine;

[CreateAssetMenu(fileName = "EnemyStateBase", menuName = "Scriptable Objects/EnemyStateBase")]
public abstract class EnemyStateBase : ScriptableObject
{
    [Tooltip("Nombre descriptivo del estado")]
    [SerializeField] private string stateName;

    [Tooltip("Descripción del comportamiento del estado")]
    [TextArea(3, 5)]
    [SerializeField] private string stateDescription;
    public abstract void EnterState(EnemyController enemy);
    public abstract void UpdateState(EnemyController enemy);
    public abstract void ExitState(EnemyController enemy);
    public abstract EnemyStateBase CheckTransitions(EnemyController enemy);
    public string StateName => stateName;
    public string StateDescription => stateDescription;
}
