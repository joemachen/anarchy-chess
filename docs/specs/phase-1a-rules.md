# Spec: Phase 1a, Chess rules engine

Status: **draft, waiting for Joe's approval**

## Goal
The rules library knows chess on a 6×6 board: how every piece moves, what counts as check, checkmate and stalemate, plus en passant and promotion. Everything in Phase 1 (scoring, enemy AI, the round) is built on this, so it gets tested hardest. Scoring and tactics are **not** part of this slice. They're 1b.

## What you'll be able to try (v0.0.2, the "rules sandbox")
A fixed 6×6 starting position appears on the board. You move **both** sides by hand: click a piece to see its legal moves as dots, then click a dot to move. The status line shows whose turn it is, plus "Check!", "Checkmate" or "Stalemate". A **Reset** button restarts the position.

This lets you test the rules by hand before any scoring sits on top. Moving both sides is temporary: the enemy AI replaces it in 1c. Everything else (drawing pieces, click to move, move dots) is kept and reused by the real game. Pieces are drawn as simple placeholder letters for now. Proper art comes later, using your zine-style mockups as the direction.

Starting position (uppercase = you, lowercase = enemy, `.` = empty):
```
6  r n b q k r
5  p p p p p p
4  . . . . . .
3  . . . . . .
2  P P P P P P
1  R N B Q K R
   a b c d e f
```

## Rules
1. **Board.** 6×6, files a–f, ranks 1–6. Your pieces start at the bottom. You always move first.
2. **Pieces move like standard chess:** king, queen, rook, bishop and knight. Their movement is defined in a data file (`game/content/pieces.json`), so a new piece is a data edit rather than new code. For example, a "Knook" is just the knight's and rook's patterns together.
   ```json
   { "id": "knight", "letter": "N", "value": 3, "moves": [ { "leap": [1, 2] } ] }
   ```
   `leap [1,2]` means "jump 1 one way and 2 the other, in every direction". `slide [1,0]` means "move any distance in a straight line until blocked", which is a rook. The piece values from DESIGN.md (pawn 1, knight 3, bishop 3, rook 5, queen 9) live in the same file, ready for 1b.
3. **Pawns** are special in code, the same as in real chess. They move 1 forward, capture 1 diagonally forward, and may move 2 forward from their starting rank (rank 2 for you, rank 5 for the enemy) if both squares are empty.
4. **En passant:** standard. If an enemy pawn moves 2 squares and lands beside your pawn, your pawn may capture it as if it had only moved 1. This is allowed **on the very next move only**.
5. **Promotion:** a pawn reaching the far rank becomes a queen automatically. Choosing the piece comes later, if we want it.
6. **Check:** a move is legal only if it leaves your own king safe. Illegal moves are simply never offered, which follows decision D5.
7. **Checkmate:** the side to move is in check and has no legal moves.
8. **Stalemate:** the side to move is **not** in check but has no legal moves. *Proposed:* that side passes its turn instead of the game ending in a draw. For you, a pass still uses up one of your round's moves. Real chess calls this a draw, but a draw makes no sense in a scoring round.
9. **Not included:** castling (pointless on random 6×6 setups, and DESIGN.md has "Castle Anywhere" as a Gambit later), and the 50-move and repetition draw rules (irrelevant with 10-move rounds).

## How it's tested
Positions in tests are written as small diagrams like the one above, so you can read them. Every rule gets its own tests, including the tricky cases:
- A pinned piece can't move off the line it's pinned along.
- A king can't step next to the enemy king.
- You can't move into check, including by moving the king along the line it's being checked on.
- En passant works only immediately, and not when doing it would expose your own king.
- Promotion works both by moving and by capturing.
- Checkmate and stalemate are detected correctly, and a stalemated side passes.
- Known move counts: in the starting position above, each side has exactly the number of legal moves we count by hand.

## Decisions for you
- **Stalemate = pass** (rule 8). Do you agree, or should stalemate end the round?
- **No castling** in Phase 1 (rule 9). OK?
- **Hand-playable sandbox (v0.0.2):** worth having, or would you rather wait for 1c, when the enemy plays itself?
