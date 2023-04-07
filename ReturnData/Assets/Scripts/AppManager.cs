using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Text;
using System;
using System.Linq;
using SimpleJSON;

public class AppManager : MonoBehaviour
{
    public enum Duration
    {
        Today = 0,
        Week = 1,
        Month = 2,
        All = 3
    }

    public string url;

    public JSONNode jsonResult;

    public static AppManager instance;

    void Awake()
    {
        instance = this;
        
    }

    public void FilterByDuration(int durIndex)
    {
        Duration dur = (Duration)durIndex;

        JSONArray records = jsonResult["result"]["records"].AsArray;

        DateTime maxDate = new DateTime();

        switch (dur)
        {
            case Duration.Today:
                maxDate = DateTime.Now.AddDays(1);
                break;
            case Duration.Week:
                maxDate = DateTime.Now.AddDays(7);
                break;
            case Duration.Month:
                maxDate = DateTime.Now.AddMonths(1);
                break;
            case Duration.All:
                maxDate = DateTime.MaxValue;
                break;
        }

        JSONArray filteredRecords = new JSONArray();

        for (int x = 0; x < records.Count; ++x)
        {
            DateTime recordDate = DateTime.Parse(records[x]["Display Date"]);

            if (recordDate.Ticks < maxDate.Ticks)
                filteredRecords.Add(records[x]);
        }

        UI.instance.SetSegments(filteredRecords);
    }

    IEnumerator GetData(string location)
    {
        UnityWebRequest webReq = new UnityWebRequest();
        webReq.downloadHandler = new DownloadHandlerBuffer();

        webReq.url = string.Format("{0}&q={1}", url, location);

        yield return webReq.SendWebRequest();

        string rawJson = Encoding.Default.GetString(webReq.downloadHandler.data);

        jsonResult = JSON.Parse(rawJson);

        UI.instance.SetSegments(jsonResult["result"]["records"]);
    }
}