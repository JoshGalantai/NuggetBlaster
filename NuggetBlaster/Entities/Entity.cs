﻿using NuggetBlaster.Properties;
using System;
using System.Drawing;

namespace NuggetBlaster.Entities
{
    public abstract class Entity
    {
        public virtual int BaseSpeed { get; set; } = 400;
        public virtual int MaxSpeed { get; set; } = 0;
        public virtual bool MoveRight { get; set; } = false;
        public virtual bool MoveLeft { get; set; } = false;
        public virtual bool MoveUp { get; set; } = false;
        public virtual bool MoveDown { get; set; } = false;
        public virtual bool Spacebar { get; set; } = false;
        public virtual int Team { get; set; } = 0;
        public virtual bool CanShoot { get; set; } = false;
        public virtual bool Damageable { get; set; } = true;
        public virtual int HitPoints { get; set; } = 1;
        public virtual long ShootCooldownTimer { get; set; } = 0;
        public virtual int ShootCooldownMS { get; set; } = 1500;
        public virtual Image Sprite { get; set; } = Resources.pickle;
        public virtual Rectangle SpriteRectangle { get; set; } = new Rectangle(0, 0, 0, 0);
        public virtual int PointsOnKill { get; set; } = 0;

        public Entity(Rectangle spriteRectangle, Image sprite = null)
        {
            MaxSpeed        = BaseSpeed;
            SpriteRectangle = spriteRectangle;
            if (sprite != null)
                Sprite = sprite;
        }

        public abstract ProjectileEntity Shoot();

        public void ShootCooldown()
        {
            ShootCooldownTimer = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + ShootCooldownMS;
        }

        public virtual bool CheckCanShoot()
        {
            return CanShoot && DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() > ShootCooldownTimer;
        }

        public void CalculateMovement(Rectangle bounds)
        {
            Point location = SpriteRectangle.Location;
            location.X += (MoveRight ? GameCore.Engine.GetPPF(GetSpeed()) : 0) - (MoveLeft ? GameCore.Engine.GetPPF(GetSpeed()) : 0);
            location.Y += (MoveDown  ? GameCore.Engine.GetPPF(GetSpeed()) : 0) - (MoveUp   ? GameCore.Engine.GetPPF(GetSpeed()) : 0);
            ProcessMovement(bounds, new Rectangle(location, SpriteRectangle.Size));
        }

        public virtual void ProcessMovement(Rectangle bounds, Rectangle proposedRectangle)
        {
            SpriteRectangle = proposedRectangle;
        }

        public double GetSpeed()
        {
            // If Entity is moving diagonally we must reduce speed for movement calculations. (1 up/down & 1 left/right is greater than 1 total unit of distance)
            double speed = ((Convert.ToInt32(MoveRight) + Convert.ToInt32(MoveLeft) + Convert.ToInt32(MoveUp) + Convert.ToInt32(MoveDown)) == 2) ? MaxSpeed / Math.Sqrt(2) : MaxSpeed;

            return Math.Round((double)speed, 0, MidpointRounding.ToEven);
        }
    }
}