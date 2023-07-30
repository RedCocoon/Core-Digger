using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public enum ToolType { Pickaxe, Shovel, Hand, Omnidrill }

public class GameManager : MonoBehaviour
{
    [Header("Core Configuration")]
    [SerializeField] List<LevelConfiguration> levelConfigurations;
    [SerializeField] float initialSpawnCount = 4;
    [SerializeField] float initialSpawnDelay;
    [SerializeField] float spawnInterval;
    [SerializeField] float wallChunkSpawnInterval;
    [SerializeField] float wallChunkYPosStart;
    [SerializeField] float pillarTopDistanceLoseThreshold = 3;
    [SerializeField] int adsContinueCubeRemoveCount = 11;

    [Header("Camera")]
    [SerializeField] Transform cameraTrans;
    [SerializeField] float cameraMoveSpeed;
    [SerializeField] float cameraSpeedMultiplier = 1;
    [SerializeField] float cameraSpeedIncreasePerLevel = 0.1f;
    [SerializeField] float maxCameraSpeedMultiplier = 2;
    [SerializeField] float distanceThreshold;
    [SerializeField] List<SpriteRenderer> bgFades;
    [SerializeField] float startRedTintPercentage = 85;

    [Header("Power Ups")]
    [SerializeField] List<PowerUp> powerUps;
    [SerializeField] int powerUpSpawnThreshold = 25;
    [SerializeField] float powerUpBaseSpawnChance = 1;
    [SerializeField] float powerUpSpawnChanceIncrement = 0.1f;
    [SerializeField] float omniDrillDuration = 5;
    [SerializeField] int scoreMultiplierValue = 2;
    [SerializeField] float scoreMultiplierDuration = 10;
    [SerializeField] Color scoreMultiplierTextColor;
    [SerializeField] ParticleSystem scoreMultiplierConfettiPs;
    [SerializeField] GameObject shieldBubble;
#if UNITY_EDITOR
    [SerializeField] Button testPower_2x, testPower_Omni, testPower_Shield;
#endif

    [Header("UI")]
    [SerializeField] Button menuButton;
    [SerializeField] GameObject menuPanel, leaderboardPanel, creditsPanel;
    [SerializeField] Button closeMenuButton, creditsButton, closeCreditsButton, leaderboardButton, closeLeaderboardButton;
    [SerializeField] TMPro.TMP_Text scoreText;
    [SerializeField] Material scoreTextMaterial;
    [SerializeField] VertexAttributeModifier scoreTextWave;
    [SerializeField] Button pickaxeTool, shovelTool, handTool, omnidrillButton;
    [SerializeField] RectTransform omnidrillRt;
    [SerializeField] Image omnidrillDurationBar;
    [SerializeField] Image omnidrillOverlay;
    [SerializeField] Color omnidrillOverlayPulseColor;
    [SerializeField] GameObject gameOverPanel;
    [SerializeField] Button retryButton, adsContinueButton;
    [SerializeField] TMPro.TMP_Text highscoreText;
    [SerializeField] TMPro.TMP_Text currentScoreText;

    [Header("Audio")]
    [SerializeField] AudioClip buttonClickSfx;
    [SerializeField] List<AudioClip> handToolSfxs;
    [SerializeField] List<AudioClip> shovelToolSfxs;
    [SerializeField] List<AudioClip> pickaxeToolSfxs;
    [SerializeField] List<AudioClip> tntSfxs;
    [SerializeField] List<AudioClip> omnidrillSfxs;
    [SerializeField] List<AudioClip> scoreMultiplierSfxs;
    [SerializeField] AudioClip shieldGetSfx;
    [SerializeField] AudioClip shieldPopSfx;

