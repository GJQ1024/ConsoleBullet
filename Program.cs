using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Game
{
    class Program
    {
        #region 游戏常量
        const int ConsoleWidth = 120;
        const int ConsoleHeight = 30;
        const int PlayerSpeed = 1;
        const int BossSpeed = 1;
        const int BulletSpeed = 2;
        const int BossBulletSpeed = 1;
        const int PlayerBulletCooldown = 300;
        const int InitialBossBulletCooldown = 1000;
        const int BossBehaviorChangeInterval = 15000; // 15秒改变行为
        #endregion

        #region// 游戏状态
        static int playerX = ConsoleWidth / 2;
        static int playerY = ConsoleHeight - 5;
        static int bossX = ConsoleWidth / 2;
        static int bossY = 5;
        static int playerHp = 100;
        static int bossHp = 500;
        static int playerAttackPower = 10;
        static int playerAttackSpeed = 300;
        static int lastPlayerBulletTime = 0;
        static int lastBossBulletTime = 0;
        static int lastBehaviorChangeTime = 0;
        static int lastDifficultyIncreaseTime = 0;
        static int bossBulletCooldown = InitialBossBulletCooldown;
        static bool gameOver = false;
        static bool gameWon = false;
        static string currentBehavior = "Normal";
        #endregion

        #region 道具系统
        static List<Item> items = new List<Item>();
        static int itemSpawnInterval = 10000; // 每10秒生成一个道具
        static int lastItemSpawnTime = 0;
        #endregion

        // 子弹系统
        static List<Bullet> bullets = new List<Bullet>();

        // 随机数生成器
        static Random random = new Random();

        // 主游戏循环
        static void Main(string[] args)
        {
            Console.CursorVisible = false;
            Console.SetWindowSize(ConsoleWidth, ConsoleHeight);
            Console.SetBufferSize(ConsoleWidth, ConsoleHeight);

            // 游戏主循环
            while (!gameOver && !gameWon)
            {
                int currentTime = Environment.TickCount;

                // 处理输入
                HandleInput();

                // 更新游戏状态
                UpdateBossBehavior(currentTime);
                UpdateBoss(currentTime);
                UpdatePlayer(currentTime);
                UpdateBullets();
                UpdateItems(currentTime);
                CheckCollisions();
                CheckDifficultyIncrease(currentTime);

                // 渲染游戏
                Render();

                // 控制游戏速度
                Thread.Sleep(16); // 约60 FPS
            }

            // 游戏结束
            Console.Clear();
            if (gameWon)
            {
                Console.WriteLine("恭喜你击败了BOSS！");
            }
            else
            {
                Console.WriteLine("游戏结束！");
            }
            Console.WriteLine("按回车退出...");

            Console.ReadLine();
        }

        // 处理玩家输入
        static void HandleInput()
        {
            if (Console.KeyAvailable)
            {
                ConsoleKeyInfo key = Console.ReadKey(true);
                switch (key.Key)
                {
                    case ConsoleKey.LeftArrow:
                        if (playerX > 0) playerX -= PlayerSpeed;
                        break;
                    case ConsoleKey.RightArrow:
                        if (playerX < ConsoleWidth - 1) playerX += PlayerSpeed;
                        break;
                    case ConsoleKey.UpArrow:
                        if (playerY > 0) playerY -= PlayerSpeed;
                        break;
                    case ConsoleKey.DownArrow:
                        if (playerY < ConsoleHeight - 1) playerY += PlayerSpeed;
                        break;
                    case ConsoleKey.Spacebar:
                        ShootPlayerBullet();
                        break;
                }
            }
        }

        // 玩家射击
        static void ShootPlayerBullet()
        {
            int currentTime = Environment.TickCount;
            if (currentTime - lastPlayerBulletTime > playerAttackSpeed)
            {
                bullets.Add(new Bullet(playerX, playerY - 1, 0, -BulletSpeed, true, playerAttackPower));
                lastPlayerBulletTime = currentTime;
            }
        }

        // 更新BOSS行为
        static void UpdateBossBehavior(int currentTime)
        {
            if (currentTime - lastBehaviorChangeTime > BossBehaviorChangeInterval)
            {
                string[] behaviors = { "Normal", "Aggressive", "Defensive", "Circling", "Predictive" };
                currentBehavior = behaviors[random.Next(behaviors.Length)];
                lastBehaviorChangeTime = currentTime;
            }
        }

        // 更新BOSS
        static void UpdateBoss(int currentTime)
        {
            // 根据行为模式移动BOSS
            switch (currentBehavior)
            {
                case "Normal":
                    // 普通追踪行为，保持适当距离
                    if (Math.Abs(bossX - playerX) > 10)
                    {
                        if (bossX < playerX) bossX += BossSpeed;
                        else if (bossX > playerX) bossX -= BossSpeed;
                    }
                    if (Math.Abs(bossY - playerY) > 10)
                    {
                        if (bossY < playerY) bossY += BossSpeed;
                        else if (bossY > playerY) bossY -= BossSpeed;
                    }
                    break;

                case "Aggressive":
                    // 激进模式，快速接近玩家
                    if (bossX < playerX) bossX += BossSpeed * 2;
                    else if (bossX > playerX) bossX -= BossSpeed * 2;
                    if (bossY < playerY) bossY += BossSpeed * 2;
                    else if (bossY > playerY) bossY -= BossSpeed * 2;
                    break;

                case "Defensive":
                    // 防御模式，保持距离
                    if (Math.Abs(bossX - playerX) < 15)
                    {
                        if (bossX < playerX) bossX -= BossSpeed;
                        else if (bossX > playerX) bossX += BossSpeed;
                    }
                    if (Math.Abs(bossY - playerY) < 15)
                    {
                        if (bossY < playerY) bossY -= BossSpeed;
                        else if (bossY > playerY) bossY += BossSpeed;
                    }
                    break;

                case "Circling":
                    // 环绕模式，围绕玩家移动
                    double angle = Math.Atan2(playerY - bossY, playerX - bossX);
                    bossX = (int)(playerX + Math.Cos(angle + Math.PI / 4) * 15);
                    bossY = (int)(playerY + Math.Sin(angle + Math.PI / 4) * 15);
                    break;

                case "Predictive":
                    // 预测模式，预测玩家移动方向
                    int predictedX = playerX;
                    int predictedY = playerY;
                    if (Console.KeyAvailable)
                    {
                        ConsoleKeyInfo key = Console.ReadKey(true);
                        switch (key.Key)
                        {
                            case ConsoleKey.LeftArrow:
                                predictedX -= PlayerSpeed * 5;
                                break;
                            case ConsoleKey.RightArrow:
                                predictedX += PlayerSpeed * 5;
                                break;
                            case ConsoleKey.UpArrow:
                                predictedY -= PlayerSpeed * 5;
                                break;
                            case ConsoleKey.DownArrow:
                                predictedY += PlayerSpeed * 5;
                                break;
                        }
                    }
                    if (bossX < predictedX) bossX += BossSpeed;
                    else if (bossX > predictedX) bossX -= BossSpeed;
                    if (bossY < predictedY) bossY += BossSpeed;
                    else if (bossY > predictedY) bossY -= BossSpeed;
                    break;
            }

            // 限制BOSS在屏幕内
            bossX = Math.Max(0, Math.Min(ConsoleWidth - 1, bossX));
            bossY = Math.Max(0, Math.Min(ConsoleHeight / 2, bossY));

            // BOSS射击
            if (currentTime - lastBossBulletTime > bossBulletCooldown)
            {
                ShootBossBullet();
                lastBossBulletTime = currentTime;
            }
        }

        // BOSS射击
        static void ShootBossBullet()
        {
            switch (currentBehavior)
            {
                case "Normal":
                    // 普通射击，朝向玩家
                    double angle = Math.Atan2(playerY - bossY, playerX - bossX);
                    int dx = (int)(Math.Cos(angle) * BossBulletSpeed);
                    int dy = (int)(Math.Sin(angle) * BossBulletSpeed);
                    bullets.Add(new Bullet(bossX, bossY + 1, dx, dy, false, 5));
                    break;

                case "Aggressive":
                    // 三连发弹幕
                    for (int i = -1; i <= 1; i++)
                    {
                        angle = Math.Atan2(playerY - bossY, playerX - bossX) + i * 0.2;
                        dx = (int)(Math.Cos(angle) * BossBulletSpeed);
                        dy = (int)(Math.Sin(angle) * BossBulletSpeed);
                        bullets.Add(new Bullet(bossX, bossY + 1, dx, dy, false, 5));
                    }
                    break;

                case "Defensive":
                    // 扇形弹幕
                    for (int i = -2; i <= 2; i++)
                    {
                        angle = Math.PI / 2 + i * 0.3;
                        dx = (int)(Math.Cos(angle) * BossBulletSpeed);
                        dy = (int)(Math.Sin(angle) * BossBulletSpeed);
                        bullets.Add(new Bullet(bossX, bossY + 1, dx, dy, false, 5));
                    }
                    break;

                case "Circling":
                    // 环形弹幕
                    for (int i = 0; i < 8; i++)
                    {
                        angle = i * Math.PI / 4;
                        dx = (int)(Math.Cos(angle) * BossBulletSpeed);
                        dy = (int)(Math.Sin(angle) * BossBulletSpeed);
                        bullets.Add(new Bullet(bossX, bossY + 1, dx, dy, false, 5));
                    }
                    break;

                case "Predictive":
                    // 预测弹幕，预测玩家位置
                    int predictedX = playerX;
                    int predictedY = playerY;
                    if (Console.KeyAvailable)
                    {
                        ConsoleKeyInfo key = Console.ReadKey(true);
                        switch (key.Key)
                        {
                            case ConsoleKey.LeftArrow:
                                predictedX -= PlayerSpeed * 10;
                                break;
                            case ConsoleKey.RightArrow:
                                predictedX += PlayerSpeed * 10;
                                break;
                            case ConsoleKey.UpArrow:
                                predictedY -= PlayerSpeed * 10;
                                break;
                            case ConsoleKey.DownArrow:
                                predictedY += PlayerSpeed * 10;
                                break;
                        }
                    }
                    angle = Math.Atan2(predictedY - bossY, predictedX - bossX);
                    dx = (int)(Math.Cos(angle) * BossBulletSpeed);
                    dy = (int)(Math.Sin(angle) * BossBulletSpeed);
                    bullets.Add(new Bullet(bossX, bossY + 1, dx, dy, false, 5));
                    break;
            }
        }

        // 更新玩家
        static void UpdatePlayer(int currentTime)
        {
            // 玩家逻辑在HandleInput中处理
        }

        // 更新子弹
        static void UpdateBullets()
        {
            for (int i = bullets.Count - 1; i >= 0; i--)
            {
                Bullet bullet = bullets[i];
                bullet.X += bullet.Dx;
                bullet.Y += bullet.Dy;

                // 移除超出屏幕的子弹
                if (bullet.X < 0 || bullet.X >= ConsoleWidth || bullet.Y < 0 || bullet.Y >= ConsoleHeight)
                {
                    bullets.RemoveAt(i);
                }
            }
        }

        // 更新道具
        static void UpdateItems(int currentTime)
        {
            // 生成新道具
            if (currentTime - lastItemSpawnTime > itemSpawnInterval)
            {
                int itemType = random.Next(3);
                string itemName = "";
                ConsoleColor color = ConsoleColor.White;

                switch (itemType)
                {
                    case 0:
                        itemName = "Health";
                        color = ConsoleColor.Green;
                        break;
                    case 1:
                        itemName = "Attack";
                        color = ConsoleColor.Red;
                        break;
                    case 2:
                        itemName = "Speed";
                        color = ConsoleColor.Yellow;
                        break;
                }

                items.Add(new Item(random.Next(ConsoleWidth), random.Next(ConsoleHeight / 2, ConsoleHeight), itemName, color));
                lastItemSpawnTime = currentTime;
            }

            // 移除已拾取的道具
            for (int i = items.Count - 1; i >= 0; i--)
            {
                if (!items[i].IsAlive)
                {
                    items.RemoveAt(i);
                }
            }
        }

        // 检查碰撞
        static void CheckCollisions()
        {
            // 检查子弹碰撞
            for (int i = bullets.Count - 1; i >= 0; i--)
            {
                Bullet bullet = bullets[i];
                if (bullet.IsPlayerBullet)
                {
                    // 玩家子弹击中BOSS
                    if (Math.Abs(bullet.X - bossX) < 2 && Math.Abs(bullet.Y - bossY) < 2)
                    {
                        bossHp -= bullet.Damage;
                        bullets.RemoveAt(i);
                        if (bossHp <= 0)
                        {
                            gameWon = true;
                        }
                    }
                }
                else
                {
                    // BOSS子弹击中玩家
                    if (Math.Abs(bullet.X - playerX) < 1 && Math.Abs(bullet.Y - playerY) < 1)
                    {
                        playerHp -= bullet.Damage;
                        bullets.RemoveAt(i);
                        if (playerHp <= 0)
                        {
                            gameOver = true;
                        }
                    }
                }
            }

            // 检查道具拾取
            for (int i = items.Count - 1; i >= 0; i--)
            {
                Item item = items[i];
                if (Math.Abs(item.X - playerX) < 2 && Math.Abs(item.Y - playerY) < 2)
                {
                    switch (item.Name)
                    {
                        case "Health":
                            playerHp = Math.Min(100, playerHp + 20);
                            break;
                        case "Attack":
                            playerAttackPower += 5;
                            break;
                        case "Speed":
                            playerAttackSpeed = Math.Max(100, playerAttackSpeed - 50);
                            break;
                    }
                    item.IsAlive = false;
                }
            }
        }

        // 检查难度增加
        static void CheckDifficultyIncrease(int currentTime)
        {
            if (currentTime - lastDifficultyIncreaseTime > 30000) // 每30秒增加难度
            {
                bossBulletCooldown = Math.Max(200, bossBulletCooldown - 100);
                lastDifficultyIncreaseTime = currentTime;
            }
        }

        // 渲染游戏
        static void Render()
        {
            Console.Clear();

            // 绘制玩家
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.SetCursorPosition(playerX, playerY);
            Console.Write("P");

            // 绘制BOSS
            Console.ForegroundColor = ConsoleColor.Red;
            Console.SetCursorPosition(bossX, bossY);
            Console.Write("B");

            // 绘制子弹
            foreach (Bullet bullet in bullets)
            {
                if (bullet.IsPlayerBullet)
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Magenta; // 更醒目的颜色
                }
                Console.SetCursorPosition(bullet.X, bullet.Y);
                Console.Write(bullet.IsPlayerBullet ? "|" : "o");
            }

            // 绘制道具
            foreach (Item item in items)
            {
                Console.ForegroundColor = item.Color;
                Console.SetCursorPosition(item.X, item.Y);
                Console.Write(item.Name[0]);
            }

            // 绘制UI
            Console.ForegroundColor = ConsoleColor.White;
            Console.SetCursorPosition(0, 0);
            Console.Write($"玩家HP: {playerHp}  BOSS HP: {bossHp}  玩家攻击力: {playerAttackPower}  玩家攻速: {playerAttackSpeed}ms");
            Console.SetCursorPosition(0, 1);
            Console.Write($"玩家弹幕: {bullets.Count(b => b.IsPlayerBullet && b.IsAlive)}  BOSS弹幕: {bullets.Count(b => !b.IsPlayerBullet && b.IsAlive)}  行为模式: {currentBehavior}");
        }
    }

    // 子弹类
    class Bullet
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int Dx { get; set; }
        public int Dy { get; set; }
        public bool IsPlayerBullet { get; set; }
        public int Damage { get; set; }
        public bool IsAlive { get; set; } = true;

        public Bullet(int x, int y, int dx, int dy, bool isPlayerBullet, int damage)
        {
            X = x;
            Y = y;
            Dx = dx;
            Dy = dy;
            IsPlayerBullet = isPlayerBullet;
            Damage = damage;
        }
    }

    // 道具类
    class Item
    {
        public int X { get; set; }
        public int Y { get; set; }
        public string Name { get; set; }
        public ConsoleColor Color { get; set; }
        public bool IsAlive { get; set; } = true;

        public Item(int x, int y, string name, ConsoleColor color)
        {
            X = x;
            Y = y;
            Name = name;
            Color = color;
        }
    }
}
