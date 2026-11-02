# Anarchy Chess — Game Scope

Nov 1, 2026 · @Joe

## Pitch and pillars

Anarchy Chess is a chess roguelike where you score points with tactics instead of chasing checkmate, then build an army so broken the board stops making sense. Balatro's run structure, Disgaea's "break the game on purpose" endgame, and r/AnarchyChess's sense of humour.

1. **Chess you recognize, rules you don't.** Early rounds feel like real chess. By the late antes it should feel like a crime.
2. **Score, don't solve.** The goal is a score target within a move limit, not checkmate. You don't need to beat an engine, only to recognize a fork.
3. **Tactics are the poker hands.** Forks, pins, skewers and en passant are the combos you level up. Players learn real chess by accident.
4. **Breaking it is the intended path.** Late content is tuned so a fair build can't win. Finding the exploit is the progression, not a bug.
5. **Short and readable.** Rounds of 10 moves or fewer, runs under 30 minutes, every move previewed.

## Core loop and scoring

Each round is a short skirmish: reach a score target within a fixed number of moves, without losing your king. The enemy army is a hazard and a clock, not the opponent you have to outthink.

**Score = Value × Mult.** A capture banks the captured piece's value. Mult comes from the tactic used to capture it, plus anything your Anarchists add.

Starting values (to tune in playtesting):

| Piece | Value |
| --- | --- |
| Pawn | 1 |
| Knight | 3 |
| Bishop | 3 |
| Rook | 5 |
| Queen | 9 |
| King (enemy, boss rounds only) | 20 |

Tactics are the hand types, ranked from plain to rare:

| Tactic | Base Mult | Notes |
| --- | --- | --- |
| Capture | ×1 | The "high card" |
| Check | ×2 | Can score with no capture via a check bonus |
| Fork | ×3 | Scores on the follow-up capture too |
| Pin / Skewer | ×4 |  |
| Discovered attack | ×5 |  |
| Promotion | ×6 | Counts as a capture of a pawn |
| En passant | ×10 | The royal flush. Holy hell. |

**Lose conditions:** your king is captured, or the move limit runs out short of the target. Unused moves pay out bonus money, like Balatro's unused hands.

## The army bag (where randomness comes from)

Chess has perfect information, so the randomness has to come from your army. Borrowing from Crazyhouse: your army is a bag of pieces, which works as your deck.

- At the start of a round you draw a hand of pieces from the bag (for example, 5).
- Instead of a fixed setup, you **drop** pieces onto your half of the board. Dropping a piece costs your move for that turn.
- Your king is always on the board. Everything else comes from the hand.
- Deckbuilding matters: thin out pawns, stack knights, add weird pieces from the shop.
- Enemy setups are also randomized per round, so every round is a different puzzle.

This gives Balatro's "what did I draw" tension without turning chess into a dice roll. Once pieces are on the board, everything is fully visible and deterministic.

## Run structure

A run is 8 antes. Each ante is three rounds, with a shop between rounds, mirroring Balatro's blinds.

- **Small Blind:** low target, can be skipped for a reward tag.
- **Big Blind:** higher target, can be skipped.
- **Boss:** a rule twist you must play through. Examples:
  - *Colorblind Bishops:* bishops can move to either colour.
  - *The Cramped:* the board shrinks to 6×6.
  - *Second Wind:* the enemy queen respawns once.
  - *Zugzwang:* every third turn you must move a piece.
  - *Pacifist:* checks score nothing this round.
- **Shop:** buy Anarchists, Gambits, tactic upgrades and new pieces for the bag. Remove or upgrade pieces. Reroll for money.
- **Economy:** money from beating rounds, unused moves, interest on savings, and Gold pieces.

## Modifiers

Four layers stack on top of the core loop, each mapped to a Balatro system.

| Layer | Balatro equivalent | What it does |
| --- | --- | --- |
| Anarchists | Jokers | Passive relics that bend rules or add Mult. Limited slots (start with 5). |
| Enhancements | Card enhancements and editions | Upgrades on a single piece in your bag |
| Gambits | Tarot and spectral cards | One-shot spells used during a round |
| Tactic levels | Planet cards | Permanently raise a tactic's Value and Mult for the run |

**Anarchists (starter set):**

- *Holy Hell:* en passant is ×5 Mult and works on any pawn, anywhere.
- *Bongcloud:* +Mult every turn your king moves.
- *Knook:* knights also move like rooks.
- *Il Vaticano:* the double-bishop capture is legal and scores huge.
- *Brick:* a random square becomes a wall that counts as a piece for tactics.
- *Google En Passant:* every en passant permanently adds +1 Mult to this Anarchist.
- *Rook Lift:* rooks can jump one piece.

