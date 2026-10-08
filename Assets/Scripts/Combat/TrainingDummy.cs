using System;
using UnityEngine;

namespace Vadronia
{
    public readonly struct SwordHit
    {
        public readonly float Damage;
        public readonly Vector2 Direction;
        public readonly float Knockback;
        public readonly string Label;
        public SwordHit(float damage, Vector2 direction, float knockback, string label)
        {
            Damage = damage; Direction = direction; Knockback = knockback; Label = label;
        }
    }

    /// <summary>Qualquer coisa que a espada possa acertar (inimigos futuros implementam isto).</summary>
    public interface ISwordTarget
    {
        Vector2 Position { get; }   // pontos dos pés, igual ao player
        float Radius { get; }
        bool Alive { get; }
        void ReceiveHit(SwordHit hit);
    }

    /// <summary>Boneco de treino: pisca, é empurrado, mostra barra de vida e se recupera sozinho.</summary>
    public sealed class TrainingDummy : ISwordTarget, IDisposable
    {
        const float MaxHp = 120f;
        static readonly Color Straw = new Color(.72f, .56f, .32f);

        readonly GameObject root;
        readonly VisualLibrary art = new VisualLibrary();
        readonly SpriteRenderer stake, torso, arms, head, barBack, bar;
        readonly Vector2 home;
        Vector2 pos, vel;
        float hp = MaxHp, flash, idle;

        public Vector2 Position => pos;
        public float Radius => .38f;
        public bool Alive => true;

        public TrainingDummy(Vector2 position)
        {
            home = pos = position;
            root = new GameObject("Boneco de treino");
            art.Add(root.transform, "Sombra", art.SoftDisc, Vector2.zero, new Vector2(.8f, .26f), new Color(.08f, .09f, .07f, .6f), -16000);
            stake = art.Add(root.transform, "Estaca", art.Square, new Vector2(0, .45f), new Vector2(.09f, .9f), new Color(.36f, .24f, .14f), 0);
            torso = art.Add(root.transform, "Corpo", art.Square, new Vector2(0, .66f), new Vector2(.46f, .5f), Straw, 0);
            arms = art.Add(root.transform, "Braços", art.Square, new Vector2(0, .78f), new Vector2(.86f, .1f), new Color(.5f, .36f, .2f), 0);
            head = art.Add(root.transform, "Cabeça", art.Disc, new Vector2(0, 1.06f), new Vector2(.34f, .34f), Straw, 0);
            barBack = art.Add(root.transform, "Vida (fundo)", art.Square, new Vector2(0, 1.5f), new Vector2(.72f, .09f), new Color(0, 0, 0, .7f), 15000);
            bar = art.Add(root.transform, "Vida", art.Square, new Vector2(0, 1.5f), new Vector2(.68f, .06f), new Color(.82f, .26f, .22f), 15001);
            Refresh();
        }

        public void ReceiveHit(SwordHit hit)
        {
            hp -= hit.Damage; flash = 1f; idle = 0;
            vel = Vector2.ClampMagnitude(vel + hit.Direction * hit.Knockback, 6f);
            Debug.Log("Boneco recebeu " + hit.Damage + " de dano  (" + hit.Label + ")  vida " + Mathf.Max(0, hp) + "/" + MaxHp);
            if (hp <= 0) { hp = MaxHp; Debug.Log("Boneco derrotado — vida restaurada."); }
        }

        public void Tick(float dt)
        {
            flash = Mathf.Max(0, flash - dt * 5f);
            pos += vel * dt;
            vel = Vector2.MoveTowards(vel, Vector2.zero, dt * 14f);
            if (vel.sqrMagnitude < .01f) pos = Vector2.MoveTowards(pos, home, dt * 1.2f);
            idle += dt;
            if (idle > 4f && hp < MaxHp) hp = Mathf.MoveTowards(hp, MaxHp, dt * 60f);
            Refresh();
        }

        void Refresh()
        {
            root.transform.position = new Vector3(pos.x, pos.y, 0);
            root.transform.rotation = Quaternion.Euler(0, 0, -vel.x * 2.5f);
            int order = -Mathf.RoundToInt(pos.y * 100) * 10 + 4;
            stake.sortingOrder = order; torso.sortingOrder = order + 1; arms.sortingOrder = order + 2; head.sortingOrder = order + 3;
            var c = Color.Lerp(Straw, Color.white, flash);
            torso.color = head.color = c;
            float ratio = Mathf.Clamp01(hp / MaxHp);
            bar.transform.localScale = new Vector3(.68f * ratio, .06f, 1f);
            bar.transform.localPosition = new Vector3(-.34f + .34f * ratio, 1.5f, 0);
        }

        public void Dispose() { if (root != null) UnityEngine.Object.Destroy(root); art.Dispose(); }
    }
}