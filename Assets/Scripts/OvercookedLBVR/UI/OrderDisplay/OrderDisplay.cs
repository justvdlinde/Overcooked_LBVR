using UnityEngine;
using UnityEngine.UI;

public class OrderDisplay : MonoBehaviour
{
    public OrderDisplayGrid grid;
    public Slider timeSlider;
    public int OrderNumber => deliveryPoint.OrderIndex;

    public Order Order => order;
    private Order order = null;

    [SerializeField] private DeliveryPoint deliveryPoint = null;
    [SerializeField] private AudioSource lowTimeLeftSource = null;
    [SerializeField] private AudioSource orderRecievedSource = null;
    private bool lowTimeHasPlayed = false;

    [SerializeField] private Color fullTimeColor = Color.green;
    [SerializeField] private Color halfTimeColor = new Color(1f, 0.62f, 0f, 1f);
    [SerializeField] private Color lowTimecolor = Color.red;

    [SerializeField] private Color lowTimeColorA = Color.red;
    [SerializeField] private Color lowTimeColorB = Color.red * new Color(0.6f,1f,1f,1f);

    [SerializeField] private Image sliderImage = null;
    [SerializeField] private Animator warningUIAnimator = null;

	private void Awake()
    {
        timeSlider.value = 0;
        warningUIAnimator.gameObject.SetActive(false);
        warningUIAnimator.SetBool("ShouldAnimate", true);
    }

    public void Update()
    {
        if (order != null)
		{

            timeSlider.value = order.timer.TimeRemaining / order.timer.Duration;
            if(!lowTimeHasPlayed && order.timer.TimeRemaining < order.timer.Duration * 0.4f)
			{
                lowTimeLeftSource.pitch = 1f + (((UnityEngine.Random.value + 1f) * 0.5f) * 0.1f);
                lowTimeLeftSource.Play();
                lowTimeHasPlayed = true;
			}

            if(order.timer.TimeRemaining < order.timer.Duration * 0.4f)
			{
                warningUIAnimator.gameObject.SetActive(true);
                float timeFactor = 1f;
                float t = (Mathf.Sin(Time.time * 5f * timeFactor) + 1) * 0.5f;
                sliderImage.color = Color.Lerp(lowTimeColorA, lowTimeColorB, t);
            }
            else
			{
                warningUIAnimator.gameObject.SetActive(false);
                float timeFactor = Mathf.InverseLerp(0.25f, 1f, order.timer.TimeRemaining / order.timer.Duration);
                sliderImage.color = ThreePointLerp(lowTimecolor, halfTimeColor, fullTimeColor, timeFactor);
			}
		}
        else
		{
            sliderImage.color = fullTimeColor;
            warningUIAnimator.gameObject.SetActive(false);

        }
    }

    private Color ThreePointLerp(Color a, Color b, Color c, float t)
	{
        if (t < 0.5f)
            return Color.Lerp(a, b, t * 2f);
        else
            return Color.Lerp(b, c, (t - 0.5f) * 2);
	}

    public void Show(Order order)
    {
        // TODO: some kind of animation/flair
        Clear();
        this.order = order;
        this.order.orderIndex = OrderNumber;

        orderRecievedSource.pitch = 1f + (((UnityEngine.Random.value + 1f) * 0.5f) * 0.1f);
        orderRecievedSource.Play();
        lowTimeHasPlayed = false;

        grid.DisplayOrder(order);
    }

    public void Clear()
    {
        // TODO: some kind of animation/flair
        grid.Clear();
        order = null;
        timeSlider.value = 0;
    }

    public bool CanBeUsed()
    {
        return order == null;
    }
}