**Enhancements:** Glass (×2 Mult on capture, may shatter), Gold (earns money each turn it survives), Steel (this king can't be checked once per round), Holographic (+Mult whenever it moves).

**Gambits:** swap two pieces, promote a pawn now, rewind one move, Castle Anywhere, drop a piece for free.

## Breaking the game (the Disgaea layer)

The late game is designed to be broken. These systems give players the tools to do it, and the escalation curve makes sure they have to.

- **Piece World (Item World):** dive inside one of your pieces for a gauntlet of procedurally generated boards. Clearing floors levels that specific piece permanently.
- **Demotion (Reincarnation):** turn a maxed piece back into a pawn that keeps part of its stats and abilities, then promote it again. Repeat until your pawn moves like three pieces at once.
- **Cursed Squares (Geo Panels):** coloured squares with effects such as ×3 Mult, no capturing, or invincible pieces. Destroying a Geo Block recolours connected squares and chain-scores, so one move can cascade across the board.
- **Stacking and throwing:** stack pieces into a tower that moves as one, or throw a piece over the board. Zero chess sense, fully intended.
- **The Cheat Shop:** opt-in sliders such as "enemy score targets ×10, rewards ×10" for players chasing the absurd tier.

## Escalation curve

The board and the rules get stranger as the run goes on, so the feel shifts from "real chess" to "crime."

| Stage | Board | What changes | Feel |
| --- | --- | --- | --- |
| Antes 1–2 | 6×6 to 8×8 | Near-standard rules, small enemy armies | Real chess with scoring |
| Antes 3–5 | 8×8 | Bigger enemy armies, Cursed Squares appear, chain captures | Chess with combos |
| Antes 6–8 | 8×8 to 10×10 | Atomic captures, duplicating enemies, boss rules stack | Barely chess |
| Post-game | Up to 12×12 | Targets in the millions, tuned to be unwinnable without a broken build | Disgaea numbers |

Rule of thumb: each ante should introduce one new kind of absurdity, not three. Players need a beat to enjoy the new toy before the next one lands.

## Meta progression

Proposed: two layers, so short runs stay the main loop while a long-term collection grows underneath.

- **Run layer (Balatro):** everything in a run resets. Unlocks come from achievements, like new Anarchists, boss rules and starting armies.
- **Persistent layer (Disgaea):** a "Barracks" of pieces you carried out of Piece World. Each run you can bring one veteran piece. Its levels persist forever.

The open question is how much a veteran piece should be allowed to carry a normal run. A cap, or a separate "veteran" difficulty tier, keeps standard runs fair.

## Pick-up-and-play guardrails

- **Score preview on every move:** tap a piece, see legal moves, and each target square shows the score it would bank and the tactic it triggers. This is the main teaching tool.
- **Tactic callouts:** when a fork or pin happens, name it on screen with a quick animation. Players learn the vocabulary by seeing it.
- **Short rounds:** 10 moves or fewer per round, runs under 30 minutes.
- **Dumb but numerous AI:** difficulty comes from targets and boss rules, not engine strength. The enemy plays greedy and predictable so players can plan.
- **Small boards early:** 6×6 for the first antes cuts the number of options to read.
- **One free undo per round** in early antes, removed later or sold as a Gambit.

## Risks and open questions

| Risk | Why it matters | Mitigation |
| --- | --- | --- |
| Analysis paralysis | A chess position takes longer to read than 8 cards | Small boards, move limits, score preview |
| Tactic detection | Recognizing pins and discovered attacks reliably is real engineering | Build the tactic detector first and test it hard |
| Snowballing too early | Broken builds in ante 2 kill the escalation arc | Gate the strongest Anarchists behind later antes |
| Chess players vs. casuals | Purists may hate it, casuals may bounce off chess | Lead with the meme humour and the scoring, not "learn chess" |
| Meme lifespan | r/AnarchyChess jokes may date or need a lighter touch | Keep meme names as flavour on solid mechanics |

Open questions:

- [ ] Should the enemy be allowed to score against you, or only threaten your king?
- [ ] Do dropped pieces have restrictions (no dropping into check, no pawns on the back rank)?
- [ ] Veteran pieces in normal runs: capped, or a separate mode?
- [ ] Platform: PC first, mobile first, or both?

## MVP and roadmap

The MVP answers one question: does banking points with a fork feel good? Everything else waits until that's a yes.

MVP scope:

- [ ] 6×6 board with drop-in pieces from a bag
- [ ] Value × Mult scoring with a live score preview
- [ ] Tactic detection for capture, check, fork, pin and en passant
- [ ] 10 Anarchists
- [ ] 3 antes with a basic shop
- [ ] Greedy enemy AI

&#91;embedded content: roadmap · 4 phases, 3 gates\]

Each phase ships only once its gate passes in playtesting; the Disgaea systems come third because they only matter once a full run works.
