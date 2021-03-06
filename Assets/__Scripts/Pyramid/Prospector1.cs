﻿using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;


public class Prospector1 : MonoBehaviour
{
    static public Prospector1 S;

    [Header("Set in Inspector")]
    public TextAsset deckXML;
    public TextAsset layoutXML;
    public float xOffset = 3;
    public float yOffset = -2.5f;
    public Vector3 layoutCenter;
    public Vector2 fsPosMid = new Vector2(0.5f, 0.90f);
    public Vector2 fsPosRun = new Vector2(0.5f, 0.75f);
    public Vector2 fsPosMid2 = new Vector2(0.4f, 1.0f);
    public Vector2 fsPosEnd = new Vector2(0.5f, 0.95f);
    public float reloadDelay = 1f; // The delay between rounds
    public Text gameOverText, roundResultText, highScoreText;

    [Header("Set Dynamically")]
    public Deck deck;
    public Layout layout;
    public List<CardProspector> drawPile;
    public Transform layoutAnchor;
    public CardProspector target;
    public List<CardProspector> table;
    public List<CardProspector> discardPile;
    public FloatingScore fsRun;

    private bool _selectionRoutineRunning = false;
    private List<CardProspector> selectedCards = new List<CardProspector>();
    public Text currentSelectionText;

    void Awake()
    {
        S = this;
        SetUpUITexts();
    }

    void SetUpUITexts()
    {
        // Set up the HighScore UI Text
        GameObject go = GameObject.Find("HighScore");
        if (go != null)
        {
            highScoreText = go.GetComponent<Text>();
        }
        int highScore = ScoreManager.HIGH_SCORE;
        string hScore = "High Score: " + Utils.AddCommasToNumber(highScore);
        go.GetComponent<Text>().text = hScore;

        // Set up the UI Texts that show at the end of the round 
        go = GameObject.Find("GameOver");
        if (go != null)
        {
            gameOverText = go.GetComponent<Text>();
        }

        go = GameObject.Find("RoundResult");
        if (go != null)
        {
            roundResultText = go.GetComponent<Text>();
        }

        // Make the end of round texts invisible 
        ShowResultsUI(false);
    }

    void ShowResultsUI(bool show)
    {
        gameOverText.gameObject.SetActive(show);
        roundResultText.gameObject.SetActive(show);
    }

    void Start()
    {
        Scoreboard.S.score = ScoreManager.SCORE;

        deck = GetComponent<Deck>();
        deck.InitDeck(deckXML.text);
        Deck.Shuffle(ref deck.cards); // This shuffles the deck by reference 

        /// Card c;
        /// for (int cNum=0; cNum<deck.cards.Count; cNum++) {
        ///	c = deck.cards[cNum];
        /// c.transform.localPosition = new Vector3((cNum % 13) * 3, cNum / 13 * 4, 0);

        layout = GetComponent<Layout>();   // Get the Layout component 
        layout.ReadLayout(layoutXML.text);  // Pass LayoutXML to it
        drawPile = ConvertListCardsToListCardProspectors(deck.cards);
        LayoutGame();
    }

    List<CardProspector> ConvertListCardsToListCardProspectors(List<Card> lCD)
    {
        List<CardProspector> lCP = new List<CardProspector>();
        CardProspector tCP;
        foreach (Card tCD in lCD)
        {
            tCP = tCD as CardProspector;
            lCP.Add(tCP);
        }
        return (lCP);
    }

    // The Draw function will pull a single card from the drawPile and return it 
    CardProspector Draw()
    {
        CardProspector cd = drawPile[0]; // Pull the 0th CardProspector
        drawPile.RemoveAt(0); // Then remove it from List<> drawPile
        return (cd); // And return it 
    }

    // LayoutGame() positions the intital tablaeu of cards, a.k.a. the "mine"
    void LayoutGame()
    {
        // Create an empty GameObject to serve as an anchor for the tableu 
        if (layoutAnchor == null)
        {
            GameObject tGO = new GameObject("_LayoutAnchor");
            // ^ Create an empty GameObject named _LayoutAnchor in the Hierarchy
            layoutAnchor = tGO.transform; // Grab its Transform
            layoutAnchor.transform.position = layoutCenter; // Position it 
        }

        CardProspector cp;
        // Follow the layout
        foreach (SlotDef tSD in layout.slotDefs)
        {
            // ^ Iterate through all the SlotDefs in the layout.slotDefs as tSD
            cp = Draw();
            cp.faceUp = tSD.faceUp;
            cp.transform.parent = layoutAnchor;
            cp.transform.localPosition = new Vector3(
                layout.multiplier.x * tSD.x,
                layout.multiplier.y * tSD.y,
                -tSD.layerID);
            cp.layoutID = tSD.id;
            cp.slotDef = tSD;
            cp.state = eCardState.tableau;
            cp.SetSortingLayerName(tSD.layerName); // Set the sorting layers

            table.Add(cp); // Add this CardProspector to the List<> table
        }

        // Set which cards are hiding others
        foreach (CardProspector tCP in table)
        {
            foreach (int hid in tCP.slotDef.hiddenBy)
            {
                cp = FindCardByLayoutID(hid);
                tCP.hiddenBy.Add(cp);
            }
        }

        // Set up the initial target card
        MoveToTarget(Draw());

        // Set up the Draw pile
        UpdateDrawPile();

    }

