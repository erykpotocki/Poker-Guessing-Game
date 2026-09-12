# Kod gry i publikacja PWA

- `main` zawiera aktualny projekt Unity i służy do dalszego rozwoju.
- `gh-pages` zawiera opublikowany build PWA.

Ustawienia GitHub Pages sprawdzono przez API 12 września 2026: publikacja typu `legacy`, źródło `gh-pages`, katalog `/`. Sam push na `main` nie publikuje wersji dla graczy. Repozytorium nie zawiera workflow automatycznie wdrażającego `main`.

Aktualizację PWA wykonujemy osobno: tworzymy i sprawdzamy build poleceniem z menu Unity opisanym w `Assets/Editor/BuildPokerPwa.cs`, następnie umieszczamy zatwierdzone pliki buildu na `gh-pages`. Dopiero wysłanie tej gałęzi publikuje wydanie. Zmiana zasad hostingu lub dodanie workflow wymaga ponownego sprawdzenia tego rozdzielenia.

Nie należy scalać plików opublikowanego buildu z `gh-pages` z powrotem do projektu Unity. Dawne gałęzie rozwojowe zostały scalone do `main`; ich historia pozostaje dostępna w commitach.
