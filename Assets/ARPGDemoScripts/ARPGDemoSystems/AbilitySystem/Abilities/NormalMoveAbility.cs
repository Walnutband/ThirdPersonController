namespace ARPGDemo.AbilitySystem
{
    public class NormalMoveAbility : AbilityBase
    {
        public override void Activate()
        {
            m_ASC.AMC.SetMoveAndRotate(true, true);
            // m_ASC.AMC.EnterNormalMoveState();
        }

        //随便打断。
        public override bool TryDeactivate()
        {
            Deactivate();
            return true;
        }

        public override void Deactivate()
        {
            //需要移动的，就让它自己调度AMC。
            // m_ASC.AMC.SetMoveAndRotate(false, false);
        }
    }
}