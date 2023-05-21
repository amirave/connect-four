using System;

namespace LlamAcademy.Spring.Runtime
{
    public interface ISpringTo<T>
    {
        void SpringTo(T Target, Action OnFinish);
    }
}
