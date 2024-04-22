using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent.Drawing;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.WorldBuilding;

namespace Verdant.NPCs.Enemy.PestControl.Neuroma;

internal class JustMakeProjectileDoDamage
{
    private static void TryDoingOnHitEffects(Projectile proj, Entity entity)
    {
        switch (proj.type)
        {
            case 221:
            case 227:
            case 614:
            case 729:
            case 908:
            case 977:
                return;
        }

        Main.player[proj.owner].OnHit(entity.Center.X, entity.Center.Y, entity);
    }

    private static int CountEnemiesWhoAreImmuneToMeRightNow(Projectile proj, int cap)
    {
        int num = 0;
        for (int i = 0; i < proj.localNPCImmunity.Length; i++)
        {
            if (proj.localNPCImmunity[i] > 0)
            {
                num++;
                if (num >= cap)
                    break;
            }
        }
        return num;
    }

    private static void LightDisc_Bounce(Projectile proj, Vector2 hitPoint, Vector2 normal)
    {
        Vector2 spinningpoint = Vector2.Reflect(proj.velocity, normal);
        for (int i = 0; i < 4; i++)
        {
            Dust dust = Dust.NewDustPerfect(hitPoint, 306, spinningpoint.RotatedBy((float)Math.PI / 4f * Main.rand.NextFloatDirection()) * 0.6f * Main.rand.NextFloat(), 200, default, 1.6f);
            dust.color = Color.Lerp(new Color(219, 253, 0), Color.Cyan, Main.rand.NextFloat());
            Dust dust2 = Dust.CloneDust(dust);
            dust2.color = Color.White;
            dust2.scale = 1f;
            dust2.alpha = 50;
        }
    }

