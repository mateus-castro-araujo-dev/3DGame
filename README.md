# Rescue Protocol 🚑🧠

Jogo 3D educacional sobre primeiros socorros, desenvolvido em Unity.

> Disciplina: Desenvolvimento de Jogos e Realidade Virtual — IFPI Campus Parnaíba
> Professor: Denylson Melo
> Trabalho 03 — Protótipo de Jogo 3D · Grupo 05

## 👥 Integrantes do grupo

- Mateus Castro
- Yago Braga
- Oresto Neto

## 🎮 Sobre o jogo

O jogador participa de um treinamento de resgate e primeiros socorros. Em cada sala, uma "vítima" precisa ser salva: é preciso memorizar um protocolo de ações e executá-las na ordem correta, antes que as paredes da sala terminem de se fechar e esmaguem o jogador.

São 3 cenas progressivas. Ao entrar em cada uma, o jogador é congelado durante um briefing com contagem regressiva para memorizar o protocolo. Depois, precisa agir rápido: interagir com os objetos certos, na ordem certa, ou recomeçar o treinamento desde o início.

### Objetivo educacional

Ensinar e fixar protocolos básicos de primeiros socorros através da repetição sob pressão de tempo, simulando a urgência de uma situação real de resgate — onde a ordem das ações certas pode ser a diferença entre salvar ou não a vítima.

## 🕹️ Como jogar

| Ação | Controle |
|---|---|
| Mover | WASD ou setas |
| Olhar em volta | Mouse |
| Realizar ação de resgate / interagir | E |
| Alternar Modo Memória / Modo Presente | Q |

> No Modo Memória, pistas escondidas ficam visíveis para ajudar a relembrar o protocolo — mas as paredes avançam mais rápido enquanto o modo estiver ativo. É um trade-off entre informação e tempo.

## 🎲 Elementos do design

| Elemento | Valor | Como aparece no jogo |
|---|---|---|
| Gênero | Survival / Puzzle | corrida contra o tempo para completar o protocolo |
| Tema | Enclosing space | as paredes da sala se fecham progressivamente até esmagar o jogador |
| Interação | Memorize & execute | memorizar o protocolo no briefing e executá-lo na ordem certa |
| Forma | First person | exploração e interação em primeira pessoa |
| Mecânica 1 | Ordered interaction | cada ação de resgate só pode ser feita no momento certo da sequência |
| Mecânica 2 | Limited memory | pistas (textos) aparecem e desaparecem, forçando a memorização |
| Mecânica 3 | Risk/reward mode switch | Modo Memória revela pistas escondidas, mas acelera as paredes |

## ✨ Principais sistemas

- 3 cenas/salas progressivas, cada uma com seu próprio protocolo de resgate
- Briefing com contagem regressiva e congelamento do jogador ([GameManager.cs](Assets/Scripts/GameManager.cs))
- Paredes que se fecham e reiniciam a sala em caso de esmagamento ([EnclosingRoom.cs](Assets/Scripts/EnclosingRoom.cs))
- Sequência de ações ordenadas, com falha e reinício do treinamento ao errar a ordem ([RescueSequence.cs](Assets/Scripts/RescueSequence.cs), [InteractionStep.cs](Assets/Scripts/InteractionStep.cs))
- Modo Memória com custo de risco (paredes mais rápidas) ([ModeSwitcher.cs](Assets/Scripts/ModeSwitcher.cs))
- Pistas com "amnésia": aparecem e somem após alguns segundos ([FadingClue.cs](Assets/Scripts/FadingClue.cs))
- Terminal de quiz com perguntas educacionais de múltipla escolha ([PuzzleTerminal.cs](Assets/Scripts/PuzzleTerminal.cs))
- Teleportes que liberam o acesso à próxima sala após o desafio ser concluído ([Teleporter.cs](Assets/Scripts/Teleporter.cs))

## 🛠️ Tecnologia

- Unity, controle em primeira pessoa com `CharacterController` ([PlayerController.cs](Assets/Scripts/PlayerController.cs))
- UI nativa do Unity (`UnityEngine.UI`) para mensagens, briefing e painel de vitória
- Cena montada no editor ([SampleScene.unity](Assets/Scenes/SampleScene.unity)), com scripts atribuídos via Inspector

## ▶️ Como rodar no editor

1. Abrir o projeto no Unity
2. Abrir a cena [Assets/Scenes/SampleScene.unity](Assets/Scenes/SampleScene.unity)
3. Apertar Play
