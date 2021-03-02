using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Card : MonoBehaviour {
	[Header("Set Dynamically")]
	public string suit; // Suit of the Card (C,D,H, or S)
	public int rank; // Rank of the Card (1-14)
	public Color color = Color.black; // Color to tint pips
	public string colS = "Black";  // or "Red"

	public List<GameObject> decoGOs = new List<GameObject>();
	public List<GameObject> pipGOs = new List<GameObject>();

	public GameObject back;  // back of card;
	public CardDefinition def;  // from DeckXML.xml	
	
	public bool faceUp { 
		get {
			return (!back.activeSelf);
		}
		set {
			back.SetActive(!value);
		}
	}
}

[System.Serializable] // A Serializable class is able to be edited in the Inspector 
public class Decorator {
	// This class stores information about each decorator or pip from DeckXML
	public string type; // For card pips, type = 'pip"
	public Vector3 loc; // For location of the Sprite on the Card
	public bool flip = false; // Whether to flip the Sprite vertically
	public float scale = 1.0f; // The scale of the Sprite
}

[System.Serializable]
public class CardDefinition {
	// This class stores information for aech rank of card
	public string face; // Sprite to use for face cart
	public int rank;    // The rank (1-13) of this card, Value from 1-13 (Ace-King)
	public List<Decorator> pips = new List<Decorator>();  // Pips Used
}