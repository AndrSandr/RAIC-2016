using Com.CodeGame.CodeWizards2016.DevKit.CSharpCgdk.Model;
using System;
using System.Threading;
using System.Globalization;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;



namespace Com.CodeGame.CodeWizards2016.DevKit.CSharpCgdk
{
    public sealed class MyStrategy : IStrategy
    {
         //VisualClient vc;
        static int backMove = 0;

        static Tree testTree;

        static long errorIdTarget;

        static int[,] myWord;

        static int[,] fieldDamage;
        static int[,] fieldScore;

        int lenField = 20;
        int stepField = 10;

        static int lenWord = 50;
        static int stepWord = 80;

        static bool isMiddleTower1;
        static bool isMiddleTower2;

        static bool isTopTower1;
        static bool isTopTower2;

        static bool isBottomTower1;
        static bool isBottomTower2;

        static LaneType myLane;

        static myLivingUnit targetUnit;
        static myLivingUnit staffTargetUnit;

        static int cntEnemy;
        static myPoint mapPoint;

        static myLivingUnit dangerUnit;

        static myLivingUnit[] livingUnits;
        static myPoint[] wayPoints;

        List<myLivingUnit> longTargets;

        static myPoint[] safePoints;


        static bool gotoBonus;
        static int pause;
        static List<myPoint> Trace;

        myBonus bonus1;
        myBonus bonus2;
        myPoint bonus;

        static double speedFactor;
        static Faction enemy;

        static bool isTop1, isBottom1;
        static Wizard dangerWizard;
        static List<myProjective> projectives;
        static double lifeTron;
        static myPoint[] midWay;

        

        public class myProjective
        {
            public double startX, startY;
            public long Id;
            public double endX, endY;
            public int startTick;
            public long ownerID;
            public double speed;
            public ProjectileType pt;
            public double radius;
            public int timeTick;
            public long endTick;
            public myProjective(Projectile p, World world)
            {
                startX = p.X - p.SpeedX;
                startY = p.Y - p.SpeedY;
                radius = p.Radius;
                startTick = world.TickIndex;
                Id = p.Id;
                ownerID = p.OwnerPlayerId;
                pt = p.Type;
                speed = 40;
                if (pt == ProjectileType.FrostBolt)
                    speed = 34;
                if (pt == ProjectileType.Fireball)
                {
                    speed = 30;
                    radius = 100;
                }
                endX = startX + 600 / speed * p.SpeedX;
                endY = startY + 600 / speed * p.SpeedY;
                timeTick = (int)(600 / speed) - 1;
                foreach (Wizard w in world.Wizards)
                    if (w.Id == p.OwnerPlayerId)
                    {
                        if (p.X == w.X)
                            startX = w.X;
                        if (p.Y == w.Y)
                            startX = w.Y;
                        endX = startX + w.CastRange / speed * p.SpeedX;
                        endY = startY + w.CastRange / speed * p.SpeedY;
                        timeTick = (int)(w.CastRange / speed) - 1;
                        break;
                    }
                endTick = startTick + timeTick;
            }
        }


        public class PathNode
        {
            // Координаты точки на карте.
            public myPoint Position { get; set; }
            // Длина пути от старта (G).
            public int PathLengthFromStart { get; set; }
            // Точка, из которой пришли в эту точку.
            public PathNode CameFrom { get; set; }
            // Примерное расстояние до цели (H).
            public int HeuristicEstimatePathLength { get; set; }
            // Ожидаемое полное расстояние до цели (F).
            public int EstimateFullPathLength
            {
                get
                {
                    return this.PathLengthFromStart + this.HeuristicEstimatePathLength;
                }
            }
        }

        public class myBonus
        {
            public double x;
            public double y;
            public int tickToSpawn;
            public int tickAfterSpawn;
            public bool isGo;
            public myBonus(int tp)
            {
                if (tp == 1)
                {
                    x = 1200;
                    y = 1200;
                }
                else
                {
                    x = 2800;
                    y = 2800;
                }
            }
            public void init(int tickIndex)
            {
                if (tickIndex <= 17500)
                    tickToSpawn = 2500 - tickIndex % 2500;
                else
                    tickToSpawn = 20000;
                if (tickIndex >= 2500)
                    tickAfterSpawn = tickIndex % 2500;
                else
                    tickAfterSpawn = 20000;

                if (tickAfterSpawn <= 1)
                    isGo = true;
            }
        }

        public class myPoint
        {
            public double x;
            public double y;

            public myPoint()
            {
                x = 0; y = 0;
            }
            public myPoint(double xx, double yy)
            {
                x = xx; y = yy;
            }
        }

        public class myLivingUnit
        {
            public LivingUnit lu;
            public double Life;
            public double Mana;
            public double RemainingActionCooldownTicks;
            public int[] RemainingCooldownTicksByAction;
            public double X, Y, angle, speedX, speedY;
            public enum typeUnit : int { tree, wizard, tower, minion };
            public typeUnit tp;
            public Wizard selfWizard;
            public Minion selfMinion;
            public Building selfBuilding;
            public LivingUnit selfUnit;

            public Faction faction;
            public double damageRadius;
            public double damage;
            public myLivingUnit()
            {

            }
            public myLivingUnit(Wizard wiz)
            {
                tp = typeUnit.wizard;
                lu = wiz;
                selfWizard = wiz;
                selfUnit = wiz;
                Life = wiz.Life;
                Mana = wiz.Mana;
                RemainingActionCooldownTicks = wiz.RemainingActionCooldownTicks;
                RemainingCooldownTicksByAction = new int[wiz.RemainingCooldownTicksByAction.Length];
                RemainingCooldownTicksByAction = wiz.RemainingCooldownTicksByAction;
                X = wiz.X;
                Y = wiz.Y;
                angle = wiz.Angle;
                speedX = wiz.SpeedX;
                speedY = wiz.SpeedY;
                angle = wiz.Angle;
                faction = wiz.Faction;
                damageRadius = wiz.CastRange;
                damage = 12;
            }
            public myLivingUnit(Minion m)
            {
                tp = typeUnit.minion;
                lu = m;
                selfMinion = m;
                selfUnit = m;
                Life = m.Life;
                RemainingActionCooldownTicks = m.RemainingActionCooldownTicks;
                X = m.X;
                Y = m.Y;
                angle = m.Angle;
                speedX = m.SpeedX;
                speedY = m.SpeedY;
                angle = m.Angle;
                faction = m.Faction;
                if (m.Faction == Faction.Neutral && m.RemainingActionCooldownTicks > 0)
                    faction = enemy;

                damageRadius = 300;
                damage = 6;
                if (m.Type == MinionType.OrcWoodcutter)
                {
                    damageRadius = 50;
                    damage = 12;
                }
            }
            public myLivingUnit(Building b)
            {
                tp = typeUnit.tower;
                lu = b;
                selfBuilding = b;
                selfUnit = b;
                Life = b.Life;
                RemainingActionCooldownTicks = b.RemainingActionCooldownTicks;
                X = b.X;
                Y = b.Y;
                angle = b.Angle;
                speedX = b.SpeedX;
                speedY = b.SpeedY;
                angle = b.Angle;
                faction = b.Faction;
                damageRadius = b.AttackRange;
                damage = b.Damage;
                if (b.Faction == enemy && b.Y == 50)
                    isTop1 = true;
                if (b.Faction == enemy && b.X == 3650)
                    isBottom1 = true;
            }
            public void CalcMove(double x, double y)
            {
                if (tp == typeUnit.minion)
                {
                    double angleToMe = selfMinion.GetAngleTo(x, y);
                    X = X + 3 * Math.Cos(angle + angleToMe);
                    Y = Y - 3 * Math.Sin(angle + angleToMe);
                }
                if (tp == typeUnit.wizard)
                {
                    double angleToMe = selfWizard.GetAngleTo(x, y);
                    X = X + 4 * Math.Cos(angle + angleToMe);
                    Y = Y - 4 * Math.Sin(angle + angleToMe);
                }
            }

        }

        public class myWizard
        {
            public Move m;
            public Game g;
            public World w;
            public int tick;
            public Wizard wizSelf;

            public double X, Y, Angle, Life, Mana, speedX, speedY, radius;
            public double get_damage;
            public Faction f;
            public int RemainingActionCooldownTicks;
            public int[] RemainingCooldownTicksByAction;
            public double score;
            public bool isCollision;
            public double xCol, yCol;

            public myWizard(Wizard wiz, Game gg, World ww)
            {
                isCollision = false;
                get_damage = 0;
                score = 0;
                wizSelf = null;
                radius = wiz.Radius;
                tick = 0;
                X = wiz.X;
                Y = wiz.Y;
                Angle = wiz.Angle;
                f = wiz.Faction;
                Life = wiz.Life;
                Mana = wiz.Mana;
                speedX = wiz.SpeedX;
                speedY = wiz.SpeedY;
                RemainingActionCooldownTicks = wiz.RemainingActionCooldownTicks;
                RemainingCooldownTicksByAction = wiz.RemainingCooldownTicksByAction;
                wizSelf = wiz;
                g = gg;
                w = ww;
                m = new Move();
            }
            public myWizard(myWizard wiz)
            {
                isCollision = wiz.isCollision;
                get_damage = wiz.get_damage;
                score = wiz.score;
                wizSelf = null;
                radius = wiz.wizSelf.Radius;
                tick = wiz.tick;
                X = wiz.X;
                Y = wiz.Y;
                Angle = wiz.Angle;
                f = wiz.f;
                Life = wiz.Life;
                Mana = wiz.Mana;
                speedX = wiz.speedX;
                speedY = wiz.speedY;
                RemainingActionCooldownTicks = wiz.RemainingActionCooldownTicks;
                RemainingCooldownTicksByAction = wiz.RemainingCooldownTicksByAction;
                wizSelf = wiz.wizSelf;
                g = wiz.g;
                w = wiz.w;
                m = new Move();
            }
            public double GetAngleTo(double XX, double YY)
            {
                double absoluteAngleTo = Math.Atan2(YY - Y, XX - X);
                double relativeAngleTo = absoluteAngleTo - Angle;

                while (relativeAngleTo > Math.PI)
                {
                    relativeAngleTo -= 2.0D * Math.PI;
                }

                while (relativeAngleTo < -Math.PI)
                {
                    relativeAngleTo += 2.0D * Math.PI;
                }

                return relativeAngleTo;
            }
            public double getDistance(double x1, double y1, double x2, double y2)
            {
                return Math.Sqrt((x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2));
            }
            public double getDistanceTo(double xTo, double yTo)
            {
                return Math.Sqrt((X - xTo) * (X - xTo) + (Y - yTo) * (Y - yTo));
            }

            public void CalcMove()
            {
                double maxSpeed = g.WizardForwardSpeed * speedFactor;
                if (m.Speed < 0)
                    maxSpeed = g.WizardBackwardSpeed * speedFactor;
                double locSpeed = m.Speed;
                double locStrafe = m.StrafeSpeed;


                tick++;
                if (Life <= 0) return;

                if (locSpeed > 0 && locSpeed > g.WizardForwardSpeed * speedFactor) locSpeed = g.WizardForwardSpeed * speedFactor;
                if (locSpeed < 0 && locSpeed < -g.WizardBackwardSpeed * speedFactor) locSpeed = -g.WizardBackwardSpeed * speedFactor;
                if (locStrafe > 0 && m.StrafeSpeed > g.WizardStrafeSpeed * speedFactor) m.StrafeSpeed = g.WizardStrafeSpeed * speedFactor;
                if (locStrafe < 0 && Math.Abs(locStrafe) > g.WizardStrafeSpeed * speedFactor) locStrafe = -g.WizardStrafeSpeed * speedFactor;

                double modSpeed = Math.Sqrt((locSpeed / maxSpeed) * (locSpeed / maxSpeed) + (locStrafe / g.WizardStrafeSpeed) * (locStrafe / g.WizardStrafeSpeed));
                if (modSpeed > 1)
                {
                    locSpeed = locSpeed / modSpeed;
                    locStrafe = locStrafe / modSpeed;
                }
                speedX = locSpeed * Math.Cos(Angle) - locStrafe * Math.Sin(Angle);
                speedY = locSpeed * Math.Sin(Angle) + locStrafe * Math.Cos(Angle);

                //проверка на столкновения с деревями и зданиями

                for (int i = 0; i < w.Buildings.Length; i++)
                    if (getDistance(X + speedX, Y + speedY, w.Buildings[i].X, w.Buildings[i].Y) <= radius + w.Buildings[i].Radius + 1)
                    {
                        xCol = w.Buildings[i].X;
                        yCol = w.Buildings[i].Y;
                        isCollision = true;
                    }
                for (int i = 0; i < w.Trees.Length; i++)
                    if (getDistance(X + speedX, Y + speedY, w.Trees[i].X, w.Trees[i].Y) <= radius + w.Trees[i].Radius + 1)
                    {
                        isCollision = true;
                        xCol = w.Trees[i].X;
                        yCol = w.Trees[i].Y;
                    }
                for (int i = 0; i < w.Wizards.Length; i++)
                {
                    if (w.Wizards[i].Id != wizSelf.Id)
                        if (getDistance(X + speedX, Y + speedY, w.Wizards[i].X, w.Wizards[i].Y) <= radius + w.Wizards[i].Radius + 0.1)
                        {
                            xCol = w.Wizards[i].X;
                            yCol = w.Wizards[i].Y;
                            isCollision = true;
                        }
                }
                foreach (Minion m in w.Minions)
                {
                    if (getDistance(X + speedX, Y + speedY, m.X, m.Y) <= radius + m.Radius)
                    {
                        xCol = m.X;
                        yCol = m.Y;
                        isCollision = true;
                    }
                }
                if (X + speedX - radius < 0 || X + speedX + radius >= w.Width)
                {
                    xCol = 0;
                    yCol = 0;
                    isCollision = true;
                }
                if (Y + speedY - radius < 0 || Y + speedY + radius >= w.Height)
                {
                    xCol = 4000;
                    yCol = 4000;
                    isCollision = true;
                }

                int Bx = (int)((X + speedX) / 400);
                int By = (int)((Y + speedY) / 400);



                if (!isCollision)
                {
                    X = X + speedX;
                    Y = Y + speedY;
                    speedX = speedX;
                    speedY = speedY;
                }
                if (m.Turn > g.WizardMaxTurnAngle) m.Turn = g.WizardMaxTurnAngle;
                if (m.Turn < -g.WizardMaxTurnAngle) m.Turn = -g.WizardMaxTurnAngle;
                Angle = Angle + m.Turn;
                for (int i = 0; i < RemainingCooldownTicksByAction.Length; i++)
                    if (RemainingCooldownTicksByAction[i] > 0) RemainingCooldownTicksByAction[i]--;
                if (RemainingActionCooldownTicks > 0) RemainingActionCooldownTicks--;

                if (Life < wizSelf.MaxLife) Life = Life + 0.05; if (Life > wizSelf.MaxLife) Life = wizSelf.MaxLife;
                if (Mana < wizSelf.MaxMana) Mana = Mana + 0.05 * 4; if (Mana > wizSelf.Mana) Mana = wizSelf.MaxMana;
            }

