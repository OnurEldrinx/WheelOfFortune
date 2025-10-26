using System;
using System.Collections.Generic;
using UnityEngine;

namespace Infra
{
    public sealed class EventBus : MonoBehaviour
    {
        private readonly Dictionary<Type, Delegate> map = new();

        public void Publish<T>(T evt)
        {
            if (map.TryGetValue(typeof(T), out var d)) (d as Action<T>)?.Invoke(evt);
        }

        public void Subscribe<T>(Action<T> h)
        {
            if (map.TryGetValue(typeof(T), out var d)) map[typeof(T)] = (Action<T>)d + h;
            else map[typeof(T)] = h;
        }

        public void Unsubscribe<T>(Action<T> h)
        {
            if (!map.TryGetValue(typeof(T), out var d)) return;
            map[typeof(T)] = (Action<T>)d - h;
        }
    }
}