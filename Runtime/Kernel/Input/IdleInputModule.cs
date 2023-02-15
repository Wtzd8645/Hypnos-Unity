namespace Hypnos.IO
{
    public class IdleInputModule : InputModuleBase
    {
        public override void Reset() { }

        protected internal override void Check() { }

        protected internal override void Process() { }

        protected override void OnAxisInputReceive(int id, float x, float y) { }

        protected override void OnButtonInputReceive(int id, bool isDown) { }
    }
}