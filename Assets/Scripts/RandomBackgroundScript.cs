using UnityEngine;
using System.Collections;

public class RandomBackgroundScript : MonoBehaviour {

	// Use this for initialization
	void Start () {
        ((SpriteRenderer) GetComponent<Renderer>()).sprite = Backgrounds[Random.Range(0, Backgrounds.Length)];
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public Sprite[] Backgrounds;
}