    LevelConfiguration currentLevelConfiguration;
    int levelConfigurationIndex = 0;
    bool gameStarted;
    float currentCameraMoveSpeed;
    float maxCameraSpeed;
    int scoreMultiplier = 1;
    int score = 0;
    List<Cube> spawnedCubes = new List<Cube>();
    int heightIndex = 0;
    int wallChunkHeightIndex = 0;
    List<WallChunk> spawnedWalls = new List<WallChunk>();
    Coroutine spawnCubeCoroutine;
    Coroutine spawnWallCoroutine;

    void Start()
    {
        ResetValues();
        DoInitialSpawning();

        AddButtonListeners();
    }
    void AddButtonListeners()
    {
        pickaxeTool.onClick.AddListener(UsePickaxe);
        shovelTool.onClick.AddListener(UseShovel);
        handTool.onClick.AddListener(UseHand);
        omnidrillButton.onClick.AddListener(UseOmnidrill);

#if UNITY_EDITOR
        testPower_2x.onClick.AddListener(ActivateScoreMultiplier);
        testPower_Omni.onClick.AddListener(ActivateOmnidrill);
        testPower_Shield.onClick.AddListener(ActivateShield);
#endif

        adsContinueButton.onClick.AddListener(AdsContinue);
        retryButton.onClick.AddListener(Retry);

        menuButton.onClick.AddListener(OpenMenu);
        closeMenuButton.onClick.AddListener(CloseMenu);
        leaderboardButton.onClick.AddListener(OpenLeaderboard);
        closeLeaderboardButton.onClick.AddListener(CloseLeaderboard);
        creditsButton.onClick.AddListener(OpenCredits);
        closeCreditsButton.onClick.AddListener(CloseCredits);
    }

    #region General Menu
    void OpenMenu() { menuPanel.SetActive(true); }
    void CloseMenu() { menuPanel.SetActive(false); }
    void OpenLeaderboard() { leaderboardPanel.SetActive(true); }
    void CloseLeaderboard() { leaderboardPanel.SetActive(false); }
    void OpenCredits() { creditsPanel.SetActive(true); }
    void CloseCredits() { creditsPanel.SetActive(false); }
    #endregion

    void DoInitialSpawning()
    {
        for (int i = 0; i < initialSpawnCount; i++)
            SpawnCube();

        SpawnWholeLevelWallChunk();
    }

    #region Level
    void EnterNewLevel()
    {
        numberOfCubeSpawnedForThisLevel = 0;
        currentCameraMoveSpeed += cameraSpeedIncreasePerLevel;
        if (currentCameraMoveSpeed > maxCameraSpeed)
            currentCameraMoveSpeed = maxCameraSpeed;

        levelConfigurationIndex++;
        ApplyLevelConfiguration(levelConfigurations[levelConfigurationIndex]);
        SpawnWholeLevelWallChunk();
    }
    void ApplyLevelConfiguration(LevelConfiguration _levelConfiguration)
    {
        currentLevelConfiguration = _levelConfiguration;
        pickaxeTool.image.color = currentLevelConfiguration.PickaxeColor;
        shovelTool.image.color = currentLevelConfiguration.ShovelColor;
    }
    #endregion