            public void CorrectColision()
            {
                double an = GetAngleTo(xCol, yCol);
                m.Speed = -Math.Cos(an) * 4;
                m.StrafeSpeed = -Math.Sin(an) * 3;
            }
            public void CalcMove(int cnt, double an = 0)
            {
                double turn = m.Turn;
                for (int i = 0; i < cnt; i++)
                {
                    if (Math.Abs(an) > Math.PI / 12)
                    {
                        an = Math.Abs(an) - Math.PI / 12;
                    }
                    else
                        m.Turn = 0;
                    CalcMove();
                    if (isCollision) return;
                }
            }


        }

        public bool checkSkill(Wizard self, SkillType sk)
        {
            foreach (SkillType s in self.Skills)
                if (s == sk)
                    return true;
            return false;
        }

        public SkillType LearnSkill(Wizard self)
        {



            if (!checkSkill(self, SkillType.StaffDamageBonusPassive1))
                return SkillType.StaffDamageBonusPassive1;
            if (!checkSkill(self, SkillType.StaffDamageBonusAura1))
                return SkillType.StaffDamageBonusAura1;
            if (!checkSkill(self, SkillType.StaffDamageBonusPassive2))
                return SkillType.StaffDamageBonusPassive2;
            if (!checkSkill(self, SkillType.StaffDamageBonusAura2))
                return SkillType.StaffDamageBonusAura2;
            if (!checkSkill(self, SkillType.Fireball))
                return SkillType.Fireball;


            if (!checkSkill(self, SkillType.RangeBonusPassive1))
                return SkillType.RangeBonusPassive1;
            if (!checkSkill(self, SkillType.RangeBonusAura1))
                return SkillType.RangeBonusAura1;
            if (!checkSkill(self, SkillType.RangeBonusPassive2))
                return SkillType.RangeBonusPassive2;
            if (!checkSkill(self, SkillType.RangeBonusAura2))
                return SkillType.RangeBonusAura2;
            if (!checkSkill(self, SkillType.AdvancedMagicMissile))
                return SkillType.AdvancedMagicMissile;





            if (!checkSkill(self, SkillType.MovementBonusFactorPassive1))
                return SkillType.MovementBonusFactorPassive1;
            if (!checkSkill(self, SkillType.MovementBonusFactorAura1))
                return SkillType.MovementBonusFactorAura1;
            if (!checkSkill(self, SkillType.MovementBonusFactorPassive2))
                return SkillType.MovementBonusFactorPassive2;
            if (!checkSkill(self, SkillType.MovementBonusFactorAura2))
                return SkillType.MovementBonusFactorAura2;
            if (!checkSkill(self, SkillType.Haste))
                return SkillType.Haste;


            if (!checkSkill(self, SkillType.MagicalDamageAbsorptionPassive1))
                return SkillType.MagicalDamageAbsorptionPassive1;
            if (!checkSkill(self, SkillType.MagicalDamageAbsorptionAura1))
                return SkillType.MagicalDamageAbsorptionAura1;
            if (!checkSkill(self, SkillType.MagicalDamageAbsorptionPassive2))
                return SkillType.MagicalDamageAbsorptionPassive2;
            if (!checkSkill(self, SkillType.MagicalDamageAbsorptionAura2))
                return SkillType.MagicalDamageAbsorptionAura2;
            if (!checkSkill(self, SkillType.Shield))
                return SkillType.Shield;







            if (!checkSkill(self, SkillType.MagicalDamageBonusPassive1))
                return SkillType.MagicalDamageBonusPassive1;
            if (!checkSkill(self, SkillType.MagicalDamageBonusAura1))
                return SkillType.MagicalDamageBonusAura1;
            if (!checkSkill(self, SkillType.MagicalDamageBonusPassive2))
                return SkillType.MagicalDamageBonusPassive2;
            if (!checkSkill(self, SkillType.MagicalDamageBonusAura2))
                return SkillType.MagicalDamageBonusAura2;
            if (!checkSkill(self, SkillType.FrostBolt))
                return SkillType.FrostBolt;




            return SkillType.Shield;


        }

        public void Move(Wizard self, World world, Game game, Move move)
        {

            if (world.TickIndex <= 0)
                initGame(self, world, game);


            //vc.BeginPre();

            if (backMove > 0)
                backMove--;



            if (pause > 0)
            {
                pause--;
                return;
            }

            if (world.TickIndex == 1230)
                pause = 0;

            Move m = GetBestMove(self, world, game);
            move.Action = m.Action;
            move.CastAngle = m.CastAngle;
            move.MaxCastDistance = m.MaxCastDistance;
            move.MinCastDistance = m.MinCastDistance;
            move.SkillToLearn = m.SkillToLearn;
            move.Speed = m.Speed;
            move.StrafeSpeed = m.StrafeSpeed;
            move.Turn = m.Turn;

            if (checkSkill(self, SkillType.Haste) && self.RemainingCooldownTicksByAction[(int)ActionType.Haste] == 0 && !isHaste(self))
            {
                Wizard targetShield = null;
                foreach (Wizard w in world.Wizards)
                    if (w.Faction == self.Faction && !w.IsMe && self.GetDistanceTo(w) < self.CastRange)
                    {
                        targetShield = w;
                        if (Math.Abs(self.GetAngleTo(w)) < game.StaffSector * 0.3)
                        {
                            move.Action = ActionType.Haste;
                            move.StatusTargetId = w.Id;
                            move.CastAngle = self.GetAngleTo(w);
                        }
                        move.Turn = self.GetAngleTo(w);
                    }
                if (targetShield == null)
                {
                    move.Action = ActionType.Haste;
                    move.StatusTargetId = self.Id;
                }
            }
            if (move.Action != ActionType.None)
            {
                backMove = 3;
                m = GetBestMoveToPointBack(new myPoint(self.X + 10 * Math.Cos(self.Angle), self.Y + 10 * Math.Sin(self.Angle)), self, world, game);
                move.Speed = m.Speed;
                move.StrafeSpeed = m.StrafeSpeed;
            }
            //vc.EndPre();
        }


        public Move gotoTarget(myLivingUnit trUnit, Wizard self, World world, Game game)
        {
            Move res = new Move();

            if (errorIdTarget != 0 && trUnit.selfUnit.Id == errorIdTarget)
            {
                double an = self.GetAngleTo(trUnit.X, trUnit.Y);
                res.Speed = getSpeed(an);
                res.StrafeSpeed = getStrafeSpeed(an);
                return res;
            }

            errorIdTarget = 0;
            int step = 10;
            int lenField = (int)((self.GetDistanceTo(trUnit.selfUnit) + self.Radius + trUnit.selfUnit.Radius + 1) / step) * 2 + 20;

            if (self.GetDistanceTo(trUnit.selfUnit) - trUnit.selfUnit.Radius < 70)
                return res;

            int[,] setka = new int[lenField, lenField];
            int lf = (int)lenField / 2;
            foreach (myLivingUnit lu in livingUnits)
                if (lu.faction == self.Faction && self.GetDistanceTo(lu.selfUnit) < self.VisionRange)
                {
                    int cx = (int)((lu.selfUnit.X - self.X) / step + lf);
                    int cy = (int)((lu.selfUnit.Y - self.Y) / step + lf);

                    int radius = (int)((self.Radius + lu.selfUnit.Radius + 2) / step);
                    if (cx > 0 && cx < lenField && cy > 0 && cy < lenField)
                        circle(setka, cx, cy, radius);
                }
            foreach (Tree lu in world.Trees)
                if (self.GetDistanceTo(lu) < self.VisionRange)
                {
                    int cx = (int)((lu.X - self.X) / step + lf);
                    int cy = (int)((lu.Y - self.Y) / step + lf);

                    int radius = (int)((self.Radius + lu.Radius + 2) / step);
                    if (cx > 0 && cx < lenField && cy > 0 && cy < lenField)
                        circle(setka, cx, cy, radius);

                }


            int destX = (int)((trUnit.selfUnit.X - self.X) / step + lf);
            int destY = (int)((trUnit.selfUnit.Y - self.Y) / step + lf);
            setka[destX, destY] = 0;
            List<myPoint> trTarget = FindPath(setka, new myPoint(lf, lf), new myPoint(destX, destY));

            if (trTarget == null)
            {
                errorIdTarget = trUnit.selfUnit.Id;
                double an = self.GetAngleTo(trUnit.X, trUnit.Y);
                res.Speed = getSpeed(an);
                res.StrafeSpeed = getStrafeSpeed(an);
                return res;
            }

            if (trTarget != null)
                if (trTarget.Count > 2)
                {
                    double toX = self.X + (trTarget[2].x - lf) * step;
                    double toY = self.Y + (trTarget[2].y - lf) * step;
                    double an = self.GetAngleTo(toX, toY);
                    res.Speed = getSpeed(an);
                    res.StrafeSpeed = getStrafeSpeed(an);
                }


            return res;
        }

        public Move gotoBackMove(myLivingUnit trUnit , Wizard self, World world, Game game)
        {
            Move res = new Move();
            myPoint p1 = midWay[0];
            myPoint p2 = midWay[0];

            if (trUnit == null)
            {
                trUnit = new myLivingUnit(self);
                trUnit.X = self.X + Math.Cos(self.Angle) * 50;
                trUnit.Y = self.Y + Math.Sin(self.Angle) * 50;
            }
                

            double distTarget = self.GetDistanceTo(trUnit.X, trUnit.Y);
            double dist = 0;
            res.Turn = self.GetAngleTo(trUnit.X, trUnit.Y);
            for (int i = 2; i < 50; i++)
                if (dist == 0 || self.GetDistanceTo(midWay[i].x, midWay[i].y) < dist)
                {
                    p1 = midWay[i];
                    p2 = midWay[i - 1];
                    dist = self.GetDistanceTo(midWay[i].x, midWay[i].y);
                    if (dist < 10)
                        p2 = midWay[i - 2];
                }



            errorIdTarget = 0;
            int step = 4;
            int lenField = (int)((self.GetDistanceTo(p2.x,p2.y)  / step) * 2) + 20;

            int[,] setka = new int[lenField, lenField];

            int lf = (int)lenField / 2;
            foreach (myLivingUnit lu in livingUnits)
                if (lu.faction == self.Faction && self.GetDistanceTo(lu.selfUnit) < self.VisionRange)
                {
                    int cx = (int)((lu.selfUnit.X - self.X) / step + lf);
                    int cy = (int)((lu.selfUnit.Y - self.Y) / step + lf);

                    int radius = (int)((self.Radius + lu.selfUnit.Radius + 2) / step);
                    if (cx > 0 && cx < lenField && cy > 0 && cy < lenField)
                        circle(setka, cx, cy, radius);
                }
            foreach (Tree lu in world.Trees)
                if (self.GetDistanceTo(lu) < self.VisionRange)
                {
                    int cx = (int)((lu.X - self.X) / step + lf);
                    int cy = (int)((lu.Y - self.Y) / step + lf);

                    int radius = (int)((self.Radius + lu.Radius + 2) / step);
                    if (cx > 0 && cx < lenField && cy > 0 && cy < lenField)
                        circle(setka, cx, cy, radius);

                }


            int destX = (int)((p2.x - self.X) / step + lf);
            int destY = (int)((p2.y - self.Y) / step + lf);

            List<myPoint> trTarget = null;
            if (setka[destX,destY]==0)
                trTarget= FindPath(setka, new myPoint(lf, lf), new myPoint(destX, destY));

            if (trTarget == null)
            {
                double an = self.GetAngleTo(p2.x ,p2.y);

                //vc.Line(self.X, self.Y, p2.x, p2.y);
                myWizard calcWiz = new myWizard(self, game, world);
                calcWiz.m.Speed = getSpeed(an);
                calcWiz.m.StrafeSpeed = getStrafeSpeed(an);
                calcWiz.CalcMove(3);
                if (!calcWiz.isCollision)
                {
                    res.Speed = getSpeed(an);
                    res.StrafeSpeed = getStrafeSpeed(an);
                    return res;
                }
                an = self.GetAngleTo(calcWiz.xCol,calcWiz.yCol);

                double spx = self.X + (self.X - calcWiz.xCol)*Math.Cos(an) *4 - (self.Y - calcWiz.yCol) * Math.Sin(an)*4;
                double spy = self.Y + (self.X - calcWiz.xCol) * Math.Sin(an) *4 + (self.Y - calcWiz.yCol) * Math.Cos(an) * 4;
                double spx1 = self.X - (self.X - calcWiz.xCol) * Math.Cos(an)  * 4+ (self.Y - calcWiz.yCol) * Math.Sin(an)*4;
                double spy1 = self.Y - (self.X - calcWiz.xCol) * Math.Sin(an) *4 - (self.Y - calcWiz.yCol) * Math.Cos(an)*4;

                if (getDistance(spx, spy, trUnit.X, trUnit.Y) > getDistance(spx1, spy1, trUnit.X, trUnit.Y))
                {
                    an = self.GetAngleTo(spx, spy);
                    res.Speed = getSpeed(an);
                    res.StrafeSpeed = getStrafeSpeed(an);
                    //vc.Line(self.X, self.Y, spx, spy, 1);
                    spx = self.X - (self.X - calcWiz.xCol) * Math.Cos(an) + self.X - (self.Y - calcWiz.yCol) * Math.Sin(an);
                    spy = self.Y - (self.X - calcWiz.xCol) * Math.Sin(an) + self.X - (self.Y - calcWiz.yCol) * Math.Cos(an);
                    //vc.Line(self.X, self.Y, spx, spy, 1, 0);
                    return res;
                }
                else
                {
                    an = self.GetAngleTo(spx1, spy1);
                    res.Speed = getSpeed(an);
                    res.StrafeSpeed = getStrafeSpeed(an);
                    //vc.Line(self.X, self.Y, spx1, spy1, 0, 1);
                    return res;
                }



            }

            if (trTarget != null)
                if (trTarget.Count > 2)
                {
                    double toX = self.X + (trTarget[2].x - lf) * step;
                    double toY = self.Y + (trTarget[2].y - lf) * step;
                    double an = self.GetAngleTo(toX, toY);
                    res.Speed = getSpeed(an);
                    res.StrafeSpeed = getStrafeSpeed(an);
                    for (int i = 1; i < trTarget.Count; i++)
                    {
                        //vc.Line(self.X + (trTarget[i-1].x - lf) * step, self.Y + (trTarget[i-1].y - lf) * step, self.X + (trTarget[i].x - lf) * step, self.Y + (trTarget[i].y - lf) * step);
                    }
                }


            return res;
        }




