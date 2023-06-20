using System.Collections.Generic;
using Bullets;
using EnemySpawners;
using GridForColliders;
using MapGenerators;
using PlayerWeapons;
using Presenters;
using UnityEngine;
using Views;

public class MainGameScript : MonoBehaviour, IForWeaponUse, IEnemyCreatorForSpawner, IForBulletOnReachTargetDestroy
{
    // todo !!! CHANGE to Screen-s (View-s) ?
    public GameObject ViewForGamePaused;
    public GameObject ViewForGameLoosed;
    public GameObject ViewForLevelWin;

    public GlobalView GlobalView;

    /// <summary>
    /// Transform (in Canvas, but is not Canvas itself, for z), on which we instantiate the View of all objects
    /// </summary>
    public Transform ParentForObjectViews;

    public PlayerView PlayerViewPrefab;

    public EnemyViewPrefabCollection EnemyViewPrefabCollection;

    public BulletViewPrefabCollection BulletViewPrefabCollection;

    public GameObject AsteroidViewPrefab;

    private readonly float PlayerCircleColliderRadius = 47; // todo !!! choose based on image size !!!

    private readonly int PlayerHealthStart = 10; // todo 10 - PROD, 1 - TEST

    private readonly float PlayerMoveSpeedPerSec = 150;

    /// <summary>
    /// Enemy Spawn Interval (sec) (based on level)
    /// </summary>
    private float EnemySpawnIntervalSec(int level) => Mathf.Max(0.1f, 2f - 0.1f * (level - 1));

    private readonly float EnemySpawnMinRadiusFromPlayer = 200;
    private readonly float EnemySpawnMaxRadiusFromPlayer = 300;

    /// <summary>
    /// Probability (percent: 0 - 100) of spawn enemy of type = 1 (otherwise: type = 2) (based on level)
    /// </summary>
    private int EnemySpawnProbabilityPercentOfEnemyType1(int level) => Mathf.Max(0, 80 - 10 * (level - 1));

    /// <summary>
    /// Period (sec) of recalculation for the enemy of the direction to the player (for optimization - not every frame)
    /// </summary>
    private const float RecalculateTargetEnemyToPlayerPeriodSec = 1.0f; // todo 0.1f - PROD, 1 - TEST


    private GameState _gameState = GameState.Normal;

    /// <summary>
    /// Difficulty level (1+)
    /// </summary>
    private int _level = 1;

    private int _scoreThisLevel = 0;

    private const int ScoreForNextLevel = 10;

    /// <summary>
    /// (null - not yet created or already destroyed)
    /// </summary>
    private PlayerPresenter _playerPresenter = null;

    private List<BulletPresenter> _allBullets;

    private List<EnemyPresenter> _allEnemies;

    private List<AsteroidPresenter> _allAsteroids;

    /// <summary>
    /// 1st usage: for iterating all IMovablePresenterInGrid
    /// 2nd usage: mapping from grid-based class to IMovablePresenterInGrid
    /// </summary>
    private Dictionary<ObjectInGridWithCollider<ObjectTypeInGrid>, IMovablePresenterInGrid> _allMovablePresenters;

    private EnemySpawner _enemySpawner;

    private readonly EnemyConfigCollection _enemyConfigCollection = new EnemyConfigCollection();

    /// <summary>
    /// Map rect to check for out of bounds
    /// todo NOTE: resolution different - gameplay different
    /// </summary>
    private readonly Rect MapRect = new Rect(0, 0, Screen.width, Screen.height);

    private MyGridForColliders _gridForColliders;

    void Start()
    {
        InitializeGridForColliders();
        InitializeLevel();
    }

    private void InitializeGridForColliders()
    {
        _gridForColliders = new MyGridForColliders(
            // todo NOTE: heuristic: cell size is slightly larger than the largest object
            150,
            Screen.width, Screen.height
        );
    }

    private void InitializeLevel()
    {
        Debug.LogWarning("InitializeLevel(); _level = " + _level);

        _gameState = GameState.Normal;
        RecalculateVisibilityOfViewsForGameState();

        _scoreThisLevel = 0;
        UpdateScoreView();

        _allBullets = new List<BulletPresenter>();
        _allEnemies = new List<EnemyPresenter>();
        _allAsteroids = new List<AsteroidPresenter>();
        _allMovablePresenters = new Dictionary<ObjectInGridWithCollider<ObjectTypeInGrid>, IMovablePresenterInGrid>();
        InitializePlayer();

        _enemySpawner = new EnemySpawnerRandomWithCooldown(EnemySpawnIntervalSec(_level), MapRect,
            EnemySpawnMinRadiusFromPlayer, EnemySpawnMaxRadiusFromPlayer,
            EnemySpawnProbabilityPercentOfEnemyType1(_level));

        GenerateMap();
    }

