using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace richland_rpg
{

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
        
        static public int Max(int a, int b) {
             if (a > b) { return a; } 
             else { return b; }
        }
       
        static public int Min(int a, int b) {
            if (a < b) { return a; }
            else { return b; }
        }

        static public bool PointOnAxisAlignedSegment(Vector2 a, Vector2 b, Vector2 p)
        {
            int maxX = Max(a.x, b.x);
            int minX = Min(a.x, b.x);
            int minY = Max(a.y, b.y);
            int maxY = Min(a.y, b.y);

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

  enum EntityType
    {
        Player,
        Character,
        Cougar,
        Item,
        Grass,
        Rat,
    }

    struct Entity
    {
        public int id;
        public int generation;

        public int index;
        public EntityType type;
    }

    struct EntityHandle
    {
        public int id;
        public int generation;

        public bool IsValid()
        {
            if (generation == 0 || id == 0)
            {
                return false;
            }

            return true;
        }
    }


    class EntityManager
    {
        int nextID;

        List<Entity> entities = new List<Entity>(2048);
        List<int> freeList = new List<int>(256);

        List<int> entitiesToDelete = new List<int>(256);

        public EntityManager()
        {
            Entity invalidEntity = new Entity();
            entities.Add(invalidEntity);
        }

        public EntityHandle AddEntity(EntityType type, int typeIndex)
        {

            if (freeList.Count > 0)
            {
                int index = freeList[freeList.Count - 1];
                freeList.RemoveAt(freeList.Count - 1);

                Entity e = entities[index];
                e.type = type;
                e.index = typeIndex;

                entities[index] = e;

                EntityHandle handle = new EntityHandle();
                handle.id = e.id;
                handle.generation = entities[index].generation;

                return handle;
            }
            else
            {
                Entity e = new Entity();
                e.id = ++nextID;
                e.generation = 1;
                e.type = type;
                e.index = typeIndex;
                entities.Add(e);

                EntityHandle handle = new EntityHandle();
                handle.id = e.id;
                handle.generation = 1;
                return handle;
            }
        }

        public void DeleteEntity(EntityHandle handle)
        {
            if (EntityHandleIsValid(handle))
            {
                entitiesToDelete.Add(handle.id);
            }
        }

        public bool EntityHandleIsValid(EntityHandle handle)
        {
            if (handle.id > entities.Count)
            {
                return false;
            }
            Entity e = entities[handle.id];

            if (handle.generation == e.generation)
            {
                return true;
            }

            return false;
        }

        public void DeleteEntities()
        {
            for (int i = 0; i < entitiesToDelete.Count; i++)
            {
                int index = entitiesToDelete[i];
                Entity e = entities[index];

                freeList.Add(index);

                e.generation++;

                int idSwappedWith = 0;
                switch (e.type)
                {
                    case EntityType.Rat:
                        {
                            SwapRemove(rats, e.index);
                            // Look at the entity that we just swapped into the old place, 
                            // set that entities index to be e.index
                            if (rats.Count > 0)
                            {
                                idSwappedWith = rats[e.index].id;
                            }
                        }
                        break;

                }

                if (idSwappedWith > 0)
                {
                    Entity swappedWith = entities[idSwappedWith];
                    swappedWith.index = e.index;

                    entities[idSwappedWith] = swappedWith;
                }
            }

            entitiesToDelete.Clear();
        }

        public Entity GetEntity(EntityHandle handle)
        {
            Entity e = new Entity();
            if (handle.IsValid() && EntityHandleIsValid(handle))
            {
                return entities[handle.id];
            }

            return e;
        }

        public EntityHandle GetEntityHandle(int id)
        {
            EntityHandle handle;
            handle.id = id;
            handle.generation = entities[id].generation;
            return handle;
        }

        public void SwapRemove<T>(List<T> list, int index)
        {
            if (index == list.Count - 1)
            {
                list.RemoveAt(list.Count - 1);
            }
            else
            {
                list[index] = list[list.Count - 1];
                list.RemoveAt(list.Count - 1);
            }
        }
        
                public Player player;
        public List<Cougar> cougars = new List<Cougar>(64);
        public List<Grass> grass = new List<Grass>(500);
        public List<Rat> rats = new List<Rat>(128);

        public EntityHandle AddGrass(Grass g) {
            EntityHandle handle = AddEntity(EntityType.Grass, grass.Count);
            grass.Add(g);
            return handle;
        }

        public EntityHandle AddPlayer(Player p) {
            EntityHandle handle = AddEntity(EntityType.Player, 0);
            player = p;
            return handle;
        }

        public EntityHandle AddRat(Rat r) {
            EntityHandle handle = AddEntity(EntityType.Rat, 0);
            r.id = entities[handle.id].id;
            rats.Add(r);
            return handle;
        }

    }

    struct Camera {
        // @NOTE: this is assumed to be the top left of the rect defined by this and dimensions
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
            for (int i = 0; i < width * height; i++) {
                renderData[i].layer = Layer.None;
            }
        
            for (int i = 0; i < manager.grass.Count; i++) {
                Grass g = manager.grass[i];

                TryAddToRenderables(camera, g.position, g.renderable, Layer.Floor);
            }


            for (int i = 0; i < manager.rats.Count; i++) {
                Rat r = manager.rats[i];

                TryAddToRenderables(camera, r.position, r.renderable, Layer.Ground);
            }

            TryAddToRenderables(camera, manager.player.position, manager.player.renderable, Layer.Ground);

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

    struct Stats {
        public int health;
        
        public int strength;

        public void Damage(int dmg) {
            health -= dmg;
        }
    }

    struct Player {
        public EntityHandle entityHandle;

        public Vector2 position;
        public Renderable renderable;
        
        public int health;
        public int strength;

        // @TODO: weapon

        public Player(Vector2 pos) {
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
        public int id;
        public Vector2 position;
        public Renderable renderable;
        
        public Stats stats;
        
        public Rat(Vector2 pos) {
            id = 0;
            position = pos;

            renderable.symbol = 'R';

            stats = new Stats();
            stats.health = 25;
            stats.strength = 5;
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
        MyRandom random;

        EntityManager entityManager;
        Renderer renderer;
        
        EntityHandle playerHandle;

        public Game()
        {
            random = new MyRandom();
            random.Seed(1001);

            screenDimensions = new Vector2(24, 16);

            entityManager = new EntityManager();
            renderer = new Renderer(screenDimensions.x, screenDimensions.y);

            EntityHandle playerHandle;

            GenerateWorld();
        }

        void GenerateWorld() {
            GenerateGrass();

            Player p = new Player(new Vector2(5, 5));
            playerHandle = entityManager.AddPlayer(p);

            for (int i = 0; i < 2; i++) {
                Rat r = new Rat(new Vector2(5 + (5 * i), 5));

                entityManager.AddRat(r);
            }
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

        // This is gonna be a pain I think... and we'll probably have to do many functions like this which
        // branch on the type 
        void MoveEntity(EntityManager manager, EntityHandle handle, Vector2 direction) {
               Entity e = manager.GetEntity(handle);
            
               switch (e.type) {
                   case EntityType.Player : {
                       manager.player.position = GameMath.Add(manager.player.position, direction);
                   } break;
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

            public EntityHandle handleA;
            public EntityHandle handleB;

            public Collision(Vector2 posA, Vector2 posB, Vector2 dirA, Vector2 dirB, EntityHandle a, EntityHandle b)
            {
                positionA = posA;
                postionB = posB;
                directionA = dirA;
                directionB = dirB;

                handleA = a;
                handleB = b;
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
                MoveEntity(entityManager, playerHandle, movementDirection);

                Player player = entityManager.player;
                Vector2 playerPosition = player.position;

                playerPosition = GameMath.Add(playerPosition, movementDirection);

                bool playerMoved = !GameMath.Equals(movementDirection, new Vector2(0, 0));


                bool playerMoveInvalid = false;

                if (playerMoveInvalid)
                {
                    playerPosition.x -= movementDirection.x;
                    playerPosition.y -= movementDirection.y;
                }

                // Render
                Console.Clear();

                Camera camera = new Camera();
                camera.dimensions = new Vector2(screenDimensions);
                renderer.GatherRenderables(entityManager, camera);
                renderer.Render();

                for (int i = 0; i < messageCount; i++)
                {
                    messages[i].PrintMessage();
                }

                entityManager.DeleteEntities();
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
        
        static void Main(string[] args)
        {
            Game game = new Game();

            //game.GenerateObstacles(25);
            //game.PlacePlayer();
            game.Update();
        }
    }
}
