
namespace ARPGDemo.ControlSystem_New
{
    public class CommandBase : ICommand
    {
        protected uint m_ID = 10000;
        public uint ID => m_ID;
    }
}