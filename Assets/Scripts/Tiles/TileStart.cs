using UnityEngine;
using System.Collections;

public class TileStart : Tile
{
    public Transform[] layouts;

    public override void Action()
    {
        Player[] players = manager.players;
        int count = players.Length;
        Transform chosenLayout = layouts[count - 1],
            player;
        Vector3 adjust = Vector3.up * 0.1f;
        for (int i = 0; i < count; i++)
        {
            player = players[i].obj.transform;
            player.SetParent(manager.playersSocket);
            player.position = chosenLayout.GetChild(i).position + adjust;
            player.rotation = chosenLayout.GetChild(i).rotation;
        }
    }
}