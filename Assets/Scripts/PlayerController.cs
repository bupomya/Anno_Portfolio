using UnityEngine.InputSystem;

public class PlayerController : PlayerBase
{
    private InputAction moveAction;
    private InputAction jumpAction;

    protected override void Awake()
    {
        base.Awake();
        moveAction = InputSystem.actions.FindAction("Move");
        jumpAction = InputSystem.actions.FindAction("Jump");
    }

    protected override void ReadInput()
    {
        SetMoveInput(moveAction.ReadValue<UnityEngine.Vector2>().x);

        if (jumpAction.WasPressedThisFrame())
            RequestJump();
    }
}
