mergeInto(LibraryManager.library, {
  PokerNextLoadingArtwork: function() {
    return window.PokerMobile && window.PokerMobile.nextArtwork
      ? window.PokerMobile.nextArtwork()
      : 1;
  },
  PokerSetOrientation: function(landscape) {
    if (window.PokerMobile && window.PokerMobile.setOrientation)
      window.PokerMobile.setOrientation(!!landscape);
  },
  PokerKeyboardFraction: function() {
    return window.PokerMobile ? (window.PokerMobile.keyboardFraction || 0) : 0;
  },
  PokerRefreshViewport: function() {
    if (window.PokerMobile && window.PokerMobile.refresh)
      window.PokerMobile.refresh();
  }
});
