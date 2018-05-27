using UnityEngine;
using System.Collections;
using System.Linq;

public class TileEnd : Tile
{
    int[] scores = { 250, 150, 100, 50 };

    public override void Action()
    {
        // Get players ordered by closest position to the end and assing their scores
        Player[] players = manager.players.OrderByDescending(p => p.curPage).ThenByDescending(p => p.curPageTile).ToArray();
        int i;
        for (i = 0; i < players.Length; i++)
            players[i].score += scores[i];
        // Assign scores back to the original players
        bool relay;
        int j = 0;
        for (i = 0; i < players.Length; i++)
        {
            relay = false;
            j = 0;
            while (!relay && j < players.Length)
            {
                if (manager.players[i].name == players[j].name)
                {
                    manager.players[i].score = players[j].score;
                    relay = true;
                }
                j++;
            }
        }
        manager.StopAllCoroutines();
        canvas.StopAllCoroutines();        
        canvas.StartCoroutine(canvas.LoadScoreboard());
    }
}