using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CallsignManager : MonoBehaviour {
    [SerializeField]
    private TextAsset icaoCodesRaw;

    public static List<string> usedCallsigns;
    public static string[] icaoCodes;
    //Excluding I and O as per IATA rules
    public static readonly string alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ";

    private void Start() {
        usedCallsigns = new List<string>();
        icaoCodes = icaoCodesRaw.text.Split(new string[] { System.Environment.NewLine }, System.StringSplitOptions.None);
    }

    public static string GetGeneralAviationCallsign() {
        string callsign;

        //Repeat until it is not a duplicate
        do {
            callsign = "N";
            callsign += Random.Range(100, 1000);

            if (Random.Range(0, 2) == 0) {
                callsign += Random.Range(10, 100);
            } else {
                callsign += alphabet[Random.Range(0, 24)].ToString() + alphabet[Random.Range(0, 24)].ToString();
            }
        } while (usedCallsigns.Contains(callsign));

        usedCallsigns.Add(callsign);
        return callsign;
    }

    public static string GetAirlinerCallsign() {
        string callsign;

        //Repeat until it is not a duplicate
        do {
            callsign = "";
            //Add airline identifier
            callsign = icaoCodes[Random.Range(0, icaoCodes.Length)];
            //Add random number as flight number with bias towards smaller flight number
            callsign += Mathf.CeilToInt(Random.Range(0.01f, 1f) * Random.Range(1, 10000));
        } while (usedCallsigns.Contains(callsign));

        usedCallsigns.Add(callsign);
        return callsign;
    }
}
