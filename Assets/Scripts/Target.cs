using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Target : MonoBehaviour
{
    [SerializeField] private Weapon player;
    [SerializeField] private Renderer enemyRenderer;

    private void Update()
    {
        if (enemyRenderer.IsVisibleFrom(Camera.main) && !player.screenTargets.Contains(transform))
        {

            if (Vector2.Distance(Camera.main.WorldToScreenPoint(transform.position + new Vector3(0, 2, 0)), new Vector2(Screen.width / 2, Screen.height / 2)) <= 150f)
            {
                player.screenTargets.Add(transform);
            }

        }

        if (!enemyRenderer.IsVisibleFrom(Camera.main) || Vector2.Distance(Camera.main.WorldToScreenPoint(transform.position + new Vector3(0, 2, 0)), new Vector2(Screen.width / 2, Screen.height / 2)) > 150f || Vector3.Distance(transform.position, player.transform.position) > 2500f && player.screenTargets.Contains(transform))
        {
            player.screenTargets.Remove(transform);
        }
    }
}
