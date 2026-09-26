using UnityEngine;

public class CollectibleItem : MonoBehaviour
{
    public enum ItemType { Key, Battery, Note }
    public ItemType itemType;
    public string itemID; // Unique identifier for the item

    [Header("UI Message")]
    public string interactMessage = "Tekan E untuk mengambil item";

    // Function to handle item collection when the player interacts with it
    public void Collect()
    {
        Debug.Log("Item berhasil diambil: " + itemID);

        // Jika item adalah baterai, tambah daya senter
        if (itemType == ItemType.Battery)
        {
            Flashlight playerFlashlight = FindObjectOfType<Flashlight>();
            if (playerFlashlight != null)
            {
                playerFlashlight.AddBattery(50f); 
            }
        }
        
        // Hapus objek dari scene setelah diambil
        Destroy(gameObject);
    }
}