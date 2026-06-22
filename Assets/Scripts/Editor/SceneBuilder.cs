using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

// Gera o jogo completo "Resgate da Memória — Treinamento de Primeiros Socorros".
//
// JOGABILIDADE: o jogador é um socorrista com amnésia. Ao entrar em cada cena de
// acidente, o PROTOCOLO (ordem das 3 ações) aparece por alguns segundos e some.
// É preciso executar as ações NA ORDEM, interagindo com objetos e pessoas [E]:
//   1. Eliminar o perigo (cones / fogão / máquina)
//   2. Chamar o SAMU no rádio (192)
//   3. Atender a vítima (consciência / respiração / RCP com vários toques de E)
// Errou a ordem? O jogo avisa e ensina. Esqueceu o protocolo? [Q] Modo Memória
// mostra o eco azul — mas as paredes vermelhas fecham mais rápido.
//
// Use: "Tools > Protocolo Lembranca > Gerar Jogo Completo (3 Salas)"
public static class SceneBuilder
{
    const float ROOM_SPACING = 80f; // longe + névoa = não se vê uma sala da outra
    const float ROOM_SIZE = 30f;

    struct RoomContent
    {
        public string title;
        public string briefing;       // protocolo mostrado ao entrar (some = amnésia)
        public string hazardAction;   // passo 1
        public string hazardSuccess;
        public string callAction;     // passo 2
        public string callSuccess;
        public string victimAction;   // passo 3
        public string victimSuccess;
        public int victimPresses;
    }

    static readonly RoomContent[] Rooms = new RoomContent[]
    {
        new RoomContent
        {
            title = "CENA 1 - ACIDENTE DE TRÂNSITO",
            briefing = "MEMORIZE O PROTOCOLO:\n\n1. SINALIZAR a área (cones)\n2. CHAMAR o SAMU no rádio (192)\n3. VERIFICAR a vítima",
            hazardAction = "Sinalizar a área com os cones",
            hazardSuccess = "Área sinalizada! Segurança primeiro: sem ela, você pode virar a segunda vítima.",
            callAction = "Chamar o SAMU (192) no rádio",
            callSuccess = "SAMU acionado! Sempre informe: local exato, número de vítimas e estado delas.",
            victimAction = "Verificar consciência da vítima",
            victimSuccess = "Vítima consciente! Converse e mantenha-a calma até o socorro chegar.",
            victimPresses = 1
        },
        new RoomContent
        {
            title = "CENA 2 - ACIDENTE DOMÉSTICO (COZINHA)",
            briefing = "MEMORIZE O PROTOCOLO:\n\n1. DESLIGAR o fogão (perigo!)\n2. CHAMAR o SAMU no rádio (192)\n3. CHECAR a respiração da vítima",
            hazardAction = "Desligar o fogão",
            hazardSuccess = "Fogão desligado! Elimine o perigo antes de tocar na vítima.",
            callAction = "Ligar para o SAMU (192)",
            callSuccess = "SAMU a caminho! Nunca desligue antes de o atendente mandar.",
            victimAction = "Checar respiração (observe o peito até 10 s)",
            victimSuccess = "Respiração presente! Vítima inconsciente que respira: posição lateral de segurança.",
            victimPresses = 1
        },
        new RoomContent
        {
            title = "CENA 3 - ACIDENTE DE TRABALHO",
            briefing = "MEMORIZE O PROTOCOLO:\n\n1. DESLIGAR a máquina\n2. CHAMAR emergência (192)\n3. INICIAR RCP: aperte E 10 vezes\n   (ritmo de 30 compressões / 2 ventilações)",
            hazardAction = "Desligar a máquina",
            hazardSuccess = "Máquina desligada! Choques e esmagamentos: nunca toque a vítima antes disso.",
            callAction = "Chamar emergência (192) no rádio",
            callSuccess = "Emergência acionada! Peça um DEA (desfibrilador) se houver no local.",
            victimAction = "Fazer compressões de RCP",
            victimSuccess = "RCP em andamento! Adultos: 30 compressões fortes e rápidas + 2 ventilações, sem parar.",
            victimPresses = 10
        },
    };

    [MenuItem("Tools/Protocolo Lembranca/Gerar Jogo Completo (3 Salas)")]
    public static void BuildGame()
    {
        EnsureTag("MemoryEcho");
        EnsureTag("Player");

        // ---- Névoa: clima de "memória apagada", mas leve o bastante para enxergar
        //      a sala. As salas vizinhas já ficam escondidas pela casca de paredes/teto,
        //      então a névoa não precisa ser densa (antes engolia o cenário inteiro). ----
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogColor = new Color(0.08f, 0.10f, 0.15f);
        RenderSettings.fogStartDistance = 12f;
        RenderSettings.fogEndDistance = 55f;

        // ---- Jogador ----
        GameObject player = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        player.name = "Player";
        player.tag = "Player";
        player.transform.position = new Vector3(0, 1, -10);
        Object.DestroyImmediate(player.GetComponent<CapsuleCollider>());
        CharacterController cc = player.AddComponent<CharacterController>();
        cc.center = new Vector3(0, 1, 0);

        Camera mainCam = Camera.main;
        GameObject camObj = mainCam != null ? mainCam.gameObject : new GameObject("Main Camera", typeof(Camera));
        camObj.transform.SetParent(player.transform);
        camObj.transform.localPosition = new Vector3(0, 1.6f, 0); // altura dos olhos
        camObj.transform.localRotation = Quaternion.identity;
        if (camObj.GetComponent<AudioListener>() == null) camObj.AddComponent<AudioListener>();
        Camera cam = camObj.GetComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.08f, 0.10f, 0.15f);
        SetupPostProcessing(cam); // bloom, tonemapping, vinheta, antialiasing

        var pc = player.AddComponent<PlayerController>();
        pc.cameraTransform = camObj.transform;

        // ---- Luz principal (sol/teto) ----
        GameObject lightObj = GameObject.Find("Directional Light");
        if (lightObj != null)
        {
            Light dl = lightObj.GetComponent<Light>();
            dl.color = new Color(0.85f, 0.88f, 1f);
            dl.intensity = 0.9f;
            dl.shadows = LightShadows.Soft;
            dl.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        }