    void Update()
    {
        UpdateCameraPosition();
        CheckPillarReachTop();

#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.Alpha1))
            UsePickaxe();
        if (Input.GetKeyDown(KeyCode.Alpha2))
            UseShovel();
        if (Input.GetKeyDown(KeyCode.Alpha3))
            UseHand();
#endif
    }
    float speedTrackingMultiplier = 2;
    Tweener cameraTrackingTweener;
    void UpdateCameraPosition()
    {
        if (gameStarted)
        {
            cameraTrans.position -= new Vector3(0, currentCameraMoveSpeed * Time.deltaTime, 0);

            float cameraDistanceToFirstCube = cameraTrans.position.y - spawnedCubes[0].transform.position.y;
            if (spawnedCubes[0].transform.position.y < cameraTrans.position.y &&
                cameraDistanceToFirstCube > distanceThreshold)
            {
                float targetYPos = spawnedCubes[0].transform.position.y + 0.5f;
                cameraTrackingTweener = cameraTrans.DOMoveY(targetYPos, 0.5f).SetEase(Ease.Linear);
            }
        }
    }
    void CheckPillarReachTop()
    {
        if (gameStarted)
        {
            float distanceFromFirstCubeToCamera = spawnedCubes[0].transform.position.y - cameraTrans.position.y;
            float percentageGettingToTop = distanceFromFirstCubeToCamera / pillarTopDistanceLoseThreshold * 100f;

            if (spawnedCubes[0].transform.position.y > cameraTrans.position.y)
            {
                if (distanceFromFirstCubeToCamera > pillarTopDistanceLoseThreshold)
                    GameOver(GameOverType.Stack);
            }

            if (percentageGettingToTop > startRedTintPercentage)
            {
                var tintPercentage = (percentageGettingToTop - startRedTintPercentage) / (100f - startRedTintPercentage);
                bgFades[0].color = new Color(0.2941177f * tintPercentage, 0, 0, 1);
                bgFades[1].color = new Color(0.2941177f * tintPercentage, 0, 0, 1);
            }
        }
    }

    void StartGame()
    {
        if (gameStarted)
            return;

        gameStarted = true;

        spawnCubeCoroutine = StartCoroutine(SpawnCubesCoroutine());
        //spawnWallCoroutine = StartCoroutine(SpawnWallChunksCoroutine());
    }

    #region Cube Spawning
    int numberOfCubeSpawnedForThisLevel = 0;
    int numberOfNormalCubeSpawned = 0;
    IEnumerator SpawnCubesCoroutine()
    {
        WaitForSeconds cubeSpawnInterval = new WaitForSeconds(spawnInterval);
        while (true)
        {
            SpawnCube();
            yield return cubeSpawnInterval;
        }
    }
    void SpawnCube()
    {
        var cube = Instantiate(GetCubeToSpawn(), new Vector3(0, heightIndex * 0.5f * (24f / 32f), 0), Quaternion.identity);
        cube.SetSortingOrder(heightIndex);
        spawnedCubes.Add(cube);
        heightIndex--;
        numberOfCubeSpawnedForThisLevel++;

        if (numberOfCubeSpawnedForThisLevel > currentLevelConfiguration.Length)
            EnterNewLevel();

        Cube GetCubeToSpawn()
        {
            if (numberOfNormalCubeSpawned > powerUpSpawnThreshold)
            {
                float spawnChanceIncreased = (numberOfNormalCubeSpawned - powerUpSpawnThreshold) * powerUpSpawnChanceIncrement;
                float totalChanceToSpawn = powerUpBaseSpawnChance + spawnChanceIncreased;
                //Debug.Log(totalChanceToSpawn);

                bool willSpawnPowerUp = ChancePercentage(totalChanceToSpawn);

                if (willSpawnPowerUp)
                {
                    var powerUpToSpawn = powerUps[Random.Range(0, powerUps.Count)];
                    numberOfNormalCubeSpawned = 0;
                    return powerUpToSpawn;
                }
            }

            Cube cubeToSpawn = currentLevelConfiguration.Cubes[Random.Range(0, currentLevelConfiguration.Cubes.Count)];
            numberOfNormalCubeSpawned++;
            return cubeToSpawn;
        }
    }
    System.Random randomHandler = new System.Random();
    bool ChancePercentage(float percentage)
    {
        // Ensure the percentage is within the valid range (0 to 100)
        percentage = Mathf.Max(0, Mathf.Min(100, percentage));

        // Generate a random number between 0 and 100
        int randomNumber = randomHandler.Next(0, 101);

        // If the random number is less than the given percentage, return true
        return randomNumber < percentage;
    }
    void RemoveCubesFromTop(int count)
    {
        for (int i = 0; i < count; i++)
        {
            Destroy(spawnedCubes[0].gameObject);
            spawnedCubes.RemoveAt(0);
        }
    }
    #endregion

