using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DeltaDNA; 

public class Seasons : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        DDNA.Instance.SetLoggingLevel(DeltaDNA.Logger.Level.DEBUG);

        // Hook up callback to fire when DDNA SDK has received session config info, including Event Triggered campaigns.
        DDNA.Instance.NotifyOnSessionConfigured(true);
        DDNA.Instance.OnSessionConfigured += (bool cachedConfig) => GetGameConfig(cachedConfig);

        // Allow multiple game parameter actions callbacks from a single event trigger        
        DDNA.Instance.Settings.MultipleActionsForEventTriggerEnabled = true;

        DDNA.Instance.Settings.DefaultGameParameterHandler = new GameParametersHandler(gameParameters =>
        {
            // do something with the game parameters
            myGameParameterHandler(gameParameters);
        });

        DDNA.Instance.StartSDK();
    }



    // The callback indicating that the deltaDNA has downloaded its session configuration, including 
    // Event Triggered Campaign actions and logic, is used to record a "sdkConfigured" event 
    // that can be used provision remotely configured parameters. 
    // i.e. deferring the game session config until it knows it has received any info it might need
    public void GetGameConfig(bool cachedConfig)
    {
        Debug.Log("SDK Session Configuration Loaded, Cached =  " + cachedConfig.ToString());
        Debug.Log("Recording getGameConfig event for Event Triggered Campaigns to react to");

        // Create an sdkConfigured event object
        var gameEvent = new GameEvent("getGameConfig")
            .AddParam("clientVersion", DDNA.Instance.ClientVersion);
            

        // Record sdkConfigured event and run default response hander
        DDNA.Instance.RecordEvent(gameEvent).Run();
    }




    private void myGameParameterHandler(Dictionary<string, object> gameParameters)
    {
        // Parameters Received      
        Debug.Log("Received game parameters from event trigger: " + DeltaDNA.MiniJSON.Json.Serialize(gameParameters));
    }
}
