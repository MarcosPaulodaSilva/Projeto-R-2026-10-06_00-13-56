using System;
using System.Collections.Generic;
using UnityEngine;

namespace Vadronia
{
    /// <summary>Efeitos curtos reutilizáveis (arco de corte, anel, rastro da lâmina, fantasma do corpo).</summary>
    public sealed class SwordFx : IDisposable
    {
        sealed class Item
        {
            public GameObject go; public SpriteRenderer sr;
            public float life, max, s0, s1, spin, rot; public Color color;
        }

        readonly List<Item> items = new List<Item>();
        readonly GameObject root = new GameObject("Efeitos da espada");

        public void Spawn(Sprite sprite, Vector2 pos, float rotDeg, float s0, float s1, Color color,
            float life, float spin, int order, bool flipX = false)
        {
            Item it = null;
            for (int i = 0; i < items.Count; i++) if (items[i].life <= 0) { it = items[i]; break; }
            if (it == null)
            {
                it = new Item { go = new GameObject("Efeito") };
                it.go.transform.SetParent(root.transform, false);
                it.sr = it.go.AddComponent<SpriteRenderer>();
                items.Add(it);
            }
            it.life = it.max = Mathf.Max(.01f, life); it.s0 = s0; it.s1 = s1; it.spin = spin; it.rot = rotDeg; it.color = color;
            it.go.SetActive(true);
            it.go.transform.position = new Vector3(pos.x, pos.y, 0);
            it.go.transform.rotation = Quaternion.Euler(0, 0, rotDeg);
            it.go.transform.localScale = new Vector3(s0, s0, 1);
            it.sr.sprite = sprite; it.sr.color = color; it.sr.sortingOrder = order; it.sr.flipX = flipX;
        }

        /// <summary>Cópia translúcida do sprite do player (rastro de movimento).</summary>
        public void Ghost(SpriteRenderer source, Color color, float life)
        {
            if (source == null || source.sprite == null) return;
            Spawn(source.sprite, source.transform.position, source.transform.eulerAngles.z, 1f, 1f,
                color, life, 0, source.sortingOrder - 2, source.flipX);
        }

        public void Tick(float dt)
        {
            for (int i = 0; i < items.Count; i++)
            {
                var it = items[i];
                if (it.life <= 0) continue;
                it.life -= dt;
                if (it.life <= 0) { it.go.SetActive(false); continue; }
                float t = 1f - it.life / it.max;
                float s = Mathf.Lerp(it.s0, it.s1, 1f - (1f - t) * (1f - t));
                it.rot += it.spin * dt;
                it.go.transform.localScale = new Vector3(s, s, 1);
                it.go.transform.rotation = Quaternion.Euler(0, 0, it.rot);
                var c = it.color; c.a *= Mathf.Pow(1f - t, 1.4f);
                it.sr.color = c;
            }
        }

        public void Dispose() { if (root != null) UnityEngine.Object.Destroy(root); }
    }
}