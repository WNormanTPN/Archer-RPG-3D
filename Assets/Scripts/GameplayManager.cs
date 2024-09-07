using System;
using Entity;
using MyEditor;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

public class GameplayManager : MonoBehaviour
{
    [InspectorGroup("End Game Panel")]
    public GameObject panel;
    public GameObject title;
    public Texture2D victoryTexture;
    public Texture2D deathTexture;
    public Text killCountText;
    public GameObject star1;
    public GameObject star2;
    public GameObject star3;
    public Texture2D starOnTexture;
    public Texture2D starOffTexture;
    
    [NonGroup]
    public string mainMenuSceneKey;
    public string levelSceneKey;
    
    private Image titleImage;
    private MonsterWaveManager monsterWaveManager;
    private Image star1Image;
    private Image star2Image;
    private Image star3Image;
    
    void Start()
    {
        titleImage = title.GetComponent<Image>();
        monsterWaveManager = GetComponent<MonsterWaveManager>();
        star1Image = star1.GetComponent<Image>();
        star2Image = star2.GetComponent<Image>();
        star3Image = star3.GetComponent<Image>();
    }

    private void Update()
    {
        if (monsterWaveManager.currentWave > monsterWaveManager.totalWave
            && MonsterWaveManager.monsters.Count == 0)
        {
            titleImage.sprite = Sprite.Create(
                victoryTexture,
                new Rect(0, 0, victoryTexture.width, victoryTexture.height),
                new Vector2(0.5f, 0.5f));
            SetUpEndGamePanel();
        }
        else if (!monsterWaveManager.player)
        {
            titleImage.sprite = Sprite.Create(
                deathTexture,
                new Rect(0, 0, deathTexture.width, deathTexture.height),
                new Vector2(0.5f, 0.5f));
            SetUpEndGamePanel();
        }
    }

    private void SetUpEndGamePanel()
    {
        panel.SetActive(true);
        killCountText.text = "Kills: " + MonsterWaveManager.killCount;
        var wavePerStar = monsterWaveManager.totalWave / 3;
        var starCount = monsterWaveManager.currentWave / wavePerStar;
        star1Image.sprite = Sprite.Create(
            starCount >= 1 ? starOnTexture : starOffTexture,
            new Rect(0, 0, starOnTexture.width, starOnTexture.height),
            new Vector2(0.5f, 0.5f));
        star2Image.sprite = Sprite.Create(
            starCount >= 2 ? starOnTexture : starOffTexture,
            new Rect(0, 0, starOnTexture.width, starOnTexture.height),
            new Vector2(0.5f, 0.5f));
        star3Image.sprite = Sprite.Create(
            starCount >= 3 && MonsterWaveManager.monsters.Count == 0 ? starOnTexture : starOffTexture,
            new Rect(0, 0, starOnTexture.width, starOnTexture.height),
            new Vector2(0.5f, 0.5f));
    }
    
    public void GoHome()
    {
        Addressables.LoadSceneAsync(mainMenuSceneKey, UnityEngine.SceneManagement.LoadSceneMode.Single);
    }
    
    public void Restart()
    {
        Addressables.LoadSceneAsync(levelSceneKey, UnityEngine.SceneManagement.LoadSceneMode.Single);
    }
}