        //------------------------------------------------------------------------------------------------------------------------------------

        private double getStrafeSpeed(double angle)
        {

            double atg = Math.Tan(angle);
            return speedFactor * 12 * Math.Sin(angle) * Math.Sqrt((atg * atg + 1) / (9 + 16 * atg * atg));
        }

        private double getSpeed(double angle)
        {
            double atg = Math.Tan(angle);
            return speedFactor * 12 * Math.Cos(angle) * Math.Sqrt((atg * atg + 1) / (9 + 16 * atg * atg));
        }

        public bool isCanDodge(Wizard self, myPoint start, myPoint end, World world, Game game, int tick, int p_Radius)
        {
            if (isIntersecObject(self, start, end, p_Radius))
            {
                myWizard wiz1 = new myWizard(self, game, world);
                myWizard wiz2 = new myWizard(self, game, world);
                myWizard wiz3 = new myWizard(self, game, world);
                myWizard wiz4 = new myWizard(self, game, world);
                double an90 = Math.PI / 2 + self.GetAngleTo(start.x, start.y);
                double an90m = self.GetAngleTo(start.x, start.y) - Math.PI / 2;
                double an0 = self.GetAngleTo(start.x, start.y);
                wiz1.m.Speed = -getSpeed(an0);
                wiz1.m.StrafeSpeed = -getStrafeSpeed(an0);
                wiz2.m.Speed = -wiz1.m.Speed;
                wiz2.m.StrafeSpeed = -wiz1.m.StrafeSpeed;
                wiz3.m.Speed = getSpeed(an90);
                wiz3.m.StrafeSpeed = getStrafeSpeed(an90);
                wiz4.m.Speed = getSpeed(an90m);
                wiz4.m.StrafeSpeed = getStrafeSpeed(an90m);
                wiz1.CalcMove(tick);
                wiz2.CalcMove(tick);
                wiz3.CalcMove(tick);
                wiz4.CalcMove(tick);
                //vc.Circle(wiz1.X, wiz1.Y, 35, 0, 1);
                //vc.Circle(wiz2.X, wiz2.Y, 35, 0, 1);
                //                vc.Circle(wiz3.X, wiz3.Y, 35, 0, 1);
                //                vc.Circle(wiz4.X, wiz4.Y, 35, 0, 1, 1);

                if (!isIntersecObject(wiz1.X, wiz1.Y, 36 + p_Radius, start, end))
                    return true;
                if (!isIntersecObject(wiz3.X, wiz3.Y, 35 + p_Radius, start, end))
                    return true;
                double dst = self.GetDistanceTo(start.x, start.y);
                if (!isIntersecObject(wiz4.X, wiz4.Y, 35 + p_Radius, start, end))
                    return true;
                if (!isIntersecObject(wiz2.X, wiz2.Y, 36 + p_Radius, start, end))
                    return true;

                return false;
            }

            return true;
        }

        public double getSpeedFactor(Wizard wiz, Game game)
        {
            double spF = 1;
            if (checkSkill(wiz, SkillType.MovementBonusFactorPassive1))
                spF = spF + game.MovementBonusFactorPerSkillLevel;
            if (checkSkill(wiz, SkillType.MovementBonusFactorPassive2))
                spF = spF + game.MovementBonusFactorPerSkillLevel;
            if (checkSkill(wiz, SkillType.MovementBonusFactorAura1))
                spF = spF + game.MovementBonusFactorPerSkillLevel;
            if (checkSkill(wiz, SkillType.MovementBonusFactorAura2))
                spF = spF + game.MovementBonusFactorPerSkillLevel;

            foreach (Status st in wiz.Statuses)
                if (st.Type == StatusType.Hastened)
                    spF = spF + game.HastenedMovementBonusFactor;
            return spF;
        }

        public bool isHitSumul(Wizard self, Wizard target, World world, Game game, ProjectileType pt = ProjectileType.MagicMissile)
        {
            if (Math.Max(target.RemainingActionCooldownTicks, target.RemainingCooldownTicksByAction[2]) > 10)
                return false;
            myWizard wiz = new myWizard(target, game, world);
            wiz.m.Speed = getSpeed(target.GetAngleTo(self));
            wiz.m.StrafeSpeed = getStrafeSpeed(target.GetAngleTo(self));
            if (!self.IsMe)
            {
                double spf = getSpeedFactor(self, game);
                wiz.m.Speed = wiz.m.Speed / speedFactor * spf;
                wiz.m.StrafeSpeed = wiz.m.StrafeSpeed / speedFactor * spf;
                wiz.CalcMove(3);
            }
            if (self.IsMe)
                wiz.CalcMove(4);
            double endX = wiz.X + target.CastRange * Math.Cos(target.Angle + target.GetAngleTo(self));
            double endY = wiz.Y + target.CastRange * Math.Sin(target.Angle + target.GetAngleTo(self));


            double dst = Math.Min(target.CastRange, self.GetDistanceTo(target));

            int tick = (int)(dst / 40) - 1;

            if (isCanDodge(self, new myPoint(wiz.X, wiz.Y), new myPoint(endX, endY), world, game, tick, 10))
                return false;
            return true;
        }

