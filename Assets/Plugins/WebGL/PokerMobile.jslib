mergeInto(LibraryManager.library, {
  PokerLoadProfile: function() {
    var json = '';
    try {
      json = localStorage.getItem('poker.profile.v1') || '';
      if (json) JSON.parse(json);
    } catch (_) { json = ''; }
    var size = lengthBytesUTF8(json) + 1;
    var buffer = _malloc(size);
    stringToUTF8(json, buffer, size);
    return buffer;
  },
  PokerSaveProfile: function(pointer) {
    try {
      var json = UTF8ToString(pointer);
      localStorage.setItem('poker.profile.v1', json);
      return localStorage.getItem('poker.profile.v1') === json ? 1 : 0;
    } catch (_) { return 0; }
  },
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