    #region Wall Chunk Spawning
    void SpawnWholeLevelWallChunk()
    {
        int wallChunksToSpawnForThisLevel = currentLevelConfiguration.Length / 5;
        for (int i = 0; i < wallChunksToSpawnForThisLevel; i++)
        {
            SpawnWallChunk(i);
        }
    }
    void SpawnWallChunk(int wallIndexOfCurrentLevel)
    {
        var wallChunkToSpawn = GetWallChunkToSpawn();
        if (wallChunkToSpawn != null)
        {
            var wall = Instantiate(wallChunkToSpawn, new Vector3(-0.5f, wallChunkYPosStart - (wallChunkHeightIndex * 1.823f), 0), Quaternion.identity);
            wall.SetSortingOrder(-wallChunkHeightIndex);
            spawnedWalls.Add(wall);
        }
        wallChunkHeightIndex++;

        WallChunk GetWallChunkToSpawn()
        {
            if (currentLevelConfiguration.EndChunk == null &&
                currentLevelConfiguration.WallChunks.Count == 0)
                return null;

            int wallChunksToSpawnForThisLevel = currentLevelConfiguration.Length / 5;
            bool hasSpecialEndChunk = currentLevelConfiguration.EndChunk != null;

            if (hasSpecialEndChunk)
            {
                bool isEndChunk = wallIndexOfCurrentLevel == wallChunksToSpawnForThisLevel - 1;
                if (isEndChunk)
                    return currentLevelConfiguration.EndChunk;
                else
                    return currentLevelConfiguration.WallChunks[Random.Range(0, currentLevelConfiguration.WallChunks.Count)];
            }

            return currentLevelConfiguration.WallChunks[Random.Range(0, currentLevelConfiguration.WallChunks.Count)];
        }
    }
    #endregion

    #region Tools
    void HideNormalToolButtons()
    {
        shovelTool.gameObject.SetActive(false);
        pickaxeTool.gameObject.SetActive(false);
        handTool.gameObject.SetActive(false);
    }
    void ShowNormalToolButtons()
    {
        shovelTool.gameObject.SetActive(true);
        pickaxeTool.gameObject.SetActive(true);
        handTool.gameObject.SetActive(true);
    }
    void UsePickaxe()
    {
        StartGame();

        AudioManager.PlaySfxs(handToolSfxs);

        spawnedCubes[0].Dig(ToolType.Pickaxe);
        if (spawnedCubes[0] is Tnt)
            HandleTntWithNonHand();

        HandleCubeDestruction();
    }
    void UseShovel()
    {
        StartGame();

        AudioManager.PlaySfxs(handToolSfxs);

        spawnedCubes[0].Dig(ToolType.Shovel);
        if (spawnedCubes[0] is Tnt)
            HandleTntWithNonHand();

        HandleCubeDestruction();
    }
    void HandleTntWithNonHand()
    {
        if (shielded)
            RemoveShield();
        else
        {
            AudioManager.PlaySfxs(tntSfxs);
            GameOver(GameOverType.Tnt);
        }
    }
    void UseHand()
    {
        StartGame();

        AudioManager.PlaySfxs(handToolSfxs);
        spawnedCubes[0].Dig(ToolType.Hand);
        HandleCubeDestruction();
    }
    void UseOmnidrill()
    {
        AudioManager.PlaySfxs(omnidrillSfxs);
        spawnedCubes[0].Dig(ToolType.Omnidrill);
        HandleCubeDestruction();
    }
    void HandleCubeDestruction()
    {
        if (spawnedCubes[0].Hp <= 0)
        {
            IncreaseScore();

            if (spawnedCubes[0] is PowerUp)
                GetPowerUp(spawnedCubes[0]);

            Destroy(spawnedCubes[0].gameObject, 1.5f);
            spawnedCubes.RemoveAt(0);

            SpawnCube();
        }
    }
    #endregion

