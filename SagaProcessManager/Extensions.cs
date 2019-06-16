using Common;

namespace SagaProcessManager
{
    internal static class Extensions
    {
        internal static bool BelongsToSaga<TMessage>(this TMessage _) where TMessage : IMessage
            => true;
    }
}