        public Move GetBestMove(Wizard self, World world, Game game)
        {
            Move move = new Move();

            if (game.IsSkillsEnabled)
            {
                if (self.Level > self.Skills.Length)//можно изучить скилл
                {
                    //прокачиваем файер-бол и дальность
                    move.SkillToLearn = LearnSkill(self);
                }
            }

            checkWorld(self, world, game);
            myPoint target = getNearTarget(self, world, game);

            //проверка на попадание в меня

            foreach (myProjective p in projectives)
            {
                //проверка что снаряд в меня

                //vc.Circle(p.endX, p.endY, (float)p.radius, 1);
                //vc.Line(p.startX, p.startY, p.endX, p.endY, 1);
                if (isIntersecObject(self, new myPoint(p.startX, p.startY), new myPoint(p.endX, p.endY), p.radius + 6))
                {
                    Move m = GetAction(target, self, world, game);
                    move.Action = m.Action;
                    move.CastAngle = m.CastAngle;
                    move.MinCastDistance = m.MinCastDistance;
                    myWizard wiz1 = new myWizard(self, game, world);
                    myWizard wiz2 = new myWizard(self, game, world);
                    myWizard wiz3 = new myWizard(self, game, world);
                    myWizard wiz4 = new myWizard(self, game, world);
                    double an90 = Math.PI / 2 + self.GetAngleTo(p.startX, p.startY);
                    double an90m = self.GetAngleTo(p.startX, p.startY) - Math.PI / 2;
                    double an0 = self.GetAngleTo(p.startX, p.startY);
                    wiz1.m.Speed = -getSpeed(an0);
                    wiz1.m.StrafeSpeed = -getStrafeSpeed(an0);
                    wiz2.m.Speed = -wiz1.m.Speed;
                    wiz2.m.StrafeSpeed = -wiz1.m.StrafeSpeed;
                    wiz3.m.Speed = getSpeed(an90);
                    wiz3.m.StrafeSpeed = getStrafeSpeed(an90);
                    wiz4.m.Speed = getSpeed(an90m);
                    wiz4.m.StrafeSpeed = getStrafeSpeed(an90m);
                    wiz1.CalcMove(Convert.ToInt32(p.endTick - world.TickIndex));
                    wiz2.CalcMove(Convert.ToInt32(p.endTick - world.TickIndex));
                    wiz3.CalcMove(Convert.ToInt32(p.endTick - world.TickIndex));
                    wiz4.CalcMove(Convert.ToInt32(p.endTick - world.TickIndex));
                    //vc.Circle(wiz1.X, wiz1.Y, 35, 0, 1);
                    //vc.Circle(wiz2.X, wiz2.Y, 35, 0, 1);
                    //vc.Circle(wiz3.X, wiz3.Y, 35, 0, 1);
                    //vc.Circle(wiz4.X, wiz4.Y, 35, 0, 1, 1);

                    if (!isIntersecObject(wiz1.X, wiz1.Y, 36 + p.radius, new myPoint(p.startX, p.startY), new myPoint(p.endX, p.endY)))
                    {
                        move.Speed = wiz1.m.Speed;
                        move.StrafeSpeed = wiz1.m.StrafeSpeed;
                        return move;
                    }

                    if (!isIntersecObject(wiz3.X, wiz3.Y, 35 + p.radius, new myPoint(p.startX, p.startY), new myPoint(p.endX, p.endY)))
                    {
                        move.Speed = wiz3.m.Speed;
                        move.StrafeSpeed = wiz3.m.StrafeSpeed;
                        return move;
                    }
                    double dst = self.GetDistanceTo(p.startX, p.startY);
                    if (!isIntersecObject(wiz4.X, wiz4.Y, 35 + p.radius, new myPoint(p.startX, p.startY), new myPoint(p.endX, p.endY)) && wiz4.getDistanceTo(p.endX, p.endY) > 46)
                    {
                        move.Speed = wiz4.m.Speed;
                        move.StrafeSpeed = wiz4.m.StrafeSpeed;
                        return move;
                    }
                    if (!isIntersecObject(wiz2.X, wiz2.Y, 36 + p.radius, new myPoint(p.startX, p.startY), new myPoint(p.endX, p.endY)))
                    {
                        move.Speed = wiz2.m.Speed;
                        move.StrafeSpeed = wiz2.m.StrafeSpeed;
                        return move;
                    }

                    /*                    double dst1 = getDistance(wiz1.X, wiz1.Y, p.startX, p.startY);
                                        double dst2 = getDistance(wiz2.X, wiz2.Y, p.startX, p.startY);
                                        double dst3 = getDistance(wiz3.X, wiz4.Y, p.startX, p.startY);
                                        double dst4 = getDistance(wiz4.X, wiz4.Y, p.startX, p.startY);
                                        if (dst1 > dst2 && dst1 > dst3 && dst1 > dst4)
                                        {
                                            move.Speed = wiz1.m.Speed;
                                            move.StrafeSpeed = wiz1.m.StrafeSpeed;
                                            return move;
                                        }
                                        if (dst2 > dst1 && dst2 > dst3 && dst2 > dst4)
                                        {
                                            move.Speed = wiz2.m.Speed;
                                            move.StrafeSpeed = wiz2.m.StrafeSpeed;
                                            return move;
                                        }
                                        if (dst3 > dst2 && dst3 > dst1 && dst3 > dst4)
                                        {
                                            move.Speed = wiz3.m.Speed;
                                            move.StrafeSpeed = wiz3.m.StrafeSpeed;
                                            return move;
                                        }
                                        if (dst4 > dst2 && dst4 > dst3 && dst4 > dst1)
                                        {
                                            move.Speed = wiz4.m.Speed;
                                            move.StrafeSpeed = wiz4.m.StrafeSpeed;
                                            return move;
                                        }
                    */

                }

            }




            mapPoint = getMapTarget(self, world, game);
            if (targetUnit == null)
                target = mapPoint;

            foreach (Tree tr in world.Trees)
                if (self.GetDistanceTo(tr) < 40 + tr.Radius && self.GetDistanceTo(target.x, target.y) > 100)
                {
                    move.Action = ActionType.MagicMissile;
                    move.CastAngle = self.GetAngleTo(tr);
                    move.MaxCastDistance = self.GetDistanceTo(tr) - tr.Radius;
                    move.Turn = self.GetAngleTo(tr);
                    move.Speed = 1;
                    if (self.RemainingCooldownTicksByAction[1] == 0)
                        move.Action = ActionType.Staff;
                    return move;
                }


            int Bx = (int)self.X / 400;
            int By = (int)self.Y / 400;


            int tickToRespawnMinion = 750 - (int)(world.TickIndex - 750 * Math.Truncate(world.TickIndex / 750.0));
            int tickToSafeOutMinion = (int)(self.GetDistanceTo(7 * 400 + 200, 3 * 400 - 200) / 3d);

            if (tickToRespawnMinion < tickToSafeOutMinion)//скоро родятся вражеские миньены
            {
                if (self.GetDistanceTo(3600, 400) < 1200)
                {
                    int cntMy = 0;
                    foreach (myLivingUnit lu in livingUnits)
                        if (lu.faction == self.Faction && lu.selfUnit.GetDistanceTo(3600, 400) < 600)
                            cntMy++;
                    if (cntMy < 4 || lifeTron > 50 || self.Life < 50)
                    {
                        Move m = GetBestMoveToPointBack(target, self, world, game);
                        move.Speed = m.Speed;
                        move.StrafeSpeed = m.StrafeSpeed;
                        move.Turn = m.Turn;
                        m = GetAction(target, self, world, game);
                        move.Action = m.Action;
                        move.CastAngle = m.CastAngle;
                        move.MinCastDistance = m.MinCastDistance;
                        return move;
                    }
                }
            }




            int tickToRespawnBonus = 2500 - (int)(world.TickIndex - 2500 * Math.Truncate(world.TickIndex / 2500d));


            //проверка взять бонус
            foreach (Bonus b in world.Bonuses)
            {
                if (self.GetDistanceTo(b) < self.VisionRange)
                {
                    myPoint bounusTarget = new myPoint(b.X, b.Y);
                    Move m = GetBestMoveToPoint(bounusTarget, target, self, world, game);
                    move.Speed = m.Speed;
                    move.StrafeSpeed = m.StrafeSpeed;
                    move.Turn = m.Turn;
                    m = GetAction(target, self, world, game);
                    move.Action = m.Action;
                    move.CastAngle = m.CastAngle;
                    move.MinCastDistance = m.MinCastDistance;
                    return move;
                }
            }





            int needSafe = isNeedSafe(self, world, game);

            if (needSafe == -2)
            {
                myPoint backP = getMapTargetBack(self, world, game);
                double anB = self.GetAngleTo(backP.x, backP.y);
                move.Turn = anB;
                move.Speed = getSpeed(anB);
                move.StrafeSpeed = getStrafeSpeed(anB);
                return move;
            }

            if (needSafe != 0)
            {
                myWizard mwz = new myWizard(self, game, world);
                Move m = gotoBackMove(dangerUnit, self, world, game);
                move.Speed = m.Speed;
                move.StrafeSpeed = m.StrafeSpeed;
                move.Turn = m.Turn;
                m = GetAction(target, self, world, game);
                move.Action = m.Action;
                move.CastAngle = m.CastAngle;
                move.MinCastDistance = m.MinCastDistance;
                move.MaxCastDistance = m.MaxCastDistance;
                return move;
            }


            if (!gotoBonus)
            {
                bonus = new myPoint(bonus2.x, bonus2.y);
                if (self.GetDistanceTo(bonus1.x, bonus1.y) < 800 || (!isMiddleTower1 && self.GetDistanceTo(bonus1.x, bonus1.y) < self.GetDistanceTo(bonus2.x, bonus2.y)))
                {
                    bonus = new myPoint(bonus1.x, bonus1.y);
                }
            }

            if (gotoBonus && bonus1.tickToSpawn > 700 && bonus1.tickAfterSpawn >= 3)
            {
                if (bonus1.isGo == false && bonus2.isGo == false)
                    gotoBonus = false;
                if (bonus1.isGo == false && bonus.x == bonus1.x)
                    gotoBonus = false;
                if (bonus2.isGo == false && bonus.x == bonus2.x)
                    gotoBonus = false;

            }


            /*
                        if (gotoBonus && bonus1.tickAfterSpawn <= 5)
                        {
                            if (bonus1.x == bonus.x && bonus1.isGo == false)
                            {
                                bonus.x = bonus2.x;
                                bonus.y = bonus2.y;
                            }
                            if (bonus2.x == bonus.x && bonus2.isGo == false)
                            {
                                bonus.x = bonus1.x;
                                bonus.y = bonus1.y;
                            }
                        }
            */

            double runTickToBonus = self.GetDistanceTo(bonus.x, bonus.y) / 3;
            if (Bx > 5)
                runTickToBonus = self.GetDistanceTo(bonus.x, bonus.y) / 2.6;

            gotoBonus = false;
            if ((runTickToBonus < 0 && tickToRespawnBonus < runTickToBonus && world.TickIndex > 2000 && world.TickIndex < 18000) || gotoBonus)
                if (Bx == By || (Bx == 1 && By == 0) || (Bx == 4 && By == 5) || (Bx == 3 && By == 4) || (Bx == 6 && By == 3) || (Bx == 7 && By == 3) || (Bx == 6 && By == 2) || (Bx == 6 && By == 4) || (Bx == 5 && By == 4) || (Bx == 4 && By == 3) || (Bx == 6 && By == 5) || gotoBonus)
                {

                    Move m = new Move();
                    if (self.GetDistanceTo(bonus.x, bonus.y) > self.VisionRange - 10)
                    {
                        m = GetBestMoveToPoint(bonus, target, self, world, game);
                        move.Speed = m.Speed;
                        move.StrafeSpeed = m.StrafeSpeed;
                        move.Turn = m.Turn;
                        gotoBonus = true;
                    }
                    else
                    {
                        if (targetUnit == null)
                            target = bonus;
                        move.Turn = self.GetAngleTo(target.x, target.y);
                        double angl = self.GetAngleTo(bonus.x, bonus.y);
                        if (self.GetDistanceTo(bonus.x, bonus.y) - self.Radius - game.BonusRadius > 4)
                        {
                            move.Speed = getSpeed(angl);
                            move.StrafeSpeed = getStrafeSpeed(angl);

                        }
                        if (self.GetDistanceTo(bonus.x, bonus.y) - self.Radius - -game.BonusRadius <= 1)
                        {
                            move.Speed = -2;
                            //move.StrafeSpeed = Math.Sin(angl) * 3;
                        }
                    }
                    m = GetAction(target, self, world, game);
                    move.Action = m.Action;
                    move.CastAngle = m.CastAngle;
                    move.MinCastDistance = m.MinCastDistance;
                    gotoBonus = true;
                    return move;

                }


            double minDstWizard = 0;
            int cntEnemyWiz = 0;
            //найдем ближайшего волшебника
            Wizard nearWizard = null;
            foreach (Wizard w in world.Wizards)
                if (w.Faction != self.Faction && (self.GetDistanceTo(w) < self.VisionRange || self.GetDistanceTo(w) < self.CastRange || self.GetDistanceTo(w) < w.CastRange))
                {
                    cntEnemyWiz++;
                    if (self.GetDistanceTo(w) < minDstWizard || minDstWizard == 0)
                    {
                        minDstWizard = self.GetDistanceTo(w);
                        nearWizard = w;
                    }
                }

            bool isTurn = true;

            if (nearWizard != null)
            {
                double distWizard = self.GetDistanceTo(nearWizard);
                double distDangerMM;
                double distDangerFrost;
                double distDangerFireboll;

                distDangerMM = nearWizard.CastRange + self.Radius + 10;
                distDangerFrost = nearWizard.CastRange + self.Radius + 45;
                distDangerFireboll = nearWizard.CastRange + self.Radius + 50;

                bool gotoBack = false;

                if (self.Life < nearWizard.Life - 12 && self.Life < self.MaxLife * 0.5)
                {
                    gotoBack = true;
                }

                if (distWizard < nearWizard.CastRange + self.Radius + 50 && !isFrost(nearWizard))
                {

                    if (checkSkill(nearWizard, SkillType.FrostBolt) && distWizard < distDangerFrost)
                    {
                        int tickToFrost = Math.Max(nearWizard.RemainingActionCooldownTicks, nearWizard.RemainingCooldownTicksByAction[(int)ActionType.FrostBolt]);
                        if ((distDangerFrost - distWizard) / 3 > tickToFrost + 30)
                            gotoBack = true;
                    }
                    if (checkSkill(nearWizard, SkillType.Fireball) && distWizard < distDangerFireboll)
                    {
                        gotoBack = true;
                    }
                    foreach (Wizard w in world.Wizards)
                        if (w.Faction == enemy && self.GetDistanceTo(w) < self.VisionRange + 50)
                        {
                            if (isHitSumul(self, w, world, game))
                            {
                                nearWizard = w;
                                gotoBack = true;
                            }

                        }

                    if (isHitSumul(self, nearWizard, world, game))
                        gotoBack = true;
                }
                if (cntEnemyWiz == 1 && (self.Life > 80 || self.Life > nearWizard.Life + 20))
                    gotoBack = false;

                if (cntEnemyWiz == 1 && isFrost(nearWizard))
                    gotoBack = false;

                if (gotoBack || distWizard < 510 || (self.RemainingActionCooldownTicks > 15))
                {
                    Move m = gotoBackMove( new myLivingUnit(nearWizard), self, world, game);
                    move.Speed = m.Speed;
                    move.StrafeSpeed = m.StrafeSpeed;
                    move.Turn = m.Turn;
                    m = GetAction(target, self, world, game);
                    move.Action = m.Action;
                    move.CastAngle = m.CastAngle;
                    move.MinCastDistance = m.MinCastDistance;
                    if (Math.Max(self.RemainingActionCooldownTicks, self.RemainingCooldownTicksByAction[2]) > 15 || (longTargets.Count == 1 && !isHit(self, nearWizard, world, game)))
                    {
                        double an = self.GetAngleTo(nearWizard);
                        if (an > 0 && an < Math.PI / 2)
                            move.Turn = an - Math.PI / 2;
                        if (an < 0 && an > -Math.PI / 2)
                            move.Turn = an + Math.PI / 2;
                        isTurn = false;
                    }

                    return move;
                }
                if (Math.Max(self.RemainingActionCooldownTicks, self.RemainingCooldownTicksByAction[2]) > 15 || (checkSkill(self, SkillType.Fireball) && Math.Max(self.RemainingActionCooldownTicks, self.RemainingCooldownTicksByAction[(int)ActionType.Fireball]) > 15))
                {
                    double an = self.GetAngleTo(nearWizard);
                    if (an > 0 && an < Math.PI / 2)
                        move.Turn = -Math.PI / 2 + an;
                    if (an < 0 && an > -Math.PI / 2)
                        move.Turn = an + Math.PI / 2;
                    isTurn = false;
                }

                if (distWizard > 515 && distWizard < self.CastRange + nearWizard.Radius + 10)
                {
                    if (staffTargetUnit != null)
                        if (self.GetDistanceTo(staffTargetUnit.selfUnit) >= distWizard)
                        {
                            double anMove = self.GetAngleTo(nearWizard);
                            if (isTurn)
                                move.Turn = anMove;
                            move.Speed = getSpeed(anMove);
                            move.StrafeSpeed = getStrafeSpeed(anMove);
                            Move m = GetAction(target, self, world, game);
                            move.Action = m.Action;
                            move.CastAngle = m.CastAngle;
                            move.MinCastDistance = m.MinCastDistance;
                            return move;
                        }
                }
            }
            if (self.GetDistanceTo(400, 3600) < 100000)
                foreach (Wizard tr in world.Wizards)
                {
                    if (!tr.IsMe && self.GetDistanceTo(tr) < self.Radius * 2 + 10 && bonus1.tickToSpawn > 100 && Math.Abs(self.GetAngleTo(tr)) < Math.PI)
                    {
                        double ancl = self.GetAngleTo(tr);
                        move.StrafeSpeed = -getStrafeSpeed(ancl);
                        move.Speed = -getSpeed(ancl);
                        Move mCol = GetAction(target, self, world, game);
                        move.Action = mCol.Action;
                        move.CastAngle = mCol.CastAngle;
                        move.MinCastDistance = mCol.MinCastDistance;
                        return move;
                    }
                }

            if (staffTargetUnit == null && nearWizard != null)
            {
                staffTargetUnit = new myLivingUnit(nearWizard);
                target = new myPoint(nearWizard.X, nearWizard.Y);
                targetUnit = staffTargetUnit;
            }


            if (staffTargetUnit == null)//цель далека
            {

                myWizard mwz = new myWizard(self, game, world);
                Move m = GetBestMoveToPoint(mapPoint, mapPoint, self, world, game);
                move.Speed = m.Speed;
                move.StrafeSpeed = m.StrafeSpeed;
                if (isTurn)
                    move.Turn = m.Turn;
                m = GetAction(target, self, world, game);
                move.Action = m.Action;
                move.MinCastDistance = m.MinCastDistance;
                move.CastAngle = m.CastAngle;
                return move;
            }

            double anTarget = self.GetAngleTo(targetUnit.selfUnit);
            if (isTurn)
                move.Turn = anTarget;
            Move mTarget = GetAction(target, self, world, game);
            move.Action = mTarget.Action;
            move.MinCastDistance = mTarget.MinCastDistance;
            move.MaxCastDistance = mTarget.MaxCastDistance;
            move.CastAngle = mTarget.CastAngle;
            mTarget = gotoTarget(staffTargetUnit, self, world, game);
            move.Speed = mTarget.Speed;
            move.StrafeSpeed = mTarget.StrafeSpeed;

            return move;
        }


        void circle(int[,] buffer, int cx, int cy, int radius)
        {
            int error = -radius;
            int x = radius;
            int y = 0;

            while (x >= y)
            {
                int lastY = y;

                error += y;
                ++y;
                error += y;

                plot4points(buffer, cx, cy, x, lastY);

                if (error >= 0)
                {
                    if (x != lastY)
                        plot4points(buffer, cx, cy, lastY, x);

                    error -= x;
                    --x;
                    error -= x;
                }
            }
        }

        void plot8points(int[,] buffer, int cx, int cy, int x, int y)
        {
            plot4points(buffer, cx, cy, x, y);
            if (x != y) plot4points(buffer, cx, cy, y, x);
        }

        // The '(x != 0 && y != 0)' test in the last line of this function
        // may be omitted for a performance benefit if the radius of the
        // circle is known to be non-zero.
        static void plot4points(int[,] buffer, int cx, int cy, int x, int y)
        {
            horizontalLine(buffer, cx - x, cy + y, cx + x);
            if (x != 0 && y != 0)
                horizontalLine(buffer, cx - x, cy - y, cx + x);
        }

        static void setPixel(int[,] buffer, int x, int y)
        {
            if (y < buffer.GetUpperBound(1) - 1 && x < buffer.GetUpperBound(1) - 1 && x > 0 && y > 0)
                buffer[x, y]++;
        }

