using System;

namespace WpfApp4
{
    public class EventArgs<T> : EventArgs
    {
        public T Data { get; }

        public EventArgs(T data) => Data = data;

        public static implicit operator EventArgs<T>(T data) => new EventArgs<T>(data);
    }
}
