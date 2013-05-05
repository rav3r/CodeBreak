using UnityEngine;
using System.Collections;

public class CodeBreak : MonoBehaviour {
	
	public OTSprite [] numbers;
	public OTSprite dumNum;
	public OTSprite [] ledCols;
	public OTSprite [] ledRows;
	public Transform nextCont;
	public OTSprite nextButton;
	public OTTextSprite timeLabel;
	public OTTextSprite stepsLabel;
	public OTTextSprite levelLabel;
	OTSprite clickedNumber = null;
	private OTSprite [,] numbersSorted = new OTSprite[3,3];
	private int [,] code = new int[3,3];
	private float time;
	private int steps;
	
	// Use this for initialization
	void Start ()
	{
		for(int i=0; i<numbers.Length; i++)
		{
			numbersSorted[i/3, i%3] = numbers[i];
		}
		
		for(int i=0; i<3; i++)
		{
			for(int j=0; j<3; j++)
			{
				code[i, j] = Random.Range(0, 3);
				numbersSorted[i, j].frameIndex = code[i, j];
			}
		}
		
		RandomCode(0);
	}
	
	int [,] levels = {{1,1,1,1,2,2,2,1,1}, {1,1,1,1,2,2,3,3,3}, {1,1,2,2,3,3,4,4,4}, {1,2,3,4,4,4,4,4,4}};
	int currLevel = 1;
	
	void RandomCode(int level)
	{
		time = 0;
		steps = 0;
		
		int levelId = level % levels.GetLength(0);
		int levelRandom = 2+level;
		int [,] randomCode = new int[3,3];
		for(int i=0; i<levels.GetLength(1); i++)
		{
			code[i/3, i%3] = randomCode[i/3, i%3] = levels[levelId, i];
		}
		bool done = false;
		while(!done)
		{
			for(int i=0; i<levelRandom; i++)
			{
				bool rowOrCol = Random.Range(0, 2) == 1;
				bool forward = Random.Range(0, 2) == 1;
				if(rowOrCol)
				{
					int row = Random.Range(0, 3);
					if(forward)
					{
						int tmp = randomCode[row, 0];
						randomCode[row, 0] = randomCode[row, 1];
						randomCode[row, 1] = randomCode[row, 2];
						randomCode[row, 2] = tmp;
					} else
					{
						int tmp = randomCode[row, 2];
						randomCode[row, 2] = randomCode[row, 1];
						randomCode[row, 1] = randomCode[row, 0];
						randomCode[row, 0] = tmp;
					}
				} else
				{
					int col = Random.Range(0, 3);
					if(forward)
					{
						int tmp = randomCode[0, col];
						randomCode[0, col] = randomCode[1, col];
						randomCode[1, col] = randomCode[2, col];
						randomCode[2, col] = tmp;
					} else
					{
						int tmp = randomCode[2, col];
						randomCode[2, col] = randomCode[1, col];
						randomCode[1, col] = randomCode[0, col];
						randomCode[0, col] = tmp;
					}
				}
			}
			
			for(int i=0; i<3; i++)
				for(int j=0; j<3; j++)
					if(randomCode[i, j] != code[i, j])
						done = true;
		}
		for(int i=0; i<3; i++)
			for(int j=0; j<3; j++)
				numbersSorted[i, j].frameIndex = randomCode[i, j];
	}
	
	void RollCode(OTSprite first, OTSprite second, OTSprite third)
	{
		audio.Play();
		OTSprite []sprites = {first, second, third};
		
		for(int i=0; i<3; i++)
		{
			iTween.MoveTo(sprites[i].gameObject,
				iTween.Hash("x",sprites[i].position.x, "y",sprites[i].position.y,
				"ease", iTween.EaseType.easeInOutSine, "time", 0.5f));
		}
		dumNum.position = third.position;
		dumNum.frameIndex = first.frameIndex;
								
		for(int j=2; j>0; j--)
			sprites[j].position = sprites[j-1].position;
		
		Vector2 dir = first.position - third.position;
		dir.Normalize();
		dir *= 350;
							
		iTween.MoveTo(first.gameObject, iTween.Hash("x",first.position.x, "y",first.position.y, "ease", iTween.EaseType.easeInSine, "time", 0.5f));
		Vector3 dest = dumNum.transform.position - new Vector3(dir.x, dir.y, 0);			
					
		iTween.MoveTo(dumNum.gameObject, iTween.Hash("x",dest.x, "y",dest.y, "ease", iTween.EaseType.easeOutSine,"time", 0.5f));
		first.transform.position +=  new Vector3(dir.x, dir.y, 0);
		
		steps++;
		stepsLabel.text = "Steps: "+steps;
	}
	
