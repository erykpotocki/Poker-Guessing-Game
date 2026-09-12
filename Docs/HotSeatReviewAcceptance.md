# Hot Seat review and deck — 2026-09-08

Source changes only. No build or publication performed.

- Vector suit pips: 9 and 10 central symbols, one large ace symbol, inverted lower index.
- Existing custom court art displayed as larger double-ended figures. Multiplayer unchanged.
- Review rows follow starterIndex and include every dealt card, with no three-card truncation.
- Six cards fit each row; larger hands scroll horizontally. Player list scrolls vertically.
- Sequential reveal; skip accelerates remaining cards rather than dismissing the result.
- MatchingCards is the shared source of truth for loss calculation, verdict, selected highlights and evidence slots.
- Evidence slots adapt to declaration length and preserve empty positions for incomplete hands.
- Review remains visible until CONTINUE, including the final round. Tapping a revealed card after the animation enlarges it.
- Current Hot Seat penalty/dealing limits were not changed to six cards.

Checked: C# syntax parsing and whitespace diff. Not yet checked: Unity compilation, on-device rendering/input, interactive gameplay.

Acceptance in Unity / iPhone:

1. Inspect every suit of 9/10/A/J/Q/K; count central pips; verify back conceals all front elements for one and multiple cards.
2. Check two aces among three cards: PAIR_A succeeds, TRIPS_A fails.
3. Verify full house, two pairs, straights and suit-specific hands: only the necessary cards populate evidence slots.
4. Use a six-player review fixture with six cards each (UI fixture, not a change to dealing rules); all 36 cards must be accessible, row and column order preserved.
5. Skip midway: all remaining cards reveal quickly; result stays open; scroll and enlarge cards; CONTINUE advances exactly once.
6. Final elimination still shows the checked hand before the game-over screen.
