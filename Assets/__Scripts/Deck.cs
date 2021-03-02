using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Deck : MonoBehaviour {

	[Header("Set in Inspector")]
	// Suits
	public Sprite suitClub;
	public Sprite suitDiamond;
	public Sprite suitHeart;
	public Sprite suitSpade;

	public Sprite[] faceSprite;
	public Sprite[] rankSprite;

	public Sprite cardBack;
	public Sprite cardBackGold;
	public Sprite cardFront;
	public Sprite cardFrontGold;

	// Prefabs
	public GameObject prefabCard;
	public GameObject prefabSprite;

	[Header("Set Dynamically")]
	public PT_XMLReader xmlr;
	public List<string> cardNames;
	public List<Card> cards;
	public List<Decorator> decorators;
	public List<CardDefinition> cardDefs;
	public Transform deckAnchor;
	public Dictionary<string, Sprite> dictSuits;

	// InitDeck is called by Prospector when it is ready
	public void InitDeck(string deckXMLTest) {
		ReadDeck(deckXMLTest);
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
		for (int i = 0; i < xDecos.Count; i++) {
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
		for (int i = 0; i < xCardDefs.Count; i++) {
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
} 