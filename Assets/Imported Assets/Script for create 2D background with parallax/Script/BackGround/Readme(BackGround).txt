
Create an empty object, add a script BackGround to it. In the inspector, in the variable player, add the object behind which the backgrounds will follow, if the game without moving backgrounds, create and add an empty object. To a variable List<BackGroundLayer> set the number of background layers. Next you specify such parameters as mySprite add a background layer that you need to it. Variable parallaxSpeedX and parallaxSpeedY are responsible for the movement relative to the character, if the value 1 is to move behind the pesonage, if less than 1 when moving, they start to lag behind it, if more than they overtake. Next you need to create a layer on which will be placed, let's say the BackGround. In the field layerName we write the name of the layer  "BackGround" and in orderInLayer write rder In Layer change in SpriteRenderer. Variables  backGroundSpeedY and backGroundSpeedX are responsible for the movement of the background along the X and Y axes, regardless of the variable player, for example, the cloud. If you leave 0, the backgrounds will not move.

C# script BackGround contain:
- variable player type GameObject (The background position changes with respect to this variable. This should be the main character of the game)
- variable List<BackGroundLayer> size - Number of background layers
- variable bool nineImage - We put the true, if we need to create cyclical parallax as X and Y
	- class BackGroundLayer contain:
		    public Sprite mySprite - fon image;
			public float moveSpeedX - The coefficient relative to which the background position on the X coordinate changes when the player's position changes
			public float moveSpeedY - The coefficient relative to which the background position on the Y coordinate changes when the player's position changes
			public string layerName - SortingLayer change in SpriteRenderer
			public int orderInLayer - order In Layer change in SpriteRenderer
			public float backGroundSpeedX - The variable is responsible for moving the background along the X axis independing on the variable player
			public float backGroundSpeedY - The variable is responsible for moving the background along the Y axis independing on the variable player
- Button Create BackGround - Creates the BackGround
- Button Delete BackGround - Removes BackGround

- Method CreateBackGround() - Calls the method Init(newObject, i, j, y)
- Method DeleteBackGround() - Removes BackGround.
- Method Update() - Calculates changes in the background position from the player and cycles the background.
- Method CheckPosition(GameObject myObject, int j) - Check the position of the background and transfer it if it is far away
- Method Init(GameObject newObject, int i, int j, int y) - Responsible for creating backgrounds on stage