    private void GenerateMap()
    {
        int safeDistFromPlayer = 300;
        Rect nonGenerationRect = new Rect(
            _playerPresenter.Pos.x - safeDistFromPlayer / 2f,
            _playerPresenter.Pos.y - safeDistFromPlayer / 2f,
            safeDistFromPlayer,
            safeDistFromPlayer
            );

        int asteroidCount = 1 + _level;
        const float asteroidCircleColliderRadius = 25; // todo !!! choose based on image size !!!

        List<Asteroid> resultAsteroids = new List<Asteroid>();
        IMapGenerator mapGenerator = new MapGeneratorWithAsteroids();
        mapGenerator.Generate(nonGenerationRect, MapRect,
            asteroidCount, asteroidCircleColliderRadius, resultAsteroids);

        Debug.Log("asteroidCount = " + asteroidCount);
        foreach (Asteroid asteroid in resultAsteroids)
        {
            Debug.Log("asteroid: Pos = " + asteroid.Pos);
            GameObject view = CreateGameObjectView(asteroid.Pos, AsteroidViewPrefab);
            AsteroidPresenter asteroidPresenter = new AsteroidPresenter(asteroid, view);
            _allAsteroids.Add(asteroidPresenter);
            _allMovablePresenters.Add(asteroid, asteroidPresenter);
            _gridForColliders.OnObjectCreated(asteroid);
        }
    }

    private void InitializePlayer()
    {
        Vector2 playerPos = new Vector2(Screen.width / 2, Screen.height / 2);
        List<PlayerWeapon> allPlayerWeapons = CreatePlayerWeapons();
        Player player = new Player(playerPos, PlayerCircleColliderRadius, PlayerHealthStart, allPlayerWeapons);

        PlayerView playerView = CreatePlayerView(playerPos);
        _playerPresenter = new PlayerPresenter(player, playerView);
        _allMovablePresenters.Add(player, _playerPresenter);
        _gridForColliders.OnObjectCreated(player);
    }

    private List<PlayerWeapon> CreatePlayerWeapons()
    {
        List<PlayerWeapon> allPlayerWeapons = new List<PlayerWeapon>()
        {
            new PlayerWeaponBulletFastCooldown(),
            new PlayerWeaponBulletSplash(),
            // todo laser weapon with ray
        };

        foreach (var weapon in allPlayerWeapons)
        {
            weapon.Init();
        }

        return allPlayerWeapons;
    }

    /// <summary>
    /// One iteration of Game Loop
    /// </summary>
    void Update()
    {
        if (_gameState == GameState.Paused)
        {
            // NOTE: in fact, we put it on pause, because do not perform any updates and ignore input
            TryUnpauseGame();
            TryRestartGame();
            TryQuitGame();
            return;
        }

        if (_gameState == GameState.Loosed)
        {
            // NOTE: in fact, we put it on pause, because do not perform any updates and ignore input
            TryRestartGame();
            TryQuitGame();
            return;
        }

        if (_gameState == GameState.LevelWin)
        {
            // NOTE: in fact, we put it on pause, because do not perform any updates and ignore input
            TryProceedToNextLevel();
            TryQuitGame();
            return;
        }

        TryPauseGame();

        _playerPresenter.TryReadInputAndHandle(Time.deltaTime, MapRect, PlayerMoveSpeedPerSec, this);
        _playerPresenter.UpdateCooldownOfAllWeapons(Time.deltaTime);

        AdditionalCheckDestroyAllBullets(Time.deltaTime);

        _enemySpawner.TrySpawnEnemies(Time.deltaTime, _playerPresenter.Pos, this);
        TryRecalculateTargetEnemyToPlayerIfDurationExpiredForAllEnemies(Time.deltaTime);

        UpdatePosForAll(Time.deltaTime);

        TryCollidePlayerWithEnemiesOrAsteroids();
        TryCollideBulletsWithEnemiesOrAsteroids();
    }

    private void TryPauseGame()
    {
        if (!Input.GetKeyDown(KeyCode.Escape)) return; // todo NOTE: 'ESC'
        Debug.LogWarning("PAUSE GAME");
        _gameState = GameState.Paused;
        RecalculateVisibilityOfViewsForGameState();
    }

