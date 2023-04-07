using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using SimpleJSON;
using System;

public class UI : MonoBehaviour
{
    public RectTransform container;

    public GameObject segmentPrefab;

    private List<GameObject> segments = new List<GameObject>();

    [Header("Info Dropdown")]

    public RectTransform infoDropdown;

    public TextMeshProUGUI infoDropdownText;

    public TextMeshProUGUI infoDropdownAddressText;

    public static UI instance;

    void Awake() => instance = this;

    void Start() => PreLoadSegments(10);

    void PreLoadSegments(int amount)
    {
        for (int x = 0; x < amount; ++x)
            CreateNewSegment();
    }

    GameObject CreateNewSegment()
    {
        GameObject segment = Instantiate(segmentPrefab);
        segment.transform.parent = container.transform;
        segments.Add(segment);

        segment.GetComponent<Button>().onClick.AddListener(() => { OnShowMoreInfo(segment); });

        segment.SetActive(false);

        return segment;
    }

    public void SetSegments(JSONNode records)
    {
        DeactivateAllSegments();
        for (int x = 0; x < records.Count; ++x)
        {
            GameObject segment = x < segments.Count ? segments[x] : CreateNewSegment();
            segment.SetActive(true);

            TextMeshProUGUI locationText = segment.transform.Find("LocationText").GetComponent<TextMeshProUGUI>();
            TextMeshProUGUI dateText = segment.transform.Find("DateText").GetComponent<TextMeshProUGUI>();

            locationText.text = records[x]["Suburb"];
            dateText.text = GetFormattedDate(records[x]["Display Date"]);
        }

        container.sizeDelta = new Vector2(container.sizeDelta.x, GetContainerHeight(records.Count));
    }

    void DeactivateAllSegments()
    {
        foreach (GameObject segment in segments)
            segment.SetActive(false);
    }

    string GetFormattedDate(string rawDate)
    {
        DateTime date = DateTime.Parse(rawDate);

        return string.Format("{0}/{1}/{2}", date.Day, date.Month, date.Year);
    }

    float GetContainerHeight(int count)
    {
        float height = 0.0f;

        height += count * (segmentPrefab.GetComponent<RectTransform>().sizeDelta.y + 1);

        height += count * container.GetComponent<VerticalLayoutGroup>().spacing;

        height += infoDropdown.sizeDelta.y;

        return height;
    }

    public void OnSearchBySuburb(TextMeshProUGUI input)
    {
        AppManager.instance.StartCoroutine("GetData", input.text);

        infoDropdown.gameObject.SetActive(false);
    }

    public void OnShowMoreInfo(GameObject segmentObject)
    {
        int index = segments.IndexOf(segmentObject);

        if (infoDropdown.transform.GetSiblingIndex() == index + 1 && infoDropdown.gameObject.activeInHierarchy)
        {
            infoDropdown.gameObject.SetActive(false);
            return;
        }

        infoDropdown.gameObject.SetActive(true);

        JSONNode records = AppManager.instance.jsonResult["result"]["records"];

        infoDropdown.transform.SetSiblingIndex(index + 1);

        infoDropdownText.text = "Starts at " + GetFormattedTime(records[index]["Times(s)"]);
        infoDropdownText.text += "\n" + records[index]["Event Type"] + " Event";
        infoDropdownText.text += "\n" + records[index]["Display Type"];

        if (records[index]["Display Address"].ToString().Length > 2)
            infoDropdownAddressText.text = records[index]["Display Address"];
        else
            infoDropdownAddressText.text = "Address not specified";
    }

    string GetFormattedTime(string rawTime)
    {
        string[] split = rawTime.Split(":"[0]);
        int hours = int.Parse(split[0]);

        return string.Format("{0}:{1} {2}", hours > 12 ? hours - 12 : hours, split[1], hours > 12 ? "PM" : "AM");
    }
}