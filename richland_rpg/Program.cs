﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace richland_rpg
{
    /*
       Create a struct and have them implement things using it. 
       Make their own structs and handle working with those

       Randomly generate some obstacles, maybe randomly choose a certain pattern
       Find a position to place it, but don't do everything randomly 
       Have a cursor that's moving left->right up -> down (this avoids placing things in the same spot)

       Room program could use file i/o and parsing
       <NAME = Hallway>
       <Desc>
       Blah blah bleh
       <PATH Hallway>
       <PATH Bedroom>
    */

    // Generate a world
    // Move around in open world
    // Better mapping between IDs, indices, and types
    // Put things in a camera to draw them

    /*
     * Open world
     * 
     * GAME ENGINE ARCHITECTURE
     * some game data
     *    Player (position, health)
     *    Enemies (positions, health, various)
     *    World data (buildings, dungeons, forests, fields)
     *    Some text to describe what's happening (responds to events)
     *    Characters (position, health, dialogue)
     *    Elements and effects
     *    Randomly generated
     *    Save progress (file I/O)
     * Game Loop
     *   keep track of the time
     *   input
     *   simulation (other loops, conditions, score/stats)
     *   render
     * */

    // IDK just use lists? I kinda wanna preview that they can do this themselves
    public class DynamicArray<T>
    {
        T[] items;
        int capacity;

        public int count;

        public T this[int i]
        {
            get { return items[i]; }
            set { items[i] = value; }
        }

        void EnsureCapacity()
        {
            if (count + 1 > capacity)
            {
                capacity *= 2;

                items = new T[capacity];
            }
        }

        public void Clear()
        {
            count = 0;
        }

        public void PushBack(T item)
        {
            EnsureCapacity();

            items[count] = item;
            count++;
        }

        public T PopBack()
        {
            count--;
            return items[count];
        }

        public DynamicArray(int capacity_)
        {
            capacity = capacity_;
            items = new T[capacity];
        }
    }

    // A struct is not allocated on the heap the way 
    // a class is. It is allocated on the Stack
    // The heap refers essentially to some random place in RAM
    // For instance an array of structs are going to be adjacent
    // in memory, instead of all over RAM
    struct Vector2
    {
        // Position, Direction, Rotation, Velocity
        public int x;
        public int y;

        public Vector2(int newX, int newY)
        {
            x = newX;
            y = newY;
        }

        public Vector2(Vector2 a)
        {
            x = a.x;
            y = a.y;
        }
    }

    struct Rect {
        // @NOTE: position is topLeft
        public Vector2 position;
        public Vector2 dimensions;

        Rect(Vector2 pos, Vector2 dim) {
            position = pos;
            dimensions = dim;
        }

    }

    static class GameMath
    {
        static public bool Equals(Vector2 a, Vector2 b)
        {
            return a.x == b.x && a.y == b.y;
        }

        public static bool IsZero(Vector2 a)
        {
            return a.x == 0 && a.y == 0;
        }

        static public Vector2 Add(Vector2 a, Vector2 b)
        {
            Vector2 result = new Vector2(a);
            result.x += b.x;
            result.y += b.y;
            return result;
        }

        static public Vector2 Sub(Vector2 a, Vector2 b)
        {
            Vector2 result = new Vector2(a);
            result.x -= b.x;
            result.y -= b.y;
            return result;
        }

        static public bool PointOnAxisAlignedSegment(Vector2 a, Vector2 b, Vector2 p)
        {
            int maxX;
            int minX;
            int minY;
            int maxY;

            if (a.x > b.x)
            {
                maxX = a.x;
            }
            else
            {
                maxX = b.x;
            }


            if (a.x < b.x)
            {
                minX = a.x;
            }
            else
            {
                minX = b.x;
            }

            if (a.y > b.y)
            {
                maxY = a.y;
            }
            else
            {
                maxY = b.y;
            }


            if (a.y < b.y)
            {
                minY = a.y;
            }
            else
            {
                minY = b.y;
            }


            if (p.x >= minX && p.x <= maxX &&
                p.y >= maxY && p.y <= minY)
            {
                return true;
            }

            return false;
        }


        public static bool PointInRect(Vector2 topLeft, Vector2 dimensions, Vector2 position) {
            bool withinWidth  = position.x >= topLeft.x && position.x <= topLeft.x + dimensions.x;
            bool withinHeight = position.y >= topLeft.y && position.y <= topLeft.y + dimensions.y;

            return withinWidth && withinHeight;
        }
    }

    enum EntityType {
        Player,
        Character,
        Cougar,
        Item,
        Grass,
    }

    struct Entity
    {
        public int id;

        
        public int generation;

        // @NOTE: this is the index into the array based on type
        public int index;
        public EntityType type;

        public void IncGeneration() {
            generation++;
        }

        public void SetType(EntityType newType) {
            type = newType;
        }

        public void SetIndex(int i) {
            index = i;
        }
    }

    struct EntityHandle {
        public int index;
        // @NOTE: generation 0 is an invalid handle
        public int generation;

        public bool IsValid() {
            if (generation == 0) {
                return false;
            }

            return true;
        }
    }


    class EntityManager {
        int nextID;

        List<Entity> entities = new List<Entity>(2048);
        List<int> freeList = new List<int>(256);
        List<int> entitiesToDelete = new List<int>(256);

        public EntityHandle AddEntity(EntityType type, int typeIndex) {
            if (freeList.Count > 0) {
                int index = freeList[freeList.Count - 1];

                entities[index].SetType(type);
                entities[index].SetIndex(index);
                
                EntityHandle handle = new EntityHandle();
                handle.index = typeIndex;
                handle.generation = entities[index].generation;

                return handle;
            }
            else {
                Entity e = new Entity();
                e.id = ++nextID;
                e.generation = 1;
                e.type = type;
                e.index = typeIndex;
                entities.Add(e);

                EntityHandle handle = new EntityHandle();
                handle.index = entities.Count - 1;
                handle.generation = 0;
                return handle;
            }
        }

        public void DeleteEntity(EntityHandle handle) {
            if (EntityHandleIsValid(handle)) {
                entitiesToDelete.Add(handle.index);
            }
        }

        // @NOTE: this is different from handle.IsValid() because this needs to check
        // to see if an entity of this generation still exists, which we cant do on the
        // IsValid() because we don't have a reference to the database (and dont want one)
        public bool EntityHandleIsValid(EntityHandle handle) {
            if (handle.index > entities.Count) {
                return false;
            }
            Entity e = entities[handle.index];

            if (handle.generation == e.generation) {
                return true;
            }

            return false;
        }

        public void DeleteEntities() {
            for (int i = 0; i < entitiesToDelete.Count; i++) {
                int index = entitiesToDelete[i];
                Entity e = entities[index];

                freeList.Add(index);

                // @NOTE: we increase the generation on delete so any handles
                // we have are now invalid because their generation no longer matches
                entities[index].IncGeneration();
                // @TODO: remove the objects from some array, maybe easier if we just
                // inherit from entity
            }
        }

        public Player player;
        public List<Cougar> cougars = new List<Cougar>(64);
        public List<Grass> grass = new List<Grass>(500);

        public void AddGrass(Grass g) {
            EntityHandle handle = AddEntity(EntityType.Grass, grass.Count);
            grass.Add(g);
        }
    }

    struct Camera {
        public Vector2 position;
        public Vector2 dimensions;
    }

    enum Layer {
        None,
        Floor,
        Ground,
        Sky,
    }

    class Renderer {
        int width;
        int height;
        char[] characters;

        struct RenderData {
            public Vector2 position;
            public Layer layer;
            public char c;
        }

        RenderData[] renderData;

        public Renderer(int w, int h) {
            width = w;
            height = h;
            characters = new char[(width * height) + (height * 2)];
            renderData = new RenderData[width * height];

            for (int y = 0; y < height; y++) {
                characters[y * (width + 2) + width + 0] = '\n';
                characters[y * (width + 2) + width + 1] = '\r';
            }
        }

        bool ShouldOverwriteLayer(Layer a, Layer b) {
            if (b == Layer.None) {
                return true;
            }

            if (b == Layer.Floor) {
                return a == Layer.Ground || a == Layer.Sky;
            }
            if (b == Layer.Ground) {
                return a == Layer.Sky;
            }
            
            // @NOTE: we sholud never get a tie anyway because two things at the same layer
            // shoud collide
            return false;
        }

        void TryAddToRenderables(Camera camera, Vector2 point, Renderable renderable, Layer layer) {
            if (GameMath.PointInRect(camera.position, camera.dimensions, point)) {
                // @TODO: all relative to top left of camera
                RenderData data = new RenderData();

                data.position = GameMath.Sub(point, camera.position);
                data.c = renderable.symbol;
                data.layer = layer;

                // @NOTE: this index needs to take into account the fact that we have
                // an extra character on each now for the newline
                int characterIndex = data.position.x + (data.position.y * (width + 2));
                int dataIndex = data.position.x + (data.position.y * width);

                if (ShouldOverwriteLayer(layer, renderData[dataIndex].layer)) {
                    renderData[dataIndex] = data;
                
                    characters[characterIndex] = data.c;
                }
                
            }
        }

        public void GatherRenderables(EntityManager manager, Camera camera) {

            for (int i = 0; i < manager.grass.Count; i++) {
                Grass g = manager.grass[i];

                TryAddToRenderables(camera, g.position, g.renderable, Layer.Floor);
            }

            // Set all the layers to None so it's easy to automatically override
        }

        public void Render() {
            Console.WriteLine(characters);
        }
    }

    // Given an entity we need to know where to look it up!
    // We can cast child to parent and store things that way
    // The problem is how do we go from that parent back to the child
    // So I think we need to store an enum type on the entity so we know
    // where to look it up.

    struct Renderable {
        public char symbol;
    };

    struct Player {
        public EntityHandle entityHandle;

        public Vector2 position;
        public Renderable renderable;
        
        public int health;
        public int strength;

        // @TODO: weapon

        Player(Vector2 pos) {
            position = pos;

            renderable.symbol = '@';

            health = 100;
            strength = 10;

            entityHandle = new EntityHandle();
        }

        public void Add(EntityManager entityManager) {
            entityHandle = entityManager.AddEntity(EntityType.Player, 0);
        }
    }

    struct Cougar {
        // @GACK: id like to get rid of this... not sure if it's gonna be needed
        EntityHandle entityHandle;

        public Vector2 position;
        public Renderable renderable;
        
        public int health;
        public int strength;

        Cougar(Vector2 pos) {
            position = pos;

            renderable.symbol = 'C';

            health = 150;
            strength = 20;

            entityHandle = new EntityHandle();
        }

        public void Add(EntityManager entityManager) {
            entityHandle = entityManager.AddEntity(EntityType.Cougar, entityManager.cougars.Count);
            // @TODO: need to set the index of data on the entity
            // @GACK: kinda weird we allocate a cougar in some code and then we
            // add it to the manager
            entityManager.cougars.Add(this);
        }
    }

    struct Rat {
        public Vector2 position;
        public Renderable renderable;
        
        public int health;
        public int strength;
        
        Rat(Vector2 pos) {
            position = pos;

            renderable.symbol = 'R';

            health = 25;
            strength = 5;
        }
    }

    struct Grass {
        public Vector2 position;
        public Renderable renderable;

        public Grass(Vector2 pos) {
            position = pos;

            renderable.symbol = '/';
        }
    }

    class Game
    {
        bool running = true;

        Vector2 screenDimensions;

        Vector2 playerPosition;
        int playerHealth;
        int playerXP;
        int playerStrength;

        Vector2[] ratPositions;
        int[] ratHealths;
        int[] ratIDs;

        Vector2 cougarPosition;
        int cougarHealth;
        int cougarStrength;

        int obstacleCount;
        Vector2[] obstacles;

        MyRandom random;

        int playerID;
        int cougarID;

        EntityManager entityManager;
        Renderer renderer;

        public Game()
        {
            random = new MyRandom();
            random.Seed(1001);

            screenDimensions = new Vector2(24, 16);

            entityManager = new EntityManager();
            renderer = new Renderer(screenDimensions.x, screenDimensions.y);

            playerHealth = 100;
            playerStrength = 2;

            ratPositions = new Vector2[2];
            ratHealths = new int[2];
            ratIDs = new int[2];

            ratPositions[0] = new Vector2(20, 12);
            ratHealths[0] = 15;
            ratIDs[0] = 2;

            ratPositions[1] = new Vector2(10, 6);
            ratHealths[1] = 15;

            ratIDs[1] = 3;


            cougarHealth = 125;
            cougarPosition = new Vector2(4, 4);
            cougarStrength = 5;

            playerID = 1;
            cougarID = 3;

            GenerateWorld();
        }

        void GenerateWorld() {
            GenerateGrass();
        }

        void GenerateGrass() {
            Vector2 cursorPosition = new Vector2(0, 0);
            Grass grass = new Grass(cursorPosition);

            for (int y = 0; y < screenDimensions.y; y++) {
                for (int x = 0; x < screenDimensions.x; x++) {
                    cursorPosition.x = x;
                    cursorPosition.y = y;

                    grass.position = new Vector2(cursorPosition);
                    entityManager.AddGrass(grass);
                }
            }
        }

        // An alternative to this idea is to make structs and store them in some way, it would be a MAJOR rewrite tho if we ever need dynamically added components
        /*
         * struct Grass
        {
            Vector2 position;
            Renderable renderable;
        }

        // Don't think we can inherit
        struct Rat
        {
            Vector2 position;
            int health;
            int strength;
        }

        class Actor
        {
            Renderable renderable;
            Vector2 position;
            int health;

            // but we can't just iterate over all acotrs and AddToCollisionSystem, we have to do it for each derived type
            void AddToCollisionSystem()
            {

            }
        }

        class Rat : Actor
        {

        }
        */
        // Maybe then we collect a bunch of the individual components like position into a separate buffer, 
        // that way we can be smart about what we try to update (no need to add the position of grass that isn't near fire for instance)
        // We still can write fairly generic systems for doing things like collision and rendering
        // AddGrassToCollisionSystem()
        // AddMonstersToCollisionSystem();
        // GenerateCollisions();
        // We could chunk that fairly well right? Might create problems when one chunk does something which should add collision stuff to another chunk
        // This is a really complicated way to introduce doing more complex things, like this is not necessary for them to implement more features... 

        // @BUG: the count small enough that it can fit that many things within the range of randomly generated numbers
        public void GenerateObstacles(int count)
        {
            obstacleCount = count;

            obstacles = new Vector2[count];

            for (int i = 0; i < count; i++)
            {
                obstacles[i] = new Vector2(random.Random(0, 15), random.Random(0, 15));
            }
        }

        public void PlacePlayer()
        {

            playerPosition = new Vector2(random.Random(0, 12), random.Random(0, 12));

            // @WARNING: could get infinite loop if the object density is high, and the range of player position is low
            while (ObstacleAtPosition(playerPosition, obstacles, obstacleCount))
            {
                playerPosition = new Vector2(random.Random(0, 12), random.Random(0, 12));
            }
        }

        Vector2 GetPlayerInput()
        {
            ConsoleKeyInfo keyInfo = Console.ReadKey(true);

            char c = keyInfo.KeyChar;

            Vector2 movementDirection = new Vector2(0, 0);

            if (c == 'w')
            {
                movementDirection.y--;
            }
            if (c == 's')
            {
                movementDirection.y++;
            }
            if (c == 'a')
            {
                movementDirection.x--;
            }
            if (c == 'd')
            {
                movementDirection.x++;
            }
            if (c == ' ')
            {

            }

            // Read the rest of the keys when they are holding down a key because
            // we'll get input faster than we can render
            while (Console.KeyAvailable)
            {
                Console.ReadKey(true);
            }

            return movementDirection;
        }

        struct Message
        {
            // @TODO: we should really start taking types of messages
            // for instance a Hit message,
            // a dead message,
            // XP message
            // push-back message
            String thing;
            String hitter;
            int damage;

            public Message(String thing_, String hitter_, int damage_)
            {
                thing = thing_;
                hitter = hitter_;
                damage = damage_;
            }

            public void PrintMessage()
            {
                Console.Write(thing);
                Console.Write(" was hit by ");
                Console.Write(hitter);
                Console.Write(" for ");
                Console.Write(damage);
                Console.WriteLine();
            }
        }

        bool ObstacleAtPosition(Vector2 position, Vector2[] obstacles, int obstacleCount)
        {
            for (int i = 0; i < obstacleCount; i++)
            {
                if (GameMath.Equals(position, obstacles[i]))
                {
                    return true;
                }
            }

            return false;
        }



        struct Collision
        {
            public Vector2 positionA;
            public Vector2 postionB;
            public Vector2 directionA;
            public Vector2 directionB;

            public int idA;
            public int idB;

            public Collision(Vector2 posA, Vector2 posB, Vector2 dirA, Vector2 dirB, int idA_, int idB_)
            {
                positionA = posA;
                postionB = posB;
                directionA = dirA;
                directionB = dirB;

                idA = idA_;
                idB = idB_;
            }
        }

        static class CollisionSystem
        {
            public static bool Collides(Vector2 posA, Vector2 posB, Vector2 dirA, Vector2 dirB)
            {
                if (GameMath.Equals(posA, posB))
                {
                    return true;
                }

                Vector2 posAPrev = new Vector2(GameMath.Sub(posA, dirA));

                if (GameMath.PointOnAxisAlignedSegment(posA, posAPrev, posB))
                {
                    return true;
                }

                return false;
            }
        }

        public void Update()
        {
            int messageCount = 0;
            Message[] messages = new Message[4];

            int collisionCount = 0;
            Collision[] collisions = new Collision[256];


            while (running)
            {
                messageCount = 0;
                collisionCount = 0;

                // Input

                // Player update
                Vector2 movementDirection = GetPlayerInput();

                playerPosition = GameMath.Add(playerPosition, movementDirection);

                bool playerMoved = !GameMath.Equals(movementDirection, new Vector2(0, 0));


                // Rat Update
                for (int i = 0; i < 2; i++)
                {


                }


                bool cougarMoved = false;

                Vector2 prevCougarPosition = new Vector2(cougarPosition);
                Vector2 cougarDirection = new Vector2(random.RandomInclusive(0, 0), 0);
                if (cougarDirection.x == 0)
                {
                    cougarDirection.y = random.RandomInclusive(0, 0);
                }

                cougarPosition = GameMath.Add(cougarPosition, cougarDirection);


                bool cougarHit = false;
                bool playerGainedXP = false;

                // handle case where they move into each other
                // means that they never actually occupy the same spot

                // direction (1, 0)
                // rat dir   (-1, 0)

                bool playerMoveInvalid = false;

                // IMPORTANT!!!!!!!!!!!!!!!!!!!!!
                // Collision detection the game way 
                // n^2 test
                // 100 = 10000
                // 1000 = 100000
                // 10000 = 1000000
                // Generate collisions! All we need to know is what collided where,
                // later on we'll worry about what to do

                // cougar player
                // player rat
                // rat rat
                // cougar rat
                // cougar cougar
                // cougar obstacle
                // rat obstacle
                // player obstacle

                for (int i = 0; i < 2; i++)
                {
                    Vector2 ratDirection = new Vector2();
                    if (ratHealths[i] > 0 && CollisionSystem.Collides(playerPosition, ratPositions[i], movementDirection, ratDirection))
                    {
                        collisions[collisionCount++] = new Collision(playerPosition, ratPositions[i], movementDirection, ratDirection, playerID, ratIDs[i]);
                        //collisions[collisionCount++] = new Collision(bigRatPosition, playerPosition, ratDirection, movementDirection, ratID, playerID);
                    }
                }


                if (cougarHealth > 0)
                {
                    if (GameMath.Equals(playerPosition, cougarPosition))
                    {
                        if (cougarMoved)
                        {
                            playerHealth -= 25;
                            // TODO: push player back really far based on strength
                        }

                        if (playerMoved)
                        {


                            if (cougarStrength < playerStrength)
                            {
                                cougarHealth -= 10;
                                cougarPosition = GameMath.Add(cougarPosition, movementDirection);
                            }
                            else
                            {
                                playerHealth -= 25;
                                playerMoveInvalid = true;

                                messages[messageCount++] = new Message("Player", "Cougar", 25);
                            }

                            cougarHit = true;
                        }
                    }
                    else if (GameMath.Equals(playerPosition, prevCougarPosition))
                    {
                        playerPosition = GameMath.Sub(playerPosition, movementDirection);

                        if (cougarMoved)
                        {
                            playerHealth -= 5;
                        }

                        if (playerMoved)
                        {
                            cougarHealth -= 10;

                            cougarPosition.x += movementDirection.x;
                            cougarPosition.y += movementDirection.y;

                            cougarHit = true;
                        }
                    }

                    if (cougarHealth <= 0)
                    {
                        playerXP += 50000;
                        playerGainedXP = true;
                    }
                }


                if (ObstacleAtPosition(playerPosition, obstacles, obstacleCount))
                {
                    playerMoveInvalid = true;
                }

                if (playerMoveInvalid)
                {
                    playerPosition.x -= movementDirection.x;
                    playerPosition.y -= movementDirection.y;
                }

                for (int i = 0; i < collisionCount; i++)
                {
                    Collision collision = collisions[i];

                    Vector2 ratDirection_ = new Vector2();
                    Vector2 playerDirection_ = new Vector2();

                    int ratID = 0;

                    if (collision.idA == 1)
                    {
                        playerDirection_ = collision.directionA;
                        ratDirection_ = collision.directionB;

                        ratID = collision.idB;
                    }
                    else if (collision.idB == 1)
                    {
                        playerDirection_ = collision.directionB;
                        ratDirection_ = collision.directionA;

                        ratID = collision.idA;
                    }

                    // @WARNING @BUG: 
                    // @NOTE: we're assuming that directionA is player
                    if (GameMath.IsZero(playerDirection_))
                    {
                        playerHealth -= 5;

                        messages[messageCount++] = new Message("Player", "Rat", 5);
                    }

                    if (GameMath.IsZero(ratDirection_))
                    {
                        int ratIndex = 0;
                        if (ratID == 2)
                        {
                            ratIndex = 0;
                        }
                        if (ratID == 3)
                        {
                            ratIndex = 1;
                        }

                        ratHealths[ratIndex] -= 10;


                        ratPositions[ratIndex] = GameMath.Add(ratPositions[ratIndex], movementDirection);

                        messages[messageCount++] = new Message("Rat", "Player", 10);
                    }
                }

                // Render
                Console.Clear();

                Camera camera = new Camera();
                camera.dimensions = new Vector2(screenDimensions);
                renderer.GatherRenderables(entityManager, camera);
                renderer.Render();

                // Instead of this just put things in a string and print it

                /* Vector2 cursorPosition = new Vector2(0, 0); */
                /* for (int y = 0; y < screenDimensions.y; y++) */
                /* { */
                /*     for (int x = 0; x < screenDimensions.x; x++) */
                /*     { */
                /*         cursorPosition.x = x; */
                /*         cursorPosition.y = y; */

                /*         if (GameMath.Equals(playerPosition, cursorPosition)) */
                /*         { */
                /*             Console.Write("@"); */
                /*         } */
                /*         else if (ratHealths[0] > 0 && GameMath.Equals(ratPositions[0], cursorPosition)) */
                /*         { */
                /*             Console.Write("R"); */
                /*         } */
                /*         else if (ratHealths[1] > 0 && GameMath.Equals(ratPositions[1], cursorPosition)) */
                /*         { */
                /*             Console.Write("R"); */
                /*         } */
                /*         else if (cougarHealth > 0 && GameMath.Equals(cougarPosition, cursorPosition)) */
                /*         { */
                /*             Console.Write("C"); */
                /*         } */
                /*         else if (ObstacleAtPosition(cursorPosition, obstacles, obstacleCount)) */
                /*         { */
                /*             Console.Write("#"); */
                /*         } */
                /*         else */
                /*         { */
                /*             Console.Write("."); */
                /*         } */
                /*     } */
                /*     Console.WriteLine(); */
                /* } */



                if (cougarHit)
                {
                    if (cougarHealth > 0)
                    {
                        Console.WriteLine("Cougar was barely hurt");
                    }
                }

                if (playerGainedXP)
                {
                    Console.WriteLine("Player gained 5 XP");
                }

                for (int i = 0; i < messageCount; i++)
                {
                    messages[i].PrintMessage();
                }
            }
        }
    }

    class MyRandom
    {
        int seed;
        int state;

        int multiplier = 98765;
        int increment = 100554387;

        // Only seed once per instance of class
        // public means that other "calling sites" can look
        // at the variable, or use the function, or change
        // the value of the variable, etc
        public void Seed(int n)
        {
            seed = n;
            state = seed;
        }

        public int Random()
        {
            state = state * multiplier + increment;

            int result = state;

            // Since it right shifts and only 0s will fill in the left
            // 1000 >> 3 = 0001
            // this number is guaranteed(?) to be non-negative so keep in mind
            //return (result >> 16) & 0x7FFF;
            return result;
        }

        // Method/function overloading is when the functions share the same name
        // but take different arguments
        public int Random(int upperLimit)
        {
            int result = Random() % upperLimit;

            return result;
        }

        // Note that this upper limit is exclusive, not inclusive
        public int Random(int lowerLimit, int upperLimit)
        {
            int range = upperLimit - lowerLimit;
            int result = lowerLimit + (Random() % range);

            if (result < lowerLimit)
            {
                result += range;
            }

            return result;
        }

        // six sided dice RandomInclusive(1, 6);
        public int RandomInclusive(int lowerLimit, int upperLimit)
        {
            return Random(lowerLimit, upperLimit + 1);
        }
    }

    class Program
    {  

        struct foo
        {

            int ax;
        }

  
        static void Main(string[] args)
        {
            Game game = new Game();
            //game.GenerateObstacles(25);
            //game.PlacePlayer();
            game.Update();
        }
    }
}
