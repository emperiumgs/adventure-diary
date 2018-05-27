using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PuzzleMinigame : MonoBehaviour, IMinigameLoader
{
    public Transform solutionSocket,
        missingPieces;
    public Transform[] pivots;
    public GameObject[] boards;
    public Material[] puzzles;

    const int PUZZLE_PIECES = 9;

    MinigameController ctrl;
    MatchManager manager;
    MatchCanvas canvas;
    AudioController audioCtrl;
    GameObject board;
    Coroutine routine;
    Vector3 colliderSize = new Vector3(1, 0.07f, 1);
    Collider[] cols;
    bool done;
    int mask;

    void Awake()
    {
        mask = 1 << LayerMask.NameToLayer("Dice");
        ctrl = MinigameController.instance;
        manager = MatchManager.instance;
        canvas = MatchCanvas.instance;
        audioCtrl = AudioController.instance;
    }

    public void LoadMinigame()
    {
        done = false;
        StartCoroutine(PuzzleCreation());
    }

    public void Solution(PuzzlePiece piece)
    {
        if (routine != null)
            return;
        piece.Attach(solutionSocket.position);
        if (piece.solution)
        {
            audioCtrl.source.PlayOneShot(canvas.right);
            routine = StartCoroutine(EndPuzzle());
        }
        else
        {
            piece.Cancel();
            ctrl.ApplyPenalty();
        }
    }

    IEnumerator PuzzleCreation()
    {
        // Create Puzzle Board
        board = Instantiate(boards[Random.Range(0, boards.Length)]);
        board.transform.SetParent(transform, true);
        // Prepare piece positions
        List<Transform> collection = new List<Transform>(pivots.Length);
        collection.AddRange(pivots);
        // Create Puzzle pieces
        int index = Random.Range(0, puzzles.Length);
        Material mat = puzzles[index];
        List<MeshRenderer> boardPieces = new List<MeshRenderer>(PUZZLE_PIECES);
        boardPieces.AddRange(board.GetComponentsInChildren<MeshRenderer>());
        foreach (MeshRenderer r in boardPieces)
            r.material = mat;
        GameObject newObj;
        for (int i = 0; i < pivots.Length; i++)
        {
            index = Random.Range(0, boardPieces.Count);
            newObj = Instantiate(boardPieces[index].gameObject);
            newObj.transform.SetParent(missingPieces, true);
            newObj.transform.rotation = boardPieces[index].transform.rotation;
            newObj.AddComponent<BoxCollider>().size = colliderSize;
            newObj.AddComponent<PuzzlePiece>().solution = i == 0 ? true : false;
            newObj.GetComponent<PuzzlePiece>().puzzle = this;
            // Puzzle Solution
            if (i == 0)
            {
                solutionSocket.transform.position = boardPieces[index].transform.position + Vector3.up * .1f;
                Destroy(boardPieces[index].gameObject);
            }
            boardPieces.RemoveAt(index);
            index = Random.Range(0, collection.Count);
            newObj.transform.position = collection[index].position;
            collection.RemoveAt(index);
            yield return new WaitForFixedUpdate();
        }
        StartCoroutine(CheckSolution());
    }

    IEnumerator EndPuzzle()
    {
        yield return new WaitForSeconds(1);
        // Remove pieces
        MeshFilter[] pieces = missingPieces.GetComponentsInChildren<MeshFilter>();
        foreach (MeshFilter m in pieces)
            Destroy(m.gameObject);
        // Remove board
        Destroy(board.gameObject);
        manager.MinigameWon();
        ctrl.ConcludeMinigame();
        done = true;
        routine = null;
    }

    IEnumerator CheckSolution()
    {
        while (!done)
        {
            cols = Physics.OverlapSphere(solutionSocket.position, .1f, mask);
            if (cols.Length > 0)
            {
                PuzzlePiece piece = cols[0].GetComponentInParent<PuzzlePiece>();
                yield return new WaitForSeconds(.5f);
                Solution(piece);                
            }
            yield return new WaitForFixedUpdate();
        }
    }
}