
namespace ARPGDemo.ControlSystem_New
{
    public class JumpCommand : CommandBase
    {
        public float moveSpeed;
        public JumpCommand() { }
    }

    public class FallCommand : CommandBase
    {
        public float moveSpeed;
        public FallCommand() { }
    }
}