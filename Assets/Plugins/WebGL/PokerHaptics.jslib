mergeInto(LibraryManager.library, {
  PokerLightHaptic: function () {
    try { if (typeof navigator.vibrate === 'function') navigator.vibrate(12); } catch (_) {}
  }
});