    private void TryUnpauseGame()
    {
        if (!Input.GetKeyDown(KeyCode.Escape)) return; // todo NOTE: 'ESC'
        Debug.LogWarning("UNPAUSE GAME");
        _gameState = GameState.Normal;
        RecalculateVisibilityOfViewsForGameState();
    }

    private void TryRestartGame()
    {
        if (!Input.GetKeyDown(KeyCode.R)) return; // todo NOTE: 'R'
        Debug.LogWarning("RESTART GAME");
        TryDestroyAll();
        _level = 1;
        InitializeLevel();
    }

    private void TryQuitGame()
    {
        if (!Input.GetKeyDown(KeyCode.Q)) return; // todo NOTE: 'Q'
        Debug.LogWarning("QUIT GAME");
        Application.Quit();
    }

    private void TryProceedToNextLevel()
    {
        if (!Input.GetKeyDown(KeyCode.E)) return; // todo NOTE: 'E'
        Debug.LogWarning("NEXT LEVEL");
        _level++;
        InitializeLevel();
    }

    void IForWeaponUse.AddBullet(Bullet bullet, int bulletViewIndex)
    {
        Debug.Log("AddBullet: bullet.Pos = " + bullet.Pos + "; bulletViewIndex = " + bulletViewIndex);
        GameObject prefab = BulletViewPrefabCollection.BulletViewPrefabByIndex[bulletViewIndex];
        GameObject view = CreateGameObjectView(bullet.Pos, prefab);
        BulletPresenter bulletPresenter = new BulletPresenter(bullet, view);
        _allBullets.Add(bulletPresenter);
        _allMovablePresenters.Add(bullet, bulletPresenter);
        _gridForColliders.OnObjectCreated(bullet);
    }

    private void AdditionalCheckDestroyAllBullets(float deltaTimeSec)
    {
        for (int i = _allBullets.Count - 1; i >= 0; i--)
        {
            BulletPresenter bulletPresenter = _allBullets[i];
            bool needDestroy = bulletPresenter.AdditionalCheckDestroyBullet(deltaTimeSec, MapRect);
            if (needDestroy)
            {
                DestroyBullet(bulletPresenter);
                _allBullets.RemoveAt(i);
            }
        }
    }

    void IEnemyCreatorForSpawner.SpawnEnemy(Vector2 pos, int enemyTypeIndex)
    {
        Debug.Log("SpawnEnemy: pos = {" + pos.x + "; " + pos.y + "}; enemyTypeIndex = " + enemyTypeIndex);

        EnemyConfig enemyConfig = _enemyConfigCollection.EnemyConfigByEnemyTypeIndex[enemyTypeIndex];
        GameObject prefab = enemyConfig.ViewPrefabFunc.Invoke(EnemyViewPrefabCollection);
        float circleColliderRadius = enemyConfig.CircleColliderRadius;
        int healthStart = enemyConfig.Health;
        float speedPerSec = enemyConfig.MoveSpeedPerSec;

        GameObject view = CreateGameObjectView(pos, prefab);
        Enemy enemy = new Enemy(pos, circleColliderRadius, healthStart, speedPerSec);
        EnemyPresenter enemyPresenter = new EnemyPresenter(enemy, view);
        _allEnemies.Add(enemyPresenter);
        _allMovablePresenters.Add(enemy, enemyPresenter);
        _gridForColliders.OnObjectCreated(enemy);
        //
        // todo !!!!!!!!!!!! debug only - CONSOLE LOGS WILL LAG !!!!!!!!!!!!!!
        //enemy.DebugDumpCellIds();
    }

    private void TryRecalculateTargetEnemyToPlayerIfDurationExpiredForAllEnemies(float deltaTimeSec)
    {
        for (int i = _allEnemies.Count - 1; i >= 0; i--)
        {
            EnemyPresenter enemyPresenter = _allEnemies[i];
            enemyPresenter.TryRecalculateTargetEnemyToPlayerIfDurationExpired(deltaTimeSec,
                RecalculateTargetEnemyToPlayerPeriodSec, _playerPresenter.Pos);
        }
    }

    private void UpdatePosForAll(float deltaTimeSec)
    {
        foreach (IMovablePresenterInGrid movablePresenter in _allMovablePresenters.Values)
        {
            movablePresenter.UpdatePos(deltaTimeSec, _gridForColliders);
        }
    }

    private void TryCollidePlayerWithEnemiesOrAsteroids()
    {
        _gridForColliders.TryCollideObjectWithOthers(_playerPresenter.Model,
            ObjectTypeInGrid.Enemy.Or(ObjectTypeInGrid.Asteroid),
            OnCollidePlayerWithEnemyOrAsteroid);
    }

