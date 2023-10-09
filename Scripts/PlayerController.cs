using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class PlayerController : MonoBehaviourPun
{
     [Header("Stats")]
    public float moveSpeed;
    public float jumpForce;
    public GameObject fireball;
    public GameObject healing;
    public GameObject buff;
    public GameObject polymorph;

    [Header("Components")]
    public Rigidbody rig;
    public AudioSource bard;
    public AudioSource fire;
    public AudioSource heal;
    public AudioSource eep;
    public ParticleSystem bless;
    public ParticleSystem fires;
    public ParticleSystem healingss;
    public GameObject wizardbody;

    [Header("Photon")]
    public int id;
    public Player photonPlayer;

    [Header("Stats")]
    public int curHp;
    public int maxHp;
    public int kills;
    public bool dead;
    private bool flashingDamage;
    public MeshRenderer mr;
    public static int class_number;

    private int curAttackerId;
    public PlayerWeapon weapon;

    [PunRPC]
    public void Initialize(Player player)
    {
        id = player.ActorNumber;
        photonPlayer = player;
        GameManager.instance.players[id - 1] = this;

        if (!photonView.IsMine)
        {
            GetComponentInChildren<Camera>().gameObject.SetActive(false);

            rig.isKinematic = true;
        }
        else
        {
            GameUI.instance.Initialize(this);
        }
    }

    void Update()
    {
        if (!photonView.IsMine || dead)
        {
            return;
        }

        Move();
        if (Input.GetKeyDown(KeyCode.Space))
            TryJump();
        if (Input.GetMouseButtonDown(0))
            weapon.TryShoot();
        if (Input.GetKeyDown(KeyCode.R))
            AbilityAttack(class_number);
            
    }

    void Move()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 dir = (transform.forward * z + transform.right * x) * moveSpeed;
        dir.y = rig.velocity.y;

        rig.velocity = dir;
    }

    void TryJump()
    {
        Ray ray = new Ray(transform.position, Vector3.down);
        if (Physics.Raycast(ray, 1.5f))
            rig.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    [PunRPC]
    public void TakeDamage(int attackerId, int damage)
    {
        if (dead)
            return;

        curHp -= damage;
        curAttackerId = attackerId;

        photonView.RPC("DamageFlash", RpcTarget.Others);

        GameUI.instance.UpdateHealthBar();

        if (curHp <= 0)
            photonView.RPC("Die", RpcTarget.All);
    }

    public void AbilityAttack(int class_number)
    {
        
        if (class_number == 1)
        {
            photonView.RPC("Polymorph", photonPlayer);
        }

        if (class_number == 2)
        {
            photonView.RPC("HealingWound", photonPlayer);
        }

        if (class_number == 3)
        {
            photonView.RPC("Fireball", RpcTarget.Others);
        }

        if (class_number == 4)
        {
            photonView.RPC("Buff", photonPlayer);
        }
    }

    [PunRPC]
    void DamageFlash()
    {
        if (flashingDamage)
            return;

        StartCoroutine(DamageFlashCoRoutine());

        IEnumerator DamageFlashCoRoutine()
        {
            flashingDamage = true;
            Color defaultColor = mr.material.color;
            mr.material.color = Color.red;

            yield return new WaitForSeconds(0.05f);

            mr.material.color = defaultColor;
            flashingDamage = false;
        }
    }

    [PunRPC]
    void Die()
    {
        curHp = 0;
        dead = true;

        GameManager.instance.alivePlayers--;
        GameUI.instance.UpdatePlayerInfoText();

        if (PhotonNetwork.IsMasterClient)
            GameManager.instance.CheckWinCondition();

        if (photonView.IsMine)
        {
            if (curAttackerId != 0)
                GameManager.instance.GetPlayer(curAttackerId).photonView.RPC("AddKill", RpcTarget.All);

            GetComponentInChildren<CameraController>().SetAsSpectator();
            rig.isKinematic = true;
            transform.position = new Vector3(0, -50, 0);
        }
    }

    [PunRPC]
    public void AddKill()
    {
        kills++;
        GameUI.instance.UpdatePlayerInfoText();
    }

    [PunRPC]
    public void Heal(int amountToHeal)
    {
        curHp = Mathf.Clamp(curHp + amountToHeal, 0, maxHp);

        // update the health bar UI
        GameUI.instance.UpdateHealthBar();
    }

    [PunRPC]
    public void Fireball()
    {
         Debug.Log("Fireball!");
         //fireball.gameObject.SetActive(true);
         fire.Play();
         fires.Play();
         curHp -= 25;
         StartCoroutine(Timer_Coroutine_Wizard());
        // fireball.gameObject.SetActive(false);'
         if (curHp <= 0)
            photonView.RPC("Die", RpcTarget.All);

    }

    [PunRPC]
    public void HealingWound()
    {
         Debug.Log("Healing Wound!");
         //healing.gameObject.SetActive(true);
         heal.Play();
         healingss.Play();
         curHp += 25;
         StartCoroutine(Timer_Coroutine_Cleric());
         //healing.gameObject.SetActive(false);
    }

    [PunRPC]
    public void Buff()
    {
         Debug.Log("Buff!");
         //buff.gameObject.SetActive(true);
         bard.Play();
         bless.Play();
         curHp += 5;
         moveSpeed += 5;
         jumpForce += 5;
         StartCoroutine(Timer_Coroutine_Bard());
        // buff.gameObject.SetActive(false);
    }

    [PunRPC]
    public void Polymorph()
    {
         Debug.Log("Polymorph!");
         //if(!PhotonNetwork.IsMasterClient) return;
         //GameObject[] playersArray = GameObject.FindGameObjectsWithTag("Player");
         //foreach(GameObject player in playersArray)
         //{
            polymorph.gameObject.SetActive(true);
            wizardbody.gameObject.SetActive(false);
            eep.Play();
            moveSpeed += 10;
            jumpForce += 10;
            curHp += 30;
            StartCoroutine(Timer_Coroutine_Druid());
        // }

    }



    IEnumerator Timer_Coroutine_Druid()
    {
        yield return new WaitForSeconds(10);
        polymorph.gameObject.SetActive(false);
        wizardbody.gameObject.SetActive(true);
        moveSpeed -= 10;
        jumpForce -= 10;
        curHp -= 30;

    }
    IEnumerator Timer_Coroutine_Cleric()
    {
        yield return new WaitForSeconds(7);
        healingss.Stop();

    }
    IEnumerator Timer_Coroutine_Bard()
    {
        yield return new WaitForSeconds(7);
        bless.Stop();

    }
    IEnumerator Timer_Coroutine_Wizard()
    {
        yield return new WaitForSeconds(7);
        fires.Stop();

    }

}
