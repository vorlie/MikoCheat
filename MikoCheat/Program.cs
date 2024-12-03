using MikoCheat;
using Swed64;
using System.Numerics;
using System.Reflection.PortableExecutable;
using System.Runtime.InteropServices;
using System.Diagnostics;

// Ensure .NET 8.0 runtime is installed before continuing
await CheckRuntime.EnsureDotNet8RuntimeAsync();

if (!IsProcessRunning("cs2"))
{
    Console.WriteLine("cs2.exe is not running.");
    Console.WriteLine("Do you want to continue running the program after starting cs2? (Y/N)");

    // Wait for the user's input
    char userInput = char.ToUpper(Console.ReadKey().KeyChar);

    if (userInput == 'Y')
    {
        Console.WriteLine("\nWaiting for cs2.exe to start...");
        while (!IsProcessRunning("cs2"))
        {
            Thread.Sleep(1000); // Check every second
        }
        Console.WriteLine("cs2.exe is running! Starting program...");
    }
    else
    {
        Console.WriteLine("\nExiting...");
        Environment.Exit(0); // Terminate the application
    }
}

static bool IsProcessRunning(string processName)
{
    return Process.GetProcessesByName(processName).Length > 0;
}

// Only start the rest of the code after runtime installation is completed
Swed swed = new Swed("cs2");

IntPtr client = swed.GetModuleBase("client.dll");

Renderer renderer = new Renderer();
Thread renderThread = new Thread(new ThreadStart(renderer.Start().Wait));
renderThread.Start();

Vector2 screenSize = renderer.screenSize;

List<Entity> entities = new List<Entity>();
Entity localPlayer = new Entity();

