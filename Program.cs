using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using Microsoft.DirectX.Direct3D;
using Microsoft.DirectX.DirectInput;
using System.Windows.Forms;

namespace SharpDX9App {

    class Program {
        static void Main(string[] args) {
            MyGame game = new MyGame();
            game.Run();
        }
    }

    class Paddle {
        public int PlayerId;
        public Texture Texture;
        public World World;

        public int PlayerScore = 0;

        public RectangleF Position;
        public const float NyoomsPerSecond = 512f;

        public Paddle(int player, World world, Texture texture) {
            this.PlayerId = player;
            this.World = world;
            this.Texture = texture;
            this.Position.Width = 16;
            this.Position.Height = 64;

            Position.Y = (480f / 2f) - 32;

            if (PlayerId == 1) {
                Position.X = 640 - 24;
            }
        }

        public void Update(double deltaTime, KeyboardState currentState) {
            if (PlayerId == 0) {
                if (currentState[Key.W]) Position.Y -= NyoomsPerSecond * (float)deltaTime;
                if (currentState[Key.S]) Position.Y += NyoomsPerSecond * (float)deltaTime;
            }

            if (PlayerId == 1) {
                if (currentState[Key.Up]) Position.Y -= NyoomsPerSecond * (float)deltaTime;
                if (currentState[Key.Down]) Position.Y += NyoomsPerSecond * (float)deltaTime;
            }

            if (Position.Y < 0) Position.Y = 0;
            if (Position.Y > 480 - 92) Position.Y = 480 - 92;

            PointF ballCenter = new PointF(
                World.Ball.Position.X,
                World.Ball.Position.Y
            );

            float seg = Position.Height / 3;
            RectangleF collider = new RectangleF(
                this.Position.X, this.Position.Y - seg,
                this.Position.Width, this.Position.Height+(seg*2)
            );

            if (collider.Contains(ballCenter)) {
                if (PlayerId == 0) World.Ball.Position.X = Position.Right + 4;
                if (PlayerId == 1) World.Ball.Position.X = Position.Left - 4;


                if ((new RectangleF(this.Position.X, this.Position.Y-seg, this.Position.Width, seg*2).Contains(ballCenter))) {
                    World.Ball.Bounce(0);
                }

                if ((new RectangleF(this.Position.X, this.Position.Y+seg, this.Position.Width, seg).Contains(ballCenter))) {
                    World.Ball.Bounce(1);
                }

                if ((new RectangleF(this.Position.X, this.Position.Y+seg+seg, this.Position.Width, seg*2).Contains(ballCenter))) {
                    World.Ball.Bounce(2);
                }
            }
        }


        public void Draw(Sprite sprite) {
            sprite.Draw2D(Texture, new PointF(0, 0), 0, new PointF(this.Position.X, this.Position.Y), Color.White);
        }
    }

    class Ball {
        public World World;
        public Texture Texture;
        public PointF Position;
        public PointF Velocity;
        public PointF Start;

        public const float NyoomsPerSecond = 256f;

        public Ball(World world, Texture texture) {
            this.Texture = texture;
            this.World = world;
            this.Velocity = new PointF(NyoomsPerSecond, 0);

            this.Start = new PointF(
                (640f/2f)-8,
                (480f/2)-8
            );

            this.Position = Start;
        }

        public void Update(double deltaTime) {
            this.Position.X += Velocity.X * (float)deltaTime;
            this.Position.Y += Velocity.Y * (float)deltaTime;

            if (this.Position.Y < 0) {
                this.Position.Y = 0;
                this.Velocity.Y = -this.Velocity.Y;
            }

            if (this.Position.Y > 480-32) {
                this.Position.Y = 480-32;
                this.Velocity.Y = -this.Velocity.Y;
            }

            if (Position.X < -32) {
                World.Player2.PlayerScore++;
                World.Served = false;
                this.Position = Start;
            }

            if (Position.X > 650) {
                World.Player1.PlayerScore++;
                World.Served = false;
                this.Position = Start;
            }
        }

