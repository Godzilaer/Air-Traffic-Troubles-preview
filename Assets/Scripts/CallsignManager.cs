using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CallsignManager : MonoBehaviour
{
    [SerializeField]
    private TextAsset iataCodesRaw;

    private List<string> usedCallsigns;
    private string[] iataCodes;
    //Excluding I and O as per IATA rules
    private readonly string alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ";

    private void Start()
    {
        usedCallsigns = new List<string>();
        iataCodes = iataCodesRaw.text.Split(new string[] { System.Environment.NewLine }, System.StringSplitOptions.None);
    }

    public string GetGeneralAviationCallsign()
    {
        string callsign;

        //Loop here so that if callsign in use a new one will be generated
        do
        {
            callsign = "N";
            callsign += Random.Range(100, 1000);

            if (Random.Range(0, 2) == 0)
            {
                callsign += Random.Range(10, 100);
            }
            else
            {
                callsign += alphabet[Random.Range(0, 24)].ToString() + alphabet[Random.Range(0, 24)].ToString();
            }
        } while (usedCallsigns.Contains(callsign));

        usedCallsigns.Add(callsign);
        return callsign;
    }

    public string GetAirlinerCallsign()
    {
        string callsign;

        do
        {
            callsign = "";
            //Add airline identifier
            callsign = iataCodes[Random.Range(0, iataCodes.Length)];
            //Add random number as flight number with bias towards smaller flight number
            callsign += Mathf.CeilToInt(Random.Range(0.01f, 1f) * Random.Range(1, 10000));
        } while (usedCallsigns.Contains(callsign));

        usedCallsigns.Add(callsign);
        return callsign;
    }
}
