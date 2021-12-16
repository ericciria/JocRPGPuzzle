using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Assertions;

public class PlayerController : MonoBehaviour
{
    public int maxHealth = 100, maxMana = 100;

    public float speed = 4f, mana;
    public float speedMultiplier = 1f;
    public float knockbackForce = 1000f;
    public int attackDamage = 1;
    public int level, health, exp, enemies;

    public Text nivellUI, vidaUI, manaUI;
    public HealthBar healthBar;
    public ManaBar  manaBar;
    public Camera camara;
    public NPC npc;
    public GameObject gameOver, bulletUp, bulletDown, bulletLeft, bulletRigth;
    AudioSource sound;

    public LayerMask enemyLayer;
    private LayerMask objectLayer;

    [SerializeField] public float attackRadius;
    [SerializeField] public Transform attackHitboxPos;
    [SerializeField] private GameObject deathParticle;
    [SerializeField] private GameObject levelUp;

    Animator anim;
    Rigidbody2D rb;
    Vector2 mov;

    public bool canSave;

    private GameObject circle;

    void Start()
    {
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        health = maxHealth;
        mana = maxMana;
        exp = 0;
        level = 1;
        canSave = true;
        enemies = 9;
        sound = gameObject.GetComponent<AudioSource>();
        objectLayer = LayerMask.GetMask("Object");

        manaUI.text = "Mana: " + mana;
        nivellUI.text = "nivell: " + level;
        vidaUI.text = "Vida: " + health;
        
    }

    void Update()
    {
        Movements();
        Animations();
        SwordAttack();
        SaveLoad();
        Magic();

        if (Input.GetKeyDown(KeyCode.P))
        {
            DamagePlayer(10);
        }
        if (Input.GetKeyDown(KeyCode.L))
        {
            CheckInteractuable();
        }

        camara.transform.position = new Vector3(rb.position.x, rb.position.y, -10);

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

        if (mana < maxMana)
        {
            //manaBar
            mana = Mathf.MoveTowards(mana, maxMana, Time.deltaTime * level);
            manaUI.text = "Mana: " + mana;
            manaBar.SetMana(mana);

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
                Interactuable qwer = objecte.GetComponent<Interactuable>();
                if(qwer.activat == false){
                    qwer.activat = true;
                    qwer.Activar();
                }
            }
        }
    }

    public void DamagePlayer(int damage)
    {
        health -= damage;
        healthBar.SetHealth(health);
        vidaUI.text = "Vida: " + health;

        if (this.health <= 0)
        {
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
            //Debug.Log("colisi� amb enemic");
        }
        if (collision.tag == "Death")
        {
            DamagePlayer(50000);
        }
        if (collision.tag == "NPC")
        {
            npc.NPCDialogue(); ;
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Speed")
        {
            speedMultiplier = 1f;
        }
    }

    private void SaveLoad()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (canSave)
            {
                SavePlayer();
            }
            else
            {
                StartCoroutine(Contador());
                Debug.Log("no es pot guardar");
            }
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            LoadPlayer();
        }
    }

    IEnumerator Contador()
    {
        yield return new WaitForSeconds(1);
        canSave = true;
    }

    private void SavePlayer()
    {
        PlayerData data = new PlayerData(this);
        data.characterLevel = level;
        data.health = health;
        data.mana = mana;

        data.position[0] = transform.position.x;
        data.position[1] = transform.position.y;
        data.position[2] = transform.position.z;

        SaveSystem.SavePlayer(data);
    }

    private void LoadPlayer()
    {
        PlayerData data = SaveSystem.LoadPlayer();
        
        level = data.characterLevel;
        health = data.health;
        mana = data.mana;

        transform.position = new Vector3(data.position[0], data.position[1], data.position[2]);

        vidaUI.text = "Vida: " + health;
        nivellUI.text = "nivell: " + level;
        healthBar.SetHealth(health);
        manaBar.SetMana(mana);
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(attackHitboxPos.position, attackRadius);
    }
}
