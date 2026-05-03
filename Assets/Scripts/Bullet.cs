using UnityEngine;

public class Bullet : MonoBehaviour
{
    // Этот метод вызывается автоматически, когда пуля входит в триггер
    private void OnTriggerEnter2D(Collider2D hitInfo)
    {
        // Проверяем, чтобы пуля не удалялась об самого игрока (если есть тег Player)
        if (!hitInfo.CompareTag("Player"))
        {
            Destroy(gameObject); // Пуля исчезает при попадании
        }
    }

    // Дополнительно: удаление пули через 5 секунд, если она ни во что не попала
    private void Start()
    {
        Destroy(gameObject, 5f);
    }
}