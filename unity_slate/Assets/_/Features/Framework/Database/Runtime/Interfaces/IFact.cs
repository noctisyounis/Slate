using System;

namespace Database.Runtime.Interfaces
{
    public interface IFact
    {
        Type ValueType { get; }
        object GetObjectValue { get; }
        void SetObjectValue(object value);
        bool IsPersistent { get; }
    }
}