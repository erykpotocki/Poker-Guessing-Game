# Dokończenie zmian — 12 września 2026

Aktualny kod znajduje się na `main` w głównym katalogu projektu. Scalono `codex/finish-game-updates` oraz historię pozostałych gałęzi rozwojowych. Lokalne zmiany użytkownika w avatarach zostały zachowane.

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
- Unity 6000.3.10f1: testy uruchomiono po zapewnieniu dostępu do systemowego cache. 29 testów Edit Mode przeszło, w tym rzeczywisty katalog rewersów i hierarchia panelu układów w scenie Game. Wyniki: `Logs/feature-tests-final.xml`.
- Pełny przebieg: 30 testów, 30 zaliczonych, 0 błędów. Obejmuje regresję Play Mode animacji waluty po ponownym utworzeniu paska menu. Wyniki: `Logs/feature-tests-complete.xml`.
- Testy bez renderera nie potwierdzają wyglądu na fizycznym telefonie ani komunikacji dwóch klientów Photon.

## Poprawki po scaleniu

- Sklep ma całkowicie nieprzezroczyste tło. Profil i spin otwierane z paska nie chowają się za sklepem.
- Przyciski sklepu, misji i profilu zachowują dopasowywany tekst; okresowe nakładanie motywu menu nie wymusza na nich dużej, stałej czcionki ani nakładających się pól kliknięcia.
- Aktywna lista układów zostaje bezpośrednią zawartością przewijania już podczas jej wybrania.
- Karty przy stole mają wspólną proporcję szerokości do wysokości 0,72; rewers wypełnia ten obszar.
- Podgląd odblokowanego rewersu pobiera grafikę z aktualnego katalogu online.
- Okno odblokowania zaczyna od zerowej przezroczystości, więc pierwsza klatka nie pojawia się nagle.
- Pasek pamięta ostatnio widzianą walutę między scenami, aby pokazać animację nagrody także po meczu. Szerokość licznika aktualizuje się podczas przyrostu cyfr.
