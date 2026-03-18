using UnityEngine;

public class Player : MonoBehaviour
{
    public PlayerAttack playerAttack{ get; private set; }
    public PlayerMove playerMove{ get; private set; }
    public PlayerState playerState{ get; private set; }
    public PlayerController playerController { get; private set; }

    void Start()
    {
        playerAttack= GetComponent<PlayerAttack>();
        playerMove= GetComponent<PlayerMove>();
        playerController= GetComponent<PlayerController>();
        playerState = GetComponent<PlayerState>();
    }

}
