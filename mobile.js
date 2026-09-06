/* Scene-aware mobile shell. Native orientation locks are optional; the blocker is not. */
(() => {
  'use strict';
  const mobile = matchMedia('(pointer: coarse)').matches;
  let desired = 'landscape', booting = true, sceneOrientation = 'portrait';
  const blocker = document.createElement('section');
  blocker.id = 'orientation-blocker';
  blocker.setAttribute('role', 'dialog');
  blocker.setAttribute('aria-modal', 'true');
  blocker.style.cssText = 'position:fixed;inset:0;z-index:10000;display:none;place-content:center;text-align:center;padding:32px;background:#07100e;color:#ffe6aa;font:600 24px Arial;touch-action:none';
  const title = document.createElement('div');
  title.textContent = 'Obróć telefon';
  const detail = document.createElement('p');
  detail.style.cssText = 'font:16px Arial;line-height:1.5;max-width:380px';
  blocker.append(title, detail);
  document.body.appendChild(blocker);
  const isEditing = () => /^(INPUT|TEXTAREA)$/.test(document.activeElement?.tagName || '');
  function refresh() {
    const vv = window.visualViewport;
    const width = window.innerWidth, height = window.innerHeight;
    const editing = isEditing();
    const landscape = width > height;
    const blocked = mobile && !editing && (desired === 'landscape' ? !landscape : landscape);
    blocker.style.display = blocked ? 'grid' : 'none';
    detail.textContent = desired === 'landscape' ? 'Ten ekran działa poziomo. Odblokuj obracanie ekranu i obróć telefon.' : 'Menu i poczekalnia działają pionowo.';
    const canvas = document.getElementById('unity-canvas');
    if (canvas) { canvas.style.pointerEvents = blocked ? 'none' : ''; canvas.inert = blocked; }
    const area = canvas?.getBoundingClientRect();
    window.PokerMobile.keyboardFraction = editing && vv && area?.height > 0
      ? Math.max(0, Math.min(1, (area.bottom - vv.offsetTop - vv.height) / area.height)) : 0;
  }
  function tryLock() {
    if (!mobile || isEditing()) return;
    try { const result = screen.orientation?.lock?.(desired); result?.catch(() => {}); } catch (_) {}
  }
  window.PokerMobile = {
    nextArtwork() {
      let previous = 0;
      try { previous = Number(localStorage.getItem('poker-loading-art') || 0); } catch (_) {}
      const next = previous >= 1 && previous <= 5 ? ((previous - 1 + 1 + Math.floor(Math.random()*4)) % 5) + 1 : 1 + Math.floor(Math.random()*5);
      try { localStorage.setItem('poker-loading-art',String(next)); } catch (_) {}
      return next;
    },
    keyboardFraction: 0,
    setOrientation(landscape) { sceneOrientation = landscape ? 'landscape' : 'portrait'; desired = booting ? 'landscape' : sceneOrientation; refresh(); tryLock(); },
    finishBoot() { booting = false; desired = sceneOrientation; refresh(); tryLock(); },
    refresh
  };
  ['resize', 'orientationchange', 'pageshow', 'focus'].forEach(name => window.addEventListener(name, () => { refresh(); tryLock(); }));
  document.addEventListener('visibilitychange', () => { if (!document.hidden) { refresh(); tryLock(); } });
  document.addEventListener('pointerup', tryLock);
  document.addEventListener('focusin', refresh);
  document.addEventListener('focusout', () => requestAnimationFrame(refresh));
  window.visualViewport?.addEventListener('resize', refresh);
  window.visualViewport?.addEventListener('scroll', refresh);
  screen.orientation?.addEventListener('change', refresh);
  refresh();
  const loading = document.getElementById('loading-screen');
  if (loading) loading.style.backgroundImage = "linear-gradient(#0005,#0008),url('StreamingAssets/LoadingScreens/" + window.PokerMobile.nextArtwork() + ".png')";
})();
