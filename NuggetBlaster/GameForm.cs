﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using NuggetBlaster.GameCore;
using NuggetBlaster.Properties;

namespace NuggetBlaster
{
    public partial class GameForm : Form
    {
        private readonly Engine GameEngine;

        private Image     Background  = Resources.background;
        private Rectangle BackgroundRect;
        private Image     Keys        = Resources.keysWhite;
        private Rectangle KeysRect;
        private Image     Title       = Resources.nuggetBlasterTitle;
        private Rectangle TitleRect;

        private readonly bool Analytics    = true;
        private          long msDraw       = 0;
        private          long msProcessing = 0;

        private readonly double AspectRatio       = (double)16/9;
        private readonly int    CanvasEdgePadding = 10;

        private readonly Font ScoreLabelFont = new("Arial Narrow", 26.25F, FontStyle.Regular, GraphicsUnit.Point);

        public GameForm()
        {
            InitializeComponent();

            GameEngine = new Engine(this);

            GameTimer.Interval = 1000/Engine.Fps;
            GameTimer.Start();

            ResizeUI();
        }

        public Rectangle GetGameCanvasAsRectangle()
        {
            return new Rectangle(GameCanvas.Location, GameCanvas.Size);
        }

        private void GameKeyDown(object sender, KeyEventArgs e)
        {
            GameEngine.GameKeyAction(e.KeyCode.ToString(), true);
        }

        private void GameKeyUp(object sender, KeyEventArgs e)
        {
            GameEngine.GameKeyAction(e.KeyCode.ToString(), false);
        }

        private void GameTimer_Tick(object sender, EventArgs e)
        {
            long timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            GameEngine.ProcessGameTick();
            msProcessing += DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - timestamp;

            GameCanvas.Invalidate();
        }

        private void GameCanvas_Paint(object sender, PaintEventArgs e)
        {
            long timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            BackgroundRect.X = BackgroundRect.X < 0 - GameCanvas.Width ? 0 :BackgroundRect.X;
            BackgroundRect.X -= (int)(Engine.GetPPF(BackgroundRect.Width / 20) * GameEngine.TicksToProcess);
            e.Graphics.DrawImage(Background, BackgroundRect);

            string analytics = Analytics ? " ticks: " + GameEngine.TickCount.ToString() + " drawMs: " + msDraw + " processMs: " + msProcessing : "";
            e.Graphics.DrawString("Score: " + GameEngine.Score + analytics, ScoreLabelFont, new SolidBrush(Color.White), CanvasEdgePadding, CanvasEdgePadding, new StringFormat());

            if (GameEngine.IsRunning)
            {
                IDictionary<string, Image> sprites = GameEngine.GetEntitySpriteList();
                foreach (KeyValuePair<string, Rectangle> rectangle in GameEngine.GetEntityRectangleList())
                    e.Graphics.DrawImage(sprites[rectangle.Key], rectangle.Value);
            }
            else
            {
                e.Graphics.DrawImage(Title, TitleRect);
            }
            e.Graphics.DrawImage(Keys, KeysRect);
            msDraw += DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - timestamp;
        }

        private void GameForm_ResizeEnd(object sender, EventArgs e)
        {
            if (GameCanvas.Height != ClientSize.Height || GameCanvas.Width != ClientSize.Width)
                ResizeUI();
        }

        private void GameForm_Resize(object sender, EventArgs e)
        {
            ResizeUI();
        }

        private void ResizeUI()
        {
            if (ClientSize.Height == 0 || ClientSize.Width == 0)
                return;

            if (Size.Width != Size.Height * AspectRatio)
                Size = new Size(Size.Width, (int)(Size.Width / AspectRatio));

            double scaling    = ClientSize.Height / (double)GameCanvas.Height;
            GameCanvas.Height = ClientSize.Height;
            GameCanvas.Width  = ClientSize.Width;

            BackgroundRect = new Rectangle((int)(BackgroundRect.X*scaling), 0, GameCanvas.Width * 2, GameCanvas.Height);
            KeysRect       = new Rectangle((int)(GameCanvas.Width - (GameCanvas.Width * 0.2)) - CanvasEdgePadding, (int)(GameCanvas.Height - (GameCanvas.Height * 0.1)) - CanvasEdgePadding, (int)(GameCanvas.Width * 0.2), (int)(GameCanvas.Height * 0.1));
            TitleRect      = new Rectangle((int)(GameCanvas.Width / 2 - GameCanvas.Width * 0.8 / 2), (int)(GameCanvas.Height / 2 - GameCanvas.Height * 0.2 / 2), (int)(GameCanvas.Width * 0.8), (int)(GameCanvas.Height * 0.2));

            Background = ResizeImage(Resources.background, BackgroundRect);
            Keys       = ResizeImage(Resources.keysWhite, KeysRect);
            Title      = ResizeImage(Resources.nuggetBlasterTitle, TitleRect);

            GameEngine.ProcessEntityRescale(scaling);
        }

        public static Image ResizeImage(Image image, Rectangle rect)
        {
            return new Bitmap(image, rect.Size);
        }

        public static Rectangle ResizeRectangle(Rectangle rectangle, double scaling)
        {
            return new Rectangle((int)(rectangle.X*scaling), (int)(rectangle.Y*scaling), (int)(rectangle.Width*scaling), (int)(rectangle.Height*scaling));
        }
    }
}