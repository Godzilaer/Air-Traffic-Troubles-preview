using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

public class Level : MonoBehaviour, IPointerClickHandler {
    public int id;
    public LevelSettings settings;

    public enum PlaneType {
        GeneralAviation, RegionalJet, DualJet, QuadJet
    }

    [System.Serializable]
    public class LevelSettings {
        public PlaneType[] usedPlanes;
        public float baseSpawnRate;
    }

    private void Start() {
        gameObject.name = id.ToString(); 
        transform.Find("Title").GetComponent<TextMeshProUGUI>().text = "Level " + id.ToString();

        Transform thumbnail = transform.Find("Thumbnail");

        if (!UserData.CanUserAccessLevel(id)) {
            thumbnail.Find("Lock").gameObject.SetActive(true);
        }

        Instantiate(Resources.Load<GameObject>("MainMenu/LevelRunwayPreviews/" + id), thumbnail);
    }

    public void OnPointerClick(PointerEventData eventData) {
        LevelSelectionManager.Instance.LevelClicked(id);
    }
}