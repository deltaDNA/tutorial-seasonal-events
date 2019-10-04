using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using DeltaDNA; 

public class Seasons : MonoBehaviour
{
    // userID override. Helpful if you want to change the userID 
    // for testing campaigns as a different/fresh user
    public string userID = null;

    // Local Images used for Seasonal backgrounds
    //private SpriteRenderer BackgroundSprite = null; 

    // Holds player in a variant group for the duration of a season
    string seasonVariantLock = null;
    string seasonName = null;
    int seasonNumber = 0;
    int currencySpent = 0; 

    void Start()
    {

        if (userID != DDNA.Instance.UserID)
        {
            ClearCurrentSeason();
            DDNA.Instance.ClearPersistentData();
        }


        // Boiler plate SDK code to enable debugging and callbacks
        DDNA.Instance.SetLoggingLevel(DeltaDNA.Logger.Level.DEBUG);

        // Hook up callback to fire when DDNA SDK has received session config info, including Event Triggered campaigns.
        DDNA.Instance.NotifyOnSessionConfigured(true);
        DDNA.Instance.OnSessionConfigured += (bool cachedConfig) => GetGameConfig(cachedConfig);

        // Hook up callback handler and allow multiple game parameter action campaigns from a single event trigger    
        DDNA.Instance.Settings.MultipleActionsForEventTriggerEnabled = true;
        DDNA.Instance.Settings.DefaultGameParameterHandler = new GameParametersHandler(gameParameters =>
        {            
            myGameParameterHandler(gameParameters);
        });

        
        // Retrieve previously stored season and season variant lock        
        GetCurrentSeason();

        DDNA.Instance.StartSDK(userID);

        

    }




    // Callback indicating session configuration has been downloaded
    // Records an event to provision remotely configured parameters.     
    // i.e. Deferring remote configuration until SDK has downloaded all campaign info
    public void GetGameConfig(bool cachedConfig)
    {
        Debug.Log("SDK Session Configuration Loaded, Cached =  " + cachedConfig.ToString());
        Debug.Log("Recording getGameConfig event for Event Triggered Campaigns to react to");

        // Create an getGameConfig event object
        var gameEvent = new GameEvent("getGameConfig")
            .AddParam("clientVersion", DDNA.Instance.ClientVersion)
            .AddParam("seasonName", !string.IsNullOrEmpty(seasonName) ? seasonName:"UNKNOWN")
            .AddParam("seasonNumber",seasonNumber);


        // Record sdkConfigured event and run default response hander
        DDNA.Instance.RecordEvent(gameEvent).Run();


        if (!string.IsNullOrEmpty(seasonName))
        {
            SetBackgroundImage();
        } 
    }




    private void myGameParameterHandler(Dictionary<string, object> gameParameters)
    {
        // Parameters Received      
        Debug.Log("Received game parameters from event trigger: " + DeltaDNA.MiniJSON.Json.Serialize(gameParameters));
        if (gameParameters.ContainsKey("seasonName"))
        {
            seasonName = gameParameters["seasonName"].ToString();
            seasonVariantLock = gameParameters["seasonVariantLock"].ToString();            
        }
        else if (gameParameters.ContainsKey("seasonNumber"))
        {
            seasonNumber = System.Convert.ToInt32(gameParameters["seasonNumber"].ToString());
            seasonName = "Season_" + gameParameters["seasonNumber"].ToString(); 
            seasonVariantLock = gameParameters["seasonVariantLock"].ToString();          
        }
        SetCurrentSeason();
    }




    private void SetBackgroundImage()
    {
        var BackgroundSprite = GameObject.Find("BackgroundImage").GetComponent<SpriteRenderer>();
        Debug.Log("Setting Season to " + seasonName);
        var s = Resources.Load<Sprite>("Images/" + seasonName);
        if (BackgroundSprite != null)
        {
            GameObject.Find("BackgroundImage").GetComponent<SpriteRenderer>().sprite = s;
        }
        
        GameObject.Find("txtSeasonName").GetComponent<UnityEngine.UI.Text>().text = "Season Name : " + seasonName;
        GameObject.Find("txtSeasonNumber").GetComponent<UnityEngine.UI.Text>().text = "Season Number : " + seasonNumber;
        GameObject.Find("txtSeasonLock").GetComponent<UnityEngine.UI.Text>().text = "Season Lock : " + seasonVariantLock;
        GameObject.Find("txtCurrencySpent").GetComponent<UnityEngine.UI.Text>().text = "Currency Spent : $" + currencySpent/100;
    }






    // The Season and Variant group that the player is placed into for the 
    // duration of the current season, is persisted locally on the client
    private void GetCurrentSeason()
    {
        seasonName = PlayerPrefs.GetString("SeasonName", null);
        seasonNumber = PlayerPrefs.GetInt("SeasonNumber", 0);
        seasonVariantLock =  PlayerPrefs.GetString("SeasonLockVariant", null);
        currencySpent = PlayerPrefs.GetInt("TotalRealCurrencySpent", 0);
    }


    private void SetCurrentSeason()
    {
        if (seasonVariantLock != null)
        {
            PlayerPrefs.SetString("SeasonLockVariant", seasonVariantLock);
        }
        if (seasonName != null)
        {
            PlayerPrefs.SetString("SeasonName", seasonName);
        }
        if (seasonNumber > 0)
        {
            PlayerPrefs.SetInt("SeasonNumber", seasonNumber);
        }
        if (currencySpent > 0)
        {
            PlayerPrefs.SetInt("TotalRealCurrencySpent", currencySpent);
        }

        SetBackgroundImage();

    }


    private void ClearCurrentSeason()
    {
        PlayerPrefs.DeleteKey("SeasonName");
        PlayerPrefs.DeleteKey("SeasonNumber");
        PlayerPrefs.DeleteKey("SeasonLockVariant");
        PlayerPrefs.DeleteKey("TotalRealCurrencySpent");

    }


    public void BuyIAP()
    {

        Debug.Log("Purchased IAP");

        var transaction = new Transaction(
        "Seasonal Purchase - " + seasonName,
        "PURCHASE",
        new Product()
            .AddVirtualCurrency("Coins", "PREMIUM", 1000),
        new Product()
            .SetRealCurrency("USD", Product.ConvertCurrency("USD", 9.99m))); // $9.99   
    
        DDNA.Instance.RecordEvent(transaction).Run();
        DDNA.Instance.Upload();


        currencySpent += 999;
        SetCurrentSeason();                   
    }

}
