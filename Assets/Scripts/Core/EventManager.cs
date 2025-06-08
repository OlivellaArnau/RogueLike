using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;

public class EventManager : MonoBehaviour
{
    private Dictionary<string, UnityEvent<object>> eventDictionary;
    private static EventManager eventManager;
    public static EventManager Instance
    {
        get
        {
            if (!eventManager)
            {
                GameObject obj = new GameObject("EventManager");
                eventManager = obj.AddComponent<EventManager>();
            }
            return eventManager;
        }
    }
    
    private void Awake()
    {
        // Inicializar el diccionario
        if (eventDictionary == null)
        {
            eventDictionary = new Dictionary<string, UnityEvent<object>>();
        }
        if (eventManager != null && eventManager != this)
        {
            Destroy(gameObject);
            return;
        }
        
        eventManager = this;
        DontDestroyOnLoad(gameObject);
    }
    
    public static void StartListening(string eventName, UnityAction<object> listener)
    {
        if (Instance.eventDictionary.TryGetValue(eventName, out UnityEvent<object> thisEvent))
        {
            thisEvent.AddListener(listener);
        }
        else
        {
            thisEvent = new UnityEvent<object>();
            thisEvent.AddListener(listener);
            Instance.eventDictionary.Add(eventName, thisEvent);
        }
    }
    
    public static void StopListening(string eventName, UnityAction<object> listener)
    {
        if (eventManager == null)
            return;
            
        if (Instance.eventDictionary.TryGetValue(eventName, out UnityEvent<object> thisEvent))
        {
            thisEvent.RemoveListener(listener);
        }
    }
    public static void TriggerEvent(string eventName, object data = null)
    {
        if (Instance.eventDictionary.TryGetValue(eventName, out UnityEvent<object> thisEvent))
        {
            thisEvent.Invoke(data);
        }
    }
}