        // Luz ambiente azulada suave: preenche as sombras sem estourar a cena,
        // deixando o cenário visível e com mais "profundidade".
        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.17f, 0.19f, 0.25f);

        // ---- GameSystems ----
        GameObject systems = new GameObject("GameSystems");
        systems.AddComponent<ModeSwitcher>();
        var gameManager = systems.AddComponent<GameManager>();
        gameManager.player = player.transform;

        // ---- UI ----
        BuildUI(gameManager);

        // ---- Salas ----
        Teleporter prevTeleporter = null;
        Transform[] spawns = new Transform[Rooms.Length];

        for (int i = 0; i < Rooms.Length; i++)
        {
            float cx = i * ROOM_SPACING;
            spawns[i] = BuildRoom(i, cx, Rooms[i], out Teleporter exitTeleporter);

            if (prevTeleporter != null)
                prevTeleporter.destination = spawns[i];

            prevTeleporter = exitTeleporter;
        }

        prevTeleporter.isFinal = true;
        gameManager.initialSpawn = spawns[0];

        Debug.Log("Jogo gerado! Aperte Play.");
    }

    static Transform BuildRoom(int index, float cx, RoomContent content, out Teleporter exitTeleporter)
    {
        string suf = "_R" + index;
        GameObject room = new GameObject("Room" + suf);

        // ---- Chão ----
        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
        floor.name = "Floor" + suf;
        floor.transform.position = new Vector3(cx, 0, 0);
        floor.transform.localScale = new Vector3(ROOM_SIZE / 10f, 1, ROOM_SIZE / 10f);
        Paint(floor, new Color(0.16f, 0.16f, 0.19f));
        SetSmoothness(floor, 0.3f); // leve brilho de "concreto polido", pega os reflexos das luzes
        floor.transform.SetParent(room.transform);

        // ---- Teto ----
        GameObject ceiling = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ceiling.name = "Ceiling" + suf;
        ceiling.transform.position = new Vector3(cx, 4f, 0);
        ceiling.transform.rotation = Quaternion.Euler(180, 0, 0);
        ceiling.transform.localScale = new Vector3(ROOM_SIZE / 10f, 1, ROOM_SIZE / 10f);
        Object.DestroyImmediate(ceiling.GetComponent<Collider>());
        Paint(ceiling, new Color(0.09f, 0.09f, 0.11f));
        ceiling.transform.SetParent(room.transform);

        // ---- Casca externa fixa (4 paredes): nada vaza pra fora da sala ----
        CreateWall("Wall_North" + suf, new Vector3(cx, 2f, 15), new Vector3(ROOM_SIZE + 1, 4, 0.3f), new Color(0.24f, 0.26f, 0.32f)).transform.SetParent(room.transform);
        CreateWall("Wall_South" + suf, new Vector3(cx, 2f, -15), new Vector3(ROOM_SIZE + 1, 4, 0.3f), new Color(0.24f, 0.26f, 0.32f)).transform.SetParent(room.transform);
        CreateWall("Wall_East_Fixed" + suf, new Vector3(cx + 15.4f, 2f, 0), new Vector3(0.3f, 4, ROOM_SIZE + 1), new Color(0.18f, 0.2f, 0.25f)).transform.SetParent(room.transform);
        CreateWall("Wall_West_Fixed" + suf, new Vector3(cx - 15.4f, 2f, 0), new Vector3(0.3f, 4, ROOM_SIZE + 1), new Color(0.18f, 0.2f, 0.25f)).transform.SetParent(room.transform);

        // ---- Paredes que se fecham (dentro da casca) ----
        Transform wallEast = CreateClosingWall("Wall_East_Closing" + suf, new Vector3(cx + 14.8f, 2f, 0)).transform;
        Transform wallWest = CreateClosingWall("Wall_West_Closing" + suf, new Vector3(cx - 14.8f, 2f, 0)).transform;
        wallEast.SetParent(room.transform);
        wallWest.SetParent(room.transform);

        // ---- Luminárias ----
        for (int li = -1; li <= 1; li++)
        {
            GameObject lamp = GameObject.CreatePrimitive(PrimitiveType.Cube);
            lamp.name = "Lamp" + li + suf;
            lamp.transform.position = new Vector3(cx + li * 8, 3.95f, 0);
            lamp.transform.localScale = new Vector3(1.5f, 0.08f, 0.6f);
            Object.DestroyImmediate(lamp.GetComponent<Collider>());
            Paint(lamp, new Color(1f, 0.97f, 0.85f));
            lamp.transform.SetParent(room.transform);

            GameObject lampLight = new GameObject("LampLight" + li + suf, typeof(Light));
            lampLight.transform.position = lamp.transform.position + Vector3.down * 0.3f;
            Light pl = lampLight.GetComponent<Light>();
            pl.type = LightType.Point;
            pl.color = new Color(1f, 0.95f, 0.8f);
            pl.intensity = 5f;
            pl.range = 14f;
            lampLight.transform.SetParent(room.transform);
        }

        // ---- Título (parede norte, lido de frente) ----
        GameObject title = CreateText("Title" + suf, content.title, new Vector3(cx, 3.4f, 14.7f), 0.35f, new Color(1f, 0.85f, 0.3f));
        title.transform.SetParent(room.transform);

        // ---- Briefing do protocolo: ao entrar, congela o jogador e mostra na tela
        //      com contagem regressiva (depois some — amnésia) ----
        GameObject briefZone = new GameObject("BriefingZone" + suf);
        briefZone.transform.position = new Vector3(cx, 2, -8);
        BoxCollider briefCol = briefZone.AddComponent<BoxCollider>();
        briefCol.isTrigger = true;
        briefCol.size = new Vector3(28, 8, 12); // faixa sul, onde o jogador chega
        BriefingTrigger brief = briefZone.AddComponent<BriefingTrigger>();
        brief.briefingText = content.briefing;
        brief.seconds = 8f;
        briefZone.transform.SetParent(room.transform);

        // Eco do briefing no mundo (Modo Memória [Q]): fica na PAREDE DO FUNDO (sul),
        // atrás do ponto de spawn — não mais flutuando no meio da sala nem junto do título.
        GameObject echo = CreateText("Briefing" + suf + "_Echo", content.briefing, new Vector3(cx, 1.9f, -14.4f), 0.28f, new Color(0.45f, 0.65f, 1f));
        echo.transform.rotation = Quaternion.Euler(0f, 180f, 0f); // girada para ser lida de dentro da sala
        echo.tag = "MemoryEcho";
        echo.transform.SetParent(room.transform);

        // ---- EnclosingRoom ----
        GameObject roomCtrl = new GameObject("EnclosingRoom" + suf);
        var enclosing = roomCtrl.AddComponent<EnclosingRoom>();
        enclosing.wallA = wallEast;
        enclosing.wallB = wallWest;
        roomCtrl.transform.SetParent(room.transform);

        // ---- Teleporte (disco + feixe de luz) ----
        GameObject tele = new GameObject("Teleporter" + suf);
        tele.transform.position = new Vector3(cx, 0, 12);
        BoxCollider teleCol = tele.AddComponent<BoxCollider>();
        teleCol.isTrigger = true;
        teleCol.center = new Vector3(0, 1.2f, 0);
        teleCol.size = new Vector3(2.4f, 2.4f, 2.4f);

        GameObject disc = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        disc.name = "Disc";
        disc.transform.SetParent(tele.transform);
        disc.transform.localPosition = new Vector3(0, 0.05f, 0);
        disc.transform.localScale = new Vector3(1.8f, 0.05f, 1.8f);
        Object.DestroyImmediate(disc.GetComponent<Collider>());

        GameObject beam = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        beam.name = "Beam";
        beam.transform.SetParent(tele.transform);
        beam.transform.localPosition = new Vector3(0, 1.6f, 0);
        beam.transform.localScale = new Vector3(0.6f, 1.6f, 0.6f);
        Object.DestroyImmediate(beam.GetComponent<Collider>());

        GameObject teleLight = new GameObject("TeleLight", typeof(Light));
        teleLight.transform.SetParent(tele.transform);
        teleLight.transform.localPosition = new Vector3(0, 1.5f, 0);
        Light tl = teleLight.GetComponent<Light>();
        tl.type = LightType.Point;
        tl.color = new Color(0.6f, 0.8f, 1f);
        tl.intensity = 4f;
        tl.range = 6f;

        exitTeleporter = tele.AddComponent<Teleporter>();
        tele.transform.SetParent(room.transform);

        // ---- Sequência de resgate (as 3 ações na ordem) ----
        GameObject seqObj = new GameObject("RescueSequence" + suf);
        var sequence = seqObj.AddComponent<RescueSequence>();
        sequence.room = enclosing;
        sequence.exitTeleporter = exitTeleporter;
        seqObj.transform.SetParent(room.transform);

        // Posições: perigo / rádio / vítima
        Vector3 hazardPos = new Vector3(cx + 8, 0, 4);
        Vector3 radioPos = new Vector3(cx + 3, 0, -3);
        Vector3 victimPos = new Vector3(cx - 4, 0, 5);

        // ---- Cenário temático + objeto de perigo ----
        BuildProps(index, cx, hazardPos, room.transform);

        // ---- Rádio de emergência (em todas as salas) ----
        GameObject radio = BuildRadio(suf, radioPos);
        radio.transform.SetParent(room.transform);

        // ---- Vítima + socorrista ajudando (tenta OBJ, depois FBX, depois boneco) ----
        // Cena 1 e 3: vítima DEITADA (inconsciente / RCP). Cena 2: sentada.
        string[] victimPoses = { "Female_Standing", "Female_Sitting", "Female_Standing" };
        Quaternion[] victimRots = {
            Quaternion.Euler(-90, 160, 0), // deitada de costas
            Quaternion.Euler(0, 160, 0),   // sentada
            Quaternion.Euler(-90, 30, 0)   // deitada de costas
        };
        GameObject victim = SpawnPerson(victimPoses[index], "Victim" + suf, victimPos, victimRots[index], room.transform);
        if (victim == null)
        {
            victim = BuildPerson("Victim" + suf, new Color(0.35f, 0.5f, 0.75f), true);
            victim.transform.position = victimPos + new Vector3(0, 0.35f, 0);
            victim.transform.SetParent(room.transform);
        }

        string[] helperPoses = { "Female_PickingUp", "Female_LookingUp", "Female_Standing_CoveringEyes" };
        GameObject helper = SpawnPerson(helperPoses[index], "Helper" + suf, victimPos + new Vector3(-1.3f, 0, 1.2f), Quaternion.Euler(0, 150, 0), room.transform);
        if (helper == null)
        {
            helper = BuildPerson("Helper" + suf, new Color(0.7f, 0.55f, 0.3f), false);
            helper.transform.position = victimPos + new Vector3(-1.3f, 0, 1.2f);
            helper.transform.rotation = Quaternion.Euler(0, 150, 0);
            helper.transform.SetParent(room.transform);
        }

        // ---- As 3 ações do protocolo ----
        // A zona do perigo precisa ser maior que o colisor do objeto (senão o
        // jogador não alcança o gatilho — caso da máquina na cena 3).
        float[] hazardZoneSizes = { 4f, 4.5f, 7f };
        var step1 = MakeStep("Step1_Hazard" + suf, hazardPos, content.hazardAction, content.hazardSuccess, 1, sequence, room.transform, hazardZoneSizes[index]);
        var step2 = MakeStep("Step2_Call" + suf, radioPos, content.callAction, content.callSuccess, 1, sequence, room.transform);
        var step3 = MakeStep("Step3_Victim" + suf, victimPos, content.victimAction, content.victimSuccess, content.victimPresses, sequence, room.transform);
        sequence.steps = new InteractionStep[] { step1, step2, step3 };

        // ---- Spawn ----
        GameObject spawn = new GameObject("Spawn" + suf);
        spawn.transform.position = new Vector3(cx, 1, -10);
        spawn.transform.SetParent(room.transform);

        return spawn.transform;
    }

    // Cria a zona de interação [E] com marcador "!" flutuante.
    static InteractionStep MakeStep(string name, Vector3 pos, string action, string success, int presses, RescueSequence sequence, Transform parent, float zoneSize = 3.5f)
    {
        GameObject obj = new GameObject(name);
        obj.transform.position = pos;
        BoxCollider col = obj.AddComponent<BoxCollider>();
        col.isTrigger = true;
        col.center = new Vector3(0, 1, 0);
        col.size = new Vector3(zoneSize, 3, zoneSize);

        GameObject markerObj = new GameObject("Marker");
        markerObj.transform.SetParent(obj.transform);
        markerObj.transform.localPosition = new Vector3(0, zoneSize > 5f ? 3.6f : 2.6f, 0);
        TextMesh marker = markerObj.AddComponent<TextMesh>();
        marker.text = "!";
        marker.characterSize = 0.5f / 4f; // alta resolução (ver CreateText): nítido, sem pixelar
        marker.fontSize = 60 * 4;
        marker.anchor = TextAnchor.MiddleCenter;
        marker.color = new Color(1f, 0.85f, 0.2f);

        InteractionStep step = obj.AddComponent<InteractionStep>();
        step.sequence = sequence;
        step.actionName = action;
        step.successMessage = success;
        step.pressesRequired = presses;
        step.marker = marker;

        obj.transform.SetParent(parent);
        return step;
    }

    // Rádio de emergência: caixa com antena e luz.
    static GameObject BuildRadio(string suf, Vector3 pos)
    {
        GameObject radio = new GameObject("Radio" + suf);
        radio.transform.position = pos;

        CreateChild(radio, "Mesa", PrimitiveType.Cube, new Vector3(0, 0.5f, 0), new Vector3(1.2f, 1f, 0.8f), new Color(0.25f, 0.25f, 0.28f));
        CreateChild(radio, "Caixa", PrimitiveType.Cube, new Vector3(0, 1.25f, 0), new Vector3(0.8f, 0.5f, 0.5f), new Color(0.15f, 0.5f, 0.2f));
        CreateChild(radio, "Antena", PrimitiveType.Cylinder, new Vector3(0.3f, 1.9f, 0), new Vector3(0.04f, 0.4f, 0.04f), new Color(0.7f, 0.7f, 0.7f));
        CreateChild(radio, "Botao", PrimitiveType.Sphere, new Vector3(-0.2f, 1.3f, 0.26f), Vector3.one * 0.12f, new Color(0.9f, 0.2f, 0.2f));

        GameObject light = new GameObject("RadioLight", typeof(Light));
        light.transform.SetParent(radio.transform);
        light.transform.localPosition = new Vector3(0, 2.2f, 0);
        Light pl = light.GetComponent<Light>();
        pl.type = LightType.Point;
        pl.color = new Color(0.4f, 1f, 0.5f);
        pl.intensity = 3f;
        pl.range = 5f;

        foreach (Collider c in radio.GetComponentsInChildren<Collider>())
            Object.DestroyImmediate(c);

        return radio;
    }

    static void BuildProps(int index, float cx, Vector3 hazardPos, Transform parent)
    {
        switch (index)
        {
            case 0: // Acidente de trânsito: o perigo são os cones a posicionar
                if (SpawnModel("Cars/sedan.fbx", "CrashedCar_R0", new Vector3(cx - 7, 0, 2), Quaternion.Euler(0, 115, 0), 4f, true, parent) == null)
                    BuildCar("Car_R0", new Vector3(cx - 7, 0, 2), parent);
                SpawnModel("Cars/ambulance.fbx", "Ambulance_R0", new Vector3(cx + 9, 0, -8), Quaternion.Euler(0, -35, 0), 5f, true, parent);
                SpawnModel("Cars/police.fbx", "Police_R0", new Vector3(cx + 11, 0, 8), Quaternion.Euler(0, -130, 0), 4f, true, parent);
                SpawnModel("Cars/taxi.fbx", "Taxi_R0", new Vector3(cx - 11.5f, 0, -8), Quaternion.Euler(0, 60, 0), 4f, true, parent);
                SpawnModel("Cars/suv.fbx", "Suv_R0", new Vector3(cx + 2, 0, 12.5f), Quaternion.Euler(0, 95, 0), 4f, true, parent);
                // pilha de cones = ponto de interação do passo 1
                SpawnModel("Cars/cone.fbx", "ConePile1_R0", hazardPos + new Vector3(-0.5f, 0, 0), Quaternion.identity, 0.7f, false, parent);
                SpawnModel("Cars/cone.fbx", "ConePile2_R0", hazardPos + new Vector3(0.4f, 0, 0.3f), Quaternion.Euler(0, 40, 0), 0.7f, false, parent);
                SpawnModel("Cars/cone.fbx", "ConePile3_R0", hazardPos + new Vector3(0, 0, -0.5f), Quaternion.Euler(0, 80, 0), 0.7f, false, parent);
                // destroços espalhados (batida)
                SpawnModel("Cars/debris-tire.fbx", "Tire_R0", new Vector3(cx - 4.5f, 0, 3.5f), Quaternion.Euler(0, 60, 0), 0.8f, false, parent);
                SpawnModel("Cars/debris-door.fbx", "Door_R0", new Vector3(cx - 9, 0, 4), Quaternion.Euler(0, 200, 0), 1.2f, false, parent);
                SpawnModel("Cars/debris-bumper.fbx", "Bumper_R0", new Vector3(cx - 5, 0, 0.5f), Quaternion.Euler(0, 150, 0), 1.3f, false, parent);
                SpawnModel("Cars/debris-plate-a.fbx", "Plate1_R0", new Vector3(cx - 6, 0, 5.5f), Quaternion.Euler(0, 20, 0), 0.9f, false, parent);
                SpawnModel("Cars/debris-plate-small-a.fbx", "Plate2_R0", new Vector3(cx - 3, 0, 1.5f), Quaternion.Euler(0, 75, 0), 0.6f, false, parent);
                SpawnModel("Cars/wheel-default.fbx", "Wheel_R0", new Vector3(cx - 10.5f, 0, 0), Quaternion.Euler(0, 30, 0), 0.7f, false, parent);
                SpawnModel("Furniture/trashcan.fbx", "Trash_R0", new Vector3(cx + 13, 0, 13), Quaternion.identity, 0.9f, false, parent);
                // mobiliário urbano (postes, semáforo, barreiras de obra)
                SpawnModel("Roads/light-curved.fbx", "StreetLight1_R0", new Vector3(cx - 13, 0, 8), Quaternion.Euler(0, 90, 0), 3.8f, true, parent);
                SpawnModel("Roads/light-curved.fbx", "StreetLight2_R0", new Vector3(cx + 13, 0, -2), Quaternion.Euler(0, -90, 0), 3.8f, true, parent);
                SpawnModel("Roads/light-square-double.fbx", "TrafficLight_R0", new Vector3(cx - 2, 0, 13), Quaternion.Euler(0, 180, 0), 3.5f, true, parent);
                SpawnModel("Roads/construction-barrier.fbx", "Barrier1_R0", new Vector3(cx + 4, 0, 1), Quaternion.Euler(0, 30, 0), 1.6f, false, parent);
                SpawnModel("Roads/construction-barrier.fbx", "Barrier2_R0", new Vector3(cx + 1, 0, 5), Quaternion.Euler(0, -20, 0), 1.6f, false, parent);
                SpawnModel("Roads/construction-light.fbx", "ConstrLight_R0", new Vector3(cx + 5.5f, 0, -1.5f), Quaternion.identity, 1.1f, false, parent);
                for (int i = 0; i < 5; i++)
                    CreateProp("RoadLine" + i + "_R0", PrimitiveType.Cube, new Vector3(cx, 0.01f, -12 + i * 4), new Vector3(0.3f, 0.02f, 1.6f), Color.white, parent);
                break;

            case 1: // Cozinha: o perigo é o fogão ligado
                SpawnModel("Furniture/kitchenStove.fbx", "Stove_R1", hazardPos, Quaternion.Euler(0, 180, 0), 1.6f, true, parent);
                // chama de fogo sobre o fogão (some quando desligar? simples: fica como decoração de perigo)
                GameObject flame = CreateProp("Flame_R1", PrimitiveType.Sphere, hazardPos + new Vector3(0, 1.2f, 0), new Vector3(0.4f, 0.6f, 0.4f), new Color(1f, 0.4f, 0.05f), parent);
                Object.DestroyImmediate(flame.GetComponent<Collider>());
                GameObject flameLight = new GameObject("FlameLight_R1", typeof(Light));
                flameLight.transform.position = hazardPos + new Vector3(0, 1.6f, 0);
                Light fl = flameLight.GetComponent<Light>();
                fl.type = LightType.Point;
                fl.color = new Color(1f, 0.5f, 0.1f);
                fl.intensity = 6f;
                fl.range = 8f;
                flameLight.transform.SetParent(parent);

                SpawnModel("Furniture/kitchenFridgeLarge.fbx", "Fridge_R1", new Vector3(cx + 11.5f, 0, 5.5f), Quaternion.Euler(0, 180, 0), 2.1f, true, parent);
                SpawnModel("Furniture/kitchenCabinet.fbx", "Cabinet_R1", new Vector3(cx + 6, 0, 5.5f), Quaternion.Euler(0, 180, 0), 1.5f, true, parent);
                SpawnModel("Furniture/kitchenCabinetDrawer.fbx", "CabinetDrawer_R1", new Vector3(cx + 4.5f, 0, 5.5f), Quaternion.Euler(0, 180, 0), 1.5f, true, parent);
                SpawnModel("Furniture/kitchenSink.fbx", "Sink_R1", new Vector3(cx + 3, 0, 5.5f), Quaternion.Euler(0, 180, 0), 1.5f, true, parent);
                SpawnModel("Furniture/kitchenBar.fbx", "Bar_R1", new Vector3(cx + 1, 0, 5.5f), Quaternion.Euler(0, 180, 0), 1.5f, true, parent);
                SpawnModel("Furniture/table.fbx", "Table_R1", new Vector3(cx - 8, 0, -1), Quaternion.identity, 1.6f, true, parent);
                SpawnModel("Furniture/chair.fbx", "Chair1_R1", new Vector3(cx - 8, 0, -2.6f), Quaternion.identity, 1.1f, false, parent);
                SpawnModel("Furniture/chair.fbx", "Chair2_R1", new Vector3(cx - 9.5f, 0, -0.5f), Quaternion.Euler(0, 90, 0), 1.1f, false, parent);
                SpawnModel("Furniture/chairRounded.fbx", "Chair3_R1", new Vector3(cx - 6.5f, 0, 0.5f), Quaternion.Euler(0, -120, 0), 1.1f, false, parent);
                SpawnModel("Furniture/rugRound.fbx", "Rug_R1", new Vector3(cx - 8, 0, -1), Quaternion.identity, 4f, false, parent);
                SpawnModel("Furniture/pottedPlant.fbx", "Plant1_R1", new Vector3(cx + 13, 0, -12.5f), Quaternion.identity, 1.2f, false, parent);
                SpawnModel("Furniture/plantSmall2.fbx", "Plant2_R1", new Vector3(cx - 13, 0, 12.5f), Quaternion.identity, 0.8f, false, parent);
                SpawnModel("Furniture/lampRoundFloor.fbx", "Lamp_R1", new Vector3(cx - 13, 0, -12.5f), Quaternion.identity, 1.7f, false, parent);
                SpawnModel("Furniture/bookcaseOpen.fbx", "Bookcase_R1", new Vector3(cx - 12.5f, 0, 4), Quaternion.Euler(0, 90, 0), 1.9f, true, parent);
                SpawnModel("Furniture/trashcan.fbx", "Trash_R1", new Vector3(cx + 12.5f, 0, 1), Quaternion.identity, 0.8f, false, parent);
                // utensílios e comida (em cima dos móveis: y = altura do tampo)
                SpawnModel("Food/frying-pan.fbx", "Pan_R1", hazardPos + new Vector3(0.3f, 0.95f, -0.1f), Quaternion.Euler(0, 70, 0), 0.55f, false, parent);
                SpawnModel("Food/pan-stew.fbx", "Pot_R1", hazardPos + new Vector3(-0.35f, 0.95f, 0.1f), Quaternion.identity, 0.45f, false, parent);
                SpawnModel("Food/cutting-board.fbx", "Board_R1", new Vector3(cx + 1, 0.93f, 5.4f), Quaternion.Euler(0, 15, 0), 0.5f, false, parent);
                SpawnModel("Food/bread.fbx", "Bread_R1", new Vector3(cx + 1.2f, 0.98f, 5.3f), Quaternion.Euler(0, 40, 0), 0.35f, false, parent);
                SpawnModel("Food/knife-block.fbx", "Knives_R1", new Vector3(cx + 6.3f, 0.93f, 5.6f), Quaternion.Euler(0, 180, 0), 0.35f, false, parent);
                SpawnModel("Food/bottle-ketchup.fbx", "Ketchup_R1", new Vector3(cx + 5.7f, 0.93f, 5.4f), Quaternion.identity, 0.3f, false, parent);
                SpawnModel("Food/bottle-oil.fbx", "Oil_R1", new Vector3(cx + 5.4f, 0.93f, 5.6f), Quaternion.identity, 0.32f, false, parent);
                SpawnModel("Food/bowl.fbx", "Bowl_R1", new Vector3(cx - 8.3f, 0.78f, -0.8f), Quaternion.identity, 0.3f, false, parent);
                SpawnModel("Food/apple.fbx", "Apple_R1", new Vector3(cx - 7.7f, 0.78f, -1.2f), Quaternion.identity, 0.2f, false, parent);
                SpawnModel("Food/banana.fbx", "Banana_R1", new Vector3(cx - 8f, 0.78f, -1.5f), Quaternion.Euler(0, 60, 0), 0.28f, false, parent);
                break;

            case 2: // Galpão: o perigo é a máquina ligada
                if (SpawnModel("Cars/tractor-shovel.fbx", "Machine_R2", hazardPos, Quaternion.Euler(0, -60, 0), 4.5f, true, parent) == null)
                    CreateProp("Machine_R2", PrimitiveType.Cube, hazardPos + new Vector3(0, 0.75f, 0), new Vector3(2.5f, 1.5f, 2), new Color(0.32f, 0.32f, 0.38f), parent);
                GameObject warnLight = new GameObject("MachineWarnLight_R2", typeof(Light));
                warnLight.transform.position = hazardPos + new Vector3(0, 2.5f, 0);
                Light wl = warnLight.GetComponent<Light>();
                wl.type = LightType.Point;
                wl.color = new Color(1f, 0.3f, 0.1f);
                wl.intensity = 5f;
                wl.range = 9f;
                warnLight.transform.SetParent(parent);

                SpawnModel("Cars/truck.fbx", "Truck_R2", new Vector3(cx + 10, 0, -9), Quaternion.Euler(0, 25, 0), 6f, true, parent);
                SpawnModel("Cars/box.fbx", "Crate1_R2", new Vector3(cx - 7.5f, 0, 2.5f), Quaternion.identity, 1f, true, parent);
                SpawnModel("Cars/box.fbx", "Crate2_R2", new Vector3(cx - 6.5f, 0, 3.5f), Quaternion.Euler(0, 30, 0), 1f, true, parent);
                SpawnModel("Cars/box.fbx", "Crate3_R2", new Vector3(cx - 7f, 1.0f, 3f), Quaternion.Euler(0, 15, 0), 0.9f, false, parent);
                SpawnModel("Cars/box.fbx", "Crate4_R2", new Vector3(cx - 10f, 0, 6f), Quaternion.Euler(0, 45, 0), 1.1f, true, parent);
                SpawnModel("Furniture/cardboardBoxClosed.fbx", "Cardboard1_R2", new Vector3(cx - 12, 0, 9), Quaternion.Euler(0, 20, 0), 1f, true, parent);
                SpawnModel("Furniture/cardboardBoxOpen.fbx", "Cardboard2_R2", new Vector3(cx - 10.8f, 0, 9.5f), Quaternion.Euler(0, 70, 0), 1f, false, parent);
                SpawnModel("Furniture/cardboardBoxClosed.fbx", "Cardboard3_R2", new Vector3(cx - 11.7f, 1.0f, 9.2f), Quaternion.Euler(0, 45, 0), 0.9f, false, parent);
                BuildShelf("Shelf_R2", new Vector3(cx - 10, 0, 1), parent);
                SpawnModel("Furniture/desk.fbx", "Desk_R2", new Vector3(cx + 12.5f, 0, 3), Quaternion.Euler(0, -90, 0), 1.8f, true, parent);
                SpawnModel("Furniture/chairDesk.fbx", "DeskChair_R2", new Vector3(cx + 11, 0, 3), Quaternion.Euler(0, 90, 0), 1.1f, false, parent);
                SpawnModel("Cars/debris-tire.fbx", "Tire1_R2", new Vector3(cx + 5, 0, -5), Quaternion.identity, 0.8f, false, parent);
                SpawnModel("Cars/wheel-truck.fbx", "Tire2_R2", new Vector3(cx + 5.8f, 0, -4.3f), Quaternion.Euler(0, 50, 0), 0.9f, false, parent);
                SpawnModel("Furniture/trashcan.fbx", "Trash_R2", new Vector3(cx - 13, 0, -12.5f), Quaternion.identity, 0.9f, false, parent);
                // estoque do galpão: barris, caixotes, baú, ferramentas
                SpawnModel("Survival/barrel.fbx", "Barrel1_R2", new Vector3(cx + 13, 0, 8), Quaternion.identity, 1.2f, true, parent);
                SpawnModel("Survival/barrel-open.fbx", "Barrel2_R2", new Vector3(cx + 12, 0, 9.3f), Quaternion.Euler(0, 45, 0), 1.2f, true, parent);
                SpawnModel("Survival/barrel.fbx", "Barrel3_R2", new Vector3(cx + 12.6f, 1.1f, 8.6f), Quaternion.Euler(0, 80, 0), 1.1f, false, parent);
                SpawnModel("Survival/box-large.fbx", "BigBox_R2", new Vector3(cx - 3, 0, 12), Quaternion.Euler(0, 25, 0), 1.5f, true, parent);
                SpawnModel("Survival/chest.fbx", "Chest_R2", new Vector3(cx - 1, 0, 12.5f), Quaternion.Euler(0, 190, 0), 1.1f, true, parent);
                SpawnModel("Survival/resource-planks.fbx", "Planks_R2", new Vector3(cx + 2, 0, -12), Quaternion.Euler(0, 75, 0), 1.4f, false, parent);
                SpawnModel("Survival/tool-axe.fbx", "Axe_R2", new Vector3(cx - 9.4f, 0, 0.4f), Quaternion.Euler(0, 30, 75), 0.9f, false, parent);
                SpawnModel("Survival/bucket.fbx", "Bucket_R2", new Vector3(cx + 6.5f, 0, 7.5f), Quaternion.identity, 0.5f, false, parent);
                SpawnModel("Survival/signpost-single.fbx", "Sign_R2", new Vector3(cx - 4, 0, -13), Quaternion.Euler(0, 160, 0), 1.7f, false, parent);
                break;
        }
    }

    // Pessoa com escala UNIFORME: o fator é calculado uma vez a partir do modelo
    // "em pé" (1,70 m) e aplicado igual a todas as poses — assim sentada/abaixada/em pé
    // têm exatamente as mesmas proporções de corpo.
    static GameObject SpawnPerson(string baseName, string name, Vector3 pos, Quaternion rot, Transform parent)
    {
        foreach (string ext in new[] { "obj", "fbx" })
        {
            float factor = GetPersonScaleFactor(ext);
            if (factor <= 0) continue;
            GameObject p = SpawnModel("People/" + baseName + "." + ext, name, pos, rot, 0, false, parent, false, factor);
            if (p != null) return p;
        }
        return null;
    }

    static float GetPersonScaleFactor(string ext)
    {
        GameObject standing = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/ImportedModels/People/Female_Standing." + ext);
        if (standing == null) return -1;
        Renderer[] rends = standing.GetComponentsInChildren<Renderer>();
        if (rends.Length == 0) return -1;
        Bounds b = rends[0].bounds;
        foreach (Renderer r in rends) b.Encapsulate(r.bounds);
        if (b.size.y < 0.0001f) return -1;
        return 1.7f / b.size.y; // pessoa em pé = 1,70 m
    }

    // Mostra no Console o estado de cada modelo importante (existe? tem malha? tamanho?).
    [MenuItem("Tools/Protocolo Lembranca/Diagnosticar Modelos")]
    public static void DiagnoseModels()
    {
        string[] paths = {
            "People/Female_Sitting.obj", "People/Female_Sitting.fbx",
            "People/Female_PickingUp.obj", "People/Female_PickingUp.fbx",
            "Cars/sedan.fbx", "Cars/ambulance.fbx", "Furniture/kitchenStove.fbx"
        };
        foreach (string rel in paths)
        {
            string assetPath = "Assets/ImportedModels/" + rel;
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
            if (prefab == null) { Debug.LogWarning("NAO CARREGOU: " + assetPath); continue; }
            Renderer[] rends = prefab.GetComponentsInChildren<Renderer>();
            Bounds b = new Bounds(prefab.transform.position, Vector3.zero);
            foreach (Renderer r in rends) b.Encapsulate(r.bounds);
            Debug.Log("OK: " + assetPath + " | renderers=" + rends.Length + " | tamanho=" + b.size);
        }
    }

    // Instancia FBX/OBJ de Assets/ImportedModels/<relPath>, normaliza escala e apoia no chão.
    // scaleByHeight: usa a altura (Y) como referência em vez da maior dimensão (para pessoas).
    // Retorna null (com aviso) se o modelo não existir ou não tiver malha visível.
    static GameObject SpawnModel(string relPath, string name, Vector3 pos, Quaternion rot, float targetSize, bool addCollider, Transform parent, bool scaleByHeight = false, float absoluteScale = -1)
    {
        string assetPath = "Assets/ImportedModels/" + relPath;
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
        if (prefab == null)
        {
            Debug.LogWarning("Modelo nao encontrado (usando bloco no lugar): " + assetPath);
            return null;
        }

        GameObject inst = (GameObject)Object.Instantiate(prefab);
        inst.name = name;

        Renderer[] renderers = inst.GetComponentsInChildren<Renderer>();
        Bounds b = GetBounds(inst);
        if (renderers.Length == 0 || b.size.magnitude < 0.001f)
        {
            Debug.LogWarning("Modelo sem malha visivel (usando bloco no lugar): " + assetPath);
            Object.DestroyImmediate(inst);
            return null;
        }

        if (absoluteScale > 0)
        {
            inst.transform.localScale = Vector3.one * absoluteScale;
        }
        else
        {
            float current = scaleByHeight ? b.size.y : Mathf.Max(b.size.x, b.size.y, b.size.z);
            if (current < 0.0001f) current = 1f;
            inst.transform.localScale = Vector3.one * (targetSize / current);
        }
        inst.transform.rotation = rot;

        Bounds b2 = GetBounds(inst);
        float bottomOffset = inst.transform.position.y - b2.min.y;
        inst.transform.position = new Vector3(pos.x, pos.y + bottomOffset, pos.z);

        if (addCollider)
        {
            Bounds b3 = GetBounds(inst);
            BoxCollider col = inst.AddComponent<BoxCollider>();
            col.center = inst.transform.InverseTransformPoint(b3.center);
            Vector3 ls = inst.transform.localScale;
            col.size = new Vector3(b3.size.x / ls.x, b3.size.y / ls.y, b3.size.z / ls.z);
        }

        inst.transform.SetParent(parent);
        return inst;
    }

    static Bounds GetBounds(GameObject go)
    {
        Renderer[] renderers = go.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) return new Bounds(go.transform.position, Vector3.zero);
        Bounds b = renderers[0].bounds;
        foreach (Renderer r in renderers) b.Encapsulate(r.bounds);
        return b;
    }

    // ---------- objetos compostos de reserva (caso modelos não importem) ----------

    static GameObject BuildPerson(string name, Color clothes, bool lying)
    {
        GameObject person = new GameObject(name);

        GameObject body = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        body.name = "Body";
        body.transform.SetParent(person.transform);
        body.transform.localPosition = new Vector3(0, 0.9f, 0);
        body.transform.localScale = new Vector3(0.5f, 0.45f, 0.35f);
        Paint(body, clothes);

        GameObject head = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        head.name = "Head";
        head.transform.SetParent(person.transform);
        head.transform.localPosition = new Vector3(0, 1.55f, 0);
        head.transform.localScale = Vector3.one * 0.35f;
        Paint(head, new Color(0.87f, 0.67f, 0.55f));

        for (int s = -1; s <= 1; s += 2)
        {
            GameObject arm = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            arm.name = s < 0 ? "ArmL" : "ArmR";
            arm.transform.SetParent(person.transform);
            arm.transform.localPosition = new Vector3(0.33f * s, 0.95f, 0);
            arm.transform.localScale = new Vector3(0.13f, 0.35f, 0.13f);
            arm.transform.localRotation = Quaternion.Euler(0, 0, 20f * s);
            Paint(arm, clothes);

            GameObject leg = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            leg.name = s < 0 ? "LegL" : "LegR";
            leg.transform.SetParent(person.transform);
            leg.transform.localPosition = new Vector3(0.13f * s, 0.3f, 0);
            leg.transform.localScale = new Vector3(0.15f, 0.32f, 0.15f);
            Paint(leg, new Color(0.2f, 0.2f, 0.25f));
        }

        foreach (Collider c in person.GetComponentsInChildren<Collider>())
            Object.DestroyImmediate(c);

        if (lying)
            person.transform.rotation = Quaternion.Euler(90, 30, 0);

        return person;
    }

    static void BuildCar(string name, Vector3 pos, Transform parent)
    {
        GameObject car = new GameObject(name);
        car.transform.position = pos;
        car.transform.rotation = Quaternion.Euler(0, 25, 0);

        CreateChild(car, "Body", PrimitiveType.Cube, new Vector3(0, 0.55f, 0), new Vector3(4, 0.7f, 1.8f), new Color(0.65f, 0.08f, 0.08f));
        CreateChild(car, "Cabin", PrimitiveType.Cube, new Vector3(-0.3f, 1.15f, 0), new Vector3(2, 0.6f, 1.6f), new Color(0.5f, 0.06f, 0.06f));
        CreateChild(car, "Windshield", PrimitiveType.Cube, new Vector3(0.75f, 1.1f, 0), new Vector3(0.1f, 0.5f, 1.5f), new Color(0.6f, 0.75f, 0.85f));

        for (int x = -1; x <= 1; x += 2)
            for (int z = -1; z <= 1; z += 2)
            {
                GameObject wheel = CreateChild(car, "Wheel", PrimitiveType.Cylinder, new Vector3(1.3f * x, 0.35f, 0.95f * z), new Vector3(0.7f, 0.12f, 0.7f), new Color(0.1f, 0.1f, 0.1f));
                wheel.transform.localRotation = Quaternion.Euler(90, 0, 0);
            }

        foreach (Collider c in car.GetComponentsInChildren<Collider>())
            Object.DestroyImmediate(c);
        BoxCollider col = car.AddComponent<BoxCollider>();
        col.center = new Vector3(0, 0.8f, 0);
        col.size = new Vector3(4.2f, 1.6f, 2f);

        car.transform.SetParent(parent);
    }

    static void BuildShelf(string name, Vector3 pos, Transform parent)
    {
        GameObject shelf = new GameObject(name);
        shelf.transform.position = pos;
        for (int i = 0; i < 3; i++)
            CreateChild(shelf, "Board" + i, PrimitiveType.Cube, new Vector3(0, 0.5f + i, 0), new Vector3(1, 0.08f, 5), new Color(0.45f, 0.33f, 0.18f));
        for (int z = -1; z <= 1; z += 2)
        {
            CreateChild(shelf, "PostA", PrimitiveType.Cube, new Vector3(0.45f, 1.4f, 2.4f * z), new Vector3(0.1f, 2.8f, 0.1f), new Color(0.3f, 0.3f, 0.32f));
            CreateChild(shelf, "PostB", PrimitiveType.Cube, new Vector3(-0.45f, 1.4f, 2.4f * z), new Vector3(0.1f, 2.8f, 0.1f), new Color(0.3f, 0.3f, 0.32f));
        }
        CreateChild(shelf, "Box1", PrimitiveType.Cube, new Vector3(0, 1.85f, -1), Vector3.one * 0.6f, new Color(0.6f, 0.45f, 0.22f));
        CreateChild(shelf, "Box2", PrimitiveType.Cube, new Vector3(0, 0.9f, 1.2f), Vector3.one * 0.65f, new Color(0.55f, 0.4f, 0.2f));
        shelf.transform.SetParent(parent);
    }

    // ---------- utilidades ----------

    static GameObject CreateChild(GameObject parent, string name, PrimitiveType type, Vector3 localPos, Vector3 scale, Color color)
    {
        GameObject go = GameObject.CreatePrimitive(type);
        go.name = name;
        go.transform.SetParent(parent.transform);
        go.transform.localPosition = localPos;
        go.transform.localScale = scale;
        Paint(go, color);
        return go;
    }

    static GameObject CreateWall(string name, Vector3 pos, Vector3 scale, Color color)
    {
        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.name = name;
        wall.transform.position = pos;
        wall.transform.localScale = scale;
        Paint(wall, color);
        return wall;
    }

    static GameObject CreateClosingWall(string name, Vector3 pos)
    {
        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.name = name;
        wall.transform.position = pos;
        wall.transform.localScale = new Vector3(0.3f, 4, ROOM_SIZE);
        Paint(wall, new Color(0.5f, 0.1f, 0.1f));

        for (int i = -2; i <= 2; i += 2)
        {
            GameObject stripe = GameObject.CreatePrimitive(PrimitiveType.Cube);
            stripe.name = "Stripe" + i;
            stripe.transform.SetParent(wall.transform);
            stripe.transform.localPosition = new Vector3(0, 0, i * 0.25f);
            stripe.transform.localScale = new Vector3(1.05f, 0.08f, 0.06f);
            Object.DestroyImmediate(stripe.GetComponent<Collider>());
            Paint(stripe, new Color(0.95f, 0.8f, 0.1f));
        }
        return wall;
    }

    static GameObject CreateText(string name, string text, Vector3 pos, float charSize, Color color)
    {
        GameObject obj = new GameObject(name);
        obj.transform.position = pos;
        TextMesh tm = obj.AddComponent<TextMesh>();
        tm.text = text;
        // Renderiza a fonte em alta resolução (fontSize grande) e encolhe o caractere
        // na mesma proporção: mesmo tamanho na tela, porém sem pixelar.
        const int sharpness = 4;
        tm.characterSize = charSize / sharpness;
        tm.fontSize = 48 * sharpness;
        tm.anchor = TextAnchor.MiddleCenter;
        tm.alignment = TextAlignment.Center;
        tm.color = color;
        return obj;
    }

    static GameObject CreateProp(string name, PrimitiveType type, Vector3 pos, Vector3 scale, Color color, Transform parent)
    {
        GameObject go = GameObject.CreatePrimitive(type);
        go.name = name;
        go.transform.position = pos;
        go.transform.localScale = scale;
        Paint(go, color);
        go.transform.SetParent(parent);
        return go;
    }

    static void Paint(GameObject go, Color color)
    {
        Renderer r = go.GetComponent<Renderer>();
        if (r == null) return;
        Material mat = new Material(r.sharedMaterial);
        mat.color = color;
        r.sharedMaterial = mat;
    }

    static void SetRect(RectTransform rt, Vector2 min, Vector2 max)
    {
        rt.anchorMin = min;
        rt.anchorMax = max;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    static GameObject CreatePanel(Transform parent, string name, Color bg)
    {
        GameObject panel = new GameObject(name, typeof(RectTransform));
        panel.transform.SetParent(parent);
        Image img = panel.AddComponent<Image>();
        img.color = bg;
        SetRect(panel.GetComponent<RectTransform>(), Vector2.zero, Vector2.one);
        return panel;
    }

    static Text CreateUIText(Transform parent, string name, int fontSize, TextAnchor align, Color color, Vector2 anchorMin, Vector2 anchorMax)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform));
        obj.transform.SetParent(parent);
        Text t = obj.AddComponent<Text>();
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        t.fontSize = fontSize;
        t.alignment = align;
        t.color = color;
        SetRect(obj.GetComponent<RectTransform>(), anchorMin, anchorMax);
        return t;
    }

    static void BuildUI(GameManager gameManager)
    {
        GameObject canvasObj = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        Canvas canvas = canvasObj.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        // EventSystem: sem ele, cliques na UI não funcionam
        if (GameObject.Find("EventSystem") == null)
            new GameObject("EventSystem",
                typeof(UnityEngine.EventSystems.EventSystem),
                typeof(UnityEngine.EventSystems.StandaloneInputModule));

        // Mensagens de evento (topo, some em 4 s)
        gameManager.messageText = CreateUIText(canvasObj.transform, "MessageText", 22, TextAnchor.UpperCenter, Color.white, new Vector2(0.15f, 0.88f), new Vector2(0.85f, 0.98f));
        // Indicador de cena (canto superior esquerdo, curto)
        gameManager.objectiveText = CreateUIText(canvasObj.transform, "ObjectiveText", 20, TextAnchor.UpperLeft, new Color(0.85f, 0.9f, 1f), new Vector2(0.02f, 0.92f), new Vector2(0.2f, 0.98f));
        // Prompt de interação (centro-baixo)
        gameManager.promptText = CreateUIText(canvasObj.transform, "PromptText", 26, TextAnchor.MiddleCenter, new Color(1f, 0.9f, 0.4f), new Vector2(0.2f, 0.15f), new Vector2(0.8f, 0.23f));

        // Painel de briefing (protocolo + contagem regressiva, congela o jogador)
        GameObject briefingPanel = CreatePanel(canvasObj.transform, "BriefingPanel", new Color(0, 0, 0, 0.8f));
        Text briefingText = CreateUIText(briefingPanel.transform, "BriefingText", 30, TextAnchor.MiddleCenter, Color.white, new Vector2(0.1f, 0.35f), new Vector2(0.9f, 0.85f));
        Text countdownText = CreateUIText(briefingPanel.transform, "CountdownText", 44, TextAnchor.MiddleCenter, new Color(1f, 0.85f, 0.2f), new Vector2(0.3f, 0.12f), new Vector2(0.7f, 0.3f));
        briefingPanel.SetActive(false);
        gameManager.briefingPanel = briefingPanel;
        gameManager.briefingText = briefingText;
        gameManager.countdownText = countdownText;

        GameObject crosshair = new GameObject("Crosshair", typeof(RectTransform));
        crosshair.transform.SetParent(canvasObj.transform);
        Image cross = crosshair.AddComponent<Image>();
        cross.color = Color.white;
        RectTransform crossRect = crosshair.GetComponent<RectTransform>();
        crossRect.anchorMin = crossRect.anchorMax = new Vector2(0.5f, 0.5f);
        crossRect.sizeDelta = new Vector2(4, 4);

        // Flash de teletransporte (overlay azul, controlado pelo GameManager)
        GameObject flashObj = new GameObject("TeleportFlash", typeof(RectTransform));
        flashObj.transform.SetParent(canvasObj.transform);
        Image flashImg = flashObj.AddComponent<Image>();
        flashImg.color = new Color(0.6f, 0.85f, 1f, 0f);
        flashImg.raycastTarget = false;
        SetRect(flashObj.GetComponent<RectTransform>(), Vector2.zero, Vector2.one);
        flashObj.SetActive(false);
        gameManager.flashImage = flashImg;

        GameObject winPanel = CreatePanel(canvasObj.transform, "WinPanel", new Color(0, 0, 0, 0.9f));
        Text winText = CreateUIText(winPanel.transform, "WinText", 34, TextAnchor.MiddleCenter, Color.white, new Vector2(0.1f, 0.3f), new Vector2(0.9f, 0.7f));
        winText.text = "Memória de Gênio,\nVocê venceu";
        winPanel.SetActive(false);
        gameManager.winPanel = winPanel;
    }

    // Liga o pós-processamento do URP reaproveitando o perfil que já existe no projeto
    // (Assets/Settings/SampleSceneProfile.asset: Bloom + Tonemapping + Vignette).
    // É o acabamento "de jogo" com maior impacto visual e nenhum asset novo.
    static void SetupPostProcessing(Camera cam)
    {
        UniversalAdditionalCameraData data = cam.GetUniversalAdditionalCameraData();
        if (data != null)
        {
            data.renderPostProcessing = true;
            data.antialiasing = AntialiasingMode.SubpixelMorphologicalAntiAliasing;
            data.antialiasingQuality = AntialiasingQuality.High;
        }

        VolumeProfile profile = AssetDatabase.LoadAssetAtPath<VolumeProfile>("Assets/Settings/SampleSceneProfile.asset");
        if (profile == null)
        {
            Debug.LogWarning("Perfil de pos-processamento nao encontrado em Assets/Settings/SampleSceneProfile.asset");
            return;
        }

        // Reaproveita o "Global Volume" que cenas URP já criam; senão, cria um.
        GameObject volObj = GameObject.Find("Global Volume");
        if (volObj == null) volObj = new GameObject("Global Volume");
        Volume vol = volObj.GetComponent<Volume>();
        if (vol == null) vol = volObj.AddComponent<Volume>();
        vol.isGlobal = true;
        vol.priority = 1f;
        vol.sharedProfile = profile;
    }

    // Dá um pouco de brilho especular a uma superfície (material URP/Lit).
    static void SetSmoothness(GameObject go, float smoothness)
    {
        Renderer r = go.GetComponent<Renderer>();
        if (r != null && r.sharedMaterial != null && r.sharedMaterial.HasProperty("_Smoothness"))
            r.sharedMaterial.SetFloat("_Smoothness", smoothness);
    }

    static void EnsureTag(string tag)
    {
        SerializedObject asset = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        SerializedProperty tagsProp = asset.FindProperty("tags");
        for (int i = 0; i < tagsProp.arraySize; i++)
            if (tagsProp.GetArrayElementAtIndex(i).stringValue == tag) return;

        tagsProp.InsertArrayElementAtIndex(tagsProp.arraySize);
        tagsProp.GetArrayElementAtIndex(tagsProp.arraySize - 1).stringValue = tag;
        asset.ApplyModifiedProperties();
    }
}
