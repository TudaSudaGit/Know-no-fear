using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class Health : MonoBehaviour
{
    public int maxHealth = 5;
    public int currentHealth;

    [Header("Настройки UI")]
    public GameObject heartPrefab;
    public Transform heartsContainer;

    private List<GameObject> hearts = new List<GameObject>();

    void Start()
    {
        currentHealth = maxHealth;
        SetupUI();
    }

    void SetupUI()
    {
        foreach (Transform child in heartsContainer) Destroy(child.gameObject);
        hearts.Clear();

        for (int i = 0; i < maxHealth; i++)
        {
            GameObject heart = Instantiate(heartPrefab, heartsContainer);
            hearts.Add(heart);
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        UpdateHearts();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void UpdateHearts()
    {
        for (int i = 0; i < hearts.Count; i++)
        {
            hearts[i].SetActive(i < currentHealth);
        }
    }

    void Die()
    {
        Debug.Log(gameObject.name + " погиб!");
        Destroy(gameObject);
    }
}