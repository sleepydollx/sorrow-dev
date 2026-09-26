using UnityEngine;

public class CollectibleItem : MonoBehaviour
{
    public enum ItemType { Key, Battery, Note }
    public ItemType itemType;
    public string itemID; // Unique identifier for the item

    [Header("UI Message")]
    public string interactMessage = "Tekan E untuk mengambil item";

    // Function to handle item collection
    public void Collect()
    {
        Debug.Log("Item berhasil diambil: " + itemID);

        // Logic UI raycast

Ray ray = new Ray(Camera.main.transform.position, Camera.main.transform.forward);
RaycastHit hit;

if (Physics.Raycast(ray, out hit, interactDistance))
{
    CollectibleItem item = hit.collider.GetComponent<CollectibleItem>();
    if (item != null)
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            item.Collect();
        }
    }

// Di dalam skrip CollectibleItem.cs:
public void Collect()
{
    if (itemType == ItemType.Battery)
    {
        // Cari komponen Flashlight pada player
        Flashlight playerFlashlight = FindObjectOfType<Flashlight>();
        if (playerFlashlight != null)
        {
            playerFlashlight.AddBattery(50f); // Menambah 50 daya baterai
        }
    }
    
    // Hapus objek baterai dari dunia setelah diambil
    Destroy(gameObject);
}
        // Hapus objek dari scene setelah diambil
        Destroy(gameObject);
    }
}