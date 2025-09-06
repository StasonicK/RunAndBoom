namespace CodeBase.Infrastructure.States
{
    public interface IPayloadedState<Tpayload> : IExitableState
    {
        void Enter(Tpayload payload);
    }
}