using UnityEngine;
using UnityEngine.Animations;

public class LookAtPlayer : MonoBehaviour
{
    public Transform Player;

    void Start()
    {
        if (Player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                Player = playerObj.transform;
            }
        }
        LookAtConstraint constraint = gameObject.AddComponent<LookAtConstraint>();
        if (Player != null)
        {
            ConstraintSource source = new ConstraintSource();
            source.sourceTransform = Player;
            source.weight = 1.0f;

            constraint.AddSource(source);
            constraint.constraintActive = true;
        }
    }
}