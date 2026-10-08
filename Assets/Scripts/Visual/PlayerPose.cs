using System;
using UnityEngine;

namespace Vadronia
{
    /// <summary>
    /// Pose extra do player aplicada DEPOIS de CharacterView.Animate (os quadros do vídeo continuam os mesmos).
    /// Corrida: inclinação para a frente, pulinho a cada passada, esmagamento ao pisar e poeira.
    /// Combate: o controlador da espada preenche Offset/Roll/Scale a cada frame.
    /// Chame Apply() uma vez por frame, logo depois de motor.Tick (que reposiciona o root).
    /// </summary>
    public sealed class PlayerPose : IDisposable
    {
        const int DustCount = 12;
        const float RunSpeed = 3.2f;   // acima disso (velocidade real) conta como corrida; andar = 2.4

        readonly CharacterView view;
        readonly SpriteRenderer body;
        readonly Transform shadow;
        readonly VisualLibrary art = new VisualLibrary();
        readonly GameObject dustRoot = new GameObject("Poeira da corrida");
        readonly SpriteRenderer[] dust = new SpriteRenderer[DustCount];
        readonly float[] dustLife = new float[DustCount];
        readonly Vector2[] dustVel = new Vector2[DustCount];
        Vector2 lastPos;
        float lean, runBlend;
        int lastStep, nextDust;

        public Vector2 Offset;
        public float Roll;
        public Vector2 Scale = Vector2.one;
        public bool Running { get; private set; }
        public SpriteRenderer Body => body;

        public PlayerPose(CharacterView view)
        {
            this.view = view;
            body = view.Transform.GetComponent<SpriteRenderer>();
            shadow = view.Transform.Find("Sombra dos pés");
            lastPos = new Vector2(view.Position.X, view.Position.Y);
            for (int i = 0; i < DustCount; i++)
            {
                dust[i] = art.Add(dustRoot.transform, "Poeira", art.SoftDisc, Vector2.zero, Vector2.one * .2f, Color.clear, 0);
                dust[i].gameObject.SetActive(false);
            }
        }

        public void Apply(float dt, bool wantRun)
        {
            Vector2 pos = new Vector2(view.Position.X, view.Position.Y);
            Vector2 vel = dt > 0 ? (pos - lastPos) / dt : Vector2.zero;
            lastPos = pos;
            bool running = wantRun && view.Cycle.Moving && vel.magnitude > RunSpeed;
            Running = running;
            runBlend = Mathf.MoveTowards(runBlend, running ? 1f : 0f, dt * 7f);
            lean = Mathf.Lerp(lean, running ? -Mathf.Clamp(vel.x / 4.6f, -1f, 1f) * 10f : 0f, 1f - Mathf.Exp(-14f * dt));

            float phase = view.AnimationPhase;
            float hop = Mathf.Abs(Mathf.Sin(phase * Mathf.PI * 2f));   // 2 pisadas por ciclo
            Vector2 off = Offset, sc = Scale;
            float roll = Roll + lean;
            if (runBlend > 0f)
            {
                float land = 1f - hop;
                off.y += hop * .085f * runBlend;
                sc.x *= 1f + .03f * land * runBlend;
                sc.y *= 1f - .045f * land * runBlend + .04f * Mathf.Abs(vel.y) / 4.6f * runBlend;
            }

            var t = view.Transform;
            t.position += new Vector3(off.x, off.y, 0);   // Place() já reposicionou o root neste frame
            t.rotation = Quaternion.Euler(0, 0, roll);
            t.localScale = new Vector3(sc.x, sc.y, 1f);
            if (shadow != null) shadow.localPosition = new Vector3(-off.x, -off.y, 0);

            int step = Mathf.FloorToInt(phase * 2f) & 1;
            if (running && step != lastStep) SpawnDust(pos, vel);
            lastStep = step;
            TickDust(dt);
        }

        /// <summary>Poeira extra (ex.: pouso do golpe 3).</summary>
        public void Burst(Vector2 feet, int count, float spread)
        {
            for (int i = 0; i < count; i++)
            {
                float a = (i + UnityEngine.Random.value) / count * Mathf.PI * 2f;
                Spawn(feet + new Vector2(Mathf.Cos(a) * .15f, Mathf.Sin(a) * .08f), new Vector2(Mathf.Cos(a), Mathf.Sin(a) * .5f) * spread);
            }
        }

        void SpawnDust(Vector2 pos, Vector2 vel)
        {
            Vector2 back = vel.sqrMagnitude > .01f ? -vel.normalized : Vector2.zero;
            Spawn(pos + back * .12f + new Vector2(UnityEngine.Random.Range(-.08f, .08f), 0), back * .5f + new Vector2(0, .12f));
        }

        void Spawn(Vector2 at, Vector2 velocity)
        {
            int i = nextDust; nextDust = (nextDust + 1) % DustCount;
            dust[i].gameObject.SetActive(true);
            dust[i].transform.position = new Vector3(at.x, at.y + .03f, 0);
            dust[i].transform.localScale = Vector3.one * .2f;
            dust[i].color = new Color(.82f, .76f, .62f, .55f);
            dust[i].sortingOrder = (body != null ? body.sortingOrder : 0) - 3;
            dustLife[i] = .38f; dustVel[i] = velocity;
        }

        void TickDust(float dt)
        {
            for (int i = 0; i < DustCount; i++)
            {
                if (dustLife[i] <= 0) continue;
                dustLife[i] -= dt;
                if (dustLife[i] <= 0) { dust[i].gameObject.SetActive(false); continue; }
                float k = 1f - dustLife[i] / .38f;
                dust[i].transform.position += new Vector3(dustVel[i].x, dustVel[i].y, 0) * dt;
                dust[i].transform.localScale = Vector3.one * Mathf.Lerp(.2f, .5f, k);
                dust[i].color = new Color(.82f, .76f, .62f, .55f * (1f - k));
            }
        }

        public void Reset()
        {
            Offset = Vector2.zero;
            Roll = 0f;
            Scale = Vector2.one;
            lean = runBlend = 0f;
            Running = false;
            var pos = view.Position;
            lastPos = new Vector2(pos.X, pos.Y);
            view.Transform.position = new Vector3(pos.X, pos.Y, 0);
            view.Transform.rotation = Quaternion.identity;
            view.Transform.localScale = Vector3.one;
            if (shadow != null) shadow.localPosition = Vector3.zero;
        }

        public void Dispose()
        {
            if (dustRoot != null) UnityEngine.Object.Destroy(dustRoot);
            art.Dispose();
        }
    }
}