    // Convert from the layoutID int to the CardProspector with that ID
    CardProspector FindCardByLayoutID(int layoutID)
    {
        foreach (CardProspector tCP in table)
        {
            // Search through all cards in the table List<>
            if (tCP.layoutID == layoutID)
            {
                // If the card has the same ID, return it
                return (tCP);
            }
        }
        // If it's not found, return null
        return (null);
    }

    // This turns cards in the Mine face-up or face-down
    void SetTableauFaces()
    {
        foreach (CardProspector cd in table)
        {
            bool faceUp = true; // Assume the card will be face-up
            foreach (CardProspector cover in cd.hiddenBy)
            {
                // If either of the covering cards are in the table
                if (cover.state == eCardState.tableau)
                {
                    faceUp = false; // then this card is face-down
                }
            }
            cd.faceUp = faceUp; // Set the value on the card 
        }
    }

    // Moves the current target to the discardPile
    public void MoveToDiscard(CardProspector cd)
    {
        Debug.Log("Move to Discard");
        // Set the state of the card to discard
        cd.state = eCardState.discard;
        discardPile.Add(cd); // Add it to the discardPile List<>
        cd.transform.parent = layoutAnchor; // Update its transform parent

        // Position this card on the discardPile
        cd.transform.localPosition = new Vector3(
            layout.multiplier.x * layout.discardPile.x,
            layout.multiplier.y * layout.discardPile.y,
            -layout.discardPile.layerID + 0.5f);
        cd.faceUp = true;
        // Place it on top of te pile for depth sorting
        cd.SetSortingLayerName(layout.discardPile.layerName);
        cd.SetSortOrder(-100 + discardPile.Count);
    }

    // Make cd the new target card 
    void MoveToTarget(CardProspector cd)
    {
        // If there is currently a target card, movie it to discardPile
        if (target != null) MoveToDiscard(target);
        target = cd; // cd is te new target
        cd.state = eCardState.target;
        cd.transform.parent = layoutAnchor;

        // Move to the target position
        cd.transform.localPosition = new Vector3(
            layout.multiplier.x * layout.discardPile.x,
            layout.multiplier.y * layout.discardPile.y,
            -layout.discardPile.layerID);

        cd.faceUp = true; // Make it face-up
                          // Set the depth sorting
        cd.SetSortingLayerName(layout.discardPile.layerName);
        cd.SetSortOrder(0);
    }

    // Arranges all the cards of the drawPile to show how many are left
    void UpdateDrawPile()
    {
        CardProspector cd;
        // Go through all the cards of the drawPile
        for (int i = 0; i < drawPile.Count; i++)
        {
            cd = drawPile[i];
            cd.transform.parent = layoutAnchor;

            if (Random.Range(0, 100) < 10)
                drawPile[i].isGoldCard = true;

            // Position it correctly with the layout.drawPile.stagger
            Vector2 dpStagger = layout.drawPile.stagger;
            cd.transform.localPosition = new Vector3(
                layout.multiplier.x * (layout.drawPile.x + i * dpStagger.x),
                layout.multiplier.y * (layout.drawPile.y + i * dpStagger.y),
                -layout.drawPile.layerID + 0.1f * i);

            cd.faceUp = false; // Make them all face-down
            cd.state = eCardState.drawpile;
            // Set depth sorting
            cd.SetSortingLayerName(layout.drawPile.layerName);
            cd.SetSortOrder(-10 * i);
        }
    }

