using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public abstract class Cube : MonoBehaviour
{
    [SerializeField] protected int hp;

    [Header("Visual")]
    [SerializeField] SpriteRenderer sr;
    [SerializeField] SpriteRenderer crackSr;
    [SerializeField] List<Sprite> crackSprites;

    [Header("Particles")]
    [SerializeField] ParticleSystem hitPs;
    [SerializeField] ParticleSystem breakPs;

    [Header("Audio")]
    [SerializeField] List<AudioClip> hitSfxs;
    [SerializeField] List<AudioClip> breakSfxs;

    public int Hp => hp;
    public virtual void Dig(ToolType toolType)
    {
        sr.transform.DOShakePosition(0.2f, new Vector3(0.075f, 0, 0), 40, 0);
    }

    int oriHp;
    private void Start()
    {
        oriHp = hp;

        Sprite sprite = sr.sprite;
        Color[] colorData;
        colorData = sprite.texture.GetPixels((int)(sprite.rect.size.x / 2), (int)(sprite.rect.size.y / 2), 7, 7);

        System.Random random = new System.Random();

        int keyCount = 5;
        List<GradientColorKey> gradientColorKeys = new List<GradientColorKey>();
        for (int i = 0; i < keyCount; i++)
        {
            var randomColor = colorData[random.Next(0, colorData.Length)];
            var time = (1f / (keyCount - 1)) * i;
            gradientColorKeys.Add(new GradientColorKey(randomColor, time));
        }

        var hitColorGradient = hitPs.main.startColor;
        //hitMain.startColor = particleTint;
        hitColorGradient.gradient.SetKeys(
            gradientColorKeys.ToArray(),
            new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) });
        var hitMain = hitPs.main;
        hitMain.startColor = hitColorGradient;

        var breakColorGradient = breakPs.main.startColor;
        //breakMain.startColor = particleTint;
        breakColorGradient.gradient.SetKeys(
            gradientColorKeys.ToArray(),
            new GradientAlphaKey[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 1f) });
        var breakMain = breakPs.main;
        breakMain.startColor = breakColorGradient;
    }
    public void SetSortingOrder(int order)
    {
        sr.sortingOrder = order;
        if (crackSr)
            crackSr.sortingOrder = order + 1;
    }
    protected void MinusHp(int reduction)
    {
        hp -= reduction;

        if (hp <= 0)
        {
            sr.enabled = false;
            if (crackSr)
                crackSr.enabled = false;
        }

        UpdateCrackSprite();

        if (hp <= 0)
        {
            breakPs.Play();
            AudioManager.PlaySfxs(breakSfxs);
        }
        else
        {
            hitPs.Play();
            AudioManager.PlaySfxs(hitSfxs);
        }
    }
    void UpdateCrackSprite()
    {
        if (crackSr)
        {
            var hpPercentage = hp / (float)oriHp;
            if (hpPercentage < 0.33f)
                crackSr.sprite = crackSprites[2];
            else if (hpPercentage < 0.66f)
                crackSr.sprite = crackSprites[1];
            else if (hpPercentage < 1f)
                crackSr.sprite = crackSprites[0];
        }
    }
}

public abstract class PowerUp : Cube
{
    public override void Dig(ToolType toolType)
    {
        base.Dig(toolType);
        switch (toolType)
        {
            case ToolType.Pickaxe:
                MinusHp(1);
                break;
            case ToolType.Shovel:
                MinusHp(1);
                break;
            case ToolType.Hand:
                MinusHp(1);
                break;
            case ToolType.Omnidrill:
                MinusHp(hp);
                break;
        }
    }
}