	void UpdateLed()
	{
		for(int i=0; i<9; i++)
		{
			ledCols[i].frameIndex = ledRows[i].frameIndex = 1;
		}
		
		int rightCount = 0;
		
		for(int row=0; row<3; row++)
		{
			int right = 0;
			int semiright = 0;
			for(int i=0; i<3; i++)
			{
				if(code[row, i] == numbersSorted[row, i].frameIndex)
					right++;
				else if((code[row, 0] == numbersSorted[row, i].frameIndex && code[row, 0] != numbersSorted[row, 0].frameIndex) ||
						(code[row, 1] == numbersSorted[row, i].frameIndex && code[row, 1] != numbersSorted[row, 1].frameIndex) ||
						(code[row, 2] == numbersSorted[row, i].frameIndex && code[row, 2] != numbersSorted[row, 2].frameIndex))
				semiright++;
			}
			int bad = 3 - right - semiright;
		
			for(int i=0; i<3; i++)
				if(bad >= 1+i) ledRows[row*3+i].frameIndex = 0;
			for(int i=0; i<3; i++)
				if(right >= 1+i) ledRows[row*3+2-i].frameIndex = 2;
			rightCount += right;
		}
		
		for(int col=0; col<3; col++)
		{
			int right = 0;
			int semiright = 0;
			for(int i=0; i<3; i++)
			{
				if(code[i, col] == numbersSorted[i, col].frameIndex)
					right++;
				else if((code[0, col] == numbersSorted[i, col].frameIndex && code[0, col] != numbersSorted[0, col].frameIndex) ||
						(code[1, col] == numbersSorted[i, col].frameIndex && code[1, col] != numbersSorted[1, col].frameIndex) ||
						(code[2, col] == numbersSorted[i, col].frameIndex && code[2, col] != numbersSorted[2, col].frameIndex))
				semiright++;
			}
			int bad = 3 - right - semiright;
		
			for(int i=0; i<3; i++)
				if(bad >= 1+i) ledCols[col*3+2-i].frameIndex = 0;
			for(int i=0; i<3; i++)
				if(right >= 1+i) ledCols[col*3+i].frameIndex = 2;
			rightCount += right;
		}
		
		if(rightCount == 18)
			ShowNext();
	}
	
	bool win = false; 
	void ShowNext()
	{
		win = true;
		iTween.MoveTo(nextCont.gameObject, iTween.Hash("x", 240, "ease", iTween.EaseType.easeOutBack, "time", 1.0f));
	}
	void HideNext()
	{
		win = false;
		iTween.MoveTo(nextCont.gameObject, iTween.Hash("x", 390, "ease", iTween.EaseType.easeOutBack, "time", 1.0f));
	}
	
	// Update is called once per frame
	void Update ()
	{	
		if(!win)
		{
			time += Time.deltaTime;
			timeLabel.text = "Time "+(int)time+"."+((int)(time*10)%10)+"s";
		}
		
		if(iTween.tweens.Count > 0)
		{
			clickedNumber = null;
			return;
		}
		
		if(OT.Clicked(nextButton))
		{
			RandomCode(currLevel);
			currLevel++;
			levelLabel.text = "Level "+currLevel;
			stepsLabel.text = "Steps: "+steps;
			HideNext();
			UpdateLed();
			return;
		}
		
		if(win)
			return;
		
		UpdateLed();
		
		if(!Input.GetMouseButton(0))
			clickedNumber = null;
		else if(clickedNumber != null)
		{
			foreach(OTSprite sprite in numbers)
			{
				if(OT.Over(sprite) && sprite != clickedNumber)
				{
					int row1=0, col1=0;
					int row2=0, col2=0;
					
					for(int i=0; i<9; i++)
					{	if(numbers[i] == sprite)
						{
							row1 = i/3; col1 = i%3;
							break;
						}
					}
					
					for(int i=0; i<9; i++)
					{	if(numbers[i] == clickedNumber)
						{
							row2 = i/3; col2 = i%3;
							break;
						}
					}
					
					if(Mathf.Abs(row1-row2) + Mathf.Abs(col1-col2) == 1)
					{
						if(row1 - row2 != 0)
						{
							if(row1 - row2 == -1)
							{
								int temp = numbersSorted[0, col1].frameIndex;
								numbersSorted[0, col1].frameIndex = numbersSorted[1, col1].frameIndex;
								numbersSorted[1, col1].frameIndex = numbersSorted[2, col1].frameIndex;
								numbersSorted[2, col1].frameIndex = temp;
								
								RollCode(numbersSorted[2, col1], numbersSorted[1, col1], numbersSorted[0, col1]);

							} else
							{
								int temp = numbersSorted[0, col1].frameIndex;
								numbersSorted[0, col1].frameIndex = numbersSorted[2, col1].frameIndex;
								numbersSorted[2, col1].frameIndex = numbersSorted[1, col1].frameIndex;
								numbersSorted[1, col1].frameIndex = temp;
								
								RollCode(numbersSorted[0, col1], numbersSorted[1, col1], numbersSorted[2, col1]);
							}
						} else 
						{
							if(col1 - col2 == -1)
							{
								int temp = numbersSorted[row1, 0].frameIndex;
								numbersSorted[row1, 0].frameIndex = numbersSorted[row1, 1].frameIndex;
								numbersSorted[row1, 1].frameIndex = numbersSorted[row1, 2].frameIndex;
								numbersSorted[row1, 2].frameIndex = temp;
								
								RollCode(numbersSorted[row1, 2], numbersSorted[row1, 1], numbersSorted[row1, 0]);
							} else
							{
								int temp = numbersSorted[row1, 0].frameIndex;
								numbersSorted[row1, 0].frameIndex = numbersSorted[row1, 2].frameIndex;
								numbersSorted[row1, 2].frameIndex = numbersSorted[row1, 1].frameIndex;
								numbersSorted[row1, 1].frameIndex = temp;
								
								RollCode(numbersSorted[row1, 0], numbersSorted[row1, 1], numbersSorted[row1, 2]);
							}	
						}
					}
					clickedNumber = null;
					break;
				}
			}
		}
		
		foreach(OTSprite sprite in numbers)
		{
			if(OT.Over(sprite) && Input.GetMouseButton(0))
			{
				clickedNumber = sprite;
			}
		}
	}
}