    // CardClicked is called any time a card in the game is clicked 
    public void CardClicked(CardProspector cd)
    {
        if (IsHidden(cd))
            return;

        // The reaction is determined by the state of the clicked card
        switch (cd.state)
        {
            case eCardState.target:
                if (cd.rank == 13)
                {
                    Debug.Log("eCardState.target");
                    table.Remove(cd);
                    MoveToDiscard(cd);
                }

                if (_selectionRoutineRunning) {
                    selectedCards.Add(cd);
                    Debug.Log("Selection Routine Running");
                }
                else
                {
                    Debug.Log("StartCortoutine");
                    StartCoroutine(CardSelectionRoutine());
                    selectedCards.Add(cd);
                }

                break;

            case eCardState.drawpile:
                // Clicking any card in the drawPile will draw the next card
                MoveToDiscard(target); // Moves the target to the discardPile
                MoveToTarget(Draw()); // Moves the next drawn card to the target
                UpdateDrawPile(); // Restacks the drawPile
                ScoreManager.EVENT(eScoreEvent.draw, false);
                FloatingScoreHandler(eScoreEvent.draw);
                break;

            case eCardState.tableau:
                if (cd.rank == 13)
                {
                    table.Remove(cd);
                    MoveToDiscard(cd);
                }

                if (_selectionRoutineRunning)
                    selectedCards.Add(cd);
                else
                {
                    StartCoroutine(CardSelectionRoutine());
                    selectedCards.Add(cd);
                }

                break;
        }
        // Check to see wheather the game is over or not
        CheckForGameOver();
    }

    private bool IsHidden(CardProspector card)
    {
        for (int i = 0; i < table.Count; i++)
            for (int j = 0; j < card.hiddenBy.Count; j++)
                if (card.hiddenBy[j].layoutID == table[i].layoutID)
                    return true;

        return false;
    }

    private IEnumerator CardSelectionRoutine()
    {
        _selectionRoutineRunning = true;
        selectedCards.Clear();

        while (selectedCards.Count < 2)
        {
            if (selectedCards.Count == 1)
            {
                currentSelectionText.text = "Currently selected card: " + selectedCards[0].gameObject.name;
                selectedCards[0].gameObject.GetComponent<SpriteRenderer>().color = Color.green;
            }

            yield return null;
        }

        if (selectedCards[0].rank + selectedCards[1].rank == 13)
        {
            StartCoroutine(ClearCardTextAfterDelay(selectedCards[0].gameObject.name + " and " + selectedCards[1].gameObject.name + " equal 13."));
            bool shouldDraw = selectedCards[0].state == eCardState.target || selectedCards[1].state == eCardState.target;

            table.Remove(selectedCards[0]);
            table.Remove(selectedCards[1]);

            MoveToDiscard(selectedCards[0]);
            MoveToDiscard(selectedCards[1]);

            Debug.Log($"card 0: {selectedCards[0].state} | card 1: {selectedCards[1].state}");
            if (shouldDraw)
            {
                Debug.Log("Automatically Drawing");
                MoveToTarget(Draw()); // Moves the next drawn card to the target
                UpdateDrawPile(); // Restacks the drawPile
                ScoreManager.EVENT(eScoreEvent.draw, false);
                FloatingScoreHandler(eScoreEvent.draw);
            }
        }
        else
        {
            StartCoroutine(ClearCardTextAfterDelay("The selection equals " + (selectedCards[0].rank + selectedCards[1].rank).ToString()));
            StartCoroutine(FlashCardsRoutine(Color.red, 3, .5f, selectedCards[0], selectedCards[1]));
        }

        selectedCards[0].gameObject.GetComponent<SpriteRenderer>().color = Color.white;

        selectedCards.Clear();

        //currentSelectionText.text = "";
        _selectionRoutineRunning = false;

        //CHECK SET TABLEU FACES FOR POSSIBLE AVAILABLE CHECK
    }

    private IEnumerator FlashCardsRoutine(Color color, int cycles, float duration, CardProspector card1, CardProspector card2)
    {
        int currentCycle = cycles;
        while (currentCycle > 0)
        {
            card1.gameObject.GetComponent<SpriteRenderer>().color = Color.red;
            card2.gameObject.GetComponent<SpriteRenderer>().color = Color.red;
            yield return new WaitForSeconds(.2f);
            card1.gameObject.GetComponent<SpriteRenderer>().color = Color.white;
            card2.gameObject.GetComponent<SpriteRenderer>().color = Color.white;
            yield return new WaitForSeconds(.2f);
            currentCycle--;
        }
    }