        static void horizontalLine(int[,] buffer, int x0, int y0, int x1)
        {
            for (int x = x0; x <= x1; ++x)
                setPixel(buffer, x, y0);
        }


        public int isNeedSafe(Wizard self, World world, Game game)
        {
            double myLife = self.Life;
            int myUnits = 0;
            //рядом башня
            myLivingUnit tower = null;
            if (backMove > 0)
                return -1;

            int cntMyMin = 0;
            int cntEnemyMin = 0;

            int cntEnemy = 0;

            dangerUnit = null;

            foreach (myLivingUnit lu in livingUnits)
                if (lu.faction == enemy && (lu.selfUnit.GetDistanceTo(self) < lu.damageRadius + self.Radius + 50 || lu.selfUnit.GetDistanceTo(self) < self.VisionRange))
                    cntEnemy++;

            foreach (myLivingUnit lu in livingUnits)
                if (self.GetDistanceTo(lu.selfUnit) < self.VisionRange)
                {
                    if (lu.faction == enemy && lu.tp == myLivingUnit.typeUnit.minion)
                        cntEnemyMin++;
                    if (lu.faction == self.Faction && lu.tp == myLivingUnit.typeUnit.minion)
                        cntMyMin++;
                }


            foreach (myLivingUnit lu in livingUnits)
            {
                if (self.GetDistanceTo(lu.selfUnit) <= lu.damageRadius + self.Radius + 5 && lu.faction == enemy && lu.tp == myLivingUnit.typeUnit.tower)
                {
                    tower = lu;

                    if (tower != null)
                    {
                        //проверка что мы в опасной зоне.

                        if ((self.Radius + tower.damageRadius - self.GetDistanceTo(tower.selfUnit)) / 3 > tower.RemainingActionCooldownTicks - 15)
                        {
                            if (myLife < tower.damage * 2)
                            {
                                dangerUnit = lu;
                                return -1;
                            }
                            myUnits = 0;
                            foreach (myLivingUnit myLu in livingUnits)
                                if (myLu.selfUnit.Life > tower.damage && myLu.faction == self.Faction && myLu.selfUnit.Id != self.Id && tower.selfUnit.GetDistanceTo(myLu.selfUnit) < tower.damageRadius)
                                    myUnits++;
                            if (myUnits == 0 && myLife < 50)
                            {
                                dangerUnit = lu;
                                return -1;
                            }
                            if (myUnits == 0 && world.TickIndex < 1250)
                            {
                                dangerUnit = lu;
                                return -1;
                            }
                        }
                        if (myLife < 50 && tower.RemainingActionCooldownTicks < 40)
                            return -1;
                        if (tower.damageRadius == 800 && tower.Life / tower.selfUnit.MaxLife > 0.2 && tower.RemainingActionCooldownTicks < 150 && myUnits < 2)
                        {
                            dangerUnit = lu;
                            return -1;
                        }
                    }
                }

            }
            //миньон фетиш

            foreach (myLivingUnit lu in livingUnits)
            {
                if (self.GetDistanceTo(lu.selfUnit) <= 350 && lu.faction == enemy && lu.tp == myLivingUnit.typeUnit.minion)
                {
                    if (lu.selfMinion.Type == MinionType.FetishBlowdart)
                    {
                        myUnits = 0;
                        foreach (myLivingUnit myLu in livingUnits)
                            if (myLu.faction == self.Faction && myLu.selfUnit.Id != self.Id && lu.selfUnit.GetDistanceTo(myLu.selfUnit) < lu.selfUnit.GetDistanceTo(self) - 10)
                                myUnits++;
                        if (myUnits < 2)
                        {
                            dangerUnit = lu;
                            return -1;
                        }
                    }
                }
            }
            //миньон дровосек
            foreach (myLivingUnit lu in livingUnits)
                if (self.GetDistanceTo(lu.selfUnit) <= 350 && lu.faction == enemy && lu.tp == myLivingUnit.typeUnit.minion)
                    if (lu.selfMinion.Type == MinionType.OrcWoodcutter)
                    {

                        myUnits = 0;
                        foreach (myLivingUnit myLu in livingUnits)
                            if (myLu.faction == self.Faction && myLu.selfUnit.Id != self.Id && lu.selfUnit.GetDistanceTo(myLu.selfUnit) < lu.selfUnit.GetDistanceTo(self) - 10)
                                myUnits++;

                        if (myUnits < 2 && cntEnemy > 1)
                        {
                            dangerUnit = lu;
                            return -1;
                        }
                    }



            if (self.Life < 40 && cntEnemy > 0)
                return -2;
            dangerUnit = null;
            return 0;
        }

        public bool isFrost(Wizard target)
        {
            foreach (Status st in target.Statuses)
                if (st.Type == StatusType.Frozen)
                    return true;
            return false;
        }

        public double getMaxDist(Wizard self, Wizard target)
        {
            if (isFrost(target))
                return self.CastRange - target.Radius;
            double dst = target.CastRange + self.Radius + 10;
            return dst;
        }

        public bool isHit(Wizard self, Wizard target, World world, Game game)
        {
            double pX = self.X;
            double pY = self.Y;
            double pSpeedX = Math.Cos(self.Angle + self.GetAngleTo(target)) * 40;
            double pSpeedY = Math.Sin(self.Angle + self.GetAngleTo(target)) * 40;


            if (isHitSumul(target, self, world, game))
                return true;


            double dist = self.GetDistanceTo(target);
            if (dist <= self.CastRange+target.Radius)//снаряд долетит до центра
            {
                return true;
            }
            if (dist >= self.CastRange + self.Radius + game.MagicMissileRadius)//снаряд долетит до центра
            {
                return false;
            }
            if (target.RemainingActionCooldownTicks < 5 && Math.Abs(target.GetAngleTo(self)) < game.StaffSector * 0.5 && dist <= 510)
                return true;
            return false;

            if (isFrost(target))
                return true;
            int tick = (int)(dist / 40d) + 1;
            double distStrafe = dist - self.CastRange - 10;
            double maxSpeed = 3;
            if ((int)(distStrafe / maxSpeed) > tick)
                return true;

            return false;
        }

        public bool isHaste(Wizard self)
        {
            foreach (Status st in self.Statuses)
                if (st.Type == StatusType.Hastened)
                    return true;
            return false;
        }

        public Move GetAction(myPoint target, Wizard self, World world, Game game)
        {
            Move res = new Move();
            res.Action = ActionType.None;
            if (self.RemainingActionCooldownTicks > 0) return res;
            double an;
            double castDist = 0;




            if (checkSkill(self, SkillType.Shield) && self.RemainingCooldownTicksByAction[(int)ActionType.Shield] == 0)
            {
                Wizard targetShield;
                foreach (Wizard w in world.Wizards)
                    if (w.Faction == self.Faction && !w.IsMe && self.GetDistanceTo(w) < self.CastRange && Math.Abs(self.GetAngleTo(w)) < game.StaffSector * 0.5)
                    {
                        res.Action = ActionType.Shield;
                        res.StatusTargetId = w.Id;
                        res.CastAngle = self.GetAngleTo(w);
                        return res;
                    }
            }


            if (checkSkill(self, SkillType.Fireball) && self.RemainingCooldownTicksByAction[(int)ActionType.Fireball] == 0 && self.Mana > game.FireballManacost)
            {
                Move moveF = GetFirebol(self, world, game);
                if (moveF.Action == ActionType.Fireball)
                {
                    res.Action = ActionType.Fireball;
                    res.CastAngle = moveF.CastAngle;
                    res.MinCastDistance = moveF.MinCastDistance;
                    res.MaxCastDistance = moveF.MaxCastDistance;
                    return res;
                }
            }

            if (staffTargetUnit != null)
            {
                an = self.GetAngleTo(staffTargetUnit.selfUnit);
                castDist = self.GetDistanceTo(staffTargetUnit.selfUnit) - staffTargetUnit.selfUnit.Radius;
                if (Math.Abs(an) < game.StaffSector * 0.5 && castDist < game.StaffRange && self.RemainingCooldownTicksByAction[1] == 0)//удар посохом
                {
                    res.Action = ActionType.Staff;
                    return res;
                }
            }
            if (longTargets.Count == 0)
                return res;
            //фрост-болт

            if (self.Mana > game.FrostBoltManacost && self.RemainingCooldownTicksByAction[(int)ActionType.FrostBolt] == 0 && longTargets.Count > 0 && checkSkill(self, SkillType.FrostBolt))
            {
                //сначала волшебник, который не может увернуться.
                foreach (myLivingUnit lu in longTargets)
                    if (lu.tp == myLivingUnit.typeUnit.wizard)
                        if (isHit(self, lu.selfWizard, world, game) && Math.Abs(self.GetAngleTo(lu.selfUnit)) < game.StaffSector * 0.5)
                        {
                            res.Action = ActionType.FrostBolt;
                            res.CastAngle = self.GetAngleTo(lu.selfUnit);
                            res.MinCastDistance = self.GetDistanceTo(lu.selfUnit) - lu.selfUnit.Radius + game.FrostBoltRadius;
                            return res;
                        }
                foreach (myLivingUnit lu in longTargets)
                    if (lu.tp == myLivingUnit.typeUnit.minion)
                        if (Math.Abs(self.GetAngleTo(lu.selfUnit)) < game.StaffSector * 0.5 && self.GetDistanceTo(lu.selfUnit) < self.CastRange && lu.selfMinion.Type == MinionType.FetishBlowdart)
                        {
                            res.Action = ActionType.FrostBolt;
                            res.CastAngle = self.GetAngleTo(lu.selfUnit);
                            res.MinCastDistance = self.GetDistanceTo(lu.selfUnit) - lu.selfUnit.Radius + game.FrostBoltRadius;
                            return res;
                        }
            }

            //маг. стрела
            if (self.RemainingCooldownTicksByAction[2] == 0 && longTargets.Count > 0)
            {
                //сначала волшебник, который не может увернуться.
                foreach (myLivingUnit lu in longTargets)
                    if (lu.tp == myLivingUnit.typeUnit.wizard)
                        if (isHit(self, lu.selfWizard, world, game) && Math.Abs(self.GetAngleTo(lu.selfUnit)) < game.StaffSector * 0.5)
                        {
                            res.Action = ActionType.MagicMissile;
                            res.CastAngle = self.GetAngleTo(lu.selfUnit);
                            res.MinCastDistance = self.GetDistanceTo(lu.selfUnit) - lu.selfUnit.Radius + game.MagicMissileRadius;
                            return res;
                        }

                //фетиш миньон
                foreach (myLivingUnit lu in longTargets)
                    if (lu.tp == myLivingUnit.typeUnit.minion)
                        if (Math.Abs(self.GetAngleTo(lu.selfUnit)) < game.StaffSector * 0.5 && self.GetDistanceTo(lu.selfUnit) < self.CastRange && lu.selfMinion.Type == MinionType.FetishBlowdart)
                        {
                            res.Action = ActionType.MagicMissile;
                            res.CastAngle = self.GetAngleTo(lu.selfUnit);
                            res.MinCastDistance = self.GetDistanceTo(lu.selfUnit) - lu.selfUnit.Radius + game.MagicMissileRadius;
                            return res;
                        }
                foreach (myLivingUnit lu in longTargets)
                    if (Math.Abs(self.GetAngleTo(lu.selfUnit)) < game.StaffSector * 0.5 && self.GetDistanceTo(lu.selfUnit) < self.CastRange)
                    {
                        res.Action = ActionType.MagicMissile;
                        res.CastAngle = self.GetAngleTo(lu.selfUnit);
                        res.MinCastDistance = self.GetDistanceTo(lu.selfUnit) - lu.selfUnit.Radius + game.MagicMissileRadius;
                        return res;
                    }
            }


            return res;
        }

