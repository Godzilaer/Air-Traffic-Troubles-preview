using System.Collections.Generic;
using UnityEngine;

public class CallsignManager : MonoBehaviour {
    public static List<string> usedCallsigns;
    //Excluding I and O as per IATA rules
    public static readonly string alphabet = "ABCDEFGHJKLMNPQRSTUVWXYZ";

    private void Awake() {
        usedCallsigns = new List<string>();
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

    public static string GetAirlinerCallsign(string airline) {
        string callsign;

        //Repeat until it is not a duplicate
        do {
            callsign = airline;
            //Add random number as flight number with bias towards smaller flight number
            callsign += Mathf.CeilToInt(Random.Range(0.001f, 1f) * Random.Range(1, 10000));
        } while (usedCallsigns.Contains(callsign));

        usedCallsigns.Add(callsign);
        return callsign;
    }
}
