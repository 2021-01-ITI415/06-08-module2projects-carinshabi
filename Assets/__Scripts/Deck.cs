using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Deck : MonoBehaviour {
	[Header("Set in Inspector")]
	public bool startFaceUp = false;
	// Suits
	public Sprite suitClub;
	public Sprite suitDiamond;
	public Sprite suitHeart;
	public Sprite suitSpade;

	public Sprite[] faceSprites;
	public Sprite[] rankSprites;

	public Sprite cardBack;
	public Sprite cardBackGold;
	public Sprite cardFront;
	public Sprite cardFrontGold;

	// Prefabs
	public GameObject prefabCard;
	public GameObject prefabSprite;


[Header("Set in Inspector")]
	//Suits
	public Sprite suitClub;
	public Sprite suitDiamond;
	public Sprite suitHeart;
	public Sprite suitSpade;
	
	public Sprite[] faceSprites;
	public Sprite[] rankSprites;
	
	public Sprite cardBack;
	public Sprite cardBackGold;
	public Sprite cardFront;
	public Sprite cardFrontGold;
	
	
	// Prefabs
	public GameObject prefabSprite;
	public GameObject prefabCard;

	[Header("Set Dynamically")]
<<<<<<< HEAD
	public PT_XMLReader xmlr;
	public List<string> cardNames;
	public List<Card> cards;
	public List<Decorator> decorators;
	public List<CardDefinition> cardDefs;
	public Transform deckAnchor;
	public Dictionary<string, Sprite> dictSuits;

	// InitDeck is called by Prospector when it is ready
	public void InitDeck(string deckXMLText) {
		if (GameObject.Find("_Deck") == null) {
			GameObject anchorGO = new GameObject("_Deck");
			deckAnchor = anchorGO.transform;
		}

		// Init the Dictionary of suits
		dictSuits = new Dictionary<string, Sprite>() {
			{"C", suitClub},
			{"D", suitDiamond},
			{"H", suitHeart},
			{"S", suitSpade}
		};

		// -------- end from page 576
		ReadDeck(deckXMLText);
		// This is the preexisting line from earlier
		MakeCards();
	}

	// ReadDeck parses the XML file passed to it into CardDefinitions
	public void ReadDeck(string deckXMLText) {
		xmlr = new PT_XMLReader(); // Create a new PT_XMLReader
		xmlr.Parse(deckXMLText); // Use that PT_XMLReader to parse DeckXML

		// The following prints a test line to show how xmlr can be used. 
		// For more information read about XML in the Useful Concepts Appendix
		string s = "xml[0] decorator[0] ";
		s += "type=" + xmlr.xml["xml"][0]["decorator"][0].att("type");
		s += "x=" + xmlr.xml["xml"][0]["decorator"][0].att("x");
		s += "y=" + xmlr.xml["xml"][0]["decorator"][0].att("y");
		s += "scale=" + xmlr.xml["xml"][0]["decorator"][0].att("scale");
		// print(s); // Comment out this line, since we're done with the test

		//Read decorators for all cards
		decorators = new List<Decorator>(); // Init the List of Decorators
		// Grab an PT_XMLHashList of all <decorator>s in the XML file
		PT_XMLHashList xDecos = xmlr.xml["xml"][0]["decorator"];
		Decorator deco;
		for (int i = 0; i <xDecos.Count; i++) {
			// For each <decorator> in the XML
			deco = new Decorator(); // Make a new Decorator
			// Copy attributes and set up location and flip if needed
			deco.type = xDecos[i].att("type");
			// bool deco.flip is true if the text of the flip attribute is "1"
			deco.flip = (xDecos[i].att("flip") == "1");   // too cute by half - if it's 1, set to 1, else set to 0
			// floats need to be parsed from the attribute strings 
			deco.scale = float.Parse(xDecos[i].att("scale"));
			// Vector3 loc initializes to [0,0,0], so we just need to modify it
			deco.loc.x = float.Parse(xDecos[i].att("x"));
			deco.loc.y = float.Parse(xDecos[i].att("y"));
			deco.loc.z = float.Parse(xDecos[i].att("z"));
			// Add the temporary deco to the List decorators
			decorators.Add(deco);
		}

		// Read pip locations for each card number
		cardDefs = new List<CardDefinition>(); // Init the List of Cards
		// Grab an PT_XMLHashList of all the  <card>s in the XML file
		PT_XMLHashList xCardDefs = xmlr.xml["xml"][0]["card"];
		for (int i = 0; i <xCardDefs.Count; i++) {
			// For each carddef in the XML, copy attributes and set up in cDef
			// Create a new CardDefinition
			CardDefinition cDef = new CardDefinition();
			// Parse the attribute values and add them to cDef
			cDef.rank = int.Parse(xCardDefs[i].att("rank"));
			// Grab an PT_XMLHashList of all the <pip>s on this <card>
			PT_XMLHashList xPips = xCardDefs[i]["pip"];
			if (xPips != null) {
				for (int j = 0; j < xPips.Count; j++) {
					// Iterate through all the <pip>s
					deco = new Decorator();
					// <pip>s on the <card> are handled via the Decorator Class
					deco.type = "pip";
					deco.flip = (xPips[j].att("flip") == "1");   // too cute by half - if it's 1, set to 1, else set to 0
					deco.loc.x = float.Parse(xPips[j].att("x"));
					deco.loc.y = float.Parse(xPips[j].att("y"));
					deco.loc.z = float.Parse(xPips[j].att("z"));
					if (xPips[j].HasAtt("scale")) {
						deco.scale = float.Parse(xPips[j].att("scale"));
					}
					cDef.pips.Add(deco);
				}

			}

			// Face cards (Jack, Queen, & King) have a face attribute
			if (xCardDefs[i].HasAtt("face")) {
				cDef.face = xCardDefs[i].att("face");
			}
			cardDefs.Add(cDef);
		}
	}

	// Get the proper CardDefintion based on Rank (1 to 14 is Ace to Kind)
	public CardDefinition GetCardDefinitionByRank(int rnk) {
		foreach (CardDefinition cd in cardDefs) {
			if (cd.rank == rnk) {
				return (cd);
			}
		} 
		return (null);
	}

	public void MakeCards() {
		// cardNames will be the names of cards build
		// Each suit goes from 2 to 14 (e.g., C1 to C14 for Clubs)
		// stub Add the code from page 577 here
		cardNames = new List<string>();
		string[] letters = new string[] { "C", "D", "H", "S" };
		foreach (string s in letters) {
			for (int i = 0; i < 13; i++) {
				cardNames.Add(s + (i + 1));
			}
		}

		// Make a List to hold all the Cards
		cards = new List<Card>();

		// Iterate through all of the card names that were just made
		for (int i = 0; i<cardNames.Count; i++) {
			// Make the card and add it to the cards Deck 
			cards.Add(MakeCard(i));
		}
	}

	private Card MakeCard(int cNum) {
		// Create a new Card GameObject
		GameObject cgo = Instantiate(prefabCard) as GameObject;
		// Set the transform.parent of the new card to the anchor.
		cgo.transform.parent = deckAnchor;
		Card card = cgo.GetComponent<Card>(); // Get the Card Component

		// This line stacks the cards so that they're all in nice rows
		cgo.transform.localPosition = new Vector3((cNum%13) *3, cNum /13*4, 0);

		// Assign basic values to the Card
		card.name = cardNames[cNum];
		card.suit = card.name[0].ToString();
		card.rank = int.Parse(card.name.Substring(1));
		if (card.suit == "D" || card.suit == "H") {
			card.colS = "Red";
			card.color = Color.red;
		}

		// Pull the CardDefintiion for this card
		card.def = GetCardDefinitionByRank(card.rank);

		AddDecorators(card);
		AddPips(card);
		AddFace(card);
		AddBack(card);

		return card;
	}

	// These private variables will be reused several times in helper methods
	private Sprite _tSp = null;
	private GameObject _tGO = null;
	private SpriteRenderer _tSR = null;

	private void AddDecorators(Card card) {
		// Add Decorators
		foreach (Decorator deco in decorators) {
			if (deco.type == "suit") {
				_tGO = Instantiate(prefabSprite) as GameObject;
				_tSR = _tGO.GetComponent<SpriteRenderer>();
				_tSR.sprite = dictSuits[card.suit];
			} else { // it is a rank
				_tGO = Instantiate(prefabSprite) as GameObject;
				_tSR = _tGO.GetComponent<SpriteRenderer>();
				_tSp = rankSprites[card.rank]; 
				_tSR.sprite = _tSp;
				_tSR.color = card.color;
			}

			// Make the deco Sprites render above the Card
			_tSR.sortingOrder = 1;
			// Make the decorator Sprite a child of the Card
			_tGO.transform.SetParent(card.transform);
			// Set the localPosition based on the location from DeckXML
			_tGO.transform.localPosition = deco.loc;
			// Flip the decorator if needed
			if (deco.flip) {
				// An Euler rotation of 180 degrees around the Z-axiz will flip it 
				_tGO.transform.rotation = Quaternion.Euler(0, 0, 180);
			}
			// Set the scale to keep decos from being too big 
			if (deco.scale != 1) {
				_tGO.transform.localScale = Vector3.one * deco.scale;
			}
			//Name this GameObject so it's easy to see
			_tGO.name = deco.type;
			// Add this deco GameObject to the List card.decoGos
			card.decoGOs.Add(_tGO);
		}
	}

	private void AddPips(Card card) { 
		// For each of the pips in the definition...
		foreach( Decorator pip in card.def.pips ) {
			// ... Instantiate a Sprite GameObject
			_tGO = Instantiate(prefabSprite) as GameObject;
			// Set the parent to be the card GameObject
			_tGO.transform.SetParent(card.transform);
			// Set the position to the specified in the XML
			_tGO.transform.localPosition = pip.loc;
			//Flip it if necessary
			if (pip.flip) {
				_tGO.transform.rotation = Quaternion.Euler(0, 0, 180);
			}
			// Scale it if necessary (only for the Ace)
			if (pip.scale !=1) {
				_tGO.transform.localScale = Vector3.one * pip.scale;
			}
			// Give this GameObject a name 
			_tGO.name = "pip";
			// Get the SpriteRenderer Component 
			_tSR = _tGO.GetComponent<SpriteRenderer>();
			// Set the Sprite to the proper suit 
			_tSR.sprite = dictSuits[card.suit];
			// Set sortingOrder so the pip is rendered above the Card_Front
			_tSR.sortingOrder = 1;
			// Add this to the Card's list of pips
			card.pipGOs.Add(_tGO);
		}
	}

	private void AddFace(Card card) { 
		if (card.def.face == "") {
			return; // No need to run if this isn't a face card
		}

		_tGO = Instantiate(prefabSprite) as GameObject;
		_tSR = _tGO.GetComponent<SpriteRenderer>();
		// Generate the right name and pass it to GetFace()
		_tSp = GetFace(card.def.face + card.suit);
		_tSR.sprite = _tSp;  // Assign this Sprite to _tSR
		_tSR.sortingOrder = 1; // Set the sortingOrder
		_tGO.transform.SetParent(card.transform);
		_tGO.transform.localPosition = Vector3.zero;
		_tGO.name = "face";
	}

	// Find the proper face card Sprite
	private Sprite GetFace(string faceS) { 
		foreach (Sprite _tSP in faceSprites) { 
			// If this Sprite has the right name...
			if (_tSP.name == faceS) {
				// ... then return the Sprite
				return (_tSP);
			}
		}
		// If nothing can be found, return null
		return (null);
	}

	private void AddBack(Card card) {
		// Add Card Back
		// The Card_Back will be able to cover everything else on the Card
		_tGO = Instantiate(prefabSprite) as GameObject;
		_tSR = _tGO.GetComponent<SpriteRenderer>();
		_tSR.sprite = cardBack;
		_tGO.transform.SetParent(card.transform);
		_tGO.transform.localPosition = Vector3.zero;
		// This is a higher sortingOrder than anything else
		_tSR.sortingOrder = 2;
		_tGO.name = "back";
		card.back = _tGO;
		// Default to face-up
		card.faceUp = startFaceUp; // Use the property faceUp of Card
	}
	// ReadDeck parses the XML file passed to it into Card Definitions
	public void ReadDeck(string deckXMLText)
	{
		xmlr = new PT_XMLReader ();
		xmlr.Parse (deckXMLText);

		// print a test line
		string s = "xml[0] decorator [0] ";
		s += "type=" + xmlr.xml ["xml"] [0] ["decorator"] [0].att ("type");
		s += " x=" + xmlr.xml ["xml"] [0] ["decorator"] [0].att ("x");
		s += " y=" + xmlr.xml ["xml"] [0] ["decorator"] [0].att ("y");
		s += " scale=" + xmlr.xml ["xml"] [0] ["decorator"] [0].att ("scale");
		print (s);
		
		//Read decorators for all cards
		// these are the small numbers/suits in the corners
		decorators = new List<Decorator>();
		// grab all decorators from the XML file
		PT_XMLHashList xDecos = xmlr.xml["xml"][0]["decorator"];
		Decorator deco;
		for (int i=0; i<xDecos.Count; i++) {
			// for each decorator in the XML, copy attributes and set up location and flip if needed
			deco = new Decorator();
			deco.type = xDecos[i].att ("type");
			deco.flip = (xDecos[i].att ("flip") == "1");   // too cute by half - if it's 1, set to 1, else set to 0
			deco.scale = float.Parse (xDecos[i].att("scale"));
			deco.loc.x = float.Parse (xDecos[i].att("x"));
			deco.loc.y = float.Parse (xDecos[i].att("y"));
			deco.loc.z = float.Parse (xDecos[i].att("z"));
			decorators.Add (deco);
		}
		
		// read pip locations for each card rank
		// read the card definitions, parse attribute values for pips
		cardDefs = new List<CardDefinition>();
		PT_XMLHashList xCardDefs = xmlr.xml["xml"][0]["card"];
		
		for (int i=0; i<xCardDefs.Count; i++) {
			// for each carddef in the XML, copy attributes and set up in cDef
			CardDefinition cDef = new CardDefinition();
			cDef.rank = int.Parse(xCardDefs[i].att("rank"));
			
			PT_XMLHashList xPips = xCardDefs[i]["pip"];
			if (xPips != null) {			
				for (int j = 0; j < xPips.Count; j++) {
					deco = new Decorator();
					deco.type = "pip";
					deco.flip = (xPips[j].att ("flip") == "1");   // too cute by half - if it's 1, set to 1, else set to 0
					
					deco.loc.x = float.Parse (xPips[j].att("x"));
					deco.loc.y = float.Parse (xPips[j].att("y"));
					deco.loc.z = float.Parse (xPips[j].att("z"));
					if(xPips[j].HasAtt("scale") ) {
						deco.scale = float.Parse (xPips[j].att("scale"));
					}
					cDef.pips.Add (deco);
				} // for j
			}// if xPips
			
			// if it's a face card, map the proper sprite
			// foramt is ##A, where ## in 11, 12, 13 and A is letter indicating suit
			if (xCardDefs[i].HasAtt("face")){
				cDef.face = xCardDefs[i].att ("face");
			}
			cardDefs.Add (cDef);
		} // for i < xCardDefs.Count
	} // ReadDeck
	
	public CardDefinition GetCardDefinitionByRank(int rnk) {
		foreach(CardDefinition cd in cardDefs) {
			if (cd.rank == rnk) {
					return(cd);
			}
		} // foreach
		return (null);
	}//GetCardDefinitionByRank
	
	
	public void MakeCards() {
		// stub Add the code from page 577 here
		cardNames = new List<string>();
		string[] letters = new string[] {"C","D","H","S"};
		foreach (string s in letters) {
			for (int i =0; i<13; i++) {
				cardNames.Add(s+(i+1));
			}
		}
		
		// list of all Cards
		cards = new List<Card>();
		
		// temp variables
		Sprite tS = null;
		GameObject tGO = null;
		SpriteRenderer tSR = null;  // so tempted to make a D&D ref here...
		
		for (int i=0; i<cardNames.Count; i++) {
			GameObject cgo = Instantiate(prefabCard) as GameObject;
			cgo.transform.parent = deckAnchor;
			Card card = cgo.GetComponent<Card>();
			
			cgo.transform.localPosition = new Vector3(i%13*3, i/13*4, 0);
			
			card.name = cardNames[i];
			card.suit = card.name[0].ToString();
			card.rank = int.Parse (card.name.Substring (1));
			
			if (card.suit =="D" || card.suit == "H") {
				card.colS = "Red";
				card.color = Color.red;
			}
			
			card.def = GetCardDefinitionByRank(card.rank);
			
			// Add Decorators
			foreach (Decorator deco in decorators) {
				tGO = Instantiate(prefabSprite) as GameObject;
				tSR = tGO.GetComponent<SpriteRenderer>();
				if (deco.type == "suit") {
					tSR.sprite = dictSuits[card.suit];
				} else { // it is a rank
					tS = rankSprites[card.rank];
					tSR.sprite = tS;
					tSR.color = card.color;
				}
				
				tSR.sortingOrder = 1;                     // make it render above card
				tGO.transform.parent = cgo.transform;     // make deco a child of card GO
				tGO.transform.localPosition = deco.loc;   // set the deco's local position
				
				if (deco.flip) {
					tGO.transform.rotation = Quaternion.Euler(0,0,180);
				}
				
				if (deco.scale != 1) {
					tGO.transform.localScale = Vector3.one * deco.scale;
				}
				
				tGO.name = deco.type;
				
				card.decoGOs.Add (tGO);
			} // foreach Deco
			
			
			//Add the pips
			foreach(Decorator pip in card.def.pips) {
				tGO = Instantiate(prefabSprite) as GameObject;
				tGO.transform.parent = cgo.transform; 
				tGO.transform.localPosition = pip.loc;
				
				if (pip.flip) {
					tGO.transform.rotation = Quaternion.Euler(0,0,180);
				}
				
				if (pip.scale != 1) {
					tGO.transform.localScale = Vector3.one * pip.scale;
				}
				
				tGO.name = "pip";
				tSR = tGO.GetComponent<SpriteRenderer>();
				tSR.sprite = dictSuits[card.suit];
				tSR.sortingOrder = 1;
				card.pipGOs.Add (tGO);
			}
			
			//Handle face cards
			if (card.def.face != "") {
				tGO = Instantiate(prefabSprite) as GameObject;
				tSR = tGO.GetComponent<SpriteRenderer>();
				
				tS = GetFace(card.def.face+card.suit);
				tSR.sprite = tS;
				tSR.sortingOrder = 1;
				tGO.transform.parent=card.transform;
				tGO.transform.localPosition = Vector3.zero;  // slap it smack dab in the middle
				tGO.name = "face";
			}

			tGO = Instantiate(prefabSprite) as GameObject;
			tSR = tGO.GetComponent<SpriteRenderer>();
			tSR.sprite = cardBack;
			tGO.transform.SetParent(card.transform);
			tGO.transform.localPosition=Vector3.zero;
			tSR.sortingOrder = 2;
			tGO.name = "back";
			card.back = tGO;
			card.faceUp = false;
			
			cards.Add (card);
		} // for all the Cardnames	
	} // makeCards
	
	//Find the proper face card
	public Sprite GetFace(string faceS) {
		foreach (Sprite tS in faceSprites) {
			if (tS.name == faceS) {
				return (tS);
			}
		}//foreach	
		return (null);  // couldn't find the sprite (should never reach this line)
	 }// getFace 

	 static public void Shuffle(ref List<Card> oCards)
	 {
	 	List<Card> tCards = new List<Card>();

	 	int ndx;   // which card to move

	 	while (oCards.Count > 0) 
	 	{
	 		// find a random card, add it to shuffled list and remove from original deck
	 		ndx = Random.Range(0,oCards.Count);
	 		tCards.Add(oCards[ndx]);
	 		oCards.RemoveAt(ndx);
	 	}

	 	oCards = tCards;

	 	//because oCards is a ref parameter, the changes made are propogated back
	 	//for ref paramters changes made in the function persist.


	 }


} // Deck class
