using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour, IsSaveable
{
    public GameObject ui;
    public int maxHealth = 100, maxMana = 100;

    public float speed = 4f, mana;
    public float speedMultiplier = 1f;
    public float knockbackForce = 1000f;
    public int attackDamage = 1;
    public int level, health, exp;

    private Text nivellUI, vidaUI, manaUI;
    private HealthBar healthBar;
    private ManaBar manaBar;
    private Camera camara;

    public NPC npc;

    public GameObject bulletUp, bulletDown, bulletLeft, bulletRigth;
    private GameObject gameOver, victory, saveLoad;

    AudioSource sound;
    [SerializeField] AudioClip audioHurt, audioAttack, audioMagic, audioVictory, audioGameOver, audioActivable;

    public LayerMask enemyLayer;
    private LayerMask objectLayer;

    [SerializeField] public float attackRadius;
    [SerializeField] public Transform attackHitboxPos;
    [SerializeField] private GameObject deathParticle;
    [SerializeField] private GameObject levelUp;
    private UIInventory uiInventory;

    public ContadorVidaGermana contadorVida;


    Animator anim;
    public Rigidbody2D rb;
    Vector2 mov;

    public bool canSave;
    public bool sumarVida;

    private GameObject circle;
    private Inventory inventory;

    private void Awake()
    {
        if (FindObjectsOfType<PlayerController>().Length > 1)
        {
            Destroy(gameObject);
        }
        else
        {
            DontDestroyOnLoad(gameObject);
        }
        inventory = new Inventory();
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        camara = GameObject.Find("Main Camera").GetComponent<Camera>();
    }

    void Start()
    {
        ui = GameObject.FindGameObjectWithTag("UI");
        healthBar = ui.GetComponentInChildren<HealthBar>();
        manaBar = ui.GetComponentInChildren<ManaBar>();
        nivellUI = GameObject.Find("/UI2/Nivell").GetComponent<Text>();
        vidaUI = GameObject.Find("/UI2/HealthBar/HealthText").GetComponent<Text>();
        manaUI = GameObject.Find("/UI2/ManaBar/ManaText").GetComponent<Text>();

        gameOver = GameObject.Find("/UI2/GameOver");
        gameOver.SetActive(false);
        victory = GameObject.Find("/UI2/Victoria");
        victory.SetActive(false);
        saveLoad = GameObject.Find("/UI2/SaveLoad");
        saveLoad.SetActive(false);

        camara = GameObject.Find("Main Camera").GetComponent<Camera>();
        uiInventory = ui.GetComponentInChildren<UIInventory>();
        contadorVida = GameObject.Find("ContadorGermana").GetComponent<ContadorVidaGermana>();

        uiInventory.SetInventory(inventory);
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        health = maxHealth;
        mana = maxMana;
        exp = 0;
        level = 1;
        canSave = true;
        sound = gameObject.GetComponent<AudioSource>();
        objectLayer = LayerMask.GetMask("Object");

        vidaUI.text = "Vida: " + health;
        nivellUI.text = "nivell: " + level;
        vidaUI.text = "Vida: " + health;


    }

    void Update()
    {
        Movements();
        Animations();
        SwordAttack();
        Magic();

        if (Input.GetKeyDown(KeyCode.P))
        {
            DamagePlayer(10);
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            CheckInteractuable();
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            saveLoad.SetActive(!saveLoad.activeSelf);
        }
        if (Input.GetKeyDown(KeyCode.M))
        {
            Item prova = new Item { itemType = Item.ItemType.HealthPotion, amount = 1 };
            inventory.RemoveItem(prova);
            uiInventory.RefreshInventory();
        }

        if (camara != null)
        {
            camara.transform.position = new Vector3(rb.position.x, rb.position.y, -10);
        }

    }

    void FixedUpdate()
    {
        rb.MovePosition(rb.position + mov * speed * Time.deltaTime * speedMultiplier);
    }

    void Movements()
    {
        mov = new Vector2(
            Input.GetAxisRaw("Horizontal"),
            Input.GetAxisRaw("Vertical")
        );
    }

    void Animations()
    {

        if (mov != Vector2.zero)
        {
            anim.SetFloat("movX", mov.x);
            anim.SetFloat("movY", mov.y);
            anim.SetBool("walking", true);
            attackHitboxPos.position = new Vector2(rb.position.x + (mov.x/2), rb.position.y + (mov.y/2));
            //Debug.Log(attackHitboxPos.position.x-rb.position.x);
        }
        else
        {
            anim.SetBool("walking", false);
        }
    }

    void SwordAttack()
    {
        AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(0);
        bool attacking = stateInfo.IsName("Attack");

        if ((Input.GetKeyDown("space") || Input.GetKeyDown(KeyCode.Z)) && !attacking)
        {
            sound.PlayOneShot(audioAttack, 0.7F);
            anim.SetTrigger("Attacking");
            CheckAttackHitbox();
        }
    }

    void Magic()
    {
        if (Input.GetKeyDown(KeyCode.X))
        {
            if (mana > 10 * level)
            {
                sound.PlayOneShot(audioMagic, 0.7F);
                mana -= 10 * level;
                manaUI.text = "Mana: " + mana;
                manaBar.SetMana(mana);
                if (attackHitboxPos.position.x - rb.position.x >0.1)
                {
                    Instantiate(bulletRigth, attackHitboxPos.transform.position, Quaternion.identity);
                }
                else if (attackHitboxPos.position.x - rb.position.x < -0.1)
                {
                    Instantiate(bulletLeft, attackHitboxPos.transform.position, Quaternion.identity);
                }
                else if (attackHitboxPos.position.y - rb.position.y > 0.1)
                {
                    Instantiate(bulletUp, attackHitboxPos.transform.position, Quaternion.identity);
                }
                else if (attackHitboxPos.position.y - rb.position.y < -0.1)
                {
                    Instantiate(bulletDown, attackHitboxPos.transform.position, Quaternion.identity);
                }
                
            }
        }
        if (Input.GetKeyDown(KeyCode.C))
        {
            if (mana > 10 * level)
            {
                if(health < maxHealth - (10 * level))
                {
                    mana -= 10 * level;
                    manaUI.text = "Mana: " + mana;
                    manaBar.SetMana(mana);
                    health += 10 * level;
                    vidaUI.text = "Vida: " + health;
                    healthBar.SetHealth(health);
                }
                else if (health < maxHealth)
                {
                    mana -= 10 * level;
                    manaUI.text = "Mana: " + mana;
                    manaBar.SetMana(mana);
                    health = maxHealth;
                    vidaUI.text = "Vida: " + health;
                    healthBar.SetHealth(health);
                }
            }
        }
        if (Input.GetKeyDown(KeyCode.F))
        {
            UsePotion();
        }

        if (mana < maxMana)
        {
            //manaBar
            mana = Mathf.MoveTowards(mana, maxMana, Time.deltaTime * level);
            manaUI.text = "Mana: " + mana;
            manaBar.SetMana(mana);

        }
    }

    private void UsePotion()
    {
        Item potion = new Item { itemType = Item.ItemType.HealthPotion, amount = 1 };
        if (inventory.CheckItem(potion))
        {
            
            if (health == maxHealth)
            {
                sound.PlayOneShot(audioMagic, 0.7F);
                return;
            }
            if (health < maxHealth / 2)
            {
                sound.PlayOneShot(audioActivable, 0.7F);
                health = health + maxHealth/2;
                vidaUI.text = "Vida: " + health;
                healthBar.SetHealth(health);
            }
            else
            {
                sound.PlayOneShot(audioActivable, 0.7F);
                health = maxHealth;
                vidaUI.text = "Vida: " + health;
                healthBar.SetHealth(health);
            }
            inventory.RemoveItem(potion);
        }
        else
        {
            sound.PlayOneShot(audioHurt, 0.7F);
        }
    }

    private void CheckAttackHitbox()
    {
        Collider2D[] detectedOjects = Physics2D.OverlapCircleAll(attackHitboxPos.position, attackRadius, enemyLayer);

        foreach (Collider2D enemic in detectedOjects)
        {
            if (enemic.GetComponent<Enemic1>() != null)
            {
                enemic.GetComponent<Enemic1>().takeDamage(attackDamage*level);
            }
            if (enemic.GetComponent<Enemic2>() != null)
            {
                enemic.GetComponent<Enemic2>().takeDamage(attackDamage * level);
            }
            if (enemic.GetComponent<Destroyable>() != null)
            {
                enemic.GetComponent<Destroyable>().Destruir();
            }
        }
    }

    private void CheckInteractuable()
    {
        Collider2D[] detectedOjects = Physics2D.OverlapCircleAll(attackHitboxPos.position, attackRadius, objectLayer);

        foreach (Collider2D objecte in detectedOjects)
        {
            if (objecte.GetComponent<Interactuable>() != null)
            {
                Interactuable interactuable = objecte.GetComponent<Interactuable>();
                if(interactuable.activat == false){
                    interactuable.activat = true;
                    interactuable.Activar();
                }
            }
            if (objecte.GetComponent<Palanca>() != null)
            {
                Palanca palanca = objecte.GetComponent<Palanca>();
                palanca.Activar();
            }
            if (objecte.GetComponent<KeyDoor>() != null)
            {
                KeyDoor keyDoor = objecte.GetComponent<KeyDoor>();
                if(keyDoor.isOpenableWithKey){
                    Item key = new Item { itemType = Item.ItemType.Key, amount = 1 };
                    Item keyFinal = new Item { itemType = Item.ItemType.FinalKey, amount = 1 };
                    if (inventory.CheckItem(key) && !keyDoor.activat && !keyDoor.final)
                    {
                        sound.PlayOneShot(audioActivable, 0.7F);
                        keyDoor.Activar();
                        inventory.RemoveItem(key);
                    }       
                    else if(inventory.CheckItem(keyFinal) && !keyDoor.activat && keyDoor.final){
                        sound.PlayOneShot(audioActivable, 0.7F);
                        keyDoor.Activar();
                        inventory.RemoveItem(keyFinal);
                    }  
                }        
            }
            if (objecte.GetComponent<movimentDona>() != null)
            {
                Item herba = new Item { itemType = Item.ItemType.HealingHerb, amount = 1 };
                Item cura = new Item { itemType = Item.ItemType.Cure, amount = 1 };
                if (inventory.CheckItem(cura))
                {
                    sound.PlayOneShot(audioActivable, 0.7F);
                    victory.SetActive(true);
                    inventory.RemoveItem(cura);
                    contadorVida.guanyat = true;
                }
                else if (inventory.CheckItem(herba))
                {
                    sound.PlayOneShot(audioActivable, 0.7F);
                    contadorVida.vida += 20;
                    inventory.RemoveItem(herba);
                }
            }
        }
    }

    public void DamagePlayer(int damage)
    {
        sound.PlayOneShot(audioHurt, 0.7F);
        health -= damage;
        healthBar.SetHealth(health);
        vidaUI.text = "Vida: " + health;

        if (this.health <= 0)
        {
            Debug.Log("hola");
            sound.PlayOneShot(audioGameOver, 0.7F);
            Instantiate(deathParticle, rb.transform.position, deathParticle.transform.rotation);
            health = 0;
            healthBar.SetHealth(health);
            vidaUI.text = "Vida: " + health;
            gameOver.SetActive(true);
            Destroy(gameObject);
            //Time.timeScale = 0;
        }
    }

    public void UpdateExp(int expPoints)
    {
        exp += expPoints;
        while(exp >= 10*level)
        {
            sound.Play();
            Instantiate(levelUp, rb.transform.position, deathParticle.transform.rotation);
            exp -= 10*level;
            level += 1;
            maxHealth += 50;
            maxMana += 50;
            health = maxHealth;
            mana = maxMana;
            vidaUI.text = "Vida: " + health;
            healthBar.SetHealth(health);
            nivellUI.text = "nivell: " + level;
            manaBar.SetMana(mana);
            manaUI.text = "Mana: " + mana;
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Speed") {
            speedMultiplier = 2f;
        }
        if (collision.tag == "Enemy")
        {
            Vector2 difference = (transform.position - collision.transform.position).normalized;
            Vector2 force = difference * knockbackForce;
            rb.AddForce(force);
        }
        if (collision.tag == "Death")
        {
            DamagePlayer(50000);
        }
        /*if (collision.tag == "NPC")
        {
            npc.NPCDialogue(); ;
        }*/

        if (collision.gameObject.tag == "vida")
        {
            sumarVida = true;
            Destroy(GameObject.FindWithTag("vida"));
            Debug.Log(sumarVida);
        }
        if (collision.gameObject.tag == "Object")
        {
            ItemGround itemGround = collision.GetComponent<ItemGround>();
            inventory.AddItem(itemGround.GetItem());
            itemGround.DestroySelf();
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Speed")
        {
            speedMultiplier = 1f;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(attackHitboxPos.position, attackRadius);
    }

    [System.Serializable]
    struct PlayerDataNou
    {
        public int level;
        public int health;
        public float mana;
        public string escena;

        public float[] position;
    }

    public object CaptureState()
    {
        PlayerDataNou data;
        data.level = level;
        data.health = health;
        data.mana = mana;
        data.escena = SceneManager.GetActiveScene().name;

        data.position = new float[3];

        data.position[0] = transform.position.x;
        data.position[1] = transform.position.y;
        data.position[2] = transform.position.z;

        return data;
    }

    public void RestoreState(object dataLoaded)
    {
        PlayerDataNou data = (PlayerDataNou)dataLoaded;
        level = data.level;
        health = data.health;
        mana = data.mana;
        SceneManager.LoadScene(data.escena);
        vidaUI.text = "Vida: " + health;
        vidaUI.text = "Vida: " + health;

        transform.position = new Vector3(data.position[0], data.position[1], data.position[2]);
    }


}