    public static void DamageNPC(Projectile proj, int npcWhoAmI)
    {
        bool flag = !proj.npcProj && !proj.trap;
        bool flag12 = proj.usesOwnerMeleeHitCD && flag && proj.owner < 255;
        int[] array = proj.localNPCImmunity;
        bool flag24 = (!proj.usesLocalNPCImmunity && !proj.usesIDStaticNPCImmunity) || (proj.usesLocalNPCImmunity && array[npcWhoAmI] == 0)
            || (proj.usesIDStaticNPCImmunity && Projectile.IsNPCIndexImmuneToProjectileType(proj.type, npcWhoAmI));
        float num = 1;

        if (flag12 && !Main.player[proj.owner].CanHitNPCWithMeleeHit(npcWhoAmI))
            flag24 = false;

        if (!((!Main.npc[npcWhoAmI].dontTakeDamage || NPCID.Sets.ZappingJellyfish[Main.npc[npcWhoAmI].type]) && flag24) || (Main.npc[npcWhoAmI].aiStyle == 112 && Main.npc[npcWhoAmI].ai[2] > 1f))
            return;

        bool canHitFlag = true;
        NPC obj = Main.npc[npcWhoAmI];
        obj.position += Main.npc[npcWhoAmI].netOffset;
        bool flag25 = !Main.npc[npcWhoAmI].friendly;
        flag25 |= proj.type == 318;
        flag25 |= Main.npc[npcWhoAmI].type == NPCID.Guide && proj.owner < 255 && Main.player[proj.owner].killGuide;
        flag25 |= Main.npc[npcWhoAmI].type == NPCID.Clothier && proj.owner < 255 && Main.player[proj.owner].killClothier;
        if (proj.owner < 255 && !Main.player[proj.owner].CanNPCBeHitByPlayerOrPlayerProjectile(Main.npc[npcWhoAmI], proj))
            flag25 = false;
        bool flag26 = Main.npc[npcWhoAmI].friendly && !Main.npc[npcWhoAmI].dontTakeDamageFromHostiles;
        if (canHitFlag || (proj.friendly && (flag25 || NPCID.Sets.ZappingJellyfish[Main.npc[npcWhoAmI].type])) || (proj.hostile && flag26))
        {
            bool flag27 = proj.maxPenetrate == 1 && !proj.usesLocalNPCImmunity && !proj.usesIDStaticNPCImmunity;
            if (canHitFlag)
                flag27 = true;
            if (proj.owner < 0 || Main.npc[npcWhoAmI].immune[proj.owner] == 0 || flag27)
            {
                bool flag28 = false;
                if (proj.type == 11 && (Main.npc[npcWhoAmI].type == NPCID.CorruptBunny || Main.npc[npcWhoAmI].type == NPCID.CorruptGoldfish))
                    flag28 = true;
                else if (proj.type == 31 && Main.npc[npcWhoAmI].type == NPCID.Antlion)
                {
                    flag28 = true;
                }
                else if (Main.npc[npcWhoAmI].trapImmune && proj.trap)
                {
                    flag28 = true;
                }
                else if (Main.npc[npcWhoAmI].immortal && proj.npcProj)
                {
                    flag28 = true;
                }
                if (canHitFlag)
                    flag28 = false;
                if (!flag28 && (Main.npc[npcWhoAmI].noTileCollide || !proj.ownerHitCheck || proj.CanHitWithMeleeWeapon(Main.npc[npcWhoAmI])))
                {
                    NPC nPC = Main.npc[npcWhoAmI];
                    if (NPCID.Sets.ZappingJellyfish[nPC.type])
                    {
                        if ((nPC.dontTakeDamage || !Main.player[proj.owner].CanNPCBeHitByPlayerOrPlayerProjectile(nPC, proj)) && (proj.aiStyle == 19 || proj.aiStyle == 161 ||
                            proj.aiStyle == 75 || proj.aiStyle == 140 || ProjectileID.Sets.IsAWhip[proj.type] || ProjectileID.Sets.AllowsContactDamageFromJellyfish[proj.type]))
                            Main.player[proj.owner].TakeDamageFromJellyfish(npcWhoAmI);

                        if (nPC.dontTakeDamage || !flag25)
                            return;
                    }
                    if (proj.type == 876)
                    {
                        Vector2 vector;

                        if (Main.rand.NextBool(20))
                        {
                            proj.tileCollide = false;
                            proj.position.X += Main.rand.Next(-256, 257);
                        }
                        if (Main.rand.NextBool(20))
                        {
                            proj.tileCollide = false;
                            proj.position.Y += Main.rand.Next(-256, 257);
                        }
                        if (Main.rand.NextBool(2))
                            proj.tileCollide = false;
                        if (!Main.rand.NextBool(3))
                        {
                            vector = proj.position;
                            proj.position -= proj.velocity * (float)Main.rand.Next(0, 40);
                            if (proj.tileCollide && Collision.SolidTiles(proj.position, proj.width, proj.height))
                            {
                                proj.position = vector;
                                proj.position -= proj.velocity * (float)Main.rand.Next(0, 40);
                                if (proj.tileCollide && Collision.SolidTiles(proj.position, proj.width, proj.height))
                                    proj.position = vector;
                            }
                        }
                        proj.velocity *= 0.6f;
                        if (Main.rand.NextBool(7))
                            proj.velocity.X += (float)Main.rand.Next(30, 31) * 0.01f;
                        if (Main.rand.NextBool(7))
                            proj.velocity.Y += (float)Main.rand.Next(30, 31) * 0.01f;
                        proj.damage = (int)((double)proj.damage * 0.9);
                        proj.knockBack *= 0.9f;
                        if (Main.rand.NextBool(20))
                            proj.knockBack *= 10f;
                        if (Main.rand.NextBool(50))
                            proj.damage *= 10;
                        if (Main.rand.NextBool(7))
                        {
                            vector = proj.position;
                            proj.position.X += Main.rand.Next(-64, 65);
                            if (proj.tileCollide && Collision.SolidTiles(proj.position, proj.width, proj.height))
                                proj.position = vector;
                        }
                        if (Main.rand.NextBool(7))
                        {
                            vector = proj.position;
                            proj.position.Y += Main.rand.Next(-64, 65);
                            if (proj.tileCollide && Collision.SolidTiles(proj.position, proj.width, proj.height))
                                proj.position = vector;
                        }
                        if (Main.rand.NextBool(14))
                            proj.velocity.X *= -1f;
                        if (Main.rand.NextBool(14))
                            proj.velocity.Y *= -1f;
                        if (Main.rand.NextBool(10))
                            proj.velocity *= (float)Main.rand.Next(1, 201) * 0.0005f;
                        if (proj.tileCollide)
                            proj.ai[1] = 0f;
                        else
                            proj.ai[1] = 1f;
                        proj.netUpdate = true;
                    }
                    bool flag3 = nPC.reflectsProjectiles;

                    if (Main.getGoodWorld && NPCID.Sets.ReflectStarShotsInForTheWorthy[Main.npc[npcWhoAmI].type] && (proj.type == 955 || proj.type == 728))
                        flag3 = true;

                    if (flag3 && proj.CanBeReflected() && nPC.CanReflectProjectile(proj))
                    {
                        nPC.ReflectProjectile(proj);
                        NPC obj2 = Main.npc[npcWhoAmI];
                        obj2.position -= Main.npc[npcWhoAmI].netOffset;
                        return;
                    }

                    if (proj.type == 604)
                        Main.player[proj.owner].Counterweight(nPC.Center, proj.damage, proj.knockBack);

                    NPC.HitModifiers modifiers = nPC.GetIncomingStrikeModifiers(proj.DamageType, proj.direction);
                    modifiers.ArmorPenetration += proj.ArmorPenetration;
                    CombinedHooks.ModifyHitNPCWithProj(proj, nPC, ref modifiers);
                    float num21 = proj.knockBack;
                    bool flag4 = false;
                    float armorPenetrationPercent = 0f;
                    bool flag5 = false;

                    switch (proj.type)
                    {
                        case 442:
                            flag5 = true;
                            break;
                        case 189:
                            if (flag && Main.player[proj.owner].strongBees)
                                modifiers.ArmorPenetration += 5f;
                            break;
                    }
                    if (flag5)
                    {
                        proj.Kill();
                        return;
                    }
                    modifiers.SourceDamage *= num;
                    float num43 = 1000f;
                    int num46 = 0;
                    if (proj.type > 0 && ProjectileID.Sets.StardustDragon[proj.type])
                    {
                        float value = (proj.scale - 1f) * 100f;
                        value = Utils.Clamp(value, 0f, 50f);
                        num43 = (int)(num43 * (1f + value * 0.23f));
                    }
                    if (proj.type > 0 && proj.type < ProjectileID.Count && ProjectileID.Sets.StormTiger[proj.type])
                    {
                        int num47 = Math.Max(0, Main.player[proj.owner].ownedProjectileCounts[831] - 1);
                        num43 = (int)(num43 * (1f + (float)num47 * 0.4f));
                    }
                    if (proj.type == 818)
                    {
                        int num48 = Math.Max(0, Main.player[proj.owner].ownedProjectileCounts[831] - 1);
                        num43 = (int)(num43 * (1.5f + (float)num48 * 0.4f));
                    }
                    if (proj.type == 963)
                    {
                        int num49 = Math.Max(0, Main.player[proj.owner].ownedProjectileCounts[970] - 1);
                        int num2 = 3 + num49 / 2;
                        if (CountEnemiesWhoAreImmuneToMeRightNow(proj, num2) >= num2)
                            return;
                        float num3 = 0.55f;
                        if (Main.hardMode)
                            num3 = 1.3f;
                        num43 = (int)(num43 * (1f + (float)num49 * num3));
                    }
                    if (flag && proj.type == 189 && Main.player[proj.owner].strongBees)
                        modifiers.SourceDamage += 5f;
                    if (flag)
                    {
                        if (proj.DamageType.UseStandardCritCalcs && Main.rand.Next(100) < proj.CritChance)
                            flag4 = true;
                        if ((uint)(proj.type - 688) <= 2u)
                        {
                            if (Main.player[proj.owner].setMonkT3)
                            {
                                if (Main.rand.NextBool(4))
                                    flag4 = true;
                            }
                            else if (Main.player[proj.owner].setMonkT2 && Main.rand.NextBool(6))
                            {
                                flag4 = true;
                            }
                        }
                    }
                    modifiers.SourceDamage *= num43 / 1000f;
                    num43 = modifiers.SourceDamage.ApplyTo(proj.damage);
                    float num4 = ProjectileID.Sets.SummonTagDamageMultiplier[proj.type];
                    ParticleOrchestraSettings settings2;
                    if (flag && (proj.minion || ProjectileID.Sets.MinionShot[proj.type] || proj.sentry || ProjectileID.Sets.SentryShot[proj.type]))
                    {
                        bool flag6 = false;
                        bool flag7 = false;
                        bool flag8 = false;
                        bool flag9 = false;
                        bool flag10 = false;
                        bool flag11 = false;
                        bool flag13 = false;
                        bool flag14 = false;
                        bool flag15 = false;
                        for (int j = 0; j < NPC.maxBuffs; j++)
                        {
                            if (nPC.buffTime[j] >= 1)
                            {
                                switch (nPC.buffType[j])
                                {
                                    case 307:
                                        flag6 = true;
                                        break;
                                    case 309:
                                        flag7 = true;
                                        break;
                                    case 313:
                                        flag8 = true;
                                        break;
                                    case 310:
                                        flag9 = true;
                                        break;
                                    case 315:
                                        flag10 = true;
                                        break;
                                    case 326:
                                        flag11 = true;
                                        break;
                                    case 319:
                                        flag14 = true;
                                        break;
                                    case 316:
                                        flag15 = true;
                                        break;
                                    case 340:
                                        flag13 = true;
                                        break;
                                }
                            }
                        }
                        if (flag6)
                            num46 += 4;
                        if (flag10)
                            num46 += 6;
                        if (flag11)
                            num46 += 7;
                        if (flag13)
                            num46 += 6;
                        if (flag7)
                            num46 += 9;
                        if (flag14)
                        {
                            num46 += 8;
                            if (Main.rand.Next(100) < 12)
                                flag4 = true;
                        }
                        if (flag9)
                        {
                            int num5 = 10;
                            num46 += num5;
                            int num6 = Projectile.NewProjectile(proj.GetSource_FromThis(), nPC.Center, Vector2.Zero, 916, (int)((float)num5 * num4), 0f, proj.owner);
                            Main.projectile[num6].localNPCImmunity[npcWhoAmI] = -1;
                            Projectile.EmitBlackLightningParticles(nPC);
                        }
                        if (flag15)
                        {
                            int num7 = 20;
                            num46 += num7;
                            if (Main.rand.NextBool(10))
                                flag4 = true;
                            settings2 = new ParticleOrchestraSettings
                            {
                                PositionInWorld = proj.Center
                            };
                            ParticleOrchestrator.RequestParticleSpawn(clientOnly: false, ParticleOrchestraType.RainbowRodHit, settings2);
                        }
                        if (flag8)
                        {
                            nPC.RequestBuffRemoval(313);
                            int num8 = (int)(num43 * 1.75f);
                            int num9 = Projectile.NewProjectile(proj.GetSource_FromThis(), nPC.Center, Vector2.Zero, 918, num8, 0f, proj.owner);
                            Main.projectile[num9].localNPCImmunity[npcWhoAmI] = -1;
                            modifiers.ScalingBonusDamage += 1.75f * num4;
                        }
                    }
                    num46 = (int)((float)num46 * num4);
                    modifiers.FlatBonusDamage += (float)num46;
                    if (flag)
                        _ = Main.player[proj.owner].luck;
                    float num10 = 1000f;
                    if (proj.type == 1002)
                        num10 /= 2f;
                    if (proj.trap && NPCID.Sets.BelongsToInvasionOldOnesArmy[nPC.type])
                        num10 /= 2f;
                    if (proj.type == 482 && (nPC.aiStyle == 6 || nPC.aiStyle == 37))
                        num10 /= 2f;
                    if (flag)
                    {
                        Vector2 positionInWorld = Main.rand.NextVector2FromRectangle(nPC.Hitbox);
                        ParticleOrchestraSettings particleOrchestraSettings = default;
                        particleOrchestraSettings.PositionInWorld = positionInWorld;
                        ParticleOrchestraSettings settings = particleOrchestraSettings;

                        switch (proj.type)
                        {
                            case 972:
                                ParticleOrchestrator.RequestParticleSpawn(clientOnly: false, ParticleOrchestraType.NightsEdge, settings, proj.owner);
                                break;
                            case 973:
                                ParticleOrchestrator.RequestParticleSpawn(clientOnly: false, ParticleOrchestraType.TrueNightsEdge, settings, proj.owner);
                                break;
                            case 984:
                            case 985:
                                settings.MovementVector = proj.velocity;
                                ParticleOrchestrator.RequestParticleSpawn(clientOnly: false, ParticleOrchestraType.TerraBlade, settings, proj.owner);
                                break;
                            case 982:
                                ParticleOrchestrator.RequestParticleSpawn(clientOnly: false, ParticleOrchestraType.Excalibur, settings, proj.owner);
                                break;
                            case 983:
                                ParticleOrchestrator.RequestParticleSpawn(clientOnly: false, ParticleOrchestraType.TrueExcalibur, settings, proj.owner);
                                break;
                        }
                    }

                    if (proj.type == 604)
                    {
                        proj.friendly = false;
                        proj.ai[1] = 1000f;
                    }
                    if ((proj.type == 400 || proj.type == 401 || proj.type == 402) && nPC.type >= NPCID.EaterofWorldsHead && nPC.type <= NPCID.EaterofWorldsTail)
                    {
                        num10 = (int)((double)num10 * 0.65);
                        if (proj.penetrate > 1)
                            proj.penetrate--;
                    }
                    if (proj.type == 710 && !WorldUtils.Find(proj.Center.ToTileCoordinates(), Searches.Chain(new Searches.Down(12), new Conditions.NotNull(), new Conditions.IsSolid()), out var _))
                        num10 = (int)(num10 * 1.5f);
                    if (proj.type == 504 || proj.type == 954 || proj.type == 979)
                    {
                        float num12 = (60f - proj.ai[0]) / 2f;
                        proj.ai[0] += num12;
                    }
                    if (proj.aiStyle == 3 && proj.type != 301 && proj.type != 866 && proj.type != 902)
                    {
                        if (proj.ai[0] == 0f)
                        {
                            if (proj.type == 106)
                                LightDisc_Bounce(proj, proj.Center + proj.velocity.SafeNormalize(Vector2.UnitX) * 8f, (-proj.velocity).SafeNormalize(Vector2.UnitX));
                            proj.velocity.X = 0f - proj.velocity.X;
                            proj.velocity.Y = 0f - proj.velocity.Y;
                            proj.netUpdate = true;
                        }
                        proj.ai[0] = 1f;
                    }
                    else if (proj.type == 951)
                    {
                        Vector2 vector2 = (nPC.Center - proj.Center).SafeNormalize(Vector2.Zero);
                        vector2.X += (-0.5f + Main.rand.NextFloat()) * 13f;
                        vector2.Y = -5f;
                        proj.velocity.X = vector2.X;
                        proj.velocity.Y = vector2.Y;
                        proj.netUpdate = true;
                    }
                    else if (proj.type == 582 || proj.type == 902)
                    {
                        if (proj.ai[0] != 0f)
                            proj.direction *= -1;
                    }
                    else if (proj.type == 612 || proj.type == 953 || proj.type == 978)
                    {
                        proj.direction = Main.player[proj.owner].direction;
                    }
                    else if (proj.type == 624)
                    {
                        float num13 = 1f;
                        if (nPC.knockBackResist > 0f)
                            num13 = 1f / nPC.knockBackResist;
                        proj.knockBack = 4f * num13;
                        num21 = proj.knockBack;
                        if (nPC.Center.X < proj.Center.X)
                            proj.direction = 1;
                        else
                            proj.direction = -1;
                    }
                    else if (proj.aiStyle == 16)
                    {
                        if (proj.timeLeft > 3)
                            proj.timeLeft = 3;
                        if (nPC.position.X + (float)(nPC.width / 2) < proj.position.X + (float)(proj.width / 2))
                            proj.direction = -1;
                        else
                            proj.direction = 1;
                    }
                    else if (proj.aiStyle == 68)
                    {
                        if (proj.timeLeft > 3)
                            proj.timeLeft = 3;
                        if (nPC.position.X + (float)(nPC.width / 2) < proj.position.X + (float)(proj.width / 2))
                            proj.direction = -1;
                        else
                            proj.direction = 1;
                    }
                    else if (proj.aiStyle == 50)
                    {
                        if (nPC.position.X + (float)(nPC.width / 2) < proj.position.X + (float)(proj.width / 2))
                            proj.direction = -1;
                        else
                            proj.direction = 1;
                    }
                    else if (proj.type == 908)
                    {
                        if (nPC.position.X + (float)(nPC.width / 2) < proj.position.X + (float)(proj.width / 2))
                            proj.direction = -1;
                        else
                            proj.direction = 1;
                    }
                    if (proj.type == 509)
                    {
                        int num14 = Main.rand.Next(2, 6);
                        for (int k = 0; k < num14; k++)
                        {
                            Vector2 vector3 = new((float)Main.rand.Next(-100, 101), (float)Main.rand.Next(-100, 101));
                            vector3 += proj.velocity * 3f;
                            vector3.Normalize();
                            vector3 *= (float)Main.rand.Next(35, 81) * 0.1f;
                            int num15 = (int)((double)proj.damage * 0.5);
                            Projectile.NewProjectile(proj.GetSource_FromThis(), proj.Center.X, proj.Center.Y, vector3.X, vector3.Y, 504, num15, proj.knockBack * 0.2f, proj.owner);
                        }
                    }
                    if ((proj.type == 476 || proj.type == 950) && !proj.npcProj)
                    {
                        float x = Main.player[proj.owner].Center.X;
                        if (nPC.Center.X < x)
                            proj.direction = -1;
                        else
                            proj.direction = 1;
                    }
                    if (proj.type == 598 || proj.type == 636 || proj.type == 614 || proj.type == 971 || proj.type == 975)
                    {
                        proj.ai[0] = 1f;
                        proj.ai[1] = npcWhoAmI;
                        proj.velocity = (nPC.Center - proj.Center) * 0.75f;
                        proj.netUpdate = true;
                    }
                    if (proj.type >= 511 && proj.type <= 513)
                    {
                        proj.ai[1] += 1f;
                        proj.netUpdate = true;
                    }
                    if (proj.type == 659)
                        proj.timeLeft = 0;
                    if (proj.type == 524)
                    {
                        proj.netUpdate = true;
                        proj.ai[0] += 50f;
                    }
                    if ((proj.type == 688 || proj.type == 689 || proj.type == 690) && nPC.type != NPCID.DungeonGuardian && nPC.defense < 999)
                        armorPenetrationPercent = 1f;
                    if (proj.aiStyle == 39)
                    {
                        if (proj.ai[1] == 0f)
                        {
                            proj.ai[1] = npcWhoAmI + 1;
                            proj.netUpdate = true;
                        }
                        if (Main.player[proj.owner].position.X + (float)(Main.player[proj.owner].width / 2) < proj.position.X + (float)(proj.width / 2))
                            proj.direction = 1;
                        else
                            proj.direction = -1;
                    }
                    if (proj.type == 41 && proj.timeLeft > 1)
                        proj.timeLeft = 1;
                    if (proj.aiStyle == 99)
                    {
                        Main.player[proj.owner].Counterweight(nPC.Center, proj.damage, proj.knockBack);
                        if (nPC.Center.X < Main.player[proj.owner].Center.X)
                            proj.direction = -1;
                        else
                            proj.direction = 1;
                        if (proj.ai[0] >= 0f)
                        {
                            Vector2 vector4 = proj.Center - nPC.Center;
                            vector4.Normalize();
                            float num16 = 16f;
                            proj.velocity *= -0.5f;
                            proj.velocity += vector4 * num16;
                            proj.netUpdate = true;
                            proj.localAI[0] += 20f;
                            if (!Collision.CanHit(proj.position, proj.width, proj.height, Main.player[proj.owner].position, Main.player[proj.owner].width, Main.player[proj.owner].height))
                            {
                                proj.localAI[0] += 40f;
                                num10 = (int)((double)num10 * 0.75);
                            }
                        }
                    }
                    if (proj.type == 856 && !Collision.CanHit(proj.position, proj.width, proj.height, Main.player[proj.owner].position, Main.player[proj.owner].width, Main.player[proj.owner].height))
                        num10 = (int)((double)num10 * 0.75);
                    if (proj.aiStyle == 93)
                    {
                        if (proj.ai[0] == 0f)
                        {
                            proj.ai[1] = 0f;
                            int num17 = -npcWhoAmI - 1;
                            proj.ai[0] = num17;
                            proj.velocity = nPC.Center - proj.Center;
                        }
                        num10 = ((proj.ai[0] != 2f) ? ((int)((double)num10 * 0.15)) : ((int)((double)num10 * 1.35)));
                    }
                    if (flag)
                    {
                        int num18 = Item.NPCtoBanner(nPC.BannerID());
                        if (num18 >= 0)
                            Main.player[Main.myPlayer].lastCreatureHit = num18;
                    }
                    if (Main.netMode != NetmodeID.Server && flag)
                        Main.player[proj.owner].ApplyBannerOffenseBuff(nPC, ref modifiers);
                    if (Main.expertMode)
                    {
                        if ((proj.type == 30 || proj.type == 397 || proj.type == 517 || proj.type == 28 || proj.type == 37 || proj.type == 516 || proj.type == 29 || proj.type == 470 || proj.type == 637 || proj.type == 108 ||
                            proj.type == 281 || proj.type == 588 || proj.type == 519 || proj.type == 773 || proj.type == 183 || proj.type == 181 || proj.type == 566 || proj.type == 1002)
                            && nPC.type >= NPCID.EaterofWorldsHead && nPC.type <= NPCID.EaterofWorldsTail)
                            num10 /= 5f;
                        if (proj.type == 280 && ((nPC.type >= NPCID.TheDestroyer && nPC.type <= NPCID.TheDestroyerTail) || nPC.type == NPCID.Probe))
                            num10 = (int)((double)num10 * 0.75);
                    }
                    if (Main.netMode != NetmodeID.Server && nPC.type == NPCID.CultistBoss && proj.type >= 0 && ProjectileID.Sets.CultistIsResistantTo[proj.type])
                        num10 = (int)(num10 * 0.75f);
                    if (proj.type == 497 && proj.penetrate != 1)
                    {
                        proj.ai[0] = 25f;
                        float num19 = proj.velocity.Length();
                        Vector2 vector5 = nPC.Center - proj.Center;
                        vector5.Normalize();
                        vector5 *= num19;
                        proj.velocity = -vector5 * 0.9f;
                        proj.netUpdate = true;
                    }
                    if (proj.type == 323 && (nPC.type == NPCID.Vampire || nPC.type == NPCID.VampireBat))
                        num10 *= 10f;
                    if (proj.type == 981 && nPC.type == NPCID.Werewolf)
                        num10 *= 3f;
                    if (proj.type == 261 && proj.velocity.Length() < 3.5f)
                    {
                        modifiers.SourceDamage /= 2f;
                        num21 /= 2f;
                    }
                    if (flag && proj.CountsAsClass(DamageClass.Melee) && Main.player[proj.owner].parryDamageBuff && !ProjectileID.Sets.DontApplyParryDamageBuff[proj.type])
                    {
                        modifiers.ScalingBonusDamage += 4f;
                        Main.player[proj.owner].parryDamageBuff = false;
                        Main.player[proj.owner].ClearBuff(198);
                    }
                    int? num26 = null;
                    switch (proj.type)
                    {
                        case 697:
                        case 699:
                        case 707:
                        case 708:
                        case 759:
                            num26 = ((Main.player[proj.owner].Center.X < nPC.Center.X) ? 1 : (-1));
                            break;
                    }
                    if (proj.aiStyle == 188 || proj.aiStyle == 189 || proj.aiStyle == 190 || proj.aiStyle == 191)
                        num26 = ((Main.player[proj.owner].Center.X < nPC.Center.X) ? 1 : (-1));
                    if (proj.aiStyle == 15)
                    {
                        num26 = ((Main.player[proj.owner].Center.X < nPC.Center.X) ? 1 : (-1));
                        if (proj.ai[0] == 0f)
                            num21 *= 0.35f;
                        if (proj.ai[0] == 6f)
                            num21 *= 0.5f;
                    }
                    modifiers.ScalingArmorPenetration += armorPenetrationPercent;
                    modifiers.Knockback *= num21 / proj.knockBack;
                    modifiers.TargetDamageMultiplier *= num10 / 1000f;
                    if (num26.HasValue)
                        modifiers.HitDirectionOverride = num26;
                    NPC.HitInfo strike = modifiers.ToHitInfo(proj.damage, flag4, num21, damageVariation: true, flag ? Main.player[proj.owner].luck : 0f);
                    num26 = strike.HitDirection;
                    if (proj.type == 294)
                        proj.damage = (int)((double)proj.damage * 0.9);
                    if (proj.type == 265)
                        proj.damage = (int)((double)proj.damage * 0.75);
                    if (proj.type == 355)
                        proj.damage = (int)((double)proj.damage * 0.75);
                    if (proj.type == 114)
                        proj.damage = (int)((double)proj.damage * 0.9);
                    if (proj.type == 76 || proj.type == 78 || proj.type == 77)
                        proj.damage = (int)((double)proj.damage * 0.95);
                    if (proj.type == 85)
                        proj.damage = (int)((double)proj.damage * 0.85);
                    if (proj.type == 866)
                        proj.damage = (int)((double)proj.damage * 0.8);
                    if (proj.type == 841)
                        proj.damage = (int)((double)proj.damage * 0.5);
                    if (proj.type == 914)
                        proj.damage = (int)((double)proj.damage * 0.6);
                    if (proj.type == 952)
                        proj.damage = (int)((double)proj.damage * 0.9);
                    if (proj.type == 913)
                        proj.damage = (int)((double)proj.damage * 0.66);
                    if (proj.type == 912)
                        proj.damage = (int)((double)proj.damage * 0.7);
                    if (proj.type == 847)
                        proj.damage = (int)((double)proj.damage * 0.8);
                    if (proj.type == 848)
                        proj.damage = (int)((double)proj.damage * 0.95);
                    if (proj.type == 849)
                        proj.damage = (int)((double)proj.damage * 0.9);
                    if (proj.type == 915)
                        proj.damage = (int)((double)proj.damage * 0.9);
                    if (proj.type == 931)
                        proj.damage = (int)((double)proj.damage * 0.8);
                    if (proj.type == 242)
                        proj.damage = (int)((double)proj.damage * 0.85);
                    if (proj.type == 323)
                        proj.damage = (int)((double)proj.damage * 0.9);
                    if (proj.type == 5)
                        proj.damage = (int)((double)proj.damage * 0.9);
                    if (proj.type == 4)
                        proj.damage = (int)((double)proj.damage * 0.95);
                    if (proj.type == 309)
                        proj.damage = (int)((double)proj.damage * 0.85);
                    if (proj.type == 132)
                        proj.damage = (int)((double)proj.damage * 0.85);
                    if (proj.type == 985)
                        proj.damage = (int)((double)proj.damage * 0.75);
                    if (proj.type == 950)
                        proj.damage = (int)((double)proj.damage * 0.98);
                    if (proj.type == 964)
                        proj.damage = (int)((double)proj.damage * 0.85);
                    if (proj.type == 477 && proj.penetrate > 1)
                    {
                        int[] array2 = new int[10];
                        int num20 = 0;
                        int num22 = 700;
                        int num23 = 20;
                        for (int l = 0; l < 200; l++)
                        {
                            if (l == npcWhoAmI || !Main.npc[l].CanBeChasedBy(proj))
                                continue;
                            Vector2 val = proj.Center - Main.npc[l].Center;
                            float num24 = val.Length();
                            if (num24 > (float)num23 && num24 < (float)num22 && Collision.CanHitLine(proj.Center, 1, 1, Main.npc[l].Center, 1, 1))
                            {
                                array2[num20] = l;
                                num20++;
                                if (num20 >= 9)
                                    break;
                            }
                        }
                        if (num20 > 0)
                        {
                            num20 = Main.rand.Next(num20);
                            Vector2 vector6 = Main.npc[array2[num20]].Center - proj.Center;
                            float num25 = proj.velocity.Length();
                            vector6.Normalize();
                            proj.velocity = vector6 * num25;
                            proj.netUpdate = true;
                        }
                    }
                    proj.StatusNPC(npcWhoAmI);
                    if (flag && nPC.life > 5)
                        TryDoingOnHitEffects(proj, nPC);
                    if (ProjectileID.Sets.ImmediatelyUpdatesNPCBuffFlags[proj.type])
                        nPC.UpdateNPC_BuffSetFlags(lowerBuffTime: false);
                    if (proj.type == 317)
                    {
                        proj.ai[1] = -1f;
                        proj.netUpdate = true;
                    }
                    NPCKillAttempt attempt = new(nPC);
                    int num27 = nPC.StrikeNPC(strike, fromNet: false, !flag);
                    if (flag && attempt.DidNPCDie())
                        Main.player[proj.owner].OnKillNPC(ref attempt, proj);
                    if (flag && Main.player[proj.owner].accDreamCatcher && !nPC.HideStrikeDamage)
                        Main.player[proj.owner].addDPS(num27);
                    bool flag16 = !nPC.immortal;
                    bool flag17 = num27 > 0 && nPC.lifeMax > 5 && proj.friendly && !proj.hostile && proj.aiStyle != 59;
                    bool flag18 = false;
                    if (flag16 && proj.active && proj.timeLeft > 10 && nPC.active && nPC.type == 676 && proj.CanBeReflected())
                    {
                        nPC.ReflectProjectile(proj);
                        proj.penetrate++;
                    }
                    if (flag && flag16)
                    {
                        if (proj.type == 997 && (!nPC.immortal || flag18) && !nPC.SpawnedFromStatue && !NPCID.Sets.CountsAsCritter[nPC.type])
                            Main.player[proj.owner].HorsemansBlade_SpawnPumpkin(npcWhoAmI, (int)((float)proj.damage * 1f), proj.knockBack);
                        if (proj.type == 756 && proj.penetrate == 1)
                        {
                            proj.damage = 0;
                            proj.penetrate = -1;
                        }
                        if ((flag18 || nPC.value > 0f) && Main.player[proj.owner].hasLuckyCoin && Main.rand.NextBool(5))
                        {
                            int num28 = 71;
                            if (Main.rand.NextBool(10))
                                num28 = 72;
                            if (Main.rand.NextBool(100))
                                num28 = 73;
                            int num29 = Item.NewItem(proj.GetSource_OnHit(nPC), (int)nPC.position.X, (int)nPC.position.Y, nPC.width, nPC.height, num28);
                            Main.item[num29].stack = Main.rand.Next(1, 11);
                            Main.item[num29].velocity.Y = (float)Main.rand.Next(-20, 1) * 0.2f;
                            Main.item[num29].velocity.X = (float)Main.rand.Next(10, 31) * 0.2f * (float)num26.Value;
                            Main.item[num29].timeLeftInWhichTheItemCannotBeTakenByEnemies = 60;
                            if (Main.netMode == NetmodeID.MultiplayerClient)
                                NetMessage.SendData(148, -1, -1, null, num29);
                        }
                        if (proj.type == 999 && proj.owner == Main.myPlayer && Main.rand.NextBool(3))
                        {
                            Player player = Main.player[proj.owner];
                            Vector2 vector7 = (proj.Center - nPC.Center).SafeNormalize(Vector2.Zero) * 0.25f;
                            int dmg = proj.damage / 2;
                            float kB = proj.knockBack;
                            int num30 = Projectile.NewProjectile(proj.GetSource_FromThis(), proj.Center.X, proj.Center.Y, vector7.X, vector7.Y, player.beeType(), player.beeDamage(dmg), player.beeKB(kB), proj.owner);
                            Main.projectile[num30].DamageType = DamageClass.Melee;
                        }
                        if (flag17)
                        {
                            if (proj.type == 304 && !Main.player[proj.owner].moonLeech)
                                proj.vampireHeal(num27, new Vector2(nPC.Center.X, nPC.Center.Y), nPC);
                            if (nPC.canGhostHeal || flag18)
                            {
                                if (Main.player[proj.owner].ghostHeal && !Main.player[proj.owner].moonLeech)
                                    proj.ghostHeal(num27, new Vector2(nPC.Center.X, nPC.Center.Y), nPC);
                                if (Main.player[proj.owner].ghostHurt)
                                    proj.ghostHurt(num27, new Vector2(nPC.Center.X, nPC.Center.Y), nPC);
                                if (proj.CountsAsClass(DamageClass.Magic) && Main.player[proj.owner].setNebula && Main.player[proj.owner].nebulaCD == 0 && Main.rand.NextBool(3))
                                {
                                    Main.player[proj.owner].nebulaCD = 30;
                                    int num31 = Utils.SelectRandom<int>(Main.rand, 3453, 3454, 3455);
                                    int num33 = Item.NewItem(proj.GetSource_OnHit(nPC), (int)nPC.position.X, (int)nPC.position.Y, nPC.width, nPC.height, num31);
                                    Main.item[num33].velocity.Y = (float)Main.rand.Next(-20, 1) * 0.2f;
                                    Main.item[num33].velocity.X = (float)Main.rand.Next(10, 31) * 0.2f * (float)num26.Value;
                                    if (Main.netMode == NetmodeID.MultiplayerClient)
                                        NetMessage.SendData(MessageID.SyncItem, -1, -1, null, num33);
                                }
                            }
                            if (proj.CountsAsClass(DamageClass.Melee) && Main.player[proj.owner].beetleOffense && (!nPC.immortal || flag18))
                            {
                                if (Main.player[proj.owner].beetleOrbs == 0)
                                    Main.player[proj.owner].beetleCounter += num27 * 3;
                                else if (Main.player[proj.owner].beetleOrbs == 1)
                                {
                                    Main.player[proj.owner].beetleCounter += num27 * 2;
                                }
                                else
                                {
                                    Main.player[proj.owner].beetleCounter += num27;
                                }
                                Main.player[proj.owner].beetleCountdown = 0;
                            }
                            if (proj.arrow && proj.type != 631 && Main.player[proj.owner].phantasmTime > 0)
                            {
                                Vector2 source = Main.player[proj.owner].position + Main.player[proj.owner].Size * Utils.RandomVector2(Main.rand, 0f, 1f);
                                Vector2 vector8 = nPC.DirectionFrom(source) * 6f;
                                int num34 = (int)((float)proj.damage * 0.3f);
                                Projectile.NewProjectile(proj.GetSource_FromThis(), source.X, source.Y, vector8.X, vector8.Y, 631, num34, 0f, proj.owner, npcWhoAmI);
                                Projectile.NewProjectile(proj.GetSource_FromThis(), source.X, source.Y, vector8.X, vector8.Y, 631, num34, 0f, proj.owner, npcWhoAmI, 15f);
                                Projectile.NewProjectile(proj.GetSource_FromThis(), source.X, source.Y, vector8.X, vector8.Y, 631, num34, 0f, proj.owner, npcWhoAmI, 30f);
                            }
                            Player player2 = Main.player[proj.owner];
                            switch (proj.type)
                            {
                                case 914:
                                    player2.AddBuff(314, 180);
                                    break;
                                case 847:
                                    player2.AddBuff(308, 180);
                                    break;
                                case 849:
                                    player2.AddBuff(311, 180);
                                    break;
                                case 912:
                                    {
                                        int num35 = 15;
                                        if (!player2.coolWhipBuff)
                                        {
                                            Projectile.NewProjectile(proj.GetSource_FromThis(), nPC.Center, Vector2.Zero, 917, num35, 0f, proj.owner);
                                            player2.coolWhipBuff = true;
                                        }
                                        player2.AddBuff(312, 180);
                                        break;
                                    }
                            }
                        }
                    }
                    if (flag && (proj.CountsAsClass(DamageClass.Melee) || ProjectileID.Sets.IsAWhip[proj.type]) && Main.player[proj.owner].meleeEnchant == 7)
                        Projectile.NewProjectile(proj.GetSource_FromThis(), nPC.Center.X, nPC.Center.Y, nPC.velocity.X, nPC.velocity.Y, 289, 0, 0f, proj.owner);
                    if (flag && proj.type == 913)
                        proj.localAI[0] = 1f;
                    if (Main.netMode != NetmodeID.SinglePlayer)
                        NetMessage.SendStrikeNPC(nPC, in strike);
                    if (proj.type == 916)
                        Projectile.EmitBlackLightningParticles(nPC);
                    if (proj.type >= 390 && proj.type <= 392)
                        proj.localAI[1] = 20f;
                    if (proj.usesIDStaticNPCImmunity)
                    {
                        if (proj.penetrate != 1 || proj.appliesImmunityTimeOnSingleHits)
                        {
                            nPC.immune[proj.owner] = 0;
                            Projectile.perIDStaticNPCImmunity[proj.type][npcWhoAmI] = Main.GameUpdateCount + (uint)proj.idStaticNPCHitCooldown;
                        }
                    }
                    else if (proj.type == 434)
                    {
                        proj.numUpdates = 0;
                    }
                    else if (proj.type == 598 || proj.type == 636 || proj.type == 614)
                    {
                        Point[] bufferForScan = new Point[6];
                        if (proj.type == 636)
                            bufferForScan = new Point[8];
                        if (proj.type == 614)
                            bufferForScan = new Point[10];
                        Projectile.KillOldestJavelin(proj.whoAmI, proj.type, npcWhoAmI, bufferForScan);
                    }
                    else if (proj.type == 632)
                    {
                        nPC.immune[proj.owner] = 5;
                    }
                    else if (proj.type == 514)
                    {
                        nPC.immune[proj.owner] = 1;
                    }
                    else if (proj.type == 611)
                    {
                        if (proj.localAI[1] <= 0f)
                            Projectile.NewProjectile(proj.GetSource_FromThis(), nPC.Center.X, nPC.Center.Y, 0f, 0f, 612, proj.damage, 10f, proj.owner, 0f, 0.85f + Main.rand.NextFloat() * 1.15f);
                        proj.localAI[1] = 4f;
                    }
                    else if (proj.type == 595 || proj.type == 735)
                    {
                        nPC.immune[proj.owner] = 5;
                    }
                    else if (proj.type == 927)
                    {
                        nPC.immune[proj.owner] = 4;
                    }
                    else if (proj.type == 286)
                    {
                        nPC.immune[proj.owner] = 5;
                    }
                    else if (proj.type == 443)
                    {
                        nPC.immune[proj.owner] = 8;
                    }
                    else if (proj.type >= 424 && proj.type <= 426)
                    {
                        nPC.immune[proj.owner] = 5;
                    }
                    else if (proj.type == 634 || proj.type == 635)
                    {
                        nPC.immune[proj.owner] = 5;
                    }
                    else if (proj.type == 659)
                    {
                        nPC.immune[proj.owner] = 5;
                    }
                    else if (proj.type == 246)
                    {
                        nPC.immune[proj.owner] = 7;
                    }
                    else if (proj.type == 249)
                    {
                        nPC.immune[proj.owner] = 7;
                    }
                    else if (proj.type == 16)
                    {
                        nPC.immune[proj.owner] = 8;
                    }
                    else if (proj.type == 409)
                    {
                        nPC.immune[proj.owner] = 6;
                    }
                    else if (proj.type == 311)
                    {
                        nPC.immune[proj.owner] = 7;
                    }
                    else if (proj.type == 582 || proj.type == 902)
                    {
                        nPC.immune[proj.owner] = 7;
                        if (proj.ai[0] != 1f)
                        {
                            proj.ai[0] = 1f;
                            proj.netUpdate = true;
                        }
                    }
                    else
                    {
                        if (proj.type == 451)
                        {
                            if (proj.ai[0] == 0f)
                                proj.ai[0] += proj.penetrate;
                            else
                                proj.ai[0] -= proj.penetrate + 1;
                            proj.ai[1] = 0f;
                            proj.netUpdate = true;
                            NPC obj3 = Main.npc[npcWhoAmI];
                            obj3.position -= Main.npc[npcWhoAmI].netOffset;
                            return;
                        }
                        if (proj.type == 864)
                        {
                            array[npcWhoAmI] = 10;
                            nPC.immune[proj.owner] = 0;
                            if (proj.ai[0] > 0f)
                            {
                                proj.ai[0] = -1f;
                                proj.ai[1] = 0f;
                                proj.netUpdate = true;
                            }
                        }
                        else if (proj.type == 661 || proj.type == 856)
                        {
                            array[npcWhoAmI] = 8;
                            nPC.immune[proj.owner] = 0;
                        }
                        else if (proj.type == 866)
                        {
                            array[npcWhoAmI] = -1;
                            nPC.immune[proj.owner] = 0;
                            proj.penetrate--;
                            if (proj.penetrate == 0)
                            {
                                proj.penetrate = 1;
                                proj.damage = 0;
                                proj.ai[1] = -1f;
                                proj.netUpdate = true;
                                NPC obj4 = Main.npc[npcWhoAmI];
                                obj4.position -= Main.npc[npcWhoAmI].netOffset;
                                return;
                            }
                            if (proj.owner == Main.myPlayer)
                            {
                                int num36 = proj.FindTargetWithLineOfSight();
                                float num37 = proj.ai[1];
                                proj.ai[1] = num36;
                                if (proj.ai[1] != num37)
                                    proj.netUpdate = true;
                                if (num36 != -1)
                                    proj.velocity = proj.velocity.Length() * proj.DirectionTo(Main.npc[num36].Center);
                            }
                        }
                        else if (proj.usesLocalNPCImmunity && proj.localNPCHitCooldown != -2)
                        {
                            nPC.immune[proj.owner] = 0;
                            array[npcWhoAmI] = proj.localNPCHitCooldown;
                        }
                        else if (proj.penetrate != 1 || proj.appliesImmunityTimeOnSingleHits)
                        {
                            nPC.immune[proj.owner] = 10;
                        }
                    }
                    //if (proj.type == 710) // TODO?
                    //    BetsySharpnel(npcWhoAmI);
                    CombinedHooks.OnHitNPCWithProj(proj, nPC, in strike, num27);
                    if (proj.penetrate > 0 && proj.type != 317 && proj.type != 866)
                    {
                        if (proj.type == 357)
                            proj.damage = (int)((double)proj.damage * 0.8);
                        proj.penetrate--;
                        if (proj.penetrate == 0)
                        {
                            NPC obj5 = Main.npc[npcWhoAmI];
                            obj5.position -= Main.npc[npcWhoAmI].netOffset;
                            if (proj.stopsDealingDamageAfterPenetrateHits)
                            {
                                proj.penetrate = -1;
                                proj.damage = 0;
                            }
                        }
                    }
                    if (proj.aiStyle == 7)
                    {
                        proj.ai[0] = 1f;
                        proj.damage = 0;
                        proj.netUpdate = true;
                    }
                    else if (proj.aiStyle == 13)
                    {
                        proj.ai[0] = 1f;
                        proj.netUpdate = true;
                    }
                    else if (proj.aiStyle == 69)
                    {
                        proj.ai[0] = 1f;
                        proj.netUpdate = true;
                    }
                    else if (proj.type == 607)
                    {
                        proj.ai[0] = 1f;
                        proj.netUpdate = true;
                        proj.friendly = false;
                    }
                    else if (proj.type == 638 || proj.type == 639 || proj.type == 640)
                    {
                        array[npcWhoAmI] = -1;
                        nPC.immune[proj.owner] = 0;
                        proj.damage = (int)((double)proj.damage * 0.96);
                    }
                    else if (proj.type == 617)
                    {
                        array[npcWhoAmI] = 8;
                        nPC.immune[proj.owner] = 0;
                    }
                    else if (proj.type == 656)
                    {
                        array[npcWhoAmI] = 8;
                        nPC.immune[proj.owner] = 0;
                        proj.localAI[0] += 1f;
                    }
                    else if (proj.type == 618)
                    {
                        array[npcWhoAmI] = 20;
                        nPC.immune[proj.owner] = 0;
                    }
                    else if (proj.type == 642)
                    {
                        array[npcWhoAmI] = 10;
                        nPC.immune[proj.owner] = 0;
                    }
                    else if (proj.type == 857)
                    {
                        array[npcWhoAmI] = 10;
                        nPC.immune[proj.owner] = 0;
                    }
                    else if (proj.type == 611 || proj.type == 612)
                    {
                        array[npcWhoAmI] = 6;
                        nPC.immune[proj.owner] = 4;
                    }
                    else if (proj.type == 645)
                    {
                        array[npcWhoAmI] = -1;
                        nPC.immune[proj.owner] = 0;
                        if (proj.ai[1] != -1f)
                        {
                            proj.ai[0] = 0f;
                            proj.ai[1] = -1f;
                            proj.netUpdate = true;
                        }
                    }
                    proj.numHits++;
                    if (proj.type == 697)
                    {
                        if (proj.ai[0] >= 42f)
                            proj.localAI[1] = 1f;
                    }
                    else if (proj.type == 699)
                    {
                        //SummonMonkGhast(); // TODO?
                    }
                    else if (proj.type == 706)
                    {
                        proj.damage = (int)((float)proj.damage * 0.95f);
                    }
                    else if (proj.type == 728)
                    {
                        //SummonSuperStarSlash(nPC.Center); // TODO?
                    }
                    else if (proj.type == 34)
                    {
                        if (proj.ai[0] == -1f)
                        {
                            proj.ai[1] = -1f;
                            proj.netUpdate = true;
                        }
                    }
                    else if (proj.type == 79)
                    {
                        if (proj.ai[0] == -1f)
                        {
                            proj.ai[1] = -1f;
                            proj.netUpdate = true;
                        }
                        settings2 = new ParticleOrchestraSettings
                        {
                            PositionInWorld = nPC.Center,
                            MovementVector = proj.velocity
                        };
                        ParticleOrchestrator.RequestParticleSpawn(clientOnly: false, ParticleOrchestraType.RainbowRodHit, settings2);
                    }
                    else if (proj.type == 931)
                    {
                        int num38 = proj.FindTargetWithLineOfSight();
                        if (num38 != -1)
                        {
                            proj.ai[0] = num38;
                            proj.netUpdate = true;
                        }
                    }
                    else if (proj.aiStyle == 165)
                    {
                        if (nPC.active)
                            Main.player[proj.owner].MinionAttackTargetNPC = npcWhoAmI;
                    }
                    else if (proj.type == 623)
                    {
                        settings2 = new ParticleOrchestraSettings
                        {
                            PositionInWorld = Vector2.Lerp(proj.Center, nPC.Hitbox.ClosestPointInRect(proj.Center), 0.5f) + new Vector2(0f, Main.rand.NextFloatDirection() * 10f),
                            MovementVector = new Vector2((float)proj.direction, Main.rand.NextFloatDirection() * 0.5f) * (3f + 3f * Main.rand.NextFloat())
                        };
                        ParticleOrchestrator.RequestParticleSpawn(clientOnly: false, ParticleOrchestraType.StardustPunch, settings2);
                    }
                    if (flag12)
                        Main.player[proj.owner].SetMeleeHitCooldown(npcWhoAmI, Main.player[proj.owner].itemAnimation);
                }
            }
        }
    }
}