using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [SerializeField]
    private float _speed;

    private Rigidbody2D _body;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _body = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        var inputX = Input.GetAxisRaw("Horizontal");
        var inputY = Input.GetAxisRaw("Vertical");
        var vec = new Vector2(inputX, inputY);
        vec.Normalize();

        _body.velocity = vec * _speed;
    }
}
