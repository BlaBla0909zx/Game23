using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Observable<T>
{
    [SerializeField] private T _value;
    private List<IObserver<T>> _observers = new List<IObserver<T>>();

    public T Value
    {
        get => _value;
        set
        {
            T oldValue = _value;
            _value = value;
            NotifyObservers(oldValue, _value);
        }
    }

    public Observable(T initialValue = default(T))
    {
        _value = initialValue;
    }


    public void Subscribe(IObserver<T> observer, bool notifyImmediately = false)
    {
        if (notifyImmediately)
        {
            observer.OnValueChanged(_value, _value);
        }

        if (!_observers.Contains(observer))
        {
            _observers.Add(observer);
        }
    }

    public void Unsubscribe(IObserver<T> observer)
    {
        _observers.Remove(observer);
    }

    private void NotifyObservers(T oldValue, T newValue)
    {
        foreach (var observer in _observers)
        {
            observer.OnValueChanged(oldValue, newValue);
        }
    }

    public void NotifyCurrentValue()
    {
        foreach (var observer in _observers)
        {
            observer.OnValueChanged(_value, _value);
        }
    }

  
}


public interface IObserver<T>
{
    void OnValueChanged(T oldValue, T newValue);
}

[Serializable]
public class ObservableValue<T>
{
    [SerializeField] private T _value;
    public event Action<T, T> OnValueChanged;
    public event Action<T> OnValueSet;

    public T Value
    {
        get => _value;
        set
        {
            T oldValue = _value;
            _value = value;
            OnValueChanged?.Invoke(oldValue, _value);
            OnValueSet?.Invoke(_value);
        }
    }

    public ObservableValue(T initialValue = default(T))
    {
        _value = initialValue;
    }

    public void SetValueSilently(T value)
    {
        _value = value;
    }

    public void NotifyValueChanged()
    {
        OnValueChanged?.Invoke(_value, _value);
        OnValueSet?.Invoke(_value);
    }
    
    public void Subscribe(Action<T, T> callback, bool notifyImmediately = false)
    {
        OnValueChanged += callback;
        if (notifyImmediately && _value != null)
        {
            callback(_value, _value);
        }
    }




    public void Unsubscribe(Action<T, T> callback)
    {
        OnValueChanged -= callback;
    }

}