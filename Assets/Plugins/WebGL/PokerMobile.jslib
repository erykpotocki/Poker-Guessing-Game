mergeInto(LibraryManager.library, {
  PokerSetOrientation: function(landscape) { window.PokerMobile?.setOrientation(!!landscape); },
  PokerKeyboardFraction: function() { return window.PokerMobile?.keyboardFraction || 0; },
  PokerRefreshViewport: function() { window.PokerMobile?.refresh(); }
});