        public Move GetBestMoveToPointBack(myPoint targetPoint, Wizard self, World word, Game game)
        {
            Move res = new Move();
            //найдем ближ. вей поинт
            myPoint p1 = midWay[0];
            myPoint p2 = midWay[0];
            double distTarget = self.GetDistanceTo(targetPoint.x, targetPoint.y);
            double dist = 0;
            res.Turn = self.GetAngleTo(targetPoint.x, targetPoint.y);
            for (int i = 2; i < 50; i++)
                if (dist == 0 || self.GetDistanceTo(midWay[i].x, midWay[i].y) < dist)
                {
                    p1 = midWay[i];
                    p2 = midWay[i - 1];
                    dist = self.GetDistanceTo(midWay[i].x, midWay[i].y);
                    if (dist < 10)
                        p2 = midWay[i - 2];
                }
            //vc.Line(self.X, self.Y, p1.x, p1.y, 1);
            //vc.Line(self.X, self.Y, p2.x, p2.y, 1);
            myWizard calcWiz = new myWizard(self, game, word);
            //проверка что можем пройти назад
            double an = self.GetAngleTo(p2.x, p2.y);
            calcWiz.m.Speed = getSpeed(an);
            calcWiz.m.StrafeSpeed = getStrafeSpeed(an);
            calcWiz.CalcMove((int)(self.GetDistanceTo(p2.x, p2.y) / 3)+2);
            if (!calcWiz.isCollision)
            {
                res.Speed = calcWiz.m.Speed;
                res.StrafeSpeed = calcWiz.m.StrafeSpeed;
                return res;
            }

            an = self.GetAngleTo(calcWiz.xCol, calcWiz.yCol);
            calcWiz = new myWizard(self, game, word);
            calcWiz.m.Speed = getStrafeSpeed(an);
            calcWiz.m.StrafeSpeed = getSpeed(an);
            calcWiz.CalcMove(5);
            if (!calcWiz.isCollision && calcWiz.getDistanceTo(targetPoint.x, targetPoint.y) > distTarget)
            {
                res.Speed = calcWiz.m.Speed;
                res.StrafeSpeed = calcWiz.m.StrafeSpeed;
                return res;
            }
            calcWiz = new myWizard(self, game, word);
            calcWiz.m.Speed = -getStrafeSpeed(an);
            calcWiz.m.StrafeSpeed = -getSpeed(an);
            calcWiz.CalcMove(5);
            if (!calcWiz.isCollision && calcWiz.getDistanceTo(targetPoint.x, targetPoint.y) > distTarget)
            {
                res.Speed = calcWiz.m.Speed;
                res.StrafeSpeed = calcWiz.m.StrafeSpeed;
                return res;
            }


            an = self.GetAngleTo(targetPoint.x, targetPoint.y);
            res.Speed = -getSpeed(an); ;
            res.StrafeSpeed = -getStrafeSpeed(an);
            return res;
        }
        public Move GetBestMoveToPoint(myPoint pointMap, myPoint pointTarget, Wizard wiz, World world, Game game)
        {


            Move res = new Move();

            if (targetUnit == null)
                pointTarget = pointMap;

            if (wiz.GetDistanceTo(pointMap.x, pointMap.y) < stepWord)
            {
                double angl = wiz.GetAngleTo(pointMap.x, pointMap.y);

                res.Speed = getSpeed(angl);
                res.StrafeSpeed = getStrafeSpeed(angl);
                return res;
            }

            int tX = (int)Math.Round(pointMap.x / stepWord);
            int tY = (int)Math.Round(pointMap.y / stepWord);

            int myBx = (int)Math.Round(wiz.X / stepWord);
            int myBy = (int)Math.Round(wiz.Y / stepWord);

            res.Turn = wiz.GetAngleTo(pointTarget.x, pointTarget.y);
            myWizard calcWiz = new myWizard(wiz, game, world);

            myWord[tX, tY] = 0;
            if (tX > 0 && tY > 0)
            {
                myWord[tX - 1, tY] = 0;
                myWord[tX, tY - 1] = 0;
                myWord[tX - 1, tY - 1] = 0;
            }
            if (tX < lenWord - 1 && tY < lenWord - 1)
            {
                myWord[tX + 1, tY] = 0;
                myWord[tX, tY + 1] = 0;
                myWord[tX + 1, tY + 1] = 0;
            }

            bool isnewTrace = false;
            if (Trace == null) isnewTrace = true;
            if (Trace != null)
                if (Trace[0].x != myBx || Trace[0].y != myBy || Trace.Count < 2)
                    isnewTrace = true;
            if (isnewTrace)
            {

                Trace = FindPath(myWord, new myPoint(myBx, myBy), new myPoint(tX, tY));
                if (Trace == null)
                {
                    res.Speed = 10;
                    res.Turn = 1;
                    pause = 10;
                    return res;
                }
                if (Trace.Count < 2)
                {
                    res.Speed = 10;
                    return res;
                }
            }

            myPoint p = Trace[1];
            calcWiz.X = myBx * stepWord;
            calcWiz.Y = myBy * stepWord;

            if (targetUnit == null)
            {
                res.Turn = wiz.GetAngleTo(p.x * stepWord, p.y * stepWord);
            }

            /*            foreach (myPoint pp in Trace)
                        {
                            Debug.fillRect(pp.x *40  - 20, pp.y * 40  - 20, pp.x * 40  + 20, pp.y * 40 + 20, 150);
                        }
                        Debug.endPost();
            */
            double an = wiz.GetAngleTo(p.x * stepWord, p.y * stepWord);

            res.Speed = getSpeed(an);
            res.StrafeSpeed = getStrafeSpeed(an);



            calcWiz = new myWizard(wiz, game, world);
            calcWiz.m.Speed = res.Speed;
            calcWiz.m.StrafeSpeed = res.StrafeSpeed;
            calcWiz.m.Turn = res.Turn;
            calcWiz.CalcMove(2);
            if (calcWiz.isCollision)
            {
                Trace = null;
                calcWiz.CorrectColision();
                res.Speed = calcWiz.m.Speed;
                res.StrafeSpeed = calcWiz.m.StrafeSpeed;
            }
            return res;
        }


        public myPoint getMapTargetBack(Wizard self, World world, Game game)
        {
            myPoint res = new myPoint();
            wayPoints = new myPoint[20];
            res.x = 50; res.y = 3800;

            int bX = (int)(self.X / 400);
            int bY = (int)(self.Y / 400);

            if (myLane == LaneType.Middle)
            {

                for (int i = 49; i < 1; i--)
                {
                    if (self.GetDistanceTo(midWay[i].x, midWay[i].y) < 60)
                    {
                        res.x = midWay[i - 1].x; res.y = midWay[i - 1].y;
                        break;
                    }
                    double distToFinish = self.GetDistanceTo(midWay[0].x, midWay[0].y);
                    if (getDistance(midWay[i].x, midWay[i].y, midWay[0].x, midWay[0].y) < distToFinish)
                    {
                        res.x = midWay[i].x; res.y = midWay[i].y;
                        break;
                    }

                }

            }
            return res;

        }

        public myPoint getMapTarget(Wizard self, World world, Game game)
        {
            myPoint res = new myPoint();
            wayPoints = new myPoint[20];
            res.x = 3500; res.y = 800;

            int bX = (int)(self.X / 400);
            int bY = (int)(self.Y / 400);

            if (myLane == LaneType.Top)
            {
                wayPoints[0] = new myPoint(200, 3800);
                wayPoints[1] = new myPoint(200, 3200);
                wayPoints[2] = new myPoint(200, 3000);
                wayPoints[3] = new myPoint(200, 2600);
                wayPoints[4] = new myPoint(200, 2200);
                wayPoints[5] = new myPoint(200, 1800);
                wayPoints[6] = new myPoint(200, 1400);
                wayPoints[7] = new myPoint(200, 1000);
                wayPoints[8] = new myPoint(200, 600);
                wayPoints[9] = new myPoint(600, 200);
                wayPoints[10] = new myPoint(1200, 200);
                wayPoints[11] = new myPoint(1200, 200);
                wayPoints[12] = new myPoint(2200, 200);
                wayPoints[13] = new myPoint(3600, 200);


                bool isZapuntka = false;

                if (bX == 3 && bY == 2)
                {
                    isZapuntka = true;
                    res.x = 2 * 400 + 200;
                    res.y = 2 * 400 + 200;
                }
                if (bX == 2 && bY == 2)
                {
                    isZapuntka = true;
                    res.x = 200;
                    res.y = 200;
                }
                if (bX == 1 && bY == 1)
                {
                    isZapuntka = true;
                    res.x = 200;
                    res.y = 200;
                }
                if (!isZapuntka)
                    for (int i = 0; i < 12; i++)
                    {
                        if (self.GetDistanceTo(wayPoints[i].x, wayPoints[i].y) < 200)
                        {
                            res.x = wayPoints[i + 1].x; res.y = wayPoints[i + 1].y;
                            break;
                        }
                        double distToFinish = self.GetDistanceTo(wayPoints[13].x, wayPoints[13].y);
                        if (getDistance(wayPoints[i].x, wayPoints[i].y, wayPoints[13].x, wayPoints[13].y) < distToFinish)
                        {
                            res.x = wayPoints[i].x; res.y = wayPoints[i].y;
                            break;
                        }

                    }
            }
            if (myLane == LaneType.Bottom)
            {
                wayPoints[0] = new myPoint(200, 3800);
                wayPoints[1] = new myPoint(1200, 3800);
                wayPoints[2] = new myPoint(2400, 3800);
                wayPoints[3] = new myPoint(3200, 3800);
                wayPoints[4] = new myPoint(3800, 3800);
                wayPoints[5] = new myPoint(3800, 3200);
                wayPoints[6] = new myPoint(3800, 2400);
                wayPoints[7] = new myPoint(3800, 1200);
                wayPoints[8] = new myPoint(3800, 600);
                bool isZapuntka = false;

                if (bX == 8 && bY == 8)
                {
                    isZapuntka = true;
                    res.x = 3800;
                    res.y = 3700;
                }
                if (bX == 8 && bY == 7)
                {
                    isZapuntka = true;
                    res.x = 3400;
                    res.y = 3800;
                }
                if (bX == 7 && bY == 7)
                {
                    isZapuntka = true;
                    res.x = 3800;
                    res.y = 3700;
                }
                if (!isZapuntka)
                    for (int i = 0; i < 8; i++)
                    {
                        if (self.GetDistanceTo(wayPoints[i].x, wayPoints[i].y) < 100)
                        {
                            res.x = wayPoints[i + 1].x; res.y = wayPoints[i + 1].y;
                            break;
                        }
                        double distToFinish = self.GetDistanceTo(wayPoints[8].x, wayPoints[8].y);
                        if (getDistance(wayPoints[i].x, wayPoints[i].y, wayPoints[8].x, wayPoints[8].y) < distToFinish)
                        {
                            res.x = wayPoints[i].x; res.y = wayPoints[i].y;
                            break;
                        }

                    }
            }
            if (myLane == LaneType.Middle)
            {

                for (int i = 0; i < 49; i++)
                {
                    if (self.GetDistanceTo(midWay[i].x, midWay[i].y) < 60)
                    {
                        res.x = midWay[i + 1].x; res.y = midWay[i + 1].y;
                        break;
                    }
                    double distToFinish = self.GetDistanceTo(midWay[49].x, midWay[49].y);
                    if (getDistance(midWay[i].x, midWay[i].y, midWay[49].x, midWay[49].y) < distToFinish)
                    {
                        res.x = midWay[i].x; res.y = midWay[i].y;
                        break;
                    }

                }

            }
            return res;

        }

        public bool isUnDeadTower(myLivingUnit lu)
        {
            if (lu.tp != myLivingUnit.typeUnit.tower)
                return false;
            if (lu.selfUnit.Y == 350 && isTop1)
                return true;
            if (lu.selfUnit.X == 3950 && isBottom1)
                return true;
            return false;
        }

        public myPoint getNearTarget(Wizard self, World world, Game game)
        {
            myPoint res = new myPoint();
            double dist = 0;
            targetUnit = null;
            dangerUnit = null;
            longTargets = new List<myLivingUnit>();
            staffTargetUnit = null;
            cntEnemy = 0;



            dist = self.GetDistanceTo(3600, 400) + 80000;
            res.x = 3600; res.y = 400;

            //ищем ближайшую цель для посоха
            double minDst = 100000;
            foreach (myLivingUnit lu in livingUnits)
                if (lu.faction == enemy && self.GetDistanceTo(lu.selfUnit) < minDst && self.GetDistanceTo(lu.selfUnit) < self.CastRange && !isUnDeadTower(lu))
                {
                    minDst = self.GetDistanceTo(lu.selfUnit);
                    staffTargetUnit = lu;
                    targetUnit = lu;
                    res.x = lu.X;
                    res.y = lu.Y;
                }

            if (staffTargetUnit != null)
                if (minDst < 100)
                {
                    longTargets.Add(targetUnit);
                    return res;
                }


            //визард по которому 100% попадем
            foreach (Wizard w in world.Wizards)
                if (w.Faction != self.Faction && self.GetDistanceTo(w) < self.VisionRange && isHit(self, w, world, game))
                {
                    targetUnit = new myLivingUnit(w);
                    longTargets.Add(targetUnit);
                    res.x = w.X;
                    res.y = w.Y;
                    return res;
                }

            if (staffTargetUnit != null)
                if (minDst < 200)
                {
                    longTargets.Add(targetUnit);
                    return res;
                }

            //башня
            foreach (myLivingUnit lu in livingUnits)
                if (lu.faction == enemy && lu.tp == myLivingUnit.typeUnit.tower && self.GetDistanceTo(lu.selfUnit) < self.CastRange + lu.selfUnit.Radius && !isUnDeadTower(lu))
                {
                    targetUnit = lu;
                    res.x = lu.X;
                    res.y = lu.Y;
                    longTargets.Add(targetUnit);
                    return res;
                }

            //миньоны фетиши, которые могут по нам попасть
            foreach (myLivingUnit lu in livingUnits)
                if (lu.faction == enemy && lu.tp == myLivingUnit.typeUnit.minion && self.GetDistanceTo(lu.selfUnit) < lu.damageRadius + self.Radius)
                {
                    targetUnit = lu;
                    res.x = lu.X;
                    res.y = lu.Y;
                    longTargets.Add(targetUnit);
                    return res;
                }
            //любой фетишь с мин. здоровьем
            double minLife = 100;
            foreach (myLivingUnit lu in livingUnits)
                if (lu.faction == enemy && lu.tp == myLivingUnit.typeUnit.minion && self.GetDistanceTo(lu.selfUnit) < self.CastRange && minLife >= lu.selfUnit.Life)
                {
                    targetUnit = lu;
                    res.x = lu.X;
                    res.y = lu.Y;
                    minLife = lu.selfUnit.Life;
                    longTargets.Add(targetUnit);
                }


            return res;
        }



