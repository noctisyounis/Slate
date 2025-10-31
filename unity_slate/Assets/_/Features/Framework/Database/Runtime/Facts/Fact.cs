using System;
using Database.Runtime.Interfaces;

namespace Database.Runtime
{
    //[Serializable] class can't be inherited if Serializable
    // no inheritance needed for now but I don't want to have to rewrite a part of the save system in the future
    public class Fact<T> : IFact
    {
        public T Value;
        public Type ValueType => typeof(T);
        public bool IsPersistent { get; set; }

        object IFact.GetObjectValue => Value;

        public Fact(T value, bool isPersistent = false)
        {
            Value = value;
            IsPersistent = isPersistent;
        }

        public void SetObjectValue(object value)
        {
            if (value is T cast) Value = cast;
            else throw new InvalidCastException("Cannot assign a value to a fact");
        }

    }
}