    private void OnCollidePlayerWithEnemyOrAsteroid(ObjectInGridWithCollider<ObjectTypeInGrid> obj1, ObjectInGridWithCollider<ObjectTypeInGrid> obj2)
    {
        Debug.LogError("OnCollidePlayerWithEnemyOrAsteroid");

        // (before a potential loose - otherwise there will be double destruction)
        if (obj2.ObjectTypeInGrid == ObjectTypeInGrid.Enemy)
        {
            Enemy enemy = obj2 as Enemy;
            // todo NOTE: mapping and cast because we need EnemyPresenter, not Enemy (which stored in grid)
            EnemyPresenter enemyPresenter = _allMovablePresenters[enemy] as EnemyPresenter;
            DestroyEnemy(enemyPresenter);
            _allEnemies.Remove(enemyPresenter);
        }
        else
        {
            // NOTE: Asteroid is not destroyed by Player collision

            // todo NOTE: Asteroid feedback for player (0.1 sec is HACK)
            _playerPresenter.Model.DeltaPosPerSec = -_playerPresenter.Model.DeltaPosPerSec;
            _playerPresenter.UpdatePos(0.1f, _gridForColliders);
        }
        //
        DamagePlayer(1);
    }

    private void TryCollideBulletsWithEnemiesOrAsteroids()
    {
        _gridForColliders.TryCollide(ObjectTypeInGrid.Bullet,
            ObjectTypeInGrid.Enemy.Or(ObjectTypeInGrid.Asteroid),
            OnCollideBulletWithEnemyOrAsteroid);
    }

    private void OnCollideBulletWithEnemyOrAsteroid(ObjectInGridWithCollider<ObjectTypeInGrid> obj1, ObjectInGridWithCollider<ObjectTypeInGrid> obj2)
    {
        Debug.LogError("OnCollideBulletWithEnemyOrAsteroid");

        Bullet bullet = obj1 as Bullet;
        // todo NOTE: mapping and cast because we need BulletPresenter, not Bullet (which stored in grid)
        BulletPresenter bulletPresenter = _allMovablePresenters[bullet] as BulletPresenter;
        DestroyBullet(bulletPresenter);
        _allBullets.Remove(bulletPresenter);

        TryDamageEnemyOrAsteroid(obj2, bullet.Damage);

        // NOTE: after possible Enemy Destroy
        // todo: !! it's better to exclude damaged enemy from new targets, but it's harder to implement
        bullet.OnReachTargetDestroy(this);
    }

    private void TryDamageEnemyOrAsteroid(ObjectInGridWithCollider<ObjectTypeInGrid> obj, int bulletDamage)
    {
        if (obj.ObjectTypeInGrid == ObjectTypeInGrid.Enemy)
        {
            Enemy enemy = obj as Enemy;
            enemy.AddHealth(-bulletDamage);
            if (!enemy.IsAlive())
            {
                AddScore(1);

                // todo NOTE: mapping and cast because we need EnemyPresenter, not Enemy (which stored in grid)
                EnemyPresenter enemyPresenter = _allMovablePresenters[enemy] as EnemyPresenter;
                DestroyEnemy(enemyPresenter);
                _allEnemies.Remove(enemyPresenter);
            }
        }
        else
        {
            // NOTE: Asteroid does not add score

            // NOTE: Asteroid is destroyed
            Asteroid asteroid = obj as Asteroid;
            // todo NOTE: mapping and cast because we need AsteroidPresenter, not Asteroid (which stored in grid)
            AsteroidPresenter asteroidPresenter = _allMovablePresenters[asteroid] as AsteroidPresenter;
            DestroyAsteroid(asteroidPresenter);
            _allAsteroids.Remove(asteroidPresenter);
        }
    }

    private void AddScore(int delta)
    {
        _scoreThisLevel += delta;
        UpdateScoreView();
        if (_scoreThisLevel >= ScoreForNextLevel)
        {
            _scoreThisLevel = 0;
            SetStateLevelWin();
        }
    }

    private void UpdateScoreView()
    {
        GlobalView.UpdateScore(_scoreThisLevel, ScoreForNextLevel);
    }