        public void checkWorld(Wizard self, World world, Game game)
        {
            //           Debug.beginPost();

            isTop1 = false;
            isBottom1 = false;



            lifeTron = 100;

            livingUnits = null;
            longTargets = new List<myLivingUnit>();
            int cnt = 0;
            bool isAddMainTower, isAddMiddleTower1, isAddMiddleTower2, isAddTopTower1, isAddTopTower2, isAddBottomTower1, isAddBottomTower2;
            speedFactor = 1;
            if (self.Skills.Length > 0)
                foreach (SkillType st in self.Skills)
                {
                    if (st == SkillType.MovementBonusFactorPassive1)
                        speedFactor = speedFactor + game.MovementBonusFactorPerSkillLevel;
                    if (st == SkillType.MovementBonusFactorPassive2)
                        speedFactor = speedFactor + game.MovementBonusFactorPerSkillLevel;
                    if (st == SkillType.MovementBonusFactorAura1)
                        speedFactor = speedFactor + game.MovementBonusFactorPerSkillLevel;
                    if (st == SkillType.MovementBonusFactorAura2)
                        speedFactor = speedFactor + game.MovementBonusFactorPerSkillLevel;
                }



            if (self.Statuses.Length > 0)
                foreach (Status st in self.Statuses)
                {
                    if (st.Type == StatusType.Hastened)
                        speedFactor = speedFactor + game.HastenedMovementBonusFactor;
                }




            bonus1.init(world.TickIndex);
            bonus2.init(world.TickIndex);


            isAddMiddleTower1 = false;
            isAddMiddleTower2 = false;
            isAddTopTower1 = false;
            isAddTopTower2 = false;
            isAddMainTower = false;
            isAddBottomTower1 = false;
            isAddBottomTower2 = false;

            if (self.GetDistanceTo(2070, 1600) < self.VisionRange - 50)
                isMiddleTower1 = false;
            if (self.GetDistanceTo(3130, 1200) < self.VisionRange - 50)
                isMiddleTower2 = false;
            if (self.GetDistanceTo(1688, 50) < self.VisionRange - 50)
                isTopTower1 = false;
            if (self.GetDistanceTo(2629.34, 350) < self.VisionRange - 50)
                isTopTower2 = false;
            if (self.GetDistanceTo(3650, 2343) < self.VisionRange - 50)
                isBottomTower1 = false;
            if (self.GetDistanceTo(3950, 1307) < self.VisionRange - 50)
                isBottomTower2 = false;

            foreach (Minion m in world.Minions)
                if (m.Faction == self.Faction)
                {
                    if (m.GetDistanceTo(2070, 1600) < m.VisionRange - 50)
                        isMiddleTower1 = false;
                    if (m.GetDistanceTo(3130, 1200) < m.VisionRange - 50)
                        isMiddleTower2 = false;
                    if (m.GetDistanceTo(1688, 50) < m.VisionRange - 50)
                        isTopTower1 = false;
                    if (m.GetDistanceTo(2629.34, 350) < m.VisionRange - 50)
                        isTopTower2 = false;
                    if (m.GetDistanceTo(3650, 2343) < m.VisionRange - 50)
                        isBottomTower1 = false;
                    if (m.GetDistanceTo(3950, 1307) < m.VisionRange - 50)
                        isBottomTower2 = false;
                }
            foreach (Wizard m in world.Wizards)
                if (m.Faction == self.Faction)
                {
                    if (m.GetDistanceTo(2070, 1600) < m.VisionRange - 50)
                        isMiddleTower1 = false;
                    if (m.GetDistanceTo(3130, 1200) < m.VisionRange - 50)
                        isMiddleTower2 = false;
                    if (m.GetDistanceTo(1688, 50) < m.VisionRange - 50)
                        isTopTower1 = false;
                    if (m.GetDistanceTo(2629.34, 350) < m.VisionRange - 50)
                        isTopTower2 = false;
                    if (m.GetDistanceTo(3650, 2343) < m.VisionRange - 50)
                        isBottomTower1 = false;
                    if (m.GetDistanceTo(3950, 1307) < m.VisionRange - 50)
                        isBottomTower2 = false;
                }



            foreach (Building b in world.Buildings)
            {
                cnt++;
                myLivingUnit lu = new myLivingUnit(b);
                Array.Resize<myLivingUnit>(ref livingUnits, cnt);
                livingUnits[cnt - 1] = lu;
                if (b.GetDistanceTo(2070, 1600) < b.Radius + 50)
                {
                    isMiddleTower1 = true;
                    isAddMiddleTower1 = true;
                }
                if (b.GetDistanceTo(3130, 1200) < b.Radius + 50)
                {
                    isMiddleTower2 = true;
                    isAddMiddleTower2 = true;
                }
                if (b.GetDistanceTo(1688, 50) < b.Radius + 50)
                {
                    isTopTower1 = true;
                    isAddTopTower1 = true;
                }
                if (b.GetDistanceTo(2629.34, 350) < b.Radius + 50)
                {
                    isTopTower2 = true;
                    isAddTopTower2 = true;
                }
                if (b.GetDistanceTo(3650, 2343) < b.Radius + 50)
                {
                    isBottomTower1 = true;
                    isAddBottomTower1 = true;
                }
                if (b.GetDistanceTo(3950, 1307) < b.Radius + 50)
                {
                    isBottomTower2 = true;
                    isAddBottomTower2 = true;
                }

                if (b.GetDistanceTo(3600, 400) < b.Radius + 50)
                {
                    isAddMainTower = true;
                }
                if (b.Y == 400)
                    lifeTron = b.Life / 1000d * 100;
            }

            Building bu;
            Faction ef = Faction.Renegades;
            if (self.Faction == Faction.Renegades)
                ef = Faction.Academy;

            //добавляем башни
            if (!isAddMiddleTower1 && isMiddleTower1)
            {
                bu = new Building(0, 2070, 1600, 0, 0, 0, ef, 50, (int)game.GuardianTowerLife, (int)game.GuardianTowerLife, self.Statuses, BuildingType.GuardianTower, 600, 600, 36, game.GuardianTowerCooldownTicks, 0);
                cnt++;
                myLivingUnit lu = new myLivingUnit(bu);
                Array.Resize<myLivingUnit>(ref livingUnits, cnt);
                livingUnits[cnt - 1] = lu;
            }
            if (!isAddMiddleTower2 && isMiddleTower2)
            {
                bu = new Building(0, 3130, 1200, 0, 0, 0, ef, 50, (int)game.GuardianTowerLife, (int)game.GuardianTowerLife, self.Statuses, BuildingType.GuardianTower, 600, 600, 36, game.GuardianTowerCooldownTicks, 0);
                cnt++;
                myLivingUnit lu = new myLivingUnit(bu);
                Array.Resize<myLivingUnit>(ref livingUnits, cnt);
                livingUnits[cnt - 1] = lu;
            }
            if (!isAddTopTower1 && isTopTower1)
            {
                bu = new Building(0, 1688, 50, 0, 0, 0, ef, 50, (int)game.GuardianTowerLife, (int)game.GuardianTowerLife, self.Statuses, BuildingType.GuardianTower, 600, 600, 36, game.GuardianTowerCooldownTicks, 0);
                cnt++;
                myLivingUnit lu = new myLivingUnit(bu);
                Array.Resize<myLivingUnit>(ref livingUnits, cnt);
                livingUnits[cnt - 1] = lu;
            }
            if (!isAddTopTower2 && isTopTower2)
            {
                bu = new Building(0, 2629.34, 350, 0, 0, 0, ef, 50, (int)game.GuardianTowerLife, (int)game.GuardianTowerLife, self.Statuses, BuildingType.GuardianTower, 600, 600, 36, game.GuardianTowerCooldownTicks, 0);
                cnt++;
                myLivingUnit lu = new myLivingUnit(bu);
                Array.Resize<myLivingUnit>(ref livingUnits, cnt);
                livingUnits[cnt - 1] = lu;
            }
            if (!isAddBottomTower1 && isBottomTower1)
            {
                bu = new Building(0, 3650, 2343, 0, 0, 0, ef, 50, (int)game.GuardianTowerLife, (int)game.GuardianTowerLife, self.Statuses, BuildingType.GuardianTower, 600, 600, 36, game.GuardianTowerCooldownTicks, 0);
                cnt++;
                myLivingUnit lu = new myLivingUnit(bu);
                Array.Resize<myLivingUnit>(ref livingUnits, cnt);
                livingUnits[cnt - 1] = lu;
            }
            if (!isAddBottomTower2 && isBottomTower2)
            {
                bu = new Building(0, 3950, 1307, 0, 0, 0, ef, 50, (int)game.GuardianTowerLife, (int)game.GuardianTowerLife, self.Statuses, BuildingType.GuardianTower, 600, 600, 36, game.GuardianTowerCooldownTicks, 0);
                cnt++;
                myLivingUnit lu = new myLivingUnit(bu);
                Array.Resize<myLivingUnit>(ref livingUnits, cnt);
                livingUnits[cnt - 1] = lu;
            }
            if (!isAddMainTower)
            {
                bu = new Building(0, 3600, 400, 0, 0, 0, ef, 50, (int)game.FactionBaseLife, (int)game.FactionBaseLife, self.Statuses, BuildingType.FactionBase, 800, 800, 48, game.FactionBaseCooldownTicks, 0);
                cnt++;
                myLivingUnit lu = new myLivingUnit(bu);
                Array.Resize<myLivingUnit>(ref livingUnits, cnt);
                livingUnits[cnt - 1] = lu;
            }

            foreach (Wizard w in world.Wizards)
            {
                if (!w.IsMe)
                {
                    cnt++;
                    myLivingUnit lu = new myLivingUnit(w);
                    Array.Resize<myLivingUnit>(ref livingUnits, cnt);
                    livingUnits[cnt - 1] = lu;
                }
                if (w.GetDistanceTo(bonus1.x, bonus1.y) < w.VisionRange - 100 && bonus1.isGo && bonus1.tickAfterSpawn > 0)
                    bonus1.isGo = false;
                if (w.GetDistanceTo(bonus2.x, bonus2.y) < w.VisionRange - 100 && bonus2.isGo && bonus2.tickAfterSpawn > 0)
                    bonus2.isGo = false;
            }

            if (self.GetDistanceTo(bonus1.x, bonus1.y) < self.VisionRange - 100 && bonus1.isGo && bonus1.tickAfterSpawn > 0)
                bonus1.isGo = false;
            if (self.GetDistanceTo(bonus2.x, bonus2.y) < self.VisionRange - 100 && bonus2.isGo && bonus2.tickAfterSpawn > 0)
                bonus2.isGo = false;




            foreach (Minion m in world.Minions)
            {
                cnt++;
                myLivingUnit lu = new myLivingUnit(m);
                Array.Resize<myLivingUnit>(ref livingUnits, cnt);
                livingUnits[cnt - 1] = lu;
            }

            if (world.TickIndex % 20 == 0 || myWord == null)
            {
                myWord = new int[lenWord, lenWord];


                for (int x = 0; x < lenWord; x++)
                    for (int y = 0; y < lenWord; y++)
                    {
                        myWord[x, y] = 0;
                        foreach (Tree un in world.Trees)
                            if (self.GetDistanceTo(un) < 700)
                            {
                                if (un.GetDistanceTo(x * stepWord, y * stepWord) <= un.Radius + self.Radius + 30)
                                    myWord[x, y] = 1;
                            }
                        foreach (myLivingUnit un in livingUnits)
                            if (self.GetDistanceTo(un.selfUnit) < 400 && un.tp == myLivingUnit.typeUnit.tower)
                            {
                                if (un.selfUnit.GetDistanceTo(x * stepWord, y * stepWord) <= un.selfUnit.Radius + self.Radius)
                                    myWord[x, y] = 1;
                            }
                        foreach (myLivingUnit un in livingUnits)
                            if (self.GetDistanceTo(un.selfUnit) < 400 && un.tp == myLivingUnit.typeUnit.minion && un.faction == Faction.Neutral)
                            {
                                if (un.selfUnit.GetDistanceTo(x * stepWord, y * stepWord) <= un.selfUnit.Radius + self.Radius)
                                    myWord[x, y] = 1;
                            }
                    }



            }

            if (world.Projectiles.Length == 0)
                projectives = new List<myProjective>();

            foreach (Projectile p in world.Projectiles)
                if (p.Faction != self.Faction && self.GetDistanceTo(p) < 650 && p.Type != ProjectileType.Dart)
                {
                    bool isp = false;
                    foreach (myProjective mp in projectives)
                        if (mp.Id == p.Id)
                            isp = true;
                    if (!isp)
                        projectives.Add(new myProjective(p, world));
                }

            foreach (myProjective mp in projectives)
            {
                bool isP = false;
                foreach (Projectile p in world.Projectiles)
                    if (p.Id == mp.Id)
                        isP = true;
                if (!isP)
                {
                    projectives.Remove(mp);
                    break;
                }

            }


            //CalcField(self, world, game);


            /*
                        for (int x = 0; x < lenWord; x++)
                            for (int y = 0; y < lenWord; y++)
                                if (myWord[x, y] == 1)
                                    Debug.rect(x * stepWord - stepWord / 2d, y * stepWord - stepWord / 2d, x * stepWord + stepWord / 2d, y * stepWord + stepWord / 2d, 150);


                        Debug.endPost();

            */


        }



        public double getDistance(double x1, double y1, double x2, double y2)
        {
            return Math.Sqrt((x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2));
        }

        public void initGame(Wizard self, World world, Game game)
        {
            //vc = new VisualClient("127.0.0.1", 13579);

            projectives = new List<myProjective>();

            lifeTron = 1000;

            enemy = Faction.Academy;
            if (self.Faction == Faction.Academy)
                enemy = Faction.Renegades;

            bonus1 = new myBonus(1);
            bonus2 = new myBonus(2);



            myLane = LaneType.Middle; //идем по средней линии.
            gotoBonus = false;

            backMove = 0;

            if (self.X == 300)
                myLane = LaneType.Bottom;
            if (self.X == 200)
                myLane = LaneType.Middle;
            if (self.X == 100)
                myLane = LaneType.Top;

            myLane = LaneType.Middle;

            pause = 20;


            isMiddleTower1 = true;
            isMiddleTower2 = true;

            isTopTower1 = true;
            isTopTower2 = true;

            isBottomTower1 = true;
            isBottomTower2 = true;


            safePoints = new myPoint[200];
            int cnt = -1;
            for (int i = 1; i < 20; i++)
            {
                if (i != 2 && i < 15)
                {
                    cnt++;
                    safePoints[cnt] = new myPoint(i * 200, 4000 - 200 * i);
                }
                cnt++;
                safePoints[cnt] = new myPoint(200, 4000 - 200 * i);
                cnt++;
                safePoints[cnt] = new myPoint(i * 200, 200);
                cnt++;
                safePoints[cnt] = new myPoint(i * 200, 3800);
                cnt++;
                safePoints[cnt] = new myPoint(3800, 4000 - 200 * i);

                cnt++;
                safePoints[cnt] = new myPoint(200 * i, 200 * i);

            }

            midWay = new myPoint[50];
            if (self.X == 100 || self.X == 200)
            {
                midWay[0] = new myPoint(self.X, 3600);
                midWay[1] = new myPoint(self.X, 3500);
                midWay[2] = new myPoint(self.X, 3400);
                midWay[3] = new myPoint(400, 3400);
            }
            else
            {
                midWay[0] = new myPoint(400, self.Y);
                midWay[1] = new myPoint(500, self.Y);
                midWay[2] = new myPoint(600, self.Y);
                midWay[3] = new myPoint(600, 3400);
            }
            double startX = 500;
            double startY = 3400;
            for (int i = 4; i < 50; i++)
            {
                midWay[i] = new myPoint(startX + 60 * i, startY - 60 * i);
                if (i == 28)
                    startX = 700;
            }
        }

