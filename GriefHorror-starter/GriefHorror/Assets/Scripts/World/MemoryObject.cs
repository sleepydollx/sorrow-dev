public override void Interact()
{
    if (Confronted)
        return;

    if (GriefMeter.Instance != null)
        GriefMeter.Instance.GrantRelief();
    else
        Debug.LogWarning("[MemoryObject] GriefMeter.Instance is null, no relief granted.", this);

    if (advancesStory)
    {
        if (GameManager.Instance != null)
            GameManager.Instance.RegisterTruthFaced();
        else
            Debug.LogWarning("[MemoryObject] GameManager.Instance is null, story not advanced.", this);
    }

    Confronted = true; // only after the effects ran
    Debug.Log($"[Memory] {memoryLine}");
    OnConfronted?.Invoke();
}