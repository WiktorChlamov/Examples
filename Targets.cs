using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
    /// <summary>
    /// Спавн персонажей, создание списков и сортировка по заданным параметрам
    /// </summary>
public class Targets: MonoBehaviour
{   
    public static Targets targets;
    [SerializeField]
    private Transform 
        enemySpawnPoint,  //место спавна врага
        playerSpawnPoint, //место спавна игрока
        playerPF,         //префаб игрока
        enemyPF,          //префаб врага
        healthBarPF,      //префаб хелсбара
        healthBarParent;  //родитель инстанциации бара
    [SerializeField]
    private List<BaseCharacter> playerCreates, enemieCreates; //персонажи, которые будут в текущей сцене
    public List<Transform> ActiveEnemies { get; set; }  //Активные в данный момент враги
    public List<Transform> ActivePlayers { get; set; }  //Активные в данный момент персонажи игрока
    private List<Transform> 
        enemiesThreat, //сортировка по вражеской угрозе
        enemiesArmor,  //сортировка по кол-ву брони врага
        enemiesHP,     //сортировка по кол-ву здоровья врага
        playersThreat, //сортировка по угрозе персонажей игрока
        playersArmor,  //сортировка по кол-ву брони у персонажей игрока
        playersHP;     //сортировка по кол-ву здоровья персонажей игрока
    private Transform farestEnemyTarget, farestPlayerTarget;  //наиболее дальние персонажи
    public Transform nearestPF, farestPF;// ближайший персонаж слева и справа
    private Coroutine Personages,  // спавнит персонажей из предварительного спсика (Creates) в активный
        farestTargets,             // вычисляет ближайших персонажей слева и справа
        SortingByLowestHP;         // сортировка по здоровью
    [SerializeField]
    private LayerMask playerMask,enemyMask; //маска для вычисления ближайших персонажей 
    private Targets()
    {
        ActiveEnemies = new List<Transform>();
        ActivePlayers = new List<Transform>();
        targets = this;
    }
    public List<BaseCharacter> EnemieCreates {set => enemieCreates = value; } // установка списка врагов сторонним скриптом
    public List<Transform> PlayersHP { get => playersHP; } //получение списка здоровья игроков для приоритета исцеления
    private void Awake()
    {
        playerCreates = Play.BaseCharacters;
    }
    private void Start()
    {
       Personages = StartCoroutine(AddingPersonages());  // спавн персонажей
       farestTargets = StartCoroutine(FarestTargets());  // вычисление дальних целей
       SortingByLowestHP = StartCoroutine(SortingByLowestHp()); //вычисление наименьшего здоровья
    }
    IEnumerator AddingPersonages()  // спавн персонажей
    { 
        while (enemieCreates.Count > 0)
        {
         MovingToActiveAndSpawn(enemieCreates[0]);
        }
        yield return new WaitForSecondsRealtime(0.1f);
        while (playerCreates.Count > 0)
        {
            MovingToActiveAndSpawn(playerCreates[0], false);
        }
        yield return new WaitForSecondsRealtime(1f);
    }
    IEnumerator FarestTargets()  //вычисление дальних целей
    {
        while (true)
        {
            yield return new WaitForSecondsRealtime(0.3f);
            {
                farestEnemyTarget =Interfaces.FindFarestTargets(farestPF, Vector2.left, enemyMask);
            }
            yield return new WaitForSecondsRealtime(0.1f);
            {
                farestPlayerTarget = Interfaces.FindFarestTargets(nearestPF, Vector2.right, playerMask);
            }
        }
    }
    public Transform SelectTarget(bool enemy = true) =>
        enemy ? farestEnemyTarget : farestPlayerTarget; //выбор дальнего врага или дальнего персонажа игрока  
    IEnumerator SortingByLowestHp() //вычисление наименьшего здоровья
    {
        while (true)
        {   if (ActivePlayers.Count > 0)
            {
                playersHP = ActivePlayers.OrderBy(x =>
                x.GetComponent<CharacterStats>().CurrentHealth).ToList();
            }
            if (ActiveEnemies.Count > 0)
            {
                enemiesHP = ActiveEnemies.OrderBy(x =>
                x.GetComponent<CharacterStats>().CurrentHealth).ToList();
            }
                yield return new WaitForSecondsRealtime(0.2f);
        } 
    }
    public void RemoveAndUpdate(Transform playerSettings) //при убийстве персонажа удаляет из активного списка и перезапускает
    {
        StopCoroutine(SortingByLowestHP); //остановка корутины
        if (ActiveEnemies.Remove(playerSettings)) //если это враг, то удаляет из списка
        {
            EnemiesSorting(); //пересортировка
        }
        else                                     
        {
            ActivePlayers.Remove(playerSettings);   //удаление игрока
            PlayersSorting();                       //пересортировка
        }
        SortingByLowestHP = StartCoroutine(SortingByLowestHp()); //запуск корутины
    }
    public void MovingToActiveAndSpawn(BaseCharacter Personage, bool enemy = true) //спавн персонажей и перенос из списка Create в Active        
    {   if (enemy)   //спавн врага и пересортировка
        {  PlayerSettings playerSettings = enemyPF.gameObject.GetComponent<PlayerSettings>();
            playerSettings.Class = (CharactersCreate)Personage;
          Transform transform =  Instantiate(enemyPF, enemySpawnPoint.position,Quaternion.identity);
            ActiveEnemies.Add(transform); enemieCreates.RemoveAt(0);
            EnemiesSorting();
        }
        else            //спавн игрока, его хелсбара
        {   PlayerSettings playerSettings = playerPF.gameObject.GetComponent<PlayerSettings>();
            playerSettings.Class = (CharactersCreate)Personage;
            playerPF.gameObject.GetComponent<CharacterStats>().healthBar = Instantiate(healthBarPF, healthBarParent).GetComponent<Healhbar>();
            Transform transform = Instantiate(playerPF, playerSpawnPoint.position, Quaternion.identity);
            ActivePlayers.Add(transform); playerCreates.RemoveAt(0);
            PlayersSorting();
        }
    }
    public void EnemiesSorting()//  сортировка активных врагов
    {
        if (ActiveEnemies.Count > 0)
        {
            enemiesThreat = ActiveEnemies.OrderBy(x => x.GetComponent<PlayerSettings>().Class.Threat).ToList();
            enemiesArmor = ActiveEnemies.OrderBy(x => x.GetComponent<PlayerSettings>().Class.Armor.GetValue()).ToList();
        }
    }
    public void PlayersSorting()  //  сортировка активных игроков
    {
        playersThreat = ActivePlayers.OrderBy(x => x.GetComponent<PlayerSettings>().Class.Threat).ToList();
        playersArmor = ActivePlayers.OrderBy(x =>x.GetComponent<PlayerSettings>().Class.Armor.GetValue()).ToList();
    }
}
