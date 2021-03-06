﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using Client.MirGraphics;
using Client.MirScenes;
using S = ServerPackets;
using System.Text.RegularExpressions;
using Client.MirControls;

namespace Client.MirObjects
{
    class ItemObject : MapObject
    {
        public override ObjectType Race{
            get { return ObjectType.Item; }
        }

        public override bool Blocking
        {
            get { return false; }
        }

        public Size Size;

        public MirAnimatedControl _effect;
        public ItemGrade Grade;

        public ItemObject(uint objectID) : base(objectID)
        {
            _effect = new MirAnimatedControl()
            {
                Animated = true,
                AnimationCount = 25,
                AnimationDelay = 100,
                Index = 680,
                Library = Libraries.StateEffect,
                Loop = true,
                Blending = true,
            };
        }


        public void Load(S.ObjectItem info)
        {
            Name = info.Name;
            NameColour = info.NameColour;

            BodyLibrary = Libraries.FloorItems;

            CurrentLocation = info.Location;
            MapLocation = info.Location;
            GameScene.Scene.MapControl.AddObject(this);
            DrawFrame = info.Image;

            Size = BodyLibrary.GetTrueSize(DrawFrame);

            DrawY = CurrentLocation.Y;

            Grade = info.Grade;

        }
        public void Load(S.ObjectGold info)
        {
            Name = string.Format("Gold ({0:###,###,###})", info.Gold);


            BodyLibrary = Libraries.FloorItems;

            CurrentLocation = info.Location;
            MapLocation = info.Location;
            GameScene.Scene.MapControl.AddObject(this);

            if (info.Gold < 100)
                DrawFrame = 112;
            else if (info.Gold < 200)
                DrawFrame = 113;
            else if (info.Gold < 500)
                DrawFrame = 114;
            else if (info.Gold < 1000)
                DrawFrame = 115;
            else
                DrawFrame = 116;

            Size = BodyLibrary.GetTrueSize(DrawFrame);

            DrawY = CurrentLocation.Y;
        }
        public override void Draw()
        {
            if (BodyLibrary != null)
                BodyLibrary.Draw(DrawFrame, DrawLocation, DrawColour);
        }

        public override void Process()
        {
            DrawLocation = new Point((CurrentLocation.X - User.Movement.X + MapControl.OffSetX) * MapControl.CellWidth, (CurrentLocation.Y - User.Movement.Y + MapControl.OffSetY) * MapControl.CellHeight);
            DrawLocation.Offset((MapControl.CellWidth - Size.Width) / 2, (MapControl.CellHeight - Size.Height) / 2);
            DrawLocation.Offset(User.OffSetMove);
            DrawLocation.Offset(GlobalDisplayLocationOffset);
            FinalDrawLocation = DrawLocation;

            DisplayRectangle = new Rectangle(DrawLocation, Size);
        }
        public override bool MouseOver(Point p)
        {
            return MapControl.MapLocation == CurrentLocation;
            // return DisplayRectangle.Contains(p);
        }

        public override void DrawName()
        {
            CreateLabel(Color.Transparent, false, true);

            if (NameLabel == null) return;
            NameLabel.Location = new Point(
                DisplayRectangle.X + (DisplayRectangle.Width - NameLabel.Size.Width) / 2,
                DisplayRectangle.Y + (DisplayRectangle.Height - NameLabel.Size.Height) / 2 - 20);
            NameLabel.Draw();
        }
        public void DrawGradeEff()
        {
           if (Grade  > ItemGrade.Common)
           {
               switch (Grade)
               {
                   case ItemGrade.Rare:
                       _effect.BackColour = Color.DeepSkyBlue;
                       _effect.ForeColour = Color.DeepSkyBlue;
                       break;
                   case ItemGrade.Legendary:
                       _effect.BackColour = Color.DarkOrange;
                       _effect.ForeColour = Color.DarkOrange;
                       break;
                   case ItemGrade.Mythical:
                       _effect.BackColour = Color.Purple;
                       _effect.ForeColour = Color.Purple;
                       break;
               }
           
               if (Size.Width > 30)
               {
                   _effect.Index = 840;
                   _effect.AnimationCount = 9;
                   _effect.AnimationDelay = 150;
                   Libraries.StateEffect.DrawBlend(0 + _effect.Index, new Point(DrawLocation.X - (Size.Width / 2) + 10, DrawLocation.Y + (Size.Height / 2) + 20), _effect.BackColour, true, 10F);
               }
               else
               {
                   _effect.Index = 680;
                   _effect.AnimationCount = 25;
                   Libraries.StateEffect.DrawBlend(0 + _effect.Index, new Point(DrawLocation.X + (Size.Width / 2), DrawLocation.Y + (Size.Height / 2)), _effect.BackColour, true, 10F);
               }
            }
        }

        public override void DrawBehindEffects(bool effectsEnabled)
        {
        }

        public override void DrawEffects(bool effectsEnabled)
        {
            

        }

        public void DrawName(int y)
        {
            CreateLabel(Color.FromArgb(100, 0, 24, 48), true, false);

            NameLabel.Location = new Point(
                DisplayRectangle.X + (DisplayRectangle.Width - NameLabel.Size.Width) / 2,
                DisplayRectangle.Y + y + (DisplayRectangle.Height - NameLabel.Size.Height) / 2 - 20);
            NameLabel.Draw();
        }

        private void CreateLabel(Color backColour, bool border, bool outline)
        {
            NameLabel = null;

            for (int i = 0; i < LabelList.Count; i++)
            {
                if (LabelList[i].Text != Name || LabelList[i].Border != border || LabelList[i].BackColour != backColour || LabelList[i].ForeColour != NameColour || LabelList[i].OutLine != outline) continue;
                NameLabel = LabelList[i];
                break;
            }
            if (NameLabel != null && !NameLabel.IsDisposed) return;

            NameLabel = new MirControls.MirLabel
            {
                AutoSize = true,
                BorderColour = Color.Black,
                BackColour = backColour,
                ForeColour = NameColour,
                OutLine = outline,
                Border = border,
                Text = Regex.Replace(Name, @"\d+$", string.Empty),
            };

            LabelList.Add(NameLabel);
        }


    }
}
