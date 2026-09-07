mergeInto(LibraryManager.library, {
  PokerNextLoadingArtwork: function() {
    return window.PokerMobile && window.PokerMobile.nextArtwork
      ? window.PokerMobile.nextArtwork()
      : 1;
  },
  PokerSetOrientation__deps: ['$JSEvents'],
  PokerSetOrientation: function(landscape) {
    // Only Unity callbacks are mapped; HTML controls keep native hit-testing.
    if (!JSEvents.pokerOrientationInputInstalled) {
      JSEvents.pokerOrientationInputInstalled = true;
      var wrap = function(handler) {
        if (!handler.handlerFunc || handler.pokerOrientationWrapped ||
            !/^(mouse|touch|pointer|click|dblclick|wheel)/.test(handler.eventTypeString) ||
            (handler.target !== Module['canvas'] && handler.target !== window && handler.target !== document)) return;
        handler.pokerOrientationWrapped = true;
        var original = handler.handlerFunc;
        handler.handlerFunc = function(event) {
          return original(window.PokerMobile && window.PokerMobile.mapInputEvent
            ? window.PokerMobile.mapInputEvent(event) : event);
        };
      };
      JSEvents.eventHandlers.forEach(wrap);
      var register = JSEvents.registerOrRemoveHandler;
      JSEvents.registerOrRemoveHandler = function(handler) {
        wrap(handler);
        return register.call(JSEvents, handler);
      };
    }
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
