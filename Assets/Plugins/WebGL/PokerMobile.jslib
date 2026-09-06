mergeInto(LibraryManager.library, {
  PokerNextLoadingArtwork: function() { return window.PokerMobile?.nextArtwork() || 1; },
  PokerSetOrientation: function(landscape) { window.PokerMobile?.setOrientation(!!landscape); },
  PokerKeyboardFraction: function() { return window.PokerMobile?.keyboardFraction || 0; },
  PokerRefreshViewport: function() { window.PokerMobile?.refresh(); }
});
