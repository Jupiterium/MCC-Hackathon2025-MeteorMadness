using UnityEngine;
using TMPro;

public class ConsequencesUI : MonoBehaviour
{
    [Header("Assign TMP text boxes")]
    public TMP_Text titleText;   // e.g., "Impact Consequences"
    public TMP_Text bodyText;    // details (heatmap radius, tsunami, etc.)

    [Header("Behaviour")]
    public bool hideOnStart = true;
    public GameObject root;      // optional: parent panel to enable/disable

    void Awake()
    {
        if (hideOnStart) SetVisible(false);
        Debug.Log("[ConsequencesUI] Ready.");
    }

    public void ShowConsequences(
        float heatmapRadiusKm,
        string regionHint = "Ocean",
        string earthquakeNote = "Aftershocks likely near epicenter",
        string tsunamiNote = "Local tsunami risk near coastlines",
        float estQuakeMagnitude = -1f // pass -1 to omit
    )
    {
        SetVisible(true);

        if (titleText) titleText.text = "Impact Consequences";

        // Build a short, readable body. Keep it simple for the demo.
        System.Text.StringBuilder sb = new System.Text.StringBuilder(256);
        sb.AppendLine($"• Heatmap radius: <b>{heatmapRadiusKm:###,###} km</b>");
        sb.AppendLine($"• Impact region: <b>{regionHint}</b>");
        if (estQuakeMagnitude >= 0f)
            sb.AppendLine($"• Estimated seismic activity: <b>M{estQuakeMagnitude:0.0}</b>");
        sb.AppendLine($"• Earthquake note: {earthquakeNote}");
        sb.AppendLine($"• Tsunami note: {tsunamiNote}");

        if (bodyText) bodyText.text = sb.ToString();

        Debug.Log($"[ConsequencesUI] Shown. Radius={heatmapRadiusKm}km, Region={regionHint}");
    }

    public void Hide()
    {
        SetVisible(false);
        Debug.Log("[ConsequencesUI] Hidden.");
    }

    void SetVisible(bool v)
    {
        if (root != null) root.SetActive(v);
        else gameObject.SetActive(v); // fallback
    }
}