        public void Bounce(int dir) {
            switch (dir) {
                case 0:
                    this.Velocity.X = -this.Velocity.X;
                    this.Velocity.Y = -NyoomsPerSecond;
                    break;

                case 1:
                    this.Velocity.X = -this.Velocity.X;
                    this.Velocity.Y = 0;
                    break;

                case 2:
                    this.Velocity.X = -this.Velocity.X;
                    this.Velocity.Y = NyoomsPerSecond;
                    break;

                default: break;
            }
            
        }

        public void Draw(Sprite sprite) {
            sprite.Draw2D(Texture, new PointF(8, 8), 0, Position, Color.White);
        }
    }

    class World {
        public Paddle Player1;
        public Paddle Player2;
        public Ball Ball;
        public bool Served;

        public const int ScoreToChickenDinner = 10;

        Microsoft.DirectX.Direct3D.Font dxfont;

        public World(Microsoft.DirectX.Direct3D.Device graphicsDevice) {
            this.Ball = new Ball(this, TextureLoader.FromFile(graphicsDevice, "ball.PNG"));
            this.Player1 = new Paddle(0, this, TextureLoader.FromFile(graphicsDevice, "paddle.PNG"));
            this.Player2 = new Paddle(1, this, TextureLoader.FromFile(graphicsDevice, "paddle.PNG"));
            dxfont = new Microsoft.DirectX.Direct3D.Font(graphicsDevice, new System.Drawing.Font(FontFamily.GenericMonospace, 24));
        }

        public void Update(double deltaTime, KeyboardState state) {
            if (state[Key.Space] && !Served) {
                Served = true;
                Random r = new Random();

                Ball.Velocity.Y = 0;
                if (r.Next(100) >= 50) {
                    Ball.Velocity.X = -Ball.NyoomsPerSecond;
                } else {
                    Ball.Velocity.X = Ball.NyoomsPerSecond;
                }
            }

            if (Served) Ball.Update(deltaTime);
            Player1.Update(deltaTime, state);
            Player2.Update(deltaTime, state);

            if (Player1.PlayerScore >= ScoreToChickenDinner) {
                MessageBox.Show("Player 1 Wins!");
                Environment.Exit(0);
            }

            if (Player2.PlayerScore >= ScoreToChickenDinner) {
                MessageBox.Show("Player 2 Wins!");
                Environment.Exit(0);
            }
        }

        public void Draw(Sprite sprite) {
            sprite.Begin(SpriteFlags.AlphaBlend);
            Ball.Draw(sprite);
            Player1.Draw(sprite);
            Player2.Draw(sprite);
            sprite.End();

            dxfont.DrawText(null, Player1.PlayerScore.ToString(), new Point(32, 32), Color.White);
            dxfont.DrawText(null, Player2.PlayerScore.ToString(), new Point(640-64, 32), Color.White);
        }
    }

    class MyGame : GameWindow {
        SpriteBatch spriteBatch;
        Texture gay;
        Microsoft.DirectX.DirectInput.Device keyboard;
        Microsoft.DirectX.Direct3D.Sprite gaysprite;
        double time;

        World world;

        public MyGame() : base("PÅNG") {
        }

        public override void LoadContent() {
            base.LoadContent();
            gaysprite = new Sprite(GraphicsDevice);
            world = new World(GraphicsDevice);

            keyboard = new Microsoft.DirectX.DirectInput.Device(SystemGuid.Keyboard);
            keyboard.Acquire();
        }

        public override void Update(double deltaTime) {
            base.Update(deltaTime);
            time += deltaTime;

            Console.WriteLine("{0}ms", (int)(deltaTime*1000));

            world.Update(deltaTime, keyboard.GetCurrentKeyboardState());
        }

        public override void Draw() {
            base.Draw();
            GraphicsDevice.Clear(ClearFlags.Target, Color.Black, 0, 0);

            GraphicsDevice.BeginScene();
            world.Draw(gaysprite);
            GraphicsDevice.EndScene();
        }
    }
}
