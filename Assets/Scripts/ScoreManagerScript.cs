using UnityEngine;
using FiroozehGameServiceAndroid.Utils;
using UnityEngine.UI;

public class ScoreManagerScript : MonoBehaviour {

    public static int Score { get; set; }
	int previousScore = -1;
	public static Text score;

	// Use this for initialization
	void Start ()
	{
		score = GetComponentInChildren<Text>();
        score.gameObject.SetActive(false);
	}
	
	// Update is called once per frame
	void Update ()
	{
	    if (previousScore == Score) return;
		    score.text = FarsiTextUtil.FixText(Score + " امتیاز ");

	}

}