    #region Power Up
    Coroutine omniDrillCoroutine;
    Coroutine scoreMultiplierCoroutine;
    Coroutine shieldFollowCoroutine;
    void GetPowerUp(Cube powerUp)
    {
        switch (powerUp)
        {
            case OmniDrill om:
                ActivateOmnidrill();
                break;
            case Shield sh:
                ActivateShield();
                break;
            case ScoreMultiplier sc:
                ActivateScoreMultiplier();
                break;
        }
    }
    void ActivateOmnidrill()
    {
        if (omniDrillCoroutine != null)
            StopCoroutine(omniDrillCoroutine);
        AudioManager.Instance.PlayDrillBgm();
        omniDrillCoroutine = StartCoroutine(ActivateOmniDrillCoroutine());
    }
    Tweener omnidrillDurationBarTweener;
    Tweener overlayPulseTweener;
    IEnumerator ActivateOmniDrillCoroutine()
    {
        HideNormalToolButtons();

        if (omnidrillDurationBarTweener != null)
            omnidrillDurationBarTweener.Kill();
        if (overlayPulseTweener != null)
            overlayPulseTweener.Kill();

        omnidrillDurationBar.fillAmount = 1;
        omnidrillRt.DOAnchorPosY(0, 0.15f);
        omnidrillDurationBarTweener = omnidrillDurationBar.DOFillAmount(0, omniDrillDuration);
        omnidrillOverlay.DOFade(1, 0.1f);
        overlayPulseTweener = omnidrillOverlay.DOColor(omnidrillOverlayPulseColor, 0.1f).SetLoops(-1, LoopType.Yoyo);

        yield return new WaitForSeconds(omniDrillDuration);
        RemoveOmniDrill();
    }
    void RemoveOmniDrill()
    {
        ShowNormalToolButtons();
        AudioManager.Instance.StopDrillBgm();
        omnidrillRt.DOAnchorPosY(-280, 0.15f);
        overlayPulseTweener.Kill();
        omnidrillOverlay.color = new Color(1, 1, 1, 0);
        omnidrillOverlay.DOFade(0, 0.1f);
    }
    bool shielded = false;
    void ActivateShield()
    {
        if (shielded)
            return;

        shielded = true;
        shieldBubble.transform.position = spawnedCubes[0].transform.position + new Vector3(0, 0.15625f, 0);
        shieldBubble.SetActive(true);
        shieldFollowCoroutine = StartCoroutine(ShieldFollow());
        AudioManager.PlaySfx(shieldGetSfx);
    }
    void RemoveShield()
    {
        shielded = false;
        shieldBubble.SetActive(false);
        if (shieldFollowCoroutine != null)
            StopCoroutine(shieldFollowCoroutine);
        AudioManager.PlaySfx(shieldPopSfx);
    }
    Tweener shieldMoveTweener;
    const float shieldFollowTweenSpeed = 0.25f;
    const float shieldFollowOffset = 0.0625f;
    IEnumerator ShieldFollow()
    {
        var followTarget = spawnedCubes[0].transform;
        shieldMoveTweener = shieldBubble.transform.DOMoveY(followTarget.position.y + shieldFollowOffset, shieldFollowTweenSpeed);
        while (true)
        {
            yield return null;

            var firstCubeTransform = spawnedCubes[0].transform;
            if (followTarget != firstCubeTransform)
            {
                followTarget = firstCubeTransform;
                shieldMoveTweener.Kill();
                shieldMoveTweener = shieldBubble.transform.DOMoveY(followTarget.position.y + shieldFollowOffset, shieldFollowTweenSpeed);
            }
        }
    }
    void ActivateScoreMultiplier()
    {
        if (scoreMultiplierCoroutine != null)
            StopCoroutine(scoreMultiplierCoroutine);
        scoreMultiplierCoroutine = StartCoroutine(ActivateScoreMultiplierCoroutine());

        scoreMultiplierConfettiPs.Play();
        scoreText.color = scoreMultiplierTextColor;
        //scoreTextWave.PlayAnimation(VertexAttributeModifier.AnimationMode.Wave);
        var glowColor = scoreTextMaterial.GetColor("_GlowColor");
        glowColor.a = 0.5f;
        scoreTextMaterial.SetColor("_GlowColor", glowColor);
        AudioManager.PlaySfxs(scoreMultiplierSfxs);
    }
    IEnumerator ActivateScoreMultiplierCoroutine()
    {
        scoreMultiplier = scoreMultiplierValue;
        yield return new WaitForSeconds(scoreMultiplierDuration);
        RemoveScoreMultipler();
    }
    void RemoveScoreMultipler()
    {
        scoreMultiplier = 1;
        scoreMultiplierConfettiPs.Stop();
        scoreText.color = Color.white;
        //scoreTextWave.Stop();
        var glowColor = scoreTextMaterial.GetColor("_GlowColor");
        glowColor.a = 0f;
        scoreTextMaterial.SetColor("_GlowColor", glowColor);
    }
    #endregion

