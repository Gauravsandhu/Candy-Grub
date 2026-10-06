using UnityEngine;
using UnityEngine.UI;

// On a level-select button: locks it until the previous level is complete
// and shows the best star count saved for its level.
[RequireComponent(typeof(Button))]
public class LevelSelectButton : MonoBehaviour
{
    [SerializeField] private int buildIndex = 1;
    [SerializeField] private Graphic label;
    [SerializeField] private Image[] stars;
    [SerializeField] private Color earnedStarColor = Color.white;
    [SerializeField] private Color missedStarColor = new Color(0f, 0f, 0f, 0.35f);

    void OnEnable()
    {
        Refresh();
    }

    public void Refresh()
    {
        bool unlocked = SaveSystem.IsUnlocked(buildIndex);
        int best = SaveSystem.GetBestStars(buildIndex);

        GetComponent<Button>().interactable = unlocked;
        if (label != null)
        {
            Color c = label.color;
            c.a = unlocked ? 1f : 0.4f;
            label.color = c;
        }

        for (int i = 0; i < stars.Length; i++)
        {
            stars[i].gameObject.SetActive(unlocked);
            stars[i].color = i < best ? earnedStarColor : missedStarColor;
        }
    }
}
