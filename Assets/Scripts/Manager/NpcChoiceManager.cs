using UnityEngine;

public class NpcChoiceManager : baseManager
{
    private NpcChoiceConfigSO _config;
    private NpcChoicePopupUI _currentUI;

    public NpcChoiceManager(NpcChoiceConfigSO config)
    {
        _config = config;
    }

    public override void GetController(GameController controller) { }

    public override void Init() { }

    public override void ActiveOff() { }

    public override void Update() { }

    public override void Destory()
    {
        if (_currentUI != null)
            GameObject.Destroy(_currentUI.gameObject);
    }

    // ===== ÇÙ½É API =====
    public void OpenChoice(NPCInteractable npc, NpcProfileSO profile)
    {
        if (_currentUI != null) return;

        var go = GameObject.Instantiate(_config.popupPrefab);
        _currentUI = go.GetComponent<NpcChoicePopupUI>();

        _currentUI.Open(
            profile.displayName,
            () => { npc.ResolveChoice(NpcChoice.Kill); Close(); },
            () => { npc.ResolveChoice(NpcChoice.Spare); Close(); }
        );
    }

    private void Close()
    {
        if (_currentUI == null) return;
        GameObject.Destroy(_currentUI.gameObject);
        _currentUI = null;
    }
}
