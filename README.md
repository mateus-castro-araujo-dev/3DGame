# EcoDefender 🌱♻️

Jogo 2D educacional sobre reciclagem, desenvolvido em Unity 6.

> Disciplina: Desenvolvimento de Jogos e Realidade Virtual — IFPI Campus Parnaíba
> Professor: Denylson Melo
> Trabalho 03 — Protótipo de Jogo 2D · Grupo 05

## 👥 Integrantes do grupo

- Mateus Castro
- Yago Braga
- Oresto Neto

## 🎮 Sobre o jogo

Lixo cai do céu sobre a cidade. O jogador defende a cidade coletando cada resíduo e entregando na lixeira de reciclagem correta antes que ele grude no chão e polua o ambiente. Cada lixo que gruda reduz a saúde da cidade — se ela chegar a 0%, é fim de jogo.

Entre as ondas há uma fase de preparação, na qual o jogador gasta as moedas ganhas reciclando para comprar e posicionar unidades (coletores, ímãs e drones) em pontos estratégicos do mapa. São 5 ondas, cada uma mais rápida e com eventos especiais.

### Objetivo educacional

Ensinar a separação correta do lixo segundo o padrão brasileiro de coleta seletiva (CONAMA):

- 🟢 Vidro (verde) — garrafas, potes, frascos
- 🔴 Plástico (vermelho) — garrafas, sacolas, embalagens
- 🔵 Papel (azul) — jornais, caixas, cadernos
- 🟡 Metal (amarelo) — latas, tampas, alumínio

## 🕹️ Como jogar

| Ação | Controle |
|---|---|
| Mover | A / D ou setas |
| Pegar lixo mais próximo | Espaço ou E (mão vazia) |
| Entregar na lixeira | Espaço ou E (carregando, perto da lixeira) |
| Iniciar a onda | Enter ou botão COMEÇAR ONDA |
| Abrir/fechar a loja | Tab ou botão LOJA |
| Selecionar unidade | 1 Coletor · 2 Ímã · 3 Drone |
| Posicionar unidade | clicar no mapa (fase de preparação) |
| Ver tutorial | botão i (canto inferior esquerdo) |

> A entrega na lixeira errada descarta o lixo (-20 pontos, mas libera a mão) — pode ser usado como estratégia para se livrar de um item quase grudando.

## 🎲 Elementos do design (sorteados — Grupo 05)

| Elemento | Valor | Como aparece no jogo |
|---|---|---|
| Gênero | Action | ritmo rápido, lixo caindo em tempo real |
| Tema | Sticky | lixo gruda no chão após 5s (4 estágios + mancha tóxica) |
| Interação | Defend | defender a saúde da cidade (100% → 0) |
| Forma | Vertical Scroller | lixo cai de cima para baixo |
| Mecânica 1 | Pick up and deliver | coletar e entregar na lixeira correta |
| Mecânica 2 | Simultaneous action | vários lixos ao mesmo tempo + unidades em paralelo |
| Mecânica 3 | Secret unit deployment | comprar e posicionar unidades antes da onda |

## ✨ Principais sistemas

- 5 ondas progressivas com eventos nomeados (Chuva de Plástico, Tempestade de Metal, Invasão de Vidros, Semana da Reciclagem)
- Economia de moedas + loja de 3 tipos de unidade (Coletor / Ímã de Metal / Drone Ambiental)
- Saúde da cidade como condição de derrota
- Combo / multiplicador (até 5×) e lixo dourado (bônus 3×)
- Power-ups (Limpeza, Câmera Lenta, Anti-Poluição)
- Áudio e arte 100% procedurais (sem assets externos)

## 🛠️ Tecnologia

- Unity 6 (6000.5.0f1) · pipeline URP
- DOTween (animações)
- Toda a cena é construída por código (`GameBootstrap.cs`) — não há montagem manual no Inspector
- Arte e áudio gerados proceduralmente em runtime
- Estilo visual: neo-brutalismo

## ▶️ Como rodar no editor

1. Abrir o projeto no Unity 6
2. Menu **EcoDefender → Create Game Scene** (cria `Assets/Scenes/GameScene.unity`)
3. Abrir a cena e apertar Play

## 🌐 Jogar online

▶️ Jogue agora no navegador: https://yago-braga.itch.io/ecodefenders
