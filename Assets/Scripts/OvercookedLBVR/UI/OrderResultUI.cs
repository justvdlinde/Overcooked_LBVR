using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[SelectionBase]
public class OrderResultUI : MonoBehaviour
{
    private const string PERFECT_SCORE = "Perfect!";
    private const string UNCOOKED_INGREDIENTS = "Uncooked ingredients!";
    private const string WRONG_INGREDIENT_ORDER = "Ingredients are in the wrong order!";
    private const string MISSING_INGREDIENTS = "Missing ingredients!";

    [SerializeField] private float lifeTime = 2f;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI infoLabel;
    [SerializeField] private Image[] stars = null;

    [Header("Feedback")]
    public AudioSource succesAudio = null;
    public AudioSource failAudio = null;
    public ParticleSystem succesParticles = null;
    public ParticleSystem minorSuccesParticles = null;

    private ScoreData score;

    public void Setup(ScoreData score)
    {
        this.score = score;
        StartCoroutine(Show());
    }

    private IEnumerator Show()
    {
        SetupInfoLabel(score);
        SetupStars(score);
        yield return new WaitForSeconds(lifeTime);
        Destroy(gameObject);
    }

    private void SetupInfoLabel(ScoreData score)
    {
        if (score.IsPerfectScore == true)
        {
            infoLabel.text = PERFECT_SCORE;
        }
        else if (score.comparisonResult.properlyCookedIngredientsPercentage != 1)
        {
            infoLabel.text = UNCOOKED_INGREDIENTS;
        }
        else if (score.comparisonResult.ingredientsAreInCorrectOrder == false)
        {
            infoLabel.text = WRONG_INGREDIENT_ORDER;
        }
        else
        {
            infoLabel.text = MISSING_INGREDIENTS;
        }
    }

    private void SetupStars(ScoreData score)
    {
        float points = score.Points;
        float maxPoints = ScoreData.MaxPoints;

        int enabledCount = 0;

        for (int i = 0; i < stars.Length ; i++)
        {
            if((points / maxPoints) >= (((float)i + 1) / stars.Length))
			{
                enabledCount++;
                stars[i].enabled = true;
			}
            else
			{
                stars[i].enabled = false;
			}
        }

        if (enabledCount > 1)
		{
            succesAudio.Play();
            minorSuccesParticles.Play();

        }
        else
            failAudio.Play();

        if (enabledCount == 3)
            succesParticles.Play();
    }

 
}
