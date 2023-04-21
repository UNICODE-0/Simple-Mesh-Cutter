using UnityEngine;

public class Knife : MonoBehaviour
{
    private GameObject _ObjectToCut;
    private void OnTriggerStay(Collider other) 
    {
        _ObjectToCut = other.gameObject;
    }
    private void OnTriggerExit(Collider other) 
    {
        _ObjectToCut = null;
    }

    private void Update() 
    {
        if(_ObjectToCut is not null && Input.GetKeyUp(KeyCode.Space))
        {
            Cutter.CutObject(_ObjectToCut, transform.position, transform.right, 
            new GameObjectProperties(ColliderType.Mesh, HasRigidbody: false),
            new GameObjectProperties(ColliderType.Mesh, HasRigidbody: true), 
            CalculateSliceUV: true);
        }
    }
}
