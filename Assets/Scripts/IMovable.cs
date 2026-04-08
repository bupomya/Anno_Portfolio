public interface IMovable
{
    bool IsGrounded { get; }
    void SetMoveInput(float input);
    void RequestJump();
}