        public static List<myPoint> FindPath(int[,] field, myPoint start, myPoint goal)
        {
            // Шаг 1.
            var closedSet = new Collection<PathNode>();
            var openSet = new Collection<PathNode>();
            // Шаг 2.
            PathNode startNode = new PathNode()
            {
                Position = start,
                CameFrom = null,
                PathLengthFromStart = 0,
                HeuristicEstimatePathLength = GetHeuristicPathLength(start, goal)
            };
            openSet.Add(startNode);
            while (openSet.Count > 0)
            {
                // Шаг 3.
                if (closedSet.Count > 500)
                    return null;
                var currentNode = openSet.OrderBy(node =>
                  node.EstimateFullPathLength).First();
                // Шаг 4.
                if (currentNode.Position.x == goal.x && currentNode.Position.y == goal.y)
                    return GetPathForNode(currentNode);
                // Шаг 5.
                openSet.Remove(currentNode);
                closedSet.Add(currentNode);
                // Шаг 6.
                foreach (var neighbourNode in GetNeighbours(currentNode, goal, field))
                {
                    // Шаг 7.
                    if (closedSet.Count(node => (node.Position.x == neighbourNode.Position.x && node.Position.y == neighbourNode.Position.y)) > 0)
                        continue;
                    var openNode = openSet.FirstOrDefault(node =>
                      (node.Position.x == neighbourNode.Position.x && node.Position.y == neighbourNode.Position.y));
                    // Шаг 8.
                    if (openNode == null)
                        openSet.Add(neighbourNode);
                    else
                      if (openNode.PathLengthFromStart > neighbourNode.PathLengthFromStart)
                    {
                        // Шаг 9.
                        openNode.CameFrom = currentNode;
                        openNode.PathLengthFromStart = neighbourNode.PathLengthFromStart;
                    }
                }
            }
            // Шаг 10.
            return null;
        }
        private static int GetDistanceBetweenNeighbours()
        {
            return 1;
        }
        private static int GetHeuristicPathLength(myPoint from, myPoint to)
        {
            if ((int)(Math.Abs(from.x - to.x) + Math.Abs(from.y - to.y)) == 1)
                return 2;
            if ((int)(Math.Abs(from.x - to.x)) == 1 && (int)(Math.Abs(from.y - to.y)) == 1)
                return 1;
            return (int)(Math.Abs(from.x - to.x) + Math.Abs(from.y - to.y));
        }
        private static Collection<PathNode> GetNeighbours(PathNode pathNode,
          myPoint goal, int[,] field)
        {
            var result = new Collection<PathNode>();

            // Соседними точками являются соседние по стороне клетки.
            myPoint[] neighbourPoints = new myPoint[8];
            neighbourPoints[0] = new myPoint(pathNode.Position.x + 1, pathNode.Position.y);
            neighbourPoints[1] = new myPoint(pathNode.Position.x - 1, pathNode.Position.y);
            neighbourPoints[2] = new myPoint(pathNode.Position.x, pathNode.Position.y + 1);
            neighbourPoints[3] = new myPoint(pathNode.Position.x, pathNode.Position.y - 1);
            neighbourPoints[4] = new myPoint(pathNode.Position.x + 1, pathNode.Position.y - 1);
            neighbourPoints[5] = new myPoint(pathNode.Position.x - 1, pathNode.Position.y - 1);
            neighbourPoints[6] = new myPoint(pathNode.Position.x - 1, pathNode.Position.y + 1);
            neighbourPoints[7] = new myPoint(pathNode.Position.x + 1, pathNode.Position.y + 1);

            foreach (var point in neighbourPoints)
            {
                // Проверяем, что не вышли за границы карты.
                if (point.x < 0 || point.x >= field.GetLength(0))
                    continue;
                if (point.y < 0 || point.y >= field.GetLength(1))
                    continue;
                // Проверяем, что по клетке можно ходить.
                if ((field[(int)point.x, (int)point.y] == 1))
                    continue;
                // Заполняем данные для точки маршрута.
                var neighbourNode = new PathNode()
                {
                    Position = point,
                    CameFrom = pathNode,
                    PathLengthFromStart = pathNode.PathLengthFromStart +
                    GetDistanceBetweenNeighbours(),
                    HeuristicEstimatePathLength = GetHeuristicPathLength(point, goal)
                };
                result.Add(neighbourNode);
            }
            return result;
        }
        private static List<myPoint> GetPathForNode(PathNode pathNode)
        {
            var result = new List<myPoint>();
            var currentNode = pathNode;
            while (currentNode != null)
            {
                result.Add(currentNode.Position);
                currentNode = currentNode.CameFrom;
            }
            result.Reverse();
            return result;
        }

        public bool isIntersecObject(LivingUnit un, myPoint start, myPoint end, double rad = 0)
        {
            double x01 = start.x - un.X;
            double y01 = start.y - un.Y;
            double x02 = end.x - un.X;
            double y02 = end.y - un.Y;

            double dx = x02 - x01;
            double dy = y02 - y01;

            double a = dx * dx + dy * dy;
            double b = 2.0 * (x01 * dx + y01 * dy);
            double c = x01 * x01 + y01 * y01 - (un.Radius + rad) * (un.Radius + rad);
            if (-b < 0) return (c < 0);
            if (-b < (2.0d * a)) return (4.0d * a * c - b * b < 0);
            return (a + b + c < 0);
        }
        public bool isIntersecObject(double unX, double unY, double unRad, myPoint start, myPoint end)
        {
            double x01 = start.x - unX;
            double y01 = start.y - unY;
            double x02 = end.x - unX;
            double y02 = end.y - unY;

            double dx = x02 - x01;
            double dy = y02 - y01;

            double a = dx * dx + dy * dy;
            double b = 2.0 * (x01 * dx + y01 * dy);
            double c = x01 * x01 + y01 * y01 - (unRad) * (unRad);
            if (-b < 0) return (c < 0);
            if (-b < (2.0d * a)) return (4.0d * a * c - b * b < 0);
            return (a + b + c < 0);
        }


        private void CalcField(Wizard self, World world, Game game)
        {

            fieldDamage = new int[lenField, lenField];
            int[,] preFied = new int[lenField, lenField];
            Faction enemy = Faction.Renegades;
            if (self.Faction == Faction.Renegades)
                enemy = Faction.Academy;
            for (int x = 0; x < lenField; x++)
                for (int y = 0; y < lenField; y++)
                {
                    double abX = self.X + (x - lenField / 2) * stepField;
                    if (abX < 0) abX = 0;
                    double abY = self.Y + (y - lenField / 2) * stepField;
                    if (abY < 0) abY = 0;
                    preFied[x, y] = 0;

                    int bX = (int)(abX / 400);
                    int bY = (int)(abY / 400);
                    if (bX == 4 && bY == 3)
                        preFied[x, y] = 100;
                    if (bX == 3 && bY == 4)
                        preFied[x, y] = 100;
                    if (bX == 4 && bY == 4)
                        preFied[x, y] = (int)(self.GetDistanceTo(2000, 2000) * 0.1);
                    if (bX == 5 && bY == 5)
                        preFied[x, y] = (int)(self.GetDistanceTo(1600, 2400) * 0.1);

                    int tickToField = (int)Math.Round(self.GetDistanceTo(abX, abY) / 3) + 1;


                    if (abX < 400 && abY < 400)
                        preFied[x, y] = preFied[x, y] + 160000 - ((int)abX * (int)abY);
                    if (abY < 200)
                        preFied[x, y] = preFied[x, y] + 200 - (int)abY;
                    if (abX < 200)
                        preFied[x, y] = preFied[x, y] + 200 - (int)abX;
                    foreach (Tree un in world.Trees)
                    {
                        if (un.GetDistanceTo(abX, abY) + un.Radius < 200)
                        {
                            if (un.GetDistanceTo(abX, abY) < self.Radius + un.Radius + 6)
                                preFied[x, y] = preFied[x, y] + 150;
                            else
                                preFied[x, y] = preFied[x, y] + 100 - (int)(un.GetDistanceTo(abX, abY) * 0.5);
                        }
                    }
                    foreach (myLivingUnit un in livingUnits)
                    {
                        if (un.selfUnit.GetDistanceTo(abX, abY) + un.selfUnit.Radius < 200)
                        {
                            if (un.selfUnit.GetDistanceTo(abX, abY) < self.Radius + un.selfUnit.Radius + 8)
                                preFied[x, y] = preFied[x, y] + 300;
                            if (un.tp != myLivingUnit.typeUnit.tower)
                                preFied[x, y] = preFied[x, y] + 20 - (int)(un.selfUnit.GetDistanceTo(abX, abY) * 0.1);
                            else
                                preFied[x, y] = preFied[x, y] + (int)(un.selfUnit.GetDistanceTo(abX, abY) * 0.1) - 20;

                        }
                        if (un.faction == enemy)
                        {
                            if (un.selfUnit.GetDistanceTo(abX, abY) < un.damageRadius + self.Radius + 20)
                                preFied[x, y] = preFied[x, y] + 20;
                            if (un.tp == myLivingUnit.typeUnit.wizard && un.selfUnit.GetDistanceTo(abX, abY) < un.damageRadius + self.Radius + 20)
                                preFied[x, y] = preFied[x, y] + 150;
                            if (un.tp == myLivingUnit.typeUnit.tower && un.selfUnit.GetDistanceTo(abX, abY) < un.damageRadius + self.Radius + 20)
                                preFied[x, y] = preFied[x, y] + 20;
                            if (un.tp == myLivingUnit.typeUnit.tower && un.selfUnit.GetDistanceTo(abX, abY) < 1000)
                                preFied[x, y] = preFied[x, y] + (int)(200 - un.selfUnit.GetDistanceTo(abX, abY) * 0.2);
                        }
                    }

                }

            for (int x = 0; x < lenField; x++)
                for (int y = 0; y < lenField; y++)
                {
                    fieldDamage[x, y] = Math.Min(preFied[x, y], 1500);
                }


            /*                                                for (int x = 0; x < lenField; x++)
                                                                for (int y = 0; y < lenField; y++)
                                                                {
                                                                    float col = fieldDamage[x, y] / 255f;
                                                                    double abX = self.X + (x - lenField / 2) * stepField;
                                                                    double abY = self.Y + (y - lenField / 2) * stepField;
                                                                           vc.FillCircle(abX , abY ,  stepField + 1, col, 0, 0);
                                                                }

              */
        }

        public Move GetFirebol(Wizard self, World world, Game game)
        {
            Move res = new Move();
            res.Action = ActionType.None;
            int stepF = 4;
            int lenFSetka = (int)(self.CastRange / stepF);
            int[,] fireSetka = new int[lenFSetka * 2, lenFSetka * 2];
            int maxDmg = 40;
            bool isWiz = false;
            for (int x = 0; x < lenFSetka * 2; x++)
                for (int y = 0; y < lenFSetka * 2; y++)
                {
                    fireSetka[x, y] = 0;
                    double fx = self.X + (x - lenFSetka / 2) * stepF;
                    double fy = self.Y + (y - lenFSetka / 2) * stepF;
                    if (Math.Abs(self.GetAngleTo(fx, fy)) < game.StaffSector * 0.3 && self.GetDistanceTo(fx, fy) > game.FireballExplosionMinDamageRange + self.Radius * 2 && self.GetDistanceTo(fx, fy) < self.CastRange)
                        foreach (myLivingUnit lu in livingUnits)
                        {
                            double koef = 1;
                            if (lu.selfUnit.Faction == self.Faction)
                                koef = -5;
                            if (lu.faction == Faction.Neutral)
                                koef = 0;
                            if (lu.tp == myLivingUnit.typeUnit.wizard)
                            {
                                koef = koef * 20;
                            }
                            if (lu.tp == myLivingUnit.typeUnit.tower)
                                koef = koef * 3;
                            if (lu.selfUnit.GetDistanceTo(fx, fy) <= lu.selfUnit.Radius + game.FireballExplosionMaxDamageRange)
                            {
                                fireSetka[x, y] = fireSetka[x, y] + (int)(game.FireballExplosionMaxDamage * koef);
                            }
                            if (lu.selfUnit.GetDistanceTo(fx, fy) < lu.selfUnit.Radius + game.FireballExplosionMinDamageRange && lu.selfUnit.GetDistanceTo(fx, fy) > lu.selfUnit.Radius + game.FireballExplosionMaxDamageRange)
                            {
                                double dam = game.FireballExplosionMaxDamage - game.FireballExplosionMinDamage;
                                fireSetka[x, y] = fireSetka[x, y] + (int)(dam * koef);
                            }
                        }
                    if (fireSetka[x, y] > maxDmg)
                    {
                        maxDmg = fireSetka[x, y];
                        res.MinCastDistance = self.GetDistanceTo(fx, fy) - game.FireballRadius;
                        res.MaxCastDistance = self.GetDistanceTo(fx, fy) + game.FireballRadius;
                        res.Action = ActionType.Fireball;
                        res.CastAngle = self.GetAngleTo(fx, fy);
                        res.Turn = self.GetAngleTo(fx, fy);
                        //vc.FillCircle(fx, fy, 20, 1, 1);
                    }

                }

            return res;
        }


    }
}