    private IEnumerator ClearCardTextAfterDelay(string message)
    {
        currentSelectionText.text = message;
        yield return new WaitForSeconds(2);
        currentSelectionText.text = "";
    }
    // Test wheather the game is over
    void CheckForGameOver()
    {
        // If the table is empty, the game is over
        if (table.Count == 0)
        {
            // Call GameOveer() with a win
            GameOver(true);
            return;
        }

        // If there are still cards in the draw pile, the game's not over
        if (drawPile.Count > 0)
        {
            return;
        }

        // Check for remaining valid plays
        foreach (CardProspector cd in table)
        {
            if (AdjacentRank(cd, target))
            {
                // If there is a valid play, the game's not over
                return;
            }
        }

        // Since there are no valid plays, the game is over
        // Call GameOver with a loss
        GameOver(false);
    }

    // Called when the game is over. Simple for now, but expandable
    void GameOver(bool won)
    {
        int score = ScoreManager.SCORE;
        if (fsRun != null) score += fsRun.score;
        if (won)
        {
            gameOverText.text = "Round Over";
            roundResultText.text = "You won this round! \nRound Score: " + score;
            ShowResultsUI(true);
            // print("Game Over. You won! :)"); // Comment out this line
            ScoreManager.EVENT(eScoreEvent.gameWin, false);
            FloatingScoreHandler(eScoreEvent.gameWin);
        }
        else
        {
            gameOverText.text = "Game Over";
            if (ScoreManager.HIGH_SCORE <= score)
            {
                string str = "You got the high score!\nHigh score: " + score;
                roundResultText.text = str;
            }
            else
            {
                roundResultText.text = "Your final score was: " + score;
            }
            ShowResultsUI(true);
            // print("Game Over. You Lost. :("); // Comment out this line
            ScoreManager.EVENT(eScoreEvent.gameLoss, false);
            FloatingScoreHandler(eScoreEvent.gameLoss);
        }
        // Reload the scene, resetting the game
        // SceneManager.LoadScene("__Prospector_Scene_0"); // Now commented out!

        // Reload the scene in reloadDelay seconds
        // This will give the score a moment to travel
        Invoke("ReloadLevel", reloadDelay);
    }

    void ReloadLevel()
    {
        // Reload the scene, resetting the game
        SceneManager.LoadScene("__Pyramid_Scene_0"); // Now commented out!
    }

    // Return true if the two cards are adjacent in rank (A & K wrap around)
    public bool AdjacentRank(CardProspector c0, CardProspector c1)
    {
        // If either card is face-down, it's not adjacent.
        if (!c0.faceUp || !c1.faceUp) return (false);

        // If they are 1 apart, they are adjacent 
        if (Mathf.Abs(c0.rank - c1.rank) == 1)
        {
            return (true);
        }
        // If one is Ace and the other King, they are adjacent 
        if (c0.rank == 1 && c1.rank == 13) return (true);
        if (c0.rank == 13 && c1.rank == 1) return (true);

        // Otherwse, return false
        return (false);
    }

    // Hnadle FloatingScore movememnt 
    void FloatingScoreHandler(eScoreEvent evt)
    {
        List<Vector2> fsPts;
        switch (evt)
        {
            // Same things need to happen whether it's a draw, a win, or a loss
            case eScoreEvent.draw: // Drawing a card
            case eScoreEvent.gameWin: // Won the round 
            case eScoreEvent.gameLoss: // Lost the round
                                       // Add fsRun to the Scoreboard score
                if (fsRun != null)
                {
                    // Create points for the Bezier Curve
                    fsPts = new List<Vector2>();
                    fsPts.Add(fsPosRun);
                    fsPts.Add(fsPosMid2);
                    fsPts.Add(fsPosEnd);
                    fsRun.reportFinishTo = Scoreboard.S.gameObject;
                    fsRun.Init(fsPts, 0, 1);
                    // Also adjust the fontSize
                    fsRun.fontSizes = new List<float>(new float[] { 28, 36, 4 });
                    fsRun = null; // Clear fsRun so it's created again
                }
                break;

            case eScoreEvent.mine: // Remove a mine card
                                   // Create a FloatingScore for this score 
                FloatingScore fs;
                // Move it from the mousePosition to fsPosRun
                Vector2 p0 = Input.mousePosition;
                p0.x /= Screen.width;
                p0.y /= Screen.height;
                fsPts = new List<Vector2>();
                fsPts.Add(p0);
                fsPts.Add(fsPosMid);
                fsPts.Add(fsPosRun);
                fs = Scoreboard.S.CreateFloatingScore(ScoreManager.CHAIN, fsPts);
                fs.fontSizes = new List<float>(new float[] { 4, 50, 28 });
                if (fsRun == null)
                {
                    fsRun = fs;
                    fsRun.reportFinishTo = null;
                }
                else
                {
                    fs.reportFinishTo = fsRun.gameObject;
                }
                break;
        }
    }

}