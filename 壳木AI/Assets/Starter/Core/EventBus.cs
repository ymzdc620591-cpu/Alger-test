using System;
using System.Collections.Generic;

namespace Starter.Core
{
    // 用法：EventBus.On<MyEvent>(handler)  EventBus.Emit(new MyEvent{...})  EventBus.Off<MyEvent>(handler)
    public static class EventBus
    {
        static readonly Dictionary<Type, Delegate> _table = new();

        public static void On<T>(Action<T> handler)
        {
            var t = typeof(T);
            _table[t] = _table.TryGetValue(t, out var d) ? Delegate.Combine(d, handler) : handler;
        }

        public static void Off<T>(Action<T> handler)
        {
            var t = typeof(T);
            if (!_table.TryGetValue(t, out var d)) return;
            var next = Delegate.Remove(d, handler);
            if (next == null) _table.Remove(t);
            else _table[t] = next;
        }

        public static void Emit<T>(T evt)
        {
            if (_table.TryGetValue(typeof(T), out var d))
                ((Action<T>)d)?.Invoke(evt);
        }

        public static void Clear() => _table.Clear();
    }
}
