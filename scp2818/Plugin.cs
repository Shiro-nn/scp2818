using Mirror;
using Qurre;
using Qurre.API;
using Qurre.API.Events;
using Qurre.API.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Version = System.Version;
namespace scp2818
{
    public class Main : Plugin
    {
        public static string UserIDShooterDead = "";
        public override string Developer => "fydne";
        public override string Name => "SCP 2818";
        public override Version Version => new Version(1, 0, 1);
        public override Version NeededQurreVersion => new Version(1, 3, 0);
        public override int Priority => 10000;
        public override void Enable()
        {
            Qurre.Events.Round.WaitingForPlayers += WFP;
            Qurre.Events.Player.Shooting += Shoot;
            Qurre.Events.Player.Damage += Damage;
            Qurre.Events.Player.PickupItem += PickupItem;
        }
        public override void Disable()
        {
            Qurre.Events.Round.WaitingForPlayers -= WFP;
            Qurre.Events.Player.Shooting -= Shoot;
            Qurre.Events.Player.Damage -= Damage;
            Qurre.Events.Player.PickupItem -= PickupItem;
        }
        private void Shoot(ShootingEvent ev)
        {
            if(ev.Shooter.Inventory.GetItemInHand().durability == 2818 && ev.WeaponType == WeaponType.Epsilon11)
            {
                ev.Shooter.Inventory.items.ModifyDuration(ev.Shooter.Inventory.items.IndexOf(ev.Shooter.Inventory.GetItemInHand()), 2819);
                UserIDShooterDead = ev.Shooter.UserId;
                Vector3 pos = ev.Shooter.Position;
                MEC.Timing.RunCoroutine(Shoot2818(ev, pos), "TimingKillSCP2818");
            }
        }
        private void Damage(DamageEvent ev)
        {
            if (ev.Attacker == null || ev.Target == null || ev.Attacker.UserId == null || ev.Target.UserId == null || ev.Target.PlayerStats == null) return;
            if (ev.Attacker.UserId == UserIDShooterDead && ev.DamageType == DamageTypes.E11StandardRifle && ev.Attacker.UserId != ev.Target.UserId && ev.Allowed)
            {
                if (ev.Target.Team != Team.SCP) ev.Target.Kill(DamageTypes.E11StandardRifle);
                else ev.Target.Kill(DamageTypes.Bleeding);
                UserIDShooterDead = "";
            }
        }
        private void PickupItem(PickupItemEvent ev)
        {
            if (ev.Pickup.durability == 2818 && ev.Pickup.ItemId == ItemType.GunE11SR)
            {
                ev.Player.ShowHint(Config.GetString("scp2818_pickup", "<b><color=red>You picked up SCP 2818</color></b>"), 5);
                UserIDShooterDead = "";
            }
        }
        private void WFP()
        {
            MEC.Timing.RunCoroutine(LoopCheck(), "LoopCheckSCP2818");
        }
        public IEnumerator<float> LoopCheck()
        {
            bool first_warn = true;
            yield return MEC.Timing.WaitForSeconds(6f);
            for (; ; )
            {
                yield return MEC.Timing.WaitForSeconds(1f);
                var locker = LockerManager.singleton.lockers.Where(x => x.name == "Glocker A").First();
                var list = Map.Pickups.Where(x => Vector3.Distance(x.transform.position, locker.gameObject.position) < 2f && x.ItemId == ItemType.GunE11SR);
                foreach (Pickup p in list)
                {
                    p.durability = 2818f;
                    p.weaponMods = new Pickup.WeaponModifiers(true, 4, 4, 1);
                    if (!first_warn)
                    {
                        Log.Custom("SCP 2818 found. I stop cyclic search.", "Found", ConsoleColor.DarkGreen);
                        first_warn = false;
                    }
                }
                if (list.Count() == 0)
                {
                    if (first_warn)
                    {
                        Log.Custom("Warning! SCP 2818 Not Found. I start a cyclic search.", "Not Found", ConsoleColor.DarkRed);
                        first_warn = false;
                    }
                }
                else
                {
                    MEC.Timing.KillCoroutines("LoopCheckSCP2818");
                }
            }
        }
        private IEnumerator<float> Shoot2818(ShootingEvent ev, Vector3 pos)
        {
            yield return MEC.Timing.WaitForSeconds(0.1f);
            ev.Shooter.Kill(DamageTypes.E11StandardRifle);
            yield return MEC.Timing.WaitForSeconds(0.3f);
            foreach (Ragdoll doll in UnityEngine.Object.FindObjectsOfType<Ragdoll>().Where(x => x.owner.PlayerId == ev.Shooter.Id && Vector3.Distance(x.transform.position, pos) < 2f))
                NetworkServer.Destroy(doll.gameObject);
            yield return MEC.Timing.WaitForSeconds(0.3f);
            MEC.Timing.KillCoroutines("TimingKillSCP2818");
        }
    }
}