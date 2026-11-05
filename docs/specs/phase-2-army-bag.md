# Spec: Phase 2, The army bag

Status: **draft, waiting for Joe's approval**

## Goal
Phase 1 proved that forks are fun. Phase 2 adds the "what did I draw?" tension from DESIGN.md: instead of a fixed army placed at random, you draw a hand from a bag and choose where your pieces go.

**The question this phase answers:** does setup feel like a fun puzzle, and do different draws make rounds feel different?

It comes in two builds, so you can test setup before drops land on top of it:
- **v0.1.1:** the bag, your hand, and free setup
- **v0.2.0:** paid drops during the round (the Phase 2 gate)

## The bag and your hand (numbers in `game/content/rounds.json`)
1. **Your bag** holds 10 pieces: 5 pawns, 2 knights, 2 bishops and 1 rook. There's still no queen, for the same reason as Phase 1: she'd do everything herself.
2. **Each round you draw 6** at random from the full bag. The draw comes from the seed, so "seed 48213, round 3" always gives the same hand.
3. **The bag refills between rounds.** Pieces you lost come back. Keeping or changing your bag is a Phase 3 thing, when the shop arrives.

## Free setup (build v0.1.1)
1. The round opens in **setup**. The enemy army is already on the board, so you can plan around it. Your king is already placed by the seed, on rank 1.
2. Your 6 drawn pieces wait in a tray beside the board. Click a piece in the tray, then click a highlighted square to place it. Click a placed piece to pick it back up. Nothing is final until you start.
3. **You place up to 4 pieces for free**, on ranks 1–2. The rest stay in your tray as **reserves**.
4. **Pawns can only go on rank 2**, so they keep their double step.
5. **You can't place a piece where it would check the enemy king,** because then you could capture the king on your first move. Those squares simply aren't offered.
6. Click **Start round** to begin. The move clock doesn't run during setup, and placing pieces scores nothing.
7. You need at least 1 piece placed before you can start.
8. **Start round is blocked while the enemy king is in check.** Picking up a piece can open a line for one behind it, so this needs its own rule. A short message tells you why.

**Guides during setup** (same G toggle): squares the enemy attacks show **UNSAFE**, and clicking a placed piece shows dots where it could move once the round starts. You shouldn't need chess knowledge to place sensibly.

## Paid drops (build v0.2.0)
1. During the round, you can spend a move to **drop** a reserve from your tray onto the board. The enemy then replies as usual.
2. **Where:** any empty square on your half of the board (ranks 1–3). Pawns can drop on ranks 2–3 only. This answers the open question in DESIGN.md.
3. **Drops follow the normal rules:** a drop can't leave your own king in check, and a drop can block a check.
4. **Drops can score and fork.** A drop that gives check scores as a Check. A drop that attacks two real targets sets up a Fork. Dropping a knight straight into a fork is the moment I expect to be the most fun here.
5. Everything you've learned to read works on drops: previews, SETS UP FORK and UNSAFE all appear on drop squares, and a reserve with a good drop shows a gold badge in the tray.

## Not included, on purpose
- **Money and blinds.** The original plan put unused-move money and Small and Big Blinds in this phase. I'm pushing back on my own plan: money with nothing to spend it on is just a number, and a blind you can skip needs a reward to skip for. Both only mean something next to the shop, so they should move to Phase 3. It also keeps this phase focused on one question.
- **Animated callouts and typing a seed in the game.** Both are still deferred. See decision 4.

## How it's tested
- **Bag and hand:** the same seed and round give the same hand, every hand is 6 pieces from the bag, and the bag is full again next round.
- **Setup rules:** placing is only offered on allowed squares, the 4-piece limit holds, pawns stay on rank 2, squares that would check the enemy king aren't offered, picking a piece back up works, and Start round is blocked if that opens a check. Across 1,000 seeds, every setup still has a legal way to start.
- **Drops:** each rule above gets a board diagram. A drop costs exactly one move, leaves your tray, can't be illegal, blocks check when it should, and scores or sets up a fork when it should. Replays include drops, so bug reports stay exact.
- **The enemy AI** needs no changes, and the existing tests must keep passing.
- **The bot** learns to place pieces and drop them, so we get fresh numbers on how reachable the targets are.

## Decisions for you
1. **Draw 6, place up to 4 free, keep the rest as reserves.** This fills in decision D6: if you placed your whole hand for free, there'd be nothing left to drop.
2. **Your king is placed for you.** Fewer choices at the start of each round, which matters for non-chess players. The alternative is placing it yourself.
3. **Money and blinds move to Phase 3,** alongside the shop.
4. **Animated callouts:** do them as a small extra slice at the end of this phase, or leave them for later? I'd leave them for later, until we've got the shop in.
