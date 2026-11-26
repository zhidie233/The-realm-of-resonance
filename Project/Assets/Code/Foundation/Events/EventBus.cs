using System;
using System.Collections.Generic;
namespace Resonance.Foundation
{

    /// <summary>
    /// Lightweight strongly-typed publish/subscribe message bus replacing static Action/delegate hubs.
    /// </summary>
    public static class EventBus
    {
        private static class Channel<T>
        {
            public static readonly List<Action<T>> Handlers = new List<Action<T>>();
        }

        private static readonly List<Action> _clearActions = new List<Action>();

        public static void Subscribe<T>(Action<T> handler)
        {
            if (handler == null) return;
            var list = Channel<T>.Handlers;
            if (list.Count == 0 && !_clearActions.Contains(ClearChannel<T>))
            {
                _clearActions.Add(ClearChannel<T>);
            }
            if (!list.Contains(handler))
            {
                list.Add(handler);
            }
        }

        public static void Unsubscribe<T>(Action<T> handler)
        {
            if (handler == null) return;
            Channel<T>.Handlers.Remove(handler);
        }

        public static void Publish<T>(T message)
        {
            var list = Channel<T>.Handlers;
            for (int i = list.Count - 1; i >= 0; i--)
            {
                if (i < list.Count)
                {
                    list[i]?.Invoke(message);
                }
            }
        }

        public static void ClearAll()
        {
            for (int i = 0; i < _clearActions.Count; i++)
            {
                _clearActions[i]?.Invoke();
            }
        }

        private static void ClearChannel<T>() => Channel<T>.Handlers.Clear();
    }
}
