// In BMSAISystem.cs
public class BMSSquadManager : MonoBehaviour
{
    // From server.c: SquadThink()
    public void UpdateSquadBehavior(List<BMSEntity> squad)
    {
        // Server.c: MaintainFormation()
        Vector3 formationCenter = CalculateFormationCenter(squad);
        foreach (var member in squad)
        {
            if (member.CurrentAIState != AIState.Combat)
            {
                Vector3 formationPos = formationCenter + GetFormationOffset(member, squad.IndexOf(member));
                member.navAgent.SetDestination(formationPos);
            }
        }

        // Server.c: CoordinateAttacks()
        if (squad.Any(m => m.CanSeePlayer))
        {
            BMSEntity primaryTarget = squad.First(m => m.CanSeePlayer);
            foreach (var member in squad)
            {
                if (member != primaryTarget)
                {
                    member.CombatTarget = primaryTarget.CombatTarget;
                    member.SetAIState(AIState.Combat);
                }
            }
        }
    }

    private Vector3 CalculateFormationCenter(List<BMSEntity> squad)
    {
        // Implementation from server.c: CalculateGroupCenter()
        Vector3 center = Vector3.zero;
        foreach (var member in squad)
        {
            center += member.transform.position;
        }
        return center / squad.Count;
    }
}