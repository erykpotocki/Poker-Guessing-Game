# Mobile release checklist

Historical published baseline (not the orientation changes below):
PWA 1.3.0, source bccc4ca, deploy 415e825. Built 2026-09-05.
Compressed baseline: data 67,220,349 bytes; wasm 8,072,101 bytes;
framework 79,420 bytes; loader 117,893 bytes.

## Orientation contract — local source changes, 2026-09-07

- Cold launch, HTML startup loading, Unity BootLoading, every menu and lobby:
  portrait. Opening the app must not automatically rejoin a saved match.
- Host START (and its notification to the other lobby players): immediately show
  landscape match loading, then landscape Game. Loading stays at least 5 real
  seconds after entering Game and until the existing Photon readiness barrier.
- Explicitly joining an already-started room is also a landscape match entry;
  recovering a connection within the current session remains supported.
- Return to menu restores portrait. Hot Seat rules/gameplay are unchanged.
- No rotate-phone modal, no disabled canvas due to an orientation mismatch.

Native builds use Unity orientation locking. PWA requests the browser's lock,
but cannot override an iOS/Safari refusal or the user's system rotation lock.
When the physical viewport has the wrong aspect, the mobile shell rotates its
logical viewport instead. The Unity render dimensions and mouse/touch positions
are adjusted together. On a portrait-locked iPhone the landscape game appears
sideways, ready for physically turning the phone, without a blocking message.
The system keyboard/status bar remain controlled by the OS, not by this fallback.
CSS safe-area insets exclude the notch/status bar/home indicator from the canvas.

Initial HTML loading never uses multiplayer landscape artwork or landscape CSS.
The PWA manifest defaults to portrait-primary; multiplayer changes it at runtime
where supported. The fallback does not require fullscreen or a permission dialog.

Status: source and runtime-library review only. No compilation, tests, deployment
or installed-PWA verification performed for these changes. The published baseline
does not acquire these changes until a separately authorized build and deployment.

Completion review: late joins read the room's existing readiness state instead of
requiring another change notification. A disconnect during the one-frame entry
transition cancels the pending explicit scene load and restores portrait. These
paths have been reviewed in source only, not exercised on a device.

## Deferred acceptance checks (do not run without the user's authorization)

iPhone SE and XS, Safari and installed PWA:

1. Fresh launch in either device position, including with a saved active room:
   portrait startup/menu/lobby, no unexpected automatic match entry.
2. Turn the phone in menu: portrait layout remains. Host START: the loader already
   has the landscape layout before Game is visible; minimum 5 seconds is respected.
3. Disable OS rotation, enter a match vertically, then physically turn the phone:
   landscape content, no modal, every seat and corner button responds at its drawn
   location. Repeat in both landscape directions with OS rotation enabled.
4. Background/resume and rotate both directions; controls remain inside safe area,
   no stretching/accumulating offsets; exiting the match returns to portrait.
5. Tap nickname on first launch, edit one character in its middle, switch fields.
   Text and caret must remain; keyboard must not cover the focused field.
6. Slow network: progress moves monotonically; loading animation continues;
   100% appears only after runtime readiness. Test offline cached relaunch.
7. Two real Photon clients: host START and client transition, readiness/dealing,
   explicit join of an active match, interrupted load, reconnect and return to menu.

Asset requests: CLASSIC WOOD transparent avatar frame; short non-vocal local-turn SFX.
Do not substitute newly generated art or a random sound.
