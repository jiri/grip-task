# GRIP Digital interview task

## Zadání

Minecraft s funkcemi:

- [x] 4 druhy kostiček, stačí barevně odlišit/jina texture
- [x] Ve stavebním módu (tj. když si uživatel vybere jednu ze 4 kostiček a chce ji položit) se ukáže wireframe/poloprůhledná kostky se kterou budeme "zaměřovat" a kostka se bude snapovat ke gridu, tzn nepůjde stavět jakkoliv
- [x] Kostky nepůjde umístit jen tak do vzduchu, tzn vždy musí jednou stranou sedět k jiné kostce nebo podlaze
- [x] Uživatel bude mít možnost bloky rozbít, každá ze 4 kostech bude mít svou "tvrdost" tudíž bude potřeba déle držet tlačítko pro rozbití
- [x] Při zapnutí nové mapy hra automaticky vygeneruje nějaký terén/svět na kterém se bude hráč pohybovat (mrkni třeba na perlin noise vcetne nejakych modifikacich pro vyraznejsi rozdily ve vysce) 
- [x] Textura/barva terénu/kostech bude proměnlivá podle toho v jaké výšce je (toto platí pouze pro generovaný teren, nikoliv kostky položené hráčem) 
- [x] V případě že hráč dojde na konec mapy, dogeneruje se další kus a staré se odstraní, ideálně by generování mělo být deterministické (bonus: zachovat uzivatelovi zmeny v odstranenem kusu mapy a obnovit je kdyz se tam hrac vrati)
- [ ] Hra a stav světa půjde uložit do souboru a načíst (není potřeba UI, stačí pevná cesta na 1 save slot)
- [ ] Zamyslet se nad pripadnymi optimalizacemi (rychlost generovani, rendering, multi threading atp) ktere sepsat a pripadne take implementovat pokud ti cas dovoli

## Optimalizace

- Ukládat chunky jako octree
- Threadovaná generace chunků
- Nepoužívat na uložení chunků `Dictionary`
