# Dokończenie zmian — 12 września 2026

Wersja robocza: `codex/finish-game-updates`, utworzona z `dev-all-2026-09-11` (3d027e4).
Główny katalog był przełączony na starszą gałąź `main`; obecnych tam zmian użytkownika nie przenoszono ani nie usuwano.

## Zachowanie

- Sklep i odblokowywanie z profilu korzystają ze wspólnych zasad cen. Prestige 1–6: 500 diamentów lub 5000 złota; 7–14: 750 diamentów lub 7500 złota; 15–24: jednocześnie 1000 diamentów i 10000 złota. Ostatnia grupa jest wykluczona ze spina.
- Drewno kosztuje 250 złota, eleganckie 1000, klasyczne 50; lodowe są wyłącznie ze spina. Wyjątkiem jest wskazany elegancki rewers `6`, darmowy dla nowych profili. Dotychczasowy wybór rewersu istniejącego profilu jest zachowany. Katalog plików nie zawiera identycznych hashy ani powtórzonych identyfikatorów.
- Ramki nigdy nie są nagrodą ze spina, również przy próbie odebrania starej oczekującej nagrody. Wcześniejsze odblokowanie dotyczy kolejnej brakującej ramki poziomu, za poziom × 10 diamentów. Ramka pierwszej gry jest odbierana w misjach.
- Misje: pierwsza gra, przeczytanie zasad, pierwsza wygrana, 10 rund. Odbiór jest ręczny i jednokrotny. Gotowe misje są zielone i na górze; odebrane przenoszą się do ukończonych. Waluta nie otwiera dodatkowych okien.
- Przygoda jest zapowiedzią przyszłego trybu singleplayer.
- Reklama spina jest oznaczonym pięciosekundowym placeholderem, bez połączenia z siecią reklam. Zamknięcie przed końcem nie przyznaje spina.
- Przyrost waluty animuje licznik i kolorowy tekst `+kwota`. Spin ma większy licznik i czas nad przyciskiem, bez tekstu o zaksięgowaniu nagrody.
- Wygrana daje 150 EXP. Pierwszy wyeliminowany otrzymuje 15 EXP, pozostałe miejsca proporcjonalnie pomiędzy tymi wartościami. Czas pozostawania w pokoju nie zwiększa EXP. Nowa wygrana na poziomie 1 daje poziom 2.
- Statystyki obejmują wygrane rundy i eliminacje. Eliminację dostaje skutecznie sprawdzający, gdy przeciwnik osiągnie etap odpadnięcia. Wyniki są zapisywane z identyfikatorem rundy, więc ponowne przetworzenie historii nie nalicza ich ponownie. Dawne statystyki nie są odtwarzane wstecz.
- Publiczny profil pokazuje poziom, datę dołączenia, liczbę kosmetyków i Beta testera. Własny profil przy stole umożliwia wysłanie sześciu animowanych reakcji do pokoju. Data dołączenia starych zapisów może być niedostępna.
- Czat ma maskowanie, dopasowaną wysokość, filtry system/gracze/wszystko, zwijanie, ustawienia widoczności i lokalną edycję rozmiaru/położenia. Wiadomości graczy są wysyłane do pokoju, bez zapisywania historii poza meczem.
- Poprawiono panel układów, reset wyszukiwania na rundę/zmianę tury, kursor nicku, szerokość rewersów, podpis autora i animację odblokowania. Usunięto nazwę krupiera.

## Sprawdzenie

- Kompilacja wszystkich skryptów przez .NET z referencjami lokalnego Unity: 0 błędów.
- Samodzielny harness: 324 asercje zasad nagród, misji, zakupów, cen, kolejności ramek i EXP zakończone powodzeniem. W tym teście grafiki/ładowanie Unity zastąpiono prostymi obiektami, wykorzystując nazwy rzeczywistych zasobów.
- Dodano/zmieniono testy NUnit w `Assets/Editor/ProfileLevelProgressionTests.cs`.
- Próba uruchomienia dodatkowego Unity w trybie batch zakończyła się wyjątkiem Win32 przed utworzeniem logu. Proces 17320 został zatrzymany. Testy Play Mode, renderowanie oraz połączenie dwóch klientów wymagają sprawdzenia w działającym edytorze; nie zostały potwierdzone tą próbą.