while (true)
{
    entities.Clear();
    Console.Clear();
    IntPtr entityList = swed.ReadPointer(client, Offsets.dwEntityList);

    IntPtr listEntry = swed.ReadPointer(entityList, 0x10);

    localPlayer.pawnAddress = swed.ReadPointer(client, Offsets.dwLocalPlayerPawn);
    localPlayer.team = swed.ReadInt(localPlayer.pawnAddress, Offsets.m_iTeamNum);
    localPlayer.origin = swed.ReadVec(localPlayer.pawnAddress, Offsets.m_vOldOrigin);
    localPlayer.viewOffset = swed.ReadVec(localPlayer.pawnAddress, Offsets.m_vecViewOffset);

    // Anti-flash logic
    if (renderer.antiFlash)
    {
        float flashDuration = swed.ReadFloat(localPlayer.pawnAddress, Offsets.m_flFlashBangTime);
        if (flashDuration > 0)
        {
            swed.WriteFloat(localPlayer.pawnAddress, Offsets.m_flFlashBangTime, 0);
            Console.WriteLine("No flash for you");
        }
    }

    // Auto Bunny Hop logic
    if (renderer.autoBunnyHop)
    {
        IntPtr jumpAddress = client + Offsets.jump;
        uint fFlag = swed.ReadUInt(localPlayer.pawnAddress, Offsets.m_fFlags);
        const int SPACE_BAR = 0x20;

        const uint STANDING = 65665;
        const uint CROUCHING = 65567;

        const uint PLUS_JUMP = 65537;
        const uint MINUS_JUMP = 256;

        if (GetAsyncKeyState(SPACE_BAR) < 0)
        {
            if (fFlag == STANDING || fFlag == CROUCHING)
            {
                Thread.Sleep(1);
                swed.WriteUInt(jumpAddress, PLUS_JUMP);
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("AutoBunnyHop: +jump");
                Console.ResetColor();
            }
            else
            {
                swed.WriteUInt(jumpAddress, MINUS_JUMP);
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine("AutoBunnyHop: -jump");
                Console.ResetColor();
            }
        }

    }

    // Entity Loop
    for (int i = 0; i < 64; i++)
    {
        if (listEntry == IntPtr.Zero) continue;

        IntPtr currentController = swed.ReadPointer(listEntry, i * 0x78);

        if (currentController == IntPtr.Zero) continue;

        int pawnHandle = swed.ReadInt(currentController, Offsets.m_hPlayerPawn);

        if (pawnHandle == 0) continue;

        IntPtr listEntry2 = swed.ReadPointer(entityList, 0x8 * ((pawnHandle & 0x7FFF) >> 9) + 0x10);

        IntPtr currentPawn = swed.ReadPointer(listEntry2, 0x78 * (pawnHandle & 0x1FF));

        if (currentPawn == localPlayer.pawnAddress) continue;

        IntPtr sceneNode = swed.ReadPointer(currentPawn, Offsets.m_pGameSceneNode);

        IntPtr boneMatrix = swed.ReadPointer(sceneNode, Offsets.m_modelState + 0x80);

        float[] viewMatrix = swed.ReadMatrix(client + Offsets.dwViewMatrix);

        int health = swed.ReadInt(currentPawn, Offsets.m_iHealth);
        int team = swed.ReadInt(currentPawn, Offsets.m_iTeamNum);
        uint lifeState = swed.ReadUInt(currentPawn, Offsets.m_lifeState);

        if (lifeState != 256) continue;
        if (team == localPlayer.team && !renderer.targetTeam) continue;

        Entity entity = new Entity();

        entity.name = swed.ReadString(currentController, Offsets.m_iszPlayerName, 32).Split("\0")[0];
        entity.pawnAddress = currentPawn;
        entity.controllerAddress = currentController;
        entity.health = health;
        entity.team = team;
        entity.lifeState = lifeState;
        entity.head = swed.ReadVec(boneMatrix, 6 * 32);
        entity.head2d = Calculate.WorldToScreen(viewMatrix, entity.head, screenSize);
        entity.position = swed.ReadVec(currentPawn, Offsets.m_vOldOrigin);
        entity.viewOffset = swed.ReadVec(currentPawn, Offsets.m_vecViewOffset);
        entity.position2D = Calculate.WorldToScreen(viewMatrix, entity.position, screenSize);
        entity.viewPosition2D = Calculate.WorldToScreen(viewMatrix, Vector3.Add(entity.position, entity.viewOffset), screenSize);
        entity.distance = Vector3.Distance(entity.position, localPlayer.position);
        entity.bones = Calculate.ReadBones(boneMatrix, swed);
        entity.bones2d = Calculate.ReadBones2d(entity.bones, viewMatrix, screenSize);
        entity.pixelDistance = Vector2.Distance(entity.head2d, new Vector2(screenSize.X / 2, screenSize.Y / 2));


        entities.Add(entity);

        Console.ForegroundColor = ConsoleColor.Green;

        if (team != localPlayer.team)
        {
            Console.ForegroundColor = ConsoleColor.Red;
        }
        //Console.WriteLine($"{entity.health}hp, head coords: {entity.head}");

        Console.ResetColor();
    }
    entities = entities.OrderBy(o => o.pixelDistance).ToList();

    float smoothingFactor = 0.4f;

    // Aim-lock logic
    if (entities.Count > 0 && renderer.aimBot)
    {
        Vector3 playerView = Vector3.Add(localPlayer.origin, localPlayer.viewOffset);
        Vector3 entityView = Vector3.Add(entities[0].origin, entities[0].origin);

        if (entities[0].pixelDistance < renderer.Radius)
        {
            Vector2 targetAngles = Calculate.CalculateAngles(playerView, entities[0].head);
            Vector3 targetAnglesVec3 = new Vector3(targetAngles.Y, targetAngles.X, 0.0f);

            // Read current view angles
            Vector3 currentAngles = swed.ReadVec(client, Offsets.dwViewAngles);

            // Smooth the transition from current to target angles
            Vector3 smoothedAngles = Calculate.SmoothAngles(currentAngles, targetAnglesVec3, smoothingFactor);

            // Write the smoothed angles
            swed.WriteVec(client, Offsets.dwViewAngles, smoothedAngles);
        }
    }


    [DllImport("user32.dll")]
    static extern short GetAsyncKeyState(int vKey);

    //Console.ForegroundColor = ConsoleColor.White;
    //Console.WriteLine($"CurrentScreen: {screenSize}");
    //Console.ResetColor();

    renderer.UpdateLocalPlayer(localPlayer);
    renderer.UpdateEntities(entities);
    Thread.Sleep(2);
}
