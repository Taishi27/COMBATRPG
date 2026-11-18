using UnityEngine;
public enum CombatRoleType 
{
    PrimaryAttacker,
    SecondaryAttacker, 
    Support,
    Chasing,
    BackStabber,
    RangedAttacker,
    Repositioning
}
public class EnemyCombatRole : MonoBehaviour
{
    public CombatRoleType roleType;
    private EnemyStateMachine enemy;

    public EnemyCombatRole(EnemyStateMachine enemy, CombatRoleType role)
    {
        this.enemy = enemy;
        this.roleType = role;
    }



    public bool ShouldFeint()
    {
        return roleType == CombatRoleType.SecondaryAttacker;
    }

    public bool ShouldHold()
    {
        return roleType == CombatRoleType.Support;
    }
}