    void IForBulletOnReachTargetDestroy.SplashDamageEnemiesOrAsteroids(Vector2 pos, float range, int damage)
    {
        Debug.LogError("SplashDamageEnemiesOrAsteroids");

        // todo NOTE: new HashSet() - not optimal
        HashSet<ObjectInGridWithCollider<ObjectTypeInGrid>> foundObjects = new HashSet<ObjectInGridWithCollider<ObjectTypeInGrid>>();
        _gridForColliders.GetObjectsInRange(pos, range,
            ObjectTypeInGrid.Enemy.Or(ObjectTypeInGrid.Asteroid),
            foundObjects);

        foreach (var obj in foundObjects)
        {
            Debug.LogError("OnSplashTargetFoundEnemyOrAsteroid: type = " + obj.ObjectTypeInGrid.AsInt() + "; pos = " + obj.Pos);
            TryDamageEnemyOrAsteroid(obj, damage);
        }
    }

    private void DamagePlayer(int damage)
    {
        _playerPresenter.Model.AddHealth(-damage);
        if (!_playerPresenter.Model.IsAlive())
        {
            SetStateGameLoose();
        }
    }

    private void SetStateGameLoose()
    {
        if (_gameState == GameState.Loosed)
        {
            Debug.LogError("SetStateGameLoose() when _gameState == GameState.Loosed");
            return; // (assert)
        }
        Debug.LogWarning("SetStateGameLoose()");
        _gameState = GameState.Loosed;
        RecalculateVisibilityOfViewsForGameState();
        TryDestroyAll();
    }

    private void SetStateLevelWin()
    {
        if (_gameState == GameState.LevelWin)
        {
            Debug.LogError("SetStateLevelWin() when _gameState == GameState.LevelWin");
            return; // (assert)
        }
        Debug.LogWarning("SetStateLevelWin()");
        _gameState = GameState.LevelWin;
        RecalculateVisibilityOfViewsForGameState();
        TryDestroyAll();
    }

    private void TryDestroyAll()
    {
        TryDestroyPlayer();
        DestroyAllBullets();
        DestroyAllEnemies();
        DestroyAllAsteroids();
    }

    private void RecalculateVisibilityOfViewsForGameState()
    {
        ViewForGamePaused.SetActive(_gameState == GameState.Paused);
        ViewForGameLoosed.SetActive(_gameState == GameState.Loosed);
        ViewForLevelWin.SetActive(_gameState == GameState.LevelWin);
    }

    private void TryDestroyPlayer()
    {
        if (_playerPresenter != null)
        {
            Destroy(_playerPresenter.View);// todo pool ?
            _gridForColliders.OnObjectRemoved(_playerPresenter.Model);
            _allMovablePresenters.Remove(_playerPresenter.Model);
            _playerPresenter = null;
        }
    }

    private void DestroyAllBullets()
    {
        for (int i = _allBullets.Count - 1; i >= 0; i--)
        {
            DestroyBullet(_allBullets[i]);
        }

        _allBullets.Clear();
    }

    // todo NOTE: does not delete from _allBullets
    private void DestroyBullet(BulletPresenter bulletPresenter)
    {
        GenericDestroy(bulletPresenter);
    }

    private void DestroyAllEnemies()
    {
        for (int i = _allEnemies.Count - 1; i >= 0; i--)
        {
            DestroyEnemy(_allEnemies[i]);
        }

        _allEnemies.Clear();
    }

    // todo NOTE: does not delete from _allEnemies
    private void DestroyEnemy(EnemyPresenter enemyPresenter)
    {
        GenericDestroy(enemyPresenter);
    }

    private void DestroyAllAsteroids()
    {
        for (int i = _allAsteroids.Count - 1; i >= 0; i--)
        {
            DestroyAsteroid(_allAsteroids[i]);
        }

        _allAsteroids.Clear();
    }

    // todo NOTE: does not delete from _allEnemies
    private void DestroyAsteroid(AsteroidPresenter asteroidPresenter)
    {
        GenericDestroy(asteroidPresenter);
    }

    private void GenericDestroy<TModel>(BasePresenter<TModel> presenter)
        where TModel : ObjectInGridWithCollider<ObjectTypeInGrid>
    {
        presenter.OnDestroy();
        Destroy(presenter.View); // todo pool ?
        _gridForColliders.OnObjectRemoved(presenter.Model);
        _allMovablePresenters.Remove(presenter.Model);
    }

    private PlayerView CreatePlayerView(Vector2 pos)
    {
        PlayerView playerView = Instantiate(PlayerViewPrefab, pos, Quaternion.identity);// todo pool ?
        playerView.transform.SetParent(ParentForObjectViews, false);
        return playerView;
    }

    private GameObject CreateGameObjectView(Vector2 pos, GameObject prefab)
    {
        GameObject view = Instantiate(prefab, pos, Quaternion.identity);// todo pool ?
        view.transform.SetParent(ParentForObjectViews, false);
        return view;
    }
}