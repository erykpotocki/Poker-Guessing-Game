# Mobile release checklist

Baseline: PWA 1.3.0, source bccc4ca, deploy 415e825. Built 2026-09-05.
Compressed baseline: data 67,220,349 bytes; wasm 8,072,101 bytes;
framework 79,420 bytes; loader 117,893 bytes.

The existing Photon turn/deal barriers remain authoritative. An orientation
overlay blocks local input, not the other players' match or the server timer.
Safari cannot expose the iOS rotation-lock setting: react when the viewport
actually changes, and offer an orientation explanation otherwise.

Required physical tests (iPhone SE and XS, Safari and installed PWA):

1. Fresh launch: landscape loading, portrait menu/lobby, landscape match.
2. Disable rotation, enter a match vertically: opaque rotate overlay; no game taps.
   Enable rotation and turn phone: overlay disappears without restarting.
3. Background/resume and rotate both directions; canvas fills viewport, controls
   remain inside safe area, no stretched background or accumulating offsets.
4. Tap nickname on first launch, edit one character in its middle, switch fields.
   Text and caret must remain; keyboard must not cover the focused field.
5. Slow network: progress moves monotonically; loading animation continues;
   100% appears only after runtime readiness. Test offline cached relaunch.
6. Test two real Photon clients: turns/dealing/readiness/history and disconnect.

Asset requests: CLASSIC WOOD transparent avatar frame; short non-vocal local-turn SFX.
Do not substitute newly generated art or a random sound.
