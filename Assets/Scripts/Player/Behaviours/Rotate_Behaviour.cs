using UnityEngine;

public class Rotate_Behaviour : MonoBehaviour
{
    private void RotateAsset(Transform gameObject, Vector2 direction)
    {
        if (gameObject != null)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            gameObject.rotation = Quaternion.Euler(new Vector3(0, 0, angle));
        }
        else
        {
            Debug.LogError("GameObject is null!");
        }
    }
}
