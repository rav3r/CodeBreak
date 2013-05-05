using UnityEngine;
using System.Collections;

public class CodeTut : MonoBehaviour
{

	public OTSprite playButton;
	
	void Start ()
	{
	
	}
	
	void Update ()
	{
		if(OT.Clicked(playButton))
		{
			Application.LoadLevel("CodeBreak");
		}
	}
}