    void IncreaseScore()
    {
        score += 1 * scoreMultiplier;
        scoreText.text = score.ToString();
    }

    #region Life Cycle
    enum GameOverType { Tnt, Stack }
    GameOverType gameOverType = GameOverType.Tnt;
    void GameOver(GameOverType _gameOverType)
    {
        gameStarted = false;
        gameOverType = _gameOverType;
        StopCoroutine(spawnCubeCoroutine);
        ShowGameOverPanel();
    }

    void ShowGameOverPanel()
    {
        gameOverPanel.SetActive(true);
        scoreText.gameObject.SetActive(false);

        var currentHighscore = PlayerPrefs.GetInt("Highscore");
        if (score > currentHighscore)
        {
            PlayerPrefs.SetInt("Highscore", score);
            currentHighscore = score;
        }

        highscoreText.text = currentHighscore.ToString();
        currentScoreText.text = score.ToString();
    }
    void HideGameOverPanel()
    {
        gameOverPanel.SetActive(false);
        scoreText.gameObject.SetActive(true);
    }

    void AdsContinue()
    {
        AudioManager.PlaySfx(buttonClickSfx);
        HideGameOverPanel();
        adsContinueButton.gameObject.SetActive(false);

        if (gameOverType == GameOverType.Stack)
            RemoveCubesFromTop(adsContinueCubeRemoveCount);

        StartGame();
    }
    void Retry()
    {
        ResetValues();
        AudioManager.PlaySfx(buttonClickSfx);
        DoInitialSpawning();
    }

    void ResetValues()
    {
        gameStarted = false;
        score = 0;
        scoreText.text = score.ToString();
        scoreText.color = Color.white;
        currentCameraMoveSpeed = cameraMoveSpeed;
        maxCameraSpeed = cameraMoveSpeed * 3;
        levelConfigurationIndex = 0;
        ApplyLevelConfiguration(levelConfigurations[levelConfigurationIndex]);
        wallChunkHeightIndex = 0;
        heightIndex = 0;
        scoreMultiplier = 0;
        adsContinueButton.gameObject.SetActive(true);
        omnidrillOverlay.color = new Color(1, 1, 1, 0);
        bgFades[0].color = Color.black;
        bgFades[1].color = Color.black;

        ShowNormalToolButtons();
        HideGameOverPanel();
        RemoveOmniDrill();
        RemoveScoreMultipler();
        RemoveShield();
        ClearAllCubes();
        ClearAllWalls();
        cameraTrans.position = new Vector3(0, -0.25f, cameraTrans.position.z);
    }

    void ClearAllCubes()
    {
        for (int i = spawnedCubes.Count - 1; i >= 0; i--)
            Destroy(spawnedCubes[i].gameObject);
        spawnedCubes.Clear();
    }
    void ClearAllWalls()
    {
        for (int i = spawnedWalls.Count - 1; i >= 0; i--)
            Destroy(spawnedWalls[i].gameObject);
        spawnedWalls.Clear();
    }
    #endregion
}
