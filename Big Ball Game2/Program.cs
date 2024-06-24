using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace BallSimulation
{
    class Ball
    {
        public double Radius { get; set; }
        public (double X, double Y) Position { get; set; }
        public (int R, int G, int B) Color { get; set; }
        public (double Dx, double Dy) Velocity { get; set; }
        public string Type { get; set; } // "Regular", "Monster", "Repellent"

        public Ball(string type, double radius, (double X, double Y) position, (int R, int G, int B) color, (double Dx, double Dy) velocity)
        {
            Type = type;
            Radius = radius;
            Position = position;
            Color = color;
            Velocity = velocity;
        }
    }

    class Simulation
    {
        private List<Ball> balls;
        private double canvasWidth;
        private double canvasHeight;
        private Random random;
        private bool isRunning;

        public Simulation(int numRegular, int numMonster, int numRepellent, double width, double height)
        {
            canvasWidth = width;
            canvasHeight = height;
            random = new Random();
            balls = new List<Ball>();
            isRunning = true;

            // Initialize regular balls
            for (int i = 0; i < numRegular; i++)
            {
                balls.Add(CreateBall("Regular"));
            }

            // Initialize monster balls
            for (int i = 0; i < numMonster; i++)
            {
                balls.Add(CreateBall("Monster"));
            }

            // Initialize repellent balls
            for (int i = 0; i < numRepellent; i++)
            {
                balls.Add(CreateBall("Repellent"));
            }
        }

        private Ball CreateBall(string type)
        {
            double radius = random.NextDouble() * 10 + 5; // Radius between 5 and 15
            (double X, double Y) position = (random.NextDouble() * canvasWidth, random.NextDouble() * canvasHeight);
            (int R, int G, int B) color = (random.Next(256), random.Next(256), random.Next(256));
            (double Dx, double Dy) velocity = (type == "Monster") ? (0, 0) : (random.NextDouble() * 2 - 1, random.NextDouble() * 2 - 1);
            return new Ball(type, radius, position, color, velocity);
        }

        public void Simulate()
        {
            while (!IsSimulationFinished() && isRunning)
            {
                UpdateBalls();
                DetectCollisions();
                PrintBallsState();
                Thread.Sleep(100); // Delay to visualize simulation steps

                // Check if user pressed a key to stop the simulation
                if (Console.KeyAvailable)
                {
                    var key = Console.ReadKey(intercept: true).Key;
                    if (key == ConsoleKey.Q)
                    {
                        isRunning = false;
                    }
                }
            }
        }

        private void UpdateBalls()
        {
            foreach (var ball in balls)
            {
                if (ball.Type != "Monster")
                {
                    ball.Position = (ball.Position.X + ball.Velocity.Dx, ball.Position.Y + ball.Velocity.Dy);
                    HandleWallCollision(ball);
                }
            }
        }

        private void HandleWallCollision(Ball ball)
        {
            if (ball.Position.X <= ball.Radius || ball.Position.X >= canvasWidth - ball.Radius)
            {
                ball.Velocity = (-ball.Velocity.Dx, ball.Velocity.Dy);
            }

            if (ball.Position.Y <= ball.Radius || ball.Position.Y >= canvasHeight - ball.Radius)
            {
                ball.Velocity = (ball.Velocity.Dx, -ball.Velocity.Dy);
            }
        }

        private void DetectCollisions()
        {
            for (int i = 0; i < balls.Count; i++)
            {
                for (int j = i + 1; j < balls.Count; j++)
                {
                    if (IsColliding(balls[i], balls[j]))
                    {
                        HandleCollision(balls[i], balls[j]);
                    }
                }
            }

            // Remove balls that have been eaten
            balls = balls.Where(b => b.Radius > 0).ToList();
        }

        private bool IsColliding(Ball a, Ball b)
        {
            double dx = a.Position.X - b.Position.X;
            double dy = a.Position.Y - b.Position.Y;
            double distance = Math.Sqrt(dx * dx + dy * dy);
            return distance <= (a.Radius + b.Radius);
        }

        private void HandleCollision(Ball a, Ball b)
        {
            if (a.Type == "Regular" && b.Type == "Regular")
            {
                if (a.Radius > b.Radius)
                {
                    a.Radius += b.Radius;
                    a.Color = CombineColors(a.Color, b.Color, a.Radius, b.Radius);
                    b.Radius = 0;
                }
                else
                {
                    b.Radius += a.Radius;
                    b.Color = CombineColors(b.Color, a.Color, b.Radius, a.Radius);
                    a.Radius = 0;
                }
            }
            else if (a.Type == "Regular" && b.Type == "Monster")
            {
                b.Radius += a.Radius;
                a.Radius = 0;
            }
            else if (a.Type == "Monster" && b.Type == "Regular")
            {
                a.Radius += b.Radius;
                b.Radius = 0;
            }
            else if (a.Type == "Regular" && b.Type == "Repellent")
            {
                a.Velocity = (-a.Velocity.Dx, -a.Velocity.Dy);
                b.Color = a.Color;
            }
            else if (a.Type == "Repellent" && b.Type == "Regular")
            {
                b.Velocity = (-b.Velocity.Dx, -b.Velocity.Dy);
                a.Color = b.Color;
            }
            else if (a.Type == "Repellent" && b.Type == "Repellent")
            {
                (a.Color, b.Color) = (b.Color, a.Color);
            }
            else if (a.Type == "Repellent" && b.Type == "Monster")
            {
                a.Radius /= 2;
            }
            else if (a.Type == "Monster" && b.Type == "Repellent")
            {
                b.Radius /= 2;
            }
        }

        private (int R, int G, int B) CombineColors((int R, int G, int B) color1, (int R, int G, int B) color2, double radius1, double radius2)
        {
            int r = (int)((color1.R * radius1 + color2.R * radius2) / (radius1 + radius2));
            int g = (int)((color1.G * radius1 + color2.G * radius2) / (radius1 + radius2));
            int b = (int)((color1.B * radius1 + color2.B * radius2) / (radius1 + radius2));
            return (r, g, b);
        }

        private bool IsSimulationFinished()
        {
            return balls.Count(b => b.Type == "Regular") == 0;
        }

        private void PrintBallsState()
        {
            Console.Clear();
            foreach (var ball in balls)
            {
                Console.WriteLine($"{ball.Type} Ball - Position: ({ball.Position.X:F2}, {ball.Position.Y:F2}), Radius: {ball.Radius:F2}, Color: ({ball.Color.R}, {ball.Color.G}, {ball.Color.B})");
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Press 'Q' to stop the simulation.");
            Simulation simulation = new Simulation(numRegular: 10, numMonster: 2, numRepellent: 3, width: 500, height: 500);
            simulation.Simulate();
        }
    